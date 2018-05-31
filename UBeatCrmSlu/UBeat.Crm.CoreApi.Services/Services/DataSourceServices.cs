using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.DataSource;
using System.Linq;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.Services.Utility;
using Newtonsoft.Json.Linq;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class DataSourceServices : BaseServices
    {
        private readonly IDataSourceRepository dataSourceRepository;
        private readonly IMapper mapper;
        private readonly IDynamicEntityRepository dynamicEntityRepository;
        public DataSourceServices(IMapper _mapper, IDataSourceRepository _dataSourceRepository, IDynamicEntityRepository _dynamicEntityRepository)
        {
            dataSourceRepository = _dataSourceRepository;
            mapper = _mapper;
            dynamicEntityRepository = _dynamicEntityRepository;
        }

        public OutputResult<object> SelectDataSource(DataSourceListModel dataSource, int userNumber)
        {
            var entity = mapper.Map<DataSourceListModel, DataSourceListMapper>(dataSource);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return new OutputResult<object>(dataSourceRepository.SelectDataSource(entity, userNumber));
        }

        public OutputResult<object> InsertSaveDataSource(DataSourceModel dataSource, int userNumber)
        {
            var entity = mapper.Map<DataSourceModel, DataSourceMapper>(dataSource);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return HandleResult(dataSourceRepository.InsertSaveDataSource(entity, userNumber));
        }

        public OutputResult<object> UpdateSaveDataSource(DataSourceModel dataSource, int userNumber)
        {
            var entity = mapper.Map<DataSourceModel, DataSourceMapper>(dataSource);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return HandleResult(dataSourceRepository.UpdateSaveDataSource(entity, userNumber));
        }

        public OutputResult<object> SelectDataSourceDetail(DataSourceDetailModel dataSource, int userNumber)
        {
            var entity = mapper.Map<DataSourceDetailModel, DataSourceDetailMapper>(dataSource);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return new OutputResult<object>(dataSourceRepository.SelectDataSourceDetail(entity, userNumber));
        }

        public OutputResult<object> InsertSaveDataSourceDetail(InsertDataSourceConfigModel dataSource, int userNumber)
        {
            var entity = mapper.Map<InsertDataSourceConfigModel, InsertDataSourceConfigMapper>(dataSource);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return HandleResult(dataSourceRepository.InsertSaveDataSourceDetail(entity, userNumber));
        }

        public OutputResult<object> UpdateSaveDataSourceDetail(UpdateDataSourceConfigModel dataSource, int userNumber)
        {
            var entity = mapper.Map<UpdateDataSourceConfigModel, UpdateDataSourceConfigMapper>(dataSource);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return HandleResult(dataSourceRepository.UpdateSaveDataSourceDetail(entity, userNumber));
        }
        public OutputResult<object> DataSourceDelete(DataSrcDeleteModel dataSource, int userNumber)
        {
            var entity = mapper.Map<DataSrcDeleteModel, DataSrcDeleteMapper>(dataSource);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            //这里需要检查数据源是否被引用了
            if (dataSourceRepository.checkDataSourceInUsed(entity.DataSrcId, userNumber, null))
            {
                OperateResult r = new OperateResult();
                r.Codes = "-1";
                r.Msg = "数据源已经被引用，不能删除";
                return HandleResult(r);
            }
            return HandleResult(dataSourceRepository.DataSourceDelete(entity, userNumber));
        }
        public OutputResult<object> SelectFieldDicType(int userNumber)
        {
            return new OutputResult<object>(dataSourceRepository.SelectFieldDicType(userNumber));
        }

        public OutputResult<object> SelectFieldDicTypeDetail(string dicTypeId, int userNumber)
        {
            return new OutputResult<object>(dataSourceRepository.SelectFieldDicTypeDetail(dicTypeId, userNumber));
        }

        public OutputResult<object> SelectFieldConfig(string dicTypeId, int userNumber)
        {
            return new OutputResult<object>(dataSourceRepository.SelectFieldConfig(dicTypeId, userNumber));
        }
    
        public OutputResult<object> SelectFieldDicVaue(DictionaryModel dic, int userNumber)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            var dicTypeId = dataSourceRepository.HasParentDicType(dic.DicTypeId);
            var dicData = dataSourceRepository.SelectFieldDicVaue(dic.DicTypeId, userNumber); //当前
            if (!string.IsNullOrEmpty(dicTypeId))
            {
                var parentDic = new Dictionary<string, object>();
                var parentData = dataSourceRepository.SelectFieldDicVaue(Convert.ToInt32(dicTypeId), userNumber); //上级
                parentDic.Add("Parent", parentData);
                result.Add(parentDic);
                foreach (var item in parentData)
                {
                    var data = dicData.Where(r => r.RelateDataId == item.DicTypeId).ToList();
                    dictionary.Add(item.DataVal, data);
                }
                result.Add(dictionary);
                return new OutputResult<object>(result);
            }
            return new OutputResult<object>(dicData);
        }

        public OutputResult<object> SaveFieldDicType(DictionaryTypeModel option, int userNumber)
        {
            var entity = mapper.Map<DictionaryTypeModel, DictionaryTypeMapper>(option);
            var falg = dataSourceRepository.SaveFieldDicType(entity, userNumber);
            if (falg)
                return new OutputResult<object>(null, "保存成功");
            else
                return new OutputResult<object>(null, "保存失败", 1);
        }

        public OutputResult<object> UpdateFieldDicType(UpdateDicTypeParam param, int userNumber)
        {
            var falg = dataSourceRepository.UpdateFieldDicType(param.DicTypeId, param.RelateDicTypeId, userNumber);
            if (falg)
                return new OutputResult<object>(null, "更新成功");
            else
                return new OutputResult<object>(null, "更新失败", 1);
        }

        public OutputResult<object> UpdateFieldConfig(UpdateFieldConfigParam data, int userNumber)
        {
            var falg = dataSourceRepository.UpdateFieldConfig(data.DicTypeId, data.FieldConfig, userNumber);
            if (falg)
                return new OutputResult<object>(null, "更新成功");
            else
                return new OutputResult<object>(null, "更新失败", 1);
        }
        public OutputResult<object> SaveFieldOptValue(DictionaryModel option, int userNumber)
        {
            var entity = mapper.Map<DictionaryModel, DictionaryMapper>(option);
            var res = HandleResult(dataSourceRepository.SaveFieldOptValue(entity, userNumber));
            IncreaseDataVersion(DataVersionType.DicData, null);
            return res;
        }
        public OutputResult<object> DisabledDicType(DictionaryDisabledModel dic, int userNumber)
        {
            var res = HandleResult(dataSourceRepository.DisabledDicType(dic.DicTypeId, userNumber));
            IncreaseDataVersion(DataVersionType.DicData, null);
            return res;
        }
        public OutputResult<object> DeleteFieldOptValue(DictionaryModel option, int userNumber)
        {
            var res = HandleResult(dataSourceRepository.DeleteFieldOptValue(option.DicId, userNumber));
            IncreaseDataVersion(DataVersionType.DicData, null);
            return res;
        }

        public OutputResult<object> OrderByFieldOptValue(ICollection<DictionaryModel> options, int userNumber)
        {
            string dicIds = string.Join(",", options.Select(t => t.DicId));
            var res = HandleResult(dataSourceRepository.OrderByFieldOptValue(dicIds, userNumber));
            IncreaseDataVersion(DataVersionType.DicData, null);
            return res;

        }





        public OutputResult<object> DynamicDataSrcQuery(DynamicDataSrcModel entityModel, int userNumber)
        {
            var entity = mapper.Map<DynamicDataSrcModel, DynamicDataSrcMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var dataSourceInfo = dataSourceRepository.GetDataSourceInfo(Guid.Parse(entity.SourceId), userNumber);
            List<DynamicEntityFieldSearch> searchFields = dynamicEntityRepository.GetEntityFields(dataSourceInfo.entityid, userNumber);
            Dictionary<string, object> dic = new Dictionary<string, object>();
            Dictionary<string, object> dicExtraData = new Dictionary<string, object>();
            entity.QueryData.ForEach(a =>
            {
                searchFields.ForEach(t =>
                {
                    if (a.ContainsKey(t.FieldName))
                    {
                        if (a["islike"] != null)
                        {
                            if (a["islike"].ToString() == "0")
                            {
                                t.IsLike = 0;
                            }
                            else
                            {
                                t.IsLike = 1;
                            }
                        }
                        if (!dic.Keys.Contains(t.FieldName))
                            dic.Add(t.FieldName, a[t.FieldName]);
                    }

                });
                if (a.Keys.Count == 0)
                    return;
                bool isExist = a.Keys.Any(t => t == "islike");
                if (!isExist) return;
                if (a["islike"] == null || string.IsNullOrEmpty(a["islike"].ToString())) return;
                string islikeVal = a["islike"].ToString();
                foreach (var tmp in a.Keys.Where(t => t != "islike"))
                {
                    if (islikeVal == "0")
                    {
                        if (!dicExtraData.Keys.Contains(tmp) && !dic.Keys.Contains(tmp))
                            dicExtraData.Add(tmp, tmp + " = '" + a[tmp] + "'");
                    }
                    else
                    {
                        if (!dicExtraData.Keys.Contains(tmp) && !dic.Keys.Contains(tmp))
                            dicExtraData.Add(tmp, tmp + " ilike '%" + a[tmp] + "%'");
                    }

                }

            });
            var validResults = DynamicProtocolHelper.AdvanceQuery(searchFields, dic);
            var validTips = new List<string>();
            var data = new Dictionary<string, string>();


            foreach (DynamicProtocolValidResult validResult in validResults.Values)
            {
                if (!validResult.IsValid)
                {
                    validTips.Add(validResult.Tips);
                }
                data.Add(validResult.FieldName, validResult.FieldData.ToString());

            }

            if (validTips.Count > 0)
            {
                return ShowError<object>(string.Join(";", validTips));
            }


            if (data.Count > 0)
            {
                entity.SqlWhere = " AND " + string.Join(" AND ", data.Values.ToArray());
            }
            if (dicExtraData.Count > 0)
            {
                entity.SqlWhere += " AND " + string.Join(" AND ", dicExtraData.Values.ToArray());
            }
            return new OutputResult<object>(dataSourceRepository.DynamicDataSrcQuery(entity, userNumber));
        }
    }
}
