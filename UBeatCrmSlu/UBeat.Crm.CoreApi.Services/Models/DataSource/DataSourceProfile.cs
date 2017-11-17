using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;

namespace UBeat.Crm.CoreApi.Services.Models.DataSource
{
    public class DataSourceProfile : Profile
    {
        public DataSourceProfile()
        {
            CreateMap<DataSourceListModel, DataSourceListMapper>();
            CreateMap<DataSourceModel, DataSourceMapper>();
            CreateMap<DataSourceDetailModel, DataSourceDetailMapper>();
            CreateMap<InsertDataSourceConfigModel, InsertDataSourceConfigMapper>();
            CreateMap<UpdateDataSourceConfigModel, UpdateDataSourceConfigMapper> ();
            CreateMap<DynamicDataSrcModel, DynamicDataSrcMapper>();
            CreateMap<DynamicDataSrcQueryDataModel, DynamicDataSrcQueryDataMapper>();
            CreateMap<DictionaryTypeModel, DictionaryTypeMapper>();
            CreateMap<DictionaryModel, DictionaryMapper>();
            CreateMap<DataSrcDeleteModel, DataSrcDeleteMapper>();
        }
    }
}
