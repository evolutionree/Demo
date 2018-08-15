using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Desktop
{
    public class Desktop
    {

        public Guid DesktopId { get; set; }

        public String DeskptopName { get; set; }

        public int DesktopType { get; set; }
        public String LeftItems { get; set; }
        public String RightItems { get; set; }

        public Guid BaseDeskId { get; set; }
        public String Description { get; set; }
        public int Status { get; set; }
    }

    public class DesktopComponent
    {
        public Guid DsComponetId { get; set; }

        public String ComName { get; set; }

        public int ComType { get; set; }

        public Decimal ComWidth { get; set; }

        public int ComHeightType { get; set; }
        public Decimal MinComHeight { get; set; }
        public Decimal MaxComHeight { get; set; }
        public String ComUrl { get; set; }
        public String ComArgs { get; set; }
        public String ComDesciption { get; set; }
        public int Status { get; set; }
    }

    public class DesktopRelation
    {

        public int UserId { get; set; }

        public Guid DesktopId { get; set; }
    }

    public class DesktopRunTime
    {
        public Guid DsComponentId { get; set; }

        public int UserId { get; set; }

        public JObject ComArgs { get; set; }
    }

    public class DesktopRoleRelation
    {

        public Guid DesktopId { get; set; }

        public Guid RoleId { get; set; }
    }


    public class DynamicListRequest
    {

        /// <summary>
        /// 数据范围类型:我的动态
        /// </summary>
        public int DataRangeType { get; set; }

        public Guid DepartmetnId { get; set; }

        public List<int> UserIds { get; set; }


        /// <summary>
        /// 时间范围类型：本季度
        /// </summary>
        public int TimeRangeType { get; set; }

        public int SpecialYear { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }


        /// <summary>
        /// 主实体id
        /// </summary>
        public Guid MainEntityId { get; set; }


        /// <summary>
        /// 查询和主实体关联的实体名称
        /// </summary>
        public string SearchKey { get; set; }



        /// <summary>
        /// 和主实体关联的实体id
        /// </summary>
        public Guid RelatedEntityId { get; set; }


        /// <summary>
        /// 单页数据
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }

    }


    public enum TimeRangeType
    {

        /// <summary>
        /// 当天
        /// </summary>
        CurrentDay = 1,

        /// <summary>
        /// 本周（周一起）
        /// </summary>
        CurrentWeek = 2,

        /// <summary>
        /// 本月
        /// </summary>
        CurrentMonth = 3,

        /// <summary>
        /// 本季度
        /// </summary>
        CurrentQuarter = 4,

        /// <summary>
        /// 本年
        /// </summary>
        CurrentYear = 5,

        /// <summary>
        /// 昨天
        /// </summary>
        Yesterday = 6,

        /// <summary>
        /// 上周
        /// </summary>
        LastWeek = 7,

        /// <summary>
        /// 上月
        /// </summary>
        LastMonth = 8,


        /// <summary>
        /// 上季度
        /// </summary>
        LastQuarter = 9,

        /// <summary>
        /// 去年
        /// </summary>
        LastYear = 10,

        /// <summary>
        /// 指定年
        /// </summary>
        SpecialYear = 11,

        /// <summary>
        /// 开始时间-结束时间
        /// </summary>
        TimeRnage = 12
    }

    public enum DataRangeType
    {
        /// <summary>
        /// 我的动态
        /// </summary>
        My = 1,

        /// <summary>
        /// 我的部门（单级）
        /// </summary>
        MyDepartment = 2,

        /// <summary>
        /// 下级部门（递归下级）
        /// </summary>
        LowerDepartment = 3,

        /// <summary>
        /// 指定部门（部门id）
        /// </summary>
        SpecialDepartment = 4,


        /// <summary>
        /// 指定员工
        /// </summary>
        SpecialUser = 5


    }

}
