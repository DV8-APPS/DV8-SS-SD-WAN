from sqlalchemy.orm import Session

from .db import Device, Firmware, AuditLog


def install(db: Session, device: str, version: str) -> str:
    device_obj = db.query(Device).filter_by(name=device).first()
    if not device_obj:
        device_obj = Device(name=device)
        db.add(device_obj)
        db.flush()
    fw = db.query(Firmware).filter_by(device_id=device_obj.id).first()
    if fw:
        fw.version = version
    else:
        fw = Firmware(device_id=device_obj.id, version=version)
        db.add(fw)
    db.add(AuditLog(action="install_firmware", entity="device", entity_id=device_obj.id, details=version))
    db.commit()
    return version


def get_version(db: Session, device: str) -> str:
    device_obj = db.query(Device).filter_by(name=device).first()
    if not device_obj or not device_obj.firmware:
        return ""
    return device_obj.firmware.version
