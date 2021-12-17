using System.Threading.Tasks;
using FS.Core.Job;
using FS.Extends;
using FS.Job;
using FSS.Infrastructure.Repository.Log;

namespace FSS.Infrastructure.Job
{
    /// <summary>
    /// 日志写入数据库
    /// </summary>
    [FssJob(Name = "FSS.AddRunLogToDb")]
    public class AddRunLogToDbJob : IFssJob
    {
        public LogAgent LogAgent { get; set; }
        public LogQueue LogQueue { get; set; }

        public async Task<bool> Execute(IFssContext context)
        {
            context.Meta.Data.TryGetValue("DataCount", out var top);
            var dataCount                = top.ConvertType(1000);
            if (dataCount < 1) dataCount = 1000;
            int result                   = 0;
            var progress                 = 10;

            while (true)
            {
                var lst = await LogQueue.GetQueueAsync(dataCount);
                if (lst == null || lst.Count == 0) return true;

                await LogAgent.AddAsync(lst);
                result += lst.Count;
                if (lst.Count != dataCount) break;
                context.SetProgress(progress += 10);
            }
            return true;
        }
    }
}