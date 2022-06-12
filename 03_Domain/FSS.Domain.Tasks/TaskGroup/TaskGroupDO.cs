using FS.Core.Exception;
using FS.Extends;
using FS.Utils.Common;
using FS.Utils.Component;
using FSS.Domain.Tasks.TaskGroup.Entity;
using FSS.Domain.Tasks.TaskGroup.Enum;
using FSS.Domain.Tasks.TaskGroup.Event;

namespace FSS.Domain.Tasks.TaskGroup;

/// <summary>
///     任务组记录
/// </summary>
public class TaskGroupDO
{
    public TaskGroupDO() { }

    /// <summary>
    /// 复制新的任务组
    /// </summary>
    public TaskGroupDO(TaskGroupDO copySource)
    {
        Caption    = copySource.Caption + "复制";
        JobName    = copySource.JobName;
        Data       = copySource.Data;
        IntervalMs = copySource.IntervalMs;
        Cron       = copySource.Cron;
        IsEnable   = copySource.IsEnable;
    }


    /// <summary>
    ///     主键
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     任务
    /// </summary>
    public TaskDO Task { get; set; }

    /// <summary>
    ///     任务组标题
    /// </summary>
    public string Caption { get; set; }

    /// <summary>
    ///     实现Job的特性名称（客户端识别哪个实现类）
    /// </summary>
    public string JobName { get; set; }

    /// <summary>
    ///     本次执行任务时的Data数据
    /// </summary>
    public Dictionary<string, string> Data { get; set; }

    /// <summary>
    ///     开始时间
    /// </summary>
    public DateTime StartAt { get; set; }

    /// <summary>
    ///     下次执行时间
    /// </summary>
    public DateTime NextAt { get; set; }

    /// <summary>
    ///     时间间隔
    /// </summary>
    public long IntervalMs { get; set; }

    /// <summary>
    ///     时间定时器表达式
    /// </summary>
    public string Cron { get; set; }

    /// <summary>
    ///     活动时间
    /// </summary>
    public DateTime ActivateAt { get; set; }

    /// <summary>
    ///     最后一次完成时间
    /// </summary>
    public DateTime LastRunAt { get; set; }

    /// <summary>
    ///     是否开启
    /// </summary>
    public bool IsEnable { get; set; }

    /// <summary>
    ///     运行平均耗时
    /// </summary>
    public long RunSpeedAvg { get; set; }
    /// <summary>
    ///     运行次数
    /// </summary>
    public int RunCount { get; set; }

    /// <summary>
    ///  保存任务组信息前，检查状态
    /// </summary>
    public void CheckInterval()
    {
        // 是否为数字
        if (IsType.IsInt(number: Cron))
        {
            IntervalMs = Cron.ConvertType(defValue: 0L);
            Cron       = "";
            NextAt     = DateTime.Now.AddMilliseconds(value: IntervalMs);
        }
        else
        {
            var cron = new Cron(expression: Cron);
            if (cron.Parse())
            {
                IntervalMs = 0;
                NextAt     = cron.GetNext(time: DateTime.Now);
            }
            else
                throw new Exception(message: "Cron格式错误");
        }

        Caption = Caption.Trim();
        JobName = JobName.Trim();
        Cron    = Cron.Trim();
    }

    /// <summary>
    /// 修改任务状态为不可用
    /// </summary>
    public void Disable()
    {
        IsEnable = false;
    }

    /// <summary>
    /// 修改了任务组信息
    /// </summary>
    public void Set(string jobName, string caption, Dictionary<string, string> data, DateTime startAt)
    {
        // 更新了JobName，则要立即更新Task的JobName
        if (JobName != jobName && Task.Status == EumTaskType.None) Task.SetJobName(jobName);
        JobName = jobName;
        Caption = caption;
        Data    = data;
        StartAt = startAt;
    }

    /// <summary>
    /// 修改了任务的时间间隔
    /// </summary>
    public void Set(string strCron, long intervalMs)
    {
        // 是否为数字
        if (Cron != strCron || IntervalMs != intervalMs)
        {
            if (IsType.IsInt(strCron))
            {
                IntervalMs = strCron.ConvertType(defValue: 0L);
                Cron       = "";
                NextAt     = DateTime.Now.AddMilliseconds(value: IntervalMs);
            }
            else
            {
                var cron = new Cron(expression: strCron);
                if (cron.Parse())
                {
                    IntervalMs = 0;
                    NextAt     = cron.GetNext(time: DateTime.Now);
                }
                else
                    throw new RefuseException(message: "Cron格式错误");
            }
        }
    }

    /// <summary>
    ///     更改启用状态
    /// </summary>
    public void SetEnable(bool enable)
    {
        // 停止了任务，需要把任务取消掉
        if (IsEnable && !enable)
        {
            IsEnable = enable;
            Cancel();
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
                    Cancel();
                    break;
                // 未开始的任务，直接保存
                case EumTaskType.None:
                case EumTaskType.Fail:
                case EumTaskType.Success:
                    Finish();
                    break;
            }
        }
    }

    /// <summary>
    ///     取消任务
    /// </summary>
    public void Cancel()
    {
        Task?.SetFail();
        // 这里不设置的话，下次执行时间，有可能还是将来的，导致如果设置错了时间的话，会一直等待原来设置错的时间
        NextAt = LastRunAt;
        // 设置下一次的执行时间
        CalculateNextTime();
        // 创建新的任务
        CreateTask();
    }

    /// <summary>
    ///     保存Task（taskGroup必须是最新的）
    /// </summary>
    public void Finish()
    {
        CalculateNextTime();
        // 如果是停止状态，创建任务不会执行。则需要在这里进行保存
        CreateTask();
    }

    /// <summary>
    /// 任务完成后，计算下一次的时间
    /// </summary>
    public void CalculateNextTime()
    {
        // 本次的时间策略晚，则通过时间策略计算出来
        if (DateTime.Now > NextAt)
        {
            var cron = new Cron();
            // 时间间隔器
            if (IntervalMs > 0) NextAt = DateTime.Now.AddMilliseconds(value: IntervalMs);
            else if (!string.IsNullOrWhiteSpace(value: Cron) && cron.Parse(expression: Cron))
                NextAt = cron.GetNext(time: DateTime.Now);
            else // 没有找到设置下一次时间的设置，则默认30S执行一次
                NextAt = DateTime.Now.AddSeconds(value: 30);
        }
    }

    /// <summary>
    ///     创建新的Task
    /// </summary>
    public void CreateTask()
    {
        if (!IsEnable)
        {
            Task = null;
            return;
        }

        if (Task != null && Task.Status != EumTaskType.Fail && Task.Status != EumTaskType.Success) return;
        if (Task is { Status: EumTaskType.Success or EumTaskType.Fail })
        {
            // 任务完成，发布完成事件
            new TaskFinishEvent(Task).PublishEvent();
        }

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
            Data        = this.Data
        };
    }

    /// <summary>
    ///     调度时设置客户端
    /// </summary>
    public void SchedulerAsync(ClientVO client)
    {
        if (Task.Status == EumTaskType.None)
        {
            Task.SetClient(client: client);
            Task.Data  = Data;
            ActivateAt = DateTime.Now;
        }
    }

    /// <summary>
    ///     检测进行中状态的任务
    /// </summary>
    public void CheckClientOffline()
    {
        if (Task.Status != EumTaskType.Scheduler && Task.Status != EumTaskType.Working) return;

        // 任务组活动时间大于1分钟，判定为客户端下线
        if ((DateTime.Now - ActivateAt).TotalMinutes >= 1) // 大于1分钟，才检查
            throw new RefuseException(message: $"【任务假死】任务：【{JobName}】 {Id} {Caption} {Task.Status.ToString()} 在{(DateTime.Now - ActivateAt).GetDateDesc()}没有反应，强制设为失败状态");

        // 如果时间小于5分钟的，则按5分钟来判定
        var timeout = Math.Max(val1: RunSpeedAvg * 2.5, val2: (long)TimeSpan.FromMinutes(value: 5).TotalMilliseconds);
        if ((DateTime.Now - Task.RunAt).TotalMilliseconds > timeout) throw new RefuseException(message: $"【任务超时】任务：【{JobName}】 {Id} {Caption} 超过平均运行时间：{timeout} ms，强制设为失败状态");

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
    ///     执行中
    /// </summary>
    public void Working(Dictionary<string, string> data, long nextTimespan, int progress, EumTaskType status, long runSpeed)
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
                Finish();
                break;
        }
    }
}