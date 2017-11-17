using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.SalesTarget;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface ISalesTargetRepository
    {

        /// <summary>
        /// 获取销售目标列表
        /// </summary>
        /// <returns></returns>
        dynamic GetSalesTargets(PageParam page, SalesTargetSelectMapper data, int userNumbe);



        /// <summary>
        /// 编辑销售目标
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        OperateResult EditSalesTarget(SalesTargetEditMapper data, int userNumber);



        /// <summary>
        /// 新增销售目标
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        OperateResult InsertSalesTarget(SalesTargetInsertMapper data, int userNumber);


        /// <summary>
        /// 获取销售目标明细
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumbe"></param>
        /// <returns></returns>
        dynamic GetSalesTargetDetail(SalesTargetSelectDetailMapper data, int userNumbe);

        /// <summary>
        /// 设置销售目标开始月份
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        OperateResult SetBeginMoth(SalesTargetSetBeginMothMapper data, int userNumber);



        /// <summary>
        /// 新增销售指标
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        OperateResult InsertSalesTargetNormType(SalesTargetNormTypeMapper data, int userNumber);


        /// <summary>
        /// 删除销售指标
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        OperateResult DeleteSalesTargetNormType(SalesTargetNormTypeDeleteMapper data, int userNumber);



        /// <summary>
        /// 获取销售指标列表
        /// </summary>
        /// <returns></returns>
        dynamic GetTargetNormTypeList();


        /// <summary>
        /// 保存销售指标列规则
        /// </summary>
        /// <param name="data"></param>
        /// <param name="normRule"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        OperateResult InsertSalesTargetNormTypeRule(SalesTargetNormTypeMapper data, SalesTargetNormRuleInsertMapper normRule, int userNumber);


        /// <summary>
        /// 编辑销售指标列规则
        /// </summary>
        /// <param name="data"></param>
        /// <param name="normRule"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        OperateResult EditSalesTargetNormTypeRule(SalesTargetNormTypeMapper data, SalesTargetNormRuleInsertMapper normRule, int userNumber);



        /// <summary>
        /// 获取销售指标明细
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumbe"></param>
        /// <returns></returns>
        List<SalesTargetNormRuleMapper> GetSalesTargetNormDetail(SalesTargetNormTypeDetailMapper data, int userNumbe);


        /// <summary>
        /// 获取实体列表
        /// </summary>
        /// <returns></returns>
        dynamic GetEntityList();

        /// <summary>
        /// 获取销售目标团队
        /// </summary>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        dynamic GetSalesTargetDept(int userNumber);

        /// <summary>
        /// 获取实体字段
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        dynamic GetEntityFields(Guid id,int fieldtype);

        /// <summary>
        /// 分配年度销售目标
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        OperateResult SaveYearSalesTarget(YearSalesTargetSaveMapper data, int userNumber);


        /// <summary>
        /// 获取下级团队和人员
        /// </summary>
        /// <param name="deptId"></param>
        /// <param name="userNumbe"></param>
        /// <returns></returns>
        dynamic GetSubDepartmentAndUser(Guid deptId, int userNumbe);



        /// <summary>
        ///  获取下级团队和人员的年度销售目标
        /// </summary>
        /// <param name="deptId"></param>
        /// <param name="selectYear"></param>
        /// <param name="normId"></param>
        /// <param name="userNumbe"></param>
        /// <returns></returns>
        dynamic GetSubDepartmentAndUserYearSalesTarget(string id, int isGroup, int year, Guid normTypeId, int userNumbe);


        /// <summary>
        /// 保存销售目标
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        OperateResult SaveSalesTarget(SalesTargetInsertMapper data, int userNumber);



        /// <summary>
        /// 销售目标是否已经存在
        /// </summary>
        /// <param name="id"></param>
        /// <param name="year"></param>
        /// <param name="isGroup"></param>
        /// <param name="normTypeId"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        bool IsSalesTargetExists(string id, int year, bool isGroup, Guid normTypeId, int userNumber);


        /// <summary>
        /// 更新销售目标
        /// </summary>
        /// <param name="id"></param>
        /// <param name="year"></param>
        /// <param name="isGroup"></param>
        /// <param name="normTypeId"></param>
        /// <param name="yearCount"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        bool UpdateSalesTarget(string id, int year, bool isGroup, Guid normTypeId, decimal yearCount, int userNumber);


        /// <summary>
        /// 新增销售目标
        /// </summary>
        /// <param name="id"></param>
        /// <param name="year"></param>
        /// <param name="isGroup"></param>
        /// <param name="normTypeId"></param>
        /// <param name="yearCount"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        bool InsertSalesTarget(string id, int year, bool isGroup, Guid normTypeId, decimal yearCount, int userNumber);

		dynamic GetCurYearALlTargetList(int year);
	}
}
