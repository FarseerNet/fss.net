using FSS.Domain.Tasks.TaskGroup.Enum;

namespace FSS.Service.Request;

public class GetAllTaskListRequest
{
    /// <summary>
    ///     状态
    /// </summary>
    public EumTaskType? Status { get; set; }
    public int PageSize  { get;       set; }
    public int PageIndex { get;       set; }
}