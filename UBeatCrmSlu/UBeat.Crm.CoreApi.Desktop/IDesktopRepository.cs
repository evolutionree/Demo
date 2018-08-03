using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Desktop
{
    public interface IDesktopRepository : IBaseRepository
    {
        DesktopMapper GetDesktop(int userId);

         OperateResult SaveDesktopComponent(DesktopComponentMapper mapper, IDbTransaction trans = null);

         OperateResult EnableDesktopComponent(DesktopComponentMapper mapper, IDbTransaction trans = null);
    }
}
