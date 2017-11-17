using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Services;
using Microsoft.AspNetCore.Hosting;
using System.IO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class InitDataController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;

        private readonly FileServices _fileService;
        public InitDataController(IHostingEnvironment hostingEnvironment, FileServices fileService)
        {
            _fileService = fileService;
            _hostingEnvironment = hostingEnvironment;
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post()
        {
            try
            {
                var staticFilesPath = Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles") ;
                var userIcons = UploadFile(Path.Combine(staticFilesPath, "usericon"), GetUserIcon());
                var configIcons = UploadFile(Path.Combine(staticFilesPath, "configuser_menu_icon"), GetConfigIcon());
                var configSelectIcons = UploadFile(Path.Combine(staticFilesPath, "configuser_select_icon"), GetConfigSelectIcon());
                var messageIcon= UploadFile(Path.Combine(staticFilesPath, "messageicon"), GetMessageIcon());
                //
                var custHomePage = UploadFile(Path.Combine(staticFilesPath, "customer_homepage_icon"), GetCustomerHomePageIcon());
                return Ok(new {
                                默认头像 = userIcons,
                                配置用户功能菜单图标 = configIcons,
                                配置用户选择图标 = configSelectIcons,
                                消息列表的菜单图标 = messageIcon,
                                客户主页图标= custHomePage,
                });
            }
            catch (Exception ex)
            {
                return Ok(ex.ToString());
            }
        }

        #region --上传文件--
        private List<string> UploadFile(string folderPath, Dictionary<Guid, string> fileDic)
        {
            List<string> result = new List<string>();
            foreach (var icon in fileDic)
            {
                string filepath = Path.Combine(folderPath, icon.Value);
                if (System.IO.File.Exists(filepath))
                {
                    using (FileStream fs = new FileStream(filepath, FileMode.Open))
                    {
                        var isexist = _fileService.Exists(null, icon.Key.ToString());

                        string fileId = string.Empty;
                        if (isexist == false)
                        {
                            fileId = _fileService.UploadFile(null, icon.Key.ToString(), icon.Value, fs);
                        }
                    }
                    result.Add(string.Format("文件：{0} 文件ID：{1} 上传完成", icon.Value, icon.Key));
                }
                else result.Add(string.Format("文件：{0} 文件ID：{1} 文件不存在", icon.Value, icon.Key));
            }
            return result;
        } 
        #endregion

        #region --usericon--
        private Dictionary<Guid, string> GetUserIcon()
        {
            //folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "usericon");
            Dictionary<Guid, string> dic = new Dictionary<Guid, string>();
            dic.Add(new Guid("a24201ce-04a9-11e7-a7a4-005056ae7f49"), "01@3x.png");
            dic.Add(new Guid("a98bb754-04a9-11e7-a7a4-005056ae7f49"), "02@3x.png");
            dic.Add(new Guid("ae5d0986-04a9-11e7-a7a4-005056ae7f49"), "03@3x.png");
            dic.Add(new Guid("b1e43ed0-04a9-11e7-a7a4-005056ae7f49"), "04@3x.png");
            dic.Add(new Guid("b591b45e-04a9-11e7-a7a4-005056ae7f49"), "05@3x.png");
            dic.Add(new Guid("b97decf4-04a9-11e7-a7a4-005056ae7f49"), "06@3x.png");
            dic.Add(new Guid("bdda242a-04a9-11e7-a7a4-005056ae7f49"), "07@3x.png");
            dic.Add(new Guid("c1dc2226-04a9-11e7-a7a4-005056ae7f49"), "08@3x.png");
            dic.Add(new Guid("c5d98cc4-04a9-11e7-a7a4-005056ae7f49"), "09@3x.png");
            dic.Add(new Guid("c99305d4-04a9-11e7-a7a4-005056ae7f49"), "10@3x.png");
            return dic;
        } 
        #endregion

        #region --配置用户功能菜单图标--
        private Dictionary<Guid, string> GetConfigIcon()
        {
            //folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "配置用户功能菜单图标");
            Dictionary<Guid, string> dic = new Dictionary<Guid, string>();
            dic.Add(new Guid("00000000-0000-0000-0000-100000000001"), "01@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-100000000002"), "02@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-100000000003"), "03@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-100000000004"), "04@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-100000000005"), "05@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-100000000006"), "06@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-100000000007"), "07@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-100000000008"), "08@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-100000000009"), "09@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-100000000010"), "10@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-100000000011"), "11@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-100000000012"), "12@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-100000000013"), "13@3x.png");
            return dic;
        } 
        #endregion

        #region --配置用户选择图标--
        private Dictionary<Guid, string> GetConfigSelectIcon()
        {
            //folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "配置用户选择图标");
            Dictionary<Guid, string> dic = new Dictionary<Guid, string>();
            dic.Add(new Guid("00000000-0000-0000-0000-200000000001"), "01@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-200000000002"), "02@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-200000000003"), "03@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-200000000004"), "04@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-200000000005"), "05@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-200000000006"), "06@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-200000000007"), "07@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-200000000008"), "08@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-200000000009"), "09@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-200000000010"), "10@3x.png");
            return dic;
        }
        #endregion

        #region --消息列表的菜单图标--
        private Dictionary<Guid, string> GetMessageIcon()
        {
            Dictionary<Guid, string> dic = new Dictionary<Guid, string>();
            dic.Add(new Guid("00000000-0000-0000-0000-300000000001"), "workreport@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-300000000002"), "task@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-300000000003"), "sale@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-300000000004"), "approval@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-300000000005"), "announcement@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-300000000006"), "alarm@3x.png");
            return dic;
        }
        #endregion


        #region --客户主页图标--
        private Dictionary<Guid, string> GetCustomerHomePageIcon()
        {
            Dictionary<Guid, string> dic = new Dictionary<Guid, string>();
            dic.Add(new Guid("00000000-0000-0000-0000-400000000001"), "add@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-400000000002"), "merge@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-400000000003"), "transfer@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-400000000004"), "relation@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-400000000005"), "receivable@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-400000000006"), "dynamic_copy@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-400000000007"), "dynamic_ordercanceconf@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-400000000008"), "dynamic_ordercancel@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-400000000009"), "dynamic_ordercomplete@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-400000000010"), "dynamic_orderconfirm@3x.png");
            dic.Add(new Guid("00000000-0000-0000-0000-400000000011"), "dynamic_tran@3x.png");
            return dic;
        }
        #endregion
    }
}
