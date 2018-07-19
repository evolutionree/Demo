using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.EntityPro
{
    public class DictionaryDataModel
    {
        public Guid DicId { get; set; }
        public int DicTypeId { get; set; }
        public int DataId { get; set; }
        public string DataVal { get; set; }
        public Guid? RelateDataId { get; set; }
        public int RecStatus { get; set; }
        public int RecOrder { get; set; }
        public string ExtField1 { get; set; }
        public string ExtField2 { get; set; }
        public string ExtField3 { get; set; }
        public string ExtField4 { get; set; }
        public string ExtField5 { get; set; }
        public dynamic DataName_Lang { get; set; }
    }

    public class DicTypeDataModel
    {
        public string RelateDicTypeId { get; set; }
        public dynamic FieldConfig { get; set; }
    }
}
