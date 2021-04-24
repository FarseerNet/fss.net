namespace FSS.Abstract.Enum
{
    /// <summary>
    /// 任务状态
    /// </summary>
    public enum EumTaskType
    {
        /// <summary>
        /// 未开始
        /// </summary>
        None = 0,

        /// <summary>
        /// 执行中
        /// </summary>
        Working = 1,

        /// <summary>
        /// 失败
        /// </summary>
        Fail = 2,

        /// <summary>
        /// 完成
        /// </summary>
        Success = 3
    }
}