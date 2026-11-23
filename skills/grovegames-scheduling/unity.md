## Unity

- Register with `builder.AddScheduler()` in the root installer. It updates the scheduler from Unity's time every frame; never call `Update` on it yourself.
- An asmdef that overrides references lists `GroveGames.Scheduling.dll` in its precompiled references.
