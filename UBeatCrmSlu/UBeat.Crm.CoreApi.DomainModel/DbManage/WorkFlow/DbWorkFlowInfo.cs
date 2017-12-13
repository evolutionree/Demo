using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.WorkFlow
{
    /// <summary>
    /// 数据库流程审批信息对象
    /// </summary>
    public class DbWorkFlowInfo
    {
        public CrmSysWorkflow WorkFlow { set; get; }

        public List<CrmSysWorkflowNode> WorkFlowNodes { set; get; }

    }

   
}
