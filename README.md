## FSS是什么？

一款跨语言分布式的调度中心（基于`.NET CORE 5`语言编写）。

服务端：推荐运行在`docker on k8s`下
客户端：是我们要运行的任务，你可以使用自己的控制台（`Console`）、`ASP.NET CORE`程序

GitHub：https://github.com/FarseerNet/FarseerSchedulerService

Docker：https://hub.docker.com/r/farseernet/fss

## 设计目标

高可用（HA）：`多实例`的job客户端。同一个任务、同一个job实例只会被调度一次

快速搭建：`服务端`可运行于docker或k8s下，1分钟即可把服务部署到您的生产环境中

轻量级：`低内存`（没有客户端连接的时候130m，有任务的时候250m）、`低CPU消耗`，依赖少。

动态执行：可定时、间隔时间、Cron、或由业务方job动态设定下次执行时间。

快速上手：借助Farseer.Net.Job组件（开源在github，并提供nuget包），可以快速实现一个job

可视化：借用FOPS，可以维护任务组，查看任务进度、耗时、日志。
## 服务端搭建

    Docker （运行在docker或k8s下）
    Net 5.0 （提供docker.hub镜像服务)
    Redis （用与多节点之间的数据同步）
    Sqlserver/MySql/Oracle（常用数据库任意选一个）
    建议：多实例运行

`1、mysql 脚本（你也可以换其它数据库）`

[Mysql脚本](https://raw.githubusercontent.com/FarseerNet/FarseerSchedulerService/main/07_Solution%20Items%EF%BC%88%E9%A1%B9%E7%9B%AE%E6%96%87%E4%BB%B6%EF%BC%89/fss.sql)

`2、docker运行脚本`

```
docker run -d --name fss -p 888:888 \
-e Database__default="DataType=MySql,Server=mysql:3306,UserID=root,PassWord=steden@123,Catalog=fss,PoolMaxSize=50,PoolMinSize=1" \
-e Redis__default=Server="Server=redis:6379,DB=13,Password=123456,ConnectTimeout=600000,SyncTimeout=10000,ResponseTimeout=10000" \
-e ElasticSearch__es="Server=http://es:9200,Username=es,Password=123456" \
-e ElasticSearch__LinkTrack="Server=http://es:9200,Username=es,Password=123456" \
--network=net farseernet/fss:latest \
--restart=always
```

你也可以使用挂载配置的方式
```
docker run -d --name fss -p 888:888 \
-v /home/appsettings.json:app/appsettings.json \
farseernet/fss:latest --restart=always
```

环境变量解释：

|  环境变量   | 说明  |
|  ----  | ----  |
| FSS__Server  | 当前FSS地址 |
| Database__default  | 数据库地址 |
| Redis__default  | redis地址 |
| ElasticSearch__es  | es地址，用于写入日志，不填，则使用数据库记录 |
| ElasticSearch__LinkTrack  | 链路追踪的ES地址，默认启用 |


## 客户端使用

    netstandard2.0
    Farseer.Net.Job 开源组件 可以实现快速接入1分钟上手（如不依赖，需自行对接）
    建议：多实例运行

### 1、通过Farseer.Net.Job组件
运行job的程序在启动之后会做：

    1、向FSS调度平台注册客户端的信息打开Channel。（如多个server，会打开多个，断开后自动重连）
    2、实时接收FSS调度平台的消息通知

`客户端appsettings.json配置`

```json
{
  "FSS": {
    "Server": "http://localhost,http://localhost"
    // FSS平台地址（多节点用,分隔）
  }
}
```

`Program.cs`

```c#
[Fss] // 开启后，才能注册到FSS平台
public class Program
{
    public static void Main()
    {
        // 初始化模块
        FarseerApplication.Run<StartupModule>().Initialize();
        Thread.Sleep(-1);
    }
}
```

`StartupModule.cs`

```c#
/// <summary>
/// 启动模块
/// </summary>
[DependsOn(typeof(JobModule))] // 依赖Job模块
public class StartupModule : FarseerModule
{
}
```

`HelloWorldJob.cs`

```c#
[FssJob(Name = "testJob")] // Name与FSS平台配置的JobName保持一致
public class HelloWorldJob : IFssJob
{
    /// <summary>
    /// 执行任务
    /// </summary>
    public Task<bool> Execute(IFssContext context)
    {
        // 告诉FSS平台，当前进度执行了 20%
        context.SetProgress(20);

        // 让FSS平台，记录日志
        context.Logger(LogLevel.Information, "你好，世界！");

        // 下一次执行时间为10秒后（如果不设置，则使用任务组设置的时间）
        context.SetNextAt(TimeSpan.FromSeconds(1));

        // 任务执行成功
        return Task.FromResult(true);
    }
}
```
### 2、通过http调用 （application/json）
header data（所有接口都需要带入）:
```json
{
    "ClientIp" : "请获取你的客户端IP",
    "ClientId" : 1465307444889034752, // 18位 int64，随机生成。同一个实例，此ID必须一致。
    "ClientName" : "客户端名称，推荐取当前hostname",
    "ClientJobs" : "helloJob,worldJob" // jobName,多个用,分开。这里告知FSS，我当前能处理的job。
}
```
#### 2.1 拉取任务：
http://{server}/task/pull

post:
```json
{
    "TaskCount" : 10 // 本次拉取的数量
}
```
说明：`FSS`返回的任务是包含在15S内将要执行的任务，因此在您拉取到任务后，需根据当前任务的执行时间，自行做Thread.Sleep线程休眠。

#### 2.2 返回任务状态：
http://{server}/task/JobInvoke

post :
```json
{
    "TaskGroupId" : 11, // 任务组ID
    "NextTimespan" : 0, // 时间戳（毫秒），0：根据任务组的设置。>0 ：按当前传入的值为下一次执行时间
    "Progress" : 20, // 当前进度：0~100
    "Status" : 2, // 任务的执行状态：0、未开始，1、已调度，2、执行中，3、失败（完成），4、成功（完成）
    "RunSpeed" : 1000, // 毫秒，当前任务执行耗时
    "Log" : { // 日志
        "LogLevel" : 1000, // 日志等级,Trace=0,Debug=1,Info=2,Warning=3,Error=4,Critical=5
        "Log" : "日志内容",
        "CreateAt" : "2021-11-30 17:25:31", // 记录时间
    }, 
    "Data" : { // 数据,json结构。传入后，会保存到任务组中，供下一次执行时使用。
        "Key1":"Value1",
        "Key2":"Value2",
        "Key3":"Value3",
    }, 
}
```

就这2个接口，实现任务的拉取，任务的执行状态反馈，任务的日志。

说明：当任务拉取后开始执行时，应立即请求`JobInvoke`接口，并传入Status=2。告知`FSS`，任务正在执行。
## UI控制台
请参考：https://github.com/FarseerNet/FOPS

支持对任务的编辑设置、查看当前实时的任务状态、任务日志。

基于ASP.NET Core Blazor 5语言编写
## 任务组

任务组：是要执行任务的基本信息：任务名称、开始时间、执行次数、执行耗时、启用状态、下一次执行的任务ID。

任务：是在任务组设定并启用后动态创建并由系统自动维护的任务信息，可以知道某次的任务是由哪个客户端执行，执行状态（成功、失败），执行耗时等信息。

    当任务组启用后，会创建一条任务信息，并标记为该任务的执行开始时间，是否执行的状态。
    调度器通过这个任务建立时间轮，来轮询时间格。