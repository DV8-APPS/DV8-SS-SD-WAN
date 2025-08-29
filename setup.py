#!/usr/bin/env python3
"""
DV8 SD-WAN One-Click Setup and Installation Script

This script provides comprehensive setup, dependency installation,
database initialization, and system integration for the DV8 SD-WAN platform.
"""

import os
import sys
import subprocess
import shutil
import importlib.util
from pathlib import Path


def check_python_version():
    """Ensure Python 3.8+ is available"""
    if sys.version_info < (3, 8):
        print("❌ Python 3.8+ is required")
        sys.exit(1)
    print(f"✓ Python {sys.version.split()[0]} detected")


def check_system_dependencies():
    """Check for required system dependencies"""
    print("\n🔍 Checking system dependencies...")
    
    # Check for dotnet SDK
    if not shutil.which("dotnet"):
        print("❌ .NET SDK not found. Please install .NET SDK 6.0+")
        print("   Visit: https://dotnet.microsoft.com/download")
        sys.exit(1)
    else:
        try:
            result = subprocess.run(["dotnet", "--version"], capture_output=True, text=True)
            version = result.stdout.strip()
            print(f"✓ .NET SDK {version} detected")
            
            # Validate minimum version
            version_parts = version.split('.')
            if len(version_parts) >= 1 and int(version_parts[0]) < 6:
                print("⚠️  .NET SDK 6.0+ recommended for optimal performance")
        except Exception:
            print("❌ .NET SDK installation appears corrupted")
            sys.exit(1)
    
    # Check for git (often needed for dependencies)
    if not shutil.which("git"):
        print("⚠️  Git not found. Some features may not work correctly")
    else:
        print("✓ Git detected")
    
    # Check disk space
    disk_usage = shutil.disk_usage(".")
    free_gb = disk_usage.free / (1024**3)
    if free_gb < 1.0:
        print(f"⚠️  Low disk space: {free_gb:.1f}GB available")
    else:
        print(f"✓ Disk space available: {free_gb:.1f}GB")
    
    # Check network connectivity
    try:
        import socket
        socket.create_connection(("8.8.8.8", 53), timeout=3)
        print("✓ Network connectivity verified")
    except OSError:
        print("⚠️  Network connectivity issues detected")


def install_python_dependencies():
    """Install Python dependencies"""
    print("\n📦 Installing Python dependencies...")
    
    requirements_file = Path("requirements.txt")
    if not requirements_file.exists():
        print("❌ requirements.txt not found")
        sys.exit(1)
    
    try:
        subprocess.run([
            sys.executable, "-m", "pip", "install", "-r", "requirements.txt"
        ], check=True)
        print("✓ Python dependencies installed successfully")
    except subprocess.CalledProcessError:
        print("❌ Failed to install Python dependencies")
        sys.exit(1)


def verify_module_imports():
    """Verify all application modules can be imported"""
    print("\n🔍 Verifying module imports...")
    
    modules = [
        'app.analytics', 'app.firmware', 'app.sandbox', 'app.zero_touch', 
        'app.discovery', 'app.db', 'app.self_heal', 'app.healer', 
        'app.guardrail', 'app.synthetics', 'app.planner', 'app.compliance',
        'app.vulnwatch', 'app.pathtracer', 'app.policy_impact', 
        'app.autocapture', 'app.collectors', 'app.quantumshield'
    ]
    
    failed = []
    for module in modules:
        try:
            importlib.import_module(module)
        except Exception as e:
            failed.append((module, str(e)))
    
    if failed:
        print("❌ Some modules failed to import:")
        for module, error in failed:
            print(f"   {module}: {error}")
        sys.exit(1)
    else:
        print(f"✓ All {len(modules)} modules imported successfully")


def initialize_database():
    """Initialize the database with all required tables"""
    print("\n🗄️  Initializing database...")
    
    try:
        from app.db import init_db, engine, SessionLocal, Device, AuditLog
        from sqlalchemy import inspect
        
        # Initialize database tables
        init_db()
        
        # Verify tables were created
        inspector = inspect(engine)
        tables = inspector.get_table_names()
        expected_tables = ['devices', 'firmware', 'zero_touch', 'warnings', 'audit_log']
        
        missing_tables = set(expected_tables) - set(tables)
        if missing_tables:
            print(f"❌ Missing database tables: {missing_tables}")
            sys.exit(1)
        
        # Test basic database operations
        with SessionLocal() as db:
            # Add a test audit log entry
            audit = AuditLog(action="system_init", entity="setup", details="Database initialized")
            db.add(audit)
            db.commit()
        
        print(f"✓ Database initialized with {len(tables)} tables")
        
    except Exception as e:
        print(f"❌ Database initialization failed: {e}")
        sys.exit(1)


def test_api_endpoints():
    """Test that the application starts and key endpoints respond"""
    print("\n🧪 Testing API endpoints...")
    
    import threading
    import time
    import requests
    from app.main import app
    import uvicorn
    
    # Start server in background thread
    server_thread = None
    try:
        def run_server():
            uvicorn.run(app, host="127.0.0.1", port=8001, log_level="critical")
        
        server_thread = threading.Thread(target=run_server, daemon=True)
        server_thread.start()
        
        # Wait for server to start
        time.sleep(3)
        
        # Test endpoints
        endpoints = [
            ("GET", "/metrics", "Metrics endpoint"),
            ("GET", "/dashboard", "Dashboard endpoint"),
            ("POST", "/self-heal", "Self-heal endpoint"),
        ]
        
        for method, endpoint, description in endpoints:
            try:
                if method == "GET":
                    response = requests.get(f"http://127.0.0.1:8001{endpoint}", timeout=5)
                else:
                    response = requests.post(f"http://127.0.0.1:8001{endpoint}", timeout=5)
                
                if response.status_code < 400:
                    print(f"✓ {description} ({response.status_code})")
                else:
                    print(f"⚠️  {description} returned {response.status_code}")
            except Exception as e:
                print(f"❌ {description} failed: {e}")
        
    except Exception as e:
        print(f"❌ Server test failed: {e}")
    finally:
        # Server will be killed when main process exits due to daemon thread
        pass


def run_integration_tests():
    """Run the full test suite"""
    print("\n🧪 Running integration tests...")
    
    try:
        result = subprocess.run([
            sys.executable, "-m", "pytest", "tests/", "-v", "--tb=short"
        ], capture_output=True, text=True)
        
        if result.returncode == 0:
            print("✓ All integration tests passed")
        else:
            print("❌ Some integration tests failed")
            print("Test output:")
            print(result.stdout)
            if result.stderr:
                print("Errors:")
                print(result.stderr)
    except Exception as e:
        print(f"❌ Failed to run tests: {e}")


def create_launch_script():
    """Create a convenient launch script"""
    print("\n📝 Creating launch script...")
    
    launch_script = Path("run_dv8.py")
    launch_content = '''#!/usr/bin/env python3
"""
DV8 SD-WAN Application Launcher
"""

import uvicorn
from app.main import app

if __name__ == "__main__":
    print("🚀 Starting DV8 SD-WAN Console...")
    print("   Access the dashboard at: http://localhost:8000/dashboard")
    print("   API documentation at: http://localhost:8000/docs")
    print("   Press Ctrl+C to stop")
    
    uvicorn.run(
        app, 
        host="0.0.0.0", 
        port=8000, 
        reload=True,
        log_level="info"
    )
'''
    
    with open(launch_script, 'w') as f:
        f.write(launch_content)
    
    # Make executable on Unix systems
    if os.name != 'nt':
        os.chmod(launch_script, 0o755)
    
    print(f"✓ Launch script created: {launch_script}")


def main():
    """Main setup function"""
    print("🚀 DV8 SD-WAN One-Click Setup")
    print("=" * 50)
    
    # Change to script directory
    script_dir = Path(__file__).parent
    os.chdir(script_dir)
    
    # Run all setup steps
    check_python_version()
    check_system_dependencies()
    install_python_dependencies()
    verify_module_imports()
    initialize_database()
    test_api_endpoints()
    run_integration_tests()
    create_launch_script()
    
    print("\n🎉 Setup completed successfully!")
    print("\nNext steps:")
    print("1. Run the application: python run_dv8.py")
    print("2. Access the dashboard: http://localhost:8000/dashboard")
    print("3. View API docs: http://localhost:8000/docs")
    print("4. Check metrics: http://localhost:8000/metrics")
    print("\nFor development, use: uvicorn app.main:app --reload")


if __name__ == "__main__":
    main()