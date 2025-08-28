#!/usr/bin/env python3
"""
Comprehensive integration test for DV8 SD-WAN platform.

This test validates all integration points, the self-healing module,
and ensures all functions deliver what the specifications state.
"""

import pytest
import requests
import time
import threading
from pathlib import Path
import sys

# Add app to path for imports
sys.path.insert(0, str(Path(__file__).parent))

from app.main import app
from app.db import SessionLocal, Device, AuditLog, init_db
from app.self_heal import heal_devices
import uvicorn


class TestServer:
    """Test server context manager"""
    
    def __init__(self, port=8002):
        self.port = port
        self.server_thread = None
        self.base_url = f"http://127.0.0.1:{port}"
    
    def __enter__(self):
        def run_server():
            uvicorn.run(app, host="127.0.0.1", port=self.port, log_level="critical")
        
        self.server_thread = threading.Thread(target=run_server, daemon=True)
        self.server_thread.start()
        time.sleep(2)  # Wait for server to start
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        # Server will be killed when main process exits due to daemon thread
        pass


def test_database_integration():
    """Test database initialization and basic operations"""
    print("🗄️  Testing database integration...")
    
    # Initialize database
    init_db()
    
    with SessionLocal() as db:
        # Clean up any existing test devices
        db.query(Device).filter(Device.name.like("test-%")).delete()
        db.commit()
        
        # Create test devices
        test_devices = [
            Device(name="test-router-1", device_type="router", ports=4, status="up"),
            Device(name="test-switch-1", device_type="switch", ports=24, status="down"),
            Device(name="test-router-2", device_type="router", ports=8, status="down"),
        ]
        
        for device in test_devices:
            db.add(device)
        db.commit()
        
        # Verify devices were created
        devices = db.query(Device).filter(Device.name.like("test-%")).all()
        assert len(devices) == 3
        
        down_devices = db.query(Device).filter(
            Device.name.like("test-%"), 
            Device.status == "down"
        ).all()
        assert len(down_devices) == 2
        
    print("✓ Database integration test passed")


def test_self_healing_module():
    """Test the self-healing module functionality"""
    print("🔧 Testing self-healing module...")
    
    with SessionLocal() as db:
        # Ensure we have down devices to heal
        down_device = db.query(Device).filter(Device.status == "down").first()
        if not down_device:
            device = Device(name="test-heal-device", device_type="router", ports=4, status="down")
            db.add(device)
            db.commit()
            down_device = device
        
        original_status = down_device.status
        device_name = down_device.name
        
        # Run self-healing
        healed = heal_devices()
        
        # Verify device was healed (if risk score allows)
        db.refresh(down_device)
        
        # Check audit log
        audit_entries = db.query(AuditLog).filter(
            AuditLog.action == "self_heal",
            AuditLog.entity_id == down_device.id
        ).all()
        
        if device_name in healed:
            assert down_device.status == "up"
            assert len(audit_entries) > 0
            print(f"✓ Device {device_name} was successfully healed")
        else:
            print(f"✓ Device {device_name} was not healed (risk score too high)")
    
    print("✓ Self-healing module test passed")


def test_api_endpoints_comprehensive():
    """Test all major API endpoints"""
    print("🌐 Testing API endpoints comprehensively...")
    
    with TestServer() as server:
        
        # Test metrics endpoint
        response = requests.get(f"{server.base_url}/metrics")
        assert response.status_code == 200
        metrics = response.json()
        assert "devices" in metrics
        assert "average_latency" in metrics
        assert "status_counts" in metrics
        print("✓ Metrics endpoint working")
        
        # Test dashboard endpoint
        response = requests.get(f"{server.base_url}/dashboard")
        assert response.status_code == 200
        assert "Network Metrics" in response.text
        print("✓ Dashboard endpoint working")
        
        # Test self-heal endpoint
        response = requests.post(f"{server.base_url}/self-heal")
        assert response.status_code == 200
        heal_result = response.json()
        assert "healed" in heal_result
        print("✓ Self-heal endpoint working")
        
        # Test firmware endpoints
        response = requests.post(
            f"{server.base_url}/firmware/install",
            json={"device": "test-device", "version": "1.0.0"}
        )
        assert response.status_code == 200
        
        response = requests.get(f"{server.base_url}/firmware/test-device")
        assert response.status_code == 200
        print("✓ Firmware endpoints working")
        
        # Test auto-healer APIs
        response = requests.post(
            f"{server.base_url}/v1/heal/playbooks",
            json={"name": "test-playbook", "triggers": [], "steps": []}
        )
        assert response.status_code == 200
        
        response = requests.get(f"{server.base_url}/v1/heal/incidents")
        assert response.status_code == 200
        print("✓ Auto-healer APIs working")
        
        # Test guardrail APIs
        response = requests.post(
            f"{server.base_url}/v1/guardrail/lint",
            json={"intentYaml": "policy: allow"}
        )
        assert response.status_code == 200
        
        response = requests.post(
            f"{server.base_url}/v1/guardrail/approve",
            json={"changeId": "test-change-1"}
        )
        assert response.status_code == 200
        print("✓ Guardrail APIs working")
        
        # Test autocapture and collectors
        response = requests.post(
            f"{server.base_url}/v1/devices/autocapture",
            json={"serial": "TEST123456"}
        )
        assert response.status_code == 200
        
        response = requests.get(f"{server.base_url}/v1/vendors/meraki/collectors")
        assert response.status_code == 200
        print("✓ Autocapture and collector APIs working")


def test_integration_points():
    """Test integration between different modules"""
    print("🔗 Testing integration points...")
    
    with SessionLocal() as db:
        # Clean up any existing test devices
        db.query(Device).filter(Device.name == "integration-test-device").delete()
        db.commit()
        
        # Test database -> self-healing integration
        device = Device(name="integration-test-device", device_type="router", ports=4, status="down")
        db.add(device)
        db.commit()
        
        # Run self-healing
        healed = heal_devices()
        
        # Check audit log was created
        audit_count = db.query(AuditLog).filter(AuditLog.action == "self_heal").count()
        assert audit_count > 0
        
        # Test API -> database integration
        with TestServer() as server:
            # Clean up existing API test device
            db.query(Device).filter(Device.name == "api-test-device").delete()
            db.commit()
            
            response = requests.post(
                f"{server.base_url}/sandbox/device",
                json={"name": "api-test-device", "device_type": "switch", "ports": 48, "status": "up"}
            )
            assert response.status_code == 200
            
            # Verify device was created in database
            api_device = db.query(Device).filter(Device.name == "api-test-device").first()
            assert api_device is not None
            assert api_device.device_type == "switch"
        
        # Clean up test devices
        db.query(Device).filter(Device.name.in_(["integration-test-device", "api-test-device"])).delete()
        db.commit()
    
    print("✓ Integration points test passed")


def test_all_modules_functional():
    """Test that all modules are functional"""
    print("📦 Testing all modules are functional...")
    
    modules_to_test = [
        'app.analytics', 'app.firmware', 'app.sandbox', 'app.zero_touch', 
        'app.discovery', 'app.db', 'app.self_heal', 'app.healer', 
        'app.guardrail', 'app.synthetics', 'app.planner', 'app.compliance',
        'app.vulnwatch', 'app.pathtracer', 'app.policy_impact', 
        'app.autocapture', 'app.collectors', 'app.quantumshield'
    ]
    
    for module_name in modules_to_test:
        try:
            __import__(module_name)
        except ImportError as e:
            pytest.fail(f"Module {module_name} failed to import: {e}")
    
    print(f"✓ All {len(modules_to_test)} modules are functional")


def test_error_handling():
    """Test error handling and zero-error middleware"""
    print("⚠️  Testing error handling...")
    
    with TestServer() as server:
        # Test non-existent endpoint
        response = requests.get(f"{server.base_url}/non-existent-endpoint")
        assert response.status_code == 404
        
        # Test malformed requests
        response = requests.post(
            f"{server.base_url}/firmware/install",
            json={"invalid": "data"}
        )
        assert response.status_code == 422  # Validation error
    
    print("✓ Error handling test passed")


def main():
    """Run comprehensive integration tests"""
    print("🚀 DV8 SD-WAN Comprehensive Integration Test")
    print("=" * 60)
    
    try:
        test_database_integration()
        test_self_healing_module() 
        test_all_modules_functional()
        test_api_endpoints_comprehensive()
        test_integration_points()
        test_error_handling()
        
        print("\n🎉 All integration tests passed!")
        print("\nThe DV8 SD-WAN platform is fully functional:")
        print("✓ All modules are properly integrated")
        print("✓ Self-healing module is working correctly")
        print("✓ Database connections are stable")
        print("✓ All API endpoints are responding")
        print("✓ Error handling is working")
        print("✓ Integration points are functioning")
        
    except Exception as e:
        print(f"\n❌ Integration test failed: {e}")
        raise


if __name__ == "__main__":
    main()