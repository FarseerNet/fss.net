using System;
using Collections.Pooled;
using FS.Core.Abstract.AspNetCore;
using FS.DI;
using FS.Extends;
using FSS.Application.Clients.Client.Entity;
using FSS.Domain.Client.Clients;
using FSS.Domain.Client.Clients.Repository;
using Microsoft.AspNetCore.Http;

namespace FSS.Application.Clients.Client;

[UseApi(Area = "meta")]
public class ClientApp : ISingletonDependency
{
    public IClientRepository    ClientRepository    { get; set; }
    public IHttpContextAccessor HttpContextAccessor { get; set; }

    /// <summary>
    ///     取出全局客户端列表
    /// </summary>
    [Api("GetClientList")]
    public PooledList<ClientDTO> ToList() => ClientRepository.ToList().Map<ClientDTO>();

    /// <summary>
    ///     客户端数量
    /// </summary>
    [Api("GetClientCount")]
    public long GetCount() => ClientRepository.GetCount();

    /// <summary>
    ///     更新客户端的使用时间
    /// </summary>
    public void UpdateClient(ClientDO clientDO) => ClientRepository.Update(clientDO);

    public ClientDTO GetClient()
    {
        var client = new ClientDTO
        {
            Ip         = HttpContextAccessor.HttpContext.Request.Headers[key: "ClientIp"].ToString().Split(separator: ',')[0].Trim(),
            Name       = HttpContextAccessor.HttpContext.Request.Headers[key: "ClientName"],
            Id         = HttpContextAccessor.HttpContext.Request.Headers[key: "ClientId"].ToString().ConvertType(defValue: 0L),
            Jobs       = HttpContextAccessor.HttpContext.Request.Headers[key: "ClientJobs"].ToString().Split(separator: ','),
            ActivateAt = DateTime.Now
        };
        // 更新客户端的使用时间
        ClientRepository.Update(client);

        return client;
    }
}