using Dapper;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Dynamics;
using UBeat.Crm.CoreApi.DomainModel.Message;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.Dynamics
{
    public class DynamicRepository : RepositoryBase, IDynamicRepository
    {


        #region --动态模板--
        //public OperateResult SaveDynamicTemplate(DynamicTemplateInsert data)
        //{
        //    var executeSql = "select * from crm_func_dynamictemplate_save(@entityid,@tempcontent,@userno)";
        //    var args = new
        //    {
        //        entityid = data.EntityID,
        //        tempcontent = data.Template,
        //        userno = data.UserNo
        //    };
        //    return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        //}

        //public dynamic SelectDynamicTemplate(DynamicTemplateSelect data)
        //{
        //    return null;
        //}

        //public OperateResult DeleteDynamicTemplate(Guid entityid, int usrno)
        //{
        //    return null;
        //} 
        public string GetDynamicTemplate(Guid entityid, Guid typeid)
        {
            var template_sql = @"SELECT tempcontent  FROM crm_sys_dynamic_template where recstatus=1 AND  entityid=@entityid AND typeid=@typeid ORDER BY reccreated DESC LIMIT 1;";
            var template_param = new DbParameter[]
             {
                        new NpgsqlParameter("entityid", entityid),
                        new NpgsqlParameter("typeid", typeid)
             };
            var templateResult = ExecuteQuery(template_sql, template_param).FirstOrDefault();
           
            if (templateResult == null)
            {
                throw new Exception("没有配置该实体的动态模板");
            }
            var temp = string.Empty;
            if (templateResult["tempcontent"] != null)
                temp = templateResult["tempcontent"].ToString();
            return temp;
        }
        /// <summary>
        /// 获取动态模板生成的数据
        /// </summary>
        /// <param name="recid"></param>
        /// <param name="entityID"></param>
        /// <param name="typeID"></param>
        /// <param name="userno"></param>
        /// <returns></returns>
        public string GetDynamicTemplateData(Guid recid, Guid entityID, Guid typeID, int userno)
        {
            string result = null;

            using (DbConnection conn = DBHelper.GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();

                try
                {
                    var executeSql = @"SELECT columnnames,columnsql FROM crm_func_dynamic_abstract_columnnames_select_extra(@typeid,@entityid,@userno);
                                       SELECT entitytable  FROM crm_sys_entity WHERE entityid=@entityid LIMIT 1;";
                    var sqlParameters = new List<DbParameter>();
                    sqlParameters.Add(new NpgsqlParameter("userno", userno));
                    sqlParameters.Add(new NpgsqlParameter("typeid", typeID));
                    sqlParameters.Add(new NpgsqlParameter("entityid", entityID));
                    var executeSqlResult = ExecuteQueryMultiple(executeSql, sqlParameters.ToArray());
                    var columnResult = executeSqlResult.FirstOrDefault().FirstOrDefault();
                    var columnnames = columnResult["columnnames"] == null ? null : columnResult["columnnames"].ToString();
                    var columnsql = columnResult["columnsql"] == null ? null : columnResult["columnsql"].ToString();
                    if (!string.IsNullOrEmpty(columnnames))
                    {
                        var tableNameReuslt = executeSqlResult[1].FirstOrDefault();
                        if (tableNameReuslt.Count > 0)
                        {
                            var tableName = tableNameReuslt["entitytable"];
                            var sql = string.Format(@"SELECT row_to_json(t)::TEXT FROM (  SELECT {0} FROM (SELECT * {1} FROM {2} AS e WHERE e.recid = @recid LIMIT 1) AS k) AS t", columnnames, columnsql, tableName);
                            var param = new DbParameter[]
                            {
                               new NpgsqlParameter("recid", recid)
                            };
                            var sqlResult = ExecuteScalar(sql, param, tran);
                            result = sqlResult == null ? null : sqlResult.ToString();
                        }
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }

            }

            return result;

        }

        #endregion

        #region --动态摘要--
        public OperateResult InsertDynamicAbstract(DynamicAbstractInsert data)
        {
            var executeSql = "select * from crm_func_dynamic_abstract_save(@typeid,@entityid,@fieldids,@userno)";
            var args = new
            {
                typeid = data.EntityID,
                entityid = data.EntityID,
                fieldids = data.Fieldids.ToArray(),
                userno = data.UserNo
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }

        public dynamic SelectDynamicAbstract(DynamicAbstractSelect data)
        {
            var executeSql = "select * from crm_func_dynamic_abstract_select(@typeid,@entityid,@userno)";

            var args = new DynamicParameters();
            args.Add("typeid", data.TypeID);
            args.Add("entityid", data.EntityID);
            args.Add("userno", data.UserNo);

            return DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);

        }

        public List<DynamicAbstractInfo> GetDynamicAbstract(Guid entityID, Guid typeID)
        {
            var executeSql = @"SELECT a.recid AS id,a.typeid,a.entityid,a.fieldid,f.fieldname,f.controltype FROM
                                crm_sys_dynamic_abstract AS a
                                INNER JOIN crm_sys_entity_fields AS f ON f.fieldid = a.fieldid
                                WHERE a.entityid = @entityid  AND a.typeid = @typeid AND a.fieldid IS NOT NULL AND a.recstatus = 1 AND f.recstatus = 1
                                ";


            var sqlParameters = new List<DbParameter>();

            sqlParameters.Add(new NpgsqlParameter("typeid", typeID));
            sqlParameters.Add(new NpgsqlParameter("entityid", entityID));

            return ExecuteQuery<DynamicAbstractInfo>(executeSql, sqlParameters.ToArray());

        }

        


        #endregion

        

        #region --动态详情--

        public bool InsertDynamic(DbTransaction tran, DynamicInsertInfo data, int userNumber, out MsgParamInfo tempData)
        {
            Guid templateId = Guid.Empty;
            Guid dynamicid = Guid.NewGuid();
            tempData = null;
            if (data.DynamicType == DynamicType.Entity)
            {
                if (data.EntityId == Guid.Empty)
                    throw new Exception("实体ID不可为空");
                if (string.IsNullOrEmpty(data.TemplateData))
                    throw new Exception("动态数据不可为空");

                var template_sql = @"SELECT templateid,tempcontent  FROM crm_sys_dynamic_template where recstatus=1 AND  entityid=@entityid AND typeid=@typeid ORDER BY reccreated DESC LIMIT 1;";
                var template_param = new DbParameter[]
                 {
                        new NpgsqlParameter("entityid", data.EntityId),
                        new NpgsqlParameter("typeid", data.TypeId)
                 };
                var templateResult = ExecuteQuery(template_sql, template_param, tran).FirstOrDefault();
                if (templateResult == null || !Guid.TryParse(templateResult["templateid"].ToString(), out templateId))
                {
                    throw new Exception("没有配置该实体的动态模板");
                }
                tempData = new MsgParamInfo();
                if (templateResult["tempcontent"] != null)
                    tempData.Template = JsonConvert.DeserializeObject(templateResult["tempcontent"].ToString());
                tempData.Data = JsonConvert.DeserializeObject(string.IsNullOrEmpty(data.TemplateData) ? "{}" : data.TemplateData);
            }

            else if (string.IsNullOrEmpty(data.Content))
            {
                throw new Exception("content不可为空");
            }


            var executeSql = @"INSERT INTO crm_sys_dynamics (entityid,typeid,businessid,relentityid,relbusinessid,dynamictype,templateid,tempdata,content,recstatus,reccreator,recupdator) 
                               VALUES(@entityid,@typeid, @businessid,@relentityid,@relbusinessid,@dynamictype, @templateid, @tempdata, @content,  1, @userno, @userno) ; ";

            var param = new DbParameter[]
             {
                    new NpgsqlParameter("entityid", data.EntityId),
                    new NpgsqlParameter("typeid", data.TypeId),
                    new NpgsqlParameter("businessid", data.BusinessId),
                    new NpgsqlParameter("relentityid", data.RelEntityId.GetValueOrDefault()),
                    new NpgsqlParameter("relbusinessid", data.RelBusinessId),
                    new NpgsqlParameter("templateid",templateId),
                    new NpgsqlParameter("tempdata",string.IsNullOrEmpty( data.TemplateData)?"{}": data.TemplateData){  NpgsqlDbType= NpgsqlDbType.Jsonb},
                    new NpgsqlParameter("dynamictype", (int)data.DynamicType),
                    new NpgsqlParameter("content", data.Content??""),
                    new NpgsqlParameter("userno", userNumber)
             };
            return ExecuteNonQuery(executeSql, param, tran) > 0;
        }


        public OperateResult InsertDynamic(DynamicInsert data)
        {
            //"_dynamictype" int4, "_entityid" uuid, "_businessid" uuid, "_typeid" uuid, "_typerecid" uuid, "_jsonstring" text, "_content" text, "_userno" int4)

            var executeSql = "select * from crm_func_dynamic_detail_insert(@dynamictype,@entityid,@businessid,@typeid,@typerecid,@jsondata,@content,@userno)";

            var args = new DynamicParameters();
            args.Add("dynamictype", data.DynamicType);
            args.Add("entityid", data.EntityId);
            args.Add("businessid", data.BusinessId);
            args.Add("typeid", data.TypeId);
            args.Add("typerecid", data.TypeRecId);
            args.Add("jsondata", data.JsonData);
            args.Add("content", data.Content);
            args.Add("userno", data.UserNo);
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }
        public OperateResult DeleteDynamic(DynamicDelete data)
        {
            var executeSql = "select * from crm_func_dynamic_detail_delete(@dynamicid,@userno)";
            var args = new
            {
                dynamicid = data.DynamicId,
                userno = data.UserNo
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }

        /// <summary>
        /// 获取某个动态的信息
        /// </summary>
        /// <param name="dynamicid"></param>
        /// <returns></returns>
        public DynamicInfo GetDynamicInfo(Guid dynamicid)
        {
            //var sql = @"SELECT d.*,t.tempcontent::jsonb FROM crm_sys_dynamics AS d
            //            LEFT JOIN crm_sys_dynamic_template AS t ON  t.templateid=d.templateid 
            //            WHERE d.dynamicid=@dynamicid";

            var sql = string.Format(@"SELECT d.*,t.tempcontent::Jsonb ,e.entityname,ec.categoryname AS TypeName, re.entityname AS relentityname,u.usericon AS reccreatorUserIcon,u.username AS reccreatorname,
				                                    array(
					                                    SELECT u.username  FROM crm_sys_dynamic_praise AS p 
					                                    LEFT JOIN crm_sys_userinfo AS u ON u.userid=p.reccreator
					                                    WHERE p.dynamicid=d.dynamicid AND p.recstatus=1 ORDER BY p.reccreated
				                                    ) AS PraiseUsers,
				                                    (SELECT array_to_json(array_agg(row_to_json(t))) FROM
						                                    (SELECT c.dynamicid, c.commentsid,c.pcommentsid,c.comments,c.reccreator,u.username AS reccreator_name,u.usericon AS reccreator_icon,c.reccreated,uc.username AS tocommentor,dc.comments AS tocomments FROM crm_sys_dynamic_comments AS c 
							                                    LEFT JOIN crm_sys_userinfo AS u ON u.userid=c.reccreator
							                                    LEFT JOIN crm_sys_dynamic_comments AS dc ON dc.commentsid=c.pcommentsid
							                                    LEFT JOIN crm_sys_userinfo AS uc ON uc.userid=dc.reccreator
							                                    WHERE c.dynamicid=d.dynamicid AND c.recstatus=1 ORDER BY c.reccreated) AS t
				                                     ) AS Comments
                                                FROM public.crm_sys_dynamics AS d 
                                                LEFT JOIN crm_sys_dynamic_template AS t ON t.templateid=d.templateid
                                                LEFT JOIN crm_sys_entity AS re ON re.entityid=d.relentityid
                                                LEFT JOIN crm_sys_entity AS e ON e.entityid=d.entityid 
                                                LEFT JOIN crm_sys_entity_category AS ec ON ec.categoryid=d.typeid
                                                LEFT JOIN crm_sys_userinfo AS u ON u.userid=d.reccreator 
                                            WHERE d.dynamicid=@dynamicid");

            var param = new DbParameter[]
            {
                 new NpgsqlParameter("dynamicid", dynamicid)
            };
            return ExecuteQuery<DynamicInfoExt>(sql, param).FirstOrDefault();
        }

        #region --增量获取动态列表--
        /// <summary>
        /// 增量获取动态列表
        /// </summary>
        /// <param name="param"></param>
        /// <param name="incrementPage"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public IncrementPageDataInfo<DynamicInfoExt> GetDynamicInfoList(DynamicListParameter param, IncrementPageParameter incrementPage, int userNumber)
        {
            List<DbParameter> dbParams = new List<DbParameter>();

            var versionSql = string.Empty;
            var limitSql = string.Empty;
            var orderby = "DESC";
            var entityIdSql = string.Empty;
            var businessIdSql = string.Empty;
            var dynamictypeSql = string.Empty;

            //处理增量参数
            if (incrementPage == null)
                throw new Exception("增量参数不可为空");
            else
            {
                //判断增量方向,如果版本号为小于等于0的数据，则获取最新N条数据
                if (incrementPage.Direction != IncrementDirection.None&& incrementPage.RecVersion>0)
                {
                    versionSql = string.Format(@" AND d.recversion{0}@recversion", incrementPage.Direction == IncrementDirection.Forward ? "<" : ">");
                    orderby = incrementPage.Direction == IncrementDirection.Forward ? "DESC" : "ASC";
                    dbParams.Add(new NpgsqlParameter("recversion", incrementPage.RecVersion));
                }
                //设置增量页大小
                if (incrementPage.PageSize > 0)
                {
                    limitSql = @"LIMIT @limitcount";
                    dbParams.Add(new NpgsqlParameter("limitcount", incrementPage.PageSize));
                }

            }
            //处理业务条件
            if (param.EntityId != Guid.Empty)
            {
                entityIdSql = " AND (d.entityid=@entityid OR d.relentityid=@entityid)";
                dbParams.Add(new NpgsqlParameter("entityid", param.EntityId));
            }
            if (param.Businessid != Guid.Empty)
            {
                businessIdSql = " AND (d.businessid=@businessid OR d.relbusinessid=@businessid)";
                dbParams.Add(new NpgsqlParameter("businessid", param.Businessid));
            }

            if (param.DynamicTypes != null && param.DynamicTypes.Count > 0)
            {
                dynamictypeSql = " AND d.dynamictype = ANY(@dynamictypes)";
                dbParams.Add(new NpgsqlParameter("dynamictypes", param.DynamicTypes.Cast<int>().ToArray()));
            }

            var executeSql = string.Format(@"SELECT d.*,t.tempcontent::Jsonb ,e.entityname,ec.categoryname AS TypeName, re.entityname AS relentityname,u.usericon AS reccreatorUserIcon,u.username AS reccreatorname,
				                                    array(
					                                    SELECT u.username  FROM crm_sys_dynamic_praise AS p 
					                                    LEFT JOIN crm_sys_userinfo AS u ON u.userid=p.reccreator
					                                    WHERE p.dynamicid=d.dynamicid AND p.recstatus=1 ORDER BY p.reccreated
				                                    ) AS PraiseUsers,
				                                    (SELECT array_to_json(array_agg(row_to_json(t))) FROM
						                                    (SELECT c.dynamicid, c.commentsid,c.pcommentsid,c.comments,c.reccreator,u.username AS reccreator_name,u.usericon AS reccreator_icon,c.reccreated,uc.username AS tocommentor,dc.comments AS tocomments FROM crm_sys_dynamic_comments AS c 
							                                    LEFT JOIN crm_sys_userinfo AS u ON u.userid=c.reccreator
							                                    LEFT JOIN crm_sys_dynamic_comments AS dc ON dc.commentsid=c.pcommentsid
							                                    LEFT JOIN crm_sys_userinfo AS uc ON uc.userid=dc.reccreator
							                                    WHERE c.dynamicid=d.dynamicid AND c.recstatus=1 ORDER BY c.reccreated) AS t
				                                     ) AS Comments
                                                FROM public.crm_sys_dynamics AS d 
                                                LEFT JOIN crm_sys_dynamic_template AS t ON t.templateid=d.templateid
                                                LEFT JOIN crm_sys_entity AS re ON re.entityid=d.relentityid
                                                LEFT JOIN crm_sys_entity AS e ON e.entityid=d.entityid 
                                                LEFT JOIN crm_sys_entity_category AS ec ON ec.categoryid=d.typeid
                                                LEFT JOIN crm_sys_userinfo AS u ON u.userid=d.reccreator 
                                            WHERE 1=1 {0} {1}  {2} {3}
                                            ORDER BY d.recversion {4}
                                            {5};", versionSql, entityIdSql, businessIdSql, dynamictypeSql, orderby, limitSql);

            var result = new IncrementPageDataInfo<DynamicInfoExt>();
            result.DataList = ExecuteQuery<DynamicInfoExt>(executeSql, dbParams.ToArray());
            if (result.DataList.Count > 0)
            {
                result.DataList = result.DataList.OrderByDescending(m => m.RecCreated).ToList();
                var firstRowVersion = result.DataList.FirstOrDefault().RecVersion;
                var lastRowVersion = result.DataList.LastOrDefault().RecVersion;
                result.PageMaxVersion = Math.Max(firstRowVersion, lastRowVersion);
                result.PageMinVersion = Math.Min(firstRowVersion, lastRowVersion);
            }
            return result;
        }
        #endregion

        #region --分页获取动态列表--
        /// <summary>
        /// 分页获取动态列表
        /// </summary>
        /// <param name="param"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public PageDataInfo<DynamicInfoExt> GetDynamicInfoList(DynamicListParameter param, int pageIndex, int pageSize, int userNumber)
        {
            List<DbParameter> dbParams = new List<DbParameter>();

            var entityIdSql = string.Empty;
            var businessIdSql = string.Empty;
            var dynamictypeSql = string.Empty;

            //处理业务条件
            if (param.EntityId != Guid.Empty)
            {
                entityIdSql = " AND (d.entityid=@entityid OR d.relentityid=@entityid)";
                dbParams.Add(new NpgsqlParameter("entityid", param.EntityId));
            }
            if (param.Businessid != Guid.Empty)
            {
                businessIdSql = " AND (d.businessid=@businessid OR d.relbusinessid=@businessid)";
                dbParams.Add(new NpgsqlParameter("businessid", param.Businessid));
            }

            if (param.DynamicTypes != null && param.DynamicTypes.Count > 0)
            {
                dynamictypeSql = " AND d.dynamictype = ANY(@dynamictypes)";
                dbParams.Add(new NpgsqlParameter("dynamictypes", param.DynamicTypes.Cast<int>().ToArray()));
            }

            var executeSql = string.Format(@"SELECT d.*,t.tempcontent::Jsonb ,e.entityname,ec.categoryname AS TypeName, re.entityname AS relentityname,u.usericon AS reccreatorUserIcon,u.username AS reccreatorname,
				                                    array(
					                                    SELECT u.username  FROM crm_sys_dynamic_praise AS p 
					                                    LEFT JOIN crm_sys_userinfo AS u ON u.userid=p.reccreator
					                                    WHERE p.dynamicid=d.dynamicid AND p.recstatus=1 ORDER BY p.reccreated
				                                    ) AS PraiseUsers,
				                                    (SELECT array_to_json(array_agg(row_to_json(t))) FROM
						                                    (SELECT c.dynamicid, c.commentsid,c.pcommentsid,c.comments,c.reccreator,u.username AS reccreator_name,u.usericon AS reccreator_icon,c.reccreated,uc.username AS tocommentor,dc.comments AS tocomments FROM crm_sys_dynamic_comments AS c 
							                                    LEFT JOIN crm_sys_userinfo AS u ON u.userid=c.reccreator
							                                    LEFT JOIN crm_sys_dynamic_comments AS dc ON dc.commentsid=c.pcommentsid
							                                    LEFT JOIN crm_sys_userinfo AS uc ON uc.userid=dc.reccreator
							                                    WHERE c.dynamicid=d.dynamicid AND c.recstatus=1 ORDER BY c.reccreated) AS t
				                                     ) AS Comments
                                                FROM public.crm_sys_dynamics AS d 
                                                LEFT JOIN crm_sys_dynamic_template AS t ON t.templateid=d.templateid
                                                LEFT JOIN crm_sys_entity AS re ON re.entityid=d.relentityid
                                                LEFT JOIN crm_sys_entity AS e ON e.entityid=d.entityid 
                                                LEFT JOIN crm_sys_entity_category AS ec ON ec.categoryid=d.typeid
                                                LEFT JOIN crm_sys_userinfo AS u ON u.userid=d.reccreator 
                                                WHERE d.recstatus=1 {0} {1} {2} 
                                                ORDER BY d.recversion DESC ", entityIdSql, businessIdSql, dynamictypeSql);

            dbParams.Add(new NpgsqlParameter("userid", userNumber));
            var result= ExecuteQueryByPaging<DynamicInfoExt>(executeSql, dbParams.ToArray(), pageSize, pageIndex);
            result.DataList = result.DataList.OrderByDescending(m => m.RecCreated).ToList();
            return result;
        } 
        #endregion


        public DynamicInfoModel SelectDynamic(Guid dynamicid, int usernumber)
        {
            var executeSql = "select * from  crm_func_dynamic_detail_select(@dynamicid,@userno)";

            var args = new DynamicParameters();
            args.Add("dynamicid", dynamicid);
            args.Add("userno", usernumber);

            var dataResult = DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
            return GetDynamicInfoList(dataResult).FirstOrDefault();
        }

        public dynamic SelectDynamic(PageParam pageParam, DynamicSelect data)
        {
            var executeSql = "select * from  crm_func_dynamic_detail_select(@businessid,@entityid,@dynamictypes,@userno,@pageindex,@pagesize,@recversion,@recstatus)";
            var dataNames = new List<string> { "PageData", "PageCount" };
            var args = new
            {
                businessid = data.Businessid,
                entityid = data.EntityId,
                dynamictypes = data.DynamicTypes.ToArray(),
                userno = data.UserNo,
                pageindex = pageParam.PageIndex,
                pagesize = pageParam.PageSize,
                recversion = data.RecVersion,
                recstatus = data.RecStatus
            };
            var dataResult = DataBaseHelper.QueryStoredProcCursor(executeSql, dataNames, args, CommandType.Text);
            //if (data.RecVersion > 0 || data.PraiseRecVersion > 0 || data.CommentRecVersion > 0)
            //{
            //    return dataResult;
            //}
            // return dataResult;
            return new { PageData = GetDynamicInfoList(dataResult["PageData"]), PageCount = dataResult["PageCount"] };
        }
        private List<DynamicInfoModel> GetDynamicInfoList(List<IDictionary<string, object>> datalist)
        {
            List<DynamicInfoModel> crmDynamics = new List<DynamicInfoModel>();
            if (datalist != null)
            {
                foreach (var m in datalist)
                {

                    int dynamictype = 0;
                    int.TryParse(m["dynamictype"].ToString(), out dynamictype);
                    int reccreator = 0;
                    int.TryParse(m["reccreator"].ToString(), out reccreator);
                    int recstatus = int.MinValue;
                    int.TryParse(m["recstatus"].ToString(), out recstatus);
                    long recversion = 0;
                    long.TryParse(m["recversion"].ToString(), out recversion);

                    DateTime reccreated = DateTime.MinValue;
                    DateTime.TryParse(m["reccreated"].ToString(), out reccreated);

                    List<DynamicComment> commentlist = null;
                    if (m["commentlist"] != null)
                    {
                        commentlist = JsonHelper.ToObject<List<DynamicComment>>(m["commentlist"].ToString());
                    }
                    Array praises = null;
                    if (m["praiselist"] != null)
                    {
                        praises = m["praiselist"] as Array;
                    }
                    //var praiselist = praises == null ? null : praises.Select(o => o.RecCreatorName).ToArray();
                    var commentlisttmp = commentlist;
                    //var commentlisttmp = GetCommentlist(commentlist, Guid.Empty.ToString());

                    crmDynamics.Add(new DynamicInfoModel()
                    {
                        DynamicId = m["dynamicid"],
                        EntityId = m["entityid"],
                        TypeId = m["typeid"],
                        TypeEntityId = m["typeentityid"],
                        TypeEntityName = m["typeentityname"],
                        TypeRecId = m["typerecid"],
                        EntityName = m["entityname"],
                        BusinessId = m["businessid"],
                        TempData = m["tempdata"] == null ? null : m["tempdata"].ToString().ToJsonDictionary(),
                        TempContent = m["tempcontent"] == null ? null : m["tempcontent"].ToString().ToJsonArray(),
                        DynamicType = dynamictype,
                        Content = m["content"],
                        RecStatus = recstatus,
                        RecCreator = reccreator,
                        RecCreatorName = m["reccreator_name"],
                        UserIcon = m["usericon"],
                        RecCreateTime = reccreated,
                        RecVersion = recversion,
                        PraiseUsers = praises ?? new string[0],
                        //Praises = praises,
                        Comments = commentlisttmp != null ? commentlisttmp.ToList() : new List<DynamicComment>()
                    });
                }
            }
            return crmDynamics;
        }

        private List<DynamicComment> GetCommentlist(List<DynamicComment> list, string pcommentsId = null)
        {
            if (list == null)
                return null;
            var temps = list.Where(o => o.PcommentsId.Equals(pcommentsId)).ToList();
            foreach (var m in temps)
            {
                m.Reply = GetCommentlist(list, m.CommentsId);
            }
            return temps;
        }

        #endregion


        #region --动态评论--

        public Guid AddDynamicComments(DynamicCommentParameter data, int userNumber)
        {
            Guid commentsid = Guid.NewGuid();
            var executeSql = @"INSERT INTO crm_sys_dynamic_comments (commentsid,pcommentsid,dynamicid,comments,reccreator,recupdator,recstatus) 
		                        VALUES( @commentsid,@pcommentsid ,@dynamicid,@comments ,@userno,@userno,1 ); 
                               ";

            var param = new DbParameter[]
             {
                    new NpgsqlParameter("commentsid", commentsid),
                    new NpgsqlParameter("pcommentsid", data.PcommentsId),
                    new NpgsqlParameter("dynamicid", data.DynamicId),
                    new NpgsqlParameter("comments", data.Comments),

                    new NpgsqlParameter("userno", userNumber)
             };
            if (!(ExecuteNonQuery(executeSql, param) > 0))
            {
                commentsid = Guid.Empty;
            }

            return commentsid;
        }

        public OperateResult AddDynamicComments(DynamicCommentsInsert data)
        {
            var executeSql = "select * from crm_func_dynamic_comments_insert(@dynamicid,@pcommentsid,@comments,@userno)";
            var args = new
            {
                dynamicid = data.DynamicId,
                pcommentsid = data.PCommentsid,
                comments = data.Comments,
                userno = data.UserNo
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }

        public OperateResult DeleteDynamicComments(Guid commentsid, int usrno)
        {
            var executeSql = "select * from crm_func_dynamic_comments_delete(@commentsid,@userno)";
            var args = new
            {
                commentsid = commentsid,
                userno = usrno
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }
        #endregion


        #region --动态点赞--
        public bool AddDynamicPraise(Guid dynamicid, int userno)
        {
            bool res = false;
            using (DbConnection conn = DBHelper.GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();

                try
                {
                    var existPraiseSql = "SELECT 1 FROM crm_sys_dynamic_praise WHERE dynamicid = @dynamicid AND reccreator=@userno  AND recstatus = 1  LIMIT 1";
                    var existPraiseSqlParam = new DbParameter[]
                    {
                        new NpgsqlParameter("dynamicid", dynamicid),
                        new NpgsqlParameter("userno", userno)
                    };
                    var existPraiseSqlResult = ExecuteScalar(existPraiseSql, existPraiseSqlParam, tran);
                    //如果已点赞了，则不重复点赞
                    if (existPraiseSqlResult == null)
                    {
                        var executeSql = @"INSERT INTO crm_sys_dynamic_praise (dynamicid,reccreator,recupdator,recstatus)  VALUES( @dynamicid,@userno,@userno,1 ) ;
                                           ";
                        var param = new DbParameter[]
                        {
                                new NpgsqlParameter("dynamicid", dynamicid),
                                new NpgsqlParameter("userno", userno)
                        };
                        res = ExecuteNonQuery(executeSql, param) > 0;
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return res;
        }
        public OperateResult AddDynamicPraise(DynamicPraiseMapper data)
        {
            var executeSql = "select * from crm_func_dynamic_praise_insert(@dynamicid,@userno)";
            var args = new
            {
                dynamicid = data.DynamicId,
                userno = data.UserNo
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }


        public OperateResult DeleteDynamicPraise(DynamicPraiseMapper data)
        {
            var executeSql = "select * from crm_func_dynamic_praise_delete(@dynamicid,@userno)";
            var args = new
            {
                dynamicid = data.DynamicId,
                userno = data.UserNo
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);

        }
        #endregion


        public bool MergeDynamic(DbTransaction tran, Guid entityid, Guid businessid, List<Guid> beMergeBusinessids, int usernumber)
        {
            var existSqlParameters = new List<DbParameter>();
            existSqlParameters.Add(new NpgsqlParameter("entityid", entityid));
            existSqlParameters.Add(new NpgsqlParameter("beMergeBusinessids", beMergeBusinessids.ToArray()));
            if (DBHelper.GetCount(tran, "crm_sys_dynamics", " (entityid=@entityid AND businessid =ANY (@beMergeBusinessids)) OR (relentityid=@entityid AND relbusinessid =ANY (@beMergeBusinessids))", existSqlParameters.ToArray()) <= 0)
                return true;


            var sql = @"UPDATE crm_sys_dynamics  SET recupdator=@recupdator,recupdated=@recupdated,businessid=@businessid
                        WHERE entityid=@entityid AND businessid =ANY (@beMergeBusinessids);
                        UPDATE crm_sys_dynamics  SET recupdator=@recupdator,recupdated=@recupdated,relbusinessid=@businessid
                        WHERE relentityid=@entityid AND relbusinessid =ANY (@beMergeBusinessids);";

            var sqlParameters = new List<DbParameter>();

            sqlParameters.Add(new NpgsqlParameter("recupdator", usernumber));
            sqlParameters.Add(new NpgsqlParameter("recupdated", DateTime.Now));
            sqlParameters.Add(new NpgsqlParameter("businessid", businessid));
            sqlParameters.Add(new NpgsqlParameter("entityid", entityid));
            sqlParameters.Add(new NpgsqlParameter("beMergeBusinessids", beMergeBusinessids.ToArray()));

            var result = DBHelper.ExecuteNonQuery(tran, sql, sqlParameters.ToArray());
            return result > 0;
        }

    }
}
