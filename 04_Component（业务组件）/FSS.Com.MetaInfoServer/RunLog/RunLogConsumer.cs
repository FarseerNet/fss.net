using System;
using System.Linq;
using System.Threading.Tasks;
using FS.Cache.Redis;
using FS.DI;
using FS.MQ.RedisStream;
using FS.MQ.RedisStream.Attr;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.RunLog.Dal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace FSS.Com.MetaInfoServer.RunLog
{
    /// <summary>
    /// 写入日志
    /// </summary>
    [Consumer(Enable = true, RedisName = "default", GroupName = "insert", QueueName = "RunLogQueue", PullCount = 10, ConsumeThreadNums = 4)]
    public class RunLogConsumer : IListenerMessage
    {
        public          IRunLogAgent RunLogAgent { get; set; }
        public          IIocManager  IocManager  { get; set; }
        static readonly bool         UseEs;

        static RunLogConsumer()
        {
            var configurationSection = FS.DI.IocManager.Instance.Resolve<IConfigurationRoot>().GetSection("ElasticSearch:0:Server").Value;
            UseEs = !string.IsNullOrWhiteSpace(configurationSection);
        }

        /// <summary>
        /// 消费
        /// </summary>
        public async Task<bool> Consumer(StreamEntry[] messages, ConsumeContext ea)
        {
            var lstPo = messages.Select(message=>JsonConvert.DeserializeObject<RunLogPO>(message.Values[0].Value.ToString())).ToList();
            if (UseEs)
            {
                return await LogContext.Data.RunLog.InsertAsync(lstPo);
            }
            return await MetaInfoContext.Data.RunLog.InsertAsync(lstPo) > 0;
            
        }

        public Task<bool> FailureHandling(StreamEntry[] messages, ConsumeContext ea) => throw new System.NotImplementedException();
    }
}