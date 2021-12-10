using System;
using System.Threading.Tasks;
using FS.Cache.Redis;
using FS.DI;
using FS.Extends;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Tasks.Dal;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Infrastructure.Repository;
using FSS.Infrastructure.Repository.Tasks.Enum;

namespace FSS.Com.MetaInfoServer.Tasks
{
    public class TaskAdd : ITaskAdd
    {
        public TaskCache TaskCache { get; set; }

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
    }
}