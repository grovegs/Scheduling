using System;
using GroveGames.DependencyInjection;
using UnityEngine;

namespace GroveGames.Scheduling.Unity
{
    internal sealed class ConcurrentSchedulerUpdater : IPreFrameUpdatable
    {
        private readonly ConcurrentScheduler _scheduler;
        private double _realtime;

        public ConcurrentSchedulerUpdater(ConcurrentScheduler scheduler)
        {
            _scheduler = scheduler;
            _realtime = Time.realtimeSinceStartupAsDouble;
        }

        public void PreFrameUpdate(DeltaTime deltaTime)
        {
            var realtime = Time.realtimeSinceStartupAsDouble;
            var realtimeDeltaTime = (float)(realtime - _realtime);
            _realtime = realtime;

            try
            {
                _scheduler.Update(deltaTime.Scaled, deltaTime.Unscaled, realtimeDeltaTime);
            }
            catch (AggregateException exception)
            {
                foreach (var inner in exception.InnerExceptions)
                {
                    Debug.LogException(inner);
                }
            }
        }
    }
}
