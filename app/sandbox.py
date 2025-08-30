from typing import Dict, List, Optional
from collections import Counter

from sqlalchemy.orm import Session

from .db import Device, Warning, AuditLog


def register(
    db: Session,
    name: str,
    device_type: str,
    ports: int,
    status: str = "up",
    warnings: Optional[Dict[str, bool]] = None,
    latitude: Optional[float] = None,
    longitude: Optional[float] = None,
) -> Dict[str, object]:
    device = db.query(Device).filter_by(name=name).first()
    if not device:
        device = Device(
            name=name,
            device_type=device_type,
            ports=ports,
            status=status,
            latitude=latitude,
            longitude=longitude,
        )
        db.add(device)
        db.flush()
    else:
        device.device_type = device_type
        device.ports = ports
        device.status = status
        device.latitude = latitude
        device.longitude = longitude
        for w in list(device.warnings):
            db.delete(w)
    if warnings:
        for w_name, active in warnings.items():
            db.add(Warning(device_id=device.id, name=w_name, active=active))
    db.add(AuditLog(action="sandbox_register", entity="device", entity_id=device.id))
    db.commit()
    return serialize_device(device)


def get(db: Session, name: str) -> Optional[Dict[str, object]]:
    device = db.query(Device).filter_by(name=name).first()
    if not device:
        return None
    return serialize_device(device)


def update(
    db: Session,
    name: str,
    status: Optional[str] = None,
    warnings: Optional[Dict[str, bool]] = None,
    latitude: Optional[float] = None,
    longitude: Optional[float] = None,
) -> Optional[Dict[str, object]]:
    device = db.query(Device).filter_by(name=name).first()
    if not device:
        return None
    if status is not None:
        device.status = status
    if latitude is not None:
        device.latitude = latitude
    if longitude is not None:
        device.longitude = longitude
    if warnings:
        for w_name, active in warnings.items():
            w = db.query(Warning).filter_by(device_id=device.id, name=w_name).first()
            if w:
                w.active = active
            else:
                db.add(Warning(device_id=device.id, name=w_name, active=active))
    db.add(AuditLog(action="sandbox_update", entity="device", entity_id=device.id))
    db.commit()
    return serialize_device(device)


def list_devices(db: Session) -> List[Dict[str, object]]:
    devices = db.query(Device).all()
    return [serialize_device(d) for d in devices]


def summary(db: Session) -> Dict[str, object]:
    devices = db.query(Device).all()
    status_counts = Counter(d.status for d in devices)
    warning_counts = Counter(
        w.name for d in devices for w in d.warnings if w.active
    )
    return {
        "total_devices": len(devices),
        "total_ports": sum(d.ports for d in devices),
        "status_counts": dict(status_counts),
        "warning_counts": dict(warning_counts),
    }


def serialize_device(device: Device) -> Dict[str, object]:
    return {
        "name": device.name,
        "device_type": device.device_type,
        "ports": device.ports,
        "status": device.status,
        "latitude": device.latitude,
        "longitude": device.longitude,
        "warnings": {w.name: w.active for w in device.warnings},
    }
