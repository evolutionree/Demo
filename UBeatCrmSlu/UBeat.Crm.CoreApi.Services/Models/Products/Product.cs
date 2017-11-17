using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Products
{
    public class ProductAddModel
    {

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 产品系列名称
        /// </summary>
        public string ProductSeriesName { get; set; }

        /// <summary>
        /// 产品描述
        /// </summary>
        public string ProductFeatures { get; set; }
    }



    public class ProductEditModel
    {
        public Guid ProductId { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 产品描述
        /// </summary>
        public string ProductFeatures { get; set; }

    }



    public class ProductDeleteModel
    {
        public string ProductId { get; set; }

    }



    public class ProductListModel
    {

        /// <summary>
        /// 产品系列id
        /// </summary>
        public Guid ProductSeriesId { get; set; }


        /// <summary>
        /// 是否包括子系列
        /// </summary>
        public bool IncludeChild { get; set; }

        /// <summary>
        /// 记录版本
        /// </summary>
        public Int64 RecVersion { get; set; }


        /// <summary>
        /// 记录状态
        /// </summary>
        public int RecStatus { get; set; }


        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }


        /// <summary>
        /// 单页数据
        /// </summary>
        public int PageSize { get; set; }


        /// <summary>
        /// 查询关键字
        /// </summary>
        public string SearchKey { get; set; }


    }
}
