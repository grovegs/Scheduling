using System;

namespace GroveGames.Scheduling;

public sealed class ConcurrentScheduler : IScheduler
{
    private const int InitialCapacity = 64;

    private readonly Scheduler _scheduler;
    private readonly object _lock;
    private PendingCall[] _pending;
    private PendingCall[] _draining;
    private int _pendingCount;

    public ConcurrentScheduler()
    {
        _scheduler = new Scheduler();
        _lock = new object();
        _pending = new PendingCall[InitialCapacity];
        _draining = new PendingCall[InitialCapacity];
        _pendingCount = 0;
    }

    public void Schedule(Action callback, double delay, ScheduleToken? token = null, TimeMode timeMode = TimeMode.Scaled)
    {
        ArgumentNullException.ThrowIfNull(callback);
        Add(new PendingCall(callback, null, delay, token, timeMode));
    }

    public void Schedule(IScheduledAction action, double delay, ScheduleToken? token = null, TimeMode timeMode = TimeMode.Scaled)
    {
        ArgumentNullException.ThrowIfNull(action);
        Add(new PendingCall(null, action, delay, token, timeMode));
    }

    public void Update(float deltaTime, float unscaledDeltaTime, float realtimeDeltaTime)
    {
        int count;

        lock (_lock)
        {
            (_pending, _draining) = (_draining, _pending);
            count = _pendingCount;
            _pendingCount = 0;
        }

        for (var i = 0; i < count; i++)
        {
            var call = _draining[i];
            _draining[i] = default;

            if (call.Token != null && call.Token.IsCanceledSince(call.TokenVersion))
            {
                continue;
            }

            if (call.Callback != null)
            {
                _scheduler.Schedule(call.Callback, call.Delay, call.Token, call.TimeMode);
            }
            else
            {
                _scheduler.Schedule(call.Action!, call.Delay, call.Token, call.TimeMode);
            }
        }

        _scheduler.Update(deltaTime, unscaledDeltaTime, realtimeDeltaTime);
    }

    private void Add(in PendingCall call)
    {
        if (double.IsNaN(call.Delay))
        {
            throw new ArgumentOutOfRangeException("delay", call.Delay, "The delay must be a number.");
        }

        if (call.Token != null && call.Token.IsCanceled)
        {
            return;
        }

        lock (_lock)
        {
            if (_pendingCount == _pending.Length)
            {
                Array.Resize(ref _pending, _pending.Length << 1);
            }

            _pending[_pendingCount++] = call;
        }
    }

    private readonly struct PendingCall
    {
        private readonly Action? _callback;
        private readonly IScheduledAction? _action;
        private readonly double _delay;
        private readonly ScheduleToken? _token;
        private readonly int _tokenVersion;
        private readonly TimeMode _timeMode;

        public Action? Callback => _callback;

        public IScheduledAction? Action => _action;

        public double Delay => _delay;

        public ScheduleToken? Token => _token;

        public int TokenVersion => _tokenVersion;

        public TimeMode TimeMode => _timeMode;

        public PendingCall(Action? callback, IScheduledAction? action, double delay, ScheduleToken? token, TimeMode timeMode)
        {
            _callback = callback;
            _action = action;
            _delay = delay;
            _token = token;
            _tokenVersion = token != null ? token.Version : 0;
            _timeMode = timeMode;
        }
    }
}
