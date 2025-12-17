# Preview Environment Scripts

Scripts for creating preview environments with **production data** using Neon database branching and Tigris storage branching.

## Overview

Preview environments (absol) allow you to:
- Test changes with real production data (safely isolated)
- Demo features to stakeholders
- Test production builds locally
- Validate external infrastructure configuration

## How It Works

1. **Database Branching (Neon)**:
   - Creates instant copy-on-write branch from production database
   - Branch inherits all data from parent (main)
   - Changes to branch don't affect production
   - Only delta is stored (cost-effective)

2. **Storage Branching (Tigris)**:
   - Creates instant copy-on-write branch from production bucket
   - Branch inherits all objects from parent bucket
   - S3-compatible API
   - Only modified objects consume additional storage

3. **Generated Configuration**:
   - Scripts generate `values.absol-generated.yaml` with branch endpoints
   - `.preview-branches` file stores metadata for cleanup

## Prerequisites

Set these environment variables before creating preview environments:

```bash
export NEON_PROJECT_ID=your-neon-project-id
export NEON_API_KEY=your-neon-api-key
export TIGRIS_PROJECT_ID=your-tigris-project-id
export TIGRIS_API_KEY=your-tigris-api-key
```

**Where to get API keys**:
- **Neon**: [console.neon.tech](https://console.neon.tech) → Settings → API Keys
- **Tigris**: [console.tigris.dev](https://console.tigris.dev) → Settings → Access Keys

## Usage

### Create Preview Environment

```bash
# Via Taskfile (recommended)
pls preview

# Or directly
./scripts/preview/create-branches.sh
```

**What happens**:
1. Derives branch name from current git branch: `preview-{git-branch}`
2. Creates Neon database branch from `main`
3. Creates Tigris bucket branch from `zinc-production-files`
4. Generates `infra/root_chart/values.absol-generated.yaml` with branch endpoints
5. Saves branch metadata to `.preview-branches` for cleanup

**Output**:
```
Creating preview environment branches...
Git branch: feature/new-endpoint
Branch name: preview-feature-new-endpoint

Creating Neon database branch...
✓ Neon branch created:
  Branch ID: br-blue-rain-12345678
  Endpoint:  ep-blue-rain-12345678.us-east-1.aws.neon.tech

Creating Tigris bucket branch...
✓ Tigris branch created:
  Bucket:   zinc-production-files--preview-feature-new-endpoint
  Endpoint: fly.storage.tigris.dev

✓ Values file created: ./infra/root_chart/values.absol-generated.yaml
✓ Branch info saved to ./.preview-branches

Preview environment branches ready!
Deploy with: garden deploy --env absol -f ./infra/root_chart/values.absol-generated.yaml
```

### Delete Preview Environment

```bash
# Via Taskfile (recommended)
pls preview:down

# Or directly
./scripts/preview/delete-branches.sh
```

**What happens**:
1. Reads branch metadata from `.preview-branches`
2. Deletes Neon database branch
3. Deletes Tigris bucket branch
4. Removes `.preview-branches` and `values.absol-generated.yaml`

**Output**:
```
Deleting preview environment branches...
Branch name: preview-feature-new-endpoint

Deleting Neon database branch...
✓ Neon branch deleted: br-blue-rain-12345678

Deleting Tigris bucket branch...
✓ Tigris branch deleted: zinc-production-files--preview-feature-new-endpoint

✓ Cleanup complete
```

## Generated Files

### values.absol-generated.yaml

Auto-generated Helm values override with branch endpoints:

```yaml
maindb:
  enable: true
  type: external
  external:
    host: ep-blue-rain-12345678.us-east-1.aws.neon.tech
    port: 5432
    database: arene-zinc
    sslMode: require
  branch:
    enabled: true
    parentBranch: main
    branchName: preview-feature-new-endpoint
    branchId: br-blue-rain-12345678
    projectId: ${NEON_PROJECT_ID}

mainstorage:
  enable: true
  type: external
  external:
    endpoint: https://fly.storage.tigris.dev
    region: auto
    bucket: zinc-production-files--preview-feature-new-endpoint
  branch:
    enabled: true
    parentBucket: zinc-production-files
    branchName: preview-feature-new-endpoint
    projectId: ${TIGRIS_PROJECT_ID}

maincache:
  enable: true
  type: crd
  crd:
    replicas: 1
```

**Important**: This file is auto-generated and should NOT be committed to git (already in `.gitignore`).

### .preview-branches

Metadata file for cleanup:

```bash
BRANCH_NAME=preview-feature-new-endpoint
NEON_BRANCH_ID=br-blue-rain-12345678
NEON_ENDPOINT=ep-blue-rain-12345678.us-east-1.aws.neon.tech
TIGRIS_BRANCH_BUCKET=zinc-production-files--preview-feature-new-endpoint
NEON_PROJECT_ID=your-project-id
TIGRIS_PROJECT_ID=your-project-id
```

**Important**: This file contains branch IDs needed for cleanup and should NOT be committed to git (already in `.gitignore`).

## Workflow Example

### 1. Preview PR Changes

```bash
# On feature branch
git checkout feature/new-endpoint

# Create preview
pls preview

# Deploy to absol cluster
garden deploy --env absol

# Test changes
curl http://api.zinc.arene.absol.lvh.me:20010/api/v1/projects

# Tear down
pls preview:down
```

### 2. Demo to Stakeholders

```bash
# Create preview
pls preview

# Deploy
garden deploy --env absol

# Share URLs with team
# - API: http://api.zinc.arene.absol.lvh.me:20010/swagger
# - MinIO Console: http://console-mainstorage.zinc.arene.absol.lvh.me:20010

# Clean up after demo
pls preview:down
```

### 3. Test External Config

```bash
# Create branches
pls preview

# Test with custom values (e.g., different cloud provider)
garden deploy --env absol -f ./infra/root_chart/values.absol-external.yaml

# Verify cloud connections
curl http://api.zinc.arene.absol.lvh.me:20010/health

# Clean up
pls preview:down
```

## Branch Naming Convention

Branch names are automatically derived from git branch:

| Git Branch | Preview Branch Name |
|------------|---------------------|
| `main` | `preview-main` |
| `feature/user-export` | `preview-feature-user-export` |
| `fix/auth-bug` | `preview-fix-auth-bug` |
| `feat/api-v2` | `preview-feat-api-v2` |

Slashes (`/`) are replaced with dashes (`-`) to ensure compatibility with Neon and Tigris naming requirements.

## Cost Considerations

**Neon Database Branching**:
- Branches use copy-on-write (only delta is billed)
- Typical preview: < $0.01 per hour
- Compute: Only when queried
- Storage: Only changed data

**Tigris Storage Branching**:
- Branches use copy-on-write (only delta is billed)
- No egress fees
- Storage: Only modified objects
- Typical preview: < $0.01 per hour

**Best Practice**: Always tear down preview environments when done to avoid unnecessary costs.

## Troubleshooting

### Error: "NEON_PROJECT_ID not set"

Set required environment variables:

```bash
export NEON_PROJECT_ID=your-neon-project-id
export NEON_API_KEY=your-neon-api-key
export TIGRIS_PROJECT_ID=your-tigris-project-id
export TIGRIS_API_KEY=your-tigris-api-key
```

### Error: "Error creating Neon branch"

Check API response:
- Verify `NEON_API_KEY` is valid
- Verify `NEON_PROJECT_ID` is correct
- Check parent branch exists (`main`)
- Check API quota limits

### Error: ".preview-branches file not found"

This happens when trying to delete branches that don't exist. Safe to ignore - script will exit gracefully.

### Branch Already Exists

Delete existing branch manually:

```bash
# Neon
neon branches delete preview-your-branch-name --project-id ${NEON_PROJECT_ID}

# Tigris
tigris delete branch --bucket zinc-production-files --branch preview-your-branch-name
```

Then retry `pls preview`.

## API Documentation

### Neon API

- **Docs**: [neon.tech/docs/reference/api-reference](https://neon.tech/docs/reference/api-reference)
- **Create Branch**: `POST /projects/{project_id}/branches`
- **Delete Branch**: `DELETE /projects/{project_id}/branches/{branch_id}`

### Tigris API

- **Docs**: [docs.tigris.dev/api/](https://docs.tigris.dev/api/)
- **Create Branch**: `POST /v1/projects/{project_id}/buckets/{bucket}/branches`
- **Delete Branch**: `DELETE /v1/projects/{project_id}/buckets/{branch_bucket}`

## Security Notes

1. **API Keys**: Never commit API keys to git
2. **Branch Isolation**: Branches are isolated but use production credentials
3. **Data Safety**: Changes to branches don't affect production
4. **Cleanup**: Always delete branches when done
5. **Access Control**: Use separate API keys for preview environments (not production keys)

## See Also

- [MIGRATION_PLAN_V3.md](../../MIGRATION_PLAN_V3.md) - Complete migration strategy
- [GARDEN_QUICKSTART.md](../../GARDEN_QUICKSTART.md) - Quick reference guide
- [Neon Branching Guide](https://neon.tech/docs/guides/branching)
- [Tigris Documentation](https://docs.tigris.dev/)
