using FS.Cache.Redis;
using FS.DI;

namespace FSS.Infrastructure.Repository
{
    public class RedisContext
    {
        /// <summary>
        ///     平台缓存
        /// </summary>
        public static IRedisCacheManager Instance => IocManager.Instance.Resolve<IRedisCacheManager>();
    }
}