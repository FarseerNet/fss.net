using System.Threading.Tasks;
using FS.Core.Job;
using FS.Extends;
using FS.Job;
using FSS.Infrastructure.Repository.Tasks;
using Microsoft.Extensions.Logging;

namespace FSS.Infrastructure.Job
{
    /// <summary>
    /// 任务写入数据库
    /// </summary>
    [FssJob(Name = "FSS.AddTaskToDb")]
    public class AddTaskToDbJob : IFssJob
    {
        public TaskCache TaskCache { get; set; }
        public TaskAgent TaskAgent { get; set; }

        public async Task<bool> Execute(IFssContext context)
        {
            context.Meta.Data.TryGetValue("DataCount", out var top);
            var dataCount                = top.ConvertType(200);
            if (dataCount < 1) dataCount = 200;

            int result = 0;
            while (true)
            {
                var lstTask = await TaskCache.GetFinishTaskListAsync(dataCount);
                if (lstTask.Count == 0) return true;

                var count = await TaskAgent.AddToDbAsync(lstTask);
                result += count;
                if (count != dataCount) break;
            }
            await context.LoggerAsync(LogLevel.Information, $"写入{result}条数据");
            return true;
        }
    }
}