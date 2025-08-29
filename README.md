# DV8 Sovereign SD-WAN & Network Management Console

Complete sovereign SD-WAN and network management platform with comprehensive device management, self-healing capabilities, and integrated security controls.

## ✨ Features

- **Sovereign SD-WAN Management**: Complete control plane for SD-WAN infrastructure
- **Self-Healing Automation**: AI-driven automatic device recovery and optimization
- **Quantum Shield Integration**: Advanced security and risk assessment
- **Zero-Touch Provisioning**: Automated device enrollment and configuration
- **Real-time Analytics**: Network metrics, device monitoring, and performance insights
- **Compliance & Security**: DISA compliance scanning and vulnerability management
- **Multi-Vendor Support**: Unified management across different network vendors

## 🚀 One-Click Installation

The fastest way to get DV8 SD-WAN running:

```bash
# Clone the repository
git clone https://github.com/DV8-TECHNOLOGY-GROUP/DV8-SS-SD-WAN.git
cd DV8-SS-SD-WAN

# One-click setup: installs all dependencies, initializes database, runs tests
python setup.py

# Start the application
python run_dv8.py
```

Or using Make:
```bash
make install  # Complete setup and testing
make run      # Start the application
```

## 📋 Requirements

- **Python 3.8+** 
- **.NET SDK 8.0+** (for C# components)
- **Git** (recommended)

## 🛠️ Manual Installation

```bash
# Install Python dependencies
pip install -r requirements.txt

# Verify system dependencies
make health

# Initialize database
python -c "from app.db import init_db; init_db()"

# Run tests
make test

# Start development server
make run-dev
```

## 🌐 Access Points

After installation, access the platform at:

- **Dashboard**: http://localhost:8000/dashboard
- **API Documentation**: http://localhost:8000/docs
- **Metrics**: http://localhost:8000/metrics
- **Self-Heal**: http://localhost:8000/self-heal

## 🔧 Key Components

### Self-Healing Module
Automatically detects and repairs network issues:
```bash
# Trigger self-healing
curl -X POST http://localhost:8000/self-heal

# View healing history
curl http://localhost:8000/metrics
```

### Device Management
Complete lifecycle management for network devices:

```bash
# Register a device
curl -X POST http://localhost:8000/sandbox/device \
  -H "Content-Type: application/json" \
  -d '{"name": "router-1", "device_type": "router", "ports": 4}'

# Zero-touch enrollment
curl -X POST http://localhost:8000/zero-touch/enroll \
  -H "Content-Type: application/json" \
  -d '{"name": "ztp-router", "template": "enterprise-router"}'
```

### Firmware Management
Centralized firmware deployment and tracking:

```bash
# Install firmware
curl -X POST http://localhost:8000/firmware/install \
  -H "Content-Type: application/json" \
  -d '{"device": "router-1", "version": "2.1.0"}'

# Check firmware status
curl http://localhost:8000/firmware/router-1
```

### Auto-Healer APIs
Advanced healing playbooks and incident management:

```bash
# Create healing playbook
curl -X POST http://localhost:8000/v1/heal/playbooks \
  -H "Content-Type: application/json" \
  -d '{"name": "network-recovery", "triggers": [], "steps": []}'

# List incidents
curl http://localhost:8000/v1/heal/incidents
```

### GuardRail APIs
Policy validation and change approval:

```bash
# Lint configuration
curl -X POST http://localhost:8000/v1/guardrail/lint \
  -H "Content-Type: application/json" \
  -d '{"intentYaml": "policy: allow"}'

# Approve changes
curl -X POST http://localhost:8000/v1/guardrail/approve \
  -H "Content-Type: application/json" \
  -d '{"changeId": "change-123"}'
```

### Device Discovery
Automated network discovery and inventory:

```bash
# Start discovery job
curl -X POST http://localhost:8000/discovery/jobs \
  -H "Content-Type: application/json" \
  -d '{"tenant": "enterprise", "scopes": ["192.168.1.0/24"]}'

# Check discovery results
curl http://localhost:8000/discovery/candidates?tenant=enterprise
```

### Compliance & Security
Comprehensive security scanning and vulnerability management:

```bash
# Start compliance scan
curl -X POST http://localhost:8000/v1/compliance/scans \
  -H "Content-Type: application/json" \
  -d '{"profile": "DISA"}'

# Sync vulnerability database
curl -X POST http://localhost:8000/v1/vulnwatch/sync

# Get vulnerability findings
curl http://localhost:8000/v1/vulnwatch/findings?device=router-1
```

## 🧪 Testing

```bash
# Run unit tests
make test

# Run comprehensive integration tests
make test-integration

# Run specific test file
python -m pytest tests/test_app.py -v
```

## 📊 Database Schema

The platform uses SQLite with the following key tables:

- **devices**: Core device inventory
- **firmware**: Firmware version tracking
- **zero_touch**: Zero-touch provisioning configs
- **warnings**: Device warning and alert states
- **audit_log**: Complete audit trail of all operations

## 🔧 Development

Start the development server with hot reload:

```bash
make run-dev
# or
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000
```

## 🏗️ Architecture

- **FastAPI**: Modern Python web framework with automatic OpenAPI docs
- **SQLAlchemy**: Database ORM with migration support
- **Pydantic**: Data validation and serialization
- **Jinja2**: Template engine for web interface
- **.NET Core**: Complementary C# API for legacy integration

## 🛡️ Security Features

- **QuantumShield Integration**: Advanced threat detection and risk scoring
- **Zero-Error Middleware**: Comprehensive error handling and logging
- **Audit Trail**: Complete operation logging for compliance
- **Decision Headers**: Policy enforcement on all endpoints

## 📊 UX Requirements & Standards Compliance

The DV8 SD-WAN Console follows international standards for user experience and accessibility:

### Standards Implemented
- **ISO/IEC/IEEE 29148**: Requirements format with formal SHALL statements
- **WCAG 2.2 AA**: Web accessibility guidelines with full compliance
- **ISO 9241-210**: Human-centred design lifecycle
- **ISO/IEC 25010**: Software quality model (usability, performance, security)
- **OWASP ASVS v4.0+**: Application security verification standard
- **NIST SP 800-63-3**: Digital identity guidelines

### Key UX Enhancements
- **Accessibility-First Design**: Full keyboard navigation, screen reader support, enhanced contrast
- **Semantic HTML Structure**: Proper landmarks, headings, and ARIA annotations  
- **Live Regions**: Dynamic content announcements for assistive technologies
- **Focus Management**: Visible focus indicators and logical tab order
- **Error Handling**: Accessible validation with clear, actionable messages

### Documentation
- **[UX_REQUIREMENTS.md](UX_REQUIREMENTS.md)**: Formal requirements with testable SHALL statements
- **[SRS_INTEGRATION.md](SRS_INTEGRATION.md)**: Traceability matrix linking UX to SRS requirements
- **[COMPLIANCE_VERIFICATION.md](COMPLIANCE_VERIFICATION.md)**: CI gates and verification methods
- **[UX_TRACEABILITY_MATRIX.md](UX_TRACEABILITY_MATRIX.md)**: End-to-end requirement traceability

### Testing & Verification
- **Automated Accessibility Testing**: Zero critical violations in CI
- **Performance Monitoring**: Core Web Vitals compliance framework
- **Security Gates**: OWASP scanning and ASVS validation
- **Cross-browser Compatibility**: Modern browser support with graceful degradation

## 📝 Documentation

- **API Docs**: Available at `/docs` when running
- **Specifications**: See `DV8_SPEC.md` for detailed technical specifications
- **Requirements**: See `TECH_REQUIREMENTS.md` for implementation details

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests: `make test`
5. Submit a pull request

## 📄 License

This project is part of the DV8 Technology Group sovereign network management suite.

## 🆘 Support

For issues and support:
1. Check the logs: `tail -f dv8.db` 
2. Run health check: `make health`
3. Review API docs: http://localhost:8000/docs
4. Run diagnostic tests: `make test-integration`
