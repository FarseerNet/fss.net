## FSS是什么？

一款跨语言分布式的调度中心（基于.NET
CORE
5
语言编写）。

服务端：推荐运行在docker
on
k8s下

客户端：是我们要运行的任务，你可以使用自己的控制台（Console）、ASP.NET
CORE程序

服务端、客户端，建议多实例运行。（能实现故障自动恢复、高可用）

与.NET
CORE高度契合，借助Farseer.Net.Job组件，可以实现简单接入（1分钟上手）

客户端不再关注调度的实现或依赖。只负责编写一个个Job代码。

GitHub：https://github.com/FarseerNet/FarseerSchedulerService

dockerhub:https://hub.docker.com/r/farseernet/fss

## 服务端依赖环境

    Docker （运行在docker或k8s下）
    Net 5.0 （提供docker.hub镜像服务)
    Redis （用与多节点之间的数据同步）
    Sqlserver/MySql/Oracle（常用数据库任意选一个）
    Grpc Server（通讯协议)

## 客户端依赖环境

    netstandard2.0
    Farseer.Net.Job 开源组件 方便快速集成到业务系统（如不依赖，需自行对接）
    Grpc Client（服务端的通信）

## 痛点解决

FSS的设计初衷是为了实现分布式的调度，运行Job的程序不应该依赖调度策略，而只专注于开发自己的业务逻辑。

我希望实现Job的程序应该是高可用的，比如我利用k8s或Docker，跑3个实例（进程）。并保证任务能被负载到做任意一个客户端执行

而目前市面上的调度，大部份是基于组件的（单体应用，在当前进程中实现的调度）。无法做到多实例下，远程调度。

## 设计目标

高可用（HA）：业务方提供多实例的job运行。同一个任务、同一个job实例只会被调度一次

快速搭建：运行于docker或k8s下，提供一键部署，1分钟即可把服务部署到您的生产环境中

轻量级：低内存（没有客户端连接的时候130m，有任务的时候250m）、低CPU消耗，快速运行，依赖少。

动态的执行触发器：可定时、间隔时间、或由业务方job动态返回下次执行时间。

可观性：job的执行日志、执行时间、耗时能全面记录在服务端，供运维了解任务的执行情况

快速上手：借助Farseer.Net.Job组件（开源在github，并提供nuget包），可以快速实现一个job

## 服务端与客户端通讯

如前面叙述，本项目希望是建立一套极简、依赖少的调度平台。

因此本项目采用的通讯方案：

    1、当调度器开始工作时，会通过客户端列表（客户端启动后注册进来），以轮询的方式取出其中一个客户端，进行调度通知。
    2、客户端在收到要执行的任务时，会实时通知服务端当前任务的状态、进度。

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
`Id` int NOT NULL AUTO_INCREMENT,
`task_id` bigint NOT NULL DEFAULT '0' COMMENT '任务记录ID',
`log_level` tinyint NOT NULL DEFAULT '0' COMMENT '日志级别',
`content` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '日志内容',
`create_at` datetime(6) NOT NULL COMMENT '日志时间',
`task_group_id` int NOT NULL DEFAULT '0' COMMENT '任务组',
PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=204448 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------
-- Table structure for task
-- ----------------------------
DROP TABLE IF EXISTS `task`;
CREATE TABLE `task` (
`Id` int NOT NULL AUTO_INCREMENT,
`task_group_id` int NOT NULL DEFAULT '0' COMMENT '任务组ID',
`start_at` datetime(6) NOT NULL COMMENT '开始时间',
`run_speed` int NOT NULL COMMENT '运行耗时',
`client_host` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '客户端',
`server_node` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '服务端节点',
`client_ip` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '客户端IP',
`progress` int NOT NULL COMMENT '进度0-100',
`status` tinyint NOT NULL COMMENT '状态',
`create_at` datetime(6) NOT NULL COMMENT '任务创建时间',
PRIMARY KEY (`Id`) USING BTREE,
KEY `group_id_status` (`task_group_id`,`status`)
) ENGINE=InnoDB AUTO_INCREMENT=77731 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------
-- Table structure for task_group
-- ----------------------------
DROP TABLE IF EXISTS `task_group`;
CREATE TABLE `task_group` (
`Id` int NOT NULL AUTO_INCREMENT,
`caption` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '任务组标题',
`job_type_name` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '实现Job的特性名称（客户端识别哪个实现类）',
`start_at` timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '开始时间',
`next_at` timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '下次执行时间',
`task_id` bigint NOT NULL DEFAULT '0' COMMENT '任务ID',
`activate_at` timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '活动时间',
`last_run_at` timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '最后一次完成时间',
`run_speed_avg` int NOT NULL DEFAULT '0' COMMENT '运行平均耗时',
`run_count` int NOT NULL DEFAULT '0' COMMENT '运行次数',
`is_enable` bit(1) NOT NULL DEFAULT b'1' COMMENT '是否开启',
`interval_ms` bigint NOT NULL DEFAULT '1000' COMMENT '时间间隔',
`cron` varchar(32) COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '时间定时器表达式',
`Data` varchar(2048) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '动态参数',
PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=201 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

SET FOREIGN_KEY_CHECKS = 1;

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
      服务(172.17.0.2)启动完成，监听 http://0.0.0.0:80
info: FSS.GrpcService.Startup[0]
      正在读取所有任务组信息
info: FSS.GrpcService.Startup[0]
      共获取到：0 条任务组信息
```
## 客户端使用

通过Farseer.Net.Job组件，运行job的程序在启动之后会做：

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
[FssJob(Name = "testJob")] // Name与FSS平台配置的JobTypeName保持一致
public class HelloWorldJob : IFssJob
{
    /// <summary>
    /// 执行任务
    /// </summary>
    public Task<bool> Execute(ReceiveContext context)
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

## 任务组

任务组：是要执行任务的基本信息：任务名称、开始时间、执行次数、执行耗时、启用状态、下一次执行的任务ID。

任务：是在任务组设定并启用后动态创建并由系统自动维护的任务信息，可以知道某次的任务是由哪个客户端执行，执行状态（成功、失败），执行耗时等信息。

    当任务组启用后，会创建一条任务信息，并标记为该任务的执行开始时间，是否执行的状态。
    调度器通过这个任务建立时间轮，来轮询时间格。

## 服务端（FSS)开发进度

|  序号   | 功能  | 状态  |
|  ----  | ----  | ---- |
| 1  | 接收客户端的注册、激活 | 完成 |
| 2  | 实现服务发现SLB | 完成 |
| 3  | 定时移除10S未激活的客户端 | 完成 |
| 4  | 远程通知客户端执行JOB | 完成 |
| 5  | 接收客户端当前执行任务的进度、状态更新 | 完成 |
| 6  | 记录客户端的运行日志、异常日志 | 完成 |
| 7  | 实现任务组功能 | 完成 |
| 8  | 动态创建任务 | 完成 |
| 9  | 根据任务调度，并通知客户端 | 完成 |
| 10  | 定时同步当前服务节点的信息到缓存 | 完成 |
| 11  | 客户端断开连接时，要检查当前任务是否已处理完 | 完成 |
| 12  | 定时扫描任务当前的客户端是否断开连接 | 完成 |
| 13  | 定时同步当前节点的客户端列表到缓存 | 完成 |
| 14  | 去中心化、分布式实现 | 完成 |
| 15  | 统计任务的平均耗时 | 完成 |
| 16  | 实现三种定时功能：时间间隔、Cron表达式、客户端返回 | 完成 |
| 17  | 支持日志写入ES、数据库二选一（根据是否有ES配置判断） | 完成 |
| 18  | 增加定时清理1天前成功的任务记录（可设置至少保留多少条） | 完成 |
| 19  | 日志改用Redis作消费写入 |  完成 |
| 20  | 客户端长时间不处理事务则进入假死判定，并自动踢除客户端 |  完成 |
## .NET客户端(Farseer.Net.Job)开发进度

|  序号   | 功能  | 状态  |
|  ----  | ----  | ---- |
| 1  | 向服务端注册并打开通讯通道 | 完成 |
| 2  | 开启GRPC服务 | 完成 |
| 3  | 实现服务端的任务执行通知服务 | 完成 |
| 4  | 实现IJob Abs接口 | 完成 |
| 5  | 向服务端实时推送JOB的进度、状态、日志 | 完成 |
| 6  | 向服务端注册我能处理的任务列表 | 完成 |

## 下一个版本要实现的功能

|  序号   | 功能  |
|  ----  | ----  |
|  1  | 增加UI管理端的支持  |