using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FS.Cache.Redis;
using FS.Core;
using FSS.Abstract.Entity;
using FSS.Abstract.Enum;
using FSS.Abstract.Server.MetaInfo;
using FSS.Abstract.Server.RegisterCenter;
using Newtonsoft.Json;

namespace FSS.Com.RegisterCenterServer.Client
{
    /// <summary>
    /// 客户端注册
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class ClientRegister : IClientRegister
    {
        private string             Key = "FSS_ClientList";
        public  IRedisCacheManager RedisCacheManager { get; set; }
        public  ITaskInfo          TaskInfo          { get; set; }
        public  ITaskUpdate        TaskUpdate        { get; set; }
        public  ITaskGroupInfo     TaskGroupInfo     { get; set; }

        /// <summary>
        /// 更新客户端调用的使用时间
        /// </summary>
        public void UpdateClient(ClientVO client)
        {
            RedisCacheManager.Db.HashSet(Key, client.Id, client.ToString());
        }

        /// <summary>
        /// 取出全局客户端
        /// </summary>
        public async Task<ClientVO> ToInfo(long clientId)
        {
            var hashGetAsync = await RedisCacheManager.Db.HashGetAsync(Key, clientId);
            return !hashGetAsync.HasValue ? null : Jsons.ToObject<ClientVO>(hashGetAsync.ToString());
        }

        /// <summary>
        /// 取出全局客户端列表
        /// </summary>
        public async Task<List<ClientVO>> ToList()
        {
            var hashGetAll = await RedisCacheManager.Db.HashGetAllAsync(Key);
            if (hashGetAll == null || hashGetAll.Length == 0) return null;
            var lst = hashGetAll.Select(o => JsonConvert.DeserializeObject<ClientVO>(o.Value.ToString())).ToList();
            for (int i = 0; i < lst.Count; i++)
            {
                // 心跳大于1秒中，任为已经下线了
                if ((DateTime.Now - lst[i].ActivateAt).TotalMinutes >= 1)
                {
                    await RedisCacheManager.Db.HashDeleteAsync(Key, lst[i].Id);
                    lst.RemoveAt(i);
                    i--;
                }
            }

            return lst;
        }

        /// <summary>
        /// 客户端是否存在
        /// </summary>
        public bool IsExists(long clientId) => RedisCacheManager.Db.HashExists(Key, clientId);

        /// <summary>
        /// 移除客户端
        /// </summary>
        public async Task RemoveAsync(long clientId)
        {
            if (!IsExists(clientId)) return;
            RedisCacheManager.Db.HashDelete(Key, clientId);
            
            // 读取当前所有任务组的任务
            var groupListAsync = await TaskInfo.ToGroupListAsync();
            var findAll        = groupListAsync.FindAll(o => o.ClientId == clientId && o.Status is EumTaskType.Scheduler or EumTaskType.Working or EumTaskType.ReScheduler);
            foreach (var vo in findAll)
            {
                var taskGroup = await TaskGroupInfo.ToInfoAsync(vo.TaskGroupId);
                vo.Status = EumTaskType.Fail;
                await TaskUpdate.SaveFinishAsync(vo, taskGroup);
            }
        }
    }
}