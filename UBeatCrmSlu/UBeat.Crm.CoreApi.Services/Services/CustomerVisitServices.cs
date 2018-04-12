using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Repository.Customer;
using UBeat.Crm.CoreApi.Repository.Repository.DynamicEntity;
using UBeat.Crm.CoreApi.Repository.Repository.EntityPro;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.Message;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class CustomerVisitServices : EntityBaseServices
    {
        private readonly IDynamicEntityRepository _dynamicEntityRepository;
        private readonly IEntityProRepository _entityProRepository;
        private readonly ICustomerVisitRepository _customerVisitRepository;
        readonly Guid custEntityId = new Guid("f9db9d79-e94b-4678-a5cc-aa6e281c1246");


        public CustomerVisitServices()
        {
            _dynamicEntityRepository = ServiceLocator.Current.GetInstance<IDynamicEntityRepository>();
            _entityProRepository = ServiceLocator.Current.GetInstance<IEntityProRepository>();
            _customerVisitRepository = ServiceLocator.Current.GetInstance<ICustomerVisitRepository>();
            CacheService= ServiceLocator.Current.GetInstance<CacheServices>();
        }

        public dynamic ExcuteFinishAction(DbTransaction transaction, object basicParamData, object preActionResult, object actionResult, object userId)
        {
            //"840da937-d454-489b-889c-24e20c6be5e1"
            //{[relatedcustomer, {"id":"aeb5a26e-207a-46f6-a25d-e594e064f815","name":"Test001"}]}
            //{[visitperson, {"id":"d58514f8-d44c-4a53-9f48-657a46588ec5","name":"张三"}]}
            //{[workoptions, 1]}
            //{[visitcontent, 111]}
            

            var paramData = basicParamData as DynamicEntityAddModel;
            if (paramData != null)
            {
                Task.Run(() =>
                {
                    try
                    {
                        var fieldData = paramData.FieldData;
                        if (fieldData != null )
                        {
                            Guid custId = Guid.Empty;
                            string custName = string.Empty;
                            string visitpersonName = string.Empty;
                            string workoptionsName = string.Empty;
                            string visitcontent = string.Empty;

                            object relatedcustomerJson = null;
                            object visitpersonJson = null;
                            object workoptionsObj = null;
                            object visitcontentObj = null;
                            fieldData.TryGetValue("relatedcustomer", out relatedcustomerJson);
                            fieldData.TryGetValue("visitperson", out visitpersonJson);
                            fieldData.TryGetValue("workoptions", out workoptionsObj);
                            fieldData.TryGetValue("visitcontent", out visitcontentObj);
                            if (relatedcustomerJson != null)
                            {
                                var relatedcustomer = JObject.Parse(relatedcustomerJson.ToString());
                                if (relatedcustomer != null && relatedcustomer["id"] != null && Guid.TryParse(relatedcustomer["id"].ToString(), out custId))
                                {
                                    custName = relatedcustomer["name"].ToString();
                                }
                            }
                            if(visitpersonJson!=null)
                            {
                                var visitperson = JObject.Parse(visitpersonJson.ToString());
                                if (visitperson != null && visitperson["name"] != null)
                                {
                                    visitpersonName = visitperson["name"].ToString();
                                }
                            }
                            if(workoptionsObj!=null)
                            {
                                var dicids = new List<int>();
                                var workoptionsArray = workoptionsObj.ToString().Split(',');
                                foreach(var item in workoptionsArray)
                                {
                                    int workoptionsId = 0;
                                    if( int.TryParse(workoptionsObj.ToString(), out workoptionsId))
                                    {
                                        dicids.Add(workoptionsId);
                                    }

                                }
                                var dicNames= _customerVisitRepository.GetDictionaryDataValues(90, dicids);
                                workoptionsName = string.Join(',', dicNames);
                            }
                            if(visitcontentObj!=null)
                            {
                                visitcontent = visitcontentObj.ToString();
                            }
                            DynamicEntityDetailtMapper detailMapper = new DynamicEntityDetailtMapper()
                            {
                                EntityId = custEntityId,
                                RecId = custId,
                                NeedPower = 0
                            };

                            var relbussinessId = Guid.Empty;
                            var entityInfotemp = _entityProRepository.GetEntityInfo(custEntityId);
                            var newDetail = _dynamicEntityRepository.Detail(detailMapper, (int)userId);
                            var newMembers = MessageService.GetEntityMember(newDetail as Dictionary<string, object>);
                            //编辑操作的消息
                            var msg = MessageService.GetEntityMsgParameter(entityInfotemp, custId, relbussinessId, "CustomerVisit", (int)userId, newMembers, null);
                            if (msg != null)
                            {
                                msg.TemplateKeyValue.Add("visitperson", visitpersonName);
                                msg.TemplateKeyValue.Add("workoptions", workoptionsName);
                                msg.TemplateKeyValue.Add("visitcontent", visitcontent);
                                MessageService.WriteMessageAsyn(msg, (int)userId);
                            }
                        }


                        
                    }
                    catch (Exception ex)
                    {

                    }
                });
            }

            return actionResult;
        }




       

    }
}
