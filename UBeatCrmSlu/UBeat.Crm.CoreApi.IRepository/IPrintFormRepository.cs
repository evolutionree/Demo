using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.PrintForm;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IPrintFormRepository
    {
        Guid InsertTemplate(CrmSysEntityPrintTemplate model, DbTransaction tran = null);

        void SetTemplatesStatus(List<Guid> recids, int recstatus, int usernumber, DbTransaction tran = null);

        void UpdateTemplate(CrmSysEntityPrintTemplate model, DbTransaction tran = null);

        CrmSysEntityPrintTemplate GetTemplateInfo(Guid recid, DbTransaction tran = null);

        List<CrmSysEntityPrintTemplate> GetTemplateList(Guid entityid, int recstate, DbTransaction tran = null);

        /// <summary>
        /// 获取某条记录可以关联的所有模板文件
        /// </summary>
        List<CrmSysEntityPrintTemplate> GetRecDataTemplateList(Guid entityid, Guid businessid, int userno, DbTransaction tran = null);

        /// <summary>
        /// 获取打印数据源（通过函数处理）
        /// </summary>
        /// <param name="entityId">实体id</param>
        /// <param name="recId">记录id</param>
        /// <param name="dbFunction">函数名称</param>
        /// <param name="usernumber">当前操作人</param>
        /// <returns>返回数据已字典形式，如果不是实体中的字段，字典中的key必须和模板定义的字段匹配上</returns>
        IDictionary<string, object> GetPrintDetailDataByProc(Guid entityId, Guid recId, string dbFunction, int usernumber);

    }
}
