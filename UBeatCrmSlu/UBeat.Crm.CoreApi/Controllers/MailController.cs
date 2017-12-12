using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Differencing;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.EMail;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.EMail;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.MailService.Mail.Enum;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class MailController : BaseController
    {

        private readonly EMailServices _emailServices;

        public MailController(EMailServices emailServices) : base(emailServices)
        {
            _emailServices = emailServices;
        }
        /// <summary>
        /// 发邮件
        /// </summary>
        /// <param name="emailServices"></param>
        [HttpPost]
        [Route("validsendmaildata")]
        public dynamic ValidSendEMailData([FromBody]SendEMailModel model = null)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            var header = GetAnalyseHeader();
            return _emailServices.ValidSendEMailData(model, header, UserId);
        }
        /// <summary>
        /// 发邮件
        /// </summary>
        /// <param name="emailServices"></param>
        [HttpPost]
        [Route("sendemail")]
        public OutputResult<object> SendEMailAsync([FromBody]SendEMailModel model = null)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            var header = GetAnalyseHeader();
            //return _emailServices.SendEMailAsync(model, header, UserId);
            return null;
        }

        /// <summary>
        /// 收邮件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("receiveemail")]
        public OutputResult<object> ReceiveEMailAsync([FromBody] ReceiveEMailModel model = null)
        {
            if (model == null || model.ConditionVal == null)
            {
                model = new ReceiveEMailModel();
                model.Conditon = SearchQueryEnum.None;
                model.ConditionVal = string.Empty;
                model.IsFirstInit = false;
            }
            return _emailServices.QueueReceiveEMailAsync(model, UserId);
        }

        #region 邮件CRUD
        [HttpPost]
        [Route("listmail")]
        public OutputResult<object> ListMail([FromBody] MailListActionParamInfo paramInfo)
        {
            PageDataInfo<MailBodyMapper> retList = _emailServices.ListMail(paramInfo, UserId);
            return new OutputResult<object>(retList);
        }
        [HttpPost]
        [Route("intoandfrolstmail")]
        public OutputResult<object> InnerToAndFroListMail([FromBody] InnerToAndFroMailModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _emailServices.InnerToAndFroListMail(model, UserId);
        }
        [HttpPost]
        [Route("maildetail")]
        public OutputResult<object> MailDetail([FromBody] MailDetailModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _emailServices.MailDetail(model, UserId);

        }

        /// <summary>
        /// 标记或者取消标记邮件
        /// 这里主要，如果一封邮件被一个用户放到两个文件夹了，两个文件夹的tag都要打上。
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("tagmail")]
        public OutputResult<object> tagMail([FromBody] TagMailModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");

            return _emailServices.TagMails(model, UserId);
        }

        /// <summary>
        /// 删除邮件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("deletemail")]
        public OutputResult<object> DeleteMails([FromBody] DeleteMailModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");

            return _emailServices.DeleteMails(model, UserId);
        }
        /// <summary>
        /// 恢复已经删除的邮件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("reconvermail")]
        public OutputResult<object> RecConverMails([FromBody] ReConverMailModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");

            return _emailServices.ReConverMails(model, UserId);
        }
        /// <summary>
        /// 设置未已读邮件或者设置为未读邮件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("readmail")]
        public OutputResult<object> ReadMail([FromBody] ReadOrUnReadMailModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");

            return _emailServices.ReadMail(model, UserId);
        }

        /// <summary>
        /// 获取邮件详情，只支持单个邮件获取
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        [Route("gettoandfromail")]
        public OutputResult<object> GetInnerToAndFroMail([FromBody] ToAndFroModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");

            return _emailServices.GetInnerToAndFroMail(model, UserId);
        }

        [HttpPost]
        [Route("gettoandfroatt")]
        public OutputResult<object> GetInnerToAndFroAttachment([FromBody] ToAndFroModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");

            return _emailServices.GetInnerToAndFroAttachment(model, UserId);
        }
        [HttpPost]
        [Route("innertransfermail")]
        public OutputResult<object> InnerTransferMail([FromBody] TransferMailDataModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");

            return _emailServices.InnerTransferMail(model, UserId);
        }

        /// <summary>
        /// 移动邮件，支持批量移动
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("movemail")]
        public OutputResult<object> MoveMail([FromBody] MoveMailModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");

            return _emailServices.MoveMail(model, UserId);
        }

        [HttpPost]
        [Route("getfiles")]
        public OutputResult<object> GetLocalFileFromCrm([FromBody] AttachmentListModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _emailServices.GetLocalFileFromCrm(model, UserId);

        }

        /// <summary>
        /// 分发邮件记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("transferrecrod")]
        public OutputResult<object> distribMail([FromBody] TransferRecordParamModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _emailServices.GetInnerTransferRecord(model, UserId);
        }


        /// <summary>
        /// 获取内部分发列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("transferinnercontact")]
        public OutputResult<object> TransferInnerContact([FromBody] OrgAndStaffTreeModel dynamicModel, int userId)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _emailServices.TransferInnerContact(dynamicModel, UserId);
        }


        #endregion

        #region 通讯录
        /// <summary>
        /// 模糊查询我的通讯人员
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getcontactbykeyword")]
        public OutputResult<object> GetContactByKeyword([FromBody] ContactSearchInfo paramInfo)
        {
            if (paramInfo == null) return ResponseError<object>("参数格式错误");

            return _emailServices.GetContactByKeyword(paramInfo, UserId);
        }
        /// <summary>
        /// 获取企业内部通讯录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getinnercontact")]
        public OutputResult<object> GetInnerContact([FromBody] OrgAndStaffTreeModel dynamicModel, int userId)
        {
            return _emailServices.GetInnerContact(dynamicModel, UserId);
        }

        /// <summary>
        /// 获取企业内部通讯录_人员查询
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getinnerpersoncontact")]
        public OutputResult<object> GetInnerPersonContact([FromBody] OrgAndStaffTreeModel dynamicModel, int userId)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _emailServices.GetInnerPersonContact(dynamicModel, UserId);
        }
        /// <summary>
        /// 获取客户通讯录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getcustomercontact")]
        public OutputResult<object> GetCustomerContact([FromBody] MailListActionParamInfo dynamicModel, int userId)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _emailServices.GetCustomerContact(dynamicModel, UserId);
        }

        /// <summary>
        /// 最近联系人
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getrecentcontact")]
        public OutputResult<object> GetRecentContact([FromBody] MailListActionParamInfo dynamicModel, int userId)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _emailServices.GetRecentContact(dynamicModel, UserId);
        }
        /// <summary>
        /// 获取内部往来人员列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getinnertoandfrouser")]
        public OutputResult<object> GetInnerToAndFroUser([FromBody] ContactSearchInfo dynamicModel, int userId)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _emailServices.GetInnerToAndFroUser(dynamicModel, UserId);
        }
        #endregion

        [HttpPost]
        [Route("testemail")]

        public OutputResult<object> TestEmailValid([FromBody] EmailModel model)
        {
            string errorInfo = string.Empty;
            if (checkEmail(model.Address, out errorInfo) == 200)
            {
                return new OutputResult<object>
                {
                    Status = 0,
                    Message = "合法"
                };
            }
            return new OutputResult<object>
            {
                Status = 0,
                Message = "非法合法"
            };
        }
        public int checkEmail(string mailAddress, out string errorInfo)
        {
            Regex reg = new Regex("^[a-zA-Z0-9_-]+@[a-zA-Z0-9_-]+(\\.[a-zA-Z0-9_-]+)+$");
            if (!reg.IsMatch(mailAddress))
            {
                errorInfo = "Email Format error!";
                return 405;

            }
            string mailServer = getMailServer(mailAddress);
            if (mailServer == null)
            {
                errorInfo = "Email Server error!";
                return 404;
            }
            TcpClient tcpc = new TcpClient();
            tcpc.NoDelay = true;
            tcpc.ReceiveTimeout = 3000;
            tcpc.SendTimeout = 3000;
            try
            {
                tcpc.ConnectAsync(mailServer, 25);
                while (true)
                {
                    if (tcpc.Connected)
                    {
                        break;
                    }
                }
                NetworkStream s = tcpc.GetStream();
                StreamReader sr = new StreamReader(s, Encoding.UTF8);
                StreamWriter sw = new StreamWriter(s, Encoding.UTF8);
                string strResponse = "";
                string strTestFrom = mailAddress;
                sw.WriteLine("helo " + mailServer);
                sw.WriteLine("mail from:<" + mailAddress + ">");
                sw.WriteLine("rcpt to:<" + strTestFrom + ">");
                strResponse = sr.ReadLine();
                if (!strResponse.StartsWith("2"))
                {
                    errorInfo = "UserName error!";
                    return 403;
                }
                sw.WriteLine("quit");
                errorInfo = String.Empty;
                return 200;

            }
            catch (Exception ee)
            {
                errorInfo = ee.Message.ToString();
                return 403;
            }
        }

        private string getMailServer(string strEmail)
        {
            string strDomain = strEmail.Split('@')[1];
            ProcessStartInfo info = new ProcessStartInfo();
            info.UseShellExecute = false;
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.FileName = "nslookup";
            info.CreateNoWindow = true;
            info.Arguments = "-type=mx " + strDomain;
            Process ns = Process.Start(info);
            StreamReader sout = ns.StandardOutput;
            Regex reg = new Regex("mail exchanger = (?<mailServer>[^\\s].*)");
            string strResponse = "";
            while ((strResponse = sout.ReadLine()) != null)
            {
                Match amatch = reg.Match(strResponse);
                if (reg.Match(strResponse).Success) return amatch.Groups["mailServer"].Value;
            }
            return null;
        }
    }

    public class EmailModel
    {
        public string Address { get; set; }
    }
}
