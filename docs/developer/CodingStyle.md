# Coding Style & Naming

Write code that reads like an explanation. A consistent style makes reviews faster and bugs rarer.

Why

- Predictability speeds up navigation and reviews.
- Linters catch small issues so humans can focus on logic.

Editor & formatting

- Controlled by `.editorconfig`:
  - 2‑space indent, UTF‑8, braces required.
  - Organize usings with System first; prefer `var` when type is apparent.
  - Expression‑bodied members allowed for properties.

Naming (cheat‑sheet)

- Types/methods/properties: PascalCase → `UserService`, `GetById`.
- Locals/parameters: camelCase → `userService`, `id`.
- Constants: PascalCase with `const` → enforced by naming styles.

Structure

- Keep features under `App/Modules/<Feature>`; avoid cross‑module dependencies.
- Keep domain (`Domain/*`) persistence‑agnostic; inject interfaces at the edges.

Linters & formatting

- Run `pls lint` to apply project linters (`dotnetlint`, `helmlint`, `infralint`, `gitlint`, `shellcheck`, `treefmt`).
- Commit messages follow Conventional Commits (see CommitConventions.md).

Tips

- Prefer small, focused methods with clear names over comments explaining complex logic.
- Don’t be clever with names; be obvious.
