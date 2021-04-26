using FS.Cache;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Task.Dal;

namespace FSS.Com.MetaInfoServer.TaskGroup
{
    public class TaskGroupAdd
    {
        public ICacheManager  CacheManager  { get; set; }
        public ITaskGroupInfo TaskGroupInfo { get; set; }

        public void CreateTask(int groupId)
        {
            var taskGroupVO = TaskGroupInfo.ToInfo(groupId);

        }
    }
}