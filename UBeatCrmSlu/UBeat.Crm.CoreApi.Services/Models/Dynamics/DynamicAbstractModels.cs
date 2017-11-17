using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Dynamics
{
    public class SaveDynamicAbstractModel
    {
        public Guid? Typeid { set; get; }
        public Guid Entityid { set; get; }

        public List<string> Fieldids { set; get; }

    }

    public class SelectDynamicAbstractModel
    {
        public Guid? TypeID { set; get; }
        public Guid EntityID { set; get; }
    } 
}
