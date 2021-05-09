using System.Threading.Tasks;
using FS.Cache.Redis;
using FS.DI;
using FS.MQ.RedisStream;
using FS.MQ.RedisStream.Attr;
using FSS.Com.MetaInfoServer.Abstract;
using FSS.Com.MetaInfoServer.RunLog.Dal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace FSS.Com.MetaInfoServer.RunLog
{
    /// <summary>
    /// 写入日志
    /// </summary>
    [Consumer(Enable = true, RedisName = "default", GroupName = "insert", QueueName = "RonLogQueue", PullCount = 1, ConsumeThreadNums = 4)]
    public class RunLogConsumer : IListenerMessage
    {
        public          IRunLogAgent RunLogAgent { get; set; }
        static readonly bool         UseEs;


        static RunLogConsumer()
        {
            var configurationSection = FS.DI.IocManager.Instance.Resolve<IConfigurationRoot>().GetSection("ElasticSearch:0:Server").Value;
            UseEs = !string.IsNullOrWhiteSpace(configurationSection);
        }

        /// <summary>
        /// 消费
        /// </summary>
        public async Task<bool> Consumer(string[] messages, ConsumeContext ea)
        {
            foreach (var message in messages)
            {
                var runLogPO = JsonConvert.DeserializeObject<RunLogPO>(message);
                if (UseEs)
                {
                    if (!await LogContext.Data.RunLog.InsertAsync(runLogPO)) return false;
                }
                else await RunLogAgent.AddAsync(runLogPO);
            }

            return true;
        }

        public Task<bool> FailureHandling(string[] messages, ConsumeContext ea) => throw new System.NotImplementedException();
    }
}