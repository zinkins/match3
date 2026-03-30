# Architecture Refactor Plan

## Goal

Make the project lighter and easier to maintain by restoring a clean dependency flow:

```text
Platform -> Presentation -> Core
```

The refactor keeps the existing gameplay behavior, reduces cross-layer leakage, and adds tests that guard the target architecture.

## Step-by-step plan

### Phase 1 - Purify `Match3.Core`

1. Remove MonoGame package references from `Match3.Core`.
2. Move runtime contracts (`IGameCanvas`, `IGameScreenHost`, `InputState`) out of `Core` into `Match3.Presentation/Runtime/`.
3. Move MonoGame host code (`Match3Game`, `MonoGameCanvas`) out of `Core` into shared platform-host files compiled by launcher projects.
4. Remove direct platform-project references to `Match3.Core` so launchers depend on `Match3.Presentation` only.
5. Add architecture guard tests for project references, package references, and removed legacy runtime types.

Acceptance criteria:

- `Match3.Core` has no MonoGame package references.
- `Match3.Core` contains only gameplay and application flow code.
- platform launchers compile without directly referencing `Match3.Core`.

### Phase 2 - Split presentation orchestration

1. Extract gameplay screen creation from `ScreenFlowController` into `GameplayScreenFactory`.
2. Extract gameplay frame updates into `GameplayRuntimeUpdater`.
3. Extract gameplay click processing into `GameplayInteractionController`.
4. Extract move-to-animation orchestration into `GameplayTurnAnimationCoordinator`.
5. Keep `PresentationScreenHost` focused on high-level routing between screen flow, input, and rendering.

Acceptance criteria:

- `ScreenFlowController` no longer constructs the gameplay graph inline.
- `PresentationScreenHost` delegates frame work and gameplay interaction to dedicated collaborators.

### Phase 3 - Move score into session state

1. Add `Score` state to `GameSession`.
2. Let `TurnProcessor` update session score when score events are produced.
3. Change `GameplayPresenter` to read score directly from `GameSession` instead of owning a second copy.
4. Add tests that verify presenter score mirrors session score.

Acceptance criteria:

- score has a single source of truth.
- presenter becomes a thin adapter over session state.

### Phase 4 - Remove duplicated board-fill logic and rigid board assumptions

1. Extract piece-selection logic into a shared `PieceFillPolicy` used by both `BoardGenerator` and `RefillResolver`.
2. Replace hardcoded column loops in presentation animation code with snapshot-driven column discovery.
3. Add tests that prove gravity and spawn logic work for columns outside the old fixed loop.

Acceptance criteria:

- fill rules live in one place.
- animation/runtime code no longer assumes a fixed `0..7` column loop.

### Phase 5 - Verify and lock the result

1. Run targeted architecture guard tests first.
2. Run the full test suite.
3. Build the desktop launcher.
4. Attempt platform builds where local tooling is available.

Acceptance criteria:

- architecture guard tests pass.
- full unit test suite passes.
- desktop build passes.

## Guard rails added during the refactor

- `ArchitectureRefactorTests` verifies project/package dependencies, runtime type placement, session score ownership, and snapshot-driven animation behavior.
