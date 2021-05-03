using FS.Cache;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.Task.Dal;

namespace FSS.Com.MetaInfoServer.TaskGroup
{
    public class TaskGroupAdd
    {
        public ITaskGroupInfo TaskGroupInfo { get; set; }

        public void CreateTask(int groupId)
        {
            var taskGroupVO = TaskGroupInfo.ToInfo(groupId);

        }
    }
}