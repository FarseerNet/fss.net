using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Cache.Redis;
using FSS.Abstract.Entity.RegisterCenter;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace FSS.Com.RegisterCenterServer.Client
{
    /// <summary>
    /// 客户端注册
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class ClientRegister : IClientRegister
    {
        private                 string                                        Key = "FSS_ClientList";
        public                  IRedisCacheManager                            RedisCacheManager { get; set; }
        public                  ITaskInfo                                     TaskInfo          { get; set; }
        public                  ITaskUpdate                                   TaskUpdate        { get; set; }
        public                  ITaskGroupInfo                                TaskGroupInfo     { get; set; }
        private static readonly ConcurrentDictionary<string, ClientConnectVO> Clients = new();

        readonly MemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions
        {
        });

        /// <summary>
        /// 注册
        /// </summary>
        public void Register(ClientConnectVO client)
        {
            Clients[client.ServerHost] = client;
        }

        /// <summary>
        /// 更新客户端调用的使用时间
        /// </summary>
        public void UpdateUseAt(string serverHost, DateTime useAt)
        {
            if (Clients.TryGetValue(serverHost, out var client))
            {
                client.UseAt = useAt;
            }
        }

        /// <summary>
        /// 更新客户端心跳时间
        /// </summary>
        public void UpdateHeartbeatAt(string serverHost, DateTime heartbeatAt)
        {
            if (Clients.TryGetValue(serverHost, out var client))
            {
                client.HeartbeatAt = heartbeatAt;
            }
        }

        /// <summary>
        /// 取出客户端列表
        /// </summary>
        public List<ClientConnectVO> ToList()
        {
            return Clients.Select(o => o.Value).ToList();
        }

        /// <summary>
        /// 取出全局客户端列表
        /// </summary>
        public async Task<List<ClientConnectVO>> ToListByRedisAsync()
        {
            var hashGetAll = await RedisCacheManager.Db.HashGetAllAsync(Key);
            if (hashGetAll == null || hashGetAll.Length == 0) return null;
            var lst = hashGetAll.Select(o => JsonConvert.DeserializeObject<ClientConnectVO>(o.Value.ToString())).ToList();
            for (int i = 0; i < lst.Count; i++)
            {
                // 心跳大于1秒中，任为已经下线了
                if ((DateTime.Now - lst[i].HeartbeatAt).TotalMinutes >= 1)
                {
                    await RedisCacheManager.Db.HashDeleteAsync(Key, lst[i].ServerHost);
                    lst.RemoveAt(i);
                    i--;
                }
            }

            return lst;
        }

        /// <summary>
        /// 取出全局客户端列表（本地缓存）
        /// </summary>
        public async Task<List<ClientConnectVO>> ToListByMemoryAsync()
        {
            return await _memoryCache.GetOrCreate(Key, o =>
            {
                o.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(1);
                return ToListByRedisAsync();
            });
        }

        /// <summary>
        /// 客户端是否存在
        /// </summary>
        public async Task<bool> IsExistsByRedis(string serverHost)
        {
            var redisValue = await RedisCacheManager.Db.HashGetAsync(Key, serverHost);
            if (!redisValue.HasValue) return false;

            var client = JsonConvert.DeserializeObject<ClientConnectVO>(redisValue.ToString());
            if (client == null) return false;
            if ((DateTime.Now - client.HeartbeatAt).TotalMinutes >= 1)
            {
                RedisCacheManager.Db.HashDelete(Key, client.ServerHost);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取客户端是否存在
        /// </summary>
        public bool Exists(string jobName) => Clients.Any(o => o.Value.JobName == jobName); //o => (DateTime.Now - o.Value.HeartbeatAt).TotalMilliseconds < 10000

        /// <summary>
        /// 获取客户端数量
        /// </summary>
        public int Count() => Clients.Count;

        /// <summary>
        /// 取出全局客户端数量
        /// </summary>
        public Task<long> ToClientCountAsync() => RedisCacheManager.Db.HashLengthAsync(Key);

        /// <summary>
        /// 取出客户端
        /// </summary>
        public ClientConnectVO ToInfo(string serverHost)
        {
            if (Clients.TryGetValue(serverHost, out var client))
            {
            }

            return client;
        }

        /// <summary>
        /// 同步本地缓存到Redis
        /// </summary>
        public void SyncCache()
        {
            var lst = ToList();
            RedisCacheManager.HashSetTransaction(Key, lst, vo => vo.ServerHost);
        }

        /// <summary>
        /// 客户端是否存在
        /// </summary>
        public bool IsExists(string serverHost) => Clients.ContainsKey(serverHost);

        /// <summary>
        /// 移除客户端
        /// </summary>
        public async Task RemoveAsync(string serverHost)
        {
            if (!IsExists(serverHost)) return;

            Clients.TryRemove(serverHost, out _);
            SyncCache();
            // 读取当前所有任务组的任务
            var groupListAsync = await TaskInfo.ToGroupListAsync();
            var findAll        = groupListAsync.FindAll(o => o.ClientHost == serverHost && o.Status is EumTaskType.Scheduler or EumTaskType.Working or EumTaskType.ReScheduler);
            foreach (var vo in findAll)
            {
                var taskGroup = await TaskGroupInfo.ToInfoAsync(vo.TaskGroupId);
                vo.Status = EumTaskType.Fail;
                await TaskUpdate.SaveAsync(vo, taskGroup);
            }
        }
    }
}