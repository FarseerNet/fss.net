using System;
using FS.Extends;
using FS.Mapper;
using FSS.Domain.Client.Clients;
using FSS.Domain.Tasks.TaskGroup.Entity;

namespace FSS.Application.Clients.Client.Entity;

[Map(typeof(ClientDO), typeof(ClientVO))]
public class ClientDTO
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

    public static implicit operator ClientDO(ClientDTO dto) => dto.Map<ClientDO>();
    public static implicit operator ClientVO(ClientDTO                            dto) => new(clientId: dto.Id, clientIp: dto.ClientIp, clientName: dto.ClientName);
}