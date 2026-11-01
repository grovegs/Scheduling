using System;

namespace GroveGames.Scheduling;

public interface IScheduler
{
    public void Schedule(Action callback, double delay, ScheduleToken? token = null, TimeMode timeMode = TimeMode.Scaled);

    public void Schedule(IScheduledAction action, double delay, ScheduleToken? token = null, TimeMode timeMode = TimeMode.Scaled);
}
