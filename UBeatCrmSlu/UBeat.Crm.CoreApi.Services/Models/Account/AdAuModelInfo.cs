using System;
namespace UBeat.Crm.CoreApi.Services.Models.Account
{
    public class AdAuthModelInfo
    {
        public string ServerIp { set; get; }
        public int ServerPort { set; get; }
        public string AdminAccount { set; get; }
        public string AdminPwd { set; get; }
        public string BaseDN { set; get; }
        public string BinDN { set; get; }

        public string Account { set; get; }
        public string Pwd { set; get; }
    }
}
