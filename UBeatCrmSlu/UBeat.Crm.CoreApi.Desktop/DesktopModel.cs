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
}
