using System;
using System.Threading;
using System.Threading.Tasks;

namespace GroveGames.Scheduling.Tests;

public sealed class ConcurrentSchedulerTests
{
    private sealed class FakeScheduledAction : IScheduledAction
    {
        private int _invokeCount;

        public int InvokeCount => _invokeCount;

        public FakeScheduledAction()
        {
            _invokeCount = 0;
        }

        public void Invoke()
        {
            _invokeCount++;
        }
    }

    [Fact]
    public void Schedule_FromManyThreads_RunsEveryCallOnceOnUpdate()
    {
        var scheduler = new ConcurrentScheduler();
        var invokeCount = 0;
        Action callback = () => invokeCount++;

        Parallel.For(0, 10000, _ => scheduler.Schedule(callback, 0d));
        scheduler.Update(0f, 0f, 0f);

        Assert.Equal(10000, invokeCount);
    }

    [Fact]
    public void Schedule_WhileAnotherThreadUpdates_LosesNoCall()
    {
        var scheduler = new ConcurrentScheduler();
        var invokeCount = 0;
        Action callback = () => invokeCount++;
        using var done = new ManualResetEventSlim();
        var producer = new Thread(() =>
        {
            for (var i = 0; i < 20000; i++)
            {
                scheduler.Schedule(callback, 0d);
            }

            done.Set();
        });
        producer.Start();

        while (!done.IsSet)
        {
            scheduler.Update(0.01f, 0.01f, 0.01f);
        }

        producer.Join();
        scheduler.Update(0.01f, 0.01f, 0.01f);

        Assert.Equal(20000, invokeCount);
    }

    [Fact]
    public void Update_DelayNotReached_CountsFromTheUpdateThatTakesTheCall()
    {
        var scheduler = new ConcurrentScheduler();
        var invokeCount = 0;
        scheduler.Update(5f, 5f, 5f);
        scheduler.Schedule(() => invokeCount++, 1d);

        scheduler.Update(0.5f, 0.5f, 0.5f);
        var before = invokeCount;
        scheduler.Update(0.5f, 0.5f, 0.5f);

        Assert.Equal(0, before);
        Assert.Equal(1, invokeCount);
    }

    [Fact]
    public void Schedule_InsideCallback_RunsOnTheNextUpdate()
    {
        var scheduler = new ConcurrentScheduler();
        var invokeCount = 0;
        scheduler.Schedule(() => scheduler.Schedule(() => invokeCount++, 0d), 0d);

        scheduler.Update(0f, 0f, 0f);
        var first = invokeCount;
        scheduler.Update(0f, 0f, 0f);

        Assert.Equal(0, first);
        Assert.Equal(1, invokeCount);
    }

    [Fact]
    public void Schedule_TokenCanceledBeforeUpdate_DropsTheCall()
    {
        var scheduler = new ConcurrentScheduler();
        var token = new ScheduleToken();
        var action = new FakeScheduledAction();
        scheduler.Schedule(action, 0d, token);

        token.Cancel();
        scheduler.Update(0f, 0f, 0f);

        Assert.Equal(0, action.InvokeCount);
    }

    [Fact]
    public void Schedule_NaNDelay_ThrowsOnTheCallingThread()
    {
        var scheduler = new ConcurrentScheduler();

        Assert.Throws<ArgumentOutOfRangeException>(() => scheduler.Schedule(() => { }, double.NaN));
    }

    [Fact]
    public void Schedule_NullCallback_ThrowsArgumentNullException()
    {
        var scheduler = new ConcurrentScheduler();

        Assert.Throws<ArgumentNullException>(() => scheduler.Schedule((Action)null!, 1d));
    }

    [Fact]
    public void Update_AfterWarmUp_DoesNotAllocate()
    {
        var scheduler = new ConcurrentScheduler();
        var action = new FakeScheduledAction();

        for (var i = 0; i < 100; i++)
        {
            scheduler.Schedule(action, 0d);
            scheduler.Update(0.01f, 0.01f, 0.01f);
        }

        var before = GC.GetAllocatedBytesForCurrentThread();

        for (var i = 0; i < 1000; i++)
        {
            scheduler.Schedule(action, 0d);
            scheduler.Update(0.01f, 0.01f, 0.01f);
        }

        Assert.Equal(0L, GC.GetAllocatedBytesForCurrentThread() - before);
        Assert.Equal(1100, action.InvokeCount);
    }
}
