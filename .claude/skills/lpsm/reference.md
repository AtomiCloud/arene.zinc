# LPSM Quick Reference

Fast lookup tables and cheat sheets for AtomiCloud's LPSM service tree.

---

## LPSM Format Quick Check

```
Local/Testing:  L - P - S - M       (LPSM)
                │   │   │   └─ Module (free-form)
                │   │   └───── Service (element)
                │   └───────── Platform (functional group)
                └───────────── Landscape (pokemon)

Deployed:       L - C - P - S - M   (LCPSM)
                │   │   │   │   └─ Module (free-form)
                │   │   │   └───── Service (element)
                │   │   └───────── Platform (functional group)
                │   └───────────── Cluster (gemstone)
                └───────────────── Landscape (pokemon)
```

---

## Landscape (L) - Pokemon Set

| Pokemon     | Format  | Purpose                 | Region    | Use Case                            |
| ----------- | ------- | ----------------------- | --------- | ----------------------------------- |
| **lapras**  | LPSM    | Development             | Local     | Default local dev with k3d          |
| **tauros**  | LPSM    | Testing                 | Local     | Integration tests, in-memory DB     |
| **absol**   | LPSM    | Development (Prod-like) | Local     | Local production testing            |
| **pinsir**  | LPSM    | CI/CD                   | CI        | Automated testing pipelines         |
| **corsola** | Special | Dumpster                | -         | Non-Kubernetes one-off dependencies |
| **pichu**   | LCPSM   | Development             | Singapore | Deployed dev environment            |
| **pikachu** | LCPSM   | Staging/UAT             | Singapore | Pre-production testing              |
| **raichu**  | LCPSM   | Production              | Singapore | Live production                     |
| **suicune** | LCPSM   | Administration          | Singapore | Admin/management tools              |
| **entei**   | LCPSM   | Physical Cluster        | Singapore | Bare metal cluster                  |
| **arceus**  | Special | Meta                    | -         | Landscape-agnostic singletons       |

### Landscape Selection Cheat Sheet

```
Need to...                          Use...
─────────────────────────────────────────────────────
Develop locally with k3d            lapras
Run integration tests               tauros
Test prod-like config locally       absol
Run CI/CD tests                     pinsir
Test without Kubernetes             corsola
Deploy to dev environment           pichu
Deploy to staging                   pikachu
Deploy to production                raichu
Install cluster-wide operator       arceus
```

---

## Cluster (C) - Gemstone Set

| Cluster Set     | Cloud Provider | Example Use                |
| --------------- | -------------- | -------------------------- |
| **opal-ruby**   | Digital Ocean  | Primary/Secondary rotation |
| **onyx-jade**   | Linode         | Primary/Secondary rotation |
| **mica-talc**   | Vultr          | Primary/Secondary rotation |
| **topaz-amber** | AWS            | Primary/Secondary rotation |
| **agate-lapis** | GCP            | Primary/Secondary rotation |
| **beryl-coral** | Azure          | Primary/Secondary rotation |

### When Cluster is Used

```
LPSM (no cluster):
- lapras, tauros, absol, pinsir, corsola
- Single cluster, no rotation needed

LCPSM (with cluster):
- pichu, pikachu, raichu, suicune, entei
- Multi-cluster capable
- Enables blue-green deployment
```

---

## Platform (P) - Functional Group Set

| Platform       | Description           | Type           | Services                      |
| -------------- | --------------------- | -------------- | ----------------------------- |
| **carboxylic** | Packages & Libraries  | Internal       | -                             |
| **ketone**     | Templates for users   | Internal       | -                             |
| **aldehyde**   | Per-system services   | Internal       | -                             |
| **sulfoxide**  | System services       | Infrastructure | ingress, secrets, vpn, certs  |
| **sulfone**    | CyanPrint scaffolding | Product        | -                             |
| **nitroso**    | BunnyBooker           | Product        | -                             |
| **alcohol**    | LazyTax               | Product        | -                             |
| **ether**      | Axnote.io             | Product        | -                             |
| **arene**      | Marketing Engine      | Product        | zinc, (future: helium, boron) |
| **azide**      | Atomi Blog            | Product        | -                             |

### Platform = Kubernetes Namespace

```
Platform: arene
Namespace: arene

kubectl get all -n arene
```

---

## Service (S) - Element Set

### Common Services (Periodic Table)

| Element    | Service Type         | Typical Use             |
| ---------- | -------------------- | ----------------------- |
| **zinc**   | Core API             | Primary backend API     |
| **helium** | Scraper              | Data collection workers |
| **boron**  | Web App (Client)     | User-facing portal      |
| **oxygen** | Web App (Backoffice) | Admin dashboard         |
| **neon**   | Mobile App           | iOS/Android application |

### Sulfoxide System Services

```
(Refer to Sulfoxide Deployment documentation for strict element mappings)

Example system services:
- traefik (ingress controller)
- external-secrets (secrets operator)
- cert-manager (certificate management)
```

---

## Module (M) - Free Form

### Common Module Names

| Module          | Description            | Type           |
| --------------- | ---------------------- | -------------- |
| **api**         | API application        | Application    |
| **worker**      | Background worker      | Application    |
| **scheduler**   | Cron jobs              | Application    |
| **webhook**     | Webhook handler        | Application    |
| **migration**   | Database migration job | Job            |
| **maindb**      | Primary database       | Infrastructure |
| **maincache**   | Primary cache          | Infrastructure |
| **mainstorage** | Object storage         | Infrastructure |
| **queue**       | Message queue          | Infrastructure |

---

## Resource Naming Patterns

### Full Resource Names

```
Pattern: {landscape}-[{cluster}-]{platform}-{service}-{module}

Local Examples:
lapras-arene-zinc-api
lapras-arene-zinc-maindb
tauros-arene-zinc-maincache

Deployed Examples:
raichu-opal-arene-zinc-api
pikachu-opal-arene-zinc-maindb
pichu-ruby-arene-helium-scraper
```

### Kubernetes Resource Names

```
Namespace:  {platform}
            Example: arene

Workload:   {service}-{module}
            Example: zinc-api

Generated:  {service}-{module}-{suffix}
            Example: zinc-api-7d9f8b-abcde

Full Path:  {workload}.{namespace}.svc.cluster.local
            Example: zinc-maindb.arene.svc.cluster.local
```

### DNS/URL Patterns

```
Standard URL (reverse LCPSM):
{module}.{service}.{platform}.{cluster}.{landscape}.atomi.cloud

Production:
https://api.zinc.arene.opal.raichu.atomi.cloud

Staging:
https://api.zinc.arene.opal.pikachu.atomi.cloud

Local (lvh.me):
http://api.zinc.arene.lapras.lvh.me:20010
```

---

## Configuration File Mapping

### Application Configuration

```
App/Config/
├── settings.yaml                   # Base (all landscapes)
├── settings.lapras.yaml            # Local dev
├── settings.tauros.yaml            # Integration testing
├── settings.absol.yaml             # Local prod-like
├── settings.corsola.yaml           # Non-k8s
├── settings.pichu.yaml             # Deployed dev
├── settings.pikachu.yaml           # Staging
└── settings.raichu.yaml            # Production

Loading order:
1. settings.yaml (base)
2. settings.{LANDSCAPE}.yaml (override)
3. Environment variables (Atomi_*)
4. Infisical secrets (highest priority)
```

### Helm Chart Configuration

```
infra/root_chart/
├── values.yaml                     # Base values
├── values.lapras.yaml              # Local dev
├── values.tauros.yaml              # Integration testing
├── values.pichu.yaml               # Deployed dev
├── values.pikachu.yaml             # Staging
└── values.raichu.yaml              # Production

Deployment:
helm upgrade --install arene-zinc ./infra/root_chart \
  -f values.yaml \
  -f values.{landscape}.yaml
```

### k3d Cluster Configuration

```
infra/
├── k3d.lapras.yaml                 # Local dev cluster
├── k3d.tauros.yaml                 # Testing cluster
└── k3d.absol.yaml                  # Local prod-like cluster

Creation:
k3d cluster create --config infra/k3d.{landscape}.yaml
```

---

## Kubectl Context Patterns

### Local Landscapes (k3d)

```
Landscape → Context
─────────────────────
lapras    → k3d-lapras
tauros    → k3d-tauros
absol     → k3d-absol

Switch context:
kubectl config use-context k3d-lapras
```

### Deployed Landscapes

```
Landscape + Cluster → Context
────────────────────────────────────
pichu + opal        → pichu-opal-cluster
pikachu + opal      → pikachu-opal-cluster
raichu + opal       → raichu-opal-cluster
raichu + ruby       → raichu-ruby-cluster

Switch context:
kubectl config use-context pikachu-opal-cluster
```

---

## Common Commands

### Working with LPSM Resources

```bash
# Set landscape
export LANDSCAPE=lapras

# Switch kubectl context
kubectl config use-context k3d-$LANDSCAPE

# List all resources in platform
kubectl get all -n arene

# Get specific service workloads
kubectl get all -n arene -l service=zinc

# Get specific module
kubectl get all -n arene -l service=zinc,module=api

# Query by LPSM labels
kubectl get all -n arene -l \
  landscape=lapras,platform=arene,service=zinc,module=api
```

### Configuration

```bash
# Check current landscape
echo $LANDSCAPE

# View config for landscape
cat App/Config/settings.$LANDSCAPE.yaml

# List Infisical secrets for landscape
infisical secrets --env=$LANDSCAPE

# Run command with landscape secrets
infisical run --env=$LANDSCAPE -- {command}
```

### Development

```bash
# Start local development
export LANDSCAPE=lapras
pls dev

# Run integration tests
export LANDSCAPE=tauros
pls int

# Run without Kubernetes
export LANDSCAPE=corsola
pls run
```

---

## Troubleshooting Quick Guide

### "Which landscape am I in?"

```bash
echo $LANDSCAPE
kubectl config current-context
```

### "Resource not found"

```bash
# Check namespace exists
kubectl get ns arene

# Check workloads exist
kubectl get all -n arene

# Verify resource naming
# Namespace = platform (arene)
# Workload = service-module (zinc-api)
```

### "Wrong configuration loaded"

```bash
# Verify landscape set
echo $LANDSCAPE

# Check config file exists
ls App/Config/settings.$LANDSCAPE.yaml

# Check Infisical environment
infisical secrets --env=$LANDSCAPE
```

### "Can't connect to service"

```bash
# Local URLs (lvh.me)
http://api.zinc.arene.lapras.lvh.me:20010

# Check ingress
kubectl get ingress -n arene

# Port-forward if needed
kubectl port-forward -n arene svc/zinc-api 8080:8080
```

---

## Resource Label Schema

### Recommended Labels

```yaml
metadata:
  labels:
    landscape: lapras # Or omit for deployed
    cluster: opal # Only for LCPSM
    platform: arene
    service: zinc
    module: api
    app.kubernetes.io/name: zinc
    app.kubernetes.io/component: api
    app.kubernetes.io/part-of: arene
```

### Querying by Labels

```bash
# By platform
kubectl get all -n arene -l platform=arene

# By service
kubectl get all -n arene -l service=zinc

# By module
kubectl get all -n arene -l module=api

# Combined
kubectl get all -n arene -l service=zinc,module=api
```

---

## URL Construction

### Pattern

```
{module}.{service}.{platform}.{cluster}.{landscape}.atomi.cloud
```

### Examples

```
Production API:
https://api.zinc.arene.opal.raichu.atomi.cloud

Production DB Console (if exposed):
https://console-maindb.zinc.arene.opal.raichu.atomi.cloud

Staging API:
https://api.zinc.arene.opal.pikachu.atomi.cloud

Local Development:
http://api.zinc.arene.lapras.lvh.me:20010

Local Storage Console:
http://console-mainstorage.zinc.arene.lapras.lvh.me:20010
```

### Local Port Mappings (k3d)

```
Landscape → HTTP Port → HTTPS Port → Registry Port
────────────────────────────────────────────────────
lapras    → 20010      → 20011       → 20012
tauros    → 20020      → 20021       → 20022
absol     → 20030      → 20031       → 20032

Example:
http://api.zinc.arene.lapras.lvh.me:20010
```

---

## Environment Variables

### Required

```bash
LANDSCAPE=lapras           # Current landscape
```

### Configuration Prefix

```bash
Atomi_Database__MainDb__ConnectionString="..."
Atomi_Auth__Enable=true
Atomi_OpenTelemetry__Enable=false

# Pattern: Atomi_{Section}__{SubSection}__{Key}
```

### Infisical Integration

```bash
# Login (one-time)
infisical login

# Run with secrets
infisical run --env=$LANDSCAPE -- {command}

# List secrets
infisical secrets --env=$LANDSCAPE
```

---

## Current Project Quick Facts

```
Platform:     arene (Marketing Engine)
Service:      zinc (Core API)
Repository:   platforms/arene/zinc
Namespace:    arene

Modules:
  - api           (ASP.NET Core API)
  - migration     (EF Core migrations)
  - maindb        (PostgreSQL)
  - maincache     (Dragonfly/Redis)
  - mainstorage   (MinIO)

Local Development:
  Landscape: lapras
  Context:   k3d-lapras
  URL:       http://api.zinc.arene.lapras.lvh.me:20010

Integration Testing:
  Landscape: tauros
  Database:  In-memory (no k3d)
  Command:   pls int

Production:
  Landscape: raichu
  Cluster:   opal (Digital Ocean)
  Context:   raichu-opal-cluster
  URL:       https://api.zinc.arene.opal.raichu.atomi.cloud
```

---

## Common Patterns Cheat Sheet

### Adding New Module

```bash
1. Choose name: worker
2. Create Helm template: zinc-worker
3. Add to values.yaml: modules.worker.enabled
4. Deploy: pls dev
```

### Adding New Service

```bash
1. Choose element: helium
2. Create repo: platforms/arene/helium
3. Create Helm chart: infra/helium_chart
4. Add to root_chart/Chart.yaml dependencies
5. Deploy: helm upgrade --install
```

### Switching Landscapes

```bash
# Local dev
export LANDSCAPE=lapras && pls dev

# Integration tests
export LANDSCAPE=tauros && pls int

# Production-like local
export LANDSCAPE=absol && pls dev
```

### Multi-Cluster Deployment

```bash
# Deploy to opal
kubectl config use-context pikachu-opal-cluster
helm upgrade --install ... --set cluster=opal

# Deploy to ruby
kubectl config use-context pikachu-ruby-cluster
helm upgrade --install ... --set cluster=ruby
```

---

## Service Tree Hierarchy Visual

```
AtomiCloud Service Tree
│
├─ Landscape (Pokemon)
│  ├─ lapras (local dev)
│  ├─ tauros (testing)
│  ├─ pikachu (staging)
│  └─ raichu (production)
│     │
│     ├─ Cluster (Gemstones) [LCPSM only]
│     │  ├─ opal (Digital Ocean)
│     │  └─ ruby (Digital Ocean)
│     │     │
│     │     ├─ Platform (Functional Groups) = Namespace
│     │     │  ├─ arene (Marketing Engine)
│     │     │  ├─ ether (Axnote.io)
│     │     │  └─ sulfoxide (System Services)
│     │     │     │
│     │     │     ├─ Service (Elements) = Label
│     │     │     │  ├─ zinc (Core API)
│     │     │     │  ├─ helium (Scraper)
│     │     │     │  └─ boron (Web App)
│     │     │     │     │
│     │     │     │     └─ Module (Free-form) = Workload
│     │     │     │        ├─ api
│     │     │     │        ├─ worker
│     │     │     │        ├─ maindb
│     │     │     │        └─ maincache
```

---

This reference provides quick lookup for LPSM service tree navigation, naming conventions, and common operations across all AtomiCloud landscapes.
