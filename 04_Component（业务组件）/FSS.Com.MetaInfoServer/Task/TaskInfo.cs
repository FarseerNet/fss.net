using FS.DI;
using FS.Extends;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Task.Dal;

namespace FSS.Com.MetaInfoServer.Task
{
    public class TaskInfo : ITaskInfo
    {
        public IIocResolver IocResolver { get; set; }

        /// <summary>
        /// 获取任务信息
        /// </summary>
        public TaskVO ToInfo(int id) => IocResolver.Resolve<ITaskAgent>().ToEntity(id).Map<TaskVO>();
    }
}