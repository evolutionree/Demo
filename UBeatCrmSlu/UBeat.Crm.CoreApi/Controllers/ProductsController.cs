using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models.Documents;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.DomainModel.Documents;
using UBeat.Crm.CoreApi.Services.Models.Products;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.Version;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class ProductsController : BaseController
    {
        private readonly ILogger<ProductsController> _logger;

        private readonly ProductsServices _service;
        private readonly DynamicEntityServices _dynamicEntityServices;



        public ProductsController(ILogger<ProductsController> logger, ProductsServices service, DynamicEntityServices dynamicEntityServices) : base(service, dynamicEntityServices)
        {
            _logger = logger;
            _service = service;
            _dynamicEntityServices = dynamicEntityServices;
        }


        /// <summary>
        /// 添加产品系列
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("addseries")]
        public OutputResult<object> AddProductSeries([FromBody] ProductSeriesAddModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.AddProductSeries(body, UserId);

        }


        /// <summary>
        /// 编辑产品系列
        /// </summary>
        /// <returns></returns>
        [HttpPost("editseries")]
        public OutputResult<object> EditProductSeries([FromBody] ProductSeriesEditModel body)
        {

            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.EditProductSeries(body, UserId);

        }


        /// <summary>
        /// 删除产品系列
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("deleteseries")]
        public OutputResult<object> DeleteProductSeries([FromBody] ProductSeriesDeleteModel body)
        {

            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.DeleteProductSeries(body, UserId);

        }


        /// <summary>
        /// 获取产品系列树
        /// </summary>
        /// <returns></returns>
        [HttpPost("getseries")]
        public OutputResult<object> GetProductSeries([FromBody] ProductSeriesListModel body)
        {

            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.GetProductSeries(body, UserId);

        }
        [HttpPost]
        [Route("productseriesdetail")]
        public OutputResult<object> GetProductSeriesDetail([FromBody] ProductSeriesListModel paramInfo) {
            Guid setid = Guid.Empty;
            if (paramInfo == null) return ResponseError<object>("参数格式异常");
            if (paramInfo.ProductsetId == null || paramInfo.ProductsetId == Guid.Empty) {
                return ResponseError<object>("参数格式异常");
            }
            setid = (System.Guid)paramInfo.ProductsetId;
            return _service.GetProductSeriesDetail(setid, UserId);
        }

        [HttpPost]
        [Route("addproduct")]
        public OutputResult<object> AddProduct([FromBody] DynamicEntityAddModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("新增产品", dynamicModel);
            var header = GetAnalyseHeader();
            var res= _dynamicEntityServices.Add(dynamicModel, header, UserId);
            _service.IncreaseDataVersion(DataVersionType.ProductData);

            return res;
        }

        [HttpPost]
        [Route("editproduct")]
        public OutputResult<object> EditProduct([FromBody] DynamicEntityEditModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("编辑产品", dynamicModel);
            var header = GetAnalyseHeader();
            var res= _dynamicEntityServices.Edit(dynamicModel, header, UserId);
            _service.IncreaseDataVersion(DataVersionType.ProductData);
            return res;
        }


        /// <summary>
        /// 删除产品
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        /// 
        [HttpPost("deleteproduct")]
        public OutputResult<object> DeleteProduct([FromBody] ProductDeleteModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.DeleteProduct(body.ProductId, UserId);

        }


        /// <summary>
        /// 获取产品
        /// </summary>
        /// <returns></returns>
        [HttpPost("getproducts")]
        public OutputResult<object> GetProducts([FromBody] ProductListModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.GetProducts(body, UserId);
        }


    }
}