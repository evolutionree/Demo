using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.StatisticsSetting;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IStatisticsSettingRepository
    {
        List<Dictionary<string, object>> GetStatisticsListData(QueryStatisticsSettingMapper mapper, DbTransaction dbTran, int userId);
        OperateResult AddStatisticsSetting(AddStatisticsSettingMapper add, DbTransaction dbTran, int userId);
        OperateResult UpdateStatisticsSetting(EditStatisticsSettingMapper edit, DbTransaction dbTran, int userId);
        OperateResult DeleteStatisticsSetting(DeleteStatisticsSettingMapper delete, DbTransaction dbTran, int userId);
        OperateResult DisabledStatisticsSetting(DeleteStatisticsSettingMapper delete, DbTransaction dbTran, int userId);



        List<Dictionary<string, object>> GetStatisticsData(QueryStatisticsMapper mapper, DbTransaction dbTran, int userId);

        List<Dictionary<string, object>> GetStatisticsDetailData(QueryStatisticsMapper mapper, DbTransaction dbTran, int userId);

        OperateResult UpdateStatisticsGroupSetting(EditStatisticsGroupMapper edit, DbTransaction dbTran, int userId);

        OperateResult SaveStatisticsGroupSumSetting(SaveStatisticsGroupMapper save, DbTransaction dbTran, int userId);
    }
}
