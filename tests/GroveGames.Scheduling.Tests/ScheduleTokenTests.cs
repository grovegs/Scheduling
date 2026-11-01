namespace GroveGames.Scheduling.Tests;

public sealed class ScheduleTokenTests
{
    [Fact]
    public void Constructor_Default_IsNotCanceled()
    {
        var token = new ScheduleToken();

        Assert.False(token.IsCanceled);
        Assert.Equal(0, token.Version);
    }

    [Fact]
    public void Cancel_Called_SetsCanceledAndBumpsVersion()
    {
        var token = new ScheduleToken();

        token.Cancel();

        Assert.True(token.IsCanceled);
        Assert.Equal(1, token.Version);
    }

    [Fact]
    public void Reset_AfterCancel_ClearsCanceledAndKeepsVersion()
    {
        var token = new ScheduleToken();
        token.Cancel();

        token.Reset();

        Assert.False(token.IsCanceled);
        Assert.Equal(1, token.Version);
    }

    [Fact]
    public void IsCanceledSince_SameVersion_ReturnsFalse()
    {
        var token = new ScheduleToken();
        var version = token.Version;

        var isCanceled = token.IsCanceledSince(version);

        Assert.False(isCanceled);
    }

    [Fact]
    public void IsCanceledSince_CanceledAndReset_ReturnsTrue()
    {
        var token = new ScheduleToken();
        var version = token.Version;
        token.Cancel();
        token.Reset();

        var isCanceled = token.IsCanceledSince(version);

        Assert.True(isCanceled);
    }
}
