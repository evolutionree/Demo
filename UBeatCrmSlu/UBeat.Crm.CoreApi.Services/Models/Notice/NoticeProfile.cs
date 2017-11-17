using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Notice;

namespace UBeat.Crm.CoreApi.Services.Models.Notice
{
    public class NoticeProfile : Profile
    {
        public NoticeProfile()
        {
            CreateMap<NoticeModel, NoticeMapper>();
            CreateMap<NoticeSendRecordModel, NoticeSendRecordMapper>();
            CreateMap<NoticeReceiverModel, NoticeReceiverMapper>();
            CreateMap<NoticeReceiverDeptModel, NoticeReceiverDeptMapper>();
            CreateMap<NoticeDisabledModel, NoticeDisabledMapper>();
            CreateMap<NoticeListModel, NoticeListMapper>();
            CreateMap<NoticeReadFlagModel, NoticeReadFlagMapper>();
        }
    }
}
