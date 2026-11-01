namespace GroveGames.Scheduling;

public sealed class ScheduleToken
{
    private bool _isCanceled;
    private int _version;

    public bool IsCanceled => _isCanceled;

    public int Version => _version;

    public ScheduleToken()
    {
        _isCanceled = false;
        _version = 0;
    }

    public void Cancel()
    {
        _isCanceled = true;
        _version++;
    }

    public void Reset()
    {
        _isCanceled = false;
    }

    public bool IsCanceledSince(int version)
    {
        return _isCanceled || _version != version;
    }
}
