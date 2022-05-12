using System;
using System.Linq;
using FS.DI;
using FS.ElasticSearch;
using FSS.Infrastructure.Repository.Log.Model;
using Microsoft.Extensions.Configuration;

namespace FSS.Infrastructure.Repository.Context;

/// <summary>
///     ES日志上下文
/// </summary>
public class EsContext : EsContext<EsContext>
{
    /// <summary>
    ///     ES索引日期格式化
    /// </summary>
    private static readonly string ElasticIndexFormat;
    public static readonly bool UseEs;

    static EsContext()
    {
        UseEs              = IocManager.Instance.Resolve<IConfigurationRoot>().GetSection(key: "ElasticSearch").GetChildren().Any(predicate: o => o.Key == "es");
        ElasticIndexFormat = IocManager.Instance.Resolve<IConfigurationRoot>().GetSection(key: "FSS:ElasticIndexFormat").Value;
        if (string.IsNullOrWhiteSpace(value: ElasticIndexFormat)) ElasticIndexFormat = "yyyy_MM";
    }

    public EsContext() : base(configName: "es")
    {
    }

    /// <summary>
    ///     用户索引
    /// </summary>
    public IndexSet<TaskLogPO> RunLog { get; set; }

    protected override void CreateModelInit()
    {
        RunLog.SetName(indexName: $"FssLog_{DateTime.Now.ToString(format: ElasticIndexFormat)}", shardsCount: 2, replicasCount: 0, refreshInterval: 1, "FssLog");
    }
}