using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using UBeat.Crm.CoreApi.DomainModel.OpreateLog;

namespace UBeat.Crm.CoreApi.Services.Models.OperateLog
{
    public class OperateLogProfile:Profile
    {
        public OperateLogProfile()
        {
            CreateMap<OperateLogRecordListModel, OperateLogRecordListMapper>();
        }
    }
}
