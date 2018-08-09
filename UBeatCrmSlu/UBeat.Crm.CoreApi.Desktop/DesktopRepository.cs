﻿using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using UBeat.Crm.CoreApi.Repository.Repository;
using UBeat.Crm.CoreApi.Repository.Utility;
using System.Data;
using UBeat.Crm.CoreApi.DomainModel;
using System.Linq;

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
            var sql = @"insert into crm_sys_desktop (desktopname,basedeskid,description) values (@desktopname,@basedeskid,@description)";
            var param = new DynamicParameters();
            param.Add("desktopid", mapper.DesktopId);
            param.Add("desktopname", mapper.DesktopName);
            param.Add("basedeskid", mapper.BaseDeskId);
            param.Add("description", mapper.Description);
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

        public DesktopMapper GetDesktopDetail(Guid desktopId)
        {
            var sql = @"select desktopid,desktopname,description from crm_sys_desktop where desktopId=@desktopId limit 1;";
            var param = new DynamicParameters();
            param.Add("desktopId", desktopId);
            var result = DataBaseHelper.QuerySingle<DesktopMapper>(sql, param);
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
        public IList<dynamic> GetRoles(int userId)
        {
            var sql = @"select * from 
(select '00000000-0000-0000-0000-000000000001'::uuid,'全局角色',now() as reccreated 
UNION
select roleid,rolename,reccreated from crm_sys_role where recstatus=1 ) as tmp ORDER BY  reccreated DESC;";
            var param = new DynamicParameters();
            param.Add("userid", userId);
            var result = DataBaseHelper.Query<dynamic>(sql, param);
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
    }
}