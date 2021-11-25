using System;
using System.Threading.Tasks;
using FS.Cache.Redis;
using FS.DI;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Tasks.Dal;
using FSS.Infrastructure.Repository;

namespace FSS.Com.MetaInfoServer.Tasks
{
    public class TaskAdd : ITaskAdd
    {
        public TaskCache      TaskCache     { get; set; }
        public ITaskGroupInfo TaskGroupInfo { get; set; }

        /// <summary>
        /// 将任务暂时写入redis集合，再通过job集中写入数据库
        /// </summary>
        public Task AddToQueueAsync(TaskVO task) => TaskCache.AddQueueAsync(task);

        /// <summary>
        /// 将任务暂时写入redis集合，再通过job集中写入数据库
        /// </summary>
        public async Task<int> AddToDbAsync(int top)
        {
            var lstTask = await TaskCache.GetFinishTaskListAsync(top);
            if (lstTask.Count == 0) return 0;
            var lstPO = lstTask.Map<TaskPO>();

            using (var db = new MetaInfoContext())
            {
                foreach (var taskPO in lstPO)
                {
                    taskPO.Id = null;
                    await db.Task.InsertAsync(taskPO);
                }
                db.SaveChanges();
            }

            return lstTask.Count;
        }

        /// <summary>
        /// 创建Task，并更新到缓存
        /// </summary>
        public async Task<TaskVO> GetOrCreateAsync(int taskGroupId)
        {
            var taskGroup = await TaskGroupInfo.ToInfoAsync(taskGroupId);
            return await GetOrCreateAsync(taskGroup);
        }

        /// <summary>
        /// 创建Task，并更新到缓存
        /// </summary>
        public Task<TaskVO> GetOrCreateAsync(TaskGroupVO taskGroup)
        {
            if (!taskGroup.IsEnable) return null;
            return CreateAsync(taskGroup);
        }

        /// <summary>
        /// 创建新的Task缓存
        /// </summary>
        private async Task<TaskVO> CreateAsync(TaskGroupVO taskGroup)
        {
            var key  = CacheKeys.TaskForGroupKey;
            var task = await RedisContext.Instance.CacheManager.GetItemAsync(key, taskGroup.Id);
            if (task != null && task.Status != EumTaskType.Fail && task.Status != EumTaskType.Success) return task;
            // 没查到时，自动创建一条对应的Task
            task = new TaskVO
            {
                TaskGroupId = taskGroup.Id,
                StartAt     = taskGroup.NextAt,
                Caption     = taskGroup.Caption,
                JobName     = taskGroup.JobName,
                RunSpeed    = 0,
                ClientId    = 0,
                ClientIp    = "",
                Progress    = 0,
                Status      = EumTaskType.None,
                CreateAt    = DateTime.Now,
                RunAt       = DateTime.Now,
                SchedulerAt = DateTime.Now,
            };
            await TaskCache.SaveAsync(task);
            return task;
        }
    }
}