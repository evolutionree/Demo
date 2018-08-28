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
        IList<DesktopMapper> GetDesktops(SearchDesktopMapper mapper,int userId);
        OperateResult SaveDesktopComponent(DesktopComponentMapper mapper, IDbTransaction trans = null);

        OperateResult EnableDesktopComponent(DesktopComponentMapper mapper, IDbTransaction trans = null);

        DesktopComponentMapper GetDesktopComponentDetail(Guid dsComponetId);
        IList<DesktopComponentMapper> GetDesktopComponents(SearchDesktopComponentMapper mapper,int userId);
        OperateResult SaveDesktop(DesktopMapper mapper, IDbTransaction trans = null);

        OperateResult EnableDesktop(DesktopMapper mapper, IDbTransaction trans = null);

        DesktopMapper GetDesktopDetail(Guid desktopId);

        OperateResult SaveDesktopRoleRelation(List<DesktopRoleRelationMapper> mapper, IDbTransaction trans = null);

        IList<dynamic> GetRoles(int userId);


        PageDataInfo<UBeat.Crm.CoreApi.DomainModel.Dynamics.DynamicInfoExt> GetDynamicList(DynamicListRequestMapper mapper, int userId);


        IList<dynamic> GetMainEntityList(int userId);


        IList<dynamic> GetRelatedEntityList(Guid entityid, int userId);


    }
}
