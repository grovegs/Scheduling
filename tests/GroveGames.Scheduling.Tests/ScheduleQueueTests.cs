using System.Collections.Generic;

namespace GroveGames.Scheduling.Tests;

public sealed class ScheduleQueueTests
{
    [Fact]
    public void Pop_PushedOutOfOrder_ReturnsEarliestTimeFirst()
    {
        var queue = new ScheduleQueue(2);
        var times = new[] { 5d, 1d, 4d, 2d, 3d, 0d };

        for (var i = 0; i < times.Length; i++)
        {
            queue.Push(new ScheduledCall(static () => { }, null, times[i], i, null));
        }

        var popped = new List<double>();

        while (queue.Count > 0)
        {
            popped.Add(queue.Pop().Time);
        }

        Assert.Equal(new[] { 0d, 1d, 2d, 3d, 4d, 5d }, popped);
    }

    [Fact]
    public void Pop_SameTime_ReturnsLowestOrderFirst()
    {
        var queue = new ScheduleQueue(4);
        queue.Push(new ScheduledCall(static () => { }, null, 1d, 2, null));
        queue.Push(new ScheduledCall(static () => { }, null, 1d, 0, null));
        queue.Push(new ScheduledCall(static () => { }, null, 1d, 1, null));

        var first = queue.Pop();
        var second = queue.Pop();
        var third = queue.Pop();

        Assert.Equal(0, first.Order);
        Assert.Equal(1, second.Order);
        Assert.Equal(2, third.Order);
    }

    [Fact]
    public void Peek_AfterPush_ReturnsEarliestWithoutRemoving()
    {
        var queue = new ScheduleQueue(4);
        queue.Push(new ScheduledCall(static () => { }, null, 2d, 0, null));
        queue.Push(new ScheduledCall(static () => { }, null, 1d, 1, null));

        var time = queue.Peek().Time;

        Assert.Equal(1d, time);
        Assert.Equal(2, queue.Count);
    }

    [Fact]
    public void Push_FullWithCanceledCalls_DropsThemInsteadOfGrowing()
    {
        var queue = new ScheduleQueue(4);
        var token = new ScheduleToken();
        queue.Push(new ScheduledCall(static () => { }, null, 3d, 0, token));
        queue.Push(new ScheduledCall(static () => { }, null, 1d, 1, null));
        queue.Push(new ScheduledCall(static () => { }, null, 2d, 2, token));
        queue.Push(new ScheduledCall(static () => { }, null, 0d, 3, null));
        token.Cancel();

        queue.Push(new ScheduledCall(static () => { }, null, 0.5d, 4, null));

        var popped = new List<double>();

        while (queue.Count > 0)
        {
            popped.Add(queue.Pop().Time);
        }

        Assert.Equal(new[] { 0d, 0.5d, 1d }, popped);
        Assert.Equal(4, queue.Capacity);
    }

    [Fact]
    public void Push_FullWithoutCanceledCalls_KeepsEveryCallInOrder()
    {
        var queue = new ScheduleQueue(2);

        for (var i = 0; i < 9; i++)
        {
            queue.Push(new ScheduledCall(static () => { }, null, (i * 7) % 9, i, null));
        }

        var popped = new List<double>();

        while (queue.Count > 0)
        {
            popped.Add(queue.Pop().Time);
        }

        Assert.Equal(new[] { 0d, 1d, 2d, 3d, 4d, 5d, 6d, 7d, 8d }, popped);
    }

    [Fact]
    public void Push_FullAndMostlyLiveAfterDroppingCanceled_Grows()
    {
        var queue = new ScheduleQueue(4);
        var token = new ScheduleToken();
        queue.Push(new ScheduledCall(static () => { }, null, 1d, 0, null));
        queue.Push(new ScheduledCall(static () => { }, null, 2d, 1, null));
        queue.Push(new ScheduledCall(static () => { }, null, 3d, 2, null));
        queue.Push(new ScheduledCall(static () => { }, null, 4d, 3, token));
        token.Cancel();
        token.Reset();

        queue.Push(new ScheduledCall(static () => { }, null, 5d, 4, token));

        Assert.Equal(8, queue.Capacity);
        Assert.Equal(4, queue.Count);
    }

    [Fact]
    public void Push_ManyCanceledAmongLive_KeepsLiveCallsInOrder()
    {
        var queue = new ScheduleQueue(64);
        var token = new ScheduleToken();

        for (var i = 0; i < 64; i++)
        {
            queue.Push(new ScheduledCall(static () => { }, null, (i * 37) % 64, i, i % 3 == 0 ? token : null));
        }

        token.Cancel();
        queue.Push(new ScheduledCall(static () => { }, null, 100d, 64, null));

        var popped = new List<double>();

        while (queue.Count > 0)
        {
            popped.Add(queue.Pop().Time);
        }

        var expected = new List<double>();

        for (var i = 0; i < 64; i++)
        {
            if (i % 3 != 0)
            {
                expected.Add((i * 37) % 64);
            }
        }

        expected.Sort();
        expected.Add(100d);
        Assert.Equal(expected, popped);
    }
}
