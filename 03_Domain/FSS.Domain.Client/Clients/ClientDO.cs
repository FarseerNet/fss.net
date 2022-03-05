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
    public string ClientIp { get; set; }
    /// <summary>
    ///     客户端名称
    /// </summary>
    public string ClientName { get; set; }
    /// <summary>
    ///     客户端能执行的任务
    /// </summary>
    public string[] Jobs { get; set; }
    /// <summary>
    ///     活动时间
    /// </summary>
    public DateTime ActivateAt { get; set; }

    /// <summary>
    ///     移除客户端
    /// </summary>
    public Task RemoveAsync() => IocManager.GetService<IClientRepository>().RemoveClientAsync(clientDO: this);

    /// <summary>
    ///     更新客户端的使用时间
    /// </summary>
    public void Update() => IocManager.GetService<IClientRepository>().Update(clientDO: this);
}