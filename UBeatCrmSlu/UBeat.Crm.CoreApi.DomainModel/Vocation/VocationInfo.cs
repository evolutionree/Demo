using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Vocation
{
    public class VocationInfo
    {
        /// <summary>
        /// 职能ID
        /// </summary>
        public Guid VocationId { set; get; }

        /// <summary>
        /// 职能名称
        /// </summary>
        public string VocationName { set; get; }


        /// <summary>
        /// 职能描述
        /// </summary>
        public string Description { set; get; }

       
        public List<FunctionInfo> Functions { set; get; } = new List<FunctionInfo>();
    }

	public class ComRuleContent
	{
		public Guid EntityId { get; set; }
		public string RuleName { get; set; }
		public string RuleSql { get; set; }
		public Guid? RuleId { get; set; }
	}

	public class ComRuleItemModel
	{
		public Guid ItemId { get; set; }
		public int ControlType { get; set; }
		public string ItemName { get; set; }
		public string EntityId { get; set; }
		public Guid FieldId { get; set; }
		public string Operate { get; set; }

		public string RuleData { get; set; }
		public int RuleType { get; set; }
		public string RuleSql { get; set; }
		public int UseType { get; set; }
		public ComRuleItemRelationModel Relation { get; set; }
	}

	public class ComRuleItemRelationModel
	{
		public Guid ItemId { get; set; }
		public string RuleId { get; set; }
		public int UserId { get; set; }
		public int RoleSub { get; set; }
		public int ParamIndex { get; set; }
	}

	public class ComRuleSetModel
	{
		public Guid RuleId { get; set; }
		public string RuleSet { get; set; }
		public int UserId { get; set; }
		public string RuleFormat { get; set; }
	}

	public class RelTabRuleSaveModel
	{
		public bool IsAdd { get; set; }
		public Guid RelTabId { get; set; }
		public Guid EntityId { get; set; }

		public ComRuleContent Rule { get; set; }
		public ICollection<ComRuleItemModel> RuleItems { get; set; }
		public ComRuleSetModel RuleSet { get; set; }
	}
}
