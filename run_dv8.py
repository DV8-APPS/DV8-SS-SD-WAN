#!/usr/bin/env python3
"""
DV8 SD-WAN Application Launcher
"""

import uvicorn

if __name__ == "__main__":
    print("🚀 Starting DV8 SD-WAN Console...")
    print("   Access the dashboard at: http://localhost:8000/dashboard")
    print("   API documentation at: http://localhost:8000/docs")
    print("   Press Ctrl+C to stop")
    
    uvicorn.run(
        "app.main:app", 
        host="0.0.0.0", 
        port=8000, 
        reload=True,
        log_level="info"
    )
