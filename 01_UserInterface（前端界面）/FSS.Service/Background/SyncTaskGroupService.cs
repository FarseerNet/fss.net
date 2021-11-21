using System;
using System.Threading;
using System.Threading.Tasks;
using FS.Core.LinkTrack;
using FS.DI;
using FSS.Abstract.Server.MetaInfo;
using FSS.Infrastructure.Repository;

namespace FSS.Service.Background
{
    /// <summary>
    /// 同步任务组信息数据库与缓存
    /// </summary>
    public class SyncTaskGroupService : LoopService
    {
        readonly ITaskGroupList _taskGroupList;
        readonly ITaskGroupInfo _taskGroupInfo;
        readonly ITaskInfo      _taskInfo;

        public SyncTaskGroupService(IIocManager ioc)
        {
            _taskGroupList = ioc.Resolve<ITaskGroupList>();
            _taskGroupInfo = ioc.Resolve<ITaskGroupInfo>();
            _taskInfo      = ioc.Resolve<ITaskInfo>();
        }

        protected override TimeSpan SleepMs { get; set; } = TimeSpan.FromMinutes(1);
        protected override async Task ExecuteJobAsync(CancellationToken stoppingToken)
        {
            // 数据库同步到缓存
            var lstGroupByDb = await _taskGroupList.ToListInDbAsync();
            foreach (var taskGroupVo in lstGroupByDb)
            {
                // 强制从缓存中再读一次，可以实现当缓存丢失时，可以重新写入该条任务组到缓存
                await _taskGroupInfo.ToInfoAsync(taskGroupVo.Id);
            }

            // 缓存对比数据库
            var lstGroupByCache = await _taskGroupList.ToListInMemoryAsync();
            foreach (var taskGroupVO in lstGroupByCache)
            {
                // 数据库中没有找到，则要删除掉缓存的任务组数据
                if (!lstGroupByDb.Exists(o => o.Id == taskGroupVO.Key))
                {
                    await CacheKeys.TaskGroupClear(taskGroupVO.Key);
                }
            }

            // 取出当前所有任务列表，判断所属任务组是否存在
            var lstTask = await _taskInfo.ToGroupListAsync();
            foreach (var taskVO in lstTask)
            {
                if (!lstGroupByDb.Exists(o => o.Id == taskVO.TaskGroupId))
                {
                    await CacheKeys.TaskForGroupClear(taskVO.TaskGroupId);
                }
            }
        }
    }
}