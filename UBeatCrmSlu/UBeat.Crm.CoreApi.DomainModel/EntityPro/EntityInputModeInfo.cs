using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.EntityPro
{
    
    public class EntityInputModeInfo
    {
        public EntityInputMethod InputMethod { get; set; }
        public int RecStatus { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// 这个参数在两个地方生效,名片扫描及手机通讯录中
        /// </summary>
        public CardOcrParamInfo CarOrcParam { get; set; }
    }
    public enum EntityInputMethod {
        CommonInput = 1 ,//普通录入
        CardOcr = 2,//名片扫描录入
        DeviceAddressBook=3//手机通讯录
    }
    public class CardOcrParamInfo {
        public List<CarOcrMapFieldInfo> MapFields { get; set; }
    }
    public class CarOcrMapFieldInfo {
        public string OcrFieldName { get; set; }
        public string EntityFieldName { get; set; }
        public string DisplayName { get; set; }
    }

}
