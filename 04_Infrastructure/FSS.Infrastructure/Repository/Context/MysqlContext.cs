using FS.Data;
using FSS.Infrastructure.Repository.Log.Model;
using FSS.Infrastructure.Repository.TaskGroup.Model;
using FSS.Infrastructure.Repository.Tasks.Model;

namespace FSS.Infrastructure.Repository.Context;

/// <summary>
///     元信息上下文
/// </summary>
public class MysqlContext : DbContext<MysqlContext>
{
    public MysqlContext() : base(name: "default")
    {
    }

    public TableSet<TaskLogPO>   RunLog    { get; set; }
    public TableSet<TaskPO>      Task      { get; set; }
    public TableSet<TaskGroupPO> TaskGroup { get; set; }

    protected override void CreateModelInit()
    {
        RunLog.SetName(tableName: "run_log");
        Task.SetName(tableName: "task");
        TaskGroup.SetName(tableName: "task_group");
    }
}