using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Services.Models.Rule;
using UBeat.Crm.CoreApi.Services.Models.Vocation;

namespace UBeat.Crm.CoreApi.Services.Models.SalesTarget
{

    public class SalesTargetSaveModel
    {

        /// <summary>
        ///  年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 一月
        /// </summary>
        public decimal JanCount { get; set; }


        /// <summary>
        /// 二月
        /// </summary>
        public decimal FebCount { get; set; }

        /// <summary>
        /// 三月
        /// </summary>
        public decimal MarCount { get; set; }

        /// <summary>
        /// 四月
        /// </summary>
        public decimal AprCount { get; set; }

        /// <summary>
        /// 五月
        /// </summary>
        public decimal MayCount { get; set; }

        /// <summary>
        /// 六月
        /// </summary>
        public decimal JunCount { get; set; }

        /// <summary>
        /// 七月
        /// </summary>
        public decimal JulCount { get; set; }

        /// <summary>
        /// 八月
        /// </summary>
        public decimal AugCount { get; set; }

        /// <summary>
        /// 九月
        /// </summary>
        public decimal SepCount { get; set; }

        /// <summary>
        /// 十月
        /// </summary>
        public decimal OctCount { get; set; }

        /// <summary>
        ///十一 月
        /// </summary>
        public decimal NovCount { get; set; }

        /// <summary>
        /// 十二月
        /// </summary>
        public decimal DecCount { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// 部门id
        /// </summary>
        public Guid DepartmentId { get; set; }

        /// <summary>
        /// 是否团队销售目标
        /// </summary>
        public bool IsGroupTarget { get; set; }

        /// <summary>
        /// 销售目标指标类型
        /// </summary>
        public Guid NormTypeId { get; set; }


    }



    public class SalesTargetSelectModel
    {
        public int Year { get; set; }
        public Guid NormTypeId { get; set; }
        public Guid DepartmentId { get; set; }
        public string SearchName { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }


    public class SalesTargetSelectDetailModel
    {
        public Guid DepartmentId { get; set; }
        public int UserId { get; set; }
        public Guid NormTypeId { get; set; }
        public bool IsGroupTarget { get; set; }
        public int Year { get; set; }


    }


    public class SalesTargetSetBeginMothModel
    {

        public DateTime BeginDate { get; set; }

        public Guid DepartmentId { get; set; }

        public int UserId { get; set; }
    }



    /// <summary>
    /// 销售指标类型
    /// </summary>
    public class SaleTargetNormTypeSaveModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }

        public Dictionary<string, string> NormTypeName_Lang { get; set; }

    }


    public class SaleTargetNormTypeDeleteModel
    {
        public Guid Id { get; set; }

    }


    public class SaleTargetNormTypeRuleSaveModel
    {

        public Guid NormId { get; set; }

        public string NormTypeName { get; set; }

        public Guid EntityId { get; set; }
        public string FieldName { get; set; }
        /// <summary>
        /// 业务日期
        /// </summary>
        public string BizDateFieldName { get; set; }

        public int CalcuteType { get; set; }

        public int TypeId { get; set; }//0 代表菜单 1代表角色 2代表动态实体
        public string Id { get; set; }// 代表规则关联的某实体唯一id
        public string RoleId { get; set; }
        public string MenuName { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }

        public string RelEntityId { get; set; }
        public string Rulesql { get; set; }


        public RuleContent Rule { get; set; }
        public ICollection<RuleItemModel> RuleItems { get; set; }
        public RuleSetModel RuleSet { get; set; }

    }


    public class SaleTargetNormRuleDetailModel
    {
        public Guid Id { get; set; }

    }



    public class SaleTargetEntityFieldSelect
    {
        public Guid EntityId { get; set; }
        public int FieldType { get; set; }//1=日期字段,其他表示统计字段

    }


    public class YearSalesTargetSaveModel
    {
        public string Id { get; set; }
        public int IsGroup { get; set; }
        public decimal YearCount { get; set; }
        public int Year { get; set; }
        public Guid NormTypeId { get; set; }
        public int UserNumber { get; set; }
        public string DepartmentId { get; set; }
    }


    public class SaleTargetDepartmentSelect
    {
        public Guid DepartmentId { get; set; }

    }


    public class YearSaleTargetSetlectModel
    {
        public string Id { get; set; }
        public int IsGroup { get; set; }
        public int Year { get; set; }
        public Guid NormTypeId { get; set; }
    }
}





