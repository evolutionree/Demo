using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models.Customer;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class CustomerController : BaseController
    {
        private readonly CustomerServices _customerServices;


        public CustomerController(CustomerServices customerServices) : base(customerServices)
        {
            _customerServices = customerServices;
        }

        [Route("querycustrel")]
        public OutputResult<object> QueryCustRelate([FromBody] CustRelateModel custModel = null)
        {
            if (custModel == null) return ResponseError<object>("参数格式错误");
            return _customerServices.QueryCustRelate(custModel.CustId, UserId);
        }

        /// <summary>
        /// 获取待合并的客户列表
        /// </summary>
        /// <returns></returns>
        [Route("needmergelist")]
        public OutputResult<object> GetMergeCustomerList([FromBody] MergeCustListModel custModel = null)
        {
            return _customerServices.GetMergeCustomerList(custModel, UserId);
        }
        /// <summary>
        /// 合并客户
        /// </summary>
        /// <param name="custModel"></param>
        /// <returns></returns>
        [Route("merge")]
        public OutputResult<object> MergeCustomer([FromBody] CustMergeModel custModel = null)
        {
            if (custModel == null) return ResponseError<object>("参数格式错误");
            return _customerServices.MergeCustomer(custModel, LoginUser);
        }

        #region 客户拜访
        [HttpPost("selecttodayindex")]
        public OutputResult<object> SelectTodayIndex()
        {
            return _customerServices.SelectTodayIndex(UserId);
        }
        [HttpPost("selectdaily")]
        public OutputResult<object> SelectDaily()
        {
            return _customerServices.SelectDaily(UserId);
        }
        #endregion


        #region 分配客户
        [HttpPost]
        [Route("distribution")]
        public OutputResult<object> DistributionCustomer([FromBody] DistributionCustomerParam entity)
        {
            if (entity == null) return ResponseError<object>("参数格式错误");
            return _customerServices.DistributionCustomer(entity, UserId);
        }
        #endregion

        [HttpPost]
        [Route("synerp")]
        public OutputResult<object> ToErpCustomer([FromBody] SyncErpCusomter sync)
        {
            var result = _customerServices.ToErpCustomer(sync, UserId);
            return result;
        }

		/// <summary>
		/// 客户联系人决策树
		/// </summary>
		/// <param name="custModel"></param>
		/// <returns></returns>
		[HttpPost("getcustcontacttree")]
		public OutputResult<object> GetCustContactTree([FromBody] CustContactTreeModel custModel = null)
		{
			if (custModel == null) return ResponseError<object>("参数格式错误");
			return _customerServices.GetCustContactTree(custModel, UserId);
		}

       //客户框架协议
        [HttpPost("getcustframeprotocol")]
        public OutputResult<object> GetCustFrameProtocol([FromBody] CustContactTreeModel custModel = null)
        {
            if (custModel == null) return ResponseError<object>("参数格式错误");
            return _customerServices.GetCustFrameProtocol(custModel, UserId);
        }
        
        //临时表校验更新到客户资料
        [HttpGet("checkqccimportcustomer")]
        public OutputResult<object> checkQccImportCustomer()
        {
            var header = GetAnalyseHeader();
            var result = _customerServices.checkQccImportCustomer(LoginUser,header);
            //var result = _customerServices.updateImportCustomer(LoginUser,header);
            
            return result;
        }
        
        [HttpGet("checkqcccustomer")]
        public OutputResult<object> checkQccCustomer(string name)
        {
            var result = _customerServices.checkQccCustomer(name);
            return result;
        }
        
        [HttpGet("updatewhenucodeisnull")]
        public OutputResult<object> UpdateWhenUCodeIsNull()
        {
            var result = _customerServices.UpdateWhenUCodeIsNull();
            return result;
        }
    }
}