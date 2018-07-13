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
            if (custModel == null) return ResponseError<object>("������ʽ����");
            return _customerServices.QueryCustRelate(custModel.CustId, UserId);
        }

        /// <summary>
        /// ��ȡ���ϲ��Ŀͻ��б�
        /// </summary>
        /// <returns></returns>
        [Route("needmergelist")]
        public OutputResult<object> GetMergeCustomerList([FromBody] MergeCustListModel custModel = null)
        {
            return _customerServices.GetMergeCustomerList(custModel, UserId);
        }
        /// <summary>
        /// �ϲ��ͻ�
        /// </summary>
        /// <param name="custModel"></param>
        /// <returns></returns>
        [Route("merge")]
        public OutputResult<object> MergeCustomer([FromBody] CustMergeModel custModel = null)
        {
            if (custModel == null) return ResponseError<object>("������ʽ����");
            return _customerServices.MergeCustomer(custModel, LoginUser);
        }

        #region �ͻ��ݷ�
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
    }
}