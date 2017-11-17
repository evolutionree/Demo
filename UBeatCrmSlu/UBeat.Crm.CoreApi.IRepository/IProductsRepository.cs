﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Products;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IProductsRepository
    {


        /// <summary>
        /// 添加产品系列
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        OperateResult AddProductSeries(DbTransaction trans,ProductSeriesInsert data, int userNumber);


        /// <summary>
        /// 编辑产品系列
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        OperateResult EditProductSeries(DbTransaction trans, ProductSeriesEdit data, int userNumber);


        /// <summary>
        /// 删除产品系列
        /// </summary>
        /// <param name="eventids"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        OperateResult DeleteProductSeries(DbTransaction trans, Guid productSeriesId, int userNumber);


        /// <summary>
        /// 获取产品系列树
        /// </summary>
        /// <returns></returns>
        dynamic GetProductSeries(DbTransaction trans, Guid? productSetId, string direction, int userNumbe);



       
        /// <summary>
        /// 删除产品
        /// </summary>
        /// <param name="eventids"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        OperateResult DeleteProduct(DbTransaction trans, string productIds, int userNumber);


        /// <summary>
        /// 获取产品
        /// </summary>
        /// <returns></returns>
        dynamic GetProducts(DbTransaction trans,string ruleSql, PageParam page, ProductList productData, string serachKey, int userNumber);


        



    }
}
