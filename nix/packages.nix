{ pkgs, atomi, pkgs-2505, pkgs-unstable }:
let
  all = rec {
    atomipkgs = (
      with atomi;
      rec {
        dotnetlint = atomi.dotnetlint.override { dotnetPackage = nix-2505.dotnet; };
        helmlint = atomi.helmlint.override { helmPackage = infrautils; };

        # Alias gardenio as garden for convenience
        garden = gardenio;

        inherit
          #infra
          infrautils
          infralint
          atomiutils
          sg
          pls
          mirrord
          ;
      }
    );
    nix-unstable = (
      with pkgs-unstable;
      { }
    );
    nix-2505 = (
      with pkgs-2505;
      {
        dotnet = dotnet-sdk;
        inherit

          # standard
          bun
          git
          infisical
          xmlstarlet

          # development tools
          k6

          treefmt
          gitlint
          shellcheck
          ;
      }
    );

  };
in
with all;
nix-2505 //
nix-unstable //
atomipkgs

