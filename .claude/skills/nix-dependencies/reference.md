# Nix Dependencies - Reference

Links and resources for managing Nix dependencies in the Zinc project.

## Official Documentation

### Nix Documentation

- **Nix Manual**: https://nixos.org/manual/nix/stable/
- **NixOS Wiki**: https://nixos.wiki/
- **Nix Pills**: https://nixos.org/guides/nix-pills/ (Deep dive tutorial)

### Package Search

- **NixOS Package Search**: https://search.nixos.org/packages
- **Nix Package Versions**: https://lazamar.co.uk/nix-versions/ (Find specific versions)
- **Repology**: https://repology.org/ (Compare package versions across repositories)

### Flakes

- **Nix Flakes Reference**: https://nixos.wiki/wiki/Flakes
- **Flakes Practical Guide**: https://www.tweag.io/blog/2020-05-25-flakes/

## AtomiCloud Registry

### Repository

- **GitHub**: https://github.com/AtomiCloud/nix-registry
- **Main Branch**: https://github.com/AtomiCloud/nix-registry/tree/v2

### Available Tools

Check the registry flake for available packages:

```bash
# List all packages in Atomi registry
nix flake show github:AtomiCloud/nix-registry/v2
```

Common AtomiCloud packages:

- `gardenio` - Garden.io development orchestration (alias as `garden` in packages.nix)
- `pls` - Task runner (alias for `task`)
- `infrautils` - Kubernetes utilities (kubectl, k3d, kubectx, helm)
- `infralint` - Infrastructure linting
- `dotnetlint` - .NET linting wrapper
- `helmlint` - Helm chart linting
- `sg` - Semantic-git release tool
- `atomiutils` - General Atomi utilities

**Important**: Some Atomi packages have different names than their commands. Always verify with:

```bash
nix flake show github:AtomiCloud/nix-registry/v2 | grep -i <search-term>
```

## Zinc Project Structure

### File Locations

```
zinc/
├── flake.nix                  # Main flake orchestration
└── nix/
    ├── packages.nix           # Package definitions (Step 1)
    ├── env.nix                # Environment categories (Step 2)
    ├── shells.nix             # Shell configurations (Step 3 - auto)
    ├── fmt.nix                # Treefmt configuration
    └── pre-commit.nix         # Pre-commit hooks
```

### Key Files

#### `flake.nix`

Main orchestration file that:

- Imports nixpkgs (stable and unstable)
- Imports AtomiCloud registry
- Wires together packages, env, and shells

```nix
{
  inputs = {
    nixpkgs-2505.url = "nixpkgs/nixos-25.05";
    nixpkgs-unstable.url = "nixpkgs/nixos-unstable";
    atomipkgs.url = "github:AtomiCloud/nix-registry/v2";
  };

  outputs = { self, nixpkgs-2505, nixpkgs-unstable, atomipkgs, ... }:
    # ... imports nix/packages.nix, nix/env.nix, nix/shells.nix
}
```

#### `nix/packages.nix`

Defines ALL available packages from all sources:

```nix
{ pkgs, atomi, pkgs-2505, pkgs-unstable }:
let
  all = rec {
    atomipkgs = ( /* Atomi packages */ );
    nix-2505 = ( /* Stable packages */ );
    nix-unstable = ( /* Unstable packages */ );
  };
in
with all;
nix-2505 // nix-unstable // atomipkgs
```

#### `nix/env.nix`

Categorizes packages into environment groups:

```nix
{ pkgs, packages }:
with packages;
{
  system = [ /* ... */ ];
  dev = [ /* ... */ ];
  infra = [ /* ... */ ];
  main = [ /* ... */ ];
  lint = [ /* ... */ ];
  releaser = [ /* ... */ ];
}
```

#### `nix/shells.nix`

Creates shell environments by combining categories:

```nix
{ pkgs, packages, env, shellHook }:
with env;
{
  default = pkgs.mkShell {
    buildInputs = system ++ main ++ lint ++ dev ++ infra;
  };

  ci = pkgs.mkShell {
    buildInputs = system ++ main ++ lint ++ infra;
    # Note: NO 'dev' category
  };

  releaser = pkgs.mkShell {
    buildInputs = system ++ main ++ lint ++ releaser ++ infra;
  };
}
```

## Shell Environments

### Available Shells

| Shell      | Categories Included                     | Use Case                        |
| ---------- | --------------------------------------- | ------------------------------- |
| `default`  | system + main + lint + dev + infra      | Local development (most common) |
| `ci`       | system + main + lint + infra            | CI/CD pipelines (NO dev tools)  |
| `releaser` | system + main + lint + releaser + infra | Release management              |

### Entering Shells

```bash
# Default shell (local development)
nix develop

# Or explicitly
nix develop .#default

# CI shell (without dev tools)
nix develop .#ci

# Releaser shell
nix develop .#releaser
```

## Package Sources

### nixpkgs-2505 (Stable)

**URL**: `nixpkgs/nixos-25.05`

**Use for**:

- Production-ready packages
- Stable versions
- Most common tools

**Search**: https://search.nixos.org/packages?channel=25.05

### nixpkgs-unstable (Latest)

**URL**: `nixpkgs/nixos-unstable`

**Use for**:

- Bleeding-edge versions
- Packages not yet in stable
- When you need latest features

**Search**: https://search.nixos.org/packages?channel=unstable

### AtomiCloud Registry

**URL**: `github:AtomiCloud/nix-registry/v2`

**Use for**:

- Custom AtomiCloud tools
- Wrapper packages
- Organization-specific utilities

## Command Reference

### Package Search

```bash
# Search nixpkgs
nix search nixpkgs#<package-name>

# Example: Search for kubevpn
nix search nixpkgs#kubevpn

# List all packages in Atomi registry
nix flake show github:AtomiCloud/nix-registry/v2
```

### Environment Management

```bash
# Enter default shell
nix develop

# Enter specific shell
nix develop .#ci
nix develop .#releaser

# Run command in shell without entering
nix develop -c <command>

# Example: Run dotnet test in Nix environment
nix develop -c dotnet test
```

### Debugging

```bash
# Show flake structure
nix flake show

# Check flake metadata
nix flake metadata

# Evaluate Nix expression
nix eval .#packages.aarch64-darwin.garden

# Show package info
nix search nixpkgs#kubevpn --json
```

### Updating Dependencies

```bash
# Update all flake inputs
nix flake update

# Update specific input
nix flake lock --update-input nixpkgs-2505
nix flake lock --update-input atomipkgs

# Check for outdated inputs
nix flake metadata
```

## Common Issues

### Package not found

**Error**: `attribute 'packageName' missing`

**Solution**: Check if package exists in the source

```bash
# For nixpkgs packages
nix search nixpkgs#packageName

# For Atomi packages
# Check: https://github.com/AtomiCloud/nix-registry/blob/v2/flake.nix
```

### Package available in packages.nix but not in shell

**Cause**: Package not added to `nix/env.nix`

**Solution**: Add package to appropriate category in `env.nix`

```nix
# nix/env.nix
dev = [
  pls
  git
  yourPackage    # <-- Add here
];
```

### Wrong package version

**Cause**: Package from wrong source

**Solution**: Check which input provides the version you need

```bash
# Check stable version
nix search nixpkgs/nixos-25.05#packageName

# Check unstable version
nix search nixpkgs/nixos-unstable#packageName

# Switch to appropriate source in packages.nix
```

### Conflicts between packages

**Error**: `collision between ... and ...`

**Cause**: Multiple packages provide same binary

**Solution**: Use package overrides or aliases

```nix
nix-2505 = (
  with pkgs-2505;
  {
    kubectl-stable = kubectl;  # Alias to avoid conflict
  }
);
```

## Best Practices

### 1. Prefer Stable Packages

Use `nix-2505` (stable) unless you specifically need unstable features.

### 2. Document Why Unstable is Used

```nix
nix-unstable = (
  with pkgs-unstable;
  {
    # Using unstable because stable version has bug #12345
    packageName = packageName;
  }
);
```

### 3. Use Semantic Naming for Overrides

```nix
atomipkgs = (
  with atomi;
  rec {
    # Clear naming for overridden versions
    dotnetlint = atomi.dotnetlint.override {
      dotnetPackage = nix-2505.dotnet;
    };
  }
);
```

### 4. Group Related Packages

```nix
dev = [
  # Version control
  git

  # Container tools
  docker
  docker-compose

  # Kubernetes
  kubectl
  kubectx
];
```

### 5. Minimize CI Dependencies

Keep `ci` shell lean by only including necessary tools in `system`, `main`, `lint`, and `infra` categories.

## Additional Resources

### Learning Nix

- **Zero to Nix**: https://zero-to-nix.com/ (Beginner-friendly guide)
- **Nix by Example**: https://nix.dev/ (Official learning resource)
- **Nix Language Basics**: https://nixos.org/manual/nix/stable/language/

### Community

- **NixOS Discourse**: https://discourse.nixos.org/
- **Nix Reddit**: https://www.reddit.com/r/NixOS/
- **Nix Matrix Chat**: https://matrix.to/#/#nix:nixos.org

### Tools

- **nix-tree**: Visualize dependency tree (`nix run nixpkgs#nix-tree`)
- **nix-diff**: Compare Nix derivations (`nix run nixpkgs#nix-diff`)
- **nixpkgs-fmt**: Format Nix code (`nix run nixpkgs#nixpkgs-fmt`)
