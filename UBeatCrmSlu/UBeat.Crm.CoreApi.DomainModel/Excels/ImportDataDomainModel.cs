using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Excels
{
    public class ImportDataDomainModel
    {
        public List<Dictionary<string, object>> DataRows { get; set; }
        public string Sql { get; set; }

        /// <summary>
        /// 导入时的默认参数，这些参数不存在与Excel文档中，由调用方传入
        /// </summary>
        public Dictionary<string, object> DefaultParameters { set; get; }

    }

    public class ImportRowDomainModel
    {
        public Dictionary<string, object> DataRow { get; set; }
        public string Sql { get; set; }

        /// <summary>
        /// 导入时的默认参数，这些参数不存在与Excel文档中，由调用方传入
        /// </summary>
        public Dictionary<string, object> DefaultParameters { set; get; }

        public int UserNo { set; get; }

        /// <summary>
        /// 新增导入(重复的放弃导入)=4
        /// 覆盖导入=5
        /// </summary>
        public int OperateType { set; get; }

    }
}
