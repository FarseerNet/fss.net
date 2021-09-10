using System;
using System.Threading.Tasks;
using FS.Cache.Redis;
using FS.DI;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;
using FSS.Com.MetaInfoServer.Tasks.Dal;
using Newtonsoft.Json;

namespace FSS.Com.MetaInfoServer.TaskGroup
{
    public class TaskGroupUpdate : ITaskGroupUpdate
    {
        public  ITaskGroupCache    TaskGroupCache    { get; set; }
        public  ITaskGroupAgent    TaskGroupAgent    { get; set; }
        private IRedisCacheManager RedisCacheManager => IocManager.Instance.Resolve<IRedisCacheManager>();

        /// <summary>
        /// 更新TaskGroup
        /// </summary>
        public Task UpdateAsync(TaskGroupVO vo) => TaskGroupCache.SaveAsync(vo.Id, vo);

        /// <summary>
        /// 移除缓存任务组ID
        /// </summary>
        public Task RemoveAsync(int taskGroupId)
        {
            return TaskGroupCache.RemoveAsync(taskGroupId);
        }

        /// <summary>
        /// 保存TaskGroup
        /// </summary>
        public async Task SaveAsync(TaskGroupVO vo)
        {
            await TaskGroupAgent.UpdateAsync(vo.Id, vo.Map<TaskGroupPO>());
            await UpdateAsync(vo);
        }

        /// <summary>
        /// 统计失败次数，按次数递增时间
        /// </summary>
        public async Task StatFailAsync(TaskVO task, TaskGroupVO taskGroupVO)
        {
            var failKey = TaskCache.FailKey(task.TaskGroupId);

            switch (task.Status)
            {
                case EumTaskType.Fail:
                case EumTaskType.ReScheduler:
                    // 失败时，要写入失败列表
                    await RedisCacheManager.Db.ListLeftPushAsync(failKey, JsonConvert.SerializeObject(task));

                    // 如果连接3次执行失败，则要把下次执行时间改为递增方式
                    var failCount = await RedisCacheManager.Db.ListLengthAsync(failKey);
                    if (failCount >= 4)
                    {
                        // 从第4次开始递增时间，每失败1次 + 5分钟
                        var count  = failCount - 3;
                        var nextAt = DateTime.Now.AddMinutes(count * 5);

                        // 当计划下次时间，比计算出来的时间要早（一般是立即就要执行），则强制更新下次时间为计算后的时间（延迟执行）
                        if (taskGroupVO.NextAt < nextAt)
                        {
                            taskGroupVO.NextAt = nextAt;
                        }
                    }

                    break;
                case EumTaskType.Success:
                    // 一旦恢复成功，则清除失败列表
                    await RedisCacheManager.Db.KeyDeleteAsync(failKey);
                    break;
            }
        }
    }
}