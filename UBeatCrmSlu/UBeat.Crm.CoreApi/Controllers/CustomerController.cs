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
    }
}