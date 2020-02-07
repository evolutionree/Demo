using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.WorkFlow
{
    /// <summary>
    /// 0普通审批 1会审
    /// </summary>
    public enum NodeType
    {
        /// <summary>
        /// 0普通审批
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 1会审
        /// </summary>
        Joint = 1,
        /// <summary>
        /// 意见征集
        /// </summary>
        SpecialJoint = 2,
    }

    /// <summary>
    /// 流程类型 0自由流程 1固定流程
    /// </summary>
    public enum WorkFlowType
    {
        FreeFlow=0,
        FixedFlow=1,

    }
    /// <summary>
    /// 流程节点审批状态
    /// </summary>
    public enum CaseStatusType
    {
        //0:未处理; 1:已读; 2:已处理 3作废
        WaitApproval=0,
        Readed=1,
        Approved=2,
        Invalid=3
    }

    /// <summary>
    /// 流程节点操作类型
    /// </summary>
    public enum ChoiceStatusType
    {
        /// <summary>
        /// 0拒绝
        /// </summary>
        Refused = 0,
        /// <summary>
        /// 1通过
        /// </summary>
        Approval = 1,
        /// <summary>
        /// 2退回
        /// </summary>
        Reback = 2,
        /// <summary>
        /// 3中止
        /// </summary>
        Stop = 3,
        /// <summary>
        /// 4编辑
        /// </summary>
        Edit = 4,
        /// <summary>
        /// 流程终点（系统自动触发）
        /// </summary>
        EndPoint = 5,
        /// <summary>
        /// 新增节点
        /// </summary>
        AddNode = 6,
        /// <summary>
        /// 转办
        /// </summary>
        Transfer = 7,
        /// <summary>
        /// 加签
        /// </summary>
        Sign = 8,
        /// <summary>
        /// 回撤
        /// </summary>
        WithDraw = 9,
        /// <summary>
        /// 驳回原节点
        /// </summary>
        RejectNode = 10
    }

    /// <summary>
    /// 审批状态 0审批中 1通过 2不通过 3发起审批
    /// </summary>
    public enum AuditStatusType
    {
        /// <summary>
        /// 0审批中
        /// </summary>
        Approving = 0,
        /// <summary>
        /// 1完成通过
        /// </summary>
        Finished = 1,
        /// <summary>
        /// 2不通过
        /// </summary>
        NotAllowed = 2,
        /// <summary>
        /// 3发起审批
        /// </summary>
        Begin = 3
    }

    public enum NodeStepType
    {
        /// <summary>
        /// -1:结束审批,
        /// </summary>
        End = -1,
        /// <summary>
        /// 0:发起审批,
        /// </summary>
        Launch = 0,
        /// <summary>
        /// 1:让用户自己选择审批人
        /// </summary>
        SelectByUser = 1,
        /// <summary>
        /// 2:指定审批人, 
        /// </summary>
        SpecifyApprover = 2,
        /// <summary>
        /// 3:会审,
        /// </summary>
        Joint = 3,
        /// <summary>
        /// 4:指定审批人的角色(特定),
        /// </summary>
        SpecifyRole = 4,
        /// <summary>
        /// 5:指定审批人所在团队(特定),
        /// </summary>
        SpecifyDepartment = 5,
        /// <summary>
        /// 6:指定审批人所在团队及角色(特定),
        /// </summary>
        SpecifyDepartment_Role = 6,
        /// <summary>
        /// 7:流程发起人,
        /// </summary>
        WorkFlowCreator = 7,

        #region -- 8XX 指定审批人所在团队-用户所在部门--
        /// <summary>
        /// 8:指定审批人所在团队-用户所在部门-上一步处理人,
        /// </summary>
        ApproverDept = 8,

        /// <summary>
        /// 801:指定审批人所在团队-用户所在部门-流程发起人,
        /// </summary>
        ApproverDept_Launcher = 801,

        /// <summary>
        /// 802:指定审批人所在团队-用户所在部门-表单中选人控件,
        /// </summary>
        ApproverDept_Select = 802,
        #endregion

        #region -- 9XX 指定审批人所在团队及角色-用户所在部门--
        /// <summary>
        /// 9:指定审批人所在团队及角色-用户所在部门-上一步处理人,
        /// </summary>
        ApproverDept_Role = 9,
        /// <summary>
        /// 901:指定审批人所在团队及角色-用户所在部门-流程发起人,
        /// </summary>
        ApproverDept_Role_Launcher = 901,

        /// <summary>
        /// 902:指定审批人所在团队及角色-用户所在部门-表单中选人控件,
        /// </summary>
        ApproverDept_Role_Select = 902,
        #endregion

        #region -- 10X 指定审批人所在团队及角色-用户所在部门的上级部门 --
        /// <summary>
        /// 10:指定审批人所在团队及角色-用户所在部门的上级部门-上一步处理人
        /// </summary>
        ApproverPreDept_Role = 10,
        /// <summary>
        /// 101:指定审批人所在团队及角色-用户所在部门的上级部门-流程发起人,
        /// </summary>
        ApproverPreDept_Role_Launcher = 101,

        /// <summary>
        /// 102:指定审批人所在团队及角色-用户所在部门的上级部门-表单中选人控件,
        /// </summary>
        ApproverPreDept_Role_Select = 102, 
        #endregion

        #region -- 11X 指定审批人所在团队-用户所在部门的上级部门--
        /// <summary>
        /// 11:指定审批人所在团队-用户所在部门的上级部门-上一步处理人,
        /// </summary>
        ApproverPreDepatrment = 11,

        /// <summary>
        /// 111:指定审批人所在团队-用户所在部门的上级部门-流程发起人,
        /// </summary>
        ApproverPreDepatrment_Launcher = 111,


        /// <summary>
        /// 112:指定审批人所在团队-用户所在部门的上级部门-表单中选人控件,
        /// </summary>
        ApproverPreDepatrment_Select = 112,
        #endregion

        #region -- 12X 指定审批人所在团队-用户管辖部门--
        /// <summary>
        /// 12:指定审批人所在团队-用户管辖部门-上一步处理人,
        /// </summary>
        ApproverDeptWidthChild = 12,

        /// <summary>
        /// 121:指定审批人所在团队-用户管辖部门-流程发起人,
        /// </summary>
        ApproverDeptWidthChild_Launcher = 121,


        /// <summary>
        /// 122:指定审批人所在团队-用户管辖部门-表单中选人控件,
        /// </summary>
        ApproverDeptWidthChild_Select = 122,
        #endregion

        #region -- 13X 指定审批人所在团队及角色-用户管辖部门 --
        /// <summary>
        /// 13:指定审批人所在团队及角色-用户管辖部门-上一步处理人
        /// </summary>
        ApproverDeptWidthChild_Role = 13,
        /// <summary>
        /// 131:指定审批人所在团队及角色-用户管辖部门-流程发起人,
        /// </summary>
        ApproverDeptWidthChild_Role_Launcher = 131,

        /// <summary>
        /// 132:指定审批人所在团队及角色-用户管辖部门-表单中选人控件,
        /// </summary>
        ApproverDeptWidthChild_Role_Select = 132,
        #endregion

        #region 14X 指定表单中的团队字段

        FormDeptGroup = 116,
        FormDeptGroupForRole = 106,

        #endregion
        #region 15X 指定汇报关系

        ReportRelation = 15,

        #endregion

        #region 16X 指定函数

        Function = 16,

        #endregion

        #region 17X 抄送人

        CPUser = 17,

        #endregion
    }



}
