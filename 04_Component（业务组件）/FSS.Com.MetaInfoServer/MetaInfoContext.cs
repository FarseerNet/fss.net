﻿using System.Collections.Generic;
using FS.Data;
using FS.Data.Map;
using FSS.Com.MetaInfoServer.RunLog.Dal;
using FSS.Com.MetaInfoServer.Task.Dal;
using FSS.Com.MetaInfoServer.TaskGroup.Dal;

namespace FSS.Com.MetaInfoServer
{
    /// <summary>
    /// 元信息上下文
    /// </summary>
    public class MetaInfoContext : DbContext<MetaInfoContext>
    {
        public MetaInfoContext() : base("default")
        {
        }
        
        public TableSet<TaskPO>      Task      { get; set; }
        public TableSet<TaskGroupPO> TaskGroup { get; set; }
        public TableSet<RunLogPO>    RunLog    { get; set; }

        protected override void CreateModelInit(Dictionary<string, SetDataMap> map)
        {
            map["Task"].SetName("task");
            map["TaskGroup"].SetName("task_group");
            map["RunLog"].SetName("run_log");
        }
    }
}