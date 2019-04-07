using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Core.Utility
{
    public class UKWebApiAttribute: Attribute
    {
        public UKWebApiAttribute() {
        }
        public UKWebApiAttribute(string name) {
            Name = name;
        }
        public string Name { get; }
        public string Description { get; set; }
    }
}
