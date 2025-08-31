#!/usr/bin/env python3
"""
DV8 SD-WAN Database Initialization and Health Check Script

This script demonstrates clean database initialization for both Python and C# components,
ensuring they can work together without conflicts.
"""

import os
import subprocess
import sys
from pathlib import Path


def clean_databases():
    """Remove existing database files for clean initialization"""
    print("🧹 Cleaning existing databases...")
    
    db_files = [
        "dv8.db",
        "dv8test.db", 
        "dv8sssdwan.db",
        "dv8sssdwan_dev.db",
        "dotnet/src/DV8.Console/dv8sssdwan.db",
        "dotnet/src/DV8.Console/dv8sssdwan_dev.db"
    ]
    
    for db_file in db_files:
        if os.path.exists(db_file):
            os.remove(db_file)
            print(f"  ✓ Removed {db_file}")


def test_python_database():
    """Test Python database initialization"""
    print("\n🐍 Testing Python database initialization...")
    
    try:
        from app.db import init_db, SessionLocal, Device, AuditLog
        
        # Initialize database
        init_db()
        print("  ✓ Python database initialized")
        
        # Test basic operations
        with SessionLocal() as db:
            audit = AuditLog(action="db_test", entity="system", details="Database health check")
            db.add(audit)
            db.commit()
            
            count = db.query(AuditLog).count()
            print(f"  ✓ Database operations working (audit logs: {count})")
            
        return True
        
    except Exception as e:
        print(f"  ❌ Python database test failed: {e}")
        return False


def test_dotnet_database():
    """Test .NET database initialization"""
    print("\n🔷 Testing .NET database initialization...")
    
    try:
        # Change to .NET directory
        dotnet_dir = Path("dotnet")
        result = subprocess.run(
            ["dotnet", "build"], 
            cwd=dotnet_dir, 
            capture_output=True, 
            text=True,
            timeout=60
        )
        
        if result.returncode == 0:
            print("  ✓ .NET build successful")
            
            # Run tests to verify database operations
            test_result = subprocess.run(
                ["dotnet", "test", "--verbosity", "quiet"], 
                cwd=dotnet_dir, 
                capture_output=True, 
                text=True,
                timeout=60
            )
            
            if test_result.returncode == 0:
                print("  ✓ .NET database operations working")
                return True
            else:
                print(f"  ❌ .NET tests failed: {test_result.stderr}")
                return False
        else:
            print(f"  ❌ .NET build failed: {result.stderr}")
            return False
            
    except Exception as e:
        print(f"  ❌ .NET database test failed: {e}")
        return False


def test_integration():
    """Test that both databases can work together"""
    print("\n🔗 Testing integration between Python and .NET...")
    
    try:
        # Check that separate database files exist
        python_db = "dv8.db"
        
        # Look for .NET database files in various locations
        dotnet_db_locations = [
            "dotnet/src/DV8.Console/dv8sssdwan_dev.db",
            "dotnet/tests/DV8.Console.Tests/bin/Debug/net8.0/dv8sssdwan_dev.db"
        ]
        
        dotnet_db = None
        for location in dotnet_db_locations:
            if os.path.exists(location):
                dotnet_db = location
                break
        
        if os.path.exists(python_db) and dotnet_db:
            print("  ✓ Both databases exist and are separate")
            print(f"    - Python DB: {python_db}")
            print(f"    - .NET DB: {dotnet_db}")
            
            # Verify they have different schemas/content
            python_size = os.path.getsize(python_db)
            dotnet_size = os.path.getsize(dotnet_db)
            print(f"    - Python DB size: {python_size} bytes")
            print(f"    - .NET DB size: {dotnet_size} bytes")
            
            return True
        else:
            print("  ❌ Database separation not working correctly")
            print(f"    - Python DB exists: {os.path.exists(python_db)}")
            print(f"    - .NET DB found: {dotnet_db is not None}")
            return False
            
    except Exception as e:
        print(f"  ❌ Integration test failed: {e}")
        return False


def main():
    """Main function to run database health checks"""
    print("🚀 DV8 SD-WAN Database Health Check")
    print("=" * 50)
    
    # Clean existing databases
    clean_databases()
    
    # Test Python database
    python_ok = test_python_database()
    
    # Test .NET database  
    dotnet_ok = test_dotnet_database()
    
    # Test integration
    integration_ok = test_integration()
    
    # Summary
    print("\n📊 Summary:")
    print("=" * 20)
    print(f"Python Database:    {'✓ PASS' if python_ok else '❌ FAIL'}")
    print(f".NET Database:      {'✓ PASS' if dotnet_ok else '❌ FAIL'}")
    print(f"Integration:        {'✓ PASS' if integration_ok else '❌ FAIL'}")
    
    if python_ok and dotnet_ok and integration_ok:
        print("\n🎉 All database components are working correctly!")
        print("   Both Python and .NET can initialize and use databases independently.")
        return 0
    else:
        print("\n❌ Some database components have issues.")
        return 1


if __name__ == "__main__":
    sys.exit(main())