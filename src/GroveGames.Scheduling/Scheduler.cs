using System;

namespace GroveGames.Scheduling;

public sealed class Scheduler : IScheduler
{
    private const int InitialCapacity = 64;

    private readonly ScheduleQueue _scaledQueue;
    private readonly ScheduleQueue _unscaledQueue;
    private readonly ScheduleQueue _realtimeQueue;
    private double _scaledTime;
    private double _unscaledTime;
    private double _realtime;
    private long _nextOrder;

    public Scheduler()
    {
        _scaledQueue = new ScheduleQueue(InitialCapacity);
        _unscaledQueue = new ScheduleQueue(InitialCapacity);
        _realtimeQueue = new ScheduleQueue(InitialCapacity);
        _scaledTime = 0d;
        _unscaledTime = 0d;
        _realtime = 0d;
        _nextOrder = 0;
    }

    public void Schedule(Action callback, double delay, ScheduleToken? token = null, TimeMode timeMode = TimeMode.Scaled)
    {
        ArgumentNullException.ThrowIfNull(callback);
        Enqueue(callback, null, delay, token, timeMode);
    }

    public void Schedule(IScheduledAction action, double delay, ScheduleToken? token = null, TimeMode timeMode = TimeMode.Scaled)
    {
        ArgumentNullException.ThrowIfNull(action);
        Enqueue(null, action, delay, token, timeMode);
    }

    public void Update(float deltaTime, float unscaledDeltaTime, float realtimeDeltaTime)
    {
        _scaledTime += deltaTime;
        _unscaledTime += unscaledDeltaTime;
        _realtime += realtimeDeltaTime;

        var orderLimit = _nextOrder;
        Process(_scaledQueue, _scaledTime, orderLimit);
        Process(_unscaledQueue, _unscaledTime, orderLimit);
        Process(_realtimeQueue, _realtime, orderLimit);
    }

    private void Enqueue(Action? callback, IScheduledAction? action, double delay, ScheduleToken? token, TimeMode timeMode)
    {
        var time = GetTime(timeMode) + Math.Max(delay, 0d);
        var call = new ScheduledCall(callback, action, time, _nextOrder++, token);
        GetQueue(timeMode).Push(in call);
    }

    private static void Process(ScheduleQueue queue, double time, long orderLimit)
    {
        while (queue.Count > 0)
        {
            ref readonly var next = ref queue.Peek();

            if (!next.IsCanceled && (next.Time > time || next.Order >= orderLimit))
            {
                break;
            }

            var call = queue.Pop();

            if (!call.IsCanceled)
            {
                call.Invoke();
            }
        }
    }

    private ScheduleQueue GetQueue(TimeMode timeMode)
    {
        return timeMode switch
        {
            TimeMode.Unscaled => _unscaledQueue,
            TimeMode.Realtime => _realtimeQueue,
            _ => _scaledQueue
        };
    }

    private double GetTime(TimeMode timeMode)
    {
        return timeMode switch
        {
            TimeMode.Unscaled => _unscaledTime,
            TimeMode.Realtime => _realtime,
            _ => _scaledTime
        };
    }
}
