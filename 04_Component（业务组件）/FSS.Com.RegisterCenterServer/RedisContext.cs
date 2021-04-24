using System;
using System.Collections.Generic;
using FS.Cache.Redis;
using FS.DI;
using Newtonsoft.Json;

namespace FSS.Com.RegisterCenterServer
{
    public class RedisContext
    {
        /// <summary>
        ///     平台缓存
        /// </summary>
        /// <remarks> 测 </remarks>
        public static RedisCacheManager Instance => IocManager.Instance.Resolve<RedisCacheManager>("default");

        /// <summary>
        ///     事务，批量写入HASH
        /// </summary>
        public static void HashSetTransaction<TPO>(string key, List<TPO> lst, Func<TPO, string> funcDataKey, Func<TPO, string> funcData = null)
        {
            HashSetTransaction(Instance, key, lst, funcDataKey, funcData);
        }

        /// <summary>
        ///     事务，批量写入HASH
        /// </summary>
        public static void HashSetTransaction<TPO>(RedisCacheManager instance, string key, List<TPO> lst, Func<TPO, string> funcDataKey, Func<TPO, string> funcData = null)
        {
            if (lst == null || lst.Count == 0) return;
            if (funcData == null) funcData = po => JsonConvert.SerializeObject(po);

            var transaction = instance.Db.CreateTransaction();
            foreach (var po in lst)
            {
                var dataKey = funcDataKey(po);
                var data    = funcData(po);
                transaction.HashSetAsync(key, dataKey, data);
            }

            transaction.Execute();
        }
    }
}