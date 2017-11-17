namespace UBeat.Crm.CoreApi.DomainModel
{
    /// <summary>
    /// 数据库-操作结果
    /// 与操作有关的，统一都以该结果返回
    /// </summary>
    public class OperateResult
    {
        /// <summary>
        /// 数据ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 标志位 0为失败 1为成功
        /// </summary>
        public int Flag { get; set; }
        /// <summary>
        /// 提示语
        /// </summary>
        public string Msg { get; set; }
        /// <summary>
        /// 错误提示栈
        /// </summary>
        public string Stacks { get; set; }
        /// <summary>
        /// 数据库错误码
        /// </summary>
        public string Codes { get; set; }
    }
}
