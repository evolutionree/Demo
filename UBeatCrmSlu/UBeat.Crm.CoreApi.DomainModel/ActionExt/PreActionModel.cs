using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.ActionExt
{


    /// <summary>
    /// 执行Action扩展逻辑
    /// </summary>
    public class ActionExtModel
    {
        public Guid recid { set; get; }
        /// <summary>
        /// api路由path
        /// </summary>
        public string routepath { set; get; }

        /// <summary>
        /// 实现方式：0为数据库函数实现，1为动态DLL方式实现
        /// </summary>
        public int implementtype { set; get; }

        /// <summary>
        /// 动态链接库DLL方式实现时，dll相对根目录的路径
        /// </summary>
        public string assemblyname { set; get; }

        /// <summary>
        /// 动态链接库DLL方式实现时，dll中实现定义的类的全名
        /// </summary>
        public string classtypename { set; get; }

        /// <summary>
        /// 执行的函数，必须固定入参和固定返回值
        /// </summary>
        public string funcname { set; get; }
        /// <summary>
        /// 操作类型:  0=预处理(action执行前)，1=action完成后执行
        /// </summary>
        public int operatetype { set; get; }

        /// <summary>
        /// 执行完该函数后的操作类型:  0=继续执行后面其他逻辑，1=立即终止，完成请求
        /// </summary>
        public int resulttype { set; get; }

        /// <summary>
        /// 实体id，如果时通用接口的routepath，则需要区分entityid
        /// </summary>
        public Guid? entityid { set; get; }
    }

    
}
