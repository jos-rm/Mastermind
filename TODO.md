# TODO

- [x] Implement new overload of `Mastermind.EvaluarIntento` with `out string log`, optional `cheatCode`, and optional `validColors`.
- [x] Add input validation: allowed colors + size must be 4 or 6; throw `ArgumentException` on invalid input.
- [x] Implement cheatcodes.json loading + per-session remaining uses; if valid and usable reveal one unguessed position; update `log` only (tuple unchanged).
- [x] Keep existing `EvaluarIntento(string,string)` signature intact; delegate to the new overload.
- [x] Ensure log format matches exact required format.
- [x] Run `dotnet test` to ensure all existing tests pass.

