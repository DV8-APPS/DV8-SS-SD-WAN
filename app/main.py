from fastapi import FastAPI, Request, HTTPException, Depends, Response, Query, Form
from fastapi.responses import HTMLResponse, RedirectResponse
from fastapi.templating import Jinja2Templates
from pydantic import BaseModel
from starlette.middleware.sessions import SessionMiddleware
import importlib
import os
import shutil
import subprocess
from typing import Dict, Optional, List
from sqlalchemy.orm import Session

from .analytics import average_latency, device_status_count
from .firmware import install as fw_install, get_version as fw_get_version
from .sandbox import register as sb_register, get as sb_get, update as sb_update, list_devices as sb_list, summary as sb_summary
from .zero_touch import enroll as zt_enroll, get as zt_get, mark_applied as zt_mark, list_devices as zt_list
from .discovery import start_job, get_job, list_candidates
from .db import get_db, init_db, SessionLocal, AuditLog
from .self_heal import heal_devices, preempt_class_library_error
from .healer import create_playbook, list_incidents, execute_incident
from .guardrail import lint as gr_lint, approve as gr_approve
from .synthetics import create_probe, get_results as syn_results
from .planner import recommend as planner_recommend
from .compliance import start_scan, get_results as compliance_results
from .vulnwatch import sync as vuln_sync, findings as vuln_findings
from .pathtracer import trace as path_trace
from .policy_impact import timeline as policy_timeline
from .autocapture import auto_capture
from .collectors import list_collectors

app = FastAPI(title="DV8 SD-WAN Console")
templates = Jinja2Templates(directory="app/templates")

# session middleware for simple login; secret key can be overridden via env
SECRET_KEY = os.getenv("DV8_SESSION_SECRET", "dev-secret")
app.add_middleware(SessionMiddleware, secret_key=SECRET_KEY)

# register zero-error middleware early so it is in place before startup
from .zero_error import init_zero_error
init_zero_error(app)

REQUIRED_MODULES = ["fastapi", "jinja2", "pydantic", "sqlalchemy", "python_multipart"]


def check_dependencies() -> None:
    missing = [m for m in REQUIRED_MODULES if importlib.util.find_spec(m) is None]
    if missing:
        raise RuntimeError(f"Missing dependencies: {', '.join(missing)}")
    for m in REQUIRED_MODULES:
        importlib.import_module(m)


def preload_dotnet_sdk() -> None:
    """Ensure the .NET SDK is available by running `dotnet --info`."""
    if shutil.which("dotnet") is None:
        raise RuntimeError("Missing dependency: dotnet SDK is not installed")
    subprocess.run(["dotnet", "--info"], check=True, stdout=subprocess.DEVNULL)


@app.on_event("startup")
async def startup_check() -> None:
    check_dependencies()
    preload_dotnet_sdk()
    init_db()


@app.middleware("http")
async def decision_header(request: Request, call_next):
    response = await call_next(request)
    if "X-DV8-Decision" not in response.headers:
        response.headers["X-DV8-Decision"] = "ALLOW"
    return response

# Sample metrics; in real system, would come from devices
device_metrics = [
    {"device": "router-1", "latency_ms": 10.5, "status": "up"},
    {"device": "switch-1", "latency_ms": 20.2, "status": "up"},
    {"device": "router-2", "latency_ms": 35.0, "status": "down"},
]


@app.get("/metrics")
async def metrics():
    return {
        "devices": device_metrics,
        "average_latency": average_latency(device_metrics),
        "status_counts": device_status_count(device_metrics),
    }


def require_login(request: Request):
    if "user" not in request.session:
        raise HTTPException(status_code=401, detail="login required")


@app.get("/login", response_class=HTMLResponse)
async def login_form(request: Request):
    return templates.TemplateResponse("login.html", {"request": request})


@app.post("/login")
async def login(request: Request, username: str = Form(...), password: str = Form(...)):
    expected_user = os.getenv("DV8_USERNAME", "admin")
    expected_pass = os.getenv("DV8_PASSWORD", "password")
    if username == expected_user and password == expected_pass:
        request.session["user"] = username
        return RedirectResponse("/dashboard", status_code=303)
    return HTMLResponse("Invalid credentials", status_code=401)


@app.get("/logout")
async def logout(request: Request):
    request.session.clear()
    return RedirectResponse("/login")


@app.get("/dashboard", response_class=HTMLResponse)
async def dashboard(request: Request, user: str = Depends(require_login)):
    data = {
        "devices": device_metrics,
        "avg_latency": average_latency(device_metrics),
        "status_counts": device_status_count(device_metrics),
    }
    return templates.TemplateResponse("dashboard.html", {"request": request, "data": data})


class InstallRequest(BaseModel):
    device: str
    version: str


@app.post("/firmware/install")
async def install_firmware(req: InstallRequest, db: Session = Depends(get_db)):
    version = fw_install(db, req.device, req.version)
    return {"device": req.device, "version": version}


@app.get("/firmware/{device}")
async def firmware_status(device: str, db: Session = Depends(get_db)):
    return {"device": device, "version": fw_get_version(db, device)}


class ZeroTouchRequest(BaseModel):
    name: str
    template: str


@app.post("/zero-touch/enroll")
async def enroll_device(req: ZeroTouchRequest, db: Session = Depends(get_db)):
    return zt_enroll(db, req.name, req.template)


@app.get("/zero-touch/device/{name}")
async def get_zero_touch(name: str, db: Session = Depends(get_db)):
    device = zt_get(db, name)
    if not device:
        raise HTTPException(status_code=404, detail="device not found")
    return device


@app.post("/zero-touch/device/{name}/applied")
async def mark_applied(name: str, db: Session = Depends(get_db)):
    device = zt_mark(db, name)
    if not device:
        raise HTTPException(status_code=404, detail="device not found")
    return device


@app.get("/zero-touch/devices")
async def list_zero_touch_devices(db: Session = Depends(get_db)):
    return zt_list(db)


class SandboxDevice(BaseModel):
    name: str
    device_type: str
    ports: int
    status: str = "up"
    warnings: Dict[str, bool] = {}
    latitude: Optional[float] = None
    longitude: Optional[float] = None


class StatusUpdate(BaseModel):
    status: Optional[str] = None
    warnings: Optional[Dict[str, bool]] = None
    latitude: Optional[float] = None
    longitude: Optional[float] = None


@app.post("/sandbox/device")
async def register_device(dev: SandboxDevice, db: Session = Depends(get_db)):
    return sb_register(
        db,
        dev.name,
        dev.device_type,
        dev.ports,
        dev.status,
        dev.warnings,
        dev.latitude,
        dev.longitude,
    )


@app.get("/sandbox/device/{name}")
async def get_device(name: str, db: Session = Depends(get_db)):
    device = sb_get(db, name)
    if not device:
        raise HTTPException(status_code=404, detail="device not found")
    return device


@app.put("/sandbox/device/{name}")
async def update_device(name: str, update: StatusUpdate, db: Session = Depends(get_db)):
    device = sb_update(
        db,
        name,
        update.status,
        update.warnings,
        update.latitude,
        update.longitude,
    )
    if not device:
        raise HTTPException(status_code=404, detail="device not found")
    return device


@app.get("/sandbox/devices")
async def list_devices(db: Session = Depends(get_db)):
    return sb_list(db)


@app.get("/sandbox/summary")
async def sandbox_summary(db: Session = Depends(get_db)):
    return sb_summary(db)


class DiscoveryRequest(BaseModel):
    tenant: str
    scopes: List[str]
    cidrs: Optional[List[str]] = None
    passiveOnly: bool = False


@app.post("/discovery/jobs", status_code=202)
async def start_discovery(req: DiscoveryRequest):
    job = start_job(req.tenant, req.scopes, req.cidrs or [], req.passiveOnly)
    return job


@app.get("/discovery/jobs/{job_id}")
async def get_discovery_job(job_id: str):
    job = get_job(job_id)
    if not job:
        raise HTTPException(status_code=404, detail="job not found")
    return job


@app.get("/discovery/candidates")
async def discovery_candidates(tenant: str):
    return list_candidates(tenant)


class ReplaceRequest(BaseModel):
    serial: str
    site: str
    requireAttestation: bool = True


@app.post("/devices/{virtual_id}:replace")
async def replace_virtual(virtual_id: str, req: ReplaceRequest):
    # Stub implementation; real system would verify attestation and swap state
    return {"physicalId": req.serial, "ledger": "ledger-placeholder"}


class CCPRequest(BaseModel):
    actor: str
    action: str
    context: Dict[str, Optional[str]]


@app.post("/v1/ccp/adjudications")
async def ccp_adjudicate(req: CCPRequest):
    return {"decision": "ALLOW", "reason": "stub"}


class HoacRequest(BaseModel):
    channel: str
    policy: str
    risk: int


@app.post("/v1/crypto/hoac/negotiate")
async def hoac_negotiate(req: HoacRequest):
    return {"profile": "ECC", "transcript": ""}


class AuditRequest(BaseModel):
    lso: bool = False
    scope: Optional[Dict[str, str]] = None


@app.post("/v1/audit/bundles", status_code=201)
async def create_audit_bundle(req: AuditRequest, response: Response):
    response.headers["Location"] = "/audit/bundles/placeholder"
    return {"status": "created"}

@app.post("/self-heal")
async def trigger_self_heal():
    return {"healed": heal_devices(), "dotnet": preempt_class_library_error()}


# ----- Auto-Healer APIs -----

class Playbook(BaseModel):
    name: str
    triggers: List[Dict[str, object]]
    steps: List[Dict[str, object]]


@app.post("/v1/heal/playbooks")
async def create_heal_playbook(pb: Playbook):
    return create_playbook(pb.name, pb.triggers, pb.steps)


@app.get("/v1/heal/incidents")
async def get_incidents(state: Optional[str] = Query(None)):
    return list_incidents(state)


@app.post("/v1/heal/incidents/{incident_id}/execute")
async def execute_heal_incident(incident_id: int, db: Session = Depends(get_db)):
    incident = execute_incident(db, incident_id)
    if not incident:
        raise HTTPException(status_code=404, detail="incident not found")
    return incident


# ----- GuardRail APIs -----

class LintRequestModel(BaseModel):
    intentYaml: str


@app.post("/v1/guardrail/lint")
async def guardrail_lint(req: LintRequestModel):
    return gr_lint(req.intentYaml)


class ApproveRequest(BaseModel):
    changeId: str


@app.post("/v1/guardrail/approve")
async def guardrail_approve(req: ApproveRequest, db: Session = Depends(get_db)):
    return gr_approve(db, req.changeId)


# ----- Synthetics APIs -----

class ProbeRequest(BaseModel):
    intentId: str
    endpoints: List[str]
    proto: str
    interval: int


@app.post("/v1/synthetics/probes")
async def create_probe_endpoint(req: ProbeRequest):
    return create_probe(req.intentId, req.endpoints, req.proto, req.interval)


@app.get("/v1/synthetics/results")
async def synthetic_results(intentId: str):
    return syn_results(intentId)


# ----- Planner API -----

class PlanRequest(BaseModel):
    site: str
    intentId: str
    horizonDays: int


@app.post("/v1/planner/recommend")
async def planner_recommend_endpoint(req: PlanRequest):
    return planner_recommend(req.site, req.intentId, req.horizonDays)


# ----- Compliance & Vulnerability APIs -----

class ComplianceScanRequest(BaseModel):
    profile: str
    scope: Dict[str, str] = {}


@app.post("/v1/compliance/scans")
async def compliance_scan(req: ComplianceScanRequest, db: Session = Depends(get_db)):
    return start_scan(db, req.profile, req.scope)


@app.get("/v1/compliance/results")
async def compliance_scan_results(profile: str, device: str):
    return compliance_results(profile, device)


@app.post("/v1/vulnwatch/sync")
async def vulnwatch_sync(db: Session = Depends(get_db)):
    return vuln_sync(db)


@app.get("/v1/vulnwatch/findings")
async def vulnwatch_findings(device: str):
    return vuln_findings(device)


@app.get("/v1/sdwan/pathtracer")
async def sdwan_pathtracer(src: str, dst: str):
    return path_trace(src, dst)


@app.get("/v1/policy/impact")
async def policy_impact(policyId: str, before: str, after: str):
    return policy_timeline(policyId, before, after)


class AutoCaptureRequest(BaseModel):
    serial: str


@app.post("/v1/devices/autocapture")
async def devices_autocapture(req: AutoCaptureRequest, db: Session = Depends(get_db)):
    return auto_capture(db, req.serial)


@app.get("/v1/vendors/{vendor}/collectors")
async def vendor_collectors(vendor: str):
    return {"collectors": list_collectors(vendor)}
