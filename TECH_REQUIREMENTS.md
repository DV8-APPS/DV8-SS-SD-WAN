# DV8 SS Flow Management & Dv8-FlowOS – Technical Requirements

## Functional
- Start/stop discovery per site/tenant with CIDR scopes; passive-only mode supported.
- Candidate classification with confidence scoring, vendor/model inference, capability map (gNMI/NETCONF).
- One-click sandbox build from discovered graph; traffic models and failure injection.
- Replace virtual with physical preserving intents, labels, ring; attestation required by default.
- Brownfield adoption: generate "intent from running" diff and safe-apply plan.

## Security
- All discovery jobs require DBGID session and CCP decision; events anchored to LedgerProof.
- Only SNMPv3 authPriv allowed; block v1/v2c.
- Secrets/PII stored via SET™; no plaintext at rest; browser stores nothing.
- HOAC: ECC default; PQC enforced on hostile zones or policy; posture surfaced in UI.

## Performance & Scale
- Controller processes ≥5k candidates/min per region; ingest ≥50k telemetry msgs/s.
- Discovery active probes ≤100pps/site; per-device ≤5pps; auto-backoff on loss/jitter spikes.
- Commit P95 ≤200 ms (10k nodes); drift MTTR ≤5 min; CCP p99 ≤10 ms.

## Reliability
- Discovery jobs idempotent; resume after controller failover.
- A/B OTA with health gates; automatic rollback on capsule risk spike.
- Components health-probed; graceful degradation to passive-only if probes blocked.

## Compliance
- ISO 27001 Annex A control mapping; PCI/POPIA export packs; audit trail verifiable (hash chain).
- FIPS 140-3 module boundary documented for crypto paths (AES-GCM, KEM, RNG).

## Testing
- Unit: fingerprint parser, classifier, rate limiter, SET/DBGID helpers.
- Integration: SNMPv3/NETCONF/gNMI lab devices; ICMP off scenarios; NAT/overlap IP.
- E2E: sandbox→pilot→GA, virtual→physical swap, brownfield adoption.
- Chaos: broker partition, vault seal/unseal, link flaps; ensure no probe storms.
- Security: replay attempts, geo-spoof of discovery operator, SNMPv3 downgrade → must DENY.
