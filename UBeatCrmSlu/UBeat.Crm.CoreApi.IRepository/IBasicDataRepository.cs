using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.BasicData;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IBasicDataRepository : IBaseRepository
    {
        Dictionary<string, List<IDictionary<string, object>>> GetMessageList(PageParam pageParam, NotifyMessageMapper searchParam, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> SyncData(SyncDataMapper versionMapper, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> SyncDataBasic(SyncDataMapper versionMapper, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> SyncDataEntity(SyncDataMapper versionMapper, int userNumber);

        List<IDictionary<string, object>> SyncDelDataEntity(int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> SyncDataView(SyncDataMapper versionMapper, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> SyncDataTemplate(SyncDataMapper versionMapper, int userNumber);

        List<IDictionary<string, object>> DeptData(DeptDataMapper deptMapper, int userNumber);
        List<IDictionary<string, object>> DeptPowerData(DeptDataMapper deptModel, int userNumber);
        Dictionary<string, List<IDictionary<string, object>>> UserContactList(PageParam pageParam, BasicDataUserContactListMapper searchParm, int userNumber);

        dynamic FuncCount(int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> FuncCountList(PageParam pageParam, Guid anaFuncId, int userNumber);



        Dictionary<string, List<IDictionary<string, object>>> AnalyseFuncQuery(AnalyseListMapper entity, int userNumber);

        OperateResult InsertAnalyseFunc(AddAnalyseMapper entity, int userNumber);

        OperateResult UpdateAnalyseFunc(EditAnalyseMapper entity, int userNumber);

        OperateResult DisabledAnalyseFunc(DisabledOrOderbyAnalyseMapper entity, int userNumber);

        OperateResult OrderByAnalyseFunc(DisabledOrOderbyAnalyseMapper entity, int userNumber);

    }
}