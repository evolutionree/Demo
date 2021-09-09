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
		public DataPlatformServices(IDataPlatformRepository dataPlatformRepository, IDynamicEntityRepository dynamicEntityRepository)
		{
			_dataPlatformRepository = dataPlatformRepository;
			_dynamicEntityRepository = dynamicEntityRepository;
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
			var date = DateTime.Now.AddDays(-60);
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
	}
}
