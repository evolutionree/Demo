using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.DomainModel.menus;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class WebMenuController : BaseController
    {
        private readonly WebMenuServices _webMenuService;
        public WebMenuController(WebMenuServices webMenuService) : base(webMenuService) {
            _webMenuService = webMenuService;
        }
        [HttpPost]
        [Route("menutree")]
        public OutputResult<object> getAllMenuTree([FromBody] WebMenuModel model) {

            if (model==null) return    ResponseError<object>("参数格式错误");

            List<WebMenuItem> obj = _webMenuService.getAllWebMenus(model.Type, UserId);
            OutputResult<object> ret = new OutputResult<object>(obj);
            ret.Status = 0;
            return ret;
        }
        [HttpPost]
        [Route("addmenu")]
        public OutputResult<object> addMenu([FromBody] WebMenuItem body) {
            _webMenuService.addMenu(body);
            OutputResult<object> ret = new OutputResult<object>(body);
            ret.Status = 0;
            return ret;
        }
        [HttpPost]
        [Route("synchmenu")]
        public OutputResult<object> synchSysMenu() {
            _webMenuService.synchCRMAndOfficeMenus();
            OutputResult<object> ret = new OutputResult<object>();
            ret.Status = 0;
            return ret;
        }
        
    }
}
