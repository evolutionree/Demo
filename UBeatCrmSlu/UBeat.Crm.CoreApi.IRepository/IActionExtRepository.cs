using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.ActionExt;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IActionExtRepository
    {
        List<ActionExtModel> GetActionExtData();

        dynamic ExcuteActionExt(DbTransaction transaction, string funcname,object basicParamData, object preActionResult,object actionResult,int usernumber);

      

    }
}
