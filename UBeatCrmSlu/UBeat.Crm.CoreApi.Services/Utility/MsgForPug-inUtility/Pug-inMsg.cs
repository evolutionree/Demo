using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility
{
    public enum MSGServiceType
    {
        WeChat,
        Dingding
    }

    public enum MSGType
    {
        Text,//纯文本
        TextCard,//文字卡片
        Picture,//图片
        PicText//图文
    }

    public class Pug_inMsg
    {
        public MSGType msgType { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string[] pictures { get; set; }
        public string responseUrl { get; set; }
        public List<string> recevier { get; set; }
        public Dictionary<string, object> extraDic { get; set; }

        public Pug_inMsg()
        {
            recevier = new List<string>();
            extraDic = new Dictionary<string, object>();
        }
    }
}
