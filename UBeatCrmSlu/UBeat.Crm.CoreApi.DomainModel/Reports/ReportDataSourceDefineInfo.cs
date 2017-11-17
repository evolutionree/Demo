using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Reports
{
    public class ReportDataSourceDefineInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        /***
         * 1=普通SQL
         * 2=函数
         * 3=实体查询SQL（校验实体数据权限）
         * 4=外部数据源
         * 5=代码数据源
         * 目前仅支持1
         * */
        public int DstType { get; set; }
        public string Params { get; set; }
        public string DataSQL { get; set; }
        public static ReportDataSourceDefineInfo fromDict(IDictionary<string, object> dict)
        {
            ReportDataSourceDefineInfo ret = new ReportDataSourceDefineInfo();
            if (dict == null) return null;
            foreach (string key in dict.Keys)
            {
                string tmp = key.ToLower();
                if (tmp.Equals("id") || tmp.Equals("recid"))
                {
                    if (dict[key] is Guid)
                    {
                        ret.Id = (Guid)dict[key];
                    }
                    else
                    {
                        ret.Id = Guid.Parse(dict[key].ToString());
                    }
                }
                else if (tmp.Equals("name") || tmp.Equals("recname"))
                {
                    ret.Name = (string)dict[key];
                }
                else if (tmp.Equals("dstype"))
                {
                    ret.DstType = int.Parse( dict[key].ToString());
                }
                else if (tmp.Equals("params"))
                {
                    ret.Params = (string)dict[key];
                }
                else if (tmp.Equals("datasql".ToLower()))
                {
                    ret.DataSQL = (string)dict[key];
                }
            }
            return ret;
        }
    }
}
