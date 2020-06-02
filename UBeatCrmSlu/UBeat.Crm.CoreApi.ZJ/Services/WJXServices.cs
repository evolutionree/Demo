using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.WJXModel;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class WJXServices : BasicBaseServices
    {
        private readonly IConfigurationRoot _configurationRoot;
        private readonly DynamicEntityServices _dynamicEntityServices;
        private readonly IWJXRepository _WJXRepository;
        public WJXServices(IConfigurationRoot configurationRoot, DynamicEntityServices dynamicEntityServices, IWJXRepository WJXRepository)
        {
            _configurationRoot = configurationRoot;
            _dynamicEntityServices = dynamicEntityServices;
            _WJXRepository = WJXRepository;
        }
        public OutputResult<object> GetWJXSSO()
        {
            var config = _configurationRoot.GetSection("WJXConfig").Get<WJXSSOConfigModel>();
            var stamp = GetTimeStamp();
            string ssoUrl = string.Format(config.SSOUrl, config.AppId, config.APPkey, config.User, SignatureHelper.Sha1Signature(config.AppId + config.APPkey + config.User + stamp));
            return new OutputResult<object>(ssoUrl);
        }

        public OutputResult<object> GetWJXQuestionList(Guid recId, Guid? entityId)
        {
            try
            {
                if (entityId == null || entityId == Guid.Empty)
                {
                    throw new Exception("实体id不能为空");
                }
                DynamicEntityDetailtMapper entityModel = new DynamicEntityDetailtMapper();
                entityModel.EntityId = Guid.Parse(entityId.ToString());
                entityModel.RecId = recId;
                entityModel.NeedPower = 0;
                var resultData = _dynamicEntityServices.Detail(entityModel, 1);
                IDictionary<string, object> detailData = null;
                if (resultData != null && resultData.ContainsKey("Detail") && resultData["Detail"] != null && resultData["Detail"].Count > 0)
                    detailData = resultData["Detail"][0];

                if (detailData == null)
                {
                    throw new Exception("没有找到对应实体记录");
                }
                var config = _configurationRoot.GetSection("WJXConfig").Get<WJXSSOConfigModel>();
                var stamp = GetTimeStamp();
                string qUrl = string.Format(config.QUrl, config.AppId, config.User, stamp, SignatureHelper.Sha1Signature(config.AppId + config.APPkey + config.User + stamp));
                Task<String> taskResult = DingdingMsgService.GetJson(qUrl, null, null);
                taskResult.Wait();
                var result = taskResult.Result;
                List<WJXQuestionModel> searchResultList = new List<WJXQuestionModel>();
                if (result != null)
                {
                    searchResultList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WJXQuestionModel>>(result);

                    foreach (var question in searchResultList)
                    {
                        question.qurl = "https://www.wjx.cn/jq/" + question.qid + ".aspx?sojumpparm=" + detailData["custcode"].ToString();
                    }
                }
                return new OutputResult<object>(searchResultList);
            }
            catch (Exception ex)
            {
                throw new Exception("获取问卷列表异常");
            }

        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        string GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
        public void SaveWXJAnswer(Dictionary<string, object> dic, int userId)
        {
            using (var conn = GetDbConnect())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    WJXCallBack callBack = new WJXCallBack()
                    {
                        Activity = dic["activity"].ToString(),
                        Name = dic["name"].ToString(),
                        Index = dic["index"].ToString(),
                        SubmitTime = Convert.ToDateTime(dic["submittime"].ToString()),
                        JoinId = dic["joinid"].ToString(),
                        Answer = dic,
                        Sign = dic["sign"].ToString(),
                        Sojumpparm = dic["sojumpparm"].ToString()
                    };
                    _WJXRepository.SaveWXJAnswer(callBack, userId, transaction);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    if (transaction != null && transaction.Connection != null)
                        transaction.Rollback();
                    if (!string.IsNullOrEmpty(ex.Message))
                        throw new Exception(ex.Message);
                    throw new Exception("跳到驳回人失败");
                }
            }
        }

        public OutputResult<object> GetWXJAnswerList(WJXCustParam custParam, int userId)
        {
            using (var conn = GetDbConnect())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    var config = _configurationRoot.GetSection("WJXConfig").Get<WJXSSOConfigModel>();
                    if (config == null && string.IsNullOrEmpty(config.AnswerUrl)) return new OutputResult<object>(null, status: 1);
                    var list = _WJXRepository.GetWXJAnswerList(new WJXCallBack
                    {
                        Sojumpparm = custParam.CustCode
                    }, userId, transaction);
                    list.ForEach(t =>
                    {
                        var ts = GetTimeStamp();
                        string sign = SignatureHelper.Sha1Signature(config.AppId + config.APPkey + config.User + t.Sojumpparm + t.Activity + t.JoinId + ts);
                        t.Url = string.Format(config.AnswerUrl, config.AppId, config.User, t.Sojumpparm, t.Activity, t.JoinId, ts, sign);
                    });
                    transaction.Commit();
                    return new OutputResult<object>(list);
                }
                catch (Exception ex)
                {
                    if (transaction != null && transaction.Connection != null)
                        transaction.Rollback();
                    if (!string.IsNullOrEmpty(ex.Message))
                        throw new Exception(ex.Message);
                    throw new Exception("跳到驳回人失败");
                }
            }
        }
    }
}
