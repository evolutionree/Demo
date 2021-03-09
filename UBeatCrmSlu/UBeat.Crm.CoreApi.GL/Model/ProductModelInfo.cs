using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.GL.Model
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
        public string StockAddress { get; set; } //库存地点
        public string StockAddressName { get; set; }
        public decimal LABST { get; set; } //非限制使用的估价的库存
        public decimal LocationStock { get; set; } //sap库存-本地未处理库存
        public string Unit { get; set; } //基本计量单位
        public string UnitName { get; set; }
        public string SpeciFlag { get; set; }//特殊库存标识
        public string SpeciFlagName { get; set; }

        public string ProductRemark { get; set; } //物料描述
        public string ProductModel { get; set; } //规格型号 
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
}
