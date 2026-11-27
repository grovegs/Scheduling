# GroveGames.Scheduling

Zero-allocation delayed callbacks with scaled, unscaled and realtime clocks for .NET and Unity.

[![NuGet](https://img.shields.io/nuget/v/GroveGames.Scheduling)](https://www.nuget.org/packages/GroveGames.Scheduling)
[![Build Status](https://github.com/grovegs/Scheduling/actions/workflows/release.yml/badge.svg)](https://github.com/grovegs/Scheduling/actions/workflows/release.yml)
[![Tests](https://github.com/grovegs/Scheduling/actions/workflows/tests.yml/badge.svg)](https://github.com/grovegs/Scheduling/actions/workflows/tests.yml)
[![Latest Release](https://img.shields.io/github/v/release/grovegs/Scheduling)](https://github.com/grovegs/Scheduling/releases/latest)

---

## Features

- **Core**: `IScheduler` and `Scheduler` in the `GroveGames.Scheduling` NuGet package run an `Action` or an `IScheduledAction` after a delay. Calls are kept in a binary heap per clock, so scheduling allocates nothing until the heap grows.
- **Three clocks**: `TimeMode.Scaled`, `TimeMode.Unscaled` and `TimeMode.Realtime` each advance from the deltas passed to `Update`, so pausing the game only stops the scaled clock.
- **Order**: Calls due at the same time run in the order they were scheduled. A call scheduled from inside a callback waits for the next `Update`, even with a zero delay. A negative delay counts as zero, an infinite one never runs, and `NaN` throws `ArgumentOutOfRangeException`.
- **Cancellation**: `ScheduleToken.Cancel()` drops every call scheduled with that token. `Reset()` lets new calls run while the canceled ones stay dropped. A call scheduled with a token that is already canceled is dropped too. Canceled calls are removed from the heap before it grows, so canceling and rescheduling does not grow it.
- **Errors**: A callback that throws does not stop the others. `Update` runs every due call on all three clocks, then throws an `AggregateException` with what went wrong.
- **Threads**: `Scheduler` is used from one thread, the one that calls `Update`. `ConcurrentScheduler` accepts `Schedule` from any thread and runs every callback on the thread that calls `Update`, which must always be the same one, so background work can hand results back to the main thread. A call scheduled on it is taken at the start of the next `Update`, and its delay counts from there. Both allocate nothing per call once warmed up. Cancel tokens on the updating thread.
- **Dependency injection**: `AddScheduler()` or `AddConcurrentScheduler()` registers `IScheduler` with [GroveGames.DependencyInjection](https://github.com/grovegs/DependencyInjection) and updates it from Unity's time every frame. Each exception a callback throws is logged on its own with `Debug.LogException`.

## Installation

### .NET

```bash
dotnet add package GroveGames.Scheduling
```

### Unity

Install the core through [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) (`GroveGames.Scheduling`), then add the package to `Packages/manifest.json`:

```json
{
    "dependencies": {
        "com.grovegames.scheduling": "https://github.com/grovegs/Scheduling.git?path=src/GroveGames.Scheduling.Unity/Packages/com.grovegames.scheduling"
    }
}
```

The core uses C# 10, so add a `csc.rsp` to `Assets/`:

```text
-langversion:10
-nullable:enable
```

## Usage

Schedule a callback and advance the scheduler once per frame with the frame's deltas:

```csharp
var scheduler = new Scheduler();

scheduler.Schedule(SpawnWave, 2d);
scheduler.Schedule(ShowPauseHint, 5d, timeMode: TimeMode.Unscaled);

scheduler.Update(deltaTime, unscaledDeltaTime, realtimeDeltaTime);
```

Cancel a group of calls with a token:

```csharp
var token = new ScheduleToken();

scheduler.Schedule(Explode, 1.5d, token);

token.Cancel();
```

Hand work from a background thread back to the main thread:

```csharp
var scheduler = new ConcurrentScheduler();

Task.Run(() =>
{
    var result = LoadLevel();
    scheduler.Schedule(() => Apply(result), 0d);
});

scheduler.Update(deltaTime, unscaledDeltaTime, realtimeDeltaTime);
```

With GroveGames.DependencyInjection installed, register one in an installer and inject `IScheduler`:

```csharp
builder.AddScheduler();
```

Use `builder.AddConcurrentScheduler()` instead when code schedules from other threads.

## Testing

```bash
dotnet test
```

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
