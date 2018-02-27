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
    }
}
