using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.ZGQY.Model;

namespace UBeat.Crm.CoreApi.ZGQY.Repository
{
	public class MiddleDbBaseRepository
	{
		 private static string _connectString;
        public static string GetDbConnectString()
        { 
            if (_connectString == null)
            {
                IConfigurationRoot config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
                _connectString = config.GetSection("MiddleServerConfig").Get<MiddleServerConfig>().DataBase;
            } 
            return _connectString;
        }

        private static string CONNECTION_STRING = GetDbConnectString(); 
        public static string ConnectionString { get { return CONNECTION_STRING; } }
	}
}
