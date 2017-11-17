﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Products
{



    public class ProductSeriesAddModel
    {

        public Guid TopSeriesId { get; set; }

        /// <summary>
        /// 产品系列名称
        /// </summary>
        public string SeriesName { get; set; }

        /// <summary>
        /// 父品系列名称
        /// </summary>
        public string ParentSeriesName { get; set; }

        /// <summary>
        /// 产品系列编码(不能重复)
        /// </summary>
        public string SeriesCode { get; set; }

    }



    public class ProductSeriesEditModel
    {

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



    public class ProductSeriesDeleteModel
    {

        public Guid ProductsetId { get; set; }


    }



    public class ProductSeriesListModel
    {

        public Guid ? ProductsetId { get; set; }

        public string Direction { get; set; }


    }
}
