using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.ZGQY.Model;

namespace UBeat.Crm.CoreApi.ZGQY.Repository
{
	public interface IDataPlatformRepository
	{
		List<SaveProjectInfo> GetProejctData(DateTime date);
		QueryModelInfo GetContractByCode(string code);
		Guid IsExitProject(String code);
	}
}
