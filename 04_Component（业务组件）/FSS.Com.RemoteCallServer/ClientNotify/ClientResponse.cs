using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FS.Extends;
using FSS.Abstract.Entity.ClientNotify;
using FSS.Abstract.Entity.MetaInfo;
using FSS.Abstract.Entity.RegisterCenter;
using FSS.Abstract.Server.RemoteCall;
using FSS.GrpcService;
using Grpc.Core;

namespace FSS.Com.RemoteCallServer.ClientNotify
{
    /// <summary>
    /// 打印文本
    /// </summary>
    public class ClientResponse : IClientResponse
    {
        /// <summary>
        /// 打印文本
        /// </summary>
        /// <param name="responseStream">响应流</param>
        /// <param name="message">打印内容</param>
        public async Task PrintAsync(object responseStream, string message)
        {
            var rspStream = (IServerStreamWriter<CommandResponse>) responseStream;
            await rspStream.WriteAsync(new CommandResponse
            {
                Command    = "Print",
                ResponseAt = DateTime.Now.ToTimestamps(),
                Data       = message
            });
        }

        /// <summary>
        /// 打印文本
        /// </summary>
        /// <param name="message">打印内容</param>
        public object Print(string message)
        {
            return new CommandResponse
            {
                Command    = "Print",
                ResponseAt = DateTime.Now.ToTimestamps(),
                Data       = message
            };
        }

        /// <summary>
        /// 通知客户端开始任务调度
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="taskGroup">任务组</param>
        /// <param name="task">任务</param>
        public async Task JobSchedulerAsync(ClientConnectVO client, TaskGroupVO taskGroup, TaskVO task)
        {
            var rspStream = (IServerStreamWriter<CommandResponse>) client.ResponseStream;
            // 转换任务组的参数为字典
            var dicData   = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(taskGroup.Data))
            {
                foreach (var data in taskGroup.Data.Split(','))
                {
                    if (data == "=") continue;
                    // 取出=号的位置
                    var indexOf = data.IndexOf('=');
                    // 没取到，那么整个数据都为key
                    if (indexOf == -1) dicData[data] = "";
                    else
                    {
                        var key   = data.Substring(0, indexOf);
                        var value = indexOf + 1 > data.Length ? "" : data.Substring(indexOf + 1);
                        dicData[key] = value;
                    }
                }
            }

            await rspStream.WriteAsync(new CommandResponse
            {
                Command    = "JobScheduler",
                ResponseAt = DateTime.Now.ToTimestamps(),
                Data = new JobSchedulerVO
                {
                    TaskId      = task.Id,
                    TaskGroupId = task.TaskGroupId,
                    Caption     = taskGroup.Caption,
                    JobTypeName = taskGroup.JobTypeName,
                    StartAt     = task.StartAt,
                    ClientIp    = task.ClientIp,
                    ClientHost  = task.ClientHost,
                    Data        = dicData
                }.ToString()
            });
        }

        /// <summary>
        /// 忽略操作
        /// </summary>
        public object Ignore(string message)
        {
            return new CommandResponse
            {
                Command    = "Ignore",
                ResponseAt = DateTime.Now.ToTimestamps(),
                Data       = message
            };
        }
    }
}