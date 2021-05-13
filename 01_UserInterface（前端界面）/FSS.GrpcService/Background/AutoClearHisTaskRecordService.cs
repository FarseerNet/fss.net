using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FS.Extends;
using FSS.Abstract.Server.MetaInfo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FSS.GrpcService.Background
{
    /// <summary>
    /// 自动清除历史任务记录
    /// </summary>
    public class AutoClearHisTaskRecordService : BackgroundService
    {
        private readonly IIocManager    _ioc;
        private readonly ITaskGroupList _taskGroupList;
        private readonly ITaskList      _taskList;
        readonly         ILogger        _logger;

        public AutoClearHisTaskRecordService(IIocManager ioc)
        {
            _ioc           = ioc;
            _logger        = _ioc.Logger<Startup>();
            _taskGroupList = _ioc.Resolve<ITaskGroupList>();
            _taskList      = _ioc.Resolve<ITaskList>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var reservedTaskCount = _ioc.Resolve<IConfigurationRoot>().GetSection("FSS:ReservedTaskCount").Value.ConvertType(20);
            while (true)
            {
                var lst = await _taskGroupList.ToListAsync();
                foreach (var taskGroupVO in lst)
                {
                    var lstTask = await _taskList.ToSuccessListAsync(taskGroupVO.Id, reservedTaskCount);
                    var taskId = lstTask.Min(o => o.Id);
                    
                    // 清除历史记录
                    await _taskList.ClearSuccessAsync(taskGroupVO.Id, taskId);
                }

                await Task.Delay(1000 * 60 * 60, stoppingToken); // 1个小时执行一次
            }
        }
    }
}