using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.ZGQY.Model;
using UBeat.Crm.CoreApi.ZGQY.Repository;

namespace UBeat.Crm.CoreApi.ZGQY.Services
{
	public class DataPlatformServices
	{
		DataPlatformRepository _dataPlatformRepository;
		public DataPlatformServices(DataPlatformRepository dataPlatformRepository)
		{
			_dataPlatformRepository = dataPlatformRepository;
		}

		#region 同步项目
		public void InitProjectDataQrtz()
		{
			InitProjectData();
		}

		public dynamic InitProjectData()
		{
			var userId = 1;
			var date = DateTime.Now;
			var projectList = _dataPlatformRepository.GetProejctData(date);
			ProcessProejctData(userId, projectList); 

			return null;
		}

		private void ProcessProejctData(int userId, List<SaveProjectInfo> dicList)
		{
			var entityId = new Guid("b560917d-ff76-41df-a0de-04ddea396572");
			foreach (var item in dicList)
			{
				 
			}
		}
		#endregion
	}
}
