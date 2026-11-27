---
name: grovegames-scheduling
description: Rules for delayed calls in game code with GroveGames.Scheduling. Use when writing or reviewing code that waits before doing something, repeats on a timer, or cancels pending work.
---

# Using GroveGames.Scheduling

Rules for game code that uses this package. They hold in every game. The API is in the package's `README.md`.

- Take `IScheduler` through the constructor for every delay. Do not count time in an update method and do not use engine timers, coroutines or `Task.Delay`.
- Pick the clock on purpose: `TimeMode.Scaled` for gameplay, `TimeMode.Unscaled` for UI and anything that must run while the game is paused, `TimeMode.Realtime` for wall-clock waits.
- Schedule with a `ScheduleToken` held in a field, and `Cancel` it in `Dispose`. A callback must never run on a disposed object.
- To reuse a token after `Cancel`, call `Reset` before scheduling again.
- Pass a cached delegate or an `IScheduledAction`, not a new lambda per call, on anything that runs often.
- A call scheduled from inside a callback runs on a later update, even with a zero delay. Do not rely on it running in the same frame.
- Schedule on `Scheduler` only from the thread that updates it. To hand work from a background thread back to the main thread, register `ConcurrentScheduler` and schedule on it from that thread with a zero delay.
