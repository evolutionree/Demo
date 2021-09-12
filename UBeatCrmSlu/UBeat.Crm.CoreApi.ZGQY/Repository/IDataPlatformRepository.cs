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

		List<SaveBillInfo> GetBillData(DateTime date);
		Guid IsExitBill(String code);
		QueryUserInfo GetUserIdByWorkcode(string code);

		List<SavePayInfo> GetPayData(DateTime date);
		Guid IsExitPay(String code);

		List<SaveZdInfo> GetZdData(DateTime date);
		Guid IsExitZd(String code);

		List<SaveClInfo> GetClData(DateTime date);
		Guid IsExitCl(String code);
	}
}
