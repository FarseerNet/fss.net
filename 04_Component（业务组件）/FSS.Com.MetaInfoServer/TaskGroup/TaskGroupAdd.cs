using System;
using System.Threading.Tasks;
using FS.Extends;
using FS.Utils.Common;
using FS.Utils.Component;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Server.MetaInfo;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;

namespace FSS.Com.MetaInfoServer.TaskGroup
{
    public class TaskGroupAdd : ITaskGroupAdd
    {
        public TaskGroupAgent TaskGroupAgent { get; set; }
        public ITaskGroupInfo TaskGroupInfo  { get; set; }

        /// <summary>
        /// 添加任务信息
        /// </summary>
        public async Task<int> AddAsync(TaskGroupVO vo)
        {
            // 是否为数字
            if (IsType.IsInt(vo.Cron))
            {
                vo.IntervalMs = vo.Cron.ConvertType(0L);
                vo.Cron       = "";
            }
            else if (new Cron().Parse(vo.Cron))
            {
                vo.IntervalMs = 0;
            }
            else
                throw new Exception("Cron格式错误");

            var po = new TaskGroupPO
            {
                Caption    = vo.Caption.Trim(),
                JobName    = vo.JobName.Trim(),
                Data       = vo.Data.Trim(),
                IntervalMs = vo.IntervalMs,
                Cron       = vo.Cron.Trim(),
                StartAt    = vo.StartAt,
                IsEnable   = vo.IsEnable
            };

            // 添加到数据库
            await TaskGroupAgent.AddAsync(po);

            // 从数据库中读出刚添加的任务组，并保存到缓存
            await TaskGroupInfo.ToInfoByDbAsync(po.Id.GetValueOrDefault());

            return po.Id.GetValueOrDefault();
        }
    }
}