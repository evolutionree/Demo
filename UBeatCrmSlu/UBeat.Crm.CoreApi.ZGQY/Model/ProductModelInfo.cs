using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.ZGQY.Model
{
    public class ProductModelInfo
    {
    }
    public class ProductSeries
    {
        /// <summary>
        /// 产品系列topid
        /// </summary>
        public Guid TopSeriesId { get; set; }
        /// <summary>
        /// 产品系列id
        /// </summary>
        public Guid ProductsetId { get; set; }

        /// <summary>
        /// 产品系列名称
        /// </summary>
        public string SeriesName { get; set; }

        /// <summary>
        /// 产品系列编码
        /// </summary>
        public string SeriesCode { get; set; }

    }

    public class ProductStockModel
    {
        public Guid ProductId { get; set; } //物料Id
        public string ProductCode { get; set; } //物料编码
        public string ProductName { get; set; } //物料名称
        public string Factory { get; set; } //工厂
        public string FactoryName { get; set; } //工厂名称
        public string StockAddress { get; set; } //库存地点
        public string StockAddressName { get; set; }
        public decimal enableSapStock { get; set; } //非限制使用的估价的库存 
        public decimal LocationStock { get; set; } //sap库存-本地未处理库存
        public string Unit { get; set; } //基本计量单位
        public string UnitName { get; set; }
        public string SpeciFlag { get; set; }//特殊库存标识
        public string SpeciFlagName { get; set; }

        public string ProductRemark { get; set; } //物料描述
        public string ProductModel { get; set; } //规格型号 
    }
    public class ProductStockRequest
    {
        public string MATNR { get; set; } //物料编号
        public string BWKEY { get; set; } //工厂
        public string LGORT { get; set; } //库存地点
        public string CHARG { get; set; } //批次
        public string MTART { get; set; } //物料类型
    }

    public class SapStockModelResult
    {

        public String TYPE { get; set; }
        public string MESSAGE { get; set; }
        public Dictionary<String, List<SapStockModel>> DATA { get; set; }
    }

    public class SapStockModel
    {
        public SapStockModel()
        {
            LABST =0;
            INSME = 0;
            EINME = 0;
            MB52_TRAUML = 0;
            SPEME = 0;
            TRAME = 0;
        }
        public String WERKS { get; set; } //工厂
        public String NAME1 { get; set; } //工厂名称
        public String LGORT { get; set; }//库位
        public String LGOBE { get; set; }//库位名称
        public String MATNR { get; set; }//物料编号
        public String MAKTX { get; set; }//物料描述
        public decimal LABST { get; set; }//非限制库存 可用库存
        public String CHARG { get; set; }//批号
        public String MEINS { get; set; }//基本计量单位
        public String MTART { get; set; }//物料类型
        public decimal INSME { get; set; }//在检库存
        public String MATKL { get; set; }//物料组
        public decimal EINME { get; set; }//全部限制批次的总计库存
        public decimal MB52_TRAUML { get; set; }//中转和转移中的总库存
        public decimal SPEME { get; set; }//已冻结的库存
        public decimal TRAME { get; set; }//在途库存
    }

    public class ProductModel
    {
        public Guid recid { get; set; }
        public string productcode { get; set; }
        public string productname { get; set; }
        public string productmodel { get; set; }
        public string unit { get; set; }
        public Int32 productgroup { get; set; }
        public string matkl { get; set; }

    }

    public class QueryProductStockModel
    {
        public List<Guid> ProductIds { get; set; }
        public Guid EntityId { get; set; }
    }
}
