using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UBeat.Crm.CoreApi.Core.Utility.Encrypt
{
    public class RSAHelper
    {
        //openssl genrsa -out rsa_1024_priv.pem 1024 Pkcs1
        private static readonly string _privateKey = @"MIICWwIBAAKBgQCG1YnPEsAanJAjl+QquJ6c8G9QSJ/EkLdxTnLMG19ff0zARtNFstB9nyV3u8aP270qKXtz3laiWpvdruac/gWMevnbtxOba1Ve62LSyWGjwzah9tWaXhNh0uYi1+AC8vayjR27ktOcjHVs5JqO2QNpCr4uaeScf2dX4sbwyBEG6wIDAQABAoGAFtpytSpUbS5EMAQ6pBMOr0MBWvY0PjjZHtdRFc895w3IkH5wqMuaEvC6hQTKru9rrI3DYuw4euQorvZKGCnl/p1fLy5cA9tvyRdt4g1RLGiigjE7/2eBMSsI9/RnIOFep4vDZQAa1QVETMyi3JLLgZeJ3G3fvNegm1OxiCYR5cECQQCZFRYHRP2684JWq8b8Ddk64MrquSCwppycvrvypichIUhSoICyfZFhfaVvKprWH+NPYfPITAWzHfk+DDwF1U6DAkEA4XvChFBczYVu5dy3ST4uDTM0h5b5vCkuwlG9DrsS19Yhn11ohRN1o+Ke97QkjhZmaSSkSEOfkf3kxFRKkeF5eQJAe+Td9Do5JIKKfXz/SeYnZwiiEgCM3Yuq+w0/be5dgum97+mo30zTSdT3/JW9xQj/3rKHLBejEUlz/GzIc2wk4wJAYTn/NxR737bwibcxZSilrNVuiiTEjKjpW6rpLYfm6SCRIOmjHva0HDWkvER2Grp38FB3ch2OmeHzmeCKwvqJaQJAU0gyVcgVcdJQju1tbdAxI0jofqkWyMZay9AIahKOHEbc6mx3cWcHGkL6WaOFnj6g5o/0sGtgoyG7PIrNnFZUJA==".Replace("\n", "");

        //openssl rsa -pubout -in rsa_1024_priv.pem -out rsa_1024_pub.pem Pkcs1
        private static readonly string _publicKey = @"MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCG1YnPEsAanJAjl+QquJ6c8G9QSJ/EkLdxTnLMG19ff0zARtNFstB9nyV3u8aP270qKXtz3laiWpvdruac/gWMevnbtxOba1Ve62LSyWGjwzah9tWaXhNh0uYi1+AC8vayjR27ktOcjHVs5JqO2QNpCr4uaeScf2dX4sbwyBEG6wIDAQAB".Replace("\n", "");

        /// <summary>
        /// RSA加密，
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="publicKey">公钥，采用rsa_1024_Pkcs1</param>
        /// <param name="encoding">编码，默认UTF8</param>
        /// <returns></returns>
        public static string Encrypt(string plainText, string publicKey, Encoding encoding = null)
        {
            encoding= Encoding.GetEncoding("gb2312");
            if (encoding == null)
                encoding = Encoding.UTF8;
            //Encrypt
            RSA rsa = CreateRsaFromPublicKey(publicKey);
            var plainTextBytes = encoding.GetBytes(plainText);
            var cipherBytes = rsa.Encrypt(plainTextBytes, RSAEncryptionPadding.Pkcs1);
            var cipher = Convert.ToBase64String(cipherBytes);
            return cipher;
        }
        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="cipher">密文</param>
        /// <param name="privateKey">私钥，采用rsa_1024_Pkcs1</param>
        /// <param name="encoding">编码，默认UTF8</param>
        /// <returns></returns>
        public static string Decrypt(string cipher, string privateKey, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            //Decrypt
            RSA rsa = CreateRsaFromPrivateKey(privateKey);
            var cipherBytes = System.Convert.FromBase64String(cipher);
            var plainTextBytes = rsa.Decrypt(cipherBytes, RSAEncryptionPadding.Pkcs1);
            var plainText = encoding.GetString(plainTextBytes);
            return plainText;
        }
    
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
