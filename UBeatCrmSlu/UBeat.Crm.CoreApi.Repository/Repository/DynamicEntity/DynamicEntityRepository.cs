using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;
using Newtonsoft.Json;
using NpgsqlTypes;

namespace UBeat.Crm.CoreApi.Repository.Repository.DynamicEntity
{
    public class DynamicEntityRepository : RepositoryBase, IDynamicEntityRepository
    {


        public Guid getGridTypeByMainType(Guid typeId, Guid entityId)
        {
            string cmdText = "Select categoryid from crm_sys_entity_category where relcategoryid = @relcategoryid and EntityId=@entityid";
            var param = new DbParameter[] {
                new NpgsqlParameter("relcategoryid",typeId),
                new NpgsqlParameter("entityid",entityId)
            };
            object obj = ExecuteScalar(cmdText, param);
            if (obj != null)
            {
                Guid ret = new Guid();
                if (Guid.TryParse(obj.ToString(), out ret))
                {
                    return ret;
                }
            }
            return Guid.Empty;

        }
        public List<DynamicEntityDataFieldMapper> GetTypeFields(Guid typeId, int operateType, int userNumber)
        {
            var procName =
              "SELECT crm_func_entity_protocol_type_fields(@typeId,@operateType,@userNo)";

            var param = new
            {
                TypeId = typeId,
                OperateType = operateType,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor<DynamicEntityDataFieldMapper>(procName, param, CommandType.Text);

            return result;
        }

        public OperateResult DynamicAdd(DbTransaction tran, Guid typeId, Dictionary<string, object> fieldData, Dictionary<string, object> extraData, int userNumber)
        {
            //插入操作
            //生成插入语句
            var sql = @"
                SELECT * FROM crm_func_entity_protocol_data_insert(@typeId,@extradata,@fieldData, @userNo)
            ";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("typeid",typeId),
                new NpgsqlParameter("extradata",JsonHelper.ToJson(extraData)),
                new NpgsqlParameter("fieldData", JsonHelper.ToJson(fieldData)),
                new NpgsqlParameter("userNo",userNumber)
            };
            List<OperateResult> result;
            if (tran == null)
            {
                result = DBHelper.ExecuteQuery<OperateResult>("", sql, param);
                return result.FirstOrDefault();
            }

            result = DBHelper.ExecuteQuery<OperateResult>(tran, sql, param);

            return result.FirstOrDefault();
        }
        public void DynamicAddList(List<DynamicEntityAddListMapper> data, int userNumber)
        {

            var executeSql = "SELECT * FROM crm_func_entity_protocol_data_insert(@typeId,@extradata,@fieldData,@userNo)";
            List<dynamic> args = new List<dynamic>();
            foreach (var m in data)
            {
                args.Add(new
                {
                    TypeId = m.TypeId,
                    FieldData = JsonHelper.ToJson(m.FieldData),
                    ExtraData = JsonHelper.ToJson(m.ExtraData),
                    userno = userNumber
                });
            }
            DataBaseHelper.ExecuteNonQuery(executeSql, args);

        }



        public OperateResult DynamicEdit(DbTransaction tran, Guid typeId, Guid recId, Dictionary<string, object> fieldData, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_protocol_data_update(@typeId,@fieldData,@recId,@userNo)
            ";
            var param = new DbParameter[]
           {
                new NpgsqlParameter("typeId",typeId),
                new NpgsqlParameter("fieldData",JsonHelper.ToJson(fieldData)),
                new NpgsqlParameter("recId", recId),
                new NpgsqlParameter("userNo",userNumber)
           };
            if (tran == null)
                return DBHelper.ExecuteQuery<OperateResult>("", sql, param).FirstOrDefault();

            var result = DBHelper.ExecuteQuery<OperateResult>(tran, sql, param);

            return result.FirstOrDefault();

        }

        public Dictionary<string, List<IDictionary<string, object>>> DataList(PageParam pageParam, Dictionary<string, object> extraData, DynamicEntityListMapper searchParm, int userNumber)
        {
            var procName =
          "SELECT crm_func_entity_protocol_data_list(@entityid,@menuid,@fieldsquery,@sortedby,@viewtype,@extradata,@pageIndex,@pageSize,@needPower,@userNo)";

            var dataNames = new List<string> { "PageData", "PageCount" };

            var param = new
            {
                EntityId = searchParm.EntityId,
                MenuId = searchParm.MenuId,
                FieldsQuery = searchParm.SearchQuery,
                SortedBy = searchParm.SearchOrder,
                ViewType = searchParm.ViewType,
                ExtraData = JsonHelper.ToJson(extraData),
                PageIndex = pageParam.PageIndex,
                PageSize = pageParam.PageSize,
                NeedPower = searchParm.NeedPower ?? 1,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public Dictionary<string, List<IDictionary<string, object>>> DataListUseFunc(string funcName, PageParam pageParam, Dictionary<string, object> extraData, DynamicEntityListMapper searchParm, int userNumber)
        {
            var param = new
            {
                EntityId = searchParm.EntityId,
                MenuId = searchParm.MenuId,
                FieldsQuery = searchParm.SearchQuery,
                SortedBy = searchParm.SearchOrder,
                ViewType = searchParm.ViewType,
                ExtraData = JsonHelper.ToJson(extraData),
                PageIndex = pageParam.PageIndex,
                PageSize = pageParam.PageSize,
                NeedPower = searchParm.NeedPower ?? 1,
                UserNo = userNumber
            };
            var procName = string.Format("SELECT {0}(@entityid,@menuid,@fieldsquery,@sortedby,@viewtype,@extradata,@pageIndex,@pageSize,@needPower,@userNo)", funcName);

            var dataNames = new List<string> { "PageData", "PageCount" };
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        /// <summary>
        /// 获取查询列表时是否有特殊处理函数的方法
        /// </summary>
        /// <param name="entityid"></param>
        /// <returns></returns>
        public string CheckDataListSpecFunction(Guid entityid)
        {
            string strSQL = string.Format("select funcname from crm_sys_entity_func_event where typeid = '{0}' AND operatetype = 3 ", entityid.ToString());
            List<Dictionary<string, object>> specFuncList = ExecuteQuery(strSQL, new DbParameter[] { }, null);
            if (specFuncList != null && specFuncList.Count > 0 && specFuncList[0]["funcname"] != null)
            {
                return (string)specFuncList[0]["funcname"];
            }
            return null;
        }




        public IDictionary<string, object> Detail(DynamicEntityDetailtMapper detailMapper, int userNumber, DbTransaction transaction = null)
        {
            var procName =
              "SELECT crm_func_entity_protocol_data_detail(@entityid,@recid,@needpower,@userNo)";

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("entityid", detailMapper.EntityId));
            sqlParameters.Add(new NpgsqlParameter("recid", detailMapper.RecId));
            sqlParameters.Add(new NpgsqlParameter("needpower", detailMapper.NeedPower ?? 1));
            sqlParameters.Add(new NpgsqlParameter("userNo", userNumber));

            //var result = DBHelper.ExecuteQueryRefCursor("", procName, sqlParameters.ToArray());
            var result = ExecuteQueryRefCursor(procName, sqlParameters.ToArray(), transaction);
            if (result.Count == 0 || result.FirstOrDefault().Value == null)
                throw new Exception("无结果返回");
            return result.FirstOrDefault().Value.FirstOrDefault();


        }

        public List<IDictionary<string, object>> DetailList(DynamicEntityDetailtListMapper detailMapper, int userNumber)
        {
            var recIds = detailMapper.RecIds.Split(',');
            var datalist = new List<IDictionary<string, object>>();
            foreach (var recid in recIds)
            {
                DynamicEntityDetailtMapper mapper = new DynamicEntityDetailtMapper();
                mapper.EntityId = detailMapper.EntityId;
                mapper.NeedPower = detailMapper.NeedPower;
                mapper.RecId = new Guid(recid);

                var result = Detail(mapper, userNumber);
                datalist.Add(result);
            }
            return datalist;
        }

        public List<DynamicEntityFieldSearch> GetSearchFields(Guid entityId, int userNumber)
        {
            var procName =
             @"
          	SELECT s.entityid,s.fieldid,s.controltype AS newtype,s.islike,f.fieldname,f.fieldlabel,f.displayname,f.controltype,f.fieldconfig FROM crm_sys_entity_search AS s
            LEFT JOIN crm_sys_entity_fields AS f ON s.fieldid = f.fieldid
            WHERE s.entityid = @entityId AND s.recstatus = 1
            ";

            var param = new
            {
                EntityId = entityId,
            };

            var result = DataBaseHelper.Query<DynamicEntityFieldSearch>(procName, param, CommandType.Text);

            return result;
        }

        public List<DynamicEntityFieldSearch> GetEntityFields(Guid entityId, int userNumber)
        {
            var procName =
             @"
            SELECT entityid, fieldid,controltype AS newtype,fieldname,fieldlabel,displayname,controltype,fieldconfig 
            FROM crm_sys_entity_fields WHERE entityid = @entityId AND recstatus = 1
            ";

            var param = new
            {
                EntityId = entityId,
            };

            var result = DataBaseHelper.Query<DynamicEntityFieldSearch>(procName, param, CommandType.Text);

            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> DetailMulti(DynamicEntityDetailtMapper detailMapper, int userNumber)
        {
            var procName =
             "SELECT crm_func_entity_protocol_data_detail(@entityid,@recid,@needpower,@userNo)";

            var param = new
            {
                EntityId = detailMapper.EntityId,
                RecId = detailMapper.RecId,
                NeedPower = detailMapper.NeedPower ?? 1,
                UserNo = userNumber
            };

            var dataNames = new List<string> { "Detail" };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public List<GeneralDicItem> GetDicItemByKeys(string dicKeys)
        {
            var procName =
                "SELECT crm_func_entity_protocol_dictionary_list(@dickeys)";

            var param = new
            {
                DicKeys = dicKeys
            };

            var result = DataBaseHelper.QueryStoredProcCursor<GeneralDicItem>(procName, param, CommandType.Text);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> PluginVisible(DynamicPluginVisibleMapper visibleMapper, int userNumber)
        {
            var procName =
          "SELECT crm_func_entity_dynamic_check_visible(@entityid,@recid,@deviceclassic,@userNo)";

            var param = new
            {
                EntityId = visibleMapper.EntityId,
                RecId = visibleMapper.RecId,
                DeviceClassic = visibleMapper.DeviceClassic,
                UserNo = userNumber
            };

            var dataNames = new List<string> { "ViewHidden" };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> PageVisible(DynamicPageVisibleMapper visibleMapper, int userNumber)
        {
            var procName =
          "SELECT crm_func_entity_page_check_visible(@entityid,@pageCode,@pageType,@recid,@userNo)";

            var param = new
            {
                EntityId = visibleMapper.EntityId,
                PageCode = visibleMapper.PageCode,
                PageType = visibleMapper.PageType,
                RecId = visibleMapper.RecId,
                UserNo = userNumber
            };

            var dataNames = new List<string>();

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult DynamicAdd(DbTransaction tran, Guid typeId, Dictionary<string, object> fieldData, Guid flowId, Guid? relEntityId, Guid? relRecId,
            int userNumber)
        {
            //插入操作
            //生成插入语句
            var sql = @"
                SELECT * FROM crm_func_entity_protocol_data_insert_workflow(@typeId,@fieldData,@flowId,@relEntityId,@relRecId, @userNo)
            ";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("typeId", typeId),
                new NpgsqlParameter("fieldData", JsonHelper.ToJson(fieldData)),
                new NpgsqlParameter("flowId", flowId),
                new NpgsqlParameter("relEntityId", relEntityId.HasValue ? relEntityId.Value.ToString() : ""),
                new NpgsqlParameter("relRecId", relRecId.HasValue ? relRecId.Value.ToString() : ""),
                new NpgsqlParameter("userNo", userNumber)
            };
            if (tran == null)
                return DBHelper.ExecuteQuery<OperateResult>("", sql, param).FirstOrDefault();
            var result = DBHelper.ExecuteQuery<OperateResult>(tran, sql, param).FirstOrDefault();

            return result;
        }




        public Guid DynamicEntityExist(Guid entityid, Dictionary<string, object> fieldData, Guid updateRecid)
        {
            var executeSql = "select * from crm_func_entity_check_repeat(@entityid,@row_data,@_updaterecid)";
            var args = new
            {
                entityid = entityid,
                row_data = JsonHelper.ToJson(fieldData),
                _updaterecid = updateRecid
            };
            var sqlResult = DataBaseHelper.Query(executeSql, args);
            var result = Guid.Empty;

            if (sqlResult != null)
            {
                result = sqlResult.Select(m => new Guid(m["recid"].ToString())).ToList().FirstOrDefault();
            }

            return result;
        }


        public OperateResult Transfer(DynamicEntityTransferMapper transMapper, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_protocol_data_transfer(@entityId,@recId,@manager,@userNo)
            ";

            var param = new
            {
                EntityId = transMapper.EntityId,
                RecId = transMapper.RecId,
                Manager = transMapper.Manager,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult AddConnect(DynamicEntityAddConnectMapper connectMapper, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_connect_relation_add(@entityIdUp,@entityIdTo,@recIdUp,@recIdTo,@remark,@userNo)
            ";

            var param = new
            {
                EntityIdUp = connectMapper.EntityIdUp,
                EntityIdTo = connectMapper.EntityIdTo,
                RecIdUp = connectMapper.RecIdUp,
                RecIdTo = connectMapper.RecIdTo,
                Remark = connectMapper.Remark,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult EditConnect(DynamicEntityEditConnectMapper connectMapper, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_connect_relation_update(@connectId,@entityIdTo,@recIdTo,@userNo)
            ";

            var param = new
            {
                ConnectId = connectMapper.ConnectId,
                EntityIdTo = connectMapper.EntityIdTo,
                RecIdTo = connectMapper.RecIdTo,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult DeleteConnect(Guid connectId, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_connect_relation_delete(@connectId,@userNo)
            ";

            var param = new
            {
                ConnectId = connectId,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public List<IDictionary<string, object>> ConnectList(Guid entityId, Guid recId, int userNumber)
        {
            var procName =
           "SELECT crm_func_entity_connect_relation_list(@entityId,@recId,@userno)";

            var param = new
            {
                EntityId = entityId,
                RecId = recId,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, param, CommandType.Text);
            return result;
        }

        public OperateResult Delete(DbTransaction trans, Guid entityId, string recIds, int pageType, string pageCode, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_protocol_data_delete(@entityId,@recId,@pagetype,@pageCode,@userNo)
            ";


            var param = new DbParameter[]
            {
                new NpgsqlParameter("entityId",entityId),
                new NpgsqlParameter("recId",recIds),
                new NpgsqlParameter("pagetype", pageType),
                new NpgsqlParameter("pageCode",pageCode),
                new NpgsqlParameter("userNo",userNumber)
            };
            return ExecuteQuery<OperateResult>(sql, param, trans).FirstOrDefault();

        }
        public OperateResult DeleteDataSrcRelation(DataSrcDeleteRelationMapper entityMapper, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_datasrc_relation_delete(@relid,@recId,@relrecid,@userNo)
            ";

            var param = new
            {
                RelId = entityMapper.RelId,
                RecId = entityMapper.RecId,
                RelRecId = entityMapper.RelRecId,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public string ReturnRelTabSql(Guid relId, Guid recId, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_return_rel_tab_sql(@relid,@recid,@userNo)
            ";

            var param = new
            {
                RelId = relId,
                RecId = recId,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<string>(sql, param);
            return result;
        }

        public RelTabSrcMapper ReturnRelTabSrcSql(Guid relId)
        {
            var sql = @"
                SELECT relid,entityid,ismanytomany,srcsql FROM crm_sys_entity_rel_tab where relid=@relid;
            ";

            var param = new
            {
                RelId = relId
            };
            var result = DataBaseHelper.QuerySingle<RelTabSrcMapper>(sql, param);
            return result;
        }

        public List<RelTabSrcValListMapper> GetSrcSqlDataList(Guid recId, string srcSql, string searchWhere, int userNumber)
        {

            if (string.IsNullOrEmpty(srcSql))
                return null;
            else
            {
                srcSql += searchWhere;
            }
            var param = new
            {
                RecId = recId
            };

            var result = DataBaseHelper.Query<RelTabSrcValListMapper>(srcSql, param, CommandType.Text);
            return result;
        }

        public List<RelTabSrcValListMapper> AddSrcSqlDataRelation(Guid recId, string srcSql, string searchWhere, int userNumber)
        {

            if (string.IsNullOrEmpty(srcSql))
                return null;
            else
            {
                srcSql += searchWhere;
            }
            var param = new
            {
                RecId = recId
            };

            var result = DataBaseHelper.Query<RelTabSrcValListMapper>(srcSql, param, CommandType.Text);
            return result;
        }

        public int initDefaultTab(RelTabListMapper entity, int userNumber)
        {
            string insertSql = "INSERT into crm_sys_entity_rel_tab " +
                "select uuid_generate_v4() as relid,@entityid,null as relentityid,a.relname,a.icon," +
                " a.recorder,a.recstatus,now() as reccreated,now() as recupdated, a.reccreator,a.recupdator,a.recversion,null as fieldid,a.entitytaburl,a.tabtype,a.web,a.mob  " +
                " from crm_sys_entity_default_tab a where (a.entityid is null or a.entityid = @entityid ) and a.recstatus = 1 order by a.recorder";
            List<dynamic> args = new List<dynamic>();
            args.Add(new
            {
                entityid = entity.EntityId
            });
            var result = DataBaseHelper.ExecuteNonQuery(insertSql, args);
            return 1;
        }

        public Dictionary<string, List<IDictionary<string, object>>> RelTabListQuery_1(RelTabListMapper entity, int userNumber)
        {
            var procName =
                "SELECT crm_func_rel_tab_list(@entityid , @userno)";

            var dataNames = new List<string> { "RelTabList" };
            var param = new DynamicParameters();
            param.Add("entityid", entity.EntityId);

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public Dictionary<string, List<IDictionary<string, object>>> RelTabInfoQuery_1(RelTabInfoMapper entity, int userNumber)
        {
            var procName =
                "SELECT crm_func_entity_info(@relid, @userno)";

            var dataNames = new List<string> { "RelTabInfo" };
            var param = new
            {
                EntityId = entity.RelId,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public OperateResult AddRelTab_1(AddRelTabMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_rel_tab_add(@entityid,@relentityid, @relname, @icon,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("entityid", entity.EntityId);
            param.Add("relentityid", entity.RelEntityId);
            param.Add("relname", entity.RelName);
            param.Add("icon", entity.ICon);

            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult UpdateRelTab_1(UpdateRelTabMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_rel_tab_update(@relid,@relentityid, @relname, @icon,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("relid", entity.RelId);
            param.Add("relentityid", entity.RelEntityId);
            param.Add("relname", entity.RelName);
            param.Add("icon", entity.ICon);

            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult DisabledRelTab_1(DisabledRelTabMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_rel_tab_disabled(@relid,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("relid", entity.RelId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> RelTabListQuery(RelTabListMapper entity, int userNumber)
        {
            var procName =
                "SELECT crm_func_rel_tab_list(@entityid , @userno)";

            var dataNames = new List<string> { "RelTabList" };
            var param = new DynamicParameters();
            param.Add("entityid", entity.EntityId);
            param.Add("userno", userNumber);

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public Dictionary<string, List<IDictionary<string, object>>> RelTabInfoQuery(RelTabInfoMapper entity, int userNumber)
        {
            var procName =
                "SELECT crm_func_rel_tab_info(@relid, @userno)";

            var dataNames = new List<string> { "RelTabInfo" };
            var param = new
            {
                relid = entity.RelId,
                userno = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public List<IDictionary<string, object>> GetRelTabEntity(RelTabListMapper entity, int userNumber)
        {
            string sql = "select distinct a.entityid,a.entityname from crm_sys_entity a inner join crm_sys_entity_fields b on a.entityid=b.entityid " +
                "inner join crm_sys_entity_datasource c on c.datasrcid::TEXT = jsonb_extract_path_text(b.fieldconfig, 'dataSource', 'sourceId') " +
                "inner join crm_sys_entity d on d.entityid = c.entityid  " +
                "where a.recstatus = 1 and b.recstatus=1 and b.controltype = 18  " +
                "and (d.entityid =@entityid  or (d.entityid='ac051b46-7a20-4848-9072-3b108f1de9b0'::uuid and entityid='f9db9d79-e94b-4678-a5cc-aa6e281c1246'::uuid)) and a.entityid != @entityid  group by a.entityid,a.entityname ";
            var param = new DynamicParameters();
            param.Add("entityid", entity.EntityId);
            return DataBaseHelper.Query(sql, param);
        }

        public List<IDictionary<string, object>> GetRelEntityFields(GetEntityFieldsMapper entity, int userNumber)
        {
            string sql = "SELECT f.fieldid,f.fieldlabel datasrcname,f.entityid relEntityId,h.icons,e.datasrcid  " +
                " FROM crm_sys_entity_fields AS f " +
                " inner join crm_sys_entity_datasource e on e.datasrcid::TEXT = jsonb_extract_path_text(f.fieldconfig, 'dataSource', 'sourceId') " +
                " inner join crm_sys_entity h on e.entityid = h.entityid " +
                " WHERE f.controltype = 18 AND f.recstatus = 1 and f.entityid = @relentityid and " +
                "  (e.entityid =@entityid or (e.entityid='ac051b46-7a20-4848-9072-3b108f1de9b0'::uuid and 'f9db9d79-e94b-4678-a5cc-aa6e281c1246'::uuid=@entityid)) ";
            var param = new DynamicParameters();
            param.Add("entityid", entity.EntityId);
            param.Add("relentityid", entity.RelEntityId);
            return DataBaseHelper.Query(sql, param);
        }

        public List<IDictionary<string, object>> GetRelConfigFields(GetEntityFieldsMapper entity, int userNumber)
        {
            string sql = @"SELECT controltype,fieldid,fieldlabel,fieldname,displayname
	                FROM crm_sys_entity_fields
	                WHERE (controltype='1001' OR controltype=6 OR controltype=7)
	                AND entityid=@relentityid
	                AND recstatus=1;";
            var param = new DynamicParameters();
            param.Add("relentityid", entity.RelEntityId);
            return DataBaseHelper.Query(sql, param);
        }

        public OperateResult AddRelTab(AddRelTabMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_rel_tab_add(@entityid,@relentityid, @fieldid,@relname, @icon,@ismanytomany,@srcsql,@srctitle,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("entityid", entity.EntityId);
            param.Add("relentityid", entity.RelEntityId);
            param.Add("fieldid", entity.FieldId);
            param.Add("relname", entity.RelName);
            param.Add("icon", entity.ICon);
            param.Add("ismanytomany", entity.IsManyToMany);
            param.Add("srcsql", entity.SrcSql);
            param.Add("srctitle", entity.SrcTitle);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult SaveRelConfig(List<RelConfig> configs, Guid RelId, int userNumber)
        {
            //清除历史配置
            string delSql = @"DELETE from crm_sys_entity_rel_config where relid=@relid;";
            var delParam = new DbParameter[]
            {
                new NpgsqlParameter("relid",RelId)
            };
            ExecuteQuery(delSql, delParam);
            string sql = @"INSERT into crm_sys_entity_rel_config(type,relid,relentityid,fieldid,calcutetype,entityid,index,func)  
                        values (@type,@relid,@relentityid,@fieldid,@calcutetype,@entityid,@index,@func)";
            Boolean isSuf = true;
            foreach (var item in configs) {
                var param = new DbParameter[]
                {
                        new NpgsqlParameter("type",item.Type),
                        new NpgsqlParameter("relid",RelId),
                        new NpgsqlParameter("relentityid",item.RelentityId),
                        new NpgsqlParameter("fieldid",item.FieldId),
                        new NpgsqlParameter("calcutetype",item.CalcuteType),
                        new NpgsqlParameter("entityid",item.EntityId),
                        new NpgsqlParameter("index",item.Index),
                        new NpgsqlParameter("func",item.Func)
                };
                int count = ExecuteNonQuery(sql, param);
                if (count == 0)
                    isSuf = false;
            }
            if (isSuf)
            {
                return new OperateResult()
                {
                    Flag = 1,
                    Msg = "新增配置成功"
                };
            }
            else {
                return new OperateResult()
                {
                    Flag = 0,
                    Msg = "新增配置失败"
                };
            }
        }


        public List<IDictionary<string, object>> GetRelConfigEntity(RelTabListMapper entity, int userNumber)
        {
            string sql = @"select distinct a.entityid,a.entityname from crm_sys_entity a inner join crm_sys_entity_fields b on a.entityid=b.entityid 
                inner join crm_sys_entity_datasource c on c.datasrcid::TEXT = jsonb_extract_path_text(b.fieldconfig, 'dataSource', 'sourceId')
                inner join crm_sys_entity d on d.entityid = c.entityid  
                where a.recstatus = 1 and b.recstatus=1 and b.controltype = 18  
                and d.entityid =@entityid  and a.entityid != @entityid  group by a.entityid,a.entityname 
                UNION all
                select  b.entityid,b.entityname from crm_sys_entity b where b.recstatus=1 and b.entityid =@entityid ";
            var param = new DynamicParameters();
            param.Add("entityid", entity.EntityId);
            return DataBaseHelper.Query(sql, param);
        }
        public RelConfigInfo GetRelConfig(Guid RelId, int userNumber)
        {
            string getConfigSql = "select * from crm_sys_entity_rel_config a where a.relid=@relid";
            string getSetSql = "select * from crm_sys_entity_rel_config_set a where a.relid=@relid";
            var param = new DbParameter[]
            {
                 new NpgsqlParameter("relid",RelId)
            };
            var relConfig = ExecuteQuery<RelConfig>(getConfigSql, param);
            var relConfigSet = ExecuteQuery<RelConfigSet>(getSetSql, param);
            RelConfigInfo info = new RelConfigInfo
            {
                RelId = RelId,
                Configs = relConfig,
                ConfigSets = relConfigSet
            };
            return info;
        }

        public decimal queryDataForDataSource_funcType(RelConfig config, Guid parentRecId, int userNumber)
        {
            var sql = @"
                SELECT * FROM " + config.Func + "(@relid,@parentRecId)";

            var param = new DbParameter[]
            {
              new NpgsqlParameter("relid",config.RelId),
              new NpgsqlParameter("parentRecId",parentRecId)
            };
            var ret = ExecuteQuery(sql, null).FirstOrDefault();
            return (decimal)ret["result"]; ;
        }

        public decimal queryDataForDataSource_CalcuteType(RelConfig config, Guid parentRecId, int userNumber)
        {
            string parentEntitySql = @"select c.entitytable,b.fieldname,b.controltype from crm_sys_entity_rel_tab a 
                                         inner JOIN crm_sys_entity c on c.entityid=a.relentityid
                                         inner join crm_sys_entity_fields b on  b.fieldid=a.fieldid
                                          where c.entityid=@entityid ";
            var param = new DbParameter[]
            {
              new NpgsqlParameter("entityid",config.RelentityId)
            };
            var parentTableInfo = ExecuteQuery(parentEntitySql, param).FirstOrDefault();

            string childfieldSql = @"select a.fieldname  from crm_sys_entity_fields a where  a.fieldid=@fieldid";
            var param1 = new DbParameter[]
            {
              new NpgsqlParameter("fieldid",config.FieldId)
            };
            var childTableInfo = ExecuteQuery(childfieldSql, param1).FirstOrDefault();
            string whereClause = "";
            string selectClause = "";
            whereClause = string.Format(@"jsonb_extract_path_text({0},'id')='{1}' ", parentTableInfo["fieldname"], parentRecId);
            //0直接取值1求和2求平均3计数
            if (config.CalcuteType == 0)
            {
                //直接取值只能是数字
                if (parentTableInfo["controltype"].ToString() == "1001")
                {
                    return new decimal(0);
                }
                else
                {
                    selectClause = childTableInfo["fieldname"].ToString() + "::decimal";
                }
            }
            else if (config.CalcuteType == 1)
            {
                selectClause = string.Format(@"COALESCE(SUM({0}),0):: DECIMAL ", childTableInfo["fieldname"]);
            }
            else if (config.CalcuteType == 2) {
                selectClause = string.Format(@"COALESCE(avg({0}),0):: DECIMAL ", childTableInfo["fieldname"]);
            } else if (config.CalcuteType == 3) {
                selectClause = string.Format(@"count({0})::decimal ", childTableInfo["fieldname"]);
            } else {
                return new decimal(0);
            }
            string expressionSqlStr = string.Format(@"select {0} as result from {1}  where  {2}", selectClause, parentTableInfo["entitytable"], whereClause);
            var ret = ExecuteQuery(expressionSqlStr, null).FirstOrDefault();
            return (decimal)ret["result"];
        }

        public OperateResult SaveRelConfigSet(List<RelConfigSet> configSets, Guid RelId, int userNumber)
        {
            //清除历史配置
            string delSql = @"DELETE from crm_sys_entity_rel_config_set where relid=@relid;";
            var delParam = new DbParameter[]
            {
                new NpgsqlParameter("relid",RelId)
            };
            ExecuteQuery(delSql, delParam);
            string sql = @"INSERT into crm_sys_entity_rel_config_set(relid,configset1,title1,configset2,title2,configset3,title3,configset4,title4)  
                        values (@relid,@configset1,@title1,@configset2,@title2,@configset3,@title3,@configset4,@title4)";
            Boolean isSuf = true;
            foreach (var item in configSets)
            {
                var param = new DbParameter[]
                {
                        new NpgsqlParameter("relid",RelId),
                        new NpgsqlParameter("configset1",item.ConfigSet1),
                        new NpgsqlParameter("title1",item.title1),
                        new NpgsqlParameter("configset2",item.ConfigSet2),
                        new NpgsqlParameter("title2",item.title2),
                        new NpgsqlParameter("configset3",item.ConfigSet3),
                        new NpgsqlParameter("title3",item.title3),
                        new NpgsqlParameter("configset4",item.ConfigSet4),
                        new NpgsqlParameter("title4",item.title4)
                };
                int count = ExecuteNonQuery(sql, param);
                if (count == 0)
                    isSuf = false;
            }
            if (isSuf)
            {
                return new OperateResult()
                {
                    Flag = 1,
                    Msg = "新增配置计算公式成功"
                };
            }
            else
            {
                return new OperateResult()
                {
                    Flag = 0,
                    Msg = "新增配置计算公式失败"
                };
            }
        }

        public OperateResult UpdateRelTab(UpdateRelTabMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_rel_tab_update(@relid,@relentityid, @fieldid,@relname, @icon,@ismanytomany,@srcsql,@srctitle,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("relid", entity.RelId);
            param.Add("relentityid", entity.RelEntityId);
            param.Add("fieldid", entity.FieldId);
            param.Add("relname", entity.RelName);
            param.Add("icon", entity.ICon);
            param.Add("ismanytomany", entity.IsManyToMany);
            param.Add("srcsql", entity.SrcSql);
            param.Add("srctitle", entity.SrcTitle);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public int UpdateDefaultRelTab(UpdateRelTabMapper entity, int userNumber)
        {
            var sql = "update crm_sys_entity_rel_tab set relname=@relname where relid=@relid;" +
                "        UPDATE crm_sys_function SET funcname=@relname WHERE relationvalue=@relid::TEXT; ";
            var param = new DynamicParameters();
            param.Add("relid", entity.RelId);
            param.Add("relname", entity.RelName);
            return DataBaseHelper.ExecuteNonQuery(sql, param);
        }

        public OperateResult DisabledRelTab(DisabledRelTabMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_rel_tab_disabled(@relid,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("relid", entity.RelId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        public OperateResult OrderbyRelTab(OrderbyRelTabMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_rel_tab_orderby(@relids,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("relids", entity.RelIds);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult AddRelTabRelationDataSrc(AddRelTabRelationDataSrcMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM reconvert_to_datasrccontrol(@relid, @recid,@idstrs,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("relid", entity.RelId);
            param.Add("recid", entity.RecId);
            param.Add("idstrs", entity.IdStr);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public List<IDictionary<string, object>> EntitySearchList(int modelType, Dictionary<string, object> searchData, int userNumber)
        {
            var procName =
            "SELECT crm_func_entity_search_list(@modelType,@searchData,@userno)";

            var param = new
            {
                ModelType = modelType,
                SearchData = JsonHelper.ToJson(searchData),
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, param, CommandType.Text);
            return result;
        }

        public List<IDictionary<string, object>> EntitySearchRepeat(Guid entityId, string checkField, string checkName, int extra, Dictionary<string, object> searchData, int userNumber)
        {
            var procName =
           "SELECT crm_func_entity_search_repeat(@entityId,@checkField,@checkName,@searchData,@extra,@userno)";

            var param = new
            {
                EntityId = entityId,
                CheckField = checkField,
                CheckName = checkName,
                SearchData = JsonHelper.ToJson(searchData),
                Extra = extra,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, param, CommandType.Text);
            return result;
        }

        public List<DynamicEntityWebFieldMapper> GetWebFields(Guid typeId, int operateType, int userNumber)
        {

            var procName = "SELECT crm_func_entity_protocol_type_fields_web(@typeId,@operateType,@userNo)";

            var param = new
            {
                TypeId = typeId,
                OperateType = operateType,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor<DynamicEntityWebFieldMapper>(procName, param, CommandType.Text);

            return result;
        }

        public List<DynamicEntityWebFieldMapper> GetWebDynamicListFields(Guid typeId, int operateType, int userNumber)
        {

            var procName = "SELECT crm_func_entity_protocol_type_fields_web_for_dynamiclist(@typeId,@operateType,@userNo)";

            var param = new
            {
                TypeId = typeId,
                OperateType = operateType,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor<DynamicEntityWebFieldMapper>(procName, param, CommandType.Text);

            return result;
        }

        public OperateResult IsHasPerssion(PermissionMapper entity, int userNumber)
        {

            var procName =
  "SELECT * from  crm_func_role_entity_has_permission(@entityid,@recid,@userNo)";

            var param = new
            {
                EntityId = entity.EntityId,
                RecId = entity.RecId,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QuerySingle<OperateResult>(procName, param, CommandType.Text);

            return result;
        }


        //获取数据源控件类型的所有字段
        public List<EntityFieldInfo> GetDataSourceEntityFields()
        {
            List<EntityFieldInfo> resutl = new List<EntityFieldInfo>();
            var sql = @"SELECT f.*,e.entitytable AS EntityTableName FROM crm_sys_entity_fields AS f
                        INNER JOIN crm_sys_entity AS e ON e.entityid=f.entityid
                        WHERE f.controltype=18 AND f.recstatus=1";

            var sqlParameters = new List<DbParameter>();
            //sqlParameters.Add(new NpgsqlParameter("entityid", entityid));

            resutl = DBHelper.ExecuteQuery<EntityFieldInfo>("", sql, sqlParameters.ToArray());

            return resutl;
        }
        public Dictionary<string, object> getEntityBaseInfoById(Guid entityid, int userNum)
        {
            try
            {
                string cmdText = "Select * from crm_sys_entity where entityid=@entityid";
                var param = new DbParameter[] {
                    new NpgsqlParameter("entityid",entityid)
                };
                List<Dictionary<string, object>> ret = ExecuteQuery(cmdText, param);
                if (ret != null && ret.Count > 0) return ret[0];
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public bool WriteBack(DbTransaction tran, List<Dictionary<string, object>> writebackrules, int userNum)
        {
            if (writebackrules == null || writebackrules.Count == 0) return true;
            Dictionary<string, List<Dictionary<string, object>>> entityWriteBack = new Dictionary<string, List<Dictionary<string, object>>>();

            foreach (Dictionary<string, object> item in writebackrules)
            {
                string entityid = null;
                if (item.ContainsKey("entityid") == false) continue;
                entityid = item["entityid"].ToString();
                if (item.ContainsKey("recid") == false || item.ContainsKey("fieldname") == false || item.ContainsKey("fieldvalue") == false) continue;
                if (entityid == null || entityid.Length == 0) continue;
                if (entityWriteBack.ContainsKey(entityid))
                {
                    List<Dictionary<string, object>> list = entityWriteBack[entityid];
                    list.Add(item);
                }
                else
                {
                    List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                    list.Add(item);
                    entityWriteBack.Add(entityid, list);
                }
            }
            foreach (string entityid in entityWriteBack.Keys)
            {
                List<Dictionary<string, object>> fieldList = entityWriteBack[entityid];
                Dictionary<string, object> entityInfo = this.getEntityBaseInfoById(Guid.Parse(entityid), userNum);
                if (entityInfo == null) continue;
                string tableName = (string)entityInfo["entitytable"];


                foreach (Dictionary<string, object> item in fieldList)
                {
                    string updateSQL = "";
                    string recid = "";
                    if (item.ContainsKey("recid") == false) continue;
                    recid = item["recid"].ToString();
                    string fieldname = item["fieldname"].ToString();
                    object fieldobject = item["fieldvalue"];
                    string fieldValue = "";
                    bool isFieldValueNull = false;
                    bool isFieldGuid = false;
                    if (fieldobject == null)
                    {
                        isFieldValueNull = true;
                    }
                    else
                    {
                        isFieldValueNull = false;
                        fieldValue = item["fieldvalue"].ToString();
                    }
                    if (item.ContainsKey("isguid"))
                    {
                        if (item["isguid"].ToString() == "1")
                        {
                            isFieldGuid = true;
                        }
                    }
                    if (isFieldValueNull)
                    {
                        updateSQL = string.Format(@" {0}=null ", fieldname);
                    }
                    else
                    {
                        if (isFieldGuid)
                        {
                            updateSQL = string.Format(@"  {0}='{1}'::uuid ", fieldname, fieldValue);
                        }
                        else
                        {
                            updateSQL = string.Format(@" {0}='{1}' ", fieldname, fieldValue);
                        }
                    }
                    string cmdText = string.Format("update {0} set {1} where recid='{2}'::uuid ", tableName, updateSQL, recid);
                    if (tran == null)
                        ExecuteNonQuery(cmdText, new DbParameter[] { });
                    else
                    {
                        DBHelper.ExecuteNonQuery(tran, cmdText, new DbParameter[] { });
                    }
                }
            }
            return true;
        }


        public OperateResult FollowRecord(FollowRecordMapper entity, int userNumber)
        {

            if (entity.IsFollow)
            {
                int flag = 0;
                if (!HasFollowed(entity, userNumber))
                {
                    var sql = @" INSERT INTO  crm_sys_entity_record_follow(entityid,followdataid,userid,reccreator)VALUES(@entityid,@followdataid,@userid,@reccreator) ";
                    var param = new DynamicParameters();
                    param.Add("entityid", entity.EntityId);
                    param.Add("followdataid", entity.RelId);
                    param.Add("userid", userNumber);
                    param.Add("reccreator", userNumber);
                    var result = DataBaseHelper.ExecuteNonQuery(sql, param);
                    if (result > 0)
                    {
                        flag = 1;
                    }
                    else
                    {
                        flag = 0;
                    }
                }
                else
                {
                    flag = 0;
                }

                return new OperateResult() { Flag = flag };

            }
            else
            {

                return UnFollowRecord(entity, userNumber);
            }

        }

        public bool HasFollowed(FollowRecordMapper entity, int userNumber)
        {
            var sql = @" SELECT COUNT(1) FROM crm_sys_entity_record_follow WHERE followdataid=@followdataid AND userid=@userid ";
            var param = new DynamicParameters();
            param.Add("followdataid", entity.RelId);
            param.Add("userid", userNumber);
            var result = DataBaseHelper.QuerySingle<int>(sql, param);
            if (result > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public OperateResult UnFollowRecord(FollowRecordMapper entity, int userNumber)
        {
            var sql = @" DELETE FROM crm_sys_entity_record_follow WHERE followdataid=@followdataid AND userid=@userid";
            var param = new DynamicParameters();
            param.Add("followdataid", entity.RelId);
            param.Add("userid", userNumber);
            var result = DataBaseHelper.ExecuteNonQuery(sql, param);

            int flag = 0;
            if (result > 0)
            {
                flag = 1;
            }
            else
            {
                flag = 0;
            }

            return new OperateResult() { Flag = flag };
        }

        public Dictionary<string, object> GetAllDataSourceDefine(DbTransaction tran)
        {
            string strSQL = "select a.datasrcid ,b.entitytable    from crm_sys_entity_datasource a inner join crm_sys_entity b on a.entityid = b.entityid where a.entityid is not null  and b.entitytable is not null  ";
            List<Dictionary<string, object>> ret = ExecuteQuery(strSQL, new DbParameter[] { }, tran);
            Dictionary<string, object> retObj = new Dictionary<string, object>();
            foreach (Dictionary<string, object> item in ret)
            {
                string recid = null;
                if (item.ContainsKey("datasrcid") && item["datasrcid"] != null
                    && item.ContainsKey("entitytable") && item["entitytable"] != null)
                {
                    string did = item["datasrcid"].ToString();
                    string eid = item["entitytable"].ToString();
                    retObj.Add(did, eid);

                }
            }
            return retObj;
        }

        public new List<Dictionary<string, object>> ExecuteQuery(string strSQL, DbTransaction tran)
        {
            return base.ExecuteQuery(strSQL, new DbParameter[] { }, tran);
        }


        public OperateResult MarkRecordComplete(Guid recId, int userNumber)
        {
            var sql = @" UPDATE crm_sys_engineering SET handle=2 WHERE recid=@recid";

            var param = new DynamicParameters();
            param.Add("recid", recId);
            var result = DataBaseHelper.ExecuteNonQuery(sql, param);

            int flag = 0;
            if (result > 0)
            {
                flag = 1;
            }
            else
            {
                flag = 0;
            }

            return new OperateResult() { Flag = flag };
        }

        /// <summary>
        /// 根据数据库函数名称，获取函数的信息
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="functionname"></param>
        /// <returns></returns>
        public EntityExtFunctionInfo getExtFunctionByFunctionName(Guid entityId, string functionname, DbTransaction tran = null)
        {
            var sql = "Select * from crm_sys_entity_extfunction where functionname=@functionname And entityid=@entityid";
            var param = new DbParameter[] {
                new NpgsqlParameter("functionname",functionname),
                new NpgsqlParameter("entityid",entityId)
            };
            return ExecuteQuery<EntityExtFunctionInfo>(sql, param, tran).FirstOrDefault();
        }

        /// <summary>
        /// 执行数据库函数
        /// </summary>
        /// <param name="functionname"></param>
        /// <param name="recIds"></param>
        /// <param name="otherParams"></param>
        /// <param name="userId"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public object ExecuteExtFunction(EntityExtFunctionInfo funcInfo, string[] recIds, Dictionary<string, object> otherParams, int userId, DbTransaction tran = null)
        {
            string functionname = funcInfo.FunctionName;
            string paramNames = funcInfo.Parameters;
            Dictionary<string, string> realParam = new Dictionary<string, string>();
            if (paramNames != null)
            {
                string[] tmp = paramNames.Split(',');
                foreach (string item in tmp)
                {
                    string tmpitem = item.ToLower();
                    if (tmpitem.StartsWith("@")) tmpitem = tmpitem.Substring(1);
                    if (realParam.ContainsKey(tmpitem)) continue;
                    if (tmpitem.Equals("recids"))
                    {
                        realParam.Add("recids", Newtonsoft.Json.JsonConvert.SerializeObject(recIds));
                    }
                    else if (tmpitem.Equals("userid"))
                    {
                        realParam.Add("userid", userId.ToString());
                    }
                    else
                    {
                        bool isOk = false;
                        foreach (string key in otherParams.Keys)
                        {
                            if (key.ToLower().Equals(tmpitem) || key.ToLower().Equals("@" + tmpitem))
                            {
                                realParam.Add(tmpitem, otherParams[key].ToString());
                                isOk = true;
                                break;
                            }
                        }
                        if (!isOk) throw (new Exception("参数不足"));
                    }
                }
            }

            string strSQL = "";
            DbParameter[] param = null;
            if (paramNames != null || paramNames.Length > 0)
            {
                strSQL = string.Format("select * from {0}({1})", functionname, paramNames);
                param = new DbParameter[realParam.Keys.Count];
                int i = 0;
                foreach (string key in realParam.Keys)
                {
                    param[i] = new NpgsqlParameter(key, realParam[key]);
                    i++;
                }

            }
            else
            {
                strSQL = string.Format("select  * from  {0}()", functionname);
                param = new DbParameter[] { };
            }
            if (funcInfo.ReturnType == EntityExtFunctionReturnType.NoReturn)
            {
                ExecuteNonQuery(strSQL, param, tran);
                return null;
            }
            else if (funcInfo.ReturnType == EntityExtFunctionReturnType.SingleQuery)
            {
                return ExecuteQuery(strSQL, param, tran);
            }
            else if (funcInfo.ReturnType == EntityExtFunctionReturnType.MultiCursor)
            {
                return ExecuteQueryRefCursor(strSQL, param, tran);
            }
            else
            {
                throw (new Exception("函数定义异常"));
            }
        }

        /// <summary>
        /// 根据用户id和实体ID获取改用的该实体的自定义WEB列
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="userId"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetPersonalWebListColumnsSetting(Guid entityId, int userId, DbTransaction tran)
        {
            try
            {
                string strSQL = "Select * from crm_sys_entity_personweblistconfig where userid=@userid and entityid=@entityid";
                DbParameter[] param = new Npgsql.NpgsqlParameter[] {
                    new Npgsql.NpgsqlParameter("@userid",userId),
                    new Npgsql.NpgsqlParameter("@entityid",entityId)
                };
                return this.ExecuteQuery(strSQL, param, tran).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object>();
            }
        }
        /// <summary>
        /// 更新某人的某实体的个性化web列表定义
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="viewConfig"></param>
        /// <param name="userId"></param>
        /// <param name="tran"></param>
        public void UpdatePersonalWebListColumnsSetting(Guid recid, WebListPersonalViewSettingInfo viewConfig, int userId, DbTransaction tran)
        {
            try
            {
                string strSQL = "update crm_sys_entity_personweblistconfig set viewconfig=@viewconfig where recid=@recid";
                DbParameter[] param = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@recid",recid),
                    new Npgsql.NpgsqlParameter("@viewconfig",Newtonsoft.Json.JsonConvert.SerializeObject(viewConfig)){ NpgsqlDbType= NpgsqlTypes.NpgsqlDbType.Jsonb}
                };
                this.ExecuteNonQuery(strSQL, param, tran);
            }
            catch (Exception ex) {
            }
        }

        public void AddPersonalWebListColumnsSetting(Guid entityId, WebListPersonalViewSettingInfo viewConfig, int userId, DbTransaction tran)
        {
            try
            {
                string strSQL = "insert into crm_sys_entity_personweblistconfig(entityid,userid,viewconfig) values(@entityid,@userid,@viewconfig)";
                DbParameter[] param = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@entityid",entityId),
                    new Npgsql.NpgsqlParameter("@userid",userId),
                    new Npgsql.NpgsqlParameter("@viewconfig",Newtonsoft.Json.JsonConvert.SerializeObject(viewConfig)){ NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb}
                };
                this.ExecuteNonQuery(strSQL, param, tran);
            }
            catch (Exception ex) {
            }
        }
        /// <summary>
        /// 查重
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="uesrNumber">用户id</param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public Dictionary<string, object> QueryEntityCondition(DynamicEntityCondition entity, DbTransaction tran = null)
        {
            Dictionary<string, object> entityConditionData = new Dictionary<string, object>();
            #region sql
            string sql = @"SELECT
	                        con.entityid,
	                        con.functype,
	                        fie.fieldid,
	                        fie.displayname,
	                        fie.fieldname
                        FROM
	                        crm_sys_entity_condition AS con
                        RIGHT OUTER  JOIN crm_sys_entity_fields AS fie ON con.fieldid = fie.fieldid
                        WHERE fie.entityid = @entityid
                        ORDER BY fie.recorder ";
            #endregion
            var data = ExecuteQuery(sql, new DbParameter[] { new Npgsql.NpgsqlParameter("@entityid",entity.EntityId)}, tran);
            var checkData = data.Where(r => r["entityid"] != null && (Guid)r["entityid"] == entity.EntityId).ToList();
            var notCheckData = data.Where(r => r["entityid"] == null).ToList();
            entityConditionData.Add("fieldvisible", checkData);
            entityConditionData.Add("fieldnotvisible", notCheckData);
            return entityConditionData;
        }
        /// <summary>
        /// 修改查重
        /// </summary>
        /// <param name="entityList">插入对象List</param>
        /// <param name="userNumber"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool UpdateEntityCondition(List<DynamicEntityCondition> entityList, int userNumber, DbTransaction trans)
        {
            try
            {
                #region sql
                string delSQL = @"Delete from crm_sys_entity_condition where entityid = @entityid";
                string insertSql = string.Format(@"INSERT INTO crm_sys_entity_condition(entityid,fieldid,functype)
                                   SELECT entityid,fieldid,functype
                                   FROM json_populate_recordset(null::crm_sys_entity_condition,@condition)");
                #endregion
                #region 删除
                var param = new DbParameter[]
                {
                    new NpgsqlParameter("@entityid",entityList[0].EntityId)
                };
                this.ExecuteNonQuery(delSQL, param, trans);
                #endregion

                #region 插入
                DbParameter[] rulesparams = new DbParameter[] { new NpgsqlParameter("condition", JsonConvert.SerializeObject(entityList)) { NpgsqlDbType = NpgsqlDbType.Json } };
                int count = ExecuteNonQuery(insertSql, rulesparams, trans);
                #endregion
                if (count > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
