using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.QRCode;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class QRCodeServices: EntityBaseServices
    {
        private readonly JavaScriptUtilsServices _javaScriptUtilsServices;
        public QRCodeServices(JavaScriptUtilsServices javaScriptUtilsServices) {
            _javaScriptUtilsServices = javaScriptUtilsServices;
        }
        public OutputResult<object> CheckQrCode(string code, int codetype, int userid) {
            List<QRCodeEntryItemInfo> checkList = new List<QRCodeEntryItemInfo>();
            QRCodeEntryItemInfo matchedItem = null;
            foreach (QRCodeEntryItemInfo item in checkList) {
                if (CheckMatchItem(item, code, codetype, userid)) {
                    matchedItem = item;
                    break;
                }
            }
            if (matchedItem == null)
            {
                //返回不做任何处理的结果
            }
            else {

            }
            return null;
        }
        private bool CheckMatchItem(QRCodeEntryItemInfo item, string code, int type, int userid) {
            if (item.CheckType == QRCodeCheckTypeEnum.StringSearch)
            {
               
            }
            else if (item.CheckType == QRCodeCheckTypeEnum.RegexSearch)
            {
            }
            else if(item.CheckType == QRCodeCheckTypeEnum.JSPlugInSearch) {

            }
            return false;
        }
    }
}
