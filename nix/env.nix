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
    garden
    mirrord
    k6
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
    # core
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
