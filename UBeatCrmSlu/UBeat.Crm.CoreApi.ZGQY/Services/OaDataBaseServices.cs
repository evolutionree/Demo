using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
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
using UBeat.Crm.CoreApi.ZGQY.Utility;
using NLog;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using Newtonsoft.Json;
using UBeat.Crm.CoreApi.ZGQY.Model;
using UBeat.Crm.CoreApi.ZGQY.Repository;
using UBeat.Crm.CoreApi.Services.Services;
using System.Data.Common;
using DocumentFormat.OpenXml.Drawing.Charts;
using DataTable = System.Data.DataTable;

namespace UBeat.Crm.CoreApi.ZGQY.Services
{
    public class OaDataBaseServices : BasicBaseServices
    {
        private readonly Logger logger = LogManager.GetLogger("UBeat.Crm.CoreApi.ZGQY.Services.OaDataBaseServices");
        private readonly DynamicEntityServices _dynamicEntityServices;
        private readonly IOaDataRepository _oaDataRepository;


        public OaDataBaseServices(IOaDataRepository baseDataRepository, DynamicEntityServices dynamicEntityServices)
        {
            _dynamicEntityServices = dynamicEntityServices;
            _oaDataRepository = baseDataRepository;
        }

        #region 
        public IDictionary<string, object> GetEntityDetailData(DbTransaction tran, Guid entityId, Guid recId, int userId)
        {
            DynamicEntityDetailtMapper entityModel = new DynamicEntityDetailtMapper();
            entityModel.EntityId = entityId;
            entityModel.RecId = recId;
            entityModel.NeedPower = 0;
            var resultData = _dynamicEntityServices.Detail(entityModel, userId,tran);

            if (resultData != null && resultData.ContainsKey("Detail") && resultData["Detail"] != null && resultData["Detail"].Count > 0)
                return resultData["Detail"][0];

            return null;
        }
		#endregion

		#region 更新合同数据
		public void InitContractDataQrtz()
		{
			var userId = 1;
			updateContractData(userId);
		}

		public DataTable updateContractData(int userid)
        {
            
            //TODO 获取OA数据库中的createtime之后的合同数据、写入到合同表
            DataSet set = _oaDataRepository.getContractFromOa();
            
            DataTable table = set.Tables["ds"];
            foreach(DataRow myRow in table.Rows)
            {
                _oaDataRepository.insertContract(myRow,userid);
            }     
           
            //TODO 处理变更逻辑
            DataSet setChange = _oaDataRepository.getContractFromOaChange();
            table = setChange.Tables["ds"];
            foreach(DataRow myRow in table.Rows)
            {
                _oaDataRepository.changeContract(myRow,userid);
            }     
            return null;
        }
        
        #endregion


    }
}
