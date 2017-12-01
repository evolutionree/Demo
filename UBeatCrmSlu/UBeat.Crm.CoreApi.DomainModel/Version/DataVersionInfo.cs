using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Version
{
    public  class DataVersionInfo
    {
        public DataVersionType DataType { set; get; }

        public int UserId { set; get; }

        public long MaxVersion { set; get; } = 1;

    }

    /// <summary>
    /// 数据总版本号类型：1=基础数据，2=字典数据，3=产品数据，4=实体数据，5=审批流程，6=权限配置，7=消息 
    /// </summary>
    public enum DataVersionType
    {
        BasicData=1,
        DicData=2,
        ProductData=3,
        EntityData=4,
        FlowData=5,
        PowerData=6,
        MsgData=7,
    }
}
