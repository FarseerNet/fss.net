using System.Threading.Tasks;
using FS.Extends;
using FS.Job;
using FS.Job.Entity;
using FSS.Abstract.Server.MetaInfo;

namespace FSS.Service.Job
{
    /// <summary>
    /// 日志写入数据库
    /// </summary>
    [FssJob(Name = "FSS.AddRunLogToDb")]
    public class AddRunLogToDbJob : IFssJob
    {
        public IRunLogAdd RunLogAdd { get; set; }

        public async Task<bool> Execute(ReceiveContext context)
        {
            context.Meta.Data.TryGetValue("DataCount", out var top);
            var dataCount                = top.ConvertType(100);
            if (dataCount < 1) dataCount = 100;

            var result = await RunLogAdd.AddToDbAsync(dataCount);
            return true;
        }
    }
}