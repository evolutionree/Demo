using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
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
using System.Linq;
using UBeat.Crm.LicenseCore;
using UBeat.Crm.CoreApi.GL.Utility;
using NLog;
using UBeat.Crm.CoreApi.Core.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class BaseDataServices : BasicBaseServices
    {
        private readonly Logger logger = LogManager.GetLogger("UBeat.Crm.CoreApi.Services.Services.BaseDataServices");
        private readonly DynamicEntityServices _dynamicEntityServices;

        private string sapUrl;

        public BaseDataServices(DynamicEntityServices dynamicEntityServices)
        {
            _dynamicEntityServices = dynamicEntityServices;
        }

        public void InitDicDataQrtz()
        {
            InitDicData(new List<int>());
        }

        public int InitDicData(List<Int32> dicTypeIdArr)
        {

            var postData = new Dictionary<string, object>();
            var headData = new Dictionary<string, string>();
            //postData.Add("Transaction_ID", "BASIC_DATA");
            headData.Add("Transaction_ID", "BASIC_DATA");
            var result = CallAPIHelper.ApiPostData(postData, headData);
            return 0;
        }
    }
}
