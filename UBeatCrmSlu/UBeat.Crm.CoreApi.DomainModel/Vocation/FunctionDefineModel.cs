using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Role;

namespace UBeat.Crm.CoreApi.DomainModel.Vocation
{
    public class FunctionDefineModel
    {
        public Guid FuncId { set; get; }

        public string FuncName { set; get; }


        public int Devicetype { set; get; }

        public int RecType { set; get; }

        public List<VocationRuleModel> VocationRules { set; get; } = new List<VocationRuleModel>();
    }


    public class VocationRuleModel
    {
        public Guid VocationId { set; get; }

        public Guid? RuleId { set; get; }
    }
    

   
}
