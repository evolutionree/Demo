using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.GL.Model
{
    public class SoOrderParamModel
    {
        public string OrderId { get; set; }
        public string ReqDate { get; set; }
        public string ERDAT_FR { get; set; }
        public string ERDAT_TO { get; set; }

    }
    public class SoOrderModel
    {

        public String TYPE { get; set; }
        public string MESSAGE { get; set; }
        public Dictionary<String, List<SoOrderDataModel>> DATA { get; set; }
    }
    public class SapOrderCreateModelResult
    {
        public string TYPE { get; set; }
        public string MESSAGE { get; set; }
        public string VBELN { get; set; }

    }
    public class SoOrderDataModel
    {

        public string VBELN { get; set; }
        public int POSNR { get; set; }
        public string AUART { get; set; }
        public string BSTNK { get; set; }
        public string BEZEI { get; set; }
        public string ERNAM { get; set; }
        public string NAME_TEXT { get; set; }
        public string ERDAT { get; set; }
        public string BSTKD { get; set; }
        public string VKORG { get; set; }
        public string VTEXT { get; set; }
        public string VTWEG { get; set; }
        public string VTEXT2 { get; set; }
        public string SPART { get; set; }
        public string VTEXT3 { get; set; }
        public string VKBUR { get; set; }
        public string BEZEI2 { get; set; }
        public string VKGRP { get; set; }
        public string BEZEI3 { get; set; }
        public string KUNNR { get; set; }
        public string NAME1 { get; set; }
        public string KLABC { get; set; }
        public string KUKLA { get; set; }
        public string BRAN1 { get; set; }
        public string VDATU { get; set; }
        public string WADAT_IST { get; set; }
        public string AUGRU { get; set; }
        public string MATNR { get; set; }
        public string MAKTX { get; set; }
        public string WGBEZ { get; set; }
        public decimal KWMENG { get; set; }
        public string KMEIN { get; set; }
        public string MEINS { get; set; }
        public string ABGRU { get; set; }
        public decimal ZHSDJ { get; set; }
        public string KMEIN2 { get; set; }
        public decimal NETPR { get; set; }
        public decimal KZWI2 { get; set; }
        public decimal NETWR { get; set; }
        public decimal KZWI6 { get; set; }
        public decimal KZWI4 { get; set; }
        public decimal MWSBP { get; set; }
        public decimal KWERT1 { get; set; }
        public decimal KWERT2 { get; set; }
        public decimal KWERT3 { get; set; }
        public string VBELN2 { get; set; }
        public decimal POSNR2 { get; set; }
        public string OTHERS_DN { get; set; }
        public string WERKS { get; set; }
        public string LGORT { get; set; }
        public string LGOBE { get; set; }
        public decimal LFIMG { get; set; }
        public string DN_UNIT { get; set; }
        public string KOSTA { get; set; }
        public string WBSTK { get; set; }
        public string PDATE { get; set; }
        public string PTIME { get; set; }
        public string VBELN3 { get; set; }
        public decimal POSNR3 { get; set; }
        public string FKDAT { get; set; }
        public decimal FKIMG { get; set; }
        public decimal KZWI22 { get; set; }
        public decimal WAVWR { get; set; }
        public decimal KZWI1 { get; set; }
        public string ZFIELD1 { get; set; }
        public string ZFIELD2 { get; set; }
        public string KOSTA1 { get; set; }
        public string BELNR { get; set; }
        public string VBELN1 { get; set; }
        public string ZQYG { get; set; }
        public string ZZZG { get; set; }
        public string ZMDG { get; set; }
        public string ZMDDLX { get; set; }
        public string ZXLXX { get; set; }
        public string ZFDLX { get; set; }
        public string ZTSQYYQ { get; set; }
        public string ZSQMFYXQTS { get; set; }
        public string ZDCDH { get; set; }
        public string ZYSHDGS { get; set; }
        public string ZYSHDLXR { get; set; }
        public string ZYSHDTEL { get; set; }
        public string ZCDH { get; set; }
        public string ZGH { get; set; }
        public string ZCPH { get; set; }
        public string ZBINGYI { get; set; }
        public string ZTIAOSHU { get; set; }
        public string ZGUIGE { get; set; }
        public string LNAME1 { get; set; }
        public string RFBSK { get; set; }
        public string ZXSJHD2 { get; set; }
        public string ZXSFPH2 { get; set; }
        public string ZGSJCGDD2 { get; set; }
        public string ZGSJFPYZ2 { get; set; }
        public string ZGSJJHD2 { get; set; }
        public string ZGSJFPH2 { get; set; }
        public string ZYDLX { get; set; }
        public string ZGCJCGDDH11 { get; set; }
        public string ZGCJCGFPYZ11 { get; set; }
        public string ZGCJJHDH11 { get; set; }
        public string ZGCJJFPH11 { get; set; }
        public string ZGCJCGDDH12 { get; set; }
        public string ZGCJJHDH12 { get; set; }
        public string ZGCJCGDDH21 { get; set; }
        public string ZGCJJHDH21 { get; set; }
        public string ZGCJCGDDH22 { get; set; }
        public string ZGCJJHDH22 { get; set; }
        public string VBAP_VTEXT { get; set; }
        public string VBKD_BEZEI { get; set; }
        public decimal KKURS { get; set; }
        public string WAERK { get; set; }
        public string ZWAERK { get; set; }
        public decimal ZLGMNG { get; set; }
        public decimal ZHSDJKG { get; set; }
        public decimal ZJHDHSDJKG { get; set; }
        public decimal ZJHDSJCB { get; set; }
        public string ZFPCJRQ { get; set; }
        public string BUDAT { get; set; }
        public string ZKJPZRQ { get; set; }
        public decimal FKLMG { get; set; }
        public string LFART { get; set; }
        public string ZBZFS { get; set; }
        public string ABLAD { get; set; }
        public string ATWRT { get; set; }
        public decimal KBETR { get; set; }
        public string KMEIN1 { get; set; }
        public string VDATU1 { get; set; }
        public string CHARG1 { get; set; }
        public decimal NETWR1 { get; set; }
        public decimal MWSBP1 { get; set; }
        public decimal KZWI61 { get; set; }
        public string KUNNR1 { get; set; }
        public string NAME11 { get; set; }
        public string KUNNR2 { get; set; }
        public string NAME12 { get; set; }
        public string KUNNR3 { get; set; }
        public string NAME13 { get; set; }
        public string VSNMR_V { get; set; }
        public string PSTYV { get; set; }
        public string SDABW { get; set; }
        public string ZTERM { get; set; }
        public string BNAME { get; set; }
        public string IHREZ { get; set; }
        public decimal NTGEW { get; set; }
        public string GEWEI { get; set; }
        public string KOSTL { get; set; }
        public string LIFNR_HYDL { get; set; }
        public string LIFNR_YWY { get; set; }
        public string AUDAT { get; set; }
        public string PRSDT { get; set; }
        public string KSCHL { get; set; }
        public decimal KBETR_ZPSG { get; set; }
        public int KPEIN { get; set; }
        public string VRKME { get; set; }
        public string EDATU { get; set; }
        public decimal KBETR_GWYF { get; set; }
        public decimal KBETR_GWBX { get; set; }
        public decimal KBETR_YJ { get; set; }
    }
}
