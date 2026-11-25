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
}
