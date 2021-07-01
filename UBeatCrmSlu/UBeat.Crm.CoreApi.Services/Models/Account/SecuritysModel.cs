using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Account
{
    public class SecuritysModel
    {
        public string PwdSalt { set; get; }

        public RSAKeysModel RSAKeys { set; get; }
    }

    /// <summary>
    /// RSA加密公钥秘钥对
    /// </summary>
    public class RSAKeysModel
    {
        /// <summary>
        /// 公钥
        /// </summary>
        public string PublicKey { set; get; }
        /// <summary>
        /// 秘钥
        /// </summary>
        public string PrivateKey { set; get; }
    }

    public class AdAuthConfigModel
    {
        public int IsOpen { set; get; }
        public string ServerIp { set; get; }
        public int ServerPort { set; get; }
        public string AdminAccount { set; get; }
        public string AdminPwd { set; get; }
        public string BinDN { set; get; }
        public string BaseDN { set; get; }
        public string SpecialAccountId { get; set; }
    }
}
