# Nix Dependencies - Examples

Complete examples of adding different types of packages to the Zinc project.

## Example 1: Adding Development Tools (Garden, KubeVPN, K6)

### Scenario

You're migrating from Tilt+mirrord to Garden+KubeVPN and need to add:

- `garden` - Development orchestration (from Atomi registry)
- `kubevpn` - Network VPN tunnel (from nixpkgs)
- `k6` - Load testing tool (from nixpkgs)

### Step 1: Add to `nix/packages.nix`

**Note**: The package in Atomi registry is called `gardenio`, but we alias it as `garden` for convenience.

```nix
{ pkgs, atomi, pkgs-2505, pkgs-unstable }:
let
  all = rec {
    atomipkgs = (
      with atomi;
      rec {
        dotnetlint = atomi.dotnetlint.override { dotnetPackage = nix-2505.dotnet; };
        helmlint = atomi.helmlint.override { helmPackage = infrautils; };

        # Alias gardenio as garden for convenience
        garden = gardenio;  # <-- Added: From Atomi registry (actual name: gardenio)

        inherit
          infrautils
          infralint
          atomiutils
          sg
          pls
      }
    );

    nix-2505 = (
      with pkgs-2505;
      {
        dotnet = dotnet-sdk;
        inherit
          bun
          git
          infisical
          xmlstarlet
          k6                # <-- Added: Load testing tool
          kubevpn           # <-- Added: VPN tunnel
          treefmt
          gitlint
          shellcheck
          ;
      }
    );

    nix-unstable = (
      with pkgs-unstable;
      { }
    );
  };
in
with all;
nix-2505 //
nix-unstable //
atomipkgs
```

### Step 2: Add to `nix/env.nix`

```nix
{ pkgs, packages }:
with packages;
{
  system = [
    atomiutils
    xmlstarlet
  ];

  dev = [
    pls
    git
    garden          # <-- Added: Development orchestration
    kubevpn         # <-- Added: Network tunneling
    k6              # <-- Added: Load testing
  ];

  infra = [
    infrautils
  ];

  main = [
    bun
    dotnet
    infisical
  ];

  lint = [
    treefmt
    gitlint
    shellcheck
    infralint
    dotnetlint
    helmlint
    sg
  ];

  releaser = [
    sg
  ];
}
```

### Step 3: Verify

```bash
# Rebuild environment
nix develop

# Verify tools are available
garden version
# Garden Core v0.x.x

kubevpn version
# KubeVPN version v2.x.x

k6 version
# k6 v0.x.x
```

---

## Example 2: Adding Infrastructure Tool (Terraform)

### Scenario

You need to add Terraform for infrastructure as code.

### Step 1: Search for the package

```bash
# Find package in nixpkgs
nix search nixpkgs#terraform
# * legacyPackages.aarch64-darwin.terraform (1.6.5)
```

### Step 2: Add to `nix/packages.nix`

```nix
nix-2505 = (
  with pkgs-2505;
  {
    inherit
      # ... existing packages
      terraform       # <-- Added
      ;
  }
);
```

### Step 3: Add to `nix/env.nix`

```nix
infra = [
  infrautils
  terraform         # <-- Added to infra category
];
```

### Result

Terraform is now available in all shells (default, ci, releaser) since `infra` is included in all.

---

## Example 3: Adding Linting Tool (yamllint)

### Scenario

Add YAML linting to the project.

### Step 1: Add to `nix/packages.nix`

```nix
nix-2505 = (
  with pkgs-2505;
  {
    inherit
      # ... existing packages
      yamllint        # <-- Added
      ;
  }
);
```

### Step 2: Add to `nix/env.nix`

```nix
lint = [
  treefmt
  gitlint
  shellcheck
  infralint
  dotnetlint
  helmlint
  sg
  yamllint          # <-- Added to lint category
];
```

### Step 3: Update pre-commit hooks

```nix
# nix/pre-commit.nix
{
  hooks = {
    yamllint = {
      enable = true;
      settings = {
        configPath = ".yamllint.yaml";
      };
    };
  };
}
```

---

## Example 4: Adding CLI Tool from Unstable (Latest Version)

### Scenario

You need the latest version of `kubectl` which isn't in stable yet.

### Step 1: Add to `nix/packages.nix`

```nix
nix-unstable = (
  with pkgs-unstable;
  {
    kubectl-latest = kubectl;  # Alias for clarity
  }
);
```

### Step 2: Add to `nix/env.nix`

```nix
infra = [
  infrautils
  kubectl-latest    # <-- Latest kubectl from unstable
];
```

**Note**: This overrides the kubectl from `infrautils` if there's a conflict. Be careful with naming.

---

## Example 5: Adding Custom Wrapper Tool from Atomi

### Scenario

AtomiCloud has a custom tool wrapper that needs a specific dependency version.

### Step 1: Add to `nix/packages.nix`

```nix
atomipkgs = (
  with atomi;
  rec {
    # Custom wrapper with override
    customtool = atomi.customtool.override {
      nodePackage = nix-2505.nodejs-18_x;  # Specific Node.js version
    };

    inherit
      # ... other packages
      ;
  }
);
```

### Step 2: Add to `nix/env.nix`

```nix
dev = [
  pls
  git
  customtool        # <-- Custom wrapper
];
```

---

## Example 6: Adding Python Package with Dependencies

### Scenario

Add Python with specific packages for scripting.

### Step 1: Add to `nix/packages.nix`

```nix
nix-2505 = (
  with pkgs-2505;
  {
    # Python with packages
    python-with-packages = python3.withPackages (ps: with ps; [
      requests
      pyyaml
      jinja2
    ]);

    inherit
      # ... other packages
      ;
  }
);
```

### Step 2: Add to `nix/env.nix`

```nix
dev = [
  pls
  git
  python-with-packages    # <-- Python + libraries
];
```

### Usage

```bash
# Python is available with all packages
python3 -c "import requests; print(requests.__version__)"
```

---

## Example 7: Conditional Package (macOS only)

### Scenario

Add a tool that's only available on macOS.

### Step 1: Add to `nix/packages.nix`

```nix
{ pkgs, atomi, pkgs-2505, pkgs-unstable }:
let
  all = rec {
    nix-2505 = (
      with pkgs-2505;
      {
        inherit
          git
          # ... other packages
          ;
      } // pkgs.lib.optionalAttrs pkgs.stdenv.isDarwin {
        # macOS-only packages
        inherit
          darwin.iproute2mac
          ;
      }
    );
  };
in
with all;
nix-2505 // atomipkgs
```

### Step 2: Add to `nix/env.nix`

```nix
system = [
  atomiutils
  xmlstarlet
] ++ pkgs.lib.optionals pkgs.stdenv.isDarwin [
  darwin.iproute2mac    # <-- Only on macOS
];
```

---

## Example 8: Multiple Related Tools (Docker Ecosystem)

### Scenario

Add Docker, Docker Compose, and Docker Credential Helpers.

### Step 1: Add to `nix/packages.nix`

```nix
nix-2505 = (
  with pkgs-2505;
  {
    inherit
      docker
      docker-compose
      docker-credential-helpers
      # ... other packages
      ;
  }
);
```

### Step 2: Add to `nix/env.nix`

```nix
dev = [
  pls
  git
  docker
  docker-compose
  docker-credential-helpers
];
```

---

## Testing Your Changes

After modifying `packages.nix` and `env.nix`:

```bash
# 1. Rebuild Nix environment
nix develop

# 2. Verify package availability
command -v garden
# /nix/store/.../bin/garden

command -v kubevpn
# /nix/store/.../bin/kubevpn

# 3. Check versions
garden version
kubevpn version
k6 version

# 4. Test in CI shell (if needed)
nix develop .#ci

# 5. Verify package NOT in CI (for dev-only tools)
command -v garden
# (should fail in CI shell if garden is in 'dev' category)
```

---

## Common Patterns Summary

| Use Case                 | Source         | Category    | Example            |
| ------------------------ | -------------- | ----------- | ------------------ |
| AtomiCloud tool          | `atomipkgs`    | `dev`       | `garden`, `pls`    |
| Infrastructure tool      | `nix-2505`     | `infra`     | `kubectl`, `helm`  |
| Development tool         | `nix-2505`     | `dev`       | `git`, `kubevpn`   |
| Application runtime      | `nix-2505`     | `main`      | `dotnet`, `bun`    |
| Linter/formatter         | `nix-2505`     | `lint`      | `shellcheck`       |
| Release tool             | `nix-2505`     | `releaser`  | `sg`               |
| System utility           | `nix-2505`     | `system`    | `xmlstarlet`       |
| Latest version needed    | `nix-unstable` | appropriate | Bleeding edge pkgs |
| Custom wrapper with deps | `atomipkgs`    | appropriate | Override pattern   |
