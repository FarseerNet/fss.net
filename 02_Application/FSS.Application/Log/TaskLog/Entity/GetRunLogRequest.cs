using Microsoft.Extensions.Logging;

namespace FSS.Application.Log.TaskLog.Entity;

public class GetRunLogRequest
{
    public string    JobName   { get; set; }
    public LogLevel? LogLevel  { get; set; }
    public int       PageSize  { get; set; }
    public int       PageIndex { get; set; }
}