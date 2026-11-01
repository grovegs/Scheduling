using System;
using System.Collections.Generic;

namespace GroveGames.Scheduling.Tests;

public sealed class SchedulerTests
{
    private sealed class TestScheduledAction : IScheduledAction
    {
        private int _invokeCount;

        public int InvokeCount => _invokeCount;

        public TestScheduledAction()
        {
            _invokeCount = 0;
        }

        public void Invoke()
        {
            _invokeCount++;
        }
    }

    [Fact]
    public void Schedule_NullCallback_ThrowsArgumentNullException()
    {
        var scheduler = new Scheduler();

        Assert.Throws<ArgumentNullException>(() => scheduler.Schedule((Action)null!, 1d));
    }

    [Fact]
    public void Schedule_NullAction_ThrowsArgumentNullException()
    {
        var scheduler = new Scheduler();

        Assert.Throws<ArgumentNullException>(() => scheduler.Schedule((IScheduledAction)null!, 1d));
    }

    [Fact]
    public void Update_BeforeDelay_DoesNotInvokeCallback()
    {
        var scheduler = new Scheduler();
        var invokeCount = 0;
        scheduler.Schedule(() => invokeCount++, 1d);

        scheduler.Update(0.5f, 0.5f, 0.5f);

        Assert.Equal(0, invokeCount);
    }

    [Fact]
    public void Update_DelayReached_InvokesCallbackOnce()
    {
        var scheduler = new Scheduler();
        var invokeCount = 0;
        scheduler.Schedule(() => invokeCount++, 1d);

        scheduler.Update(0.5f, 0.5f, 0.5f);
        scheduler.Update(0.5f, 0.5f, 0.5f);
        scheduler.Update(0.5f, 0.5f, 0.5f);

        Assert.Equal(1, invokeCount);
    }

    [Fact]
    public void Update_DifferentDelays_InvokesInTimeOrder()
    {
        var scheduler = new Scheduler();
        var order = new List<int>();
        scheduler.Schedule(() => order.Add(3), 3d);
        scheduler.Schedule(() => order.Add(1), 1d);
        scheduler.Schedule(() => order.Add(2), 2d);

        scheduler.Update(5f, 5f, 5f);

        Assert.Equal(new[] { 1, 2, 3 }, order);
    }

    [Fact]
    public void Update_SameTime_InvokesInScheduleOrder()
    {
        var scheduler = new Scheduler();
        var order = new List<int>();

        for (var i = 0; i < 100; i++)
        {
            var value = i;
            scheduler.Schedule(() => order.Add(value), 1d);
        }

        scheduler.Update(1f, 1f, 1f);

        Assert.Equal(100, order.Count);

        for (var i = 0; i < order.Count; i++)
        {
            Assert.Equal(i, order[i]);
        }
    }

    [Theory]
    [InlineData(TimeMode.Scaled, 1f, 0f, 0f)]
    [InlineData(TimeMode.Unscaled, 0f, 1f, 0f)]
    [InlineData(TimeMode.Realtime, 0f, 0f, 1f)]
    public void Update_TimeMode_AdvancesOnlyItsOwnClock(TimeMode timeMode, float deltaTime, float unscaledDeltaTime, float realtimeDeltaTime)
    {
        var scheduler = new Scheduler();
        var invokedModes = new List<TimeMode>();
        scheduler.Schedule(() => invokedModes.Add(TimeMode.Scaled), 1d, null, TimeMode.Scaled);
        scheduler.Schedule(() => invokedModes.Add(TimeMode.Unscaled), 1d, null, TimeMode.Unscaled);
        scheduler.Schedule(() => invokedModes.Add(TimeMode.Realtime), 1d, null, TimeMode.Realtime);

        scheduler.Update(deltaTime, unscaledDeltaTime, realtimeDeltaTime);

        Assert.Equal(new[] { timeMode }, invokedModes);
    }

    [Fact]
    public void Schedule_AfterTimeAdvanced_CountsDelayFromCurrentTime()
    {
        var scheduler = new Scheduler();
        var invokeCount = 0;
        scheduler.Update(5f, 5f, 5f);
        scheduler.Schedule(() => invokeCount++, 1d);

        scheduler.Update(0.5f, 0.5f, 0.5f);
        var countBeforeDelay = invokeCount;
        scheduler.Update(0.5f, 0.5f, 0.5f);

        Assert.Equal(0, countBeforeDelay);
        Assert.Equal(1, invokeCount);
    }

    [Fact]
    public void Schedule_NegativeDelay_InvokesOnNextUpdate()
    {
        var scheduler = new Scheduler();
        var invokeCount = 0;
        scheduler.Schedule(() => invokeCount++, -1d);

        scheduler.Update(0f, 0f, 0f);

        Assert.Equal(1, invokeCount);
    }

    [Fact]
    public void Schedule_FromCallback_WaitsForNextUpdate()
    {
        var scheduler = new Scheduler();
        var invokeCount = 0;
        scheduler.Schedule(() => scheduler.Schedule(() => invokeCount++, 0d), 0d);

        scheduler.Update(0f, 0f, 0f);
        var countAfterFirstUpdate = invokeCount;
        scheduler.Update(0f, 0f, 0f);

        Assert.Equal(0, countAfterFirstUpdate);
        Assert.Equal(1, invokeCount);
    }

    [Fact]
    public void Update_CanceledToken_SkipsCallback()
    {
        var scheduler = new Scheduler();
        var token = new ScheduleToken();
        var invokeCount = 0;
        scheduler.Schedule(() => invokeCount++, 1d, token);
        token.Cancel();

        scheduler.Update(1f, 1f, 1f);

        Assert.Equal(0, invokeCount);
    }

    [Fact]
    public void Update_TokenResetAfterCancel_RunsOnlyNewCallbacks()
    {
        var scheduler = new Scheduler();
        var token = new ScheduleToken();
        var order = new List<int>();
        scheduler.Schedule(() => order.Add(1), 1d, token);
        token.Cancel();
        token.Reset();
        scheduler.Schedule(() => order.Add(2), 1d, token);

        scheduler.Update(1f, 1f, 1f);

        Assert.Equal(new[] { 2 }, order);
    }

    [Fact]
    public void Update_CanceledCallbackFirst_StillInvokesLaterCallbacks()
    {
        var scheduler = new Scheduler();
        var token = new ScheduleToken();
        var invokeCount = 0;
        scheduler.Schedule(() => invokeCount += 10, 0.5d, token);
        scheduler.Schedule(() => invokeCount++, 1d);
        token.Cancel();

        scheduler.Update(1f, 1f, 1f);

        Assert.Equal(1, invokeCount);
    }

    [Fact]
    public void Schedule_ScheduledAction_InvokesActionWhenDue()
    {
        var scheduler = new Scheduler();
        var action = new TestScheduledAction();
        scheduler.Schedule(action, 1d);

        scheduler.Update(1f, 1f, 1f);

        Assert.Equal(1, action.InvokeCount);
    }

    [Fact]
    public void Schedule_MoreThanInitialCapacity_InvokesAll()
    {
        var scheduler = new Scheduler();
        var invokeCount = 0;

        for (var i = 0; i < 1000; i++)
        {
            scheduler.Schedule(() => invokeCount++, i % 7);
        }

        scheduler.Update(7f, 7f, 7f);

        Assert.Equal(1000, invokeCount);
    }
}
