using GroveGames.DependencyInjection;
using UnityEngine;

namespace GroveGames.Scheduling.Unity
{
    internal sealed class SchedulerUpdater : IPreFrameUpdatable
    {
        private readonly Scheduler _scheduler;
        private double _realtime;

        public SchedulerUpdater(Scheduler scheduler)
        {
            _scheduler = scheduler;
            _realtime = Time.realtimeSinceStartupAsDouble;
        }

        public void PreFrameUpdate(float deltaTime)
        {
            var realtime = Time.realtimeSinceStartupAsDouble;
            var realtimeDeltaTime = (float)(realtime - _realtime);
            _realtime = realtime;
            _scheduler.Update(deltaTime, Time.unscaledDeltaTime, realtimeDeltaTime);
        }
    }
}
