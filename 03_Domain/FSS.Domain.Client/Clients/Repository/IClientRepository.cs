using FS.DI;

namespace FSS.Domain.Client.Clients.Repository;

public interface IClientRepository : ISingletonDependency
{
    /// <summary>
    ///     获取客户端列表
    /// </summary>
    List<ClientDO> ToList();

    /// <summary>
    ///     移除客户端
    /// </summary>
    void RemoveClient(long id);

    /// <summary>
    ///     获取客户端
    /// </summary>
    ClientDO ToEntity(long clientId);

    /// <summary>
    ///     更新客户端的使用时间
    /// </summary>
    ClientDO Update(ClientDO clientDO);

    /// <summary>
    ///     客户端数量
    /// </summary>
    long GetCount();
}