using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.WorkFlow
{
   public  class WorkFlowEventInfo
    {
        public string FuncName { set; get; }
        /// <summary>
        /// 0为caseitemadd执行 1为caseitemaudit执行
        /// </summary>
        public int StepType { set; get; }

        
    }
}
