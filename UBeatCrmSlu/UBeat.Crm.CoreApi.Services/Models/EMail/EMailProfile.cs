using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.EMail;

namespace UBeat.Crm.CoreApi.Services.Models.EMail
{
    public class EMailProfile : Profile
    {
        public EMailProfile()
        {
            CreateMap<SendEMailModel, SendEMailMapper>();
            CreateMap<AttachmentFileModel, AttachmentFileMapper>();
            CreateMap<MailAddressModel, MailAddressMapper>();
            CreateMap<ReceiveEMailModel, ReceiveEMailMapper>();
            CreateMap<TagMailModel, TagMailMapper>();
            CreateMap<DeleteMailModel, DeleteMailMapper>();
            CreateMap<ReadOrUnReadMailModel, ReadOrUnReadMailMapper>();
            CreateMap<MailDetailModel, MailDetailMapper>();

            CreateMap<ReadOrUnReadMailModel, ReadOrUnReadMailMapper>();

            CreateMap<MailAttachmentModel, MailAttachmentMapper>();
            CreateMap<TransferMailDataModel, TransferMailDataMapper>();

            CreateMap<MoveMailModel, MoveMailMapper>();
            CreateMap<ReConverMailModel, ReConverMailMapper>();

            CreateMap<ToAndFroModel, ToAndFroMapper>();

            CreateMap<AttachmentListModel, AttachmentListMapper>();

            CreateMap<TransferRecordParamModel, TransferRecordParamMapper>();
        }
    }
}
