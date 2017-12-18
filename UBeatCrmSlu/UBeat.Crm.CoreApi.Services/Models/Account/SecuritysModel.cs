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
}
