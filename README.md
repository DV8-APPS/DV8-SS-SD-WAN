# DV8 Quantum Shield

Prototype console for SD-WAN and router/switch management backed by SQLite with an audit log for all device operations.

QuantumShield primitives are stubbed end-to-end. All privileged endpoints emit an `X-DV8-Decision` header and accept the `X-DV8-DBGID` credential, mirroring the production controller's adjudication flow.

## Running the console

```bash
pip install -r requirements.txt
# install .NET SDK 8.0 (Ubuntu example)
apt-get update && apt-get install -y dotnet-sdk-8.0
uvicorn app.main:app --reload
```

The dashboard is served at `http://localhost:8000/dashboard` and includes
basic analytics such as average latency and device status counts.

On startup the FastAPI application verifies that core dependencies are
installed, failing fast if any required modules are missing. It also
preloads the .NET SDK by running `dotnet --info`; if the SDK is
unavailable the service will terminate on startup.

State is persisted in a SQLite database via SQLAlchemy. Firmware, zero-touch, and sandbox modules share a unified device table and every mutation is recorded in an `audit_log` table for traceability.

Discovery jobs can be started to automatically identify devices on the network:

- `POST /discovery/jobs` to start a discovery job
- `GET /discovery/jobs/{id}` to retrieve job status
- `GET /discovery/candidates?tenant=acme` to list discovered candidates

Firmware installations can be simulated via the API:

- `POST /firmware/install` with JSON body `{ "device": "router-1", "version": "1.2.3" }`
- `GET /firmware/{device}` to retrieve the installed version.

### Sandboxed device simulation

The API can track simulated routers or switches with configurable
ports, warning lights and status.

- `POST /sandbox/device` with JSON body
  `{ "name": "sim-router", "device_type": "router", "ports": 8 }`
- `PUT /sandbox/device/{name}` to update status and warning lights
  (e.g. `{ "status": "down", "warnings": { "temperature": true } }`)
- `GET /sandbox/summary` to retrieve aggregate counts of devices,
  ports, statuses and active warnings.

### Zero-touch configuration management

Devices can enroll with a configuration template and mark when the template
has been applied:

- `POST /zero-touch/enroll` with `{ "name": "ztp-router", "template": "basic-router" }`
- `POST /zero-touch/device/{name}/applied` to indicate a device finished
  applying its template
- `GET /zero-touch/device/{name}` or `/zero-touch/devices` to retrieve
  enrolled devices and their application status

### Auto-capture & vendor collectors

Devices can auto-populate hostname, management IP and OS version using a
serial number, and vendor-specific collectors are exposed for inventory and
health telemetry:

- `POST /v1/devices/autocapture` with `{ "serial": "ABC123" }`
- `GET /v1/vendors/meraki/collectors`

### Self-healing and zero-error runtime

The console preloads all Python dependencies at startup and registers a
zero-error middleware. Any unhandled exception is logged to the audit trail
and returned as a generic error response. The companion ASP.NET Core API
preloads its singleton services at launch and uses a global error-handling
middleware to record unexpected failures and append the `X-DV8-Decision`
header on every response.

AI-driven self-healing can be triggered via:

- `POST /self-heal` to automatically recover devices in the sandbox.

### Compliance scanning and vulnerability watch

The console can simulate compliance checks and vulnerability database
syncs:

- `POST /v1/compliance/scans` with `{ "profile": "DISA" }` to start a
  scan
- `GET /v1/compliance/results?profile=DISA&device=router-1` to retrieve
  results
- `POST /v1/vulnwatch/sync` to fetch CVE advisories
- `GET /v1/vulnwatch/findings?device=router-1` to list vulnerabilities

### SD-WAN path tracing and policy impact

- `GET /v1/sdwan/pathtracer?src=a&dst=b` returns hop-by-hop metrics
- `GET /v1/policy/impact?policyId=p1&before=old&after=new` shows policy
  impact timelines

## Testing

```bash
pytest
```

## C# Web API

A lightweight ASP.NET Core service offers equivalent firmware, sandbox, and zero-touch endpoints.

Run the API:

```bash
cd dotnet
dotnet run --project src/DV8.Console
```

Run the .NET tests:

```bash
cd dotnet
dotnet test
```

The ASP.NET Core service preloads its singleton services at launch to ensure
required dependencies are available.
