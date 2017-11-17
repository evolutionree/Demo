using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.Core.Utility;

namespace UBeat.Crm.CoreApi.Services.Utility
{
    public class CamCardHelper
    {
        private static readonly string CamUser;
        private static readonly string CamPwd;
        private static readonly string CamUrl;


        static CamCardHelper()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationRoot>().GetSection("CamCardsConfig");
            CamUser = config.GetValue<string>("CamCardUser");
            CamPwd = config.GetValue<string>("CamCardPass");
            CamUrl = config.GetValue<string>("CamServiceUrl");
        }

        public static string GetCardInfo(byte[] imgData)
        {
            //构建POST请求
            var url = string.Format(CamUrl, CamUser, CamPwd, imgData.Length);

            var response = HttpLib.Post(url, imgData, "bcr2.intsig.net", "x-www-form-urlencoded");

            return response;
        }

        public static Dictionary<string,List<string>> VCardTransfer(string data)
        {
            var vcardData = new Dictionary<string, List<string>>();

            var cardInfo = new Dictionary<int, List<string>>();

            if (!string.IsNullOrEmpty(data))
            {
                cardInfo = new Dictionary<int, List<string>>();
                for (int i = 0; i < 17; i++)
                {
                    cardInfo.Add(i, new List<string>());
                }
                string[] infoarr = data.Split(Environment.NewLine.ToCharArray());
                infoarr = infoarr.Where(s => !string.IsNullOrEmpty(s)).ToArray();
                for (int i = 5; i < infoarr.Length; i += 2)
                {
                    if (string.IsNullOrEmpty(infoarr[i]))
                    {
                        continue;
                    }
                    if (infoarr[i] == "END:VCARD" || i+1 >= infoarr.Length)//结束
                    {
                        break;
                    }
                    string content = infoarr[i].Substring(infoarr[i].IndexOf(':') + 1).TrimStart('+');
                    string[] optionaltype = infoarr[i + 1].Substring(infoarr[i + 1].IndexOf(';') + 1).Split(',');
                    foreach (var item in optionaltype)
                    {
                        int itemIndex;
                        if (!int.TryParse(item, out itemIndex))
                        {
                            itemIndex = -1;
                        }
                        if (!cardInfo.ContainsKey(itemIndex))
                        {
                            continue;
                        }

                        //公司、地址去掉分号
                        if (item == "11")
                        {
                            content = content.Replace(';', ' ').TrimStart(' ');
                        }
                        if (item == "10")
                        {
                            content = content.TrimEnd(';');
                        }
                        if (item == "5")
                        {
                            content = content.Replace("p", "-");
                        }
                        content = content.TrimEnd('\r');
                        cardInfo[itemIndex].Insert(0, content);//.Add(content);
                        break;//第一次只添加第一个，以保证优先选择
                    }
                }
                for (int i = 5; i < infoarr.Length; i += 2)
                {
                    if (string.IsNullOrEmpty(infoarr[i]))
                    {
                        continue;
                    }
                    if (infoarr[i] == "END:VCARD" || i + 1 >= infoarr.Length)//结束
                    {
                        break;
                    }
                    string content = infoarr[i].Substring(infoarr[i].IndexOf(':') + 1).TrimStart('+');
                    string[] optionaltype = infoarr[i + 1].Substring(infoarr[i + 1].IndexOf(';') + 1).Split(',');
                    for (int k = 1; k < optionaltype.Count(); k++)//去掉第一个选项
                    {
                        int itemIndex;
                        if (!int.TryParse(optionaltype[k], out itemIndex))
                        {
                            itemIndex = -1;
                        }
                        if (!cardInfo.ContainsKey(itemIndex))
                        {
                            continue;
                        }

                        //公司、地址去掉分号
                        if (optionaltype[k] == "11")
                        {
                            content = content.Replace(';', ' ').TrimStart(' ');
                        }
                        if (optionaltype[k] == "10")
                        {
                            content = content.TrimEnd(';');
                        }
                        if (optionaltype[k] == "5")
                        {
                            content = content.Replace("p", "-");
                        }
                        content = content.TrimEnd('\r');
                        cardInfo[itemIndex].Add(content);
                    }
                }
            }

            if (cardInfo.Count >= 14)
            {
                vcardData.Add("recname", cardInfo[0]);
                vcardData.Add("phone", cardInfo[3]);
                vcardData.Add("fax", cardInfo[5]);
                vcardData.Add("mobilephone", cardInfo[6]);
                vcardData.Add("email", cardInfo[7]);
                vcardData.Add("website", cardInfo[8]);
                vcardData.Add("contactposition", cardInfo[9]);
                vcardData.Add("company", cardInfo[10]);
                vcardData.Add("address", cardInfo[11]);
                vcardData.Add("remark", cardInfo[13]);
            }

            return vcardData;
        }
    }
}
