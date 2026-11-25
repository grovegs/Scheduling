using System;

namespace GroveGames.Scheduling;

internal readonly struct ScheduledCall
{
    private readonly object? _target;
    private readonly double _time;
    private readonly long _order;
    private readonly ScheduleToken? _token;
    private readonly int _tokenVersion;

    public double Time => _time;

    public long Order => _order;

    public bool IsCanceled => _token != null && _token.IsCanceledSince(_tokenVersion);

    public ScheduledCall(Action? callback, IScheduledAction? action, double time, long order, ScheduleToken? token)
    {
        _target = (object?)callback ?? action;
        _time = time;
        _order = order;
        _token = token;
        _tokenVersion = token != null ? token.Version : 0;
    }

    public void Invoke()
    {
        if (_target is Action callback)
        {
            callback();
            return;
        }

        if (_target is IScheduledAction action)
        {
            action.Invoke();
        }
    }
}
