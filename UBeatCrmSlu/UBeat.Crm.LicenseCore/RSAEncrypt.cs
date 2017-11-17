using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UBeat.Crm.LicenseCore
{
    public static class RSAEncrypt
    {
        // 基于openssl 安全套接字层密码库产生的密钥 
        //openssl genrsa -out rsa_1024_priv.pem 102
        //openssl rsa -pubout -in rsa_1024_priv.pem -out rsa_1024_pub.peml
        //上面的命令必须要在Mac的cmd上执行,产生这个公钥和私钥  不然系统跨平台到mac上会不兼容
        //目前兼容 linux window Ubuntu mac 
        private static readonly string _privateKey = "MIICXQIBAAKBgQDc2kDWYHMSmurTKVBvjvcuJipLfDmTiwEf2ftUeSXEKLYvgmnU\n" +
"ELuSX0LXV3RUO02jcqrRgzGRJWpCjKgJemspxGBnDwHlKmiAclk04tXZL1x4cvEf\n" +
"MLsFn5AZL1YJPQAEYL6lv0/r2kLScJ/jYxnbaA8MqP0jPBEJ5FA1CnWsaQIDAQAB\n" +
"AoGAcBbdfXBqNvElaZK4XkZjMarxIGqmFjogkffiVVc/UbWP8cDw0U7ExF9Q31Zl\n" +
"ljKbDu+tvrQlPhONQMV+q4DUTecEsLJWMm1OazIxjRmYsA1Lkkfj9cfXNZ5hAT/1\n" +
"f8RAFtCzmbirFSLQcTCBFJqBNO6ZWJtqRUsypNkbSV+LVeECQQD0vwotzH/BJdVR\n" +
"0AztYDyoaPBSjJmGeL0S0DceiJp4UaGhT1NStFq6Abk42AHp86ca9fVgVr/ywkXq\n" +
"BiwmcfJdAkEA5wH0qV8a6cUDE+CENxyoiKW9NJ38i9mpZ8VZtSG5/toroo3zXurL\n" +
"vjw7+7cnzokL9S/RKb/jc7spis/+7NBZfQJAATa1UTbTZ1zNqwMyHTc99cPXdUFX\n" +
"PZB5t48qxs78nR4ihA9FpYJZdNSvW3XOxJ89s+eGWPz1JLoL6LYxH9uuzQJBANWW\n" +
"flO3xpeMfAr91OFsEvu9WpJy3NWlm7eBxi7ujx1vNVcoLnR9XYJ4CPH0585T8oVx\n" +
"/8+9Rx1zWLarF8Z34bUCQQCbvjqdlniGN/vNv/GrVIYEZbZDta062UMDfEmjX3g7\n" +
"ycuPVMjlGprdzyv8tPVa505rT3AdvE06KhrPZr4JpODl".Replace("\n", "");

        private static readonly string _publicKey = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDc2kDWYHMSmurTKVBvjvcuJipL\n" +
"fDmTiwEf2ftUeSXEKLYvgmnUELuSX0LXV3RUO02jcqrRgzGRJWpCjKgJemspxGBn\n" +
"DwHlKmiAclk04tXZL1x4cvEfMLsFn5AZL1YJPQAEYL6lv0/r2kLScJ/jYxnbaA8M\n" +
"qP0jPBEJ5FA1CnWsaQIDAQAB".Replace("\n", "");

        /// <summary>
        /// 加密  利用了分段加密 防止加密内容超长
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RSAEncryptStr(string content)
        {
            RSA rsa = CreateRsaFromPublicKey(_publicKey);
            var plainTextBytes = Encoding.UTF8.GetBytes(content);
            int bufferSize = (rsa.KeySize / 8) - 11;//单块最大长度
            var buffer = new byte[bufferSize];
            using (MemoryStream inputStream = new MemoryStream(plainTextBytes),
                 outputStream = new MemoryStream())
            {
                while (true)
                { //分段加密
                    int readSize = inputStream.Read(buffer, 0, bufferSize);
                    if (readSize <= 0)
                    {
                        break;
                    }

                    var temp = new byte[readSize];
                    Array.Copy(buffer, 0, temp, 0, readSize);
                    var encryptedBytes = rsa.Encrypt(temp, RSAEncryptionPadding.Pkcs1);
                    outputStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                }
                var cipherBytes = outputStream.ToArray();
                var cipher = Convert.ToBase64String(cipherBytes);
                return cipher;
            }
        }
        /// <summary>
        /// 解密 分段解密
        /// </summary>
        /// <param name="cipher"></param>
        /// <returns></returns>
        public static string RSADecryptStr(string cipher)
        {
            RSA rsa = CreateRsaFromPrivateKey(_privateKey);
            var cipherBytes = Convert.FromBase64String(cipher);
            int bufferSize = rsa.KeySize / 8;//单块最大长度
            var buffer = new byte[bufferSize];
            using (MemoryStream inputStream = new MemoryStream(cipherBytes),
                                outputStream = new MemoryStream())
            {
                while (true)
                {
                    int readSize = inputStream.Read(buffer, 0, bufferSize);
                    if (readSize <= 0)
                    {
                        break;
                    }

                    var temp = new byte[readSize];
                    Array.Copy(buffer, 0, temp, 0, readSize);
                    var rawBytes = rsa.Decrypt(temp, RSAEncryptionPadding.Pkcs1);
                    outputStream.Write(rawBytes, 0, rawBytes.Length);
                }
                return Encoding.UTF8.GetString(outputStream.ToArray());
            }
        }
        /// <summary>
        /// 重写RSA 解密私钥方法
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        private static RSA CreateRsaFromPrivateKey(string privateKey)
        {
            var privateKeyBits = System.Convert.FromBase64String(privateKey);
            var rsa = RSA.Create();
            var RSAparams = new RSAParameters();

            using (var binr = new BinaryReader(new MemoryStream(privateKeyBits)))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();
                else
                    throw new Exception("Unexpected value read binr.ReadUInt16()");

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new Exception("Unexpected version");

                bt = binr.ReadByte();
                if (bt != 0x00)
                    throw new Exception("Unexpected value read binr.ReadByte()");

                RSAparams.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.D = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.P = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Q = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DP = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DQ = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }

            rsa.ImportParameters(RSAparams);
            return rsa;
        }
        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();
            else
                if (bt == 0x82)
            {
                highbyte = binr.ReadByte();
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }
        /// <summary>
        /// 重写RSA 加密私钥方法
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        private static RSA CreateRsaFromPublicKey(string publicKeyString)
        {
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] x509key;
            byte[] seq = new byte[15];
            int x509size;

            x509key = Convert.FromBase64String(publicKeyString);
            x509size = x509key.Length;

            using (var mem = new MemoryStream(x509key))
            {
                using (var binr = new BinaryReader(mem))
                {
                    byte bt = 0;
                    ushort twobytes = 0;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130)
                        binr.ReadByte();
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();
                    else
                        return null;

                    seq = binr.ReadBytes(15);
                    if (!CompareBytearrays(seq, SeqOID))
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8103)
                        binr.ReadByte();
                    else if (twobytes == 0x8203)
                        binr.ReadInt16();
                    else
                        return null;

                    bt = binr.ReadByte();
                    if (bt != 0x00)
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130)
                        binr.ReadByte();
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();
                    else
                        return null;

                    twobytes = binr.ReadUInt16();
                    byte lowbyte = 0x00;
                    byte highbyte = 0x00;

                    if (twobytes == 0x8102)
                        lowbyte = binr.ReadByte();
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte();
                        lowbyte = binr.ReadByte();
                    }
                    else
                        return null;
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    int modsize = BitConverter.ToInt32(modint, 0);

                    int firstbyte = binr.PeekChar();
                    if (firstbyte == 0x00)
                    {
                        binr.ReadByte();
                        modsize -= 1;
                    }

                    byte[] modulus = binr.ReadBytes(modsize);

                    if (binr.ReadByte() != 0x02)
                        return null;
                    int expbytes = (int)binr.ReadByte();
                    byte[] exponent = binr.ReadBytes(expbytes);

                    var rsa = RSA.Create();
                    var rsaKeyInfo = new RSAParameters
                    {
                        Modulus = modulus,
                        Exponent = exponent
                    };
                    rsa.ImportParameters(rsaKeyInfo);
                    return rsa;
                }

            }
        }

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }
    }
}
