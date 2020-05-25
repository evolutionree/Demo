using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.WJXModel;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class WJX1Services : BasicBaseServices
    {
        private readonly IConfigurationRoot _configurationRoot;
        private readonly DynamicEntityServices _dynamicEntityServices;
        public WJX1Services(IConfigurationRoot configurationRoot, DynamicEntityServices dynamicEntityServices)
        {
            _configurationRoot = configurationRoot;
            _dynamicEntityServices = dynamicEntityServices;
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
                //sign=sha1(appid+appkey+username+joiner+activity+joinid+realname+dept+extf+ts)
                string sign = SignatureHelper.Sha1Signature(config.AppId + config.APPkey + config.User + "20200000167" + "75174687" + "106173436875" + stamp);
                string sign1 = SignatureHelper.Sha1Signature(config.AppId + config.APPkey+ "73334626" + stamp);
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
                        question.qurl = "https://www.wjx.cn/jq/" + question.qid + ".aspx?sojumpparm=" + detailData["reccode"].ToString();
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

    }
}
