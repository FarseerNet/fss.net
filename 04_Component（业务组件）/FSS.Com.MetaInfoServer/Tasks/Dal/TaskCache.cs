using FSS.Abstract.Server.MetaInfo;

namespace FSS.Com.MetaInfoServer.Tasks.Dal
{
    /// <summary>
    /// 任务缓存
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class TaskCache : ITaskCache
    {
        public const string Key = "FSS_Task_All";
        public static string FailKey(int groupId) => $"FSS_Task_Fail:{groupId}";
    }
}