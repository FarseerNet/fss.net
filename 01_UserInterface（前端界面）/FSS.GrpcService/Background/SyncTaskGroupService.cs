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
        readonly         ITaskInfo        _taskInfo;
        readonly         ITaskUpdate      _taskUpdate;

        public SyncTaskGroupService(IIocManager ioc)
        {
            _ioc             = ioc;
            _taskGroupList   = _ioc.Resolve<ITaskGroupList>();
            _taskGroupInfo   = _ioc.Resolve<ITaskGroupInfo>();
            _taskGroupUpdate = _ioc.Resolve<ITaskGroupUpdate>();
            _taskInfo        = _ioc.Resolve<ITaskInfo>();
            _taskUpdate      = _ioc.Resolve<ITaskUpdate>();
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
                
                // 取出当前所有任务列表，判断所属任务组是否存在
                var lstTask = await _taskInfo.ToGroupListAsync();
                foreach (var taskVO in lstTask)
                {
                    if (!lstGroupByDb.Exists(o => o.Id == taskVO.TaskGroupId))
                    {
                        await _taskUpdate.RemoveAsync(taskVO.TaskGroupId);
                    }
                }
            }
        }
    }
}