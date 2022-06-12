using System;
using FS.Extends;
using FS.Mapper;
using FSS.Domain.Client.Clients;
using FSS.Domain.Tasks.TaskGroup.Entity;

namespace FSS.Application.Clients.Client.Entity;

public class ClientDTO
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

    public static implicit operator ClientDO(ClientDTO dto) => dto.Map<ClientDO>();
    public static implicit operator ClientVO(ClientDTO dto) => dto.Map<ClientVO>();
}