using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using UBeat.Crm.CoreApi.DomainModel.Contact;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Contact;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class ContactServices : BaseServices
    {
        private readonly FileServices _fileServices;
        private readonly IContactRepository _contactRepository;
        private readonly IMapper _mapper;

        public ContactServices(IMapper mapper, IContactRepository contactRepository, FileServices fileServices)
        {
            _contactRepository = contactRepository;
            _fileServices = fileServices;
            _mapper = mapper;
        }

        public OutputResult<object> VCardInfo(ContactVCardModel vcardModel, int userNumber)
        {
            if (vcardModel?.FileId == null)
            {
                return ShowError<object>("名片扫描文件ID不能为空");
            }
            //获取文件ID
            var fileData = _fileServices.GetFileData(vcardModel.CollectionName, vcardModel.FileId);
            if (fileData == null)
            {
                return ShowError<object>("无法获取名片数据");
            }
            var cardInfo = CamCardHelper.GetCardInfo(fileData);
            if (string.IsNullOrWhiteSpace(cardInfo))
            {
                return ShowError<object>("名片数据获取为空");
            }
            var cardModel = CamCardHelper.VCardTransfer(cardInfo);

            return new OutputResult<object>(cardModel);
        }

        public PageDataInfo<LinkManMapper> GetFlagLinkman(LinkManModel model, int userNumber)
        {
            var contactMapper = new ContactMapper()
            {
                SearchKey=model.SearchKey,
                PageIndex=model.PageIndex,
                PageSize=model.PageSize
            };
            return _contactRepository.GetFlagLinkman(contactMapper, userNumber);
        }

        public PageDataInfo<LinkManMapper> GetRecentCall(LinkManModel model, int userNumber)
        {
            var contactMapper = new ContactMapper()
            {
                SearchKey = model.SearchKey,
                PageIndex = model.PageIndex,
                PageSize = model.PageSize
            };
            return _contactRepository.GetRecentCall(contactMapper, userNumber);
        }

        public OutputResult<object> FlagLinkman(LinkManModel model, int userNumber)
        {
            var contactMapper = new ContactMapper()
            {
                userid = model.userid,
                flag = model.flag
            };
            return new OutputResult<object>(_contactRepository.FlagLinkman(contactMapper, userNumber));
        }

        public OutputResult<object> AddRecentCall(LinkManModel model, int userNumber)
        {
            var contactMapper = new ContactMapper()
            {
                userid = model.userid
            };
            return new OutputResult<object>(_contactRepository.AddRecentCall(contactMapper, userNumber));
        }
    }
}
