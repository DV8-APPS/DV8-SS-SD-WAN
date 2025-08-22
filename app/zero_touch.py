from typing import List, Optional, Dict

from sqlalchemy.orm import Session

from .db import Device, ZeroTouchConfig, AuditLog


def enroll(db: Session, name: str, template: str) -> Dict[str, object]:
    device = db.query(Device).filter_by(name=name).first()
    if not device:
        device = Device(name=name)
        db.add(device)
        db.flush()
    zt = db.query(ZeroTouchConfig).filter_by(device_id=device.id).first()
    if zt:
        zt.template = template
    else:
        zt = ZeroTouchConfig(device_id=device.id, template=template)
        db.add(zt)
    db.add(AuditLog(action="zero_touch_enroll", entity="device", entity_id=device.id, details=template))
    db.commit()
    return {"name": name, "template": template, "applied": zt.applied}


def get(db: Session, name: str) -> Optional[Dict[str, object]]:
    device = db.query(Device).filter_by(name=name).first()
    if not device or not device.zero_touch:
        return None
    zt = device.zero_touch
    return {"name": name, "template": zt.template, "applied": zt.applied}


def list_devices(db: Session) -> List[Dict[str, object]]:
    zts = db.query(ZeroTouchConfig).all()
    return [{"name": z.device.name, "template": z.template, "applied": z.applied} for z in zts]


def mark_applied(db: Session, name: str) -> Optional[Dict[str, object]]:
    device = db.query(Device).filter_by(name=name).first()
    if not device or not device.zero_touch:
        return None
    zt = device.zero_touch
    zt.applied = True
    db.add(AuditLog(action="zero_touch_applied", entity="device", entity_id=device.id))
    db.commit()
    return {"name": name, "template": zt.template, "applied": zt.applied}
