using System;
using System.Threading;
using System.Threading.Tasks;
using FS.DI;
using FSS.Abstract.Server.MetaInfo;
using Microsoft.Extensions.Hosting;

namespace FSS.GrpcService.Background
{
    /// <summary>
    /// 同步任务组信息数据库与缓存
    /// </summary>
    public class SyncTaskGroupService : BackgroundService
    {
        private readonly IIocManager      _ioc;
        readonly         ITaskGroupList   _taskGroupList;
        readonly         ITaskGroupInfo   _taskGroupInfo;
        readonly         ITaskGroupUpdate _taskGroupUpdate;

        public SyncTaskGroupService(IIocManager ioc)
        {
            _ioc             = ioc;
            _taskGroupList   = _ioc.Resolve<ITaskGroupList>();
            _taskGroupInfo   = _ioc.Resolve<ITaskGroupInfo>();
            _taskGroupUpdate = _ioc.Resolve<ITaskGroupUpdate>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                // 数据库同步到缓存
                var lstGroupByDb    = await _taskGroupList.ToListByDbAsync();
                foreach (var taskGroupVo in lstGroupByDb)
                {
                    // 强制从缓存中再读一次，可以实现当缓存丢失时，可以重新写入该条任务组到缓存
                    await _taskGroupInfo.ToInfoAsync(taskGroupVo.Id);
                }
                
                // 缓存对比数据库
                var lstGroupByCache = await _taskGroupList.ToListByMemoryAsync();
                foreach (var taskGroupVO in lstGroupByCache)
                {
                    // 数据库中没有找到，则要删除掉缓存的任务组数据
                    if (!lstGroupByDb.Exists(o => o.Id == taskGroupVO.Key))
                    {
                        await _taskGroupUpdate.RemoveAsync(taskGroupVO.Key);
                    }
                }
            }
        }
    }
}