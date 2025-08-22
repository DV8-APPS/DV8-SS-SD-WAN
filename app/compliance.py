from typing import Dict, List
from sqlalchemy.orm import Session
from .db import AuditLog

def start_scan(db: Session, profile: str, scope: Dict[str, str]) -> Dict[str, str]:
    job_id = f"{profile}-scan"
    db.add(AuditLog(action="compliance_scan", entity="profile", details=profile))
    db.commit()
    return {"job": job_id}

def get_results(profile: str, device: str) -> Dict[str, str]:
    return {"device": device, "profile": profile, "status": "pass"}
