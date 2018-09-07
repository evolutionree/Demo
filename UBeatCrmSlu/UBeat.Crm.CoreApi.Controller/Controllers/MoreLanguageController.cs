using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Controllers;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class MoreLanguageController : BaseController
    {
        [HttpPost]
        [Route("morelanglist")]
        public OutputResult<object> MoreLanguageList()
        {
            string[] language = { "cn-中文-GMT+8", "en-English-GMT", "tw-繁体-GMT+8" };
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            foreach (var item in language)
            {
                Dictionary<string, string> detail = new Dictionary<string, string>();
                var msg = item.Split('-');
                detail.Add("key", msg[0]);
                detail.Add("dispaly", msg[1]);
                detail.Add("gmt", msg[2]);
                list.Add(detail);
            }
            return new OutputResult<object>(list);
        }
    }
}
