import os, sys
from fastapi.testclient import TestClient

# configure test database
DB_PATH = os.path.join(os.path.dirname(__file__), "test.db")
if os.path.exists(DB_PATH):
    os.remove(DB_PATH)
os.environ["DATABASE_URL"] = f"sqlite:///{DB_PATH}"

sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from app.db import init_db, SessionLocal, AuditLog  # type: ignore
from app.main import app  # type: ignore

init_db()
client = TestClient(app)


def login():
    resp = client.post("/login", data={"username": "admin", "password": "password"}, allow_redirects=False)
    assert resp.status_code == 303


def test_metrics_endpoint():
    resp = client.get("/metrics")
    assert resp.status_code == 200
    data = resp.json()
    assert "average_latency" in data
    assert "status_counts" in data
    assert len(data["devices"]) == 3


def test_dashboard_requires_login():
    resp = client.get("/dashboard")
    assert resp.status_code == 401
    login()
    resp2 = client.get("/dashboard")
    assert resp2.status_code == 200
    assert "Network Metrics" in resp2.text


def test_firmware_installation():
    resp = client.post("/firmware/install", json={"device": "router-1", "version": "1.2.3"})
    assert resp.status_code == 200
    data = resp.json()
    assert data["device"] == "router-1"
    assert data["version"] == "1.2.3"
    resp2 = client.get("/firmware/router-1")
    assert resp2.status_code == 200
    assert resp2.json()["version"] == "1.2.3"
    with SessionLocal() as db:
        assert db.query(AuditLog).filter_by(action="install_firmware").count() == 1


def test_sandbox_device_lifecycle():
    resp = client.post(
        "/sandbox/device",
        json={
            "name": "sim-router",
            "device_type": "router",
            "ports": 8,
            "latitude": -33.9,
            "longitude": 18.4,
        },
    )
    assert resp.status_code == 200
    data = resp.json()
    assert data["ports"] == 8
    assert data["latitude"] == -33.9
    assert data["longitude"] == 18.4

    resp = client.put(
        "/sandbox/device/sim-router",
        json={
            "status": "down",
            "warnings": {"temperature": True},
            "latitude": -34.0,
            "longitude": 18.5,
        },
    )
    assert resp.status_code == 200
    data = resp.json()
    assert data["status"] == "down"
    assert data["warnings"]["temperature"] is True
    assert data["latitude"] == -34.0
    assert data["longitude"] == 18.5

    resp = client.get("/sandbox/summary")
    assert resp.status_code == 200
    summary = resp.json()
    assert summary["total_devices"] >= 1
    assert summary["total_ports"] >= 8
    assert summary["warning_counts"]["temperature"] >= 1


def test_zero_touch_workflow():
    resp = client.post(
        "/zero-touch/enroll",
        json={"name": "ztp-router", "template": "basic-router"},
    )
    assert resp.status_code == 200
    data = resp.json()
    assert data["template"] == "basic-router"
    assert data["applied"] is False

    resp = client.post("/zero-touch/device/ztp-router/applied")
    assert resp.status_code == 200
    assert resp.json()["applied"] is True

    resp = client.get("/zero-touch/devices")
    assert resp.status_code == 200
    devices = resp.json()
    assert any(d["name"] == "ztp-router" for d in devices)


def test_autocapture_and_collectors():
    resp = client.post("/v1/devices/autocapture", json={"serial": "ABC12345"})
    assert resp.status_code == 200
    data = resp.json()
    assert "hostname" in data and "mgmt_ip" in data

    resp2 = client.get("/v1/vendors/meraki/collectors")
    assert resp2.status_code == 200
    assert "collectors" in resp2.json()


def test_self_heal_endpoint():
    client.post('/sandbox/device', json={'name':'heal-router','device_type':'router','ports':4,'status':'down'})
    resp = client.post('/self-heal')
    assert resp.status_code == 200
    data = resp.json()
    assert 'heal-router' in data['healed']
    assert 'corrective_action' in data['dotnet']


def test_discovery_flow():
    resp = client.post('/discovery/jobs', json={
        'tenant': 'tenantA',
        'scopes': ['site:A'],
        'cidrs': ['10.0.0.0/24']
    })
    assert resp.status_code == 202
    job_id = resp.json()['id']
    resp2 = client.get(f'/discovery/jobs/{job_id}')
    assert resp2.status_code == 200
    resp3 = client.get('/discovery/candidates', params={'tenant': 'tenantA'})
    assert resp3.status_code == 200


def test_auto_healer_and_guardrail():
    resp = client.post('/v1/heal/playbooks', json={'name': 'pb', 'triggers': [], 'steps': []})
    assert resp.status_code == 200
    pb_id = resp.json()['id']
    incidents = client.get('/v1/heal/incidents').json()
    assert any(i['id'] == pb_id for i in incidents)
    exec_resp = client.post(f'/v1/heal/incidents/{pb_id}/execute')
    assert exec_resp.status_code == 200

    lint_resp = client.post('/v1/guardrail/lint', json={'intentYaml': 'policy: allow'})
    assert lint_resp.status_code == 200
    assert lint_resp.json()['valid'] is True
    app_resp = client.post('/v1/guardrail/approve', json={'changeId': '1'})
    assert app_resp.status_code == 200


def test_synthetics_and_planner():
    probe = client.post('/v1/synthetics/probes', json={'intentId': 'i1', 'endpoints': ['a'], 'proto': 'http', 'interval': 10})
    assert probe.status_code == 200
    res = client.get('/v1/synthetics/results', params={'intentId': 'i1'})
    assert res.status_code == 200
    plan = client.post('/v1/planner/recommend', json={'site': 's1', 'intentId': 'i1', 'horizonDays': 5})
    assert plan.status_code == 200


def test_compliance_vuln_pathtracer():
    scan = client.post('/v1/compliance/scans', json={'profile': 'DISA', 'scope': {}})
    assert scan.status_code == 200
    res = client.get('/v1/compliance/results', params={'profile': 'DISA', 'device': 'router-1'})
    assert res.status_code == 200

    sync = client.post('/v1/vulnwatch/sync')
    assert sync.status_code == 200
    findings = client.get('/v1/vulnwatch/findings', params={'device': 'router-1'})
    assert findings.status_code == 200

    path = client.get('/v1/sdwan/pathtracer', params={'src': 'a', 'dst': 'b'})
    assert path.status_code == 200
    impact = client.get('/v1/policy/impact', params={'policyId': 'p1', 'before': 'old', 'after': 'new'})
    assert impact.status_code == 200
