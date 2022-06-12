using FS.DI;
using FSS.Domain.Client.Clients.Repository;

namespace FSS.Domain.Client.Clients;

public class ClientDO
{
    /// <summary>
    ///     客户端ID
    /// </summary>
    public long Id { get; set; }
    /// <summary>
    ///     客户端IP
    /// </summary>
    public string Ip { get; set; }
    /// <summary>
    ///     客户端名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    ///     客户端能执行的任务
    /// </summary>
    public string[] Jobs { get; set; }
    /// <summary>
    ///     活动时间
    /// </summary>
    public DateTime ActivateAt { get; set; }

    /// <summary>
    /// 是否超时下线
    /// </summary>
    public bool IsTimeout() => (DateTime.Now - ActivateAt).TotalMinutes >= 1;
}