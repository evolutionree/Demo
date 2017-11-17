using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Excels
{

    public class TaskListRequestModel
    {
        public List<string> TaskIds { set; get; }
    }

    public class TaskRequestModel
    {
        public string TaskId { set; get; }
    }

    /// <summary>
    /// 导入导出进度对象
    /// </summary>
    public class ProgressModel
    {
        public string TaskName { set; get; }
        public string TaskId { set; get; }
        /// <summary>
        /// 总条数
        /// </summary>
        public long TotalRowsCount { set; get; }
        /// <summary>
        /// 已处理条数
        /// </summary>
        public long DealRowsCount { set; get; }
        /// <summary>
        /// 错误条数
        /// </summary>
        public long ErrorRowsCount { set; get; }
        /// <summary>
        /// 错误文件id
        /// </summary>
        public string ResultFileId { set; get; }
    }
}
