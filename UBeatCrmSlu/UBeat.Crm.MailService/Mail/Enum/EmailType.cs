using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.MailService.Mail.Enum
{
    /// <summary>
    /// 邮箱类型
    /// </summary>
    public enum EmailType
    {
        None = 0,
        /// <summary>
        /// Google 的网络邮件服务
        /// </summary>
        Gmail = 2,
        /// <summary>
        /// HotMail/Live
        /// </summary>
        HotMail = 4,
        /// <summary>
        /// QQ/FoxMail（Foxmail被腾讯收购）
        /// </summary>
        QQ_FoxMail = 8,
        /// <summary>
        /// QQ/FoxMail（Foxmail被腾讯收购）
        /// </summary>
        QQ_Exmail = 9,
        /// <summary>
        /// 网易126   http://zhidao.baidu.com/question/254278953.html
        /// </summary>
        Mail_126 = 16,
        /// <summary>
        /// 网易163   http://zhidao.baidu.com/question/254278953.html
        /// </summary>
        Mail_163 = 32,
        /// <summary>
        /// 新浪邮箱
        /// </summary>
        Sina = 64,
        /// <summary>
        /// Tom
        /// </summary>
        Tom = 128,
        /// <summary>
        /// 搜狐邮箱
        /// </summary>
        SoHu = 256,
        /// <summary>
        /// 雅虎邮箱    http://zhidao.baidu.com/question/543474569.html
        /// </summary>
        Yahoo = 512,
    }

    public enum EmailSubType
    {
        Alternative = 0,
        Related = 1,
        Mixed = 2
    }
    //
    // 摘要:
    //     A search term.
    //
    // 备注:
    //     The search term as used by MailKit.Search.SearchQuery.
    public enum SearchQueryEnum
    {
        None = -1,
        DeliveredBetweenDate = 0,
        DeliveredAfterDate = 1,
        DeliveredBetweenDateTime = 2,
        FirstInit = 3,
        DeliveredAfterDateAndNotSeen = 4
    }
}
