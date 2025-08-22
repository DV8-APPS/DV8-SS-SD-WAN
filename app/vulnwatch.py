from typing import Dict
from sqlalchemy.orm import Session
from .db import AuditLog

def sync(db: Session) -> Dict[str, str]:
    db.add(AuditLog(action="vuln_sync", entity="cve"))
    db.commit()
    return {"status": "synced"}

def findings(device: str) -> Dict[str, list]:
    return {"device": device, "vulnerabilities": []}
