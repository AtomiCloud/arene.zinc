# LPSM & Service Tree

Understanding and working with AtomiCloud's LPSM (Landscape-Platform-Service-Module) service tree for resource organization, naming, and navigation.

## What This Skill Covers

- Understanding the LPSM/LCPSM hierarchy
- Naming conventions and thematic sets (Pokemon, Gemstones, Functional Groups, Elements)
- Mapping LPSM to Kubernetes resources (namespaces, workloads, URLs)
- Landscape selection and purpose
- Resource naming patterns
- Integration with configuration system

## When to Use This Skill

Use this skill when you need to:

- Understand resource naming in the codebase
- Determine which landscape to use for a task
- Navigate the service tree hierarchy
- Name new resources following conventions
- Understand Kubernetes resource organization
- Construct URLs or DNS names
- Work with multi-cluster deployments
- Debug configuration or deployment issues

## Key Concepts

### LPSM Service Tree

All AtomiCloud resources follow the **LPSM** (domain) or **LCPSM** (tech-specific) hierarchy:

```
Landscape (L) → [Cluster (C)] → Platform (P) → Service (S) → Module (M)
```

**Metaphor**: Each Pokemon (landscape) wears gemstones (cluster) consisting of functional groups (platform) which are made of elements (service) which are made of sub-atomic particles (module).

**Format**:

- **LPSM**: Local/testing environments (cluster omitted)
- **LCPSM**: Deployed environments (cluster included for blue-green rotation)

### Current Project Mapping

```
Platform: arene (Marketing Engine)
Service: zinc (Core API)
Modules: api, migration, maindb, maincache, mainstorage, etc.

Full Examples:
- Local dev:        lapras-arene-zinc-api
- Testing:          tauros-arene-zinc-api
- Production:       raichu-opal-arene-zinc-api
```

---

## Landscape (L) - Pokemon Set

**Purpose**: Represents a deployment environment encoding:

- Purpose (Development, Production, Testing)
- Region (Singapore, US, Malaysia, Local)
- Segment (Beta, Internal, Public)

### Naming Theme

Pokemon set with loose evolution pattern:

- **Legendary**: Meta/Administration
- **First Evolution**: Development
- **Second Evolution**: Staging/UAT
- **Final Evolution**: Production
- **Non-Evolution**: Local/Testing

### Available Landscapes

| Pokemon     | Purpose                 | Region    | Cluster Format | Notes                               |
| ----------- | ----------------------- | --------- | -------------- | ----------------------------------- |
| **lapras**  | Development             | Local     | LPSM           | Default local dev, uses k3d         |
| **tauros**  | Testing                 | Local     | LPSM           | Integration tests, in-memory DB     |
| **absol**   | Development (Prod-like) | Local     | LPSM           | Local production testing            |
| **pinsir**  | Continuous Integration  | CI        | LPSM           | CI/CD pipelines                     |
| **pichu**   | Development             | Singapore | LCPSM          | Deployed dev environment            |
| **pikachu** | Staging/UAT             | Singapore | LCPSM          | Pre-production testing              |
| **raichu**  | Production              | Singapore | LCPSM          | Live production                     |
| **suicune** | Administration          | Singapore | LCPSM          | Admin/management tools              |
| **entei**   | Physical Cluster        | Singapore | LCPSM          | Bare metal cluster                  |
| **arceus**  | Meta                    | -         | Special        | Landscape-agnostic singletons       |
| **corsola** | Dumpster                | -         | Special        | Non-Kubernetes one-off dependencies |

### Landscape Selection Guide

**For Development**:

- `lapras`: Standard local development with k3d cluster
- `corsola`: Non-Kubernetes testing, quick iterations without cluster overhead
- `absol`: Local testing with production-like configuration

**For Testing**:

- `tauros`: Integration tests (in-memory database)
- `pinsir`: CI/CD automated testing
- `pikachu`: Staging/UAT testing

**For Production**:

- `raichu`: Live production environment

**Special Cases**:

- `arceus`: Singleton applications without landscape concept (e.g., cluster-wide operators)
- `corsola`: Dependencies/tools not conforming to Kubernetes patterns

---

## Cluster (C) - Gemstone Set

**Purpose**: Represents a Kubernetes cluster within a landscape. Only unique within a single landscape.

**Why**: Enables blue-green deployments, cluster migrations, and fallback strategies during transitions.

### Naming Theme: Gemstone Set by Cloud Provider

| Cluster Set | Cloud Provider |
| ----------- | -------------- |
| opal-ruby   | Digital Ocean  |
| onyx-jade   | Linode         |
| mica-talc   | Vultr          |
| topaz-amber | AWS            |
| agate-lapis | GCP            |
| beryl-coral | Azure          |

### When Cluster is Omitted (LPSM)

Local landscapes use **LPSM** (no cluster):

- `lapras`, `tauros`, `absol`, `pinsir`, `corsola`
- Single cluster, no rotation needed
- Simpler naming

### When Cluster is Included (LCPSM)

Deployed landscapes use **LCPSM**:

- `pichu-opal-*`, `pikachu-opal-*`, `raichu-opal-*`
- Supports multi-cluster deployments
- Enables cluster rotation/migration

**Example Scenario**: Migrating from `raichu-opal-arene-zinc-api` to `raichu-ruby-arene-zinc-api` during Digital Ocean cluster upgrade.

---

## Platform (P) - Functional Group Set

**Purpose**: Represents a single product/domain. Maps to a **Kubernetes namespace**.

**Business Definition**: Same target segment, single app, or single domain.

### Naming Theme: Functional Groups (Organic Chemistry)

| Platform       | Description                                            | Status      | DRI              |
| -------------- | ------------------------------------------------------ | ----------- | ---------------- |
| **carboxylic** | Packages & Libraries                                   | NIL         | @Ernest Ng       |
| **ketone**     | Templates for users                                    | NIL         | @Ernest Ng       |
| **aldehyde**   | Per-system services                                    | NIL         | @Ernest Ng       |
| **sulfoxide**  | System services (ingress, policy, secrets, VPN, certs) | NIL         | @Ernest Ng       |
| **sulfone**    | CyanPrint (scaffolding engine)                         | Launched    | @Ernest Ng       |
| **nitroso**    | BunnyBooker (KTMB ticket booking)                      | Launched    | @Ernest Ng       |
| **alcohol**    | LazyTax (productivity app)                             | Launched    | @Yekkhan         |
| **ether**      | Axnote.io (AI flashcards)                              | Development | @Matthew Tan     |
| **arene**      | Marketing Engine                                       | Development | @Jay, @Ernest Ng |
| **azide**      | Main Atomi Blog                                        | Development | -                |

### Current Project

```
Platform: arene
Kubernetes Namespace: arene
All Arene services share this namespace
```

**Future Services** (hypothetical):

- `arene/zinc-api` - Core API (current)
- `arene/helium-scraper` - Data scraper
- `arene/boron-webapp` - Client-facing web app
- `arene/oxygen-backoffice` - Admin portal

---

## Service (S) - Element Set (Periodic Table)

**Purpose**: An API or application needed to build the platform. Usually viewed as a single git repository and Helm chart.

### Naming Theme: Periodic Table Elements

**Common Services**:
| Element | Service Type | Example |
|---------|--------------|---------|
| **zinc** | Core API | Marketing API |
| **helium** | Scrapers | Data collection workers |
| **boron** | Client-facing web app | User portal |
| **oxygen** | Backoffice web app | Admin dashboard |
| **neon** | Mobile app | iOS/Android app |

**Sulfoxide (System Platform) Services** - Strict Mapping:

- System services have predefined element mappings
- Refer to Sulfoxide Deployment documentation for details

### Current Project

```
Service: zinc
Repository: platforms/arene/zinc
Helm Chart: infra/root_chart/
Type: Core API
```

---

## Module (M) - Free Form

**Purpose**: Actual components needed to build the service (databases, APIs, caches, sub-components).

**No Set**: Free-form naming based on component purpose.

### Current Project Modules

| Module          | Description           | Type           |
| --------------- | --------------------- | -------------- |
| **api**         | ASP.NET Core API      | Application    |
| **migration**   | EF Core migration job | Job            |
| **maindb**      | PostgreSQL database   | Infrastructure |
| **maincache**   | Dragonfly/Redis cache | Infrastructure |
| **mainstorage** | MinIO object storage  | Infrastructure |

**Workload Naming**: `{service}-{module}` (e.g., `zinc-api`, `zinc-maindb`)

---

## Resource Naming Patterns

### Full Resource Names

**Format**: `{landscape}-[{cluster}-]{platform}-{service}-{module}`

**Examples**:

```
Local Development (LPSM):
- lapras-arene-zinc-api
- lapras-arene-zinc-maindb
- tauros-arene-zinc-api

Deployed Production (LCPSM):
- raichu-opal-arene-zinc-api
- raichu-opal-arene-zinc-maindb
- raichu-opal-arene-zinc-migration

Deployed Staging (LCPSM):
- pikachu-opal-arene-zinc-api
- pikachu-opal-arene-zinc-maincache
```

### Kubernetes Resource Names

**Namespace**: `{platform}` (e.g., `arene`)

**Workload**: `{service}-{module}` (e.g., `zinc-api`)

**Generated Resources**: `{service}-{module}-{suffix}` (e.g., `zinc-api-7d9f8b-abcde`)

**Full Qualified**:

```bash
# Pod example
kubectl get pod zinc-api-7d9f8b-abcde -n arene --context k3d-lapras

# Service example
kubectl get svc zinc-maindb -n arene --context k3d-lapras
```

### DNS/URL Patterns

**Standard URL Format** (reverse LCPSM):

```
{module}.{service}.{platform}.{cluster}.{landscape}.atomi.cloud
```

**Examples**:

```
Production API:
https://api.zinc.arene.opal.raichu.atomi.cloud

Staging API:
https://api.zinc.arene.opal.pikachu.atomi.cloud

Local Development (lvh.me):
http://api.zinc.arene.lapras.lvh.me:20010
```

**Swagger URLs**:

```
Local: http://api.zinc.arene.lapras.lvh.me:20010/swagger
Prod:  https://api.zinc.arene.opal.raichu.atomi.cloud/swagger
```

**Storage Console URLs**:

```
Local: http://console-mainstorage.zinc.arene.lapras.lvh.me:20010
Prod:  https://console-mainstorage.zinc.arene.opal.raichu.atomi.cloud
```

---

## Integration with Configuration System

### Landscape-Based Configuration

**Config File Cascade**:

```
App/Config/settings.yaml                    # Base
App/Config/settings.{landscape}.yaml        # Landscape override
Environment Variables (Atomi_*)             # Runtime override
Infisical Secrets                           # Secret values
```

**Examples**:

```
App/Config/settings.yaml           # All landscapes
App/Config/settings.lapras.yaml    # Local dev overrides
App/Config/settings.tauros.yaml    # Testing overrides
App/Config/settings.raichu.yaml    # Production overrides
```

### Helm Value Overrides

**Helm Chart Structure**:

```
infra/root_chart/values.yaml                # Base values
infra/root_chart/values.{landscape}.yaml    # Landscape-specific values
```

**Platform/Service/Module in Helm**:

```yaml
# values.yaml
platform: arene
service: zinc
# Generates resources:
# - Namespace: arene
# - Deployment: zinc-api
# - Service: zinc-maindb
# - ConfigMap: zinc-config
```

### k3d Cluster Configuration

**Cluster Config by Landscape**:

```
infra/k3d.{landscape}.yaml

Examples:
- infra/k3d.lapras.yaml    # Local dev cluster
- infra/k3d.tauros.yaml    # Testing cluster
```

**Cluster Naming**:

```yaml
metadata:
  name: lapras # Cluster name = landscape (local)

# kubectl context: k3d-lapras
```

### Environment Variable

**Required**: `LANDSCAPE` environment variable

```bash
export LANDSCAPE=lapras    # Local development
export LANDSCAPE=tauros    # Integration testing
export LANDSCAPE=raichu    # Production
```

**Used by**:

- Application configuration loading
- Infisical secret fetching
- Helm value selection
- k3d cluster targeting

---

## Working with LPSM

### Determining Current Landscape

```bash
# Check environment variable
echo $LANDSCAPE

# Check kubectl context (local)
kubectl config current-context
# Output: k3d-lapras

# Check namespace
kubectl get ns
# Should see: arene

# Check current workloads
kubectl get pods -n arene
# Should see: zinc-api-*, zinc-maindb-*, etc.
```

### Switching Landscapes

```bash
# Local development
export LANDSCAPE=lapras
pls dev

# Integration testing
export LANDSCAPE=tauros
pls int

# Production (deployed, would use different cluster)
export LANDSCAPE=raichu
kubectl config use-context raichu-opal-cluster
```

### Querying Resources by LPSM

```bash
# All resources for platform (arene)
kubectl get all -n arene

# Specific service workloads (zinc)
kubectl get all -n arene -l service=zinc

# Specific module (api)
kubectl get all -n arene -l service=zinc,module=api

# Full LPSM query (if labeled)
kubectl get all -n arene -l \
  landscape=lapras,platform=arene,service=zinc,module=api
```

### Resource Labels (Recommended)

```yaml
# Kubernetes resource metadata
metadata:
  name: zinc-api
  namespace: arene
  labels:
    landscape: lapras # Or omit for deployed (use cluster label)
    cluster: opal # Only for deployed (LCPSM)
    platform: arene
    service: zinc
    module: api
```

---

## Common Patterns

### Adding a New Module

1. **Choose module name** (free-form): `worker`, `scheduler`, `webhook`
2. **Update Helm chart**: Add module to `values.yaml`
3. **Create workload**: Name it `{service}-{module}` (e.g., `zinc-worker`)
4. **Add configuration**: Update `settings.{landscape}.yaml` if needed
5. **Deploy**: Tilt will auto-deploy, or use `helm upgrade`

### Creating a New Service

1. **Choose element name**: `helium` (scraper), `boron` (webapp), etc.
2. **Create repository**: `platforms/{platform}/{service}`
3. **Create Helm chart**: `infra/{service}_chart/`
4. **Add to umbrella chart**: Update `root_chart/Chart.yaml` dependencies
5. **Configure LPSM**: Set labels, namespaces, workload names
6. **Deploy**: Add to Tiltfile or deploy via Helm

### Multi-Cluster Deployment

**Scenario**: Deploy to both `opal` and `ruby` clusters in `pikachu` landscape

```bash
# Deploy to opal cluster
export LANDSCAPE=pikachu
kubectl config use-context pikachu-opal-cluster
helm upgrade --install zinc ./infra/root_chart \
  -f values.yaml \
  -f values.pikachu.yaml \
  --set cluster=opal

# Deploy to ruby cluster (fallback)
kubectl config use-context pikachu-ruby-cluster
helm upgrade --install zinc ./infra/root_chart \
  -f values.yaml \
  -f values.pikachu.yaml \
  --set cluster=ruby

# Both deployments coexist:
# - pikachu-opal-arene-zinc-api
# - pikachu-ruby-arene-zinc-api
```

### Understanding a Resource Name

**Given**: `raichu-opal-arene-zinc-maindb`

**Parse**:

- **Landscape**: `raichu` → Production, Singapore
- **Cluster**: `opal` → Digital Ocean cluster
- **Platform**: `arene` → Marketing Engine
- **Service**: `zinc` → Core API
- **Module**: `maindb` → PostgreSQL database

**Location**:

- **Kubernetes Namespace**: `arene`
- **Workload Name**: `zinc-maindb`
- **Context**: `raichu-opal-cluster`

**URL** (if exposed):

```
maindb.zinc.arene.opal.raichu.atomi.cloud
```

---

## Special Landscapes

### Corsola - Non-Kubernetes

**Purpose**: One-off dependencies and applications that don't conform to Kubernetes/landscape patterns.

**Use Cases**:

- Quick scripts without containerization
- Legacy dependencies
- Local-only tools

**Example**:

```bash
export LANDSCAPE=corsola
pls run  # Runs .NET directly without k3d cluster
```

**No k3d cluster, no Helm, no Kubernetes resources**.

### Arceus - Meta/Singleton

**Purpose**: Landscape-agnostic singleton applications.

**Use Cases**:

- Cluster-wide operators (External Secrets Operator, Traefik)
- Global configurations
- Cross-landscape services

**Characteristics**:

- Deployed once per cluster
- Not tied to specific landscape
- Often system-level services

---

## Troubleshooting

### "Which landscape am I in?"

```bash
echo $LANDSCAPE
kubectl config current-context
```

### "Resource not found in namespace"

**Check namespace**:

```bash
kubectl get ns
# Should see: arene
```

**Check resource exists**:

```bash
kubectl get all -n arene
```

**Verify LPSM naming**:

- Namespace = platform (`arene`)
- Workload = `{service}-{module}` (`zinc-api`)

### "Wrong cluster"

**Local landscapes**:

```bash
kubectl config use-context k3d-lapras
# or
kubectl config use-context k3d-tauros
```

**Deployed landscapes**:

```bash
kubectl config use-context {landscape}-{cluster}-cluster
# Example: pikachu-opal-cluster
```

### "Configuration not loading"

**Check landscape variable**:

```bash
echo $LANDSCAPE
# Should match config file: settings.{landscape}.yaml
```

**Verify config file exists**:

```bash
ls App/Config/settings.$LANDSCAPE.yaml
```

---

## Best Practices

1. **Always set LANDSCAPE**: Required for configuration and secrets
2. **Use LPSM for local**: Simpler naming, no cluster needed
3. **Use LCPSM for deployed**: Enables cluster rotation
4. **Follow naming sets**: Pokemon, gemstones, elements, etc.
5. **Namespace = Platform**: All services in platform share namespace
6. **Label resources**: Include LPSM labels for queryability
7. **Reverse DNS for URLs**: `{module}.{service}.{platform}.{cluster}.{landscape}`
8. **Document new platforms/services**: Update service tree documentation

---

## Quick Reference

| Layer         | Current Value          | Set/Theme         | Kubernetes Mapping   |
| ------------- | ---------------------- | ----------------- | -------------------- |
| **Landscape** | lapras, tauros, raichu | Pokemon           | Context              |
| **Cluster**   | opal, ruby             | Gemstones         | Context (LCPSM only) |
| **Platform**  | arene                  | Functional Groups | Namespace            |
| **Service**   | zinc                   | Elements          | Label                |
| **Module**    | api, maindb, maincache | Free-form         | Workload Name        |

**Full Example**:

```
raichu-opal-arene-zinc-api
  │     │    │     │    └─ Module (workload)
  │     │    │     └────── Service (label)
  │     │    └──────────── Platform (namespace)
  │     └───────────────── Cluster (context)
  └─────────────────────── Landscape (context)
```

---

## Additional Resources

- **Service Tree Documentation**: AtomiCloud internal docs
- **Sulfoxide Deployment**: System service element mappings
- **Cluster Rotation Guide**: Blue-green deployment strategies
- **Functional Groups Reference**: Complete platform list

---

This skill helps you navigate AtomiCloud's service tree, understand resource naming, and work effectively across multiple landscapes and clusters.
