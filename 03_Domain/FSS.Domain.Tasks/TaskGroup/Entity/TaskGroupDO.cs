using System;
using System.Threading.Tasks;
using FS.DI;
using FS.Extends;
using FS.Mapper;
using FS.Utils.Common;
using FS.Utils.Component;
using FSS.Infrastructure.Repository.TaskGroup.Interface;
using FSS.Infrastructure.Repository.TaskGroup.Model;
using FSS.Infrastructure.Repository.Tasks.Enum;

namespace FSS.Domain.Tasks.TaskGroup.Entity
{
    /// <summary>
    /// 任务组记录
    /// </summary>
    [Serializable]
    [Map(typeof(TaskGroupPO))]
    public class TaskGroupDO
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 任务
        /// </summary>
        public TaskDO Task { get; set; }

        /// <summary>
        /// 任务组标题
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// 实现Job的特性名称（客户端识别哪个实现类）
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// 传给客户端的参数，按逗号分隔
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartAt { get; set; }

        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime NextAt { get; set; }

        /// <summary>
        /// 时间间隔
        /// </summary>
        public long IntervalMs { get; set; }

        /// <summary>
        /// 时间定时器表达式
        /// </summary>
        public string Cron { get; set; }

        /// <summary>
        /// 活动时间
        /// </summary>
        public DateTime ActivateAt { get; set; }

        /// <summary>
        /// 最后一次完成时间
        /// </summary>
        public DateTime LastRunAt { get; set; }

        /// <summary>
        /// 是否开启
        /// </summary>
        public bool IsEnable { get; set; }

        public static implicit operator TaskGroupPO(TaskGroupDO taskGroupDO) => taskGroupDO.Map<TaskGroupPO>();

        /// <summary>
        /// 添加任务组信息
        /// </summary>
        public async Task<int> AddAsync()
        {
            // 是否为数字
            if (IsType.IsInt(Cron))
            {
                IntervalMs = Cron.ConvertType(0L);
                Cron       = "";
                NextAt     = DateTime.Now.AddMilliseconds(IntervalMs);
            }
            else
            {
                var cron = new Cron(Cron);
                if (cron.Parse())
                {
                    IntervalMs = 0;
                    NextAt     = cron.GetNext(DateTime.Now);
                }
                else
                    throw new Exception("Cron格式错误");
            }

            Caption = Caption.Trim();
            JobName = JobName.Trim();
            Data    = Data.Trim();
            Cron    = Cron.Trim();


            // 添加到数据库
            Id = await IocManager.GetService<ITaskGroupAgent>().AddAsync(this.Map<TaskGroupPO>());

            // 发布任务组创建事件
            //IocManager.GetService<TaskGroupPublish>().CreateTaskGroup(this, taskGroupDO.Id);

            // 发布任务组开启事件
            if (IsEnable)
            {
                IocManager.GetService<TaskGroupPublish>().EnableTaskGroup(this, Id);
            }
            return Id;
        }

        /// <summary>
        /// 删除任务组
        /// </summary>
        public async Task DeleteAsync()
        {
            // 如果任务组是开启状态，则需要先暂定任务组
            if (IsEnable)
            {
                IsEnable = false;
                var taskGroupAgent = IocManager.GetService<ITaskGroupAgent>();
                await taskGroupAgent.SaveAsync(this);
                await taskGroupAgent.UpdateAsync(Id, this);
            }

            // 发布删除任务组事件
            IocManager.GetService<TaskGroupPublish>().DeleteTaskGroup(this, Id);
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        public async Task CancelTask()
        {
            if (Task != null) Task.Status = EumTaskType.Fail;

            // 这里不设置的话，下次执行时间，有可能还是将来的，导致如果设置错了时间的话，会一直等待原来设置错的时间
            NextAt = LastRunAt;

            await FinishAsync();
        }

        /// <summary>
        /// 保存Task（taskGroup必须是最新的）
        /// </summary>
        public async Task FinishAsync()
        {
            // 本次的时间策略晚，则通过时间策略计算出来
            if (DateTime.Now > NextAt)
            {
                var cron = new Cron();
                // 时间间隔器
                if (IntervalMs > 0) NextAt = DateTime.Now.AddMilliseconds(IntervalMs);
                else if (!string.IsNullOrWhiteSpace(Cron) && cron.Parse(Cron))
                {
                    NextAt = cron.GetNext(DateTime.Now);
                }
                else // 没有找到设置下一次时间的设置，则默认30S执行一次
                {
                    NextAt = DateTime.Now.AddSeconds(30);
                }
            }

            await IocManager.GetService<ITaskGroupAgent>().SaveAsync(this);

            // 发布任务完成事件
            IocManager.GetService<TaskGroupPublish>().TaskFinish(this, this);
        }

        /// <summary>
        /// 创建新的Task
        /// </summary>
        public async Task CreateTask()
        {
            if (!IsEnable)
            {
                Task = null;
                return;
            }

            if (Task != null && Task.Status != EumTaskType.Fail && Task.Status != EumTaskType.Success) return;

            // 没查到时，自动创建一条对应的Task
            Task = new TaskDO
            {
                TaskGroupId = Id,
                StartAt     = NextAt,
                Caption     = Caption,
                JobName     = JobName,
                RunSpeed    = 0,
                ClientId    = 0,
                ClientIp    = "",
                Progress    = 0,
                Status      = EumTaskType.None,
                CreateAt    = DateTime.Now,
                RunAt       = DateTime.Now,
                SchedulerAt = DateTime.Now
            };
            
            await IocManager.GetService<ITaskGroupAgent>().SaveAsync(this);
        }
    }
}