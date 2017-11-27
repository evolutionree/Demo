using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Products;
using UBeat.Crm.CoreApi.DomainModel.Products;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class ProductsServices : BasicBaseServices
    {
        private IProductsRepository _repository;

        private readonly Guid productEntityId = new Guid("59cf141c-4d74-44da-bca8-3ccf8582a1f2");//固定值



        public ProductsServices(IProductsRepository repository)
        {
            _repository = repository;
            
        }


        /// <summary>
        /// 添加产品系列
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> AddProductSeries(ProductSeriesAddModel body, int userNumber)
        {
            var crmData = new ProductSeriesInsert()
            {
                TopSeriesId=body.TopSeriesId,
                SeriesName = body.SeriesName,
                SeriesCode = body.SeriesCode
            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            
            var res= ExcuteAction((transaction, arg, userData) =>
            {
               
                return HandleResult(_repository.AddProductSeries(transaction,crmData, userNumber));

            }, body, userNumber);
            IncreaseDataVersion(DataVersionType.ProductData, null);
            return res;
        }




        /// <summary>
        /// 编辑产品系列
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> EditProductSeries(ProductSeriesEditModel body, int userNumber)
        {
            var crmData = new ProductSeriesEdit()
            {
                ProductsetId = body.ProductsetId,
                SeriesName = body.SeriesName,
                SeriesCode = body.SeriesCode

            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            
            var res= ExcuteAction((transaction, arg, userData) =>
            {

                return HandleResult(_repository.EditProductSeries(transaction, crmData, userNumber));

            }, body, userNumber);
            IncreaseDataVersion(DataVersionType.ProductData, null);
            return res;
        }


        /// <summary>
        /// 删除产品系列
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> DeleteProductSeries(ProductSeriesDeleteModel body, int userNumber)
        {
            

            var res= ExcuteAction((transaction, arg, userData) =>
            {
                return HandleResult(_repository.DeleteProductSeries(transaction,body.ProductsetId, userNumber));
            }, body, userNumber);
            IncreaseDataVersion(DataVersionType.ProductData, null);
            return res;

        }

        //启用产品系列
        public OutputResult<object> ToEnableProductSeries(ProductSeriesDeleteModel body, int userNumber)
        {


            var res = ExcuteAction((transaction, arg, userData) =>
            {
                return HandleResult(_repository.ToEnableProductSeries(transaction, body.ProductsetId, userNumber));
            }, body, userNumber);
            IncreaseDataVersion(DataVersionType.ProductData, null);
            return res;

        }


        /// <summary>
        /// 根据id获取产品系列详情，不包含子系列
        /// </summary>
        /// <param name="productsetId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OutputResult<object> GetProductSeriesDetail(Guid productsetId, int userId)
        {
            var res = ExcuteAction((transaction, arg, userData) =>
            {
                return new OutputResult<object>(_repository.GetProductSeriesDetail(transaction, productsetId, userId));
            }, productsetId, userId);
            return res;
        }



        /// <summary>
        /// 获取产品系列树
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> GetProductSeries(ProductSeriesListModel body, int userNumber)
        {
           
            return ExcuteAction((transaction, arg, userData) =>
            {
                return new OutputResult<object>(_repository.GetProductSeries(transaction,body.ProductsetId, body.Direction, body.IsGetDisable, userNumber));
            }, body, userNumber);
        }



       


        /// <summary>
        /// 删除产品
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> DeleteProduct(string productIds, int userNumber)
        {
            
            var res= ExcuteAction((transaction, arg, userData) =>
            {
                if(string.IsNullOrEmpty(productIds))
                {
                    throw new Exception("productIds不可为空");
                }
                var productIdsArray = productIds.Split(',');
                var recids = new List<Guid>();
                foreach (var pid in productIdsArray)
                {
                    recids.Add(new Guid(pid));
                }

                //判断某个实体的业务是否有权限新增文档，此处是判断实体业务表的id
                if (!userData.HasDataAccess(transaction, RoutePath, productEntityId, DeviceClassic, recids))
                {
                    throw new Exception("您没有权限停用该数据");
                }
                return HandleResult(_repository.DeleteProduct(transaction,productIds, userNumber));
            }, productIds, userNumber);
            IncreaseDataVersion(DataVersionType.ProductData, null);
            return res;
        }
        /// <summary>
        /// 启用产品
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> ToEnableProduct(string productIds, int userNumber)
        {

            var res = ExcuteAction((transaction, arg, userData) =>
            {
                if (string.IsNullOrEmpty(productIds))
                {
                    throw new Exception("productIds不可为空");
                }
                var productIdsArray = productIds.Split(',');
                var recids = new List<Guid>();
                foreach (var pid in productIdsArray)
                {
                    recids.Add(new Guid(pid));
                }

                //判断某个实体的业务是否有权限新增文档，此处是判断实体业务表的id
                if (!userData.HasDataAccess(transaction, RoutePath, productEntityId, DeviceClassic, recids))
                {
                    throw new Exception("您没有权限启用该数据");
                }
                return HandleResult(_repository.ToEnableProduct(transaction, productIds, userNumber));
            }, productIds, userNumber);
            IncreaseDataVersion(DataVersionType.ProductData, null);
            return res;
        }

        /// <summary>
        /// 获取产品
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> GetProducts(ProductListModel body, int userNumber)
        {
            var crmData = new ProductList()
            {
                ProductSeriesId = body.ProductSeriesId,
                IsAllProduct = body.IncludeChild,
                RecVersion = body.RecVersion,
                RecStatus = body.RecStatus
            };


            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            var page = new PageParam()
            {
                PageIndex = body.PageIndex,
                PageSize = body.PageSize
            };

            if (!page.IsValid())
            {
                return HandleValid(page);
            }
            return ExcuteAction((transaction, arg, userData) =>
            {
                string ruleSql = userData.RuleSqlFormat(RoutePath, productEntityId, DeviceClassic);
                return new OutputResult<object>(_repository.GetProducts(transaction, ruleSql, page, crmData, body.SearchKey, userNumber));
            }, body, userNumber);
           
        }


    }
}

