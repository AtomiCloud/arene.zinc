# LPSM Examples

Practical examples of working with the LPSM service tree in AtomiCloud.

---

## Example 1: Understanding Existing Resources

### Scenario: New Developer Onboarding

You see these resources in the codebase and need to understand what they are:

```
lapras-arene-zinc-api
tauros-arene-zinc-maindb
raichu-opal-arene-zinc-mainstorage
```

### Analysis

**Resource 1**: `lapras-arene-zinc-api`

```
Format: LPSM (no cluster)
├─ Landscape: lapras (Local development)
├─ Platform: arene (Marketing Engine namespace)
├─ Service: zinc (Core API service)
└─ Module: api (API application component)

Location:
- Kubernetes Context: k3d-lapras
- Namespace: arene
- Workload: zinc-api
- URL: http://api.zinc.arene.lapras.lvh.me:20010
```

**Resource 2**: `tauros-arene-zinc-maindb`

```
Format: LPSM (no cluster)
├─ Landscape: tauros (Integration testing)
├─ Platform: arene (Marketing Engine namespace)
├─ Service: zinc (Core API service)
└─ Module: maindb (PostgreSQL database)

Location:
- Kubernetes Context: k3d-tauros
- Namespace: arene
- Workload: zinc-maindb
- Connection: zinc-maindb.arene.svc.cluster.local:5432
```

**Resource 3**: `raichu-opal-arene-zinc-mainstorage`

```
Format: LCPSM (with cluster)
├─ Landscape: raichu (Production, Singapore)
├─ Cluster: opal (Digital Ocean cluster)
├─ Platform: arene (Marketing Engine namespace)
├─ Service: zinc (Core API service)
└─ Module: mainstorage (MinIO object storage)

Location:
- Kubernetes Context: raichu-opal-cluster
- Namespace: arene
- Workload: zinc-mainstorage
- URL: https://console-mainstorage.zinc.arene.opal.raichu.atomi.cloud
```

---

## Example 2: Starting Local Development

### Scenario: Working on Zinc API Locally

**Goal**: Start the Zinc API in local development mode with all dependencies.

### Steps

1. **Set the landscape**:

```bash
export LANDSCAPE=lapras
```

2. **Verify k3d cluster exists**:

```bash
# Check if cluster exists
k3d cluster list | grep lapras

# If not, create it
pls dev  # This will create the cluster automatically
```

3. **Verify namespace and resources**:

```bash
# Switch to correct context
kubectl config use-context k3d-lapras

# Check namespace exists
kubectl get ns arene

# Check workloads
kubectl get all -n arene
# Should see:
# - pod/zinc-api-*
# - pod/zinc-maindb-*
# - pod/zinc-maincache-*
# - pod/zinc-mainstorage-*
```

4. **Access services**:

```bash
# API Swagger
open http://api.zinc.arene.lapras.lvh.me:20010/swagger

# MinIO Console
open http://console-mainstorage.zinc.arene.lapras.lvh.me:20010

# Tilt Dashboard
open http://localhost:11001
```

5. **Make code changes**:

- Edit files in `App/`
- `dotnet watch` automatically reloads
- Changes visible in <2 seconds

### Resource Names Generated

```
lapras-arene-zinc-api              # API pod
lapras-arene-zinc-maindb           # PostgreSQL
lapras-arene-zinc-maincache        # Dragonfly/Redis
lapras-arene-zinc-mainstorage      # MinIO
```

---

## Example 3: Running Integration Tests

### Scenario: Testing Zinc API End-to-End

**Goal**: Run integration tests in isolated environment.

### Steps

1. **Set the landscape**:

```bash
export LANDSCAPE=tauros
```

2. **Run tests**:

```bash
pls int
# Equivalent to:
# LANDSCAPE=tauros pls exec -- dotnet test IntTest/
```

3. **What happens**:

```
Configuration loaded:
- App/Config/settings.yaml (base)
- App/Config/settings.tauros.yaml (overrides)
  - Database: InMemory (no real PostgreSQL)
  - Auth: Disabled
  - OpenTelemetry: Disabled

Test Factory:
- Creates WebApplicationFactory
- No k3d cluster used
- All dependencies mocked or in-memory
```

4. **Resource naming** (logical, not actual k8s):

```
tauros-arene-zinc-api              # Test API (in-memory)
tauros-arene-zinc-maindb           # In-memory database
```

### Key Difference from Lapras

| Aspect       | Lapras (Dev)     | Tauros (Test)      |
| ------------ | ---------------- | ------------------ |
| **Cluster**  | k3d-lapras       | None (in-memory)   |
| **Database** | Real PostgreSQL  | EF In-Memory       |
| **Auth**     | Enabled          | Disabled           |
| **Secrets**  | Infisical        | Mock values        |
| **Speed**    | Slower (real DB) | Faster (in-memory) |

---

## Example 4: Adding a New Module

### Scenario: Add Background Worker to Zinc Service

**Goal**: Create a new `worker` module that processes jobs from a queue.

### Implementation

1. **Choose module name** (free-form):

```
Module: worker
Full name: {landscape}-{cluster}-arene-zinc-worker
Example: lapras-arene-zinc-worker
```

2. **Create Helm template**:

```yaml
# infra/api_chart/templates/worker-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: {{ .Values.service }}-worker  # zinc-worker
  namespace: {{ .Values.platform }}    # arene
  labels:
    platform: {{ .Values.platform }}
    service: {{ .Values.service }}
    module: worker
spec:
  replicas: 1
  selector:
    matchLabels:
      service: {{ .Values.service }}
      module: worker
  template:
    metadata:
      labels:
        platform: {{ .Values.platform }}
        service: {{ .Values.service }}
        module: worker
    spec:
      containers:
      - name: worker
        image: {{ .Values.image }}
        command: ["dotnet", "run", "--", "worker"]
        envFrom:
        - secretRef:
            name: zinc  # Infisical secrets
```

3. **Add to values**:

```yaml
# infra/root_chart/values.yaml
platform: arene
service: zinc
cluster: '' # Empty for local

modules:
  api:
    enabled: true
  worker:
    enabled: true # New module
  migration:
    enabled: true
```

4. **Update Tiltfile** (for local dev):

```python
# config/tilt/dev.Tiltfile
if config.parse_yaml(watch_file('config/dev.yaml'))['type'] == 'local':
    # API via mirrord (existing)
    local_resource(
        'zinc-api',
        serve_cmd='infisical run -- mirrord exec -- dotnet watch run',
        deps=['App/'],
    )

    # Worker via mirrord (new)
    local_resource(
        'zinc-worker',
        serve_cmd='infisical run -- mirrord exec -- dotnet watch run -- worker',
        deps=['App/'],
    )
```

5. **Deploy and verify**:

```bash
# Restart Tilt
pls stop
pls dev

# Check workload exists
kubectl get pods -n arene | grep worker
# Should see: zinc-worker-*

# Check logs
kubectl logs -n arene -l module=worker -f
```

### Result

New resources created:

```
lapras-arene-zinc-worker              # Local development
tauros-arene-zinc-worker              # Integration testing
pikachu-opal-arene-zinc-worker        # Staging
raichu-opal-arene-zinc-worker         # Production
```

---

## Example 5: Creating a New Service in Arene Platform

### Scenario: Add Web Scraper Service to Marketing Engine

**Goal**: Create a new `helium` service (scraper) alongside `zinc` (API) in the `arene` platform.

### Implementation

1. **Choose service name** (element set):

```
Service: helium (scrapers)
Platform: arene (Marketing Engine)
Full name: {landscape}-{cluster}-arene-helium-{module}
```

2. **Create repository structure**:

```bash
mkdir -p platforms/arene/helium
cd platforms/arene/helium

# Create ASP.NET project
dotnet new worker -n Helium

# Create Helm chart
mkdir -p infra/helium_chart
```

3. **Create Helm chart**:

```yaml
# infra/helium_chart/Chart.yaml
apiVersion: v2
name: helium
description: Helium scraper service for Arene Marketing Engine
version: 1.0.0
appVersion: 1.0.0

# infra/helium_chart/values.yaml
platform: arene
service: helium
cluster: ''

image:
  repository: atomi/arene-helium
  tag: latest

modules:
  scraper:
    enabled: true
```

4. **Add to root chart**:

```yaml
# infra/root_chart/Chart.yaml (in zinc repo)
dependencies:
  - name: zinc
    version: '1.0.0'
    repository: 'file://../api_chart'
    condition: zinc.enabled

  - name: helium # New service
    version: '1.0.0'
    repository: 'file://../../helium/infra/helium_chart'
    condition: helium.enabled

# infra/root_chart/values.yaml
zinc:
  enabled: true
  platform: arene
  service: zinc

helium: # New service
  enabled: true
  platform: arene
  service: helium
```

5. **Deploy**:

```bash
# Update Tilt
cd platforms/arene/zinc
pls stop
pls dev

# Both services now running in arene namespace
kubectl get pods -n arene
# Should see:
# - zinc-api-*
# - zinc-maindb-*
# - helium-scraper-*
```

### Result

New resources in `arene` namespace:

```
Namespace: arene (shared by zinc and helium)

Zinc Service:
- lapras-arene-zinc-api
- lapras-arene-zinc-maindb
- lapras-arene-zinc-maincache

Helium Service:
- lapras-arene-helium-scraper
```

**URL Structure**:

```
http://api.zinc.arene.lapras.lvh.me:20010          # Zinc API
http://scraper.helium.arene.lapras.lvh.me:20010    # Helium Scraper
```

---

## Example 6: Multi-Cluster Production Deployment

### Scenario: Deploy to Both Opal and Ruby Clusters for Blue-Green

**Goal**: Deploy Zinc to both `opal` (primary) and `ruby` (standby) clusters in `pikachu` staging environment for testing cluster rotation.

### Implementation

1. **Deploy to opal cluster**:

```bash
export LANDSCAPE=pikachu

# Switch to opal cluster
kubectl config use-context pikachu-opal-cluster

# Deploy via Helm
helm upgrade --install arene-zinc ./infra/root_chart \
  -f infra/root_chart/values.yaml \
  -f infra/root_chart/values.pikachu.yaml \
  --set cluster=opal \
  --namespace arene \
  --create-namespace

# Verify
kubectl get pods -n arene
# Should see: zinc-api-*, zinc-maindb-*
```

2. **Deploy to ruby cluster** (standby):

```bash
# Switch to ruby cluster
kubectl config use-context pikachu-ruby-cluster

# Deploy same chart
helm upgrade --install arene-zinc ./infra/root_chart \
  -f infra/root_chart/values.yaml \
  -f infra/root_chart/values.pikachu.yaml \
  --set cluster=ruby \
  --namespace arene \
  --create-namespace

# Verify
kubectl get pods -n arene
# Should see: zinc-api-*, zinc-maindb-*
```

3. **Configure DNS/Load Balancer**:

```
Primary:  api.zinc.arene.opal.pikachu.atomi.cloud  → opal cluster
Standby:  api.zinc.arene.ruby.pikachu.atomi.cloud  → ruby cluster
Failover: api.zinc.arene.pikachu.atomi.cloud       → health check routing
```

4. **Traffic Routing**:

```yaml
# Example Traefik IngressRoute
apiVersion: traefik.containo.us/v1alpha1
kind: IngressRoute
metadata:
  name: zinc-api
  namespace: arene
spec:
  entryPoints:
    - websecure
  routes:
    - match: Host(`api.zinc.arene.opal.pikachu.atomi.cloud`)
      kind: Rule
      services:
        - name: zinc-api
          port: 8080
```

5. **Switch from Opal to Ruby** (cluster rotation):

```bash
# Update DNS to point to ruby cluster
# Or update load balancer upstream

# Traffic now flows to:
# pikachu-ruby-arene-zinc-api (instead of opal)

# Decomission opal when ready
helm uninstall arene-zinc -n arene --kube-context pikachu-opal-cluster
```

### Result

**During Rotation**:

```
pikachu-opal-arene-zinc-api          # Primary (receiving traffic)
pikachu-opal-arene-zinc-maindb

pikachu-ruby-arene-zinc-api          # Standby (ready for failover)
pikachu-ruby-arene-zinc-maindb
```

**After Rotation**:

```
pikachu-ruby-arene-zinc-api          # Now primary
pikachu-ruby-arene-zinc-maindb

(opal cluster decommissioned)
```

---

## Example 7: Debugging Across Landscapes

### Scenario: API Works in Lapras but Fails in Pikachu

**Goal**: Compare configurations and identify differences between local and staging.

### Investigation Steps

1. **Compare configurations**:

```bash
# Local configuration
cat App/Config/settings.lapras.yaml

# Staging configuration
cat App/Config/settings.pikachu.yaml

# Look for differences in:
# - Database connection strings
# - Auth settings
# - External service URLs
# - Feature flags
```

2. **Check resource names**:

```bash
# Local resources
kubectl get pods -n arene --context k3d-lapras
# Output: zinc-api-*, zinc-maindb-*

# Staging resources
kubectl get pods -n arene --context pikachu-opal-cluster
# Output: zinc-api-*, zinc-maindb-*
# (Workload names are the same, landscape/cluster differ)
```

3. **Compare environment variables**:

```bash
# Local (via mirrord)
infisical run --env=lapras -- env | grep Atomi_

# Staging (in pod)
kubectl exec -n arene zinc-api-xxx --context pikachu-opal-cluster -- env | grep Atomi_
```

4. **Check Infisical secrets**:

```bash
# Local secrets
infisical secrets --env=lapras

# Staging secrets
infisical secrets --env=pikachu

# Look for missing or different values
```

5. **Compare database connections**:

```bash
# Local: Uses k3d PostgreSQL
zinc-maindb.arene.svc.cluster.local:5432

# Staging: Might use managed database
some-managed-postgres.cloud-provider.com:5432
```

6. **Test in absol landscape** (local prod-like):

```bash
# Use absol to test production-like config locally
export LANDSCAPE=absol
pls dev

# This uses settings.absol.yaml which should mirror pikachu/raichu
```

### Common Issues

| Issue        | Lapras          | Pikachu          | Solution                                          |
| ------------ | --------------- | ---------------- | ------------------------------------------------- |
| **Database** | k3d PostgreSQL  | Cloud SQL        | Update connection string in settings.pikachu.yaml |
| **Auth**     | Disabled/Mock   | Real Cognito     | Ensure Infisical has correct auth tokens          |
| **CORS**     | `localhost:*`   | Specific domains | Update AllowedOrigins in settings.pikachu.yaml    |
| **Secrets**  | Local Infisical | Prod Infisical   | Verify External Secrets Operator synced           |
| **Ingress**  | `lvh.me:20010`  | `atomi.cloud`    | Check Traefik IngressRoute                        |

---

## Example 8: Using Corsola for Quick Scripts

### Scenario: Test Database Connection Without Kubernetes

**Goal**: Quickly test PostgreSQL connection logic without spinning up k3d cluster.

### Implementation

1. **Set corsola landscape**:

```bash
export LANDSCAPE=corsola
```

2. **Create minimal config**:

```yaml
# App/Config/settings.corsola.yaml
Database:
  MainDb:
    ConnectionString: 'Host=localhost;Port=5432;Database=zinc_test;Username=postgres;Password=postgres'

Auth:
  Enable: false

OpenTelemetry:
  Enable: false
```

3. **Run PostgreSQL locally** (Docker):

```bash
docker run -d \
  --name postgres-corsola \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=zinc_test \
  -p 5432:5432 \
  postgres:15-alpine
```

4. **Run .NET app directly**:

```bash
# No k3d, no mirrord, no Tilt
dotnet run --project App
```

5. **Test connection**:

```bash
curl http://localhost:5000/health/database
# Should return: healthy
```

### When to Use Corsola

✅ **Good for**:

- Quick database migration testing
- Testing configuration changes
- Debugging connection issues
- Running scripts
- Local-only dependencies

❌ **Not for**:

- Production-like testing (use absol)
- Integration testing (use tauros)
- Development with full stack (use lapras)

---

## Summary

These examples demonstrate:

- **Understanding** existing LPSM resource names
- **Working** with different landscapes (lapras, tauros, raichu)
- **Adding** new modules and services
- **Deploying** to multi-cluster environments
- **Debugging** across landscapes
- **Using** special landscapes (corsola, absol)

The LPSM system provides consistent resource organization across all environments while allowing flexibility for different deployment patterns.
