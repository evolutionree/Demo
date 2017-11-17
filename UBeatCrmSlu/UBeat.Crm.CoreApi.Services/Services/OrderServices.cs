using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.Order;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Services.Models.Message;
using UBeat.Crm.CoreApi.Services.Models.Order;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class OrderServices : BaseServices
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IDynamicEntityRepository _dynamicEntityRepository;
        private readonly IMapper _mapper;

        readonly Guid custEntityId = new Guid("0ccd079d-ec1f-404e-a9d5-cfbe965aca6b");

        public OrderServices(IMapper mapper, IOrderRepository orderRepository,IDynamicEntityRepository dynamicEntityRepository)
        {
            _orderRepository = orderRepository;
            _dynamicEntityRepository = dynamicEntityRepository;
            _mapper = mapper;
        }

        public OutputResult<object> OrderPaymentQuery(OrderPaymentListModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<OrderPaymentListModel, OrderPaymentListMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            return new OutputResult<object>(_orderRepository.OrderPaymentQuery(entity, userNumber));
        }

        public OutputResult<object> UpdateOrderStatus(OrderStatusModel entityModel, UserInfo userinfo)
        {
            OrderStatusMapper entity = new OrderStatusMapper
            {
                RecId=entityModel.RecId,
                Status=entityModel.Status
            };
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            int result = _orderRepository.UpdateOrderStatus(entity, userinfo.UserId);
            if (entityModel.Status == 3) {
                SendMessage(entityModel.RecId, userinfo, custEntityId,"TabOrderFinish");
            } else if (entityModel.Status == 4) {
                SendMessage(entityModel.RecId, userinfo, custEntityId, "TabOrderInvalid");
            }
            return new OutputResult<object>(result);
        }

        private void SendMessage(Guid bussinessId, UserInfo userinfo,Guid custEntityId,string FuncCode)
        {
            Task.Run(() =>
            {
                try
                {
                    DynamicEntityDetailtMapper mainDetailMapper = new DynamicEntityDetailtMapper()
                    {
                        EntityId = custEntityId,
                        RecId = bussinessId,
                        NeedPower = 0
                    };
                    var mainCustDetail = _dynamicEntityRepository.Detail(mainDetailMapper, userinfo.UserId);

                    var relentityid = Guid.Empty;
                    var typeid = mainCustDetail.ContainsKey("rectype") ? Guid.Parse(mainCustDetail["rectype"].ToString()) : custEntityId;
                    var newMembers = MessageService.GetEntityMember(mainCustDetail as Dictionary<string, object>);
                    var msg = new MessageParameter();
                    msg.EntityId = custEntityId;
                    msg.TypeId = typeid;
                    msg.RelBusinessId = Guid.Empty;
                    msg.RelEntityId = Guid.Empty;
                    msg.BusinessId = bussinessId;
                    msg.ParamData = null;
                    msg.FuncCode = FuncCode;

                    msg.Receivers = MessageService.GetEntityMessageReceivers(newMembers, null);

                    var paramData = new Dictionary<string, string>();
                    paramData.Add("operator", userinfo.UserName);

                    msg.TemplateKeyValue = paramData;

                    MessageService.WriteMessageAsyn(msg, userinfo.UserId);
                }
                catch (Exception ex)
                {

                }
            });
        }

    }
}
