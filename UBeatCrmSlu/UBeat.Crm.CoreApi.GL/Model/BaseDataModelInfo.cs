using System;
using System.Collections.Generic;

namespace UBeat.Crm.CoreApi.GL.Model
{
    public class BaseDataModelInfo
    {
        public BaseDataModelInfo()
        {

        }
    }

    public class QueryBaseDataModel
    {
        public List<Int32> DicTypeId { get; set; }
    }

    public enum DicTypeEnum
    {
        None = 0,
        客户类型=53,
        币种 = 54,
        分销渠道 = 62,
        销售组织 = 63,
        销售办事处 = 64,
        产品组 = 65,
        工厂 = 66,
        销售地区 = 67,
        物料类型 = 68,
        订单类型 = 69,
        库位 = 70,
        付款条件 = 55,
        订单原因 = 60,
        拒绝原因 = 61,
        成本中心 = 83,
        特殊处理标识 = 71,
        行项目类别 = 72,
        国家 = 85,
        客户账户组 = 86,
        税分类 = 87,
        装运条件 =88,
        销售组=89,
        地区 = 90,
        客户行业 =91
    }

    public class DataSourceInfo
    {
        public DataSourceInfo()
        {
            id = Guid.Empty;
            code = string.Empty;
            name = string.Empty;
        }

        public Guid id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }

    public class SaveDataSourceInfo
    {
        public Guid id { get; set; } 
        public string name { get; set; }
    }

    public class SimpleUserInfo
    {
        public Int32 userid { get; set; }
        public string username { get; set; }
        public string workcode { get; set; }
    }

    public class SimpleProductnfo
    {
        public Guid productid { get; set; }
        public string productcode { get; set; }
        public string productmodel { get; set; } 
    }

    public enum Status
    {
        Disable = 0,
        Enable = 1
    }

    public class Address
    {
        public Address()
        {
            lat = 0;
            lon = 0;
        }
        public decimal lat { get; set; }
        public decimal lon { get; set; }
        public string address { get; set; }
    }

    public class SaveAddress
    {
        public SaveAddress()
        { 
        } 
        public string address { get; set; }
    }

    public enum SapOptType
    {
        I = 0,
        U = 1,
        D = 2,
    }

    public enum SynchrosapStatus
    {
        Yes = 1,
        No = 2,
        Syncing = 3,
        Modify = 4
    }

    public enum Sapsynchstatus
    {
        //行项目同步状态
        No = 1,
        Yes = 2,
        Modify = 3
    }

    public static class MyExtensions
    {
        public static string StringMax(this string obj, int start, int length)
        {
            if (string.IsNullOrEmpty(obj)) return string.Empty;
            if (start > length) return string.Empty;
            if (obj.Length < start) return string.Empty;
            if (obj.Length < (length - start)) return obj;

            var result = obj.Substring(start, length);

            return result;
        }
        public static string StringSubMax(this string obj, int start, int length)
        {
            try {
                if (string.IsNullOrEmpty(obj)) return string.Empty;
                if (obj.Length < start) return string.Empty;
                if (obj.Length < (length + start))
                {
                    length = obj.Length - start;
                }

                var result = obj.Substring(start, length);

                return result;
            }
            catch (Exception ex) {
                Console.WriteLine("StringSubMax:" + ex.Message);
                return string.Empty;
            }
        }

        public static string StringRemoveStartChar(this string obj, char str)
        {
            if (string.IsNullOrEmpty(obj)) return string.Empty;
            var result = obj;
            while (result.StartsWith(str))
            {
                result = result.Substring(1, result.Length - 1);
            }

            return result;
        }
    }

    public class RegionCityClass
    {
        public Int32 RegionId { get; set; }
        public string RegionName { get; set; }
        public string FullName { get; set; }
    }

    public class EntityReg
    {
        public static Guid OrderEntityId()
        {
            return new Guid("817200fc-33c4-4689-8d0b-ea4cc460f13a");
        } 
		public static Guid CustomerEntityId()
		{
			return new Guid("f9db9d79-e94b-4678-a5cc-aa6e281c1246");
		}
		public static Guid ProjectEntityId()
		{
			return new Guid("2c63b681-1de9-41b7-9f98-4cf26fd37ef1");
		}
	}

	public enum CreateFrom
	{
		SAP创建 = 1,
		CRM创建 = 2
	}

	public enum IsSynchroSap
	{
		是 = 1,
		否 = 2,
		同步中 = 3,
		修改待同步 = 4
	}
}
