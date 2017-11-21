using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.DomainModel.EMail;
using Microsoft.AspNetCore.Authorization;
using UBeat.Crm.CoreApi.Services.Models.EMail;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class MailSetController : BaseController
    {
        private readonly EMailServices _eMailServices;
        public MailSetController(EMailServices eMailServices) : base(eMailServices)
        {
            _eMailServices = eMailServices;
        }
        #region �ʼ�Ŀ¼����
        /// <summary>
        /// ��ȡ�ҵ�Ŀ¼��
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("catalogtree")]
        public OutputResult<object> GetCatalogTree([FromBody] CatalogModel dynamicModel = null) {
            int userid = UserId;
            if (dynamicModel != null && dynamicModel.SearchUserId != 0)
            {
                userid = dynamicModel.SearchUserId;
            }

            List<MailCatalogInfo> retList = _eMailServices.GetMailCataLog(dynamicModel.CatalogType, dynamicModel.CatalogName,userid);
            return new OutputResult<object>(retList);
        }

        /// <summary>
        /// ��ȡ�����Լ������ʼ�Ŀ¼�ӿ�
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getorgandstafftreebylevel")]
        public OutputResult<object> GetOrgAndStaffTreeByLevel([FromBody] OrgAndStaffTreeModel dynamicModel = null)
        {
            int userid = UserId;
            string deptId = "";
            if (dynamicModel != null)
                deptId = dynamicModel.treeId;
            List<OrgAndStaffTree> retList = _eMailServices.GetOrgAndStaffTreeByLevel(userid, deptId, dynamicModel.keyword);
            return new OutputResult<object>(retList);
        }

        /// <summary>
        /// ����catalogtype��ȡ��Ӧ�û���Ŀ¼
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getmailcatalogbycode")]
        public OutputResult<object> GetMailCatalogByCode([FromBody] UserCatalogModel dynamicModel = null)
        {
            int userid = UserId;
            if (dynamicModel == null) { return new OutputResult<object>("�����쳣", "�����쳣", -1); }
            MailCatalogInfo ret = _eMailServices.GetMailCatalogByCode(userid, dynamicModel.catalogType);
            return new OutputResult<object>(ret);
        }

        /// <summary>
        /// ����Ŀ¼�����ӿ�
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("insertpersonalcatalog")]
        public OutputResult<object> InsertPersonalCatalog([FromBody] AddCatalogModel dynamicModel = null)
        {
            int userid = UserId;
            if (dynamicModel == null) { return new OutputResult<object>("�����쳣", "�����쳣", -1); }
            OutputResult<object> ret = _eMailServices.InsertPersonalCatalog(userid, dynamicModel);
            return ret;
        }

        /// <summary>
        /// ����Ŀ¼ɾ���ӿ�
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("delpersonalcatalog")]
        public OutputResult<object> DelPersonalCatalog([FromBody] DelCatalogModel dynamicModel = null)
        {
            int userid = UserId;
            if (dynamicModel == null) { return new OutputResult<object>("�����쳣", "�����쳣", -1); }
            OutputResult<object> ret = _eMailServices.DelPersonalCatalog(userid, dynamicModel);
            return ret;
        }

        /// <summary>
        /// �༭�����û�Ŀ¼
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("updatepersonalcatalog")]
        public OutputResult<object> UpdatePersonalCatalog([FromBody] AddCatalogModel dynamicModel) {
            int userid = UserId;
            if (dynamicModel == null) { return new OutputResult<object>("�����쳣", "�����쳣", -1); }
            OutputResult<object> ret = _eMailServices.UpdatePersonalCatalog(userid, dynamicModel);
            return ret;
        }

        /// <summary>
        /// �ƶ�����Ŀ¼
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("movecatalog")]
        public OutputResult<object> MovePersonCatalog([FromBody] MoveCatalogModel paramInfo) {
            
            if (paramInfo == null) return new OutputResult<object>("�����쳣", "�����쳣", -1);
            return _eMailServices.MovePersonalCatalog(paramInfo, UserId);
        }

        [HttpPost]
        [Route("transfercatalog")]
        public OutputResult<object> TransferCatalog([FromBody] TransferCatalogModel paramInfo) {
            if (paramInfo == null) return new OutputResult<object>("�����쳣", "�����쳣", -1);
            return _eMailServices.TransferCatalog(paramInfo, UserId);
        }

        [HttpPost]
        [Route("toordercatalog")]
        public OutputResult<object> ToOrderCatalog([FromBody] OrderCatalogModel dynamicModel) {
            if (dynamicModel == null|| dynamicModel.doType==null|| dynamicModel.recId==null)
                new OutputResult<object>("�����쳣", "�����쳣", -1);
            return _eMailServices.ToOrderCatalog(dynamicModel);
        }

        #endregion

        #region ��������

        /// <summary>
        /// ��ȡ�ҵ�webmail����̨����
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getwebmaillayout")]
        public OutputResult<object> GetWebMailLayout (){
            int userid = UserId;
            WebMailPersonelLayoutInfo ret = new WebMailPersonelLayoutInfo()
            {
                UserId = userid,
                LeftPrecent = new Decimal(0.25),
                RightPrecent = new Decimal(0.25),
                BottomPrecent = new Decimal(0.4),
                ShowBottom = true,
                ShowRight = true
            };
            return new OutputResult<object>(ret);
        }
        /// <summary>
        /// �����ҵ�webmail����̨����
        /// </summary>
        /// <param name="layoutInfo">������Ϣ</param>
        /// <returns></returns>
        [HttpPost]
        [Route("savewebmaillayout")]
        public OutputResult<object> SaveWebMailLayaout([FromBody] WebMailPersonelLayoutInfo layoutInfo) {
            if (layoutInfo == null) { return new OutputResult<object>("�����쳣", "�����쳣", -1); }
            layoutInfo.UserId = UserId;
            return new OutputResult<object>(layoutInfo);
        }

        /// <summary>
        /// ��ȡ���Ի�ǩ���б�
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getsignature")]
        public OutputResult<object> GetSignature()
        {
            MailBox ret1 = new MailBox()
            {
                recid = new Guid("10e59749-28fa-42f5-9ce5-9f2b6cd70834"),
                accountid = "yinjunyu@renqiankeji.com",
                recname = "����1",
                inwhitelist = 1
            };
            PersonalSign ret = new PersonalSign()
            {
                recname="ǩ��1",
                signcontent="xxx ��˾",
                devicetype=1,
                recid= new Guid("4024dd18-b8de-47ee-83a4-1ee7a46eeb05"),
                mailbox= ret1
            };
            List<PersonalSign> list = new List<PersonalSign>();
            list.Add(ret);
            return new OutputResult<object>(list);
        }

        /// <summary>
        /// ��ȡ�ҵ������б�
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getmailboxlist")]
        public OutputResult<object> GetMailBoxList([FromBody] MailListActionParamInfo dynamicModel, int userId)
        {
            if (dynamicModel == null) return ResponseError<object>("������ʽ����");
            return _eMailServices.GetMailBoxList(dynamicModel, UserId);
        }

        #endregion

        #region ��������

        /// <summary>
        /// ָ������ӵ����
        /// </summary>
        /// <param name="dynamicModel">����ָ����Ϣ</param>
        /// <returns></returns>
        [HttpPost]
        [Route("savemailowner")]
        public OutputResult<object> SaveMailOwner([FromBody] MailOwnModel dynamicModel = null)
        {
            if (dynamicModel == null || dynamicModel.RecIds == null)
                return ResponseError<object>("������ʽ����");
            WriteOperateLog("ָ������ӵ����", dynamicModel);
            OutputResult<object> result = new OutputResult<object>();
             _eMailServices.SaveMailOwner(dynamicModel.RecIds, dynamicModel.NewUserId);
            result.Message = "ָ���ɹ�";
            return result;
        }

        /// <summary>
        /// �趨�������б�
        /// </summary>
        /// <param name="dynamicModel">��������Ϣ</param>
        /// <returns></returns>
        [HttpPost]
        [Route("savewhitelist")]
        public OutputResult<object> SaveWhiteList([FromBody] WhiteListModel dynamicModel = null)
        {
            if (dynamicModel == null|| dynamicModel.RecIds==null)
                return ResponseError<object>("������ʽ����");
            WriteOperateLog("�������������", dynamicModel);
            OutputResult<object> result = new OutputResult<object>();
            _eMailServices.SaveWhiteList(dynamicModel.RecIds, dynamicModel.enable);
            result.Message = "���óɹ�";
            return result;
        }
        #endregion
    }

}