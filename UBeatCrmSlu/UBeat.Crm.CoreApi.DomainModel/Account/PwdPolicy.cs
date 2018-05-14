using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Account
{
    /// <summary>
    /// 安全机制
    /// </summary>
    public class PwdPolicy
    {
        /// <summary>
        /// 是否启用密码策略
        /// </summary>
        public int IsUserPolicy { get; set; }
        /// <summary>
        /// 是否启动包含*个字母
        /// </summary>
        public int IsSetPwdLength { get; set; }
        /// <summary>
        /// 包含*个字母
        /// </summary>
        public int SetPwdLength { get; set; }
        /// <summary>
        /// 是否为数字
        /// </summary>
        public int IsNumber { get; set; }
        /// <summary>
        /// 是否大小写
        /// </summary>
        public int IsUpper { get; set; }
        /// <summary>
        /// 是否特殊字符
        /// </summary>
        public int IsSpecialStr { get; set; }
        /// <summary>
        /// 是否不得连续多于*位相同字母
        /// </summary>
        public int IsLikeLetter { get; set; }
        /// <summary>
        /// *位相同字母
        /// </summary>
         public int LikeLetter { get; set; }
        /// <summary>
        /// 是否不得包含用户名
        /// </summary>
        public int IsContainAccount { get; set; }
        /// <summary>
        /// 是否首次修改密码
        /// </summary>
        public int IsFirstUpdatePwd { get; set; }
        /// <summary>
        /// 是否开启密码有效期
        /// </summary>
        public int IsPwdExpiry { get; set; }
        /// <summary>
        /// 密码有效期*月
        /// </summary>
        public int PwdExpiry { get; set; }
        /// <summary>
        /// 是否提前通知用户
        /// </summary>
        public int IsCueUser { get; set; }
        /// <summary>
        /// 提前*月通知用户（提前月份需小于有效月份一半）
        /// </summary>
        public int CueUserDate { get; set; }
    }
}
