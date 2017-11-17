using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;

namespace UBeat.Crm.CoreApi.Services.Models.SalesStage
{
    public class SalesstageTypeModel
    {
        public string SalesstageTypeId { get; set; }
        public int ForAdmin { get; set; }
        public SalesstageTypeModel() {
            ForAdmin = 0;
        }

    }

    public class SalesStageRestartModel
    {
        public string RecId { get; set; }
        public string TypeId { get; set; }
    }

    public class SaleStageCheckPushModel {
        public string TypeId { get; set; }
        public string FromStageId { get; set; }
        public string ToStageId { get; set; }

    }
    public class SaveSalesStageModel
    {
        public string SalesStageTypeId { get; set; }
        public string SalesStageId { get; set; }
        public string StageName { get; set; }
        public decimal WinRate { get; set; }

    }

    public class DisabledSalesStageModel
    {
        public string SalesStageId { get; set; }

        public int RecStatus { get; set; }

    }
    public class OrderBySalesStageModel
    {
        public string SalesStageIds { get; set; }
    }

    public class OpenHighSettingModel
    {
        public string TypeId { get; set; }
        public int IsOpenHighSetting { get; set; }

    }




    public class AddSalesStageEventSetModel
    {
        public string EventName { get; set; }

        public int IsNeedUpFile { get; set; }

        public string SalesStageId { get; set; }

    }

    public class UpdateSalesStageEventSetModel
    {
        public string EventSetId { get; set; }
        public string EventName { get; set; }

        public int IsNeedUpFile { get; set; }

        public string SalesStageId { get; set; }

    }


    public class AddSalesStageDynEntitySetModel
    {
        public string RelEntityId { get; set; }
        public string SalesStageId { get; set; }

    }
    public class DelSalesStageDynEntitySetModel
    {
        public string DynEntityId { get; set; }

    }

    public class SalesStageSetLstModel
    {
        public string SalesStageId { get; set; }

    }
    public class DisabledSalesStageEventSetModel
    {
        public string EventSetId { get; set; }
    }



    //public class SalesStageOppInfoSetLstModel
    //{
    //    public string SalesStageId { get; set; }

    //}
    public class SalesStageOppInfoFieldsModel
    {
        public string EntityId { get; set; }
        public string SalesStageId { get; set; }
        public string SalesStageTypeId { get; set; }

    }


    public class SaveSalesStageOppInfoSetModel
    {
        public string EntityId { get; set; }

        public string FieldIds { get; set; }

        public string SalesStageId { get; set; }
    }



    public class SalesStageStepInfoModel
    {
        public string RecId { get; set; }

        public string SalesStageId { get; set; }

        public string SalesStageTypeId { get; set; }
    }


    public class PushSalesStageStepInfoModel
    {
        public string RecId { get; set; }

        public string SalesStageIds { get; set; }

    }

    public class ReturnSalesStageStepInfoModel
    {
        public string RecId { get; set; }

        public string TypeId { get; set; }
        public string SalesStageId { get; set; }


    }

    public class SaveSalesStageStepInfoModel
    {
        public int SalesStageFlag { get; set; }
        public string RecId { get; set; }
        public string TypeId { get; set; }
        public string RelRecId { get; set; }
        public string SalesStageId
        {
            get
            {
                string[] splitStr = SalesStageIds.Split(',');
                if (splitStr.Length > 0)
                {
                    return splitStr[splitStr.Length - 1];
                }
                throw new Exception("参数异常");
            }
        }
        public string SalesStageIds { get; set; }
        public int IsWeb { get; set; }

        public ICollection<EventSetModel> Event { get; set; }
        public DynamicEntityAddModel DynEntity { get; set; }
        public DynamicEntityEditModel EditDynEntity { get; set; }
        public DynamicEntityEditModel Info { get; set; }
    }



    public class SaveDynEntityModel
    {
        public string DynRecId { get; set; }
        public string RecId { get; set; }

        public string SalesStageId { get; set; }

    }
    public class UpdateOpportunityStatusModel
    {
        public string RecId { get; set; }

        public string SalesStageId { get; set; }
    }

    public class EventSetModel
    {
        public int isfinish { get; set; }
        public string fileid { get; set; }
        public int isuploadfile { get; set; }

        public string targetid { get; set; }


        public string eventsetid { get; set; }
    }

    public class OppInfoSetModel
    {
        public string fieldid { get; set; }
        public string fieldvalue { get; set; }
        public int isfinish { get; set; }
    }

    public class OrderInfoModel
    {
        public int TypeOrder { get; set; }
        public string RecId { get; set; }

    }

    public class LoseOrderModel
    {
        public string LoseOrderId { get; set; }
        public string LoseReason { get; set; }

        public string ReasonSupplement { get; set; }

        public string OpportunityId { get; set; }
    }


    public class WinOrderModel
    {
        public string WinOrderId { get; set; }
        public string IncomeType { get; set; }

        public string Remark { get; set; }
        public DateTime SigneDate { get; set; }

        public string OpportunityId { get; set; }

    }
}
