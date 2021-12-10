using System;
using System.Linq;
using System.Threading.Tasks;
using FS.Core.Job;
using FS.Extends;
using FS.Job;
using FS.Job.Entity;
using FSS.Abstract.Server.MetaInfo;
using FSS.Application.Tasks.Tasks.Interface;
using FSS.Infrastructure.Repository.Tasks.Interface;
using Microsoft.Extensions.Logging;

namespace FSS.Service.Job
{
    /// <summary>
    /// 任务写入数据库
    /// </summary>
    [FssJob(Name = "FSS.AddTaskToDb")]
    public class AddTaskToDbJob : IFssJob
    {
        public ITaskApp TaskApp { get; set; }

        public async Task<bool> Execute(IFssContext context)
        {
            context.Meta.Data.TryGetValue("DataCount", out var top);
            var dataCount                = top.ConvertType(200);
            if (dataCount < 1) dataCount = 200;

            int result = 0;
            while (true)
            {
                var count = await TaskApp.AddToDbAsync(dataCount);
                result += count;
                if (count != dataCount) break;
            }
            await context.LoggerAsync(LogLevel.Information, $"写入{result}条数据");
            return true;
        }
    }
}