using System;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class AdAuthServices: BasicBaseServices
    {
        public AdAuthServices()
        {
        }

		public string CheckAdAuthByAccount(AdAuthModelInfo model)
		{
			var result = string.Empty;
			if (model == null)
				result = "请检查参数有效性";
			else
			{
				if (string.IsNullOrEmpty(model.ServerIp))
					result = "IP地址不能为空";

				if (model.ServerPort <= 0)
					result = "Port不能小于或者等于0";

				if (string.IsNullOrEmpty(model.AdminAccount))
					result = "管理员账号不能为空";

				if (string.IsNullOrEmpty(model.AdminAccount))
					result = "管理员密码不能为空";

				if (string.IsNullOrEmpty(model.AdminAccount))
					result = "账号不能为空";

				if (string.IsNullOrEmpty(model.AdminAccount))
					result = "密码不能为空";
			}

			if (!string.IsNullOrEmpty(result))
				return result;
             
			return CheckLogin389(model);
		}

		#region private method
		private string CheckLogin389(AdAuthModelInfo model)
		{
            LDAPUtil.Register(model.ServerIp, model.ServerPort,
                string.Format("CN={0},{1}", model.AdminAccount, model.BinDN),
                model.AdminPwd,
                model.BaseDN);
            var result = LDAPUtil.ValidateDotnetCore(model.Account, model.Pwd);
            if (string.IsNullOrEmpty(result))
                return "AD验证成功";
            else
                return result;          
		}
		#endregion
	}
}
