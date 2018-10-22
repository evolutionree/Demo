using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.DingTalk.Services
{
    public class DingTalkAccountServices: BasicBaseServices
    {
        public DingTalkAccountServices() {

        }
        /// <summary>
        /// H5页面登陆使用
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public OutputResult<object> H5LoginWithCode(string code) {
            return null;
        }
    }
}
