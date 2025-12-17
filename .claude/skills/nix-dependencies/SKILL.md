---
name: nix-dependencies
description: Add or update Nix dependencies in the Zinc project using the proper three-step process (packages.nix, env.nix, shells.nix)
---

# Nix Dependencies Skill

Use this skill when adding or updating development tools, CLI utilities, or other dependencies via Nix flakes.

## Related Documentation

- **[examples.md](examples.md)** - Complete examples of adding different types of packages
- **[reference.md](reference.md)** - Links to Nix registries and documentation

## Overview

The Zinc project uses Nix flakes for reproducible development environments. Dependencies come from three sources:

1. **Atomi Registry** (`atomipkgs`) - Custom tools from AtomiCloud
2. **nixpkgs-2505** (`nix-2505`) - Stable NixOS 25.05 packages
3. **nixpkgs-unstable** (`nix-unstable`) - Latest packages

## Critical Three-Step Process

**IMPORTANT**: Adding a package requires THREE steps, not just one!

### Step 1: Add to `nix/packages.nix`

Define the package in the appropriate source section:

**IMPORTANT**: Atomi registry package names may differ from the command name. Always check actual package names using `nix flake show github:AtomiCloud/nix-registry/v2`.

```nix
{ pkgs, atomi, pkgs-2505, pkgs-unstable }:
let
  all = rec {
    atomipkgs = (
      with atomi;
      rec {
        # Alias gardenio (actual package name) as garden (command name)
        garden = gardenio;

        inherit
          infrautils
          # Add other Atomi packages here
      }
    );

    nix-2505 = (
      with pkgs-2505;
      {
        inherit
          git
          kubevpn  # <-- Add stable packages here
          ;
      }
    );

    nix-unstable = (
      with pkgs-unstable;
      {
        # <-- Add bleeding-edge packages here
      }
    );
  };
in
with all;
nix-2505 //
nix-unstable //
atomipkgs
```

### Step 2: Add to `nix/env.nix` (CRITICAL!)

Categorize the package so it's available in shell environments:

```nix
{ pkgs, packages }:
with packages;
{
  system = [
    atomiutils      # Core system utilities
    xmlstarlet      # System tools
  ];

  dev = [
    pls             # Development tools
    git             # Version control
    garden          # <-- Add development orchestration tools here
    kubevpn         # <-- Add network tools here
    k6              # <-- Add testing tools here
  ];

  infra = [
    infrautils      # Infrastructure management (kubectl, k3d, etc.)
  ];

  main = [
    bun             # Primary runtime/build tools
    dotnet          # Application runtime
    infisical       # Secrets management
  ];

  lint = [
    treefmt         # Code formatters and linters
    gitlint
    shellcheck
  ];

  releaser = [
    sg              # Release management tools
  ];
}
```

### Step 3: Verify in `nix/shells.nix` (Auto-configured)

The shells automatically combine package groups:

```nix
{ pkgs, packages, env, shellHook }:
with env;
{
  default = pkgs.mkShell {
    buildInputs = system ++ main ++ lint ++ dev ++ infra;
    inherit shellHook;
  };

  ci = pkgs.mkShell {
    buildInputs = system ++ main ++ lint ++ infra;
    inherit shellHook;
  };

  releaser = pkgs.mkShell {
    buildInputs = system ++ main ++ lint ++ releaser ++ infra;
    inherit shellHook;
  };
}
```

**Note**: You don't modify `shells.nix` directly. It automatically picks up packages from the categories you defined in `env.nix`.

## Package Sources

### When to Use Each Source

| Source         | Use Case                               | Examples                      |
| -------------- | -------------------------------------- | ----------------------------- |
| `atomipkgs`    | AtomiCloud custom tools and wrappers   | `garden`, `pls`, `infrautils` |
| `nix-2505`     | Stable, well-tested packages (default) | `git`, `dotnet`, `kubevpn`    |
| `nix-unstable` | Latest versions not yet in stable      | Cutting-edge tools            |

### How to Find Packages

1. **AtomiCloud Registry**: Check `github.com/AtomiCloud/nix-registry`
2. **nixpkgs**: Search at `search.nixos.org/packages`
3. **Verify availability**: Use `nix search nixpkgs#<package-name>`

```bash
# Search for a package
nix search nixpkgs#kubevpn

# Check if package is in Atomi registry
# (Visit github.com/AtomiCloud/nix-registry/blob/v2/flake.nix)
```

## Environment Categories

### `system` - Core System Utilities

Low-level tools required by scripts or other tools.

**Examples**: `xmlstarlet`, `atomiutils`, `coreutils`

**Available in**: All shells (default, ci, releaser)

### `dev` - Development Tools

Tools used during active development but not required for CI/build.

**Examples**: `git`, `garden`, `kubevpn`, `k6`, `pls`

**Available in**: `default` shell only (NOT in `ci` shell)

**Use when**: Developer productivity tools, local orchestration, interactive debugging

### `infra` - Infrastructure Management

Kubernetes and infrastructure deployment tools.

**Examples**: `infrautils` (kubectl, k3d, kubectx, helm)

**Available in**: All shells (default, ci, releaser)

**Use when**: Cluster management, deployment, infrastructure automation

### `main` - Primary Runtimes

Core language runtimes and build tools required for the application.

**Examples**: `dotnet`, `bun`, `infisical`

**Available in**: All shells (default, ci, releaser)

**Use when**: Building, running, or testing the application

### `lint` - Code Quality Tools

Formatters, linters, and code quality checkers.

**Examples**: `treefmt`, `gitlint`, `shellcheck`, `dotnetlint`

**Available in**: All shells (default, ci, releaser)

**Use when**: Code formatting, linting, pre-commit hooks

### `releaser` - Release Management

Tools for creating releases, changelogs, and version management.

**Examples**: `sg` (semantic-git)

**Available in**: `releaser` shell only

**Use when**: Publishing releases, managing versions

## Complete Example: Adding Garden, KubeVPN, and K6

### 1. Add to `nix/packages.nix`

```nix
{ pkgs, atomi, pkgs-2505, pkgs-unstable }:
let
  all = rec {
    atomipkgs = (
      with atomi;
      rec {
        inherit
          infrautils
          pls
          garden;        # <-- From Atomi registry
      }
    );

    nix-2505 = (
      with pkgs-2505;
      {
        inherit
          git
          dotnet
          kubevpn        # <-- From nixpkgs 25.05
          k6             # <-- From nixpkgs 25.05
          ;
      }
    );
  };
in
with all;
nix-2505 // atomipkgs
```

### 2. Add to `nix/env.nix`

```nix
{ pkgs, packages }:
with packages;
{
  dev = [
    pls
    git
    garden         # <-- Development orchestration
    kubevpn        # <-- Network tunneling
    k6             # <-- Load testing
  ];

  # ... other categories remain unchanged
}
```

### 3. Test the Changes

```bash
# Rebuild Nix environment
nix develop

# Verify packages are available
garden version
kubevpn version
k6 version
```

## Common Mistakes

### ❌ Mistake 1: Only updating `packages.nix`

```nix
# nix/packages.nix - Added kubevpn ✓
nix-2505 = (
  with pkgs-2505;
  { inherit kubevpn; }
);

# nix/env.nix - FORGOT TO ADD! ✗
# Result: Package not available in shell
```

**Fix**: Always add to BOTH `packages.nix` AND `env.nix`

### ❌ Mistake 2: Wrong package source

```nix
# Trying to get garden from nixpkgs
nix-2505 = (
  with pkgs-2505;
  { inherit garden; }  # ✗ Not in nixpkgs!
);
```

**Fix**: Check package source first. Garden is in Atomi registry:

```nix
atomipkgs = (
  with atomi;
  { inherit garden; }  # ✓ Correct source
);
```

### ❌ Mistake 3: Wrong environment category

```nix
# Putting development tool in 'main'
main = [
  dotnet
  garden  # ✗ Should be in 'dev', not 'main'
];
```

**Fix**: Use appropriate category:

```nix
dev = [
  pls
  git
  garden  # ✓ Development orchestration belongs in 'dev'
];
```

## Verification Checklist

After adding a package:

- [ ] Added to appropriate section in `nix/packages.nix` (atomipkgs, nix-2505, or nix-unstable)
- [ ] Added to appropriate category in `nix/env.nix` (system, dev, infra, main, lint, or releaser)
- [ ] Ran `nix develop` to rebuild environment
- [ ] Verified package is available: `<package> --version` or `command -v <package>`
- [ ] Tested in CI (if package should be available in CI)

## Troubleshooting

### Package not found in shell

**Problem**: Added to `packages.nix` but not available

**Solution**: Check if you added it to `env.nix`

```bash
# Verify package is defined
grep -r "your-package" nix/

# Should appear in BOTH packages.nix AND env.nix
```

### Package not available in CI

**Problem**: Works locally but fails in CI

**Solution**: Check if package category is included in `ci` shell

```nix
# nix/shells.nix
ci = pkgs.mkShell {
  buildInputs = system ++ main ++ lint ++ infra;
  # Note: 'dev' is NOT included in CI shell
};
```

If your package is in `dev` category but needed in CI, move it to appropriate category (`infra`, `main`, or `system`).

### Wrong package version

**Problem**: Getting old or new version unexpectedly

**Solution**: Check which source you're using

- `nix-2505` → Stable, older versions
- `nix-unstable` → Latest versions
- `atomipkgs` → AtomiCloud versions

Switch sources if needed:

```nix
# Move from stable to unstable for newer version
nix-2505 = (
  with pkgs-2505;
  { inherit git; }  # Stable version
);

# To get latest version:
nix-unstable = (
  with pkgs-unstable;
  { inherit git; }  # Latest version
);
```

## Advanced: Overriding Package Versions

Some Atomi packages are wrappers that depend on other packages:

```nix
atomipkgs = (
  with atomi;
  rec {
    # Override dotnetlint to use specific dotnet version
    dotnetlint = atomi.dotnetlint.override {
      dotnetPackage = nix-2505.dotnet;
    };

    # Override helmlint to use specific helm
    helmlint = atomi.helmlint.override {
      helmPackage = infrautils;
    };

    inherit infrautils pls garden;
  }
);
```

This ensures wrapper tools use the correct versions of their dependencies.

## Summary

**Golden Rule**: Adding a Nix package is a THREE-step process:

1. **Define** in `nix/packages.nix` (from appropriate source)
2. **Categorize** in `nix/env.nix` (into appropriate environment group)
3. **Verify** with `nix develop` and test the command

Skip step 2, and your package won't be available in the shell!
