using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.EntWeChat.Model
{
    public class WJXCustParam
    {
        public string CustCode { get; set; }
    }
    public class WJXCallBack
    {
        public string Activity { get; set; }
        public string Name { get; set; }
        public string Index { get; set; }
        public string JoinId { get; set; }
        public DateTime SubmitTime { get; set; }
        public Dictionary<string, object> Answer { get; set; }
        public string Question { get; set; }
        public string Sign { get; set; }
        public string Sojumpparm { get; set; }
        public string Url { get; set; }
        //"activity": "75174687",
        //"name": "中京问卷2",
        //"ipaddress": "183.238.58.120",
        //"q1_1": "123",
        //"q1_2": "123",
        //"q1_3": "123",
        //"q2": "111",
        //"q3": "1",
        //"index": "4",
        //"joinid": "106172588749",
        //"timetaken": "7",
        //"submittime": "2020-05-22 17:40:10",
        //"sign": "c88e0eb88cb21802b40b4627cf25ba92135cf062"

    }
}
