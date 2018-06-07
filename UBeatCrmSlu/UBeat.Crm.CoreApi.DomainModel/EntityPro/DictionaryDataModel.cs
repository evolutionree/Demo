using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.EntityPro
{
    public class DictionaryDataModel
    {
        public int DicTypeId { get; set; }
        public int DataId { get; set; }
        public string DataVal { get; set; }
        public int RelateDataId { get; set; }
        public string ExtField1 { get; set; }
        public string ExtField2 { get; set; }
        public string ExtField3 { get; set; }
        public string ExtField4 { get; set; }
        public string ExtField5 { get; set; }
    }

    public class DicTypeDataModel
    {
        public Guid? RelateDicTypeId { get; set; }
        public dynamic FieldConfig { get; set; }
    }
}
