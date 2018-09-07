using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility
{
    public interface IMSGService
    {
        /// <summary>
        /// 根据企业微信/钉钉等第三方对接平台API接口请求token过期时间更新token
        /// </summary>
        /// <returns></returns>
        string updateToken();

        /// <summary>
        /// 发送文本信息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool sendTextMessage(Pug_inMsg msg);

        /// <summary>
        /// 发送文字卡片信息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool sendTextCardMessage(Pug_inMsg msg);

        /// <summary>
        /// 发送图片信息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool sendPictureMessage(Pug_inMsg msg);

        /// <summary>
        /// 发送图文信息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool sendPicTextMessage(Pug_inMsg msg);
    }
}
