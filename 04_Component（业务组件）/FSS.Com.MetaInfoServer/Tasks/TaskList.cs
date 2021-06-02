using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.Tasks.Dal;

namespace FSS.Com.MetaInfoServer.Tasks
{
    /// <summary>
    /// 任务列表
    /// </summary>
    public class TaskList : ITaskList
    {
        public ITaskAgent TaskAgent { get; set; }

        /// <summary>
        /// 获取指定任务组执行成功的任务列表
        /// </summary>
        public Task<List<TaskVO>> ToSuccessListAsync(int groupId, int top) => TaskAgent.ToSuccessListAsync(groupId, top).MapAsync<TaskVO, TaskPO>();
        
        /// <summary>
        /// 清除成功的任务记录（1天前）
        /// </summary>
        public Task ClearSuccessAsync(int groupId, int taskId) => TaskAgent.ClearSuccessAsync(groupId, taskId);

        /// <summary>
        /// 获取未执行的任务列表
        /// </summary>
        public Task<List<TaskVO>> ToNoneListAsync() => TaskAgent.ToNoneListAsync().MapAsync<TaskVO, TaskPO>();
    }
}