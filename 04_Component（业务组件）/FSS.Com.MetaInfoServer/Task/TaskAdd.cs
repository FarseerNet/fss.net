using System;
using FS.Cache;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.Task.Dal;

namespace FSS.Com.MetaInfoServer.Task
{
    public class TaskAdd : ITaskAdd
    {
        public ITaskAgent       TaskAgent       { get; set; }
        public ICacheManager    CacheManager    { get; set; }
        public ITaskGroupInfo   TaskGroupInfo   { get; set; }
        public ITaskGroupUpdate TaskGroupUpdate { get; set; }

        /// <summary>
        /// 创建Task，并更新到缓存
        /// </summary>
        public TaskVO GetOrCreate(int taskGroupId)
        {
            var taskGroup = TaskGroupInfo.ToInfo(taskGroupId);
            var task      = TaskAgent.ToUnExecutedTask(taskGroup.Id);
            if (task == null)
            {
                // 没查到时，自动创建一条对应的Task
                task = new TaskPO
                {
                    TaskGroupId = taskGroup.Id,
                    StartAt     = taskGroup.NextAt,
                    RunSpeed    = 0,
                    ClientHost  = "",
                    ClientIp    = "",
                    Progress    = 0,
                    Status      = EumTaskType.None,
                    CreateAt    = DateTime.Now,
                    ServerNode  = ""
                };
                TaskAgent.Add(task, out int id);
                task.Id = id;
                TaskGroupUpdate.UpdateTaskId(taskGroup.Id, id);
            }

            var taskVo = task.Map<TaskVO>();
            CacheManager.Save(TaskCache.Key, taskVo, taskVo.TaskGroupId);

            return taskVo;
        }
    }
}