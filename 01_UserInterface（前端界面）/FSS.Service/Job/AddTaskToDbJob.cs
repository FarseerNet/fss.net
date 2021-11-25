using System;
using System.Linq;
using System.Threading.Tasks;
using FS.Extends;
using FS.Job;
using FS.Job.Entity;
using FSS.Abstract.Server.MetaInfo;
using Microsoft.Extensions.Logging;

namespace FSS.Service.Job
{
    /// <summary>
    /// 任务写入数据库
    /// </summary>
    [FssJob(Name = "FSS.AddTaskToDb")]
    public class AddTaskToDbJob : IFssJob
    {
        public ITaskAdd TaskAdd { get; set; }

        public async Task<bool> Execute(ReceiveContext context)
        {
            context.Meta.Data.TryGetValue("DataCount", out var top);
            var dataCount                = top.ConvertType(100);
            if (dataCount < 1) dataCount = 100;

            var result = await TaskAdd.AddToDbAsync(dataCount);
            await context.LoggerAsync(LogLevel.Information, $"写入{result}条数据");
            return true;
        }
    }
}