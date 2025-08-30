"""Self-healing utilities for DV8 console services."""

from pathlib import Path
from typing import Dict, List

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


def preempt_class_library_error() -> Dict[str, str]:
    """Detect attempts to run a class library project and provide a fix.

    Visual Studio may show the error
    "A project with an Output Type of Class Library cannot be started directly"
    when developers try to run a test project. To help, inspect the test
    project and return guidance on the correct command.
    """

    csproj = Path("dotnet/tests/DV8.Console.Tests/DV8.Console.Tests.csproj")
    if not csproj.exists():
        return {"status": "ok"}

    text = csproj.read_text()
    # Test projects use the default library output type. Warn developers to run
    # tests instead of trying to execute the project directly.
    if "<OutputType>" not in text:
        return {
            "error": "class library cannot be started",
            "corrective_action": "Use 'dotnet test' or run the console with 'dotnet run --project dotnet/src/DV8.Console'",
        }
    return {"status": "ok"}
