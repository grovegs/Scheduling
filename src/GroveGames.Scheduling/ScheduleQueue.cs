using System;

namespace GroveGames.Scheduling;

internal sealed class ScheduleQueue
{
    private ScheduledCall[] _items;
    private int _count;

    public int Count => _count;

    public ScheduleQueue(int capacity)
    {
        _items = new ScheduledCall[capacity];
        _count = 0;
    }

    public ref readonly ScheduledCall Peek()
    {
        return ref _items[0];
    }

    public void Push(in ScheduledCall call)
    {
        if (_count == _items.Length)
        {
            Array.Resize(ref _items, _count << 1);
        }

        var index = _count++;

        while (index > 0)
        {
            var parent = (index - 1) >> 1;

            if (!Precedes(in call, in _items[parent]))
            {
                break;
            }

            _items[index] = _items[parent];
            index = parent;
        }

        _items[index] = call;
    }

    public ScheduledCall Pop()
    {
        var top = _items[0];
        var last = _items[--_count];
        _items[_count] = default;

        if (_count == 0)
        {
            return top;
        }

        var index = 0;
        var half = _count >> 1;

        while (index < half)
        {
            var child = (index << 1) + 1;
            var right = child + 1;

            if (right < _count && Precedes(in _items[right], in _items[child]))
            {
                child = right;
            }

            if (!Precedes(in _items[child], in last))
            {
                break;
            }

            _items[index] = _items[child];
            index = child;
        }

        _items[index] = last;
        return top;
    }

    private static bool Precedes(in ScheduledCall left, in ScheduledCall right)
    {
        return left.Time < right.Time || (left.Time == right.Time && left.Order < right.Order);
    }
}
