# CLAUDE.md

## Package

- The core is the `GroveGames.Scheduling` NuGet package at `src/GroveGames.Scheduling` (`net10.0`, `net9.0`, `net8.0`, `netstandard2.1`). It holds `IScheduler`, `Scheduler`, `ConcurrentScheduler`, `IScheduledAction`, `ScheduleToken` and `TimeMode`. `ScheduleQueue` is a binary heap of `ScheduledCall` structs ordered by time, then by schedule order. Tests are in `tests/GroveGames.Scheduling.Tests` (xUnit v3).
- `Scheduler` keeps its own scaled, unscaled and realtime clocks. They start at zero and advance only from `Update(deltaTime, unscaledDeltaTime, realtimeDeltaTime)`. A delay counts from the clock's current time. Calls scheduled during an `Update` wait for the next one. `Scheduler` is single-threaded. `ConcurrentScheduler` wraps one: `Schedule` from any thread adds to a locked pending array, and `Update` swaps it with a second array and schedules its calls before updating, so nothing allocates after warm-up.
- The Unity package at `src/GroveGames.Scheduling.Unity/Packages/com.grovegames.scheduling` references `GroveGames.Scheduling.dll`, installed with NuGetForUnity. There is no Godot addon.
- The GroveGames.DependencyInjection integration in `Integrations/GroveGames.DependencyInjection` compiles only when that package is installed. It adds `AddScheduler()` and `AddConcurrentScheduler()`, and `SchedulerUpdater` and `ConcurrentSchedulerUpdater` feed Unity's deltas to `Update` before each frame.
- GroveGames.Tween builds on `ScheduleToken` and `TimeMode`, so changes to them are breaking for Tween too.

## Code Rules

- No comments.
- Unity code uses block namespaces and C# 10.
- Every file and folder in the package needs a `.meta` with a 32-character lowercase hex GUID. Existing asmdef GUIDs must not change, because projects reference them.
- Tests are named `MethodName_Condition_Expected`, with arrange, act and assert separated by blank lines.

## Agent Skills

- The rules for game code that uses this package are an agent skill. Its source is `skills/grovegames-scheduling/`: `SKILL.md` holds the rules that apply everywhere, `unity.md` holds only what differs in Unity. It is optional.
- The release writes the skill the Unity package ships, the same way it copies the README and the license: `SKILL.md` followed by `unity.md`, when there is one. Never edit the copy under `Skills~/`; the next release replaces it.
- The NuGet package ships `skills/grovegames-scheduling/SKILL.md` as it is.
