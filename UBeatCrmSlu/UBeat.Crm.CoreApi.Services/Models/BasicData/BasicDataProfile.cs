using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using UBeat.Crm.CoreApi.DomainModel.BasicData;
using UBeat.Crm.CoreApi.Services.Models.Notify;

namespace UBeat.Crm.CoreApi.Services.Models.BasicData
{
    public class BasicDataProfile:Profile
    {
        public BasicDataProfile()
        {
            CreateMap<BasicDataMessageModel, NotifyMessageMapper>();
            CreateMap<BasicDataSyncModel, SyncDataMapper>();
            CreateMap<BasicDataDeptModel, DeptDataMapper>();
            CreateMap<BasicDataUserContactListModel, BasicDataUserContactListMapper>();


            CreateMap<AnalyseListModel, AnalyseListMapper>();
            CreateMap<AddAnalyseModel, AddAnalyseMapper>();
            CreateMap<EditAnalyseModel, EditAnalyseMapper>();
            CreateMap<DisabledOrOderbyAnalyseModel, DisabledOrOderbyAnalyseMapper>();

        }
    }
}
