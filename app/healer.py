from __future__ import annotations
from typing import Dict, List, Optional
from sqlalchemy.orm import Session
from .db import AuditLog

_playbooks: Dict[int, Dict[str, object]] = {}
_incidents: Dict[int, Dict[str, object]] = {}


def create_playbook(name: str, triggers: List[dict], steps: List[dict]) -> Dict[str, object]:
    pid = len(_playbooks) + 1
    playbook = {"id": pid, "name": name, "triggers": triggers, "steps": steps}
    _playbooks[pid] = playbook
    # automatically create open incident
    _incidents[pid] = {"id": pid, "playbook_id": pid, "state": "open"}
    return playbook


def list_incidents(state: Optional[str] = None) -> List[Dict[str, object]]:
    incidents = list(_incidents.values())
    if state:
        incidents = [i for i in incidents if i["state"] == state]
    return incidents


def execute_incident(db: Session, incident_id: int) -> Optional[Dict[str, object]]:
    incident = _incidents.get(incident_id)
    if not incident or incident["state"] != "open":
        return None
    incident["state"] = "closed"
    db.add(AuditLog(action="auto_heal", entity="incident", entity_id=incident_id))
    db.commit()
    return incident
