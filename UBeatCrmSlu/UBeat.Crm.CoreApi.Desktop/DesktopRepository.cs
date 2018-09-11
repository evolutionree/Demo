using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using UBeat.Crm.CoreApi.Repository.Repository;
using UBeat.Crm.CoreApi.Repository.Utility;
using System.Data;
using UBeat.Crm.CoreApi.DomainModel;
using System.Linq;
using Npgsql;
using System.Data.Common;
using System.Globalization;

namespace UBeat.Crm.CoreApi.Desktop
{
    public class DesktopRepository : RepositoryBase, IDesktopRepository
    {
        public DesktopRepository(IConfigurationRoot config)
        {


        }

        #region config

        public OperateResult SaveDesktop(DesktopMapper mapper, IDbTransaction trans = null)
        {
            var sqlDel = @"delete from crm_sys_desktop where desktopid=@desktopid;";
            var sql = @"insert into crm_sys_desktop (desktopname,desktoptype,basedeskid,description) values (@desktopname,@desktoptype,@basedeskid,@description) returning desktopid";
            var param = new DynamicParameters();
            param.Add("desktopid", mapper.DesktopId);
            param.Add("desktopname", mapper.DesktopName);
            param.Add("desktoptype", mapper.DesktopType);
            param.Add("basedeskid", mapper.BaseDeskId);
            param.Add("description", mapper.Description);
            DataBaseHelper.ExecuteNonQuery(sqlDel, trans.Connection, trans, param);
            var result = DataBaseHelper.ExecuteScalar<Guid>(sql, trans.Connection, trans, param);
            List<DesktopRoleRelationMapper> relations = new List<DesktopRoleRelationMapper>();
            if (!String.IsNullOrEmpty(mapper.VocationsId))
            {
                var ids = mapper.VocationsId.Split(",");
                foreach (var tmp in ids)
                {
                    DesktopRoleRelationMapper relation = new DesktopRoleRelationMapper
                    {
                        DesktopId = result,
                        RoleId = Guid.Parse(tmp)
                    };
                    relations.Add(relation);
                }
                SaveDesktopRoleRelation(relations, trans);
            }
            if (result != Guid.Empty)
            {
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "保存成功"
                };
            }
            else
            {
                return new OperateResult
                {
                    Msg = "保存成功"
                };
            }
        }

        public DesktopMapper GetDesktopDetail(Guid desktopId)
        {
            var sql = @"select desktopid,desktopname,description from crm_sys_desktop where desktopId=@desktopId limit 1;";
            var param = new DynamicParameters();
            param.Add("desktopId", desktopId);
            var result = DataBaseHelper.QuerySingle<DesktopMapper>(sql, param);
            return result;
        }
        public IList<DesktopsMapper> GetDesktops(SearchDesktopMapper mapper, int userId)
        {
            var sql = @"select *,(SELECT array_to_string(ARRAY(SELECT unnest(array_agg(ro1.vocationname))),',') FROM crm_sys_desktop_role_relation relate
  INNER JOIN crm_sys_vocation ro1 ON ro1.vocationid=relate.roleid where desktopid=de1.desktopid ) as vocations_name,(SELECT array_to_string(ARRAY(SELECT unnest(array_agg(ro1.vocationid))),',') FROM crm_sys_desktop_role_relation relate
  INNER JOIN crm_sys_vocation ro1 ON ro1.vocationid=relate.roleid where desktopid=de1.desktopid ) as vocations_id from crm_sys_desktop  de1
 where de1.status=@status {0}";
            var param = new DynamicParameters();
            param.Add("status", mapper.Status);
            param.Add("desktopname", mapper.DesktopName);
            String condition = String.Empty;
            if (!string.IsNullOrEmpty(mapper.DesktopName))
            {
                condition = " and de1.desktopname ILIKE '%' || @desktopname || '%' ESCAPE '`'";
            }
            sql = string.Format(sql, condition);
            var result = DataBaseHelper.Query<DesktopsMapper>(sql, param);
            return result;
        }

        public OperateResult EnableDesktop(DesktopMapper mapper, IDbTransaction trans = null)
        {
            var sql = @"update crm_sys_desktop set status=@status where  desktopid=@desktopid;";
            var param = new DynamicParameters();
            param.Add("desktopid", mapper.DesktopId);
            param.Add("status", mapper.Status);
            var result = DataBaseHelper.ExecuteNonQuery(sql, trans.Connection, trans, param);
            if (result > 0)
            {
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "保存成功"
                };
            }
            else
            {
                return new OperateResult
                {
                    Msg = "保存成功"
                };
            }
        }

        public OperateResult AssignComsToDesktop(ComToDesktopMapper mapper, int userId)
        {
            var sql = @"update crm_sys_desktop leftitems=@leftitems,rightitems=@rightitems where desktopid=@desktopid";
            var param = new DynamicParameters();
            param.Add("desktopid", mapper.DesktopId);
            param.Add("leftitems", String.Join(",", mapper.LeftItems.Cast<String>().ToList().ToArray()));
            param.Add("rightitems", String.Join(",", mapper.RightItems.Cast<String>().ToList().ToArray()));
            var result = DataBaseHelper.ExecuteNonQuery(sql, param);
            if (result > 0)
            {
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "保存成功"
                };
            }
            else
            {
                return new OperateResult
                {
                    Msg = "保存成功"
                };
            }
        }

        public OperateResult AssignComsToDesktop(ActualDesktopRelateToComMapper mapper, int userId, IDbTransaction dbTrans = null)
        {
            var sqlDel = @"delete from crm_sys_desktop_com_relation where   desktopid=@desktopid;";
            var sql = @"insert into crm_sys_desktop_com_relation(comid,desktopid) values (@comid,@desktopid)";
            var param = new DynamicParameters();
            param.Add("desktopid", mapper.DesktopId);
            int result = 0;
            DataBaseHelper.ExecuteNonQuery(sqlDel, dbTrans.Connection, dbTrans, param);
            foreach (var tmp in mapper.ComItems)
            {
                var args = new DynamicParameters();
                args.Add("desktopid", mapper.DesktopId);
                args.Add("comid", tmp.DsComponetId);
                result = DataBaseHelper.ExecuteNonQuery(sql, dbTrans.Connection, dbTrans, args);
            }
            if (result > 0)
            {
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "保存成功"
                };
            }
            else
            {
                return new OperateResult
                {
                    Msg = "保存成功"
                };
            }
        }

        public ActualDesktopComMapper GetActualDesktopCom(Guid desktopId, int userId)
        {
            var sql = @"select * from crm_sys_desktop where desktopid=@desktopid and status=@status limit 1;";
            var param = new DynamicParameters();
            param.Add("desktopid", desktopId);
            param.Add("status", 1);
            var result = DataBaseHelper.QuerySingle<ActualDesktopComMapper>(sql, param);
            result.ComItems = DataBaseHelper.Query<ActualDesktopComponentMapper>("select * from crm_sys_desktop_component_actual where dscomponetid in (select comid from crm_sys_desktop_com_relation where desktopid=@desktopid);", param);
            return result;

        }


        public OperateResult SaveDesktopComponent(DesktopComponentMapper mapper, IDbTransaction trans = null)
        {
            var sqlDel = @"delete from crm_sys_desktop_component where dscomponetid=@dscomponetid;";
            var sql = @"insert into crm_sys_desktop_component (comname,comtype,comwidth, comheighttype,mincomheight,maxcomheight,comurl,comargs,comdesciption) values (@comname,@comtype,@comwidth, @comheighttype,@mincomheight,@maxcomheight,@comurl,@comargs,@comdesciption)";
            var param = new DynamicParameters();
            param.Add("dscomponetid", mapper.DsComponetId);
            param.Add("comname", mapper.ComName);
            param.Add("comtype", mapper.ComType);
            param.Add("comwidth", mapper.ComWidth);
            param.Add("comheighttype", mapper.ComHeightType);
            param.Add("mincomheight", mapper.MinComHeight);
            param.Add("maxcomheight", mapper.MaxComHeight);
            param.Add("comurl", mapper.ComUrl);
            param.Add("comargs", mapper.ComArgs);
            param.Add("comdesciption", mapper.ComDesciption);

            DataBaseHelper.ExecuteNonQuery(sqlDel, trans.Connection, trans, param);
            var result = DataBaseHelper.ExecuteNonQuery(sql, trans.Connection, trans, param);
            if (result > 0)
            {
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "保存成功"
                };
            }
            else
            {
                return new OperateResult
                {
                    Msg = "保存成功"
                };
            }
        }


        public OperateResult SaveActualDesktopComponent(ActualDesktopComponentMapper mapper, IDbTransaction trans = null)
        {
            var sqlDel = @"delete from crm_sys_desktop_component_actual where dscomponetid=@dscomponetid;";
            var sql = @"insert into crm_sys_desktop_component_actual (dscomponetid,comname,comtype,comwidth, comheighttype,mincomheight,maxcomheight,comurl,comargs,comdesciption,postion) values (@dscomponetid,@comname,@comtype,@comwidth, @comheighttype,@mincomheight,@maxcomheight,@comurl,@comargs,@comdesciption,@postion::jsonb) returning dscomponetid";
            var param = new DynamicParameters();
            param.Add("dscomponetid", mapper.DsComponetId);
            param.Add("comname", mapper.ComName);
            param.Add("comtype", mapper.ComType);
            param.Add("comwidth", mapper.ComWidth);
            param.Add("comheighttype", mapper.ComHeightType);
            param.Add("mincomheight", mapper.MinComHeight);
            param.Add("maxcomheight", mapper.MaxComHeight);
            param.Add("comurl", mapper.ComUrl);
            param.Add("comargs", mapper.ComArgs);
            param.Add("comdesciption", mapper.ComDesciption);
            param.Add("postion", mapper.Postion);
            DataBaseHelper.ExecuteNonQuery(sqlDel, trans.Connection, trans, param);
            var result = DataBaseHelper.ExecuteNonQuery(sql, trans.Connection, trans, param);
            if (result > 0)
            {
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "保存成功",
                    Id = result.ToString()
                };
            }
            else
            {
                return new OperateResult
                {
                    Msg = "保存成功"
                };
            }
        }
        public OperateResult EnableDesktopComponent(DesktopComponentMapper mapper, IDbTransaction trans = null)
        {
            var sql = @"update crm_sys_desktop_component set status=@status where  dscomponetid=@dscomponetid;";
            var param = new DynamicParameters();
            param.Add("dscomponetid", mapper.DsComponetId);
            param.Add("status", mapper.Status);
            var result = DataBaseHelper.ExecuteNonQuery(sql, trans.Connection, trans, param);
            if (result > 0)
            {
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "保存成功"
                };
            }
            else
            {
                return new OperateResult
                {
                    Msg = "保存成功"
                };
            }
        }

        public DesktopComponentMapper GetDesktopComponentDetail(Guid dsComponetId)
        {
            var sql = @"select * from crm_sys_desktop_component where dscomponetid=@dscomponetid limit 1;";

            var param = new DynamicParameters();
            param.Add("dscomponetid", dsComponetId);
            var result = DataBaseHelper.QuerySingle<DesktopComponentMapper>(sql, param);
            return result;
        }
        public IList<DesktopComponentMapper> GetDesktopComponents(SearchDesktopComponentMapper mapper, int userId)
        {
            var sql = @"select * from crm_sys_desktop_component where status=@status {0};";

            var param = new DynamicParameters();
            param.Add("status", mapper.Status);
            param.Add("comname", mapper.ComName);
            String condition = String.Empty;
            if (!string.IsNullOrEmpty(mapper.ComName))
            {
                condition = " and comname ILIKE '%' || @comname || '%' ESCAPE '`'";
            }
            sql = string.Format(sql, condition);
            var result = DataBaseHelper.Query<DesktopComponentMapper>(sql, param);
            return result;
        }


        public OperateResult SaveDesktopRoleRelation(List<DesktopRoleRelationMapper> mapper, IDbTransaction trans = null)
        {
            var sqlDel = @"delete from crm_sys_desktop_role_relation where desktopid=@desktopid;";
            var args = new DynamicParameters();
            args.Add("desktopid", mapper.FirstOrDefault().DesktopId);
            DataBaseHelper.ExecuteNonQuery(sqlDel, trans.Connection, trans, args);
            var sql = @"insert into crm_sys_desktop_role_relation(desktopid,roleid) values (@desktopid,@roleid);";
            foreach (DesktopRoleRelationMapper entity in mapper)
            {
                var param = new DynamicParameters();
                param.Add("desktopid", entity.DesktopId);
                param.Add("roleid", entity.RoleId);
                var result = DataBaseHelper.ExecuteNonQuery(sql, trans.Connection, trans, param);
                if (result <= 0)
                {
                    throw new Exception("保存异常");
                }
            }
            return new OperateResult
            {
                Flag = 1,
                Msg = "保存成功"
            };

        }
        public IList<RoleRelationMapper> GetRoles(Guid desktopId, int userId)
        {
            //            select* from
            // (select '00000000-0000-0000-0000-000000000001'::uuid as roleid,'全局角色' as rolename,now() as reccreated,(SELECT count(1) FROM crm_sys_desktop_role_relation where roleid = '00000000-0000-0000-0000-000000000001' and desktopid = @desktopid limit 1 ) as ischecked
            //UNION
            var sql = @"
select vocationid ,vocationname,reccreated,(SELECT count(1) FROM crm_sys_desktop_role_relation where roleid=tmprole.vocationid and  desktopid=@desktopid limit 1 )
 as ischecked from crm_sys_vocation as tmprole where recstatus=1 ) as tmp ORDER BY  reccreated DESC;";
            var param = new DynamicParameters();
            param.Add("userid", userId);
            param.Add("desktopid", desktopId);
            var result = DataBaseHelper.Query<RoleRelationMapper>(sql, param);
            return result;
        }





        #endregion

        public DesktopMapper GetDesktop(int userId)
        {
            var sql = @"select * from crm_sys_desktop where desktopid in (select desktopid from crm_sys_desktop_relation where userid=@userid) AND EXISTS(
select 1 FROM crm_sys_desktop_role_relation desktop 
INNER JOIN  crm_sys_userinfo_role_relate userinfo
ON desktop.roleid=userinfo.roleid
 where userinfo.userid =1 and  desktop.desktopid =(select desktopid from crm_sys_desktop_relation where userid=1 limit 1)) limit 1;";
            var sqlLeft = @"select * from crm_sys_desktop_component where dscomponetid in (
            select   regexp_split_to_table::uuid as leftid  from  regexp_split_to_table((select leftitems from crm_sys_desktop where desktopid in (select desktopid from crm_sys_desktop_relation where userid=@userid) limit 1),','))";
            var sqlRight = @"select * from crm_sys_desktop_component where dscomponetid in (
            select   regexp_split_to_table::uuid as rightid  from  regexp_split_to_table((select rightitems from crm_sys_desktop where desktopid in (select desktopid from crm_sys_desktop_relation where userid=@userid) limit 1),','))";
            var param = new DynamicParameters();
            param.Add("userid", userId);
            var result = DataBaseHelper.QuerySingle<DesktopMapper>(sql, param);
            result.LeftDesktopComponents = DataBaseHelper.Query<DesktopComponentMapper>(sqlLeft, param);
            result.RightDesktopComponents = DataBaseHelper.Query<DesktopComponentMapper>(sqlRight, param);
            return result;
        }


        #region  动态列表

        public PageDataInfo<UBeat.Crm.CoreApi.DomainModel.Dynamics.DynamicInfoExt> GetDynamicList(DynamicListRequestMapper mapper, int userId)
        {
            List<DbParameter> dbParams = new List<DbParameter>();
            int pageIndex = mapper.PageIndex;
            int pageSize = mapper.PageSize;


            DateTime now = DateTime.Now;

            int month = now.Month;
            int year = now.Year;
            int week = GetWeekOfYear(now);
            int quarter = GetQuarterOfYear(now);


            var entityIdSql = string.Empty;
            var businessIdSql = string.Empty;
            var dynamictypeSql = string.Empty;


            string dataRangeSql = string.Empty;
            switch (mapper.DataRangeType)
            {
                case (int)DataRangeType.My:
                    dataRangeSql = string.Format(" and  d.reccreator={0} ", userId);
                    break;

                case (int)DataRangeType.MyDepartment:

                    dataRangeSql = string.Format(@" and au.deptid=(   
                            select au.deptid 
                            from crm_sys_userinfo u
                            inner join crm_sys_account_userinfo_relate au on u.userid = au.userid
                            inner join crm_sys_account a on au.accountid = a.accountid
                            where u.recstatus = 1
                            AND a.recstatus = 1
                            AND au.recstatus = 1
                            AND u.userid = {0} )", userId);
                    break;

                case (int)DataRangeType.LowerDepartment:
                    dataRangeSql = string.Format(@" and au.deptid  IN (SELECT deptid FROM  crm_func_department_tree_power((   
                            select au.deptid
                            from crm_sys_userinfo u
                            inner join crm_sys_account_userinfo_relate au on u.userid = au.userid
                            inner join crm_sys_account a on au.accountid = a.accountid
                            where u.recstatus = 1
                            AND a.recstatus = 1
                            AND au.recstatus = 1
                            AND u.userid = {0}),1,1,{0}))", userId, userId);
                    break;

                case (int)DataRangeType.SpecialDepartment:
                    dataRangeSql = string.Format(" and au.deptid='{0}'", mapper.DepartmetnId);
                    break;

                case (int)DataRangeType.SpecialUser:
                    string idSql = string.Join(",", mapper.UserIds);
                    dataRangeSql = string.Format(" and  d.reccreator in ({0}) ", idSql);
                    break;

                default:
                    break;
            }


            string timeRangeSql = string.Empty;
            switch (mapper.TimeRangeType)
            {
                case (int)TimeRangeType.CurrentDay:
                    timeRangeSql = " and d.reccreated::date=now()::date ";
                    break;

                case (int)TimeRangeType.CurrentWeek:
                    timeRangeSql = " and date_part('year',d.reccreated)=date_part('year',now()) and date_part('week',d.reccreated)=date_part('week',now()) ";
                    break;

                case (int)TimeRangeType.CurrentMonth:
                    timeRangeSql = " and date_part('year',d.reccreated)=date_part('year',now()) and date_part('month',d.reccreated)=date_part('month',now()) ";
                    break;

                case (int)TimeRangeType.CurrentQuarter:
                    timeRangeSql = " and date_part('year',d.reccreated)=date_part('year',now()) and date_part('quarter',d.reccreated)=date_part('quarter',now()) ";
                    break;

                case (int)TimeRangeType.CurrentYear:
                    timeRangeSql = " and date_part('year',d.reccreated)=date_part('year',now()) ";
                    break;



                case (int)TimeRangeType.Yesterday:
                    DateTime yesterday = DateTime.Now.AddDays(-1);
                    timeRangeSql = string.Format(" and d.reccreated::date='{0}'::date ", yesterday.ToString());
                    break;


                case (int)TimeRangeType.LastWeek:
                    if (week == 1)
                    {
                        week = 52;
                        year = year - 1;
                    }
                    else
                    {
                        week = week - 1;
                    }
                    timeRangeSql = string.Format(" and date_part('year',d.reccreated)={0} and date_part('week',d.reccreated)={1} ", year, week);
                    break;


                case (int)TimeRangeType.LastMonth:
                    if (month == 1)
                    {
                        month = 12;
                        year = year - 1;
                    }
                    else
                    {
                        month = month - 1;
                    }

                    timeRangeSql = string.Format(" and date_part('year',d.reccreated)={0} and date_part('month',d.reccreated)={1} ", year, month);
                    break;


                case (int)TimeRangeType.LastQuarter:
                    if (quarter == 1)
                    {
                        quarter = 4;
                        year = year - 1;
                    }
                    else
                    {
                        quarter = quarter - 1;

                    }
                    timeRangeSql = string.Format(" and date_part('year',d.reccreated)={0} and date_part('quarter',d.reccreated)={1} ", year, quarter);
                    break;


                case (int)TimeRangeType.LastYear:
                    year = year - 1;
                    timeRangeSql = string.Format(" and date_part('year',d.reccreated)={0} ", year);
                    break;


                case (int)TimeRangeType.SpecialYear:
                    timeRangeSql = string.Format(" and date_part('year',d.reccreated)={0} ", mapper.SpecialYear);
                    break;


                case (int)TimeRangeType.TimeRnage:
                    timeRangeSql = string.Format(" and d.reccreated between '{0}' and '{1}'", mapper.StartTime, mapper.EndTime);
                    break;


                default:
                    break;
            }




            //处理主实体id
            if (mapper.MainEntityId != Guid.Empty)
            {
                entityIdSql = " and  d.entityid=@entityid ";
                dbParams.Add(new NpgsqlParameter("entityid", mapper.MainEntityId));
            }


            string relatedEntitySql = string.Empty;
            if (mapper.RelatedEntityId != Guid.Empty)
            {
                relatedEntitySql = " and d.relentityid=@relentityid ";
                dbParams.Add(new NpgsqlParameter("relentityid", mapper.RelatedEntityId));
            }
            else
            {
                relatedEntitySql = string.Format(@" and  re.entityname like '%{0}%'", mapper.SearchKey);
            }


            string strSql = @"SELECT d.*,t.tempcontent::Jsonb,e.entityname,ec.categoryname AS TypeName, re.entityname AS relentityname,
                                u.usericon AS reccreatorUserIcon,u.username AS reccreatorname,
				                array(
					                SELECT u.username  
                                    FROM crm_sys_dynamic_praise AS p 
					                LEFT JOIN crm_sys_userinfo AS u ON u.userid=p.reccreator
					                WHERE p.dynamicid=d.dynamicid 
                                    AND p.recstatus=1 
                                    ORDER BY p.reccreated
				                ) AS PraiseUsers,
				                (SELECT array_to_json(array_agg(row_to_json(t))) 
                                 FROM
						         (
                                            SELECT c.dynamicid, c.commentsid,c.pcommentsid,c.comments,c.reccreator,
                                                   u.username AS reccreator_name,u.usericon AS reccreator_icon,
                                                   c.reccreated,uc.username AS tocommentor,dc.comments AS tocomments
                                            FROM crm_sys_dynamic_comments AS c 
							                LEFT JOIN crm_sys_userinfo AS u ON u.userid=c.reccreator
							                LEFT JOIN crm_sys_dynamic_comments AS dc ON dc.commentsid=c.pcommentsid
							                LEFT JOIN crm_sys_userinfo AS uc ON uc.userid=dc.reccreator
							                WHERE c.dynamicid=d.dynamicid 
                                            AND c.recstatus=1 
                                            ORDER BY c.reccreated
                                   ) AS t
				                 ) AS Comments
                            FROM public.crm_sys_dynamics AS d 
                            LEFT JOIN crm_sys_dynamic_template AS t ON t.templateid=d.templateid
                            LEFT JOIN crm_sys_entity AS re ON re.entityid=d.relentityid
                            LEFT JOIN crm_sys_entity AS e ON e.entityid=d.entityid 
                            LEFT JOIN crm_sys_entity_category AS ec ON ec.categoryid=d.typeid
                            LEFT JOIN crm_sys_userinfo AS u ON u.userid=d.reccreator 
                            LEFT JOIN crm_sys_account_userinfo_relate au on u.userid =au.userid
                            LEFT JOIN crm_sys_account a on au.accountid=a.accountid
                            WHERE d.recstatus=1
                            AND u.recstatus=1 
                            AND a.recstatus=1 
                            AND au.recstatus=1
                            {0} {1} {2} {3}
                            ORDER BY d.recversion DESC ";


            var executeSql = string.Format(strSql, dataRangeSql, timeRangeSql, entityIdSql, relatedEntitySql);

            dbParams.Add(new NpgsqlParameter("userid", userId));
            var result = ExecuteQueryByPaging<UBeat.Crm.CoreApi.DomainModel.Dynamics.DynamicInfoExt>(executeSql, dbParams.ToArray(), pageSize, pageIndex);
            result.DataList = result.DataList.OrderByDescending(m => m.RecCreated).ToList();
            return result;
        }



        public IList<dynamic> GetMainEntityList(int userId)
        {
            var strSql = @" select '00000000-0000-0000-0000-000000000000' as entityid,'全部' as entityname
                            union all
                            select entityid,entityname
                            from crm_sys_entity 
                            where modeltype=0
                            and recstatus=1 ";

            var param = new DynamicParameters();
            var result = DataBaseHelper.Query<dynamic>(strSql, param);
            return result;

        }

        public IList<dynamic> GetRelatedEntityList(Guid entityid, int userId)
        {
            var strSql = @"select '00000000-0000-0000-0000-000000000000' as entityid,'全部' as entityname
                            union all
                            select entityid,entityname
                            from crm_sys_entity 
                            where  recstatus=1
                            and relentityid=@relentityid ";

            var param = new DynamicParameters();
            param.Add("relentityid", entityid);
            var result = DataBaseHelper.Query<dynamic>(strSql, param);
            return result;

        }


        public int GetWeekOfYear(DateTime date)
        {
            int week = 0;

            CultureInfo myCI = new CultureInfo("en-US");
            Calendar myCal = myCI.Calendar;
            CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
            week = myCal.GetWeekOfYear(date, myCWR, DayOfWeek.Monday);

            return week;

        }



        private int GetQuarterOfYear(DateTime date)
        {

            var month = date.Month;
            int quarter = 0;

            if (month >= 1 && month <= 3)
            {

                quarter = 1;
            }
            else if (month >= 4 && month <= 6)
            {

                quarter = 2;
            }

            else if (month >= 7 && month <= 9)
            {

                quarter = 3;
            }
            else if (month >= 10 && month <= 12)
            {

                quarter = 4;
            }
            else
            {

                quarter = 0;
            }

            return quarter;

        }

        #endregion



    }
}
