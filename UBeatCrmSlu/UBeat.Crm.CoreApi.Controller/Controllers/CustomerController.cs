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


        #region ����ͻ�
        [HttpPost]
        [Route("distribution")]
        public OutputResult<object> DistributionCustomer([FromBody] DistributionCustomerParam entity)
        {
            if (entity == null) return ResponseError<object>("������ʽ����");
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
		/// �ͻ���ϵ�˾�����
		/// </summary>
		/// <param name="custModel"></param>
		/// <returns></returns>
		[HttpPost("getcustcontacttree")]
		public OutputResult<object> GetCustContactTree([FromBody] CustContactTreeModel custModel = null)
		{
			if (custModel == null) return ResponseError<object>("������ʽ����");
			return _customerServices.GetCustContactTree(custModel, UserId);
		}

       //�ͻ����Э��
        [HttpPost("getcustframeprotocol")]
        public OutputResult<object> GetCustFrameProtocol([FromBody] CustContactTreeModel custModel = null)
        {
            if (custModel == null) return ResponseError<object>("������ʽ����");
            return _customerServices.GetCustFrameProtocol(custModel, UserId);
        }
        
        //��ʱ��У����µ��ͻ�����
        [HttpGet("checkqccimportcustomer")]
        public OutputResult<object> checkQccImportCustomer()
        {
            var header = GetAnalyseHeader();
            var result = _customerServices.checkQccImportCustomer(LoginUser,header);
            //var result = _customerServices.updateImportCustomer(LoginUser,header);
            
            return result;
        }
    }
}