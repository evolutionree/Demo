using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Controllers;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Utility
{
    public class StartupHelper
    {

        /// <summary>
        /// 启动服务时，初始化数据和清除缓存数据
        /// </summary>
        public static void InitApplicationData()
        {
            //初始化上传默认图片到文件服务
            //var initDataController = ServiceLocator.Current.GetInstance<InitDataController>();
            //initDataController.Post();

            //清除非登录信息的公共和用户缓存数据
            var _metaService = ServiceLocator.Current.GetInstance<MetaDataServices>();
            _metaService.RemoveCommonCache();
            _metaService.RemoveAllUserCache();

            
        }


    }
}
