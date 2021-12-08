using System.Threading.Tasks;
using FS.Core.Job;
using FS.Extends;
using FS.Job;
using FS.Job.Entity;
using FSS.Abstract.Server.MetaInfo;
using FSS.Domain.Log.TaskLog.Interface;

namespace FSS.Service.Job
{
    /// <summary>
    /// 日志写入数据库
    /// </summary>
    [FssJob(Name = "FSS.AddRunLogToDb")]
    public class AddRunLogToDbJob : IFssJob
    {
        public ITaskLogService TaskLogService { get; set; }

        public async Task<bool> Execute(IFssContext context)
        {
            context.Meta.Data.TryGetValue("DataCount", out var top);
            var dataCount                = top.ConvertType(1000);
            if (dataCount < 1) dataCount = 1000;
            int result                   = 0;
            var progress                 = 10;
            while (true)
            {
                var count = await TaskLogService.SaveAsync(dataCount);
                result += count;
                if (count != dataCount) break;
                await context.SetProgressAsync(progress += 10);
            }

            return true;
        }
    }
}