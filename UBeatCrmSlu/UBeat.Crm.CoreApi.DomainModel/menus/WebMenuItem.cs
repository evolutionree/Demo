using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel.menus
{
    public class WebMenuItem
    {
        public Guid Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> Name_Lang { get; set; }
        public string Icon { get; set; }
        public string path { get; set; }
        public string FuncID { get; set; }
        public Guid ParentId { get; set; }
        public int IsDynamic { get; set; }
        public int IsLogicMenu { get; set; }
        /// <summary>
        /// 此字段不保存，仅用于返回个人默认页面
        /// </summary>
        public int IsDefaultPage { get; set; }
        public List<WebMenuItem> ChildRen { get; set; }
        public String ReportName { get; set; }
        public String ReferReportUrl { get; set; }
        public int ReportType { get; set; }
        public WebMenuItem()
        {
            this.ChildRen = new List<WebMenuItem>();
        }
        public static WebMenuItem parseFromDict(IDictionary<string, object> dict)
        {
            WebMenuItem retItem = new WebMenuItem();
            if (dict.ContainsKey("id") && dict["id"] != null)
            {
                retItem.Id = Guid.Parse(dict["id"].ToString());
            }
            if (dict.ContainsKey("name") && dict["name"] != null)
            {
                retItem.Name = (string)dict["name"];
            }
            if (dict.ContainsKey("index") && dict["index"] != null)
            {
                retItem.Index = Int32.Parse(dict["index"].ToString());
            }
            if (dict.ContainsKey("icon") && dict["icon"] != null)
            {
                retItem.Icon = (string)dict["icon"];
            }
            if (dict.ContainsKey("path") && dict["path"] != null)
            {
                retItem.path = (string)dict["path"];
            }
            if (dict.ContainsKey("funcid") && dict["funcid"] != null)
            {
                retItem.FuncID = (string)dict["funcid"];
            }
            if (dict.ContainsKey("parentid") && dict["parentid"] != null)
            {
                retItem.ParentId = Guid.Parse(dict["parentid"].ToString());
            }
            if (dict.ContainsKey("isdynamic") && dict["isdynamic"] != null)
            {
                retItem.IsDynamic = Int32.Parse(dict["isdynamic"].ToString());
            }
            if (dict.ContainsKey("reportname") && dict["reportname"] != null)
            {
                retItem.ReportName =dict["reportname"].ToString();
            }
            if (dict.ContainsKey("reporttype") && dict["reporttype"] != null)
            {
                retItem.ReportType = Int32.Parse(dict["reporttype"].ToString());
            }
            if (dict.ContainsKey("referreporturl") && dict["referreporturl"] != null)
            {
                retItem.ReferReportUrl = dict["referreporturl"].ToString();
            }
            return retItem;
        }

    }

    public class WebMenuModel
    {
        public int Type { get; set; }
    }
}
