using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.StatisticsSetting
{

    public class AddStatisticsSettingModel
    {
        // public Guid AnaFuncId { get; set; }
        public String AnaFuncName { get; set; }
        public int MoreFlag { get; set; }
        public String CountFunc { get; set; }
        public String MoreFunc { get; set; }
        public Guid? EntityId { get; set; }
        public int AllowInto { get; set; }
        public string AnaFuncName_Lang { get; set; }

    }

    public class EditStatisticsSettingModel
    {
        public Guid AnaFuncId { get; set; }
        public String AnaFuncName { get; set; }
        public int MoreFlag { get; set; }
        public String CountFunc { get; set; }
        public String MoreFunc { get; set; }
        public Guid? EntityId { get; set; }
        public int AllowInto { get; set; }
        public string AnaFuncName_Lang { get; set; }

    }


    public class DeleteStatisticsSettingModel
    {
        public List<Guid> AnaFuncIds { get; set; }
        public int RecStatus { get; set; }

    }

    public class QueryStatisticsSettingModel
    {
        public String AnaFuncName { get; set; }
    }

    public class QueryStatisticsModel
    {
        public String AnaFuncName { get; set; }

        public String GroupName { get; set; }
    }


    public class EditStatisticsGroupModel
    {
        public String NewGroupName { get; set; }

        public String GroupName { get; set; }
        public String NewGroupName_Lang { get; set; }
    }

    public class SaveStatisticsGroupModel
    {
        public List<SaveStatisticsGroupSumModel> Data { get; set; }

        public int IsDel { get; set; }
    }
    public class SaveStatisticsGroupSumModel
    {
        public Guid? AnafuncId { get; set; }

        public int RecOrder { get; set; }

        public string GroupName_Lang { get; set; }
        public String GroupName { get; set; }
    }
}
