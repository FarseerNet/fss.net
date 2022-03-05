using FS.DI;

namespace FSS.Domain.Client.Clients.Repository;

public interface IClientRepository : ISingletonDependency
{
    /// <summary>
    ///     获取客户端列表
    /// </summary>
    Task<List<ClientDO>> ToListAsync();

    /// <summary>
    ///     移除客户端
    /// </summary>
    Task RemoveClientAsync(ClientDO clientDO);

    /// <summary>
    ///     获取客户端
    /// </summary>
    Task<ClientDO> ToEntityAsync(long clientId);

    /// <summary>
    ///     更新客户端的使用时间
    /// </summary>
    void Update(ClientDO clientDO);

    /// <summary>
    ///     客户端数量
    /// </summary>
    Task<long> GetCountAsync();
}