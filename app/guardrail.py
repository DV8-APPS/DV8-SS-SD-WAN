from __future__ import annotations
from typing import Dict
from sqlalchemy.orm import Session
from .db import AuditLog


def lint(intent_yaml: str) -> Dict[str, object]:
    ok = "policy" in intent_yaml
    return {"valid": ok, "issues": [] if ok else ["missing policy"]}


def approve(db: Session, change_id: str) -> Dict[str, str]:
    db.add(AuditLog(action="guardrail_approve", entity="change", details=change_id))
    db.commit()
    return {"changeId": change_id, "status": "approved"}
