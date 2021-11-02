using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.ZGQY.Model;
using UBeat.Crm.CoreApi.ZGQY.Repository;

namespace UBeat.Crm.CoreApi.ZGQY.Services
{
	public class DataPlatformServices: BasicBaseServices
	{
		IDataPlatformRepository _dataPlatformRepository;
		IDynamicEntityRepository _dynamicEntityRepository;
		private int _synBeginDateDay = -60;
		public DataPlatformServices(IDataPlatformRepository dataPlatformRepository, IDynamicEntityRepository dynamicEntityRepository)
		{
			_dataPlatformRepository = dataPlatformRepository;
			_dynamicEntityRepository = dynamicEntityRepository;
		}

		public void InitDataQrtz()
		{
			var init = false;
			InitProjectData(init);
			InitBillData(init);
			InitPayData(init);
			InitZdData(init);
			InitClData(init);
		}

		public void InitData()
		{
			var init = true;
			InitProjectData(init);
			InitBillData(init);
			InitPayData(init);
			//InitZdData(init);
			//InitClData(init);
		}

		#region 同步项目
		public void InitProjectDataQrtz()
		{
			var init = false;
			InitProjectData(init);
		}

		public dynamic InitProjectData(bool init = false)
		{
			var userId = 1;
			var date = DateTime.Now.AddDays(_synBeginDateDay);
			if (init)
			{
				date = DateTime.MinValue;
			}
			var dataList = _dataPlatformRepository.GetProejctData(date);
			ProcessProejctData(userId, dataList); 

			return null;
		}

		private void ProcessProejctData(int userId, List<SaveProjectInfo> dataList)
		{
			var entityId = new Guid("57ba6974-ea53-4e4c-90b8-a82669ba800d");
			foreach (var item in dataList)
			{
				if (string.IsNullOrEmpty(string.Concat(item.project_code))) continue;

				var contract = _dataPlatformRepository.GetContractByCode(item.contract_code);
				var recmanager = userId;
				if (contract != null)
				{ 
					item.contract_code_json = JsonHelper.ToJson(new DataSourceInfo() { id = contract.RecId, name = contract.RecName });
					recmanager = contract.Recmanager;
				}
				else
				{
					continue;
				}

				Dictionary<string, object> dic = new Dictionary<string, object>();
				dic.Add("projectcode", item.project_code);
				dic.Add("projectname", item.project_name);
				dic.Add("contract", item.contract_code_json);
				dic.Add("progress", item.status_name);
				dic.Add("recmanager", recmanager);
				dic.Add("recstatus", 1);

				var recId = _dataPlatformRepository.IsExitProject(item.project_code);
				if (recId != Guid.Empty)
				{
					_dynamicEntityRepository.DynamicEdit(null, entityId, recId, dic, userId);
				}
				else
				{
					_dynamicEntityRepository.DynamicAdd(null, entityId, dic, null, userId);
				}
			}
		}
		#endregion

		#region 同步开票
		public void InitBillDataQrtz()
		{
			var init = false;
			InitBillData(init);
		}

		public dynamic InitBillData(bool init = false)
		{
			var userId = 1;
			var date = DateTime.Now.AddDays(_synBeginDateDay);
			if (init)
			{
				date = DateTime.MinValue;
			}
			var dataList = _dataPlatformRepository.GetBillData(date);
			ProcessBillData(userId, dataList);

			return null;
		}

		private void ProcessBillData(int userId, List<SaveBillInfo> dataList)
		{
			var entityId = new Guid("bc1fc80d-5991-44e1-b822-a0a5a7af3432");
			foreach (var item in dataList)
			{
				if (string.IsNullOrEmpty(string.Concat(item.paf_num))) continue;

				var contract = _dataPlatformRepository.GetContractByCode(item.contract_code);
				var recmanager = userId;
				if (contract != null)
				{
					item.contract_code_json = JsonHelper.ToJson(new DataSourceInfo() { id = contract.RecId, name = contract.RecName });
					recmanager = contract.Recmanager;
				}
				else
				{
					continue;
				}
				var invoicingperson = userId;
				var deptId = "7f74192d-b937-403f-ac2a-8be34714278b";
				if (!string.IsNullOrEmpty(item.applicant_num))
				{
					var u = _dataPlatformRepository.GetUserIdByWorkcode(item.applicant_num);
					if(u != null)
					{
						invoicingperson = u.UserId;
						deptId = u.DeptId.ToString();
					}
					else
					{
						continue;
					}
				}
				else
				{
					continue;
				}
				Dictionary<string, object> dic = new Dictionary<string, object>();
				dic.Add("invoicingcode", item.paf_num);
				dic.Add("invoicingperson", invoicingperson);
				dic.Add("workcode", item.applicant_num);
				dic.Add("department", deptId);
				dic.Add("invoicingdate", item.invoice_date);
				dic.Add("contract", item.contract_code_json);
				dic.Add("invoicingamount", item.money);
				dic.Add("recmanager", recmanager);
				dic.Add("recstatus", 1);

				var recId = _dataPlatformRepository.IsExitBill(item.paf_num);
				if (recId != Guid.Empty)
				{
					_dynamicEntityRepository.DynamicEdit(null, entityId, recId, dic, userId);
				}
				else
				{
					_dynamicEntityRepository.DynamicAdd(null, entityId, dic, null, userId);
				}
			}
		}
		#endregion

		#region 同步回款
		public void InitPayDataQrtz()
		{
			var init = false;
			InitPayData(init);
		}

		public dynamic InitPayData(bool init = false)
		{
			var userId = 1;
			var date = DateTime.Now.AddDays(_synBeginDateDay);
			if (init)
			{
				date = DateTime.MinValue;
			}
			var dataList = _dataPlatformRepository.GetPayData(date);
			ProcessPayData(userId, dataList);

			return null;
		}

		private void ProcessPayData(int userId, List<SavePayInfo> dataList)
		{
			var entityId = new Guid("be7e7291-6984-42b1-b61b-03d176efff36");
			foreach (var item in dataList)
			{
				if (string.IsNullOrEmpty(string.Concat(item.bill_code))) continue;

				var contract = _dataPlatformRepository.GetContractByCode(item.contract_code);
				var recmanager = userId;
				if (contract != null)
				{
					item.contract_code_json = JsonHelper.ToJson(new DataSourceInfo() { id = contract.RecId, name = contract.RecName });
					recmanager = contract.Recmanager;
				}
				else
				{
					continue;
				}
				Dictionary<string, object> dic = new Dictionary<string, object>();
				dic.Add("recname", item.bill_code);
				dic.Add("paidtime", item.receivable_date);
				dic.Add("contract", item.contract_code_json);
				dic.Add("paidmoney", item.distribute_amount);
				dic.Add("recmanager", recmanager);
				dic.Add("recstatus", 1);

				var recId = _dataPlatformRepository.IsExitPay(item.bill_code);
				if (recId != Guid.Empty)
				{
					_dynamicEntityRepository.DynamicEdit(null, entityId, recId, dic, userId);
				}
				else
				{
					_dynamicEntityRepository.DynamicAdd(null, entityId, dic, null, userId);
				}
			}
		}
		#endregion

		#region 同步招待费
		public void InitZdDataQrtz()
		{
			var init = false;
			InitZdData(init);
		}

		public dynamic InitZdData(bool init = false)
		{
			var userId = 1;
			var date = DateTime.Now.AddDays(_synBeginDateDay);
			if (init)
			{
				date = DateTime.MinValue;
			}
			var dataList = _dataPlatformRepository.GetZdData(date);
			ProcessZdData(userId, dataList);

			return null;
		}

		private void ProcessZdData(int userId, List<SaveZdInfo> dataList)
		{
			var entityId = new Guid("a4120e63-4339-447f-9b03-9ba143bacb00");
			foreach (var item in dataList)
			{
				if (string.IsNullOrEmpty(string.Concat(item.number))) continue;

				var enperson = userId;
				var deptId = "7f74192d-b937-403f-ac2a-8be34714278b";
				if (!string.IsNullOrEmpty(item.appliernumber))
				{
					var u = _dataPlatformRepository.GetUserIdByWorkcode(item.appliernumber);
					if (u != null)
					{
						enperson = u.UserId;
						deptId = u.DeptId.ToString();
					}
					else
					{
						continue;
					}
				}
				else
				{
					continue;
				}
				Dictionary<string, object> dic = new Dictionary<string, object>();
				dic.Add("encode", item.number);
				dic.Add("enperson", enperson);
				dic.Add("workcode", item.appliernumber);
				dic.Add("department", deptId);
				dic.Add("endate", item.trave_date);
				dic.Add("entertainamount", item.amount);
				dic.Add("recmanager", enperson);
				dic.Add("recstatus", 1);

				var recId = _dataPlatformRepository.IsExitZd(item.number);
				if (recId != Guid.Empty)
				{
					_dynamicEntityRepository.DynamicEdit(null, entityId, recId, dic, userId);
				}
				else
				{
					_dynamicEntityRepository.DynamicAdd(null, entityId, dic, null, userId);
				}
			}
		}
		#endregion

		#region 同步差旅费
		public void InitClDataQrtz()
		{
			var init = false;
			InitClData(init);
		}

		public dynamic InitClData(bool init = false)
		{
			var userId = 1;
			var date = DateTime.Now.AddDays(_synBeginDateDay);
			if (init)
			{
				date = DateTime.MinValue;
			}
			var dataList = _dataPlatformRepository.GetClData(date);
			ProcessClData(userId, dataList);

			return null;
		}

		private void ProcessClData(int userId, List<SaveClInfo> dataList)
		{
			var entityId = new Guid("48ac6537-f175-4db7-8a4a-2be53e7be20b");
			foreach (var item in dataList)
			{
				if (string.IsNullOrEmpty(string.Concat(item.number))) continue;

				var enperson = userId;
				var deptId = "7f74192d-b937-403f-ac2a-8be34714278b";
				if (!string.IsNullOrEmpty(item.appliernumber))
				{
					var u = _dataPlatformRepository.GetUserIdByWorkcode(item.appliernumber);
					if (u != null)
					{
						enperson = u.UserId;
						deptId = u.DeptId.ToString();
					}
					else
					{
						continue;
					}
				}
				else
				{
					continue;
				}
				Dictionary<string, object> dic = new Dictionary<string, object>();
				dic.Add("travelcode", item.number);
				dic.Add("person", enperson);
				dic.Add("workcode", item.appliernumber);
				dic.Add("department", deptId);
				dic.Add("traveldate", item.bizdate);
				dic.Add("travelamount", item.amount);
				dic.Add("recmanager", enperson);
				dic.Add("recstatus", 1);

				var recId = _dataPlatformRepository.IsExitCl(item.number);
				if (recId != Guid.Empty)
				{
					_dynamicEntityRepository.DynamicEdit(null, entityId, recId, dic, userId);
				}
				else
				{
					_dynamicEntityRepository.DynamicAdd(null, entityId, dic, null, userId);
				}
			}
		}
		#endregion
	}
}
