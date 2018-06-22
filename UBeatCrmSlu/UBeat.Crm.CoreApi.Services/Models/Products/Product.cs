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
    public class ProductSearchModel {
        /// <summary>
        /// 是否直接返回顶级数据
        /// </summary>
        public int IsTopSet { get; set; }
        /// <summary>
        /// 父产品系列id
        /// </summary>
        public string PSetId { get; set; }
        /// <summary>
        /// 查询关键字（一旦使用查询关键字，则说明上面两个参数没有意义了）
        /// </summary>
        public string SearchKey { get; set; }
        /// <summary>
        /// “包含”的js过滤字符串
        /// </summary>
        public string IncludeFilter { get; set; }
        /// <summary>
        /// 内部转换使用，参数无需传入
        /// </summary>
        public string[] IncludeFilters { get; set; }
        /// <summary>
        /// js过滤参数，在产品控件中无效
        /// </summary>
        public int IncludeSubSets { get; set; }
        /// <summary>
        /// JS参数，排除字符串
        /// </summary>
        public string ExcludeFilter { get; set; }
        /// <summary>
        /// 内部转换使用，参数无需传入
        /// </summary>
        public string[] ExcludeFilters { get; set; }
        /// <summary>
        /// 是否只返回产品系列。0=返回产品和产品系列，1=只返回产品系列
        /// </summary>
        public int IsProductSetOnly { get; set; }
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        public ProductSearchModel() {
            PageIndex = 1;
            PageCount = 10;
        }
    }
    public class ProductDetailModel {
        public string recids { get; set; }
        public int ProductOrSet { get; set; }
    }
}
