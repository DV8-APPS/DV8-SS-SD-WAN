from sqlalchemy.orm import Session

from .db import Device, AuditLog


def auto_capture(db: Session, serial: str):
    hostname = f"dev-{serial[-4:].lower()}"
    mgmt_ip = "192.0.2.1"
    os_version = "FlowOS 1.0"
    device = db.query(Device).filter_by(name=hostname).first()
    if not device:
        device = Device(name=hostname)
        db.add(device)
        db.flush()
    db.add(AuditLog(action="autocapture", entity="device", entity_id=device.id, details=serial))
    db.commit()
    return {"hostname": hostname, "mgmt_ip": mgmt_ip, "os": os_version}
