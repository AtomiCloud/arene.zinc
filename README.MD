# Zinc - ASP.NET Core 8 Microservice

Microservice written in .NET 8 using Domain-Driven Design with Result monad pattern.

> **🎉 Migration Complete!** This project now uses **Garden + KubeVPN** instead of Tilt + mirrord.
> - Quick Start: See [GARDEN_QUICKSTART.md](./GARDEN_QUICKSTART.md)
> - Migration Details: See [MIGRATION_COMPLETE.md](./MIGRATION_COMPLETE.md)
> - Full Architecture: See [MIGRATION_PLAN.md](./MIGRATION_PLAN.md)

## Prerequisites

- [Nix](https://nixos.org/)
- [direnv](https://direnv.net/)
- [Docker](https://www.docker.com/)

All development tools (Garden, KubeVPN, k6, etc.) are provided via Nix.

## Setup

```bash
# 1. Enter Nix environment (installs all tools)
nix develop

# 2. Initialize repository
pls setup

# 3. Verify tools
garden version    # From Atomi nix flake
kubevpn version   # From Nix
k6 version        # From Nix
```

## Development

Start the development environment with Garden + KubeVPN:

```bash
# Start development (creates cluster, connects VPN, deploys infra, runs API locally)
pls dev

# Stop development (disconnects VPN)
pls stop

# Tear down cluster
pls tear
```

**What `pls dev` does**:
1. Creates k3d cluster (if needed)
2. Connects KubeVPN to cluster
3. Deploys platform operators (CloudNativePG, Dragonfly, MinIO)
4. Deploys infrastructure (Postgres, Redis, MinIO)
5. Runs API locally with hot reload

See [GARDEN_QUICKSTART.md](./GARDEN_QUICKSTART.md) for complete guide.
