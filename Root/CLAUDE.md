# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

```bash
# Build entire solution
dotnet build Janthus.sln

# Build a single project
dotnet build src/Janthus.Model/Janthus.Model.csproj

# Run all tests
dotnet test

# Run a single test by name
dotnet test --filter "FullyQualifiedName~Janthus.Model.Tests.LeveledActorTests.RandomRoll_CreatesNpcWithCorrectAttributeSum"

# Run the game
dotnet run --project src/Janthus.Game/Janthus.Game.csproj

# Restore MonoGame content pipeline tool (needed once after clone)
dotnet tool restore
```

## Architecture

.NET 8 solution with strict layered dependencies: **Model** ← **Data** ← **Game**

### Janthus.Model (domain layer, zero external dependencies)
Pure domain logic. Entities live in `Janthus.Model.Entities`, with this hierarchy:
- `JanthusObject` (base: Id, InternalId, Name, Description) → `Actor` → `LeveledActor` → `PlayerCharacter` / `NonPlayerCharacter`
- `Effect` → `Attack`; standalone: `CharacterAttribute`, `CharacterClass`, `ActorType`, `Item`, `Operation` → `CraftOperation`, etc.
- `ActorLevel` and `Skill` are NOT `JanthusObject` subclasses

`LeveledActor` has 7 attributes (Constitution, Dexterity, Intelligence, Luck, Attunement, Strength, Willpower) and a random-roll constructor that distributes points according to `CharacterClass` weights.

Key services (static classes): `CharacterCalculator` (HP/Mana formulas), `AdversaryCalculator` (alignment-based hostility with deterministic seeded randomness).

`IGameDataProvider` (`Janthus.Model.Services`) is the sole interface boundary between Model and Data.

### Janthus.Data (persistence layer)
SQLite via EF Core 8. `JanthusDbContext` manages 5 DbSets (ActorTypes, CharacterClasses, ActorLevels, SkillTypes, SkillLevels). `GameDataRepository` implements `IGameDataProvider` with lazy `??=` caching of all collections.

Seed data in `SeedData.cs` populates 5 classes, 20 levels (7 pts/level), 5 actor types, 5 skill types, 5 skill levels via `HasData()`.

DB file: `janthus_data.db` in working directory.

### Janthus.Game (MonoGame DesktopGL)
Stack-based `GameStateManager` with states: `MenuState`, `OptionsState`, `PlayingState` (active), `PausedState`/`CombatState` (stubs).

All graphics are programmatic — no content pipeline assets. Font is a runtime-built bitmap (`BuildRuntimeFont()`). Isometric rendering uses 64x32 tiles drawn as pixel-scanline diamonds. Actors are colored rectangles depth-sorted by `TileX + TileY`.

Key subsystems in `PlayingState`: `TileMap` + `IsometricRenderer` + `Camera` (lerp follow, zoom), `PlayerController` (WASD + click-to-move with A* pathfinding), `NpcController` (random wander), `UIManager` (HUD, CharacterPanel, PauseMenu, Dialog, ContextMenu).

Settings saved to `%APPDATA%\Janthus\settings.json`.

### Tests (xUnit)
13 tests in `Janthus.Model.Tests`: `CharacterCalculatorTests` (4, pure math) and `LeveledActorTests` (9, uses in-memory SQLite). Tests reference both Model and Data projects.

## Conventions

- **Nullable disabled** across all projects — no nullability annotations
- **File-scoped namespaces** everywhere, mirroring directory structure (e.g., `Janthus.Game.World`)
- **Manual DI** — no container; `IGameDataProvider` injected via constructors
- **Lazy init** via `??=` pattern (entity attributes, repository caches)
- **Static service classes** for pure logic (`CharacterCalculator`, `AdversaryCalculator`, `Pathfinder`)
- **Deterministic seeds** — map generation uses `Random(42)`, adversary checks use seeded hashes
- Renamed from legacy: `Attribute` → `CharacterAttribute`, `Class` → `CharacterClass`
- Enums are standalone files in `Enums/` (not nested in a wrapper class)
