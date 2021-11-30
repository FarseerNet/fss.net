## FSS是什么？

一款跨语言分布式的调度中心（基于`.NET CORE 5`语言编写）。

服务端：推荐运行在`docker on k8s`下
客户端：是我们要运行的任务，你可以使用自己的控制台（`Console`）、`ASP.NET CORE`程序

服务端、客户端，建议多实例运行。（能实现故障自动恢复、实现高可用）

与.NET CORE 高度契合，借助`Farseer.Net.Job`组件，可以实现快速接入（1分钟上手）

客户端不再关注调度的实现或依赖。只负责编写一个个Job代码。

GitHub：https://github.com/FarseerNet/FarseerSchedulerService

dockerhub:https://hub.docker.com/r/farseernet/fss

## 服务端依赖环境

    Docker （运行在docker或k8s下）
    Net 5.0 （提供docker.hub镜像服务)
    Redis （用与多节点之间的数据同步）
    Sqlserver/MySql/Oracle（常用数据库任意选一个）

## 客户端依赖环境

    netstandard2.0
    Farseer.Net.Job 开源组件 方便快速集成到业务系统（如不依赖，需自行对接）

## 痛点解决

FSS的设计初衷是为了实现分布式的调度，运行Job的程序不应该依赖调度策略，而只专注于开发自己的业务逻辑。

我希望实现Job的程序应该是高可用的，比如我利用k8s或Docker，跑3个实例（进程）。并保证任务能被负载到做任意一个客户端执行

而目前市面上的调度，大部份是基于组件的（单体应用，在当前进程中实现的调度）。无法做到多实例下，远程调度。

## 设计目标

高可用（HA）：业务方提供多实例的job运行。同一个任务、同一个job实例只会被调度一次

快速搭建：运行于docker或k8s下，提供一键部署，1分钟即可把服务部署到您的生产环境中

轻量级：低内存（没有客户端连接的时候130m，有任务的时候250m）、低CPU消耗，快速运行，依赖少。

动态的执行触发器：可定时、间隔时间、或由业务方job动态设定下次执行时间。

可观性：job的执行日志、执行时间、耗时能全面记录在服务端，供运维了解任务的执行情况

快速上手：借助Farseer.Net.Job组件（开源在github，并提供nuget包），可以快速实现一个job

## 服务端搭建

`1、mysql 脚本（你也可以换其它数据库）`
```mysql
SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for run_log
-- ----------------------------
DROP TABLE IF EXISTS `run_log`;
CREATE TABLE `run_log` (
`Id` int(11) NOT NULL AUTO_INCREMENT,
`task_id` bigint(20) NOT NULL DEFAULT '0' COMMENT '任务记录ID',
`log_level` tinyint(4) NOT NULL DEFAULT '0' COMMENT '日志级别',
`content` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '日志内容',
`create_at` datetime(6) NOT NULL COMMENT '日志时间',
`task_group_id` int(11) NOT NULL DEFAULT '0' COMMENT '任务组',
`caption` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '任务组标题',
`job_name` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '实现Job的特性名称（客户端识别哪个实现类）',
PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=218630 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------
-- Table structure for task
-- ----------------------------
DROP TABLE IF EXISTS `task`;
CREATE TABLE `task` (
`Id` int(11) NOT NULL AUTO_INCREMENT,
`task_group_id` int(11) NOT NULL DEFAULT '0' COMMENT '任务组ID',
`start_at` datetime(6) NOT NULL COMMENT '开始时间',
`run_speed` int(11) NOT NULL COMMENT '运行耗时',
`client_host` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '客户端',
`server_node` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '服务端节点',
`client_ip` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '客户端IP',
`progress` int(11) NOT NULL COMMENT '进度0-100',
`status` tinyint(4) NOT NULL COMMENT '状态',
`create_at` datetime(6) NOT NULL COMMENT '任务创建时间',
`caption` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '任务组标题',
`job_name` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '实现Job的特性名称（客户端识别哪个实现类）',
`run_at` timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '实际执行时间',
PRIMARY KEY (`Id`) USING BTREE,
KEY `group_id_status` (`task_group_id`,`status`,`create_at`,`Id`) USING BTREE,
KEY `task_group_id` (`create_at`,`task_group_id`) USING BTREE,
KEY `start_at` (`start_at`,`status`) USING BTREE,
KEY `create_at` (`status`,`create_at`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=322048 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------
-- Table structure for task_group
-- ----------------------------
DROP TABLE IF EXISTS `task_group`;
CREATE TABLE `task_group` (
`Id` int(11) NOT NULL AUTO_INCREMENT,
`caption` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '任务组标题',
`job_name` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '实现Job的特性名称（客户端识别哪个实现类）',
`start_at` timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '开始时间',
`next_at` timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '下次执行时间',
`task_id` bigint(20) NOT NULL DEFAULT '0' COMMENT '任务ID',
`activate_at` timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '活动时间',
`last_run_at` timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '最后一次完成时间',
`run_speed_avg` int(11) NOT NULL DEFAULT '0' COMMENT '运行平均耗时',
`run_count` int(11) NOT NULL DEFAULT '0' COMMENT '运行次数',
`is_enable` bit(1) NOT NULL DEFAULT b'1' COMMENT '是否开启',
`interval_ms` bigint(20) NOT NULL DEFAULT '1000' COMMENT '时间间隔',
`cron` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '时间定时器表达式',
`Data` varchar(2048) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '动态参数',
PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=206 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

SET FOREIGN_KEY_CHECKS = 1;
```mysql

```

`2、docker运行脚本`

```
docker run -d --name fss \
-e Redis__0__Server=127.0.0.1:6379 \
-e Redis__0__Password=123456 \
-e Database__Items__0__Server=127.0.0.1 \
-e Database__Items__0__Port=1433 \
-e Database__Items__0__UserID=sa \
-e Database__Items__0__PassWord=123456 \
-e Database__Items__0__Catalog=fss \
-e Database__Items__0__DataType=MySql \
farseernet/fss:latest --restart=always
```

环境变量解释：

|  环境变量   | 说明  |
|  ----  | ----  |
| Redis__0__Server  | redis地址 |
| Redis__0__Password  | redis密码，默认123456（没有，把value去掉） |
| Database__Items__0__Server  | 数据库地址 |
| Database__Items__0__Port  | 数据库端口 |
| Database__Items__0__UserID  | 数据库账号 |
| Database__Items__0__PassWord  | 数据库密码 |
| Database__Items__0__Catalog  | 数据库名称 |
| Database__Items__0__DataType  | 数据库类型，SqlServer,OleDb,SQLite,Oracle,PostgreSql |

你也可以使用挂载配置的方式
```
docker run -d --name fss \
-v /home/appsettings.json:app/appsettings.json \
farseernet/fss:latest --restart=always
```

`3、查看打印日志`
```
docker logs fss
```

```
info: FS.FarseerApplication[0]
      注册系统核心组件
info: FS.Modules.FarseerModuleManager[0]
      总共找到 11 个模块
info: FS.Modules.FarseerModuleManager[0]
      已经加载模块: FSS.GrpcService.Startup, FSS.GrpcService, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
info: FS.Modules.FarseerModuleManager[0]
      已经加载模块: FS.Core.FarseerCoreModule, Farseer.Net.Core, Version=2.5.1.0, Culture=neutral, PublicKeyToken=null
info: FS.Modules.FarseerModuleManager[0]
      已经加载模块: FS.Mapper.MapperModule, Farseer.Net.Mapper, Version=2.5.0.0, Culture=neutral, PublicKeyToken=null
info: FS.Modules.FarseerModuleManager[0]
      已经加载模块: FS.Cache.Redis.RedisModule, Farseer.Net.Cache.Redis, Version=2.5.1.0, Culture=neutral, PublicKeyToken=null
info: FS.Modules.FarseerModuleManager[0]
      已经加载模块: FS.Cache.CacheManagerModule, Farseer.Net.Cache, Version=2.5.1.0, Culture=neutral, PublicKeyToken=null
info: FS.Modules.FarseerModuleManager[0]
      已经加载模块: FS.Data.DataModule, Farseer.Net.Data, Version=2.5.0.0, Culture=neutral, PublicKeyToken=null
info: FS.Modules.FarseerModuleManager[0]
      已经加载模块: FSS.Com.MetaInfoServer.MetaInfoModule, FSS.Com.MetaInfoServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
info: FS.Modules.FarseerModuleManager[0]
      已经加载模块: FSS.Com.SchedulerServer.SchedulerModule, FSS.Com.SchedulerServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
info: FS.Modules.FarseerModuleManager[0]
      已经加载模块: FSS.Com.RegisterCenterServer.RegisterCenterModule, FSS.Com.RegisterCenterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
info: FS.Modules.FarseerModuleManager[0]
      已经加载模块: FSS.Com.RemoteCallServer.RemoteCallModule, FSS.Com.RemoteCallServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
info: FS.Modules.FarseerModuleManager[0]
      已经加载模块: FS.Modules.FarseerKernelModule, Farseer.Net, Version=2.5.0.0, Culture=neutral, PublicKeyToken=null
info: FS.Modules.FarseerModuleManager[0]
      模块加载完毕，开始启动11个模块...
info: FS.Modules.FarseerModuleManager[0]
      模块启动完毕...
info: FS.FarseerApplication[0]
      系统初始化完毕，耗时731.57ms
info: FSS.GrpcService.Startup[0]
      服务(172.17.0.2)启动完成，监听 http://0.0.0.0:88
info: FSS.GrpcService.Startup[0]
      正在读取所有任务组信息
info: FSS.GrpcService.Startup[0]
      共获取到：0 条任务组信息
```
## 客户端使用
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