using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Contact;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class ZjBusinessCardServices : BasicBaseServices
    {
        private readonly FileServices _fileServices;
        private readonly BaiduBusinessCardServices _baiduBusinessCardServices;

        public ZjBusinessCardServices(FileServices fileServices, BaiduBusinessCardServices baiduBusinessCardServices)
        {
            _fileServices = fileServices;
            _baiduBusinessCardServices = baiduBusinessCardServices;
        }
        public OutputResult<object> GetBusinessCardInfo(ContactVCardModel vcardModel, int userNumber)
        {
            if (vcardModel?.FileId == null)
            {
                return ShowError<object>("名片扫描文件ID不能为空");
            }
            //获取文件ID
            var fileData = _fileServices.GetFileData(vcardModel.CollectionName, vcardModel.FileId);
            if (fileData == null)
            {
               return ShowError<object>("无法获取名片数据");
            }

            //string fileName = "d:\\test1.jpg";
            //FileStream filestream = new FileStream(fileName, FileMode.Open);
            //byte[] fileData = new byte[filestream.Length];
            //filestream.Read(fileData, 0, (int)filestream.Length);
            //filestream.Close();

            var cardInfo = _baiduBusinessCardServices.GetBusinessCardInfo(fileData);

            if (string.IsNullOrWhiteSpace(cardInfo))
            {
                return ShowError<object>("名片数据获取为空");
            }
            var cardModel = CardInfoTransfer(cardInfo);

            return new OutputResult<object>(cardModel);
        }

        public Dictionary<string, List<string>> CardInfoTransfer(string data)
        {
            var cardResult = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString());
            var workResult = cardResult["words_result"].ToString();
            var cardData = JsonConvert.DeserializeObject<Dictionary<string, object>>(workResult.ToString());

            List<string> recname = null;
            List<string> phone = null;
            List<string> fax = null;
            List<string> mobilephone = null;
            List<string> email = null;
            List<string> website = null;
            List<string> contactposition = null;
            List<string> company = null;
            List<string> address = null;
            List<string> remark = null;

            if (cardData["NAME"] != null)
            {
                string[] recnames = JsonConvert.DeserializeObject<string[]>(cardData["NAME"].ToString());
                recname = new List<string>(recnames);
            }

            if (cardData["TEL"] != null)
            {
                string[] phones = JsonConvert.DeserializeObject<string[]>(cardData["TEL"].ToString());
                phone = new List<string>(phones);
            }

            if (cardData["FAX"] != null)
            {
                string[] faxs = JsonConvert.DeserializeObject<string[]>(cardData["FAX"].ToString());
                fax = new List<string>(faxs);
            }

            if (cardData["MOBILE"] != null)
            {
                string[] mobilephones = JsonConvert.DeserializeObject<string[]>(cardData["MOBILE"].ToString());
                mobilephone = new List<string>(mobilephones);
            }

            if (cardData["EMAIL"] != null)
            {
                string[] emails = JsonConvert.DeserializeObject<string[]>(cardData["EMAIL"].ToString());
                email = new List<string>(emails);
            }

            if (cardData["EMAIL"] != null)
            {
                string[] emails = JsonConvert.DeserializeObject<string[]>(cardData["EMAIL"].ToString());
                email = new List<string>(emails);
            }

            if (cardData["URL"] != null)
            {
                string[] websites = JsonConvert.DeserializeObject<string[]>(cardData["URL"].ToString());
                website = new List<string>(websites);
            }

            if (cardData["TITLE"] != null)
            {
                string[] contactpositions = JsonConvert.DeserializeObject<string[]>(cardData["TITLE"].ToString());
                contactposition = new List<string>(contactpositions);
            }

            if (cardData["COMPANY"] != null)
            {
                string[] companys = JsonConvert.DeserializeObject<string[]>(cardData["COMPANY"].ToString());
                company = new List<string>(companys);
            }

            if (cardData["ADDR"] != null)
            {
                string[] addresss = JsonConvert.DeserializeObject<string[]>(cardData["ADDR"].ToString());
                address = new List<string>(addresss);
            }

            var vcardData = new Dictionary<string, List<string>>();           
            vcardData.Add("recname", recname);
            vcardData.Add("phone", phone);
            vcardData.Add("fax", fax);
            vcardData.Add("mobilephone", mobilephone);
            vcardData.Add("email", email);
            vcardData.Add("website", website);
            vcardData.Add("contactposition", contactposition);
            vcardData.Add("company", company);
            vcardData.Add("address", address);
            vcardData.Add("remark", remark);
            
            return vcardData;
        }
    }
}
