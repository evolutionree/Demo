using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.GL.Model
{
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
}
