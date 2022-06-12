namespace FSS.Domain.Tasks.TaskGroup.Entity;

/// <summary>
///     任务执行速度
/// </summary>
public class TaskSpeed
{
    /// <summary>
    ///     任务的所有执行速度
    /// </summary>
    private readonly IList<long> _speedList;

    public TaskSpeed(IList<long> speedList)
    {
        _speedList = speedList;
    }

    /// <summary>
    ///     任务的执行平均速度
    /// </summary>
    public long GetAvgSpeed()
    {
        if (_speedList.Count == 0) return 0;
        return _speedList.Sum() / _speedList.Count;
    }
}