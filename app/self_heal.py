from typing import List
from .db import SessionLocal, Device, AuditLog
from .quantumshield import risk_score

def heal_devices() -> List[str]:
    healed: List[str] = []
    with SessionLocal() as db:
        devices = db.query(Device).all()
        for d in devices:
            if d.status == "down" and risk_score(d.status) < 80:
                d.status = "up"
                healed.append(d.name)
                db.add(AuditLog(action="self_heal", entity="device", entity_id=d.id))
        db.commit()
    return healed
