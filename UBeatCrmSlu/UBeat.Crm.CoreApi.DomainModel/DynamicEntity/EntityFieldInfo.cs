using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DynamicEntity
{
    public  class EntityFieldInfo
    {
        public string EntityTableName { set; get; }

        public Guid FieldId { set; get; }
        public string FieldName { set; get; }

        public Guid EntityId { set; get; }

        public string FieldLabel { set; get; }

        public string DisplayName { set; get; }

        public int ControlType { set; get; }

        public int FieldType { set; get; }
        
        public string FieldConfig { set; get; }

        public int Recorder { set; get; }

        public int RecStatus { set; get; }

        public int RecCreator { set; get; }

        public int RecUpdator { set; get; }

        public DateTime RecCreated { set; get; }

        public DateTime RecUpdated { set; get; }

        public long RecVersion { set; get; }


    }
}
