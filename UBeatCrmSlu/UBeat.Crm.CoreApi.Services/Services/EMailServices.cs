using AutoMapper;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.EMail;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.EMail;
using UBeat.Crm.MailService.Mail.Helper;
using System.Linq;
using UBeat.Crm.MailService;
using UBeat.Crm.MailService.Mail.Enum;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.FileService;
using UBeat.Crm.MailService.Mail;
using MailKit.Search;
using System.Threading;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using MimeKit.Text;
using System.Threading.Tasks;
using System.IO;
using System.Dynamic;
using System.Data.Common;
using MailKit;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class EMailServices : EntityBaseServices
    {
        private const string _entityId = "60ec5c79-dfe2-4c11-aaf8-51177e921c5d";
        private readonly IMapper _mapper;
        private readonly EMail _email;
        private readonly FileServices _fileServices;
        private readonly DynamicEntityServices _dynamicEntityServices;
        private readonly IMailCatalogRepository _mailCatalogRepository;
        private readonly IMailRepository _mailRepository;
        private readonly IDocumentsRepository _docmentsRepository;
        public EMailServices(IMapper mapper, FileServices fileServices, DynamicEntityServices dynamicEntityServices,
            IMailCatalogRepository mailCatalogRepository,
            IMailRepository mailRepository)
        {
            _email = new EMail();
            _mapper = mapper;
            _fileServices = fileServices;
            _dynamicEntityServices = dynamicEntityServices;
            _mailCatalogRepository = mailCatalogRepository;
            _mailRepository = mailRepository;
        }

        private SearchQuery BuilderSearchQuery(SearchQueryEnum query, string conditionVal, int userId)
        {
            SearchQuery dataQuery = null;
            switch (query)
            {
                case SearchQueryEnum.None:
                    ReceiveMailRelatedMapper receiveConfig = _mailRepository.GetUserReceiveMailTime(userId);
                    if (receiveConfig != null)
                    {
                        dataQuery = SearchQuery.DeliveredAfter(receiveConfig.ReceiveTime);
                    }
                    else
                    {
                        dataQuery = SearchQuery.All;
                    }
                    return dataQuery;
                case SearchQueryEnum.DeliveredBetweenDate:
                    string[] split1 = conditionVal.Split(',');
                    if (split1.Length != 2)
                        throw new Exception("邮件搜索条件的时间索引溢出");
                    foreach (var tmp in split1)
                    {
                        if (!CommonHelper.IsMatchDateTime(tmp))
                            throw new Exception("邮件搜索条件的时间格式不正确");
                    }
                    dataQuery = SearchQuery.And(SearchQuery.DeliveredAfter(DateTime.Parse(split1[0])), SearchQuery.DeliveredBefore(DateTime.Parse(split1[1])));
                    return dataQuery;
                case SearchQueryEnum.DeliveredAfterDate:
                    if (!CommonHelper.IsMatchDateTime(conditionVal))
                        throw new Exception("邮件搜索条件的时间格式不正确");
                    dataQuery = SearchQuery.DeliveredAfter(DateTime.Parse(conditionVal));
                    return dataQuery;
                case SearchQueryEnum.FirstInit:
                    dataQuery = SearchQuery.All;
                    return dataQuery;
                case SearchQueryEnum.DeliveredAfterDateAndNotSeen:
                    if (!CommonHelper.IsMatchDateTime(conditionVal))
                        throw new Exception("邮件搜索条件的时间格式不正确");
                    dataQuery = SearchQuery.And(SearchQuery.DeliveredAfter(DateTime.Parse(conditionVal)), SearchQuery.NotSeen);
                    return dataQuery;
                default:
                    throw new Exception("不支持该邮件搜索条件");

            }
        }

        private OutputResult<object> SaveMailDataInDb(MimeMessageResult msgResult, int userNumber)
        {
            Dictionary<string, string> dicHeader = new Dictionary<string, string>();
            string key = String.Empty;
            foreach (var tmp in msgResult.Msg.Headers)
            {
                key = tmp.Id.ToHeaderName();
                if (!dicHeader.ContainsKey(key))
                    dicHeader.Add(key, tmp.Value);
            }
            var fieldData = new Dictionary<string, object>();
            fieldData.Add("recname", msgResult.Msg.Subject);//邮件主题
            fieldData.Add("relativemailbox", BuilderAddress(msgResult.Msg.From));//邮件发送人
            fieldData.Add("headerinfo", JsonHelper.ToJson(dicHeader));//邮件头文件 用json存
            fieldData.Add("title", msgResult.Msg.Subject);//邮件主题
            fieldData.Add("mailbody", msgResult.Msg.GetTextBody(TextFormat.Html));//邮件主题内容
            fieldData.Add("sender", BuilderAddress(msgResult.Msg.From));//邮件发送人
            fieldData.Add("receivers", BuilderAddress(msgResult.Msg.To));//邮件接收人
            fieldData.Add("ccers", BuilderAddress(msgResult.Msg.Cc));//邮件抄送人
            fieldData.Add("bccers", BuilderAddress(msgResult.Msg.Bcc));//邮件密送人
            fieldData.Add("attachcount", msgResult.Msg.Attachments.Count());//邮件附件
            fieldData.Add("urgency", msgResult.Msg.Attachments.Count());//邮件优先级          
            fieldData.Add("senttime", DateTime.Now);//邮件优先级
            fieldData.Add("mongoid", string.Join(";", msgResult.AttachFileRecord.Select(t => ((dynamic)t).mongoid)));//文件id
            var extraData = new Dictionary<string, object>();            //额外数据
            extraData.Add("relatedmailuser", BuilderSenderReceivers(msgResult.Msg));
            extraData.Add("attachfile", msgResult.AttachFileRecord.Select(t => new
            {
                mongoid = ((dynamic)t).mongoid,
                filename = ((dynamic)t).filename,
                filesize = ((dynamic)t).filesize,
                filetype = ((dynamic)t).filetype
            }));
            extraData.Add("sendrecord", new
            {
                aciontype = msgResult.ActionType,
                status = msgResult.Status,
                message = msgResult.ExceptionMsg
            });
            extraData.Add("issendoreceive", 0);
            DynamicEntityAddModel dynamicEntity = new DynamicEntityAddModel()
            {
                TypeId = Guid.Parse(_entityId),
                FieldData = fieldData,
                ExtraData = extraData
            };
            _dynamicEntityServices.RoutePath = "api/dynamicentity/add";//赋予新增权限
            return _dynamicEntityServices.Add(dynamicEntity, header, userNumber);
        }

        /// <summary>
        /// 转移客户的目录，只能移动客户目录
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OutputResult<object> TransferCatalog(TransferCatalogModel paramInfo, int userId)
        {
            DbTransaction tran = null;
            if (userId == paramInfo.newUserId) throw (new Exception("无需转移"));
            MailCatalogInfo catalog = _mailCatalogRepository.GetMailCataLogById(paramInfo.recId, userId, tran);
            if (catalog == null) throw (new Exception("转移目录错误"));
            if (catalog.ViewUserId != userId) throw (new Exception("没有权限转移此目录"));

            if (catalog.CType != MailCatalogType.Cust) throw (new Exception("只能转移客户的目录"));
            MailCatalogInfo parentCatalog = _mailCatalogRepository.GetMailCataLogById(catalog.PId, userId, tran);
            if (parentCatalog.CType != MailCatalogType.CustType) throw (new Exception("目录异常"));
            if (parentCatalog.CustCatalog == null || parentCatalog.CustCatalog == Guid.Empty) { throw (new Exception("目录异常")); }
            MailCatalogInfo newParentCatalog = _mailCatalogRepository.GetCatalogForCustType(parentCatalog.CustCatalog, paramInfo.newUserId, tran);
            Guid newParentCatalogid;
            if (newParentCatalog == null)
            {
                this.InitMailCatalog(paramInfo.newUserId);
                //找新用户的收件箱
                MailCatalogInfo inboxCatalog = _mailCatalogRepository.GetMailCatalogByCode(paramInfo.newUserId, MailCatalogType.InBox.ToString());

            }
            else
            {
                newParentCatalogid = newParentCatalog.RecId;
            }
            _mailCatalogRepository.TransferCatalog(paramInfo.recId, paramInfo.newUserId, newParentCatalogid, tran);
            return null;
        }

        /// <summary>
        /// 移动个人目录，只能移动个人目录，其他目录不能移动
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OutputResult<object> MovePersonalCatalog(MoveCatalogModel paramInfo, int userId)
        {
            DbTransaction tran = null;
            MailCatalogInfo catalogInfo = _mailCatalogRepository.GetMailCataLogById(paramInfo.recId, userId, tran);
            if (catalogInfo == null) throw (new Exception("无法获取目录信息"));
            if (catalogInfo.CType != MailCatalogType.Personal) throw (new Exception("只能移动个人目录"));
            if (catalogInfo.UserId != userId) throw (new Exception("只能移动用户自己的目录"));
            MailCatalogInfo currentParantInfo = _mailCatalogRepository.GetMailCataLogById(catalogInfo.PId, userId, tran);

            MailCatalogInfo newParentInfo = _mailCatalogRepository.GetMailCataLogById(paramInfo.newPid, userId, tran);
            if (newParentInfo == null) throw (new Exception("目标目录不存在"));
            if (newParentInfo.CType != MailCatalogType.Personal) throw (new Exception("新的父目录必须为个人目录"));
            if (newParentInfo.UserId != userId) throw (new Exception("目标目录必须是用户自己的目录"));
            //判断目录是否空目录，非空目录不能
            if (_mailCatalogRepository.checkHasMails(newParentInfo.RecId.ToString(), tran)) throw (new Exception("目标目录不能拥有邮件"));
            if (_mailCatalogRepository.checkCycleCatalog(newParentInfo.RecId.ToString(), catalogInfo.RecId.ToString(), tran)) throw (new Exception("造成循环目录，移动失败"));
            //现在开始移动
            _mailCatalogRepository.MoveCatalog(catalogInfo.RecId.ToString(), newParentInfo.RecId.ToString(), tran);
            return new OutputResult<object>("成功移动");


        }

        public OutputResult<object> SendEMailAsync(SendEMailModel model, AnalyseHeader header, int userNumber)
        {

            var entity = _mapper.Map<SendEMailModel, SendEMailMapper>(model);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var userMailInfo = _mailCatalogRepository.GetUserMailInfo(entity.FromAddress, userNumber);
            if (userMailInfo == null)
                throw new Exception("缺少发件人邮箱信息");

            IList<MailboxAddress> fromAddressList;
            IList<MailboxAddress> toAddressList;
            IList<MailboxAddress> ccAddressList;
            IList<MailboxAddress> bccAddressList;
            BuilderEmailAddress(entity, out fromAddressList, out toAddressList, out ccAddressList, out bccAddressList);
            List<ExpandoObject> attachFileRecord;//用来批量写db记录的
            BuilderAttachmentFile(entity, out attachFileRecord);
            var emailMsg = EMailHelper.CreateMessage(fromAddressList, toAddressList, ccAddressList, bccAddressList, entity.Subject, entity.BodyContent, attachFileRecord);

            AutoResetEvent _workerEvent = new AutoResetEvent(false);
            try
            {
                var taskResult = _email.SendMessageAsync(userMailInfo.SmtpAddress, userMailInfo.SmtpPort, userMailInfo.AccountId, userMailInfo.EncryptPwd, emailMsg, false);
                OutputResult<object> repResult = new OutputResult<object>();
                taskResult.GetAwaiter().OnCompleted(() =>
               {
                   var msg = taskResult.Result;
                   MimeMessageResult msgResult = new MimeMessageResult
                   {
                       Msg = msg,
                       ActionType = (int)MailActionType.ExternalSend,
                       Status = (int)MailStatus.SendSuccess,
                       AttachFileRecord = attachFileRecord,
                   };
                   var result = SaveMailDataInDb(msgResult, userNumber);
                   if (result.Status != 0)
                       throw new Exception("保存邮件数据异常");
                   _workerEvent.Set();
               });
                _workerEvent.WaitOne();
                repResult = new OutputResult<object>()
                {
                    Status = repResult.Status,
                    Message = repResult.Status == 0 ? "发送邮件成功" : "发送邮件失败"
                };
                return repResult;
            }
            catch (Exception ex)
            {
                MimeMessageResult msgResult = new MimeMessageResult
                {
                    Msg = emailMsg,
                    ActionType = (int)MailActionType.ExternalSend,
                    Status = (int)MailStatus.SendFail,
                    AttachFileRecord = attachFileRecord,
                };
                SaveMailDataInDb(msgResult, userNumber);
                return new OutputResult<object>()
                {
                    Status = 1,
                    Message = "发送邮件失败"
                };
            }
        }
        public OutputResult<object> ReceiveEMailAsync(ReceiveEMailModel model, int userNumber)
        {
            var entity = _mapper.Map<ReceiveEMailModel, ReceiveEMailMapper>(model);
            var userMailInfoLst = _mailCatalogRepository.GetAllUserMail((int)DeviceType, userNumber);
            AutoResetEvent _workerEvent = new AutoResetEvent(false);
            try
            {
                OutputResult<object> repResult = new OutputResult<object>();
                foreach (var userMailInfo in userMailInfoLst)
                {
                    SearchQuery searchQuery = BuilderSearchQuery(model.Conditon, model.ConditionVal, userMailInfo.Owner);
                    var taskResult = _email.ImapRecMessageAsync(userMailInfo.ImapAddress, userMailInfo.ImapPort, userMailInfo.AccountId, userMailInfo.EncryptPwd, searchQuery, false);
                    taskResult.GetAwaiter().OnCompleted(() =>
                    {

                        DynamicEntityAddListModel addList = new DynamicEntityAddListModel()
                        {
                            EntityFields = new List<DynamicEntityFieldDataModel>()
                        };
                        var mailRelatedLst = _mailRepository.GetReceiveMailRelated(userNumber);
                        foreach (var msg in taskResult.Result)
                        {
                            var obj = mailRelatedLst.SingleOrDefault(t => t.MailServerId == msg.MessageId && t.UserId == userNumber);
                            if (obj != null)
                                continue;
                            Dictionary<string, string> dicHeader = new Dictionary<string, string>();
                            string key = String.Empty;
                            foreach (var header in msg.Headers)
                            {
                                key = header.Id.ToHeaderName();
                                if (!dicHeader.ContainsKey(key))
                                    dicHeader.Add(key, header.Value);
                            }
                            var fieldData = new Dictionary<string, object>();
                            fieldData.Add("recname", msg.Subject);//邮件主题
                            fieldData.Add("relativemailbox", BuilderAddress(msg.From));//收件人
                            fieldData.Add("headerinfo", JsonHelper.ToJson(dicHeader));//邮件头文件 用json存
                            fieldData.Add("title", msg.Subject);//邮件主题
                            fieldData.Add("mailbody", msg.GetTextBody(TextFormat.Html));//邮件主题内容
                            fieldData.Add("sender", BuilderAddress(msg.From));//邮件发送人
                            fieldData.Add("receivers", BuilderAddress(msg.To));//邮件接收人
                            fieldData.Add("ccers", BuilderAddress(msg.Cc));//邮件抄送人
                            fieldData.Add("bccers", BuilderAddress(msg.Bcc));//邮件密送人
                            fieldData.Add("attachcount", msg.Attachments.Count());//邮件附件
                            fieldData.Add("urgency", 1);//邮件优先级
                            fieldData.Add("receivedtime", DateTime.Now);//邮件优先级
                            var fileTask = UploadAttachmentFiles(msg.Attachments);
                            fieldData.Add("mongoid", string.Join(";", fileTask.Result.Select(t => t.mongoid)));//文件id
                            var extraData = new Dictionary<string, object>();
                            extraData.Add("relatedmailuser", BuilderSenderReceivers(msg));
                            extraData.Add("attachfile", fileTask.Result);
                            extraData.Add("issendoreceive", 1);
                            extraData.Add("receivetimerecord", new
                            {
                                ReceiveTime = msg.Date,
                                ServerId = msg.MessageId
                            });
                            DynamicEntityFieldDataModel dynamicEntity = new DynamicEntityFieldDataModel()
                            {
                                TypeId = Guid.Parse(_entityId),
                                FieldData = fieldData,
                                ExtraData = extraData
                            };
                            addList.EntityFields.Add(dynamicEntity);
                        }
                        _dynamicEntityServices.RoutePath = "api/dynamicentity/add";
                        _dynamicEntityServices.AddList(addList, header, userNumber);

                        _workerEvent.Set();
                    });
                }
                _workerEvent.WaitOne();
                repResult = new OutputResult<object>()
                {
                    Status = repResult.Status,
                    Message = repResult.Status == 0 ? "接收邮件成功" : "接收邮件失败"
                };
                return repResult;
            }
            catch (Exception ex)
            {
                return new OutputResult<object>()
                {
                    Status = 1,
                    Message = "接收邮件失败"
                };
            }
        }

        private void BuilderAttachmentFile(SendEMailMapper entity, out List<ExpandoObject> attachFileRecord)
        {
            attachFileRecord = new List<ExpandoObject>();
            var tmp = _fileServices.GetFileListData(string.Empty, entity.AttachmentFile.Select(t => t.FileId).AsEnumerable());
            foreach (var t in tmp)
            {
                string newFileId = string.Empty;
                var attachFile = entity.AttachmentFile.FirstOrDefault(x => x.FileId == t.FileId);
                if (attachFile.FileType == 0)
                {
                    newFileId = _fileServices.UploadFile(string.Empty, t.FileId, t.FileName, t.Data);
                }
                else
                {
                    newFileId = t.FileId;
                }
                dynamic expandObj = new ExpandoObject();
                expandObj.mongoid = newFileId;
                expandObj.data = t.Data;
                expandObj.filename = t.FileName;
                expandObj.filesize = t.Data.Length;
                expandObj.filetype = t.FileName.Substring(t.FileName.LastIndexOf("."));
                attachFileRecord.Add(expandObj);
            }
        }
        private async Task<IList<dynamic>> UploadAttachmentFiles(IEnumerable<MimeEntity> mimeEntities)
        {
            return await Task.Run(() =>
            {
                IList<dynamic> attachFileRecord = new List<dynamic>();
                foreach (var tmp in mimeEntities)
                {
                    if (tmp is MimePart)
                    {
                        MimePart part = (MimePart)tmp;
                        byte[] buffer;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            part.ContentObject.DecodeTo(ms);
                            ms.Flush();
                            ms.Position = 0;
                            buffer = new byte[ms.Length];
                            ms.Seek(0, SeekOrigin.Begin);
                            ms.WriteAsync(buffer, 0, buffer.Length);
                        }
                        string fileId = _fileServices.UploadFile(string.Empty, Guid.NewGuid().ToString(), part.FileName, buffer);
                        attachFileRecord.Add(new
                        {
                            mongoid = fileId,
                            data = buffer,
                            filename = part.FileName,
                            filesize = buffer.Length,
                            filetype = part.FileName.Substring(part.FileName.LastIndexOf("."))
                        });
                    }
                    else
                    {
                        throw new Exception("获取邮件附件异常");
                    }
                }
                if (mimeEntities.Count() != attachFileRecord.Count)
                {
                    throw new Exception("获取邮件附件异常");
                }
                return attachFileRecord;
            });
        }
        private string BuilderAddress(InternetAddressList internetAddressList)
        {
            string address = string.Empty;
            List<string> addressCol = new List<string>();
            foreach (var tmp in internetAddressList.ToList())
            {

                var mailboxAddress = (MailboxAddress)tmp;
                addressCol.Add(mailboxAddress.Name + "<" + mailboxAddress.Address + ">");
            }
            return string.Join(";", addressCol);
        }
        private void BuilderEmailAddress(SendEMailMapper entity, out IList<MailboxAddress> fromAddressList, out IList<MailboxAddress> toAddressList, out IList<MailboxAddress> ccAddressList, out IList<MailboxAddress> bccAddressList)
        {
            fromAddressList = new List<MailboxAddress>();
            fromAddressList.Add(new MailboxAddress(entity.FromName, entity.FromAddress));

            toAddressList = new List<MailboxAddress>();
            foreach (var to in entity.ToAddress)
            {
                toAddressList.Add(new MailboxAddress(to.DisplayName, to.Address));
            }
            ccAddressList = new List<MailboxAddress>();
            foreach (var cc in entity.CCAddress)
            {
                ccAddressList.Add(new MailboxAddress(cc.DisplayName, cc.Address));
            }
            bccAddressList = new List<MailboxAddress>();
            foreach (var bcc in entity.BCCAddress)
            {
                bccAddressList.Add(new MailboxAddress(bcc.DisplayName, bcc.Address));
            }
        }
        private IList<MailSenderReceiversMapper> BuilderSenderReceivers(MimeMessage msg)
        {
            IList<MailSenderReceiversMapper> senderReceivers = new List<MailSenderReceiversMapper>();

            foreach (var tmp in msg.From)
            {
                var mailBoxAddress = tmp as MailboxAddress;
                if (mailBoxAddress != null)
                    senderReceivers.Add(new MailSenderReceiversMapper
                    {
                        Ctype = (int)EmailAddrType.From,
                        DisplayName = mailBoxAddress.Name,
                        MailAddress = mailBoxAddress.Address
                    });
            }

            foreach (var tmp in msg.To)
            {
                var mailBoxAddress = tmp as MailboxAddress;
                if (mailBoxAddress != null)
                    senderReceivers.Add(new MailSenderReceiversMapper
                    {
                        Ctype = (int)EmailAddrType.To,
                        DisplayName = mailBoxAddress.Name,
                        MailAddress = mailBoxAddress.Address
                    });
            }

            foreach (var tmp in msg.Cc)
            {
                var mailBoxAddress = tmp as MailboxAddress;
                if (mailBoxAddress != null)
                    senderReceivers.Add(new MailSenderReceiversMapper
                    {
                        Ctype = (int)EmailAddrType.CC,
                        DisplayName = mailBoxAddress.Name,
                        MailAddress = mailBoxAddress.Address
                    });
            }

            foreach (var tmp in msg.Bcc)
            {
                var mailBoxAddress = tmp as MailboxAddress;
                if (mailBoxAddress != null)
                    senderReceivers.Add(new MailSenderReceiversMapper
                    {
                        Ctype = (int)EmailAddrType.Bcc,
                        DisplayName = mailBoxAddress.Name,
                        MailAddress = mailBoxAddress.Address
                    });
            }
            return senderReceivers;
        }
        private dynamic BuilderSendRecord(int actionType, int status, string msg)
        {
            return new
            {
                aciontype = actionType,
                status = status,
                message = msg
            };
        }

        private IList<MailError> ValidEmailAddressAuth(SendEMailModel model, int userId)
        {
            IList<MailError> errors = new List<MailError>();
            //#region 拿出邮件白名单
            //var whiteList = _mailRepository.GetIsWhiteList(0, userId);
            //foreach (var tmp in whiteList)
            //{
            //    foreach (var dic in model.ToAddress)
            //    {
            //        if (tmp.Accountid == dic.Key)//不在白名单内
            //        {
            //            errors.Add(new MailError
            //            {
            //                DisplayName = tmp.OwnerName,
            //                EmailAddress = tmp.Accountid,
            //                ErrorTime = DateTime.Now.ToString("yyyy-MM-dd hh::mm"),
            //                Status = 0,
            //                ErrorMsg = "该收件人邮箱不在白名单内"
            //            });
            //        }
            //    }
            //    foreach (var dic in model.CCAddress)
            //    {
            //        if (tmp.Accountid == dic.Key)//不在白名单内
            //        {
            //            var error = errors.FirstOrDefault(t => t.EmailAddress == tmp.Accountid);
            //            if (error != null)
            //            {
            //                error.ErrorMsg += ";该抄送人邮箱不在白名单内";
            //            }
            //            else
            //            {
            //                errors.Add(new MailError
            //                {
            //                    DisplayName = tmp.OwnerName,
            //                    EmailAddress = tmp.Accountid,
            //                    ErrorTime = DateTime.Now.ToString("yyyy-MM-dd hh::mm"),
            //                    Status = 0,
            //                    ErrorMsg = "该抄送人邮箱不在白名单内"
            //                });
            //            }
            //        }
            //    }
            //    foreach (var dic in model.BCCAddress)
            //    {
            //        if (tmp.Accountid == dic.Key)//不在白名单内
            //        {
            //            var error = errors.FirstOrDefault(t => t.EmailAddress == tmp.Accountid);
            //            if (error != null)
            //            {
            //                error.ErrorMsg += ";该抄送人邮箱不在白名单内";
            //            }
            //            else
            //            {
            //                errors.Add(new MailError
            //                {
            //                    DisplayName = tmp.OwnerName,
            //                    EmailAddress = tmp.Accountid,
            //                    ErrorTime = DateTime.Now.ToString("yyyy-MM-dd hh::mm"),
            //                    Status = 0,
            //                    ErrorMsg = "该密送人邮箱不在白名单内"
            //                });
            //            }
            //        }
            //    }
            //}
            //#endregion
            ////当白名单校验通过则不再校验其他规则 
            //if (errors.Count > 0)
            //{
            //    #region 客户联系人
            //    var lst = _mailRepository.GetManagerContactAndInnerUser(userId).Select(t => t.EmailAddress);
            //    foreach (var tmp in model.ToAddress)
            //    {
            //        if (!lst.Contains(tmp.Key))
            //        {
            //            var error = errors.FirstOrDefault(t => t.EmailAddress == tmp.Key);
            //            if (error != null)
            //            {
            //                error.ErrorMsg += ";该收件人邮箱不是自己负责客户的联系人也不是内部人员";
            //            }
            //            else
            //            {
            //                errors.Add(new MailError
            //                {
            //                    DisplayName = tmp.Value,
            //                    EmailAddress = tmp.Key,
            //                    ErrorTime = DateTime.Now.ToString("yyyy-MM-dd hh::mm"),
            //                    Status = 0,
            //                    ErrorMsg = "该收件人邮箱不是自己负责客户的联系人也不是内部人员"
            //                });
            //            }
            //        }
            //    }
            //    foreach (var tmp in model.CCAddress)
            //    {
            //        if (!lst.Contains(tmp.Key))
            //        {
            //            var error = errors.FirstOrDefault(t => t.EmailAddress == tmp.Key);
            //            if (error != null)
            //            {
            //                error.ErrorMsg += ";该收件人邮箱不是自己负责客户的联系人也不是内部人员";
            //            }
            //            else
            //            {
            //                errors.Add(new MailError
            //                {
            //                    DisplayName = tmp.Value,
            //                    EmailAddress = tmp.Key,
            //                    ErrorTime = DateTime.Now.ToString("yyyy-MM-dd hh::mm"),
            //                    Status = 0,
            //                    ErrorMsg = "该收件人邮箱不是自己负责客户的联系人也不是内部人员"
            //                });
            //            }
            //        }
            //    }
            //    foreach (var tmp in model.BCCAddress)
            //    {
            //        if (!lst.Contains(tmp.Key))
            //        {
            //            var error = errors.FirstOrDefault(t => t.EmailAddress == tmp.Key);
            //            if (error != null)
            //            {
            //                error.ErrorMsg += ";该收件人邮箱不是自己负责客户的联系人也不是内部人员";
            //            }
            //            else
            //            {
            //                errors.Add(new MailError
            //                {
            //                    DisplayName = tmp.Value,
            //                    EmailAddress = tmp.Key,
            //                    ErrorTime = DateTime.Now.ToString("yyyy-MM-dd hh::mm"),
            //                    Status = 0,
            //                    ErrorMsg = "该收件人邮箱不是自己负责客户的联系人也不是内部人员"
            //                });
            //            }
            //        }
            //    }
            //    #endregion
            //}
            return errors;
        }

        /// <summary>
        /// 根据catalogtype获取对应用户的目录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MailCatalogInfo GetMailCatalogByCode(int userId, string catalogType)
        {

            InitMailCatalog(userId);
            return _mailCatalogRepository.GetMailCatalogByCode(userId, catalogType);
        }

        /// <summary>
        ///  插入个人目录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OutputResult<object> InsertPersonalCatalog(int userId, AddCatalogModel dynamicModel)
        {
            MailCatalogInfo catalogInfo = _mailCatalogRepository.GetMailCataLogById(dynamicModel.pid, userId, null);
            int Ctype = 3001;
            if (catalogInfo.CType == MailCatalogType.CustType)
            {
                Ctype = 2002;
            }

            var catalog = new CUMailCatalogMapper
            {
                CatalogName = dynamicModel.recName,
                Ctype = Ctype,
                CatalogPId = dynamicModel.pid,
            };
            return HandleResult(_mailCatalogRepository.InsertCatalog(catalog, userId));
        }

        /// <summary>
        /// 编辑个人用户目录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OutputResult<object> UpdatePersonalCatalog(int userId, AddCatalogModel dynamicModel)
        {
            var catalog = new CUMailCatalogMapper
            {
                CatalogId = dynamicModel.recId,
                CatalogName = dynamicModel.recName
            };
            return HandleResult(_mailCatalogRepository.EditCatalog(catalog, userId));
        }


        /// <summary>
        /// 删除个人用户目录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OutputResult<object> DelPersonalCatalog(int userId, DelCatalogModel dynamicModel)
        {
            var catalog = new DeleteMailCatalogMapper
            {
                CatalogId = dynamicModel.recId
            };
            return HandleResult(_mailCatalogRepository.DeleteCatalog(catalog, userId));
        }

        /// <summary>
        /// 初始化用户邮件目录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public void InitMailCatalog(int userId)
        {
            int initCount = _mailCatalogRepository.NeedInitCatalog(userId);
            if (initCount == 0)
                return;
            List<Dictionary<string, object>> defaultList = _mailCatalogRepository.GetDefaultCatalog(userId);
            foreach (var catalog in defaultList)
            {
                if (catalog["defaultid"] == null)
                {
                    catalog["defaultid"] = catalog["recid"];
                    string newGuid = Guid.NewGuid().ToString().ToLower();
                    catalog["recid"] = new Guid(newGuid);
                    _mailCatalogRepository.InitCatalog(catalog, userId);
                }
                foreach (var item in defaultList)
                {
                    if (item["vpid"].ToString() == catalog["defaultid"].ToString() && item["defaultid"] == null)
                    {
                        //下级节点
                        item["defaultid"] = item["recid"];
                        string newGuid = Guid.NewGuid().ToString().ToLower();
                        item["recid"] = new Guid(newGuid);
                        if (catalog["existid"] == null)
                        {
                            item["pid"] = catalog["recid"];
                            item["vpid"] = catalog["recid"];
                        }
                        else
                        {
                            item["pid"] = catalog["existid"];
                            item["vpid"] = catalog["existid"];
                        }
                        _mailCatalogRepository.InitCatalog(item, userId);
                    }
                }
            }
        }


        /// <summary>
        /// 获取下属以及下属邮件目录接口
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<OrgAndStaffTree> GetOrgAndStaffTreeByLevel(int userId, string deptId, string keyword)
        {
            List<OrgAndStaffTree> list = _mailCatalogRepository.GetOrgAndStaffTreeByLevel(userId, deptId, keyword);
            return list;
        }


        /// <summary>
        /// 查询邮件目录
        /// </summary>
        /// <param name="catalogName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<MailCatalogInfo> GetMailCataLog(string catalogType, string keyword, int userId)
        {
            InitMailCatalog(userId);

            //查找文件夹，返回列表
            if (!string.IsNullOrEmpty(keyword))
            {
                return _mailCatalogRepository.GetMailCataLogTreeByKeyword(keyword, userId);
            }
            List<MailCatalogInfo> list = _mailCatalogRepository.GetMailCataLog(catalogType, keyword, userId);
            List<MailCatalogInfo> resultList = new List<MailCatalogInfo>();
            foreach (var catalog in list)
            {
                if (new Guid("00000000-0000-0000-0000-000000000000") == catalog.VPId)
                {
                    resultList.Add(catalog);
                }
                foreach (var item in list)
                {
                    if (item.VPId == catalog.RecId)
                    {
                        if (catalog.SubCatalogs == null)
                        {
                            catalog.SubCatalogs = new List<MailCatalogInfo>();
                        }
                        catalog.SubCatalogs.Add(item);
                    }
                }
            }
            return resultList;
        }

        /// <summary>
        /// 查询邮件
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public PageDataInfo<MailBodyMapper> ListMail(MailListActionParamInfo paramInfo, int userNum)
        {
            //判断catalog是否该用户所有
            MailCatalogInfo catalogInfo = _mailCatalogRepository.GetMailCataLogById(paramInfo.Catalog, userNum, null);
            if (catalogInfo == null || catalogInfo.ViewUserId != userNum)
            {
                throw (new Exception("用户与目录不匹配"));
            }
            if (paramInfo.FetchUserId != userNum && paramInfo.FetchUserId > 0)
            {
                if (!_mailRepository.IsHasSubUserAuth(paramInfo.FetchUserId, userNum))
                    throw (new Exception("没有权限查看该下属的邮件"));
            }

            #region 处理排序规则
            string sortfieldname = "reccreated";
            #endregion


            var lst = _mailRepository.ListMail(paramInfo, sortfieldname, paramInfo.SearchKey, userNum, null);
            foreach (var tmp in lst.DataList)
            {
                tmp.Summary = CommonHelper.NoHTML(tmp.MailBody);
            }
            return lst;
        }

        public OutputResult<object> TagMails(TagMailModel model, int userNum)
        {
            var entity = _mapper.Map<TagMailModel, TagMailMapper>(model);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return HandleResult(_mailRepository.TagMails(entity.MailIds, entity.actionType, userNum));
        }

        public OutputResult<object> DeleteMails(DeleteMailModel model, int userNum)
        {
            var entity = _mapper.Map<DeleteMailModel, DeleteMailMapper>(model);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return HandleResult(_mailRepository.DeleteMails(entity, userNum));
        }

        public OutputResult<object> ReConverMails(ReConverMailModel model, int userNum)
        {
            var entity = _mapper.Map<ReConverMailModel, ReConverMailMapper>(model);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return HandleResult(_mailRepository.ReConverMails(entity, userNum));
        }
        public OutputResult<object> ReadMail(ReadOrUnReadMailModel model, int userNum)
        {
            var entity = _mapper.Map<ReadOrUnReadMailModel, ReadOrUnReadMailMapper>(model);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return HandleResult(_mailRepository.ReadMail(entity, userNum));
        }

        public OutputResult<object> MailDetail(MailDetailModel model, int userNum)
        {
            var entity = _mapper.Map<MailDetailModel, MailDetailMapper>(model);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return new OutputResult<object>(_mailRepository.MailDetail(entity, userNum));
        }

        public OutputResult<object> InnerTransferMail(TransferMailDataModel model, int userId)
        {
            var entity = _mapper.Map<TransferMailDataModel, TransferMailDataMapper>(model);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            #region 先把附件下载 然后再上传一份
            var attachments = _mailRepository.MailAttachment(entity.MailIds);
            var fileListData = _fileServices.GetFileListData(string.Empty, attachments.Select(t => t.MongoId.ToString()));
            foreach (var tmp in fileListData)
            {
                var fileId = _fileServices.UploadFile(string.Empty, string.Empty, tmp.FileName, tmp.Data);
                foreach (var att in attachments)
                {
                    if (tmp.FileId == att.MongoId.ToString())
                    {
                        att.MongoId = Guid.Parse(fileId);
                        att.FileSize = tmp.Data.Length;
                    }
                }
            }
            #endregion


            entity.Attachment = attachments.ToList();

            var res = ExcuteInsertAction((transaction, arg, userData) =>
            {
                return HandleResult(_mailRepository.InnerTransferMail(entity, userId, transaction));
            }, entity, Guid.Empty, userId);
            return res;
        }

        public OutputResult<object> MoveMail(MoveMailModel model, int userId)
        {
            var entity = _mapper.Map<MoveMailModel, MoveMailMapper>(model);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var res = ExcuteInsertAction((transaction, arg, userData) =>
            {
                return HandleResult(_mailRepository.MoveMail(entity, userId, transaction));
            }, entity, Guid.Empty, userId);
            return res;
        }

        public OutputResult<object> GetInnerToAndFroMail(ToAndFroModel model, int userId)
        {
            var entity = new ToAndFroMapper
            {
                relatedMySelf = (int)model.RelatedMySelf,
                relatedSendOrReceive = (int)model.RelatedSendOrReceive,
                MailId = model.MailId,
                PageIndex = model.PageIndex,
                PageSize = model.PageSize
            };
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return new OutputResult<object>(_mailRepository.GetInnerToAndFroMail(entity, userId));
        }
        public OutputResult<object> GetInnerToAndFroAttachment(ToAndFroModel model, int userId)
        {
            var entity = new ToAndFroMapper
            {
                relatedMySelf = (int)model.RelatedMySelf,
                relatedSendOrReceive = (int)model.RelatedSendOrReceive,
                MailId = model.MailId,
                PageIndex = model.PageIndex,
                PageSize = model.PageSize
            };
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return new OutputResult<object>(_mailRepository.GetInnerToAndFroAttachment(entity, userId));
        }
        public OutputResult<object> GetLocalFileFromCrm(AttachmentListModel model, int userId)
        {
            var entity = _mapper.Map<AttachmentListModel, AttachmentListMapper>(model);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return ExcuteSelectAction((transaction, arg, userData) =>
            {
                var ruleSql = userData.RuleSqlFormat("api/documents/documentlist", Guid.Parse("a3500e78-fe1c-11e6-aee4-005056ae7f49"), DeviceClassic);//获取权限
                return new OutputResult<object>(_mailRepository.GetLocalFileFromCrm(entity, ruleSql, userId));
            }, entity, Guid.Parse("a3500e78-fe1c-11e6-aee4-005056ae7f49"), userId);
        }

        public OutputResult<object> GetInnerTransferRecord(TransferRecordParamModel model, int userId)
        {
            var entity = _mapper.Map<TransferRecordParamModel, TransferRecordParamMapper>(model);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return new OutputResult<object>(_mailRepository.GetInnerTransferRecord(entity, userId));
        }
        public OutputResult<object> SaveMailOwner(List<Guid> Mails, int newUserId)
        {
            _mailCatalogRepository.SaveMailOwner(Mails, newUserId);
            return null;
        }

        public OutputResult<object> SaveWhiteList(List<Guid> Mails, int enable)
        {
            _mailCatalogRepository.SaveWhiteList(Mails, enable);
            return null;
        }

        public OutputResult<object> GetMailBoxList(MailListActionParamInfo dynamicModel, int userId)
        {
            int pageSize = 10;
            if (dynamicModel != null && dynamicModel.pageSize > 0)
                pageSize = dynamicModel.pageSize;
            return new OutputResult<object>(_mailRepository.GetMailBoxList(dynamicModel.PageIndex, pageSize, userId));
        }

        public OutputResult<object> ToOrderCatalog(OrderCatalogModel dynamicModel)
        {
            return HandleResult(_mailCatalogRepository.ToOrderCatalog(dynamicModel.recId, dynamicModel.doType));
        }



        #region  模糊查询我的通讯人员限制10个
        public OutputResult<object> GetContactByKeyword(ContactSearchInfo paramInfo, int userId)
        {
            return new OutputResult<object>(_mailRepository.GetContactByKeyword(paramInfo.keyword, paramInfo.count, userId));
        }

        public OutputResult<object> GetInnerContact(OrgAndStaffTreeModel dynamicModel, int userId)
        {
            string treeId = "";
            if (dynamicModel != null)
                treeId = dynamicModel.treeId;
            return new OutputResult<object>(_mailRepository.GetInnerContact(treeId, userId));
        }
        /// <summary>
        /// 获取客户联系人
        /// </summary>
        /// <returns></returns>    
        public OutputResult<object> GetCustomerContact(MailListActionParamInfo dynamicModel, int userId)
        {
            int pageSize = 10;
            if (dynamicModel != null && dynamicModel.pageSize > 0)
                pageSize = dynamicModel.pageSize;
            return new OutputResult<object>(_mailRepository.GetCustomerContact(dynamicModel.PageIndex, pageSize, userId));
        }
        /// <summary>
        /// 获取内部往来人员列表
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="userId"></param>
        /// <returns></returns>      
        public OutputResult<object> GetInnerToAndFroUser(ContactSearchInfo dynamicModel, int userId)
        {
            return new OutputResult<object>(_mailRepository.GetInnerToAndFroUser(dynamicModel.keyword,userId));
        }

        /// <summary>
        /// 最近联系人
        /// </summary>
        /// <returns></returns>    
        public OutputResult<object> GetRecentContact(MailListActionParamInfo dynamicModel, int userId)
        {
            int pageSize = 10;
            if (dynamicModel != null && dynamicModel.pageSize > 0)
                pageSize = dynamicModel.pageSize;
            return new OutputResult<object>(_mailRepository.GetRecentContact(dynamicModel.PageIndex, pageSize, userId));
        }

        #endregion
    }
}
