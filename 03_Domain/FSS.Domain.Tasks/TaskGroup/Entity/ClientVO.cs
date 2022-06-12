namespace FSS.Domain.Tasks.TaskGroup.Entity;

/// <summary>
///     客户端
/// </summary>
/// <param name="Id">客户端Id</param>
/// <param name="Ip">客户端IP</param>
/// <param name="Name">客户端名称</param>
public readonly record struct ClientVO
(
    long   Id,
    string Ip,
    string Name
);