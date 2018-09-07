namespace UBeat.Crm.CoreApi.Services.Models
{
    public class AnalyseHeader
    {
        /// <summary>
        /// 设备 Android,Ios,Web
        /// </summary>
        public string Device { get; set; }
        /// <summary>
        /// 设备id
        /// </summary>
        public string DeviceId { get; set; }
        /// <summary>
        /// 客户端版本
        /// </summary>
        public string VerNum { get; set; }
        /// <summary>
        /// 系统信息(品牌,机型,系统版本)
        /// </summary>
        public string SysMark { get; set; }

        /// <summary>
        /// 当前session的语言
        /// </summary>
        public string UserLanguage { get; set; }

    }
}
