using System;
using System.Collections.Generic;

namespace UBeat.Crm.CoreApi.GL.Model
{
    public class SapModelInfo
    {
        public SapModelInfo()
        {
        }
    }

    public class SynSapModel
    {
        public SynSapModel()
        {
            type = -1;
        }
        public int type { get; set; }
        public Guid EntityId { get; set; }
        public List<Guid> RecIds { get; set; }
        public Dictionary<string, object> OtherParams { get; set; }
    }

    public enum BizSynEnum
    {
        None = 0,
        验证业务 = 1,
        产品=3,
    }

    public class SynResultModel
    {
        public SynResultModel()
        {
            Result = false;
            Message = string.Empty;
        }
        public bool Result { get; set; }
        public string Message { get; set; }
    }

    public enum DataSourceType
    {
        SAP = 1,
        CRM = 2,
    }

    public class AutoSynSapModel
    {
        public Guid EntityId { get; set; }
        public Guid RecId { get; set; }
    }
    #region 字典model

    public class SapDicModelResult
    {
        public string TYPE { get; set; }
        public string MESSAGE { get; set; }
        public SapDicModel DATA { get; set; }

    }
    public class SaveDicData
    {
        public SaveDicData()
        {
            DicId = Guid.NewGuid();
            RecCreated = DateTime.Now;
            RecCreator = 1;
            RecUpdated = DateTime.Now;
            RecUpdator = 1;
            RecStatus = 1;
        }
        public Guid DicId { get; set; }
        public Int32 DicTypeId { get; set; }
        public Int32 DataId { get; set; }
        public string DataVal { get; set; }
        public Int32 RecStatus { get; set; }

        public Int32 RecOrder { get; set; }
        public DateTime RecCreated { get; set; }
        public DateTime RecUpdated { get; set; }
        public int RecCreator { get; set; }
        public int RecUpdator { get; set; }

        public string ExtField1 { get; set; }
        public string ExtField2 { get; set; }
        public string ExtField3 { get; set; }
        public string ExtField4 { get; set; }
    }
    public enum DicTypeIdSynEnum
    {
        None = 0,
        分销渠道 = 62,
        销售组织 = 63,
        销售办事处 = 64,
        产品组 = 65,
        工厂 = 66,
        销售地区 = 67,
        物料类型 = 68,
        订单类型 = 69,
        库位 = 70,
        付款条件 = 55,
        订单原因=60,
        拒绝原因 = 61,
        成本中心 = 83,
        特殊处理标识 = 71,
        行项目类别 = 72,
        销售组织与公司关系 = 92,
        销售组织与渠道关系 = 93,
        销售组织与产品组关系 = 94,
        销售组织与渠道与产品组关系 = 95
    }
    public class TVTWT
    {
        public string VTWEG { get; set; }
        public string VTEXT { get; set; }
    }
    public class TVKOT
    {
        public string VKORG { get; set; }
        public string VTEXT { get; set; }
    }
    public class TVKBT
    {
        public string VKBUR { get; set; }
        public string BEZEI { get; set; }
    }
    public class TSPAT
    {
        public string SPART { get; set; }
        public string VTEXT { get; set; }
    }
    public class T001W
    {
        public string WERKS { get; set; }
        public string BWKEY { get; set; }
        public string NAME1 { get; set; }
    }
    public class T171T
    {
        public string BZIRK { get; set; }
        public string BZTXT { get; set; }
    }
    public class T134T
    {
        public string MTART { get; set; }
        public string MTBEZ { get; set; }
    }
    public class TVAKT
        {
        public string AUART { get; set; }
        public string BEZEI { get; set; }
    }
    public class T001L
    {
        public string WERKS { get; set; }
        public string LGORT { get; set; }
        public string LGOBE { get; set; }
    }
    public class T052U
        {
        public string ZTERM { get; set; }
        public string ZTAGG { get; set; }
        public string ZTEXT { get; set; }
    }
    public class TVAUT
        {
        public string AUGRU { get; set; }
        public string BEZEI { get; set; }
    }
    public class TVAGT
        {
        public string ABGRU { get; set; }
        public string BEZEI { get; set; }
    }
    public class CSKT
        {
        public string KOSTL { get; set; }
        public string DATBI { get; set; }
        public string KTEXT { get; set; }
     }
    public class TVSAKT
        {
        public string SDABW { get; set; }
        public string BEZEI { get; set; }
    }
    public class TVAPT
        {
        public string PSTYV { get; set; }
        public string VTEXT { get; set; }
    }
    public class TVKO
    {
        public string VKORG { get; set; }
        public string BUKRS { get; set; }
    }
    public class TVKOV
    {
        public string VKORG { get; set; }
        public string VTWEG { get; set; }
    }
    public class TVKOS
    {
        public string VKORG { get; set; }
        public string SPART { get; set; }
    }
    public class TVTA
    {
        public string VKORG { get; set; }
        public string VTWEG { get; set; }
        public string SPART { get; set; }
        public string VTWKU { get; set; }
        public string SPAKU { get; set; }
    }
    public class SapDicModel
    {
        //渠道
        public List<TVTWT> TVTWT { get; set; }
        //销售组织
        public List<TVKOT> TVKOT { get; set; }
        //销售办事处
        public List<TVKBT> TVKBT { get; set; }
        //产品组
        public List<TSPAT> TSPAT { get; set; }
        //工厂
        public List<T001W> T001W { get; set; }
        //销售地区
        public List<T171T> T171T { get; set; }
        //物料类型
        public List<T134T> T134T { get; set; }
        //销售凭证类型
        public List<TVAKT> TVAKT { get; set; }
        //存储地点
        public List<T001L> T001L { get; set; }
        //付款条件
        public List<T052U> T052U { get; set; }
        //订单原因
        public List<TVAUT> TVAUT { get; set; }
        //拒绝原因
        public List<TVAGT> TVAGT { get; set; }
        //成本中心
        public List<CSKT> CSKT { get; set; }
        //特殊处理标识
        public List<TVSAKT> TVSAKT { get; set; }
        //行项目类别
        public List<TVAPT> TVAPT { get; set; }
        //分配关系：销售组织<->公司
        public List<TVKO> TVKO { get; set; }
        //分配关系：销售组织<->分销渠道
        public List<TVKOV> TVKOV { get; set; }
        //分配关系：销售组织<->产品组
        public List<TVKOS> TVKOS { get; set; }
        //分配关系：销售组织<->分销渠道<->产品组
        public List<TVTA> TVTA { get; set; }
    }
    #endregion

    public class SyncCreditToSap
    {
        
    }
}
