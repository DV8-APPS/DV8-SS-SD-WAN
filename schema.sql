create table device (
  id uuid primary key,
  tenant text not null,
  kind text check (kind in ('router','switch','ap','virtual','gateway')),
  vendor text, model text, sw_version text,
  fingerprint text unique, trust_state jsonb,
  site text, posture text check (posture in ('ECC','PQC')),
  created_at timestamptz default now()
);

create table interface (
  id uuid primary key, device_id uuid references device(id) on delete cascade,
  name text, mac macaddr, ipv4 inet, ipv6 inet, speed_mbps int, up bool
);

create table neighbor (
  a_if uuid references interface(id) on delete cascade,
  z_if uuid references interface(id) on delete cascade,
  proto text check (proto in ('lldp','cdp','bgp','passive')),
  confidence real, primary key (a_if, z_if, proto)
);

create table discovery_job (
  id uuid primary key, tenant text, passive_only bool, cidrs cidr[],
  started_at timestamptz default now(), by_user text, dbg_session text
);

create table discovery_result (
  job_id uuid references discovery_job(id) on delete cascade,
  fingerprint text, addrs inet[], signals text[], confidence real,
  raw jsonb, primary key (job_id, fingerprint)
);

-- Secrets/PII are stored via SET™; only encrypted blobs here:
create table secret_field (
  id uuid primary key, device_id uuid references device(id),
  field_name text, ciphertext bytea, iv bytea, aad bytea, ledger_ptr text
);

create table dbg_session (
  id uuid primary key, user_id text, dbg_hash bytea, geo jsonb, risk_floor int,
  created_at timestamptz default now(), expires_at timestamptz
);

create table ccp_decision (
  id uuid primary key, actor text, action text, context jsonb,
  decision text check (decision in ('ALLOW','ESCALATE','LOCK','DENY')),
  reason text, ledger_hash text, created_at timestamptz default now()
);

create table hoac_event (
  id uuid primary key, channel text, profile text check (profile in ('ECC','PQC')),
  risk int, device_id uuid, transcript bytea, created_at timestamptz default now()
);
