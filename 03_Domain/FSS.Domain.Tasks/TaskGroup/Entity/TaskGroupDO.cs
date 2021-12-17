using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Core.Exception;
using FS.DI;
using FS.Extends;
using FS.Utils.Common;
using FS.Utils.Component;
using FSS.Domain.Tasks.TaskGroup.Enum;
using FSS.Domain.Tasks.TaskGroup.Publish;
using FSS.Domain.Tasks.TaskGroup.Repository;

namespace FSS.Domain.Tasks.TaskGroup.Entity
{
    /// <summary>
    /// 任务组记录
    /// </summary>
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
        /// 本次执行任务时的Data数据
        /// </summary>
        public Dictionary<string, string> Data { get; set; }

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

        /// <summary>
        /// 运行平均耗时
        /// </summary>
        public long RunSpeedAvg { get; set; }
        /// <summary>
        /// 运行次数
        /// </summary>
        public int RunCount { get; set; }

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
            Cron    = Cron.Trim();


            // 添加到数据库
            Id = await IocManager.GetService<ITaskGroupRepository>().AddAsync(this);

            // 创建任务
            await CreateTask();
            return Id;
        }

        /// <summary>
        /// 复制任务组
        /// </summary>
        public Task<int> CopyAsync()
        {
            Caption     += "复制";
            Id          =  0;
            ActivateAt  =  DateTime.MinValue;
            NextAt      =  DateTime.MinValue;
            LastRunAt   =  DateTime.MinValue;
            RunSpeedAvg =  0;

            return IocManager.GetService<ITaskGroupRepository>().AddAsync(this);
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
                await IocManager.GetService<ITaskGroupRepository>().SaveAsync(this);
            }

            await IocManager.GetService<ITaskGroupRepository>().DeleteAsync(Id);

            // 发布删除任务组事件
            IocManager.GetService<IPublishDeleteTaskGroup>().Publish(this, Id);
        }

        /// <summary>
        /// 保存
        /// </summary>
        public async Task SaveAsync(TaskGroupDO newTaskGroupDO)
        {
            var taskGroupRepository = IocManager.GetService<ITaskGroupRepository>();

            // 更新了JobName，则要立即更新Task的JobName
            if (JobName != newTaskGroupDO.JobName)
            {
                if (Task.Status == EumTaskType.None)
                {
                    Task.JobName = newTaskGroupDO.JobName;
                }
            }

            Caption = newTaskGroupDO.Caption;
            JobName = newTaskGroupDO.JobName;
            Data    = newTaskGroupDO.Data;
            StartAt = newTaskGroupDO.StartAt;

            // 是否为数字
            if (IsType.IsInt(newTaskGroupDO.Cron))
            {
                IntervalMs = newTaskGroupDO.Cron.ConvertType(0L);
                Cron       = "";
                NextAt     = DateTime.Now.AddMilliseconds(IntervalMs);
            }
            else
            {
                var cron = new Cron(newTaskGroupDO.Cron);
                if (cron.Parse())
                {
                    IntervalMs = 0;
                    NextAt     = cron.GetNext(DateTime.Now);
                }
                else
                    throw new RefuseException("Cron格式错误");
            }

            // 停止了任务，需要把任务取消掉
            if (IsEnable != newTaskGroupDO.IsEnable)
            {
                await SetEnable(newTaskGroupDO.IsEnable);
            }
            else
            {
                await taskGroupRepository.SaveAsync(this);
            }
        }

        /// <summary>
        /// 更改启用状态
        /// </summary>
        public async Task SetEnable(bool enable)
        {
            // 停止了任务，需要把任务取消掉
            if (IsEnable && !enable)
            {
                IsEnable = enable;
                await CancelAsync();
            }
            // 重新开启了任务
            else if (!IsEnable && enable)
            {
                IsEnable = enable;
                switch (Task.Status)
                {
                    // 进行中的任务，要先取消
                    case EumTaskType.Scheduler:
                    case EumTaskType.Working:
                        await CancelAsync();
                        break;
                    // 未开始的任务，直接保存
                    case EumTaskType.None:
                    case EumTaskType.Fail:
                    case EumTaskType.Success:
                        await FinishAsync();
                        break;
                }
            }

        }

        /// <summary>
        /// 取消任务
        /// </summary>
        public async Task CancelAsync()
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

            await CreateTask();
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
            if (Task is { Status: EumTaskType.Success or EumTaskType.Fail }) await Task.AddQueueAsync();

            // 没查到时，自动创建一条对应的Task
            Task = new TaskDO
            {
                TaskGroupId = Id,
                StartAt     = NextAt,
                Caption     = Caption,
                JobName     = JobName,
                RunSpeed    = 0,
                Client      = new ClientVO(),
                Progress    = 0,
                Status      = EumTaskType.None,
                CreateAt    = DateTime.Now,
                RunAt       = DateTime.Now,
                SchedulerAt = DateTime.Now,
                Data        = new()
            };

            await IocManager.GetService<ITaskGroupRepository>().SaveAsync(this);
        }

        /// <summary>
        /// 调度时设置客户端
        /// </summary>
        public Task SchedulerAsync(ClientVO client)
        {
            Task.SetClient(client);
            Task.Data  = Data;
            ActivateAt = DateTime.Now;
            return IocManager.GetService<ITaskGroupRepository>().SaveAsync(this);
        }

        /// <summary>
        /// 检测进行中状态的任务
        /// </summary>
        public async Task CheckClientOffline()
        {
            if (Task == null)
            {
                await CreateTask();
                return;
            }

            if (Task.Status != EumTaskType.Scheduler && Task.Status != EumTaskType.Working)
            {
                return;
            }

            // 任务组活动时间大于1分钟，判定为客户端下线
            if ((DateTime.Now - ActivateAt).TotalMinutes >= 1) // 大于1分钟，才检查
            {
                var message = $"【任务假死】任务：【{JobName}】 {Id} {Caption} {Task.Status.ToString()} 在{(DateTime.Now - ActivateAt).GetDateDesc()}没有反应，强制设为失败状态";
                await CancelAsync();
                throw new RefuseException(message);
            }

            // 如果时间小于5分钟的，则按5分钟来判定
            var timeout = Math.Max(RunSpeedAvg * 2.5, (long)TimeSpan.FromMinutes(5).TotalMilliseconds);
            if ((DateTime.Now - Task.RunAt).TotalMilliseconds > timeout)
            {
                await CancelAsync();
                throw new RefuseException($"【任务超时】任务：【{JobName}】 {Id} {Caption} 超过平均运行时间：{timeout} ms，强制设为失败状态");
            }

            // // 加个时间，来限制并发
            // if (Task.Status == EumTaskType.Scheduler && (DateTime.Now - Task.StartAt).TotalSeconds < 5) return;
            // if (Task.Status == EumTaskType.Working   && (DateTime.Now - Task.RunAt).TotalSeconds   < 5) return;
            //
            // // 找出当前客户端对应的所有任务、并且执行时间 已经到了
            // var lstClientTask = await IocManager.GetService<ITaskGroupRepository>().ToListAsync(Task.Client.ClientId);
            // if (lstClientTask.Count == 0) return;
            //
            // // 全部处于调度、工作状态，说明客户端已经假死了
            // if (lstClientTask.All(o => o.Task.Status is EumTaskType.Scheduler or EumTaskType.Working))
            // {
            //     var message = $"【客户端假死】客户端：{Task.Client.ClientId}，强制下线客户端";
            //     await CancelAsync();
            //     throw new RefuseException(message);
            // }
        }

        /// <summary>
        /// 执行执行中
        /// </summary>
        public async Task Working(Dictionary<string, string> data, long nextTimespan, int progress, EumTaskType status, long runSpeed)
        {
            // 数据库的状态处于调度状态，说明客户端第一次请求进来
            if (Task.Status == EumTaskType.Scheduler)
            {
                Task.RunAt = DateTime.Now; // 首次执行，记录时间
                // 更新group元信息
                RunCount++;
                LastRunAt = DateTime.Now;
            }

            Data          = data;
            ActivateAt    = DateTime.Now;
            Task.Progress = progress;
            Task.Status   = status;
            Task.RunSpeed = runSpeed;

            // 客户端设置了动态时间
            if (nextTimespan > 0) NextAt = nextTimespan.ToTimestamps();

            // 如果是成功、错误状态，则要立即更新数据库
            switch (Task.Status)
            {
                case EumTaskType.Fail:
                case EumTaskType.Success:
                    await FinishAsync();
                    break;
                default:
                    await IocManager.GetService<ITaskGroupRepository>().SaveAsync(this);
                    break;
            }
        }

        /// <summary>
        /// 计算平均耗时
        /// </summary>
        public async Task UpdateAvgSpeed()
        {
            var speedList = await IocManager.GetService<ITaskGroupRepository>().ToTaskSpeedListAsync(Id);
            RunSpeedAvg = new TaskSpeed(speedList).GetAvgSpeed();
            if (RunSpeedAvg > 0) await IocManager.GetService<ITaskGroupRepository>().SaveAsync(this);
        }
    }
}