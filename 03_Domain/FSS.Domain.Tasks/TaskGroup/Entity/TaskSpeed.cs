using System.Collections.Generic;
using System.Linq;

namespace FSS.Domain.Tasks.TaskGroup.Entity
{
    /// <summary>
    /// 任务执行速度
    /// </summary>
    public class TaskSpeed
    {
        /// <summary>
        /// 任务的所有执行速度
        /// </summary>
        private readonly List<long> _speedList;

        public TaskSpeed(List<long> speedList)
        {
            this._speedList = speedList;
        }

        /// <summary>
        /// 任务的执行平均速度
        /// </summary>
        public long GetAvgSpeed()
        {
            if (_speedList.Count == 0) return 0;
            return _speedList.Sum() / _speedList.Count;
        }
    }
}