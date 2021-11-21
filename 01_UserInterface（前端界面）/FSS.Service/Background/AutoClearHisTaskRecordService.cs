using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FS.Core.LinkTrack;
using FS.DI;
using FS.Extends;
using FSS.Abstract.Server.MetaInfo;
using Microsoft.Extensions.Configuration;

namespace FSS.Service.Background
{
    /// <summary>
    /// 自动清除历史任务记录
    /// </summary>
    public class AutoClearHisTaskRecordService : LoopService
    {
        private readonly ITaskGroupList _taskGroupList;
        private readonly ITaskList      _taskList;
        private readonly int            _reservedTaskCount;
        public AutoClearHisTaskRecordService(IIocManager ioc)
        {
            _taskGroupList     = ioc.Resolve<ITaskGroupList>();
            _taskList          = ioc.Resolve<ITaskList>();
            _reservedTaskCount = ioc.Resolve<IConfigurationRoot>().GetSection("FSS:ReservedTaskCount").Value.ConvertType(20);
        }

        protected override TimeSpan SleepMs { get; set; } = TimeSpan.FromHours(1);
        protected override async Task ExecuteJobAsync(CancellationToken stoppingToken)
        {
            var lst = await _taskGroupList.ToListInCacheAsync();
            foreach (var taskGroupVO in lst)
            {
                var lstTask = await _taskList.ToSuccessListAsync(taskGroupVO.Id, _reservedTaskCount);
                var taskId  = lstTask.Min(o => o.Id);

                // 清除历史记录
                await _taskList.ClearSuccessAsync(taskGroupVO.Id, taskId);
                Thread.Sleep(1000);
            }
        }
    }
}