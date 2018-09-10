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
        IList<DesktopsMapper> GetDesktops(SearchDesktopMapper mapper,int userId);
        OperateResult SaveDesktopComponent(DesktopComponentMapper mapper, IDbTransaction trans = null);
        OperateResult SaveActualDesktopComponent(ActualDesktopComponentMapper mapper, IDbTransaction trans = null);
        OperateResult EnableDesktopComponent(DesktopComponentMapper mapper, IDbTransaction trans = null);

        DesktopComponentMapper GetDesktopComponentDetail(Guid dsComponetId);
        IList<DesktopComponentMapper> GetDesktopComponents(SearchDesktopComponentMapper mapper,int userId);
        OperateResult SaveDesktop(DesktopMapper mapper, IDbTransaction trans = null);

        OperateResult EnableDesktop(DesktopMapper mapper, IDbTransaction trans = null);
        OperateResult AssignComsToDesktop(ComToDesktopMapper mapper, int userId);
        OperateResult AssignComsToDesktop(ActualDesktopRelateToComMapper mapper, int userId, IDbTransaction dbTrans = null);
        ActualDesktopComMapper GetActualDesktopCom(Guid desktopId, int userId);

            DesktopMapper GetDesktopDetail(Guid desktopId);

        OperateResult SaveDesktopRoleRelation(List<DesktopRoleRelationMapper> mapper, IDbTransaction trans = null);

        IList<RoleRelationMapper> GetRoles(Guid desktopId, int userId);


        PageDataInfo<UBeat.Crm.CoreApi.DomainModel.Dynamics.DynamicInfoExt> GetDynamicList(DynamicListRequestMapper mapper, int userId);


        IList<dynamic> GetMainEntityList(int userId);


        IList<dynamic> GetRelatedEntityList(Guid entityid, int userId);


    }
}
