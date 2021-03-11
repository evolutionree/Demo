using System;
using System.Collections.Generic;
using System.Data.Common;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.GL.Model;

namespace UBeat.Crm.CoreApi.GL.Repository
{
    public interface ISapProductRepository
    {
        DictionaryDataModel getProductLine(string productcode);
        dynamic getProductInfoById(Guid recId);
        dynamic getProductInfoByIds(List<Guid> recIds);
        ProductModel IsExitProduct(String productCode, int userId);
        ProductSeries IsExitProductSeries(String productSetName, int userId);
        OperateResult EditProductSeries(DbTransaction trans, ProductSeries data, int userNumber);
        OperateResult AddProductSeries(DbTransaction trans, ProductSeries data, int userNumber);
        void DelAllProductFactory( int userId);
        void DelAllProductSaleOrg( int userId);
        void DelAllProductFactoryTemp(int userId);
        void DelAllProductSaleOrgTemp(int userId);
        void ProductFactoryTempToFormal(int userId);
        void ProductSaleOrgTempToFormal(int userId);
        /// <summary>
        /// 获取SAP仓位对应关系（从字典表中获取）
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetStockMapping(DbTransaction trans, int userId);
        List<Dictionary<string, object>> GetDeliveryStock(Guid productId);
        List<Dictionary<string, object>> GetDeliveryStock(List<Guid> productIds);

        dynamic getProductInfoId();
        bool delProductInfoMprice();
    }
}
