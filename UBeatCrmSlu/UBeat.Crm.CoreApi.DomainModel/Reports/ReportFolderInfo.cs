using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Reports
{
    public class ReportFolderInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public Guid ParentId { get; set; }
        public bool IsFolder { get; set; }
        public int Index { get; set; }
        public string ReportUrl { get; set; }
        public string Icon { get; set; }
        /// <summary>
        /// 关联的权限ID，原则上应该是Reportid一致，只有关联ReportID才会有FuncId
        /// </summary>
        public string FuncId { get; set; }
        public List<ReportFolderInfo> SubFolders { get; set; }
        public Guid? ReportId { get; set; }        public ReportDefineInfo ReportDefined { get; set; }

        public ReportFolderInfo() {
            SubFolders = new List<ReportFolderInfo>();
        }
        /// <summary>
        /// 
        /// </summary>
        public string WebUrl { get; set; }
    }
}
