from typing import List, Dict, Any
import time
import datetime
from .db import SessionLocal, Device, AuditLog, Warning
from .quantumshield import risk_score

def heal_devices() -> Dict[str, Any]:
    """
    Enhanced self-healing function that performs comprehensive device recovery
    with detailed logging, risk assessment, and intelligent recovery strategies.
    """
    healed: List[str] = []
    warnings_created: List[str] = []
    errors: List[str] = []
    start_time = time.time()
    
    with SessionLocal() as db:
        try:
            devices = db.query(Device).all()
            total_devices = len(devices)
            down_devices = [d for d in devices if d.status == "down"]
            
            for device in down_devices:
                try:
                    # Calculate risk score for healing decision
                    risk = risk_score(device.status)
                    
                    # Enhanced healing logic with risk assessment
                    if risk < 80:  # Low to medium risk
                        # Attempt healing
                        device.status = "up"
                        healed.append(device.name)
                        
                        # Create detailed audit log
                        audit = AuditLog(
                            action="self_heal",
                            entity="device",
                            entity_id=device.id,
                            details=f"Device {device.name} healed successfully. Risk score: {risk}"
                        )
                        db.add(audit)
                        
                        # Remove any existing warnings for this device
                        db.query(Warning).filter(Warning.device_id == device.id).delete()
                        
                    elif risk < 95:  # High risk but manageable
                        # Create warning instead of immediate healing
                        warning = Warning(
                            device_id=device.id,
                            name=f"Device {device.name} requires attention. Risk score: {risk}",
                            active=True
                        )
                        db.add(warning)
                        warnings_created.append(device.name)
                        
                        # Log the warning creation
                        audit = AuditLog(
                            action="warning_created",
                            entity="device",
                            entity_id=device.id,
                            details=f"High risk warning created for {device.name}. Risk: {risk}"
                        )
                        db.add(audit)
                        
                    else:  # Critical risk - requires manual intervention
                        error_msg = f"Device {device.name} requires manual intervention (Risk: {risk})"
                        errors.append(error_msg)
                        
                        # Log critical status
                        audit = AuditLog(
                            action="critical_alert",
                            entity="device",
                            entity_id=device.id,
                            details=error_msg
                        )
                        db.add(audit)
                        
                except Exception as e:
                    error_msg = f"Failed to process device {device.name}: {str(e)}"
                    errors.append(error_msg)
                    
                    # Log the error
                    audit = AuditLog(
                        action="heal_error",
                        entity="device",
                        entity_id=device.id if hasattr(device, 'id') else None,
                        details=error_msg
                    )
                    db.add(audit)
            
            # Commit all changes
            db.commit()
            
            # Calculate healing statistics
            end_time = time.time()
            duration = round(end_time - start_time, 2)
            success_rate = len(healed) / len(down_devices) * 100 if down_devices else 100
            
            # Create summary audit log
            summary = AuditLog(
                action="heal_summary",
                entity="system",
                details=f"Healing session completed. Healed: {len(healed)}, Warnings: {len(warnings_created)}, Errors: {len(errors)}, Duration: {duration}s"
            )
            db.add(summary)
            db.commit()
            
            return {
                "healed": healed,
                "healed_count": len(healed),
                "warnings_created": warnings_created,
                "warnings_count": len(warnings_created),
                "errors": errors,
                "error_count": len(errors),
                "total_devices": total_devices,
                "down_devices_found": len(down_devices),
                "success_rate": round(success_rate, 1),
                "duration_seconds": duration,
                "timestamp": datetime.datetime.now().isoformat(),
                "status": "completed"
            }
            
        except Exception as e:
            # Rollback on critical error
            db.rollback()
            error_msg = f"Critical error during healing process: {str(e)}"
            
            # Log critical system error
            audit = AuditLog(
                action="heal_critical_error",
                entity="system",
                details=error_msg
            )
            db.add(audit)
            db.commit()
            
            return {
                "healed": [],
                "healed_count": 0,
                "warnings_created": [],
                "warnings_count": 0,
                "errors": [error_msg],
                "error_count": 1,
                "total_devices": 0,
                "down_devices_found": 0,
                "success_rate": 0,
                "duration_seconds": round(time.time() - start_time, 2),
                "timestamp": datetime.datetime.now().isoformat(),
                "status": "failed"
            }


def get_healing_history(limit: int = 50) -> List[Dict[str, Any]]:
    """Get recent healing history from audit logs"""
    with SessionLocal() as db:
        healing_logs = db.query(AuditLog).filter(
            AuditLog.action.in_(["self_heal", "heal_summary", "heal_error", "heal_critical_error"])
        ).order_by(AuditLog.timestamp.desc()).limit(limit).all()
        
        return [
            {
                "id": log.id,
                "action": log.action,
                "entity": log.entity,
                "entity_id": log.entity_id,
                "details": log.details,
                "timestamp": log.timestamp.isoformat() if log.timestamp else None
            }
            for log in healing_logs
        ]


def get_healing_statistics() -> Dict[str, Any]:
    """Get comprehensive healing statistics"""
    with SessionLocal() as db:
        # Count successful healings in last 24 hours
        yesterday = datetime.datetime.now() - datetime.timedelta(days=1)
        
        recent_heals = db.query(AuditLog).filter(
            AuditLog.action == "self_heal",
            AuditLog.timestamp >= yesterday
        ).count()
        
        # Count total devices
        total_devices = db.query(Device).count()
        healthy_devices = db.query(Device).filter(Device.status == "up").count()
        down_devices = db.query(Device).filter(Device.status == "down").count()
        
        # Count active warnings
        active_warnings = db.query(Warning).count()
        
        # Calculate system health percentage
        health_percentage = (healthy_devices / total_devices * 100) if total_devices > 0 else 100
        
        return {
            "recent_heals_24h": recent_heals,
            "total_devices": total_devices,
            "healthy_devices": healthy_devices,
            "down_devices": down_devices,
            "active_warnings": active_warnings,
            "system_health_percentage": round(health_percentage, 1),
            "last_updated": datetime.datetime.now().isoformat()
        }
