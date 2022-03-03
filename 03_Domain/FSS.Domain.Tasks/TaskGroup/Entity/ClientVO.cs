namespace FSS.Domain.Tasks.TaskGroup.Entity;

/// <summary>
///     客户端
/// </summary>
public class ClientVO
{
    public ClientVO()
    {
        ClientIp   = "";
        ClientName = "";
    }

    public ClientVO(long clientId, string clientIp, string clientName)
    {
        ClientId   = clientId;
        ClientIp   = clientIp;
        ClientName = clientName;
    }

    /// <summary>
    ///     客户端Id
    /// </summary>
    public long ClientId { get; set; }

    /// <summary>
    ///     客户端IP
    /// </summary>
    public string ClientIp { get; set; }

    /// <summary>
    ///     客户端名称
    /// </summary>
    public string ClientName { get; set; }
}