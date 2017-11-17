using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using AutoMapper;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models.DJCloud;
using Newtonsoft.Json;
using UBeat.Crm.CoreApi.DomainModel.DJCloud;
using Newtonsoft.Json.Linq;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class DJCloudServices : BaseServices
    {
        private readonly IDJCloudRepository _djCloudRepository;

        public DJCloudServices(IDJCloudRepository djCloudRepository)
        {
            _djCloudRepository = djCloudRepository;
        }

        public OutputResult<object> Call(DJCloudCallBody djClouddModel, int userNumber, bool isMobile)
        {
            //web拨号，主叫号码直接从数据库取当前登录人的手机号码
            if (!isMobile) {
               string currentLoginMobile = _djCloudRepository.getCurrentLoginMobileNO(userNumber);
                if (string.IsNullOrEmpty(currentLoginMobile)) {
                    return ShowError<object>("当前登录人手机号码不能为空！");
                }
                djClouddModel.Subject.Caller = currentLoginMobile;
            }
            string modelJsonStr = JsonConvert.SerializeObject(djClouddModel);
            byte[] byteArray = Encoding.UTF8.GetBytes(modelJsonStr);

            var callResponse = DJCloudHelper.Call(byteArray);

            //call log
            var resultData = JsonConvert.DeserializeObject<CallResultModel>(callResponse);

            DJCloudCallMapper cloudCall = new DJCloudCallMapper()
            {
                CallId = resultData.Info.callID,
                SessionId = resultData.Info.sessionID,
                Caller = djClouddModel.Subject.Caller,
                Called = djClouddModel.Subject.Called,
                IsSeccess = resultData.code == 0 ? 1 : resultData.code,
                FailMsg = resultData.msg
            };
            _djCloudRepository.AddDJCloudCallLog(cloudCall);

            return new OutputResult<object>(callResponse);
        }
    }
}
