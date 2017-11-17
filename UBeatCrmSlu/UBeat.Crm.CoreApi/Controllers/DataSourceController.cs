using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models.DataSource;
using UBeat.Crm.CoreApi.Services.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class DataSourceController : BaseController
    {
        private readonly DataSourceServices _dataSourceServices;

        public DataSourceController(DataSourceServices dataSourceServices) : base(dataSourceServices)
        {
            this._dataSourceServices = dataSourceServices;
        }

        #region  数据源配置
        [HttpPost]
        [Route("querydatasource")]
        public OutputResult<object> SelectDataSource([FromBody]DataSourceListModel entityQuery = null)
        {
            if (entityQuery == null) return ResponseError<object>("参数格式错误");

            return _dataSourceServices.SelectDataSource(entityQuery, UserId);
        }

        [HttpPost]
        [Route("insertdatasource")]
        public OutputResult<object> InsertSaveDataSource([FromBody]DataSourceModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _dataSourceServices.InsertSaveDataSource(entityModel, UserId);
        }

        [HttpPost]
        [Route("updatedatasource")]
        public OutputResult<object> UpdateSaveDataSource([FromBody]DataSourceModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _dataSourceServices.UpdateSaveDataSource(entityModel, UserId);
        }

        [HttpPost]
        [Route("querydatasourcedetail")]
        public OutputResult<object> SelectDataSourceDetail([FromBody]DataSourceDetailModel entityQuery = null)
        {
            if (entityQuery == null) return ResponseError<object>("参数格式错误");

            return _dataSourceServices.SelectDataSourceDetail(entityQuery, UserId);
        }

        [HttpPost]
        [Route("insertdatasourcedetail")]
        public OutputResult<object> InsertSaveDataSourceDetail([FromBody]InsertDataSourceConfigModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return new OutputResult<object>(_dataSourceServices.InsertSaveDataSourceDetail(entityModel, UserId));
        }

        [HttpPost]
        [Route("updatedatasourcedetail")]
        public OutputResult<object> UpdateSaveDataSourceDetail([FromBody]UpdateDataSourceConfigModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _dataSourceServices.UpdateSaveDataSourceDetail(entityModel, UserId);
        }

        [HttpPost]
        [Route("deletedatasource")]
        public OutputResult<object> DataSourceDelete([FromBody]DataSrcDeleteModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _dataSourceServices.DataSourceDelete(entityModel, UserId);
        }
        #endregion

        #region 字段数据源
        [HttpPost]
        [Route("queryfieldopt")]
        public OutputResult<object> SelectFieldDicType()
        {

            return _dataSourceServices.SelectFieldDicType(UserId);
        }

        [HttpPost]
        [Route("queryfielddicvalue")]
        public OutputResult<object> SelectFieldDicVaue([FromBody]DictionaryModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _dataSourceServices.SelectFieldDicVaue(entityModel, UserId);
        }
        [HttpPost]
        [Route("savefielddictype")]
        public OutputResult<object> SaveFieldDicType([FromBody]DictionaryTypeModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _dataSourceServices.SaveFieldDicType(entityModel, UserId);
        }
        [HttpPost]
        [Route("savefieldoptval")]
        public OutputResult<object> SaveFieldOpt([FromBody]DictionaryModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _dataSourceServices.SaveFieldOptValue(entityModel, UserId);
        }
        [HttpPost]
        [Route("disableddictype")]
        public OutputResult<object> DisabledDicType([FromBody]DictionaryDisabledModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _dataSourceServices.DisabledDicType(entityModel, UserId);
        }
        [HttpPost]
        [Route("deletefieldoptval")]
        public OutputResult<object> DeleteFieldOpt([FromBody]DictionaryModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _dataSourceServices.DeleteFieldOptValue(entityModel, UserId);
        }

        [HttpPost]
        [Route("orderbyfieldoptval")]
        public OutputResult<object> OrderByFieldOpt([FromBody]ICollection<DictionaryModel> entityModels = null)
        {
            if (entityModels == null) return ResponseError<object>("参数格式错误");

            return _dataSourceServices.OrderByFieldOptValue(entityModels, UserId);
        }
        #endregion






        #region 数据源关联控件通用接口
        [HttpPost]
        [Route("querydynamicdatasrc")]
        public OutputResult<object> DynamicDataSrcQuery([FromBody]DynamicDataSrcModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
           
            return _dataSourceServices.DynamicDataSrcQuery(entityModel, UserId);
        }
        #endregion
    }
}
