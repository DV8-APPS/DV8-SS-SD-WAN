# DV8 Sovereign SD-WAN & Network Management

## Developer Technical Specification (Build-Ready)

## 0) Outcomes & non-negotiables

* **Sovereignty:** All control/telemetry keys, logs, and ML artefacts remain under national jurisdiction.
* **Post-Quantum by design:** ECC default; lattice KEM/signature profiles ready; HOAC™ switches under risk.
* **Zero-Trust at the session grain:** Identity = biometric+geo+time (DBGID™). Decrypt nothing by default.
* **Field-level confidentiality:** Per-field AEAD (SET™) + optional linguistic camouflage (LSO™).
* **Tamper-evident audit:** Every sensitive action immutably anchored (LedgerProof™).
* **Edge resilience:** Air-gap/low-bandwidth operability; offline capsules; A/B OTA with anti-rollback.

---

## 1) System architecture (runnable mental model)

**Planes:**

* **Management plane:** React/Next console, tablet field app. Auth via mTLS+DBGID; RBAC/ABAC; audit-only mode for regulators.
* **Control plane:** Policy compiler → Capsule Control Plane (CCP™) → device/overlay drivers.
* **Data plane:** SD-WAN forwarding (DPDK/XDP), IPsec/WireGuard overlays, PQC-agile KEX, QoS/FEC/AQM.

**Core subsystems:**

* **QuantumShield Crypto:** DBGID™ (biometric+geo+time KDF), SET™ (per-field AEAD), HOAC™ (ECC↔PQC), LSO™ (camouflage).
* **AIC™ capsules:** SessionOracle™, EntropyGuard™, KeySurgeon™, VectorMind™; hot-pluggable, explainable, versioned.
* **Telemetry & detection:** APE-CIR™ (entropy drift, geo velocity, behaviour vector) with hard/soft lockdowns via CCP™.
* **Audit:** LedgerProof™ (permissioned chain), plus Elastic/Loki mirrors for search; export packs for ISO/PCI/SOC2.

**Infra profile:**

* **Kubernetes 1.27+** (Calico L3 policies), **Vault** (HSM/TPM backed), **MinIO** (S3-compatible sovereign object store), **Falco** (eBPF runtime), **OPA/Gatekeeper** (admission), **OpenTelemetry** (traces/metrics).

---

## 2) Northbound APIs (management, automation, audit)

**Protocol set:** gRPC (control), REST (admin/3rd-party), WebSocket (streamed insights), OpenAPI 3.1 + protobuf.

### 2.1 Policy & SD-WAN intent (gRPC)

```proto
// dv8/policy/v1/policy.proto
syntax = "proto3";
package dv8.policy.v1;

message AppClass { string name=1; repeated string match_dpi=2; int32 priority=3; }
message PathPref { string name=1; int32 max_latency_ms=2; float max_loss=3; int32 max_jitter_ms=4; }
message SegmentPolicy {
  string id=1; string tenant=2; repeated AppClass apps=3; repeated PathPref paths=4;
  bool pqc_required=5; // force HOAC->PQC for this segment
}
service PolicyService {
  rpc UpsertSegment(SegmentPolicy) returns (SegmentPolicy);
  rpc GetSegmentById(Id) returns (SegmentPolicy);
  rpc ListSegments(Tenant) returns (stream SegmentPolicy);
}
```

### 2.2 QuantumShield envelope (REST)

```http
POST /v1/crypto/envelope/decrypt
Headers:
  X-DV8-DBGID: <hex>         ; biometric+geo+time KDF
  X-Geo: lat,lon,alt         ; signed by device cert
  X-Device-Cert: mTLS peer   ; hardware-bound
Body: { "fields":[{"name":"passport_id","ciphertext":"...","iv":"..."}] }
```

Response returns **only** authorised fields; CCP gates per role/zone/risk.

### 2.3 Capsule Control Plane (CCP) adjudication (gRPC)

```proto
// dv8/ccp/v1/ccp.proto
message AdjudicationReq {
  string capsule; string user; string device_id; string zone;
  uint32 identity_score; uint32 risk_score; bool pqc_requested;
}
message AdjudicationRes { enum Decision {ALLOW=0; ESCALATE=1; LOCK=2; DENY=3;}
  Decision decision=1; string reason=2; map<string,string> obligations=3; // e.g. "pqc":"on"
}
service CCP { rpc Adjudicate(AdjudicationReq) returns (AdjudicationRes); }
```

### 2.4 Audit export (REST)

* `GET /v1/audit/bundle?case=<id>&format=pdf|json` → immutable, signed pack; regulator-safe redactions applied.

---

## 3) Southbound & device management (routers/switches)

### 3.1 Protocols

* **Zero-touch provisioning:** Secure bootstrap via mTLS + short-lived enrollment token + TPM attestation.
* **Config/telemetry:** gNMI (OpenConfig + DV8 extensions), NETCONF/YANG (for legacy), SNMPv3 (read-only minimal).
* **Streaming telemetry:** gNMI Subscribe + OTLP; device pushes APE-CIR features (latency/loss, entropy deltas, keystroke timing for local UI, geo deltas).
* **Zero-touch configuration management:** Devices pull signed configuration templates from the CCP at first boot; templates follow IETF and vendor best practices and can be versioned and atomically applied via gNMI/gNOI.

### 3.2 DV8 YANG augment (extract)

```yang
module dv8-quantumshield {
  namespace "urn:dv8:qs"; prefix qs;
  container quantumshield {
    leaf pqc-required { type boolean; default false; }
    container dbg-id { leaf enabled { type boolean; default true; } }
    list set-field { key "name"; leaf name { type string; }
      leaf kdf { type enumeration { enum DBGID; enum PBKDF2; } }
      leaf aead { type enumeration { enum AES256_GCM; } }
    }
    container lso { leaf enabled { type boolean; default false; } }
  }
}
```

### 3.3 Sandboxed device simulation

For development and offline testing the console exposes an in-memory
sandbox where virtual routers and switches can be registered with a
dynamic number of network ports, warning lights and status fields. The
sandbox API supports registering devices, updating their state and
retrieving aggregate summaries such as total ports or active warnings.

---

## 4) Cryptography wiring (implementable details)

### 4.1 Key hierarchy & flows

* **Root of trust:** Device TPM/TEE + manufacturer root + DV8 intermediate; secure boot verified chain.
* **Session seed:** `DBGID = SHA-512(biometric_hash || geohash || timestamp)`; derive ephemeral keys via HKDF.
* **Per-field AEAD (SET™):** Unique key+IV per field; AAD binds tenant, role, and ledger pointer.
* **Transport:** TLS 1.3 mTLS everywhere; Control-plane supports **hybrid KEX** (X25519+Kyber/ML-KEM) profile; data-plane IPsec IKEv2 or WireGuard with PQC-hybrid PSK overlay option.
* **Agility (HOAC™):** ECC by default → switch to NTRU/ML-KEM on CCP mandate (risk>threshold, hostile zone, or policy `pqc-required=true`).
* **Camouflage (LSO™):** Optional wrapper on exported artefacts (e.g., regulator packs, offline bundles) to defeat classifier-based interdiction; never weakens AEAD.

### 4.2 Reference pseudocode (concise)

**DBGID → SET™ helper (Go)**

```go
func DeriveDBGID(bio, geo []byte, ts time.Time) []byte {
    h := sha512.New()
    h.Write(bio); h.Write(geo); binary.Write(h, binary.BigEndian, ts.Unix())
    return h.Sum(nil) // 64 bytes
}
func EncryptField(val []byte, dbgid []byte) (ct, iv, tag []byte) {
    salt := hkdf.Extract(sha256.New, dbgid, []byte("dv8:set:v1"))
    key := hkdf.Expand(sha256.New, salt, []byte("field-key"))[:32]
    iv  := randBytes(12)
    ct  := aesgcm(key).Seal(nil, iv, val, []byte("tenant|role|ledger"))
    return ct, iv, nil
}
```

**HOAC switch (Rust, sketch)**

```rust
fn kex(session: &Context, risk: u32) -> Secret {
  if risk >= RISK_PQC || session.policy.pqc_required { return ntru_kem(); }
  else { return ecdh_p521(); }
}
```

---

## 5) SD-WAN dataplane & path control

* **Path selection:** Multi-metric (latency/jitter/loss/cost) with **risk bias**: `score = f(qos, APE-CIR risk, entropy drift, geo trust)`.
* **App-aware steering:** DPI signatures + SNI/QUIC hints; privacy-sensitive classes are **LSO-eligible** and **PQC-preferred**.
* **Reliability:** Per-class FEC, BBR/CC tuning, active probing; brownout mode for constrained uplinks.
* **Segments:** Overlay VRFs per tenant/zone; micro-segmentation enforced by Calico + device ACLs synced from CCP.

---

## 6) Firmware & hardware (sovereign control)

### 6.1 Platform stack

* **Boot:** ROM → Verified bootloader → Measured kernel → Read-only base FS; A/B **OTA** images; anti-rollback fuses; SBOM attached.
* **OS:** Hardened Linux LTS, musl-based userspace; DPDK for high-TPS routers; eBPF/XDP for L4 filtering/telemetry.
* **Crypto modules:** FIPS-friendly boundary around AEAD/KEM/PRNG; self-tests on boot (KATs); zeroise on fault.
* **Accel:** AES-NI/ARMv8 CryptoExt; PQC via CPU (initial) with future NPU offload hooks.
* **Update channel:** Capsule-validated OTA package (signature + policy); **offline** via USB/sat bundle; LedgerProof writes provenance.

### 6.2 Device services

* **gnmid / netconfd / telemetryd / ccpgate / aic-agent / ipsec-mgr / route-orchestrator / ota-agent** — each as supervised units with health probes and signed images.

---

## 7) Data, storage, and schemas

* **Config DB:** PostgreSQL (strong consistency); CRDT-based edge cache for offline branches.
* **Events & time-series:** Loki (logs), VictoriaMetrics/Prom (metrics), Elastic (searchable audit mirrors).
* **Objects:** MinIO with tenant buckets; WORM policy on immutable audit snapshots.
* **Ledger:** Hyperledger Fabric (permissioned), channel per tenant; hash pointers referenced in app AAD.

---

## 8) Security controls & governance (operators’ run-book)

* **ZTA:** All requests adjudicated by CCP; least-privilege scopes; tokens short-lived; device posture checks.
* **Secrets:** Vault; no secrets in images; envelope encryption via KMS; just-in-time decryption bounded by DBGID.
* **Admission:** Gatekeeper policies (only signed images; drop CAP_*; read-only FS).
* **Runtime:** Falco rules for process/file/syscall anomalies; auto-quarantine using CCP LOCK.
* **Privacy:** SET™ tags with retention class; POPIA/GDPR erasure executes token revocation and field shredding; audit proves action.

---

## 9) Observability, AIOps, and APE-CIR loop

* **OpenTelemetry** traces for every user/API/capsule action; **redaction at source** for PII.
* **APE-CIR signals:** entropy_delta, geo_velocity, behaviour_vector → unified risk in [0..100]; thresholds drive ALLOW/ESCALATE/LOCK.
* **Explainability:** Each capsule emits decision trace; console shows “why” chain for every lockdown or re-key.

---

## 10) SLOs & performance budgets (hard targets)

* Control-plane decision (CCP): **p50 ≤ 1.5 ms**, p99 ≤ 10 ms.
* Capsule inference (AIC): **≤ 3 ms** typical per capsule, ≤ 10 ms chained.
* Per-field AEAD (SET): **≤ 2 ms** @ 1 KB field on router-class CPU.
* PQC KEM (ML-KEM/NTRU) handshake: **≤ 6 ms** budgeted on edge CPU.
* SD-WAN failover: **≤ 300 ms** detection, **≤ 1 s** re-route.
* OTA package flash (A/B): **≤ 120 s**; always recoverable.

---

## 11) Test strategy (ship-stopper gates)

**Crypto & protocol**

* KATs for AES-GCM/HKDF/DRBG; HOAC switch tests; PQC interop (ML-KEM/NTRU).
* IV uniqueness fuzz; misuse-resistant AEAD tests; TLS1.3 hybrid suites canary.

**ZTA & CCP**

* Red-team bypasses (geo-spoof, biometric drift, device swap).
* Replay/forgery harness: same bio, different geo/time ⇒ must fail.

**Firmware**

* Secure boot break attempts; anti-rollback; SBOM attestation; fault-injection (brownout, flash errors).

**SD-WAN**

* Link impairment lab (loss/jitter/cost churn); policy conformance; app steer correctness.

**Privacy & audit**

* Erasure requests verify cryptographic shredding; ledger/audit bundle verification.

**Chaos & resilience**

* Capsule crash/latency injection; CCP degraded mode; offline operation + resync conflict resolution.

---

## 12) Compliance track (evidence-first)

* **FIPS 140-3**: define crypto module boundary, roles/services, self-tests; lab pre-assessment artefacts in repo.
* **ISO/IEC 27001 + 27701**: Annex-A control matrix mapped to components; SoA auto-export.
* **PCI DSS 4.x (if in scope):** scope reduction via SET; section-10 logging satisfied by LedgerProof + syslog trails.
* **POPIA/GDPR:** data maps, ROPA, DSR run-books; redaction proofs.

---

## 13) Developer scaffolding (repos & starters)

```
/platform
  /console           # NextJS (RBAC, OTEL, dark mode, i18n)
  /ccp               # Go/Rust PDP, gRPC, OPA policies
  /crypto            # Go libs: dbgID, set, hoac, lso; C FFI for firmware
  /aic               # Capsule SDK (Py/Go), SessionOracle, EntropyGuard, KeySurgeon
  /device-agents     # gnmid, telemetryd, ota-agent (Rust/Go)
  /sdwan             # path engine, dpi, qos, fec, ipsec/wg driver
  /audit             # LedgerProof writer, export packs
  /infra             # Helm charts, Gatekeeper, Falco rules, Calico policies
```

**Capsule SDK (Python)**

```python
class Capsule:
    def features(self, session): ...
    def infer(self, feats)->dict: ...
    def obligations(self, result)->dict: ...
# Example: EntropyGuard
class EntropyGuard(Capsule):
    TH=0.15
    def infer(self, f): 
        delta=abs(f['entropy_now']-f['entropy_base'])
        return {"risk": min(100, int(100*delta/self.TH))}
```

**Makefile targets**

```
make dev          # run CCP, AIC, console with kind cluster
make test         # all unit + fuzz
make e2e          # spin SD-WAN sim, chaos tests
make sbom         # generate attestations
make release      # signed images, provenance
```

---

## 14) Rollout plan (90-day)

* **Weeks 1–3:** Repos/CI; CCP MVP; DBGID/SET libs; gNMI skeleton; Helm baseline.
* **Weeks 4–6:** AIC capsules (SessionOracle, EntropyGuard); HOAC switch; SD-WAN path engine; console MVP.
* **Weeks 7–9:** Firmware OTA A/B; secure boot chain; device agents; PQC canary channel.
* **Weeks 10–12:** Full test harness; chaos runs; regulator audit bundle; pilot at 2 branches + 1 DC.

**Exit criteria:** All SLOs met; PQC failover verified; audit bundle passes external read-only review; offline mode proven.

---

## 15) Threat simulations (what we break before adversaries try)

* **Impossible travel:** same user at two geos inside 15 min → APE-CIR spikes; CCP LOCK; KeySurgeon rekeys on next session.
* **Classifier interdiction:** traffic marked "confidential" through hostile ISP → HOAC→PQC enforced; LSO camouflage on control artefacts.
* **Insider drift:** keystroke and decision speed anomaly for reviewer → capsule flags; CCP escalates to quorum approval.

-
