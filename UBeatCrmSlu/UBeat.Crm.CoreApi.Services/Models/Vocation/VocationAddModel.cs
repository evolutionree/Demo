using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using UBeat.Crm.CoreApi.Services.Models.Rule;

namespace UBeat.Crm.CoreApi.Services.Models.Vocation
{
    public class VocationSaveModel
    {
        public Guid? VocationId { get; set; }
        public string VocationName { get; set; }
        public string Description { get; set; }
        public Dictionary<string,string> VocationName_Lang { get; set; }


    }

    public class CopyVocationSaveModel
    {

        public string VocationName { get; set; }

        public string Description { get; set; }

        public Guid  VocationId { get; set; }

    }


    public class VocationDeleteModel
    {
        public List<Guid> VocationIds { get; set; }

    }


    public class VocationSelectModel
    {
        public string VocationName { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }



    public class VocationFunctionSelectModel
    {
        public Guid FuncId { get; set; }

        public Guid VocationId { get; set; }

        public int Direction { get; set; }
    }


    public enum TreeDirection
    {
        /// <summary>
        /// 向上
        /// </summary>
        Up = 0,

        /// <summary>
        /// 向下
        /// </summary>
        Down = 1

    }


    public class VocationFunctionEditModel
    {

        public Guid VocationId { get; set; }
        public List<FunctionJsonItem> FunctionJson { get; set; }

    }


    public class FunctionJsonItem
    {

        public Guid FunctionId { get; set; }

        public Guid ParentId { get; set; }

        public string FunctionCode { get; set; }

    }



    public class FunctionItem
    {
        public Guid FunctionId { get; set; }

        public string relationValue { get; set; }

    }



    public class RuleContent
    {
        public Guid EntityId { get; set; }
        public string RuleName { get; set; }
        public string RuleSql { get; set; }
        public Guid? RuleId { get; set; }
    }

    public class FunctionRuleAddModel
    {
        public Guid VocationId { get; set; }
        public Guid FunctionId { get; set; }
        public Guid EntityId { get; set; }
        public int SyncDevice { get; set; }

        public RuleContent Rule { get; set; }
        public ICollection<RuleItemModel> RuleItems { get; set; }
        public RuleSetModel RuleSet { get; set; }


    }


    public class FunctionRuleEditModel
    {
        public string Rule { get; set; }
        public string RuleItem { get; set; }
        public string RuleSet { get; set; }
    }


    public class FunctionRuleSelectModel
    {
        public Guid VocationId { get; set; }
        public Guid FunctionId { get; set; }
        public Guid EntityId { get; set; }

    }


    public class VocationUserSelectModel
    {

        public Guid VocationId { get; set; }

        public string UserName { get; set; }
        public Guid DeptId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }


    public class VocationUserDeleteModel
    {

        public Guid VocationId { get; set; }

        public List<int> UserIds { get; set; }
    }

    public class UserFunctionSelectModel
    {

        public int UserNumber { get; set; }
        public int DeviceType { get; set; }
        public Int64 Version { get; set; }

    }


    public class FunctionAddModel
    {
        public Guid? FuncId { get; set; }
        public Guid TopFuncId { get; set; }
        public string FuncName { get; set; }
        public string FuncCode { get; set; }
        public Guid? EntityId { get; set; }
        public int DeviceType { get; set; }


    }


    public class FunctionItemEditModel
    {
        public Guid FuncId { get; set; }
        public string FuncName { get; set; }
        public string FuncCode { get; set; }
    }


    public class FunctionItemDeleteModel
    {
        public Guid FuncId { get; set; }

    }
    public class FunctionTreeSelectModel
    {
        public Guid TopFuncId { get; set; }

        public int Direction { get; set; }
    }

    public class VocationRuleInfoModel
    {
        public string VocationId { get; set; }
        public string FunctionId { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public ICollection<RuleItemInfoModel> RuleItems { get; set; }
        public RuleSetInfoModel RuleSet { get; set; }
    }


    public enum FunctionType
    {
        /// <summary>
        /// 入口
        /// </summary>
        Entrance = 0,

        /// <summary>
        /// 实体
        /// </summary>
        Entity = 1,

        /// <summary>
        /// 菜单
        /// </summary>
        Menu = 2,

        /// <summary>
        /// 功能
        /// </summary>
        Function = 3,

        /// <summary>
        /// 页签
        /// </summary>
        Tab = 4,

        /// <summary>
        /// 动态
        /// </summary>
        Dynamic = 5,

        /// <summary>
        /// 页签下的功能
        /// </summary>
        TabFunction = 6,

        /// <summary>
        ///动态页签
        /// </summary>
        TabDynamic = 7

    }



    //end of class
}

