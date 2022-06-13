using System;
using System.Collections.Generic;
using Collections.Pooled;
using FS.Cache.Attribute;
using FSS.Domain.Client.Clients;
using FSS.Domain.Client.Clients.Repository;

namespace FSS.Infrastructure.Repository;

public class ClientRepository : IClientRepository
{
    private const string cacheKey = "FSS_ClientList";

    [Cache(cacheKey)]
    private PooledList<ClientDO> ToListInternal() => new();

    public PooledList<ClientDO> ToList()
    {
        var lst = ToListInternal();
        for (var i = 0; i < lst.Count; i++)
        {
            if ((DateTime.Now - lst[index: i].ActivateAt).TotalMinutes >= 1) // 心跳大于1秒中，任为已经下线了
            {
                RemoveClient(clientId: lst[index: i].Id);
                lst.RemoveAt(index: i);
                i--;
            }
        }
        return lst;
    }

    [CacheRemove(cacheKey)]
    public void RemoveClient(long clientId) { }

    [CacheItem(cacheKey)]
    public ClientDO ToEntity(long clientId) => null;

    [CacheUpdate(cacheKey)]
    public ClientDO Update(ClientDO clientDO) => clientDO;

    [CacheCount(cacheKey)]
    public long GetCount() => 0;
}