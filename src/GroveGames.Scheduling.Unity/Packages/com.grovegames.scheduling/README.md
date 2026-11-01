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
- **Order**: Calls due at the same time run in the order they were scheduled. A call scheduled from inside a callback waits for the next `Update`, even with a zero delay.
- **Cancellation**: `ScheduleToken.Cancel()` drops every call scheduled with that token. `Reset()` lets new calls run while the canceled ones stay dropped.
- **Dependency injection**: `AddScheduler()` registers `IScheduler` with [GroveGames.DependencyInjection](https://github.com/grovegs/DependencyInjection) and updates it from Unity's time every frame.

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

With GroveGames.DependencyInjection installed, register it in an installer and inject `IScheduler`:

```csharp
builder.AddScheduler();
```

## Testing

```bash
dotnet test
```

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
