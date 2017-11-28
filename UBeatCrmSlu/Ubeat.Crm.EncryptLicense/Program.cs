using MessagePack;
using MessagePack.Resolvers;
using System;
using System.IO;
using System.Text;
using UBeat.Crm.LicenseCore;

namespace Ubeat.Crm.EncryptLicense
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.WriteLine("=========================加密项目许可信息===============================");
            LicenseInfo info = new LicenseInfo();
            Console.Write("企业名称 : ");
            info.Company = Console.ReadLine();
            Console.Write("项目生效时间 : ");
            info.BeginTime = ReadDateTime();
            Console.Write("项目过期时间 : ");
            info.EndTime = ReadDateTime();
            Console.Write("项目限制注册人数 : ");
            info.LimitPersonNum = ReadInt();
            Console.Write("确定加密上面输入的信息(y/n)?");
            string confirm = Console.ReadLine();
            if (confirm == "y")
            {
                var bytes = MessagePackSerializer.Serialize(info, ContractlessStandardResolver.Instance);
                var json = MessagePackSerializer.ToJson(bytes);
                string encryptData = RSAEncrypt.RSAEncryptStr(json);
                if (!CreateAndWriteFile(encryptData))
                {
                    throw new Exception("创建加密文件失败");
                }
                Console.WriteLine("=========================加密项目许可信息成功===============================");
            }
            else
            {
                Console.WriteLine("=========================已取消加密操作================================");
            }
            Console.WriteLine("按回车退出.......");
            Console.ReadKey();
        }

        private static bool CreateAndWriteFile(string encryptData)
        {
            string path = GetApplicationPath();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = path + "\\encryptdata.dat";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(fs); // 创建写入流
            try
            {
                sw.WriteLine(encryptData); // 写入Hello World
                sw.Dispose(); //关闭文件
                return true;
            }
            catch (Exception)
            {
                throw new Exception("文件写入操作异常");
            }
            finally
            {
                fs.Dispose();
                sw.Dispose();
            }
        }
        /// <summary>
        /// 获取主应用程序根路径
        /// </summary>
        private static string GetApplicationPath()
        {
            string path = Directory.GetCurrentDirectory();
            if (!string.IsNullOrEmpty(path))
            {
                path = path.Substring(0, path.LastIndexOf("\\"));
            }
            else
            {
                throw new Exception("应用主程序目录不存在");
            }
            return path + "\\UBeat.Crm.CoreApi";
            //   return path;
        }

        public static string ReadDateTime()
        {
            DateTime date;
            do
            {
                try
                {

                    date = DateTime.Parse(Console.ReadLine());
                    return date.ToString("yyyy-MM-dd");
                }
                catch
                {
                    Console.WriteLine("输入时间有误，重新输入！");
                }
            }
            while (true);
        }
        public static int ReadInt()
        {
            int number = 0;
            do
            {
                try
                {
                    number = System.Int32.Parse(Console.ReadLine());
                    return number;
                }
                catch
                {
                    Console.WriteLine("输入数字有误，重新输入！");
                }
            }
            while (true);
        }
    }

    [MessagePackObject]
    public class LicenseInfo
    {
        [Key("company")]
        public string Company { get; set; }
        [Key("begindate")]
        public string BeginTime { get; set; }
        [Key("enddate")]
        public string EndTime { get; set; }
        [Key("limitperson")]
        public int LimitPersonNum { get; set; }
    }
}