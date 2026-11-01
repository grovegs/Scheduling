# CLAUDE.md

## Package

- The core is the `GroveGames.Scheduling` NuGet package at `src/GroveGames.Scheduling` (`net10.0`, `net9.0`, `net8.0`, `netstandard2.1`). It holds `IScheduler`, `Scheduler`, `IScheduledAction`, `ScheduleToken` and `TimeMode`. `ScheduleQueue` is a binary heap of `ScheduledCall` structs ordered by time, then by schedule order. Tests are in `tests/GroveGames.Scheduling.Tests` (xUnit v3).
- `Scheduler` keeps its own scaled, unscaled and realtime clocks. They start at zero and advance only from `Update(deltaTime, unscaledDeltaTime, realtimeDeltaTime)`. A delay counts from the clock's current time. Calls scheduled during an `Update` wait for the next one.
- The Unity package at `src/GroveGames.Scheduling.Unity/Packages/com.grovegames.scheduling` references `GroveGames.Scheduling.dll`, installed with NuGetForUnity. There is no Godot addon.
- The GroveGames.DependencyInjection integration in `Integrations/GroveGames.DependencyInjection` compiles only when that package is installed. It adds `AddScheduler()`, and `SchedulerUpdater` feeds Unity's deltas to `Update` before each frame.
- GroveGames.Tween builds on `ScheduleToken` and `TimeMode`, so changes to them are breaking for Tween too.

## Code Rules

- No comments.
- Unity code uses block namespaces and C# 10.
- Every file and folder in the package needs a `.meta` with a 32-character lowercase hex GUID. Existing asmdef GUIDs must not change, because projects reference them.
- Tests are named `MethodName_Condition_Expected`, with arrange, act and assert separated by blank lines.
