using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Utility;

namespace UBeat.Crm.CoreApi.DomainModel.Account
{
    

    public class UpdateSoftwareEntity
    {

        public Guid recid { set; get; }

        public int clienttype { set; get; }

        public string clienttypename { set; get; }

        public int versionno { set; get; }

        public string versionname { set; get; }

        public string updateurl { set; get; }

        public bool enforceupdate { set; get; }

        public int buildno { set; get; }

        public int versionstatus { set; get; }

        [JsonIgnore]
        public string remark { set; get; }

        public JArray remarks
        {
            get
            {
                return remark.ToJsonArray();
            }
        }
    }
}
