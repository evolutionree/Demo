using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Department;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.Department
{
    public class DepartmentPermissionRepository : RepositoryBase, IDeptPermissionRepository
    {
        public void DeletePermissionItemByRole(DbTransaction tran, Guid schemeId, Guid authorized_roleid, int userId)
        {
            try
            {
                string strSQL = @"delete from crm_sys_pm_orgschemeentry where schemeid = @schemeid and authorized_roleid=@authorized_roleid;";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@schemeid",schemeId),
                    new Npgsql.NpgsqlParameter("@authorized_roleid",authorized_roleid)
                };
                if (tran == null)
                    DBHelper.ExecuteNonQuery("", strSQL, p);
                else
                    DBHelper.ExecuteNonQuery(tran, strSQL, p);
            }
            catch (Exception ex) {

            }
        }

        public void DeletePermissionItemByUser(DbTransaction tran, Guid schemeId, int authorized_userid, int userId)
        {
            try
            {
                string strSQL = @"delete from crm_sys_pm_orgschemeentry where schemeid = @schemeid and authorized_userid=@authorized_userid;";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@schemeid",schemeId),
                    new Npgsql.NpgsqlParameter("@authorized_userid",authorized_userid)
                };
                if (tran == null)
                    DBHelper.ExecuteNonQuery("", strSQL, p);
                else
                    DBHelper.ExecuteNonQuery(tran, strSQL, p);
            }
            catch (Exception ex)
            {

            }
        }

        public List<Guid> GetRolesByUser(DbTransaction tran, int searchResultUserId, int userId)
        {
            try
            {
                string strSQL = @"select distinct  a.roleid 
from crm_sys_userinfo_role_relate  a
	inner join crm_sys_role b on a.roleid = b.roleid 
where a.userid =@userid";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@userid",searchResultUserId)
                };
                List<Dictionary<string, object>> ret = null;
                if (tran == null)
                    ret = DBHelper.ExecuteQuery("", strSQL, p);
                else
                    ret = DBHelper.ExecuteQuery(tran, strSQL, p);
                List<Guid> retList = new List<Guid>();
                foreach(Dictionary<string,object> item in ret)
                {
                    if (item.ContainsKey("roleid") && item["roleid"] != null) {
                        Guid tmp = Guid.Empty;
                        if (Guid.TryParse(item["roleid"].ToString(), out tmp)) {
                            retList.Add(tmp);
                        }
                    }
                }
            }
            catch (Exception ex) {

            }
            return new List<Guid>();
        }

        public List<Dictionary<string, object>> ListAuthorizedObjects(DbTransaction tran, Guid schemeId, int userId)
        {
            try
            {
                string strSQL = @"select distinct *
from (select a.authorized_type, a.authorized_userid ,b.username authorized_username ,null authorized_roleid ,'' authorized_rolename 
FROM crm_sys_pm_orgschemeentry a
	inner join (select *from crm_sys_userinfo where recstatus =1 )  b on a.authorized_userid = b.userid 
where a.authorized_type =1  and a.schemeid = @schemeid
union all 
select a.authorized_type, 0 authorized_userid ,	''  authorized_username ,a.authorized_roleid ,b.deptname  authorized_rolename 
FROM crm_sys_pm_orgschemeentry a
	inner join (select *from crm_sys_department where recstatus =1 )  b on a.authorized_roleid = b.deptid  
where a.authorized_type =2
    and a.schemeid = @schemeid
) total 
order by total.authorized_type,total.authorized_rolename ,total.authorized_username";
                DbParameter[] p = new Npgsql.NpgsqlParameter[] {
                    new Npgsql.NpgsqlParameter("@schemeid",schemeId)
                };
                if (tran == null)
                    return DBHelper.ExecuteQuery("", strSQL, p);
                else
                    return DBHelper.ExecuteQuery(tran, strSQL, p);
            }
            catch (Exception ex) {
                
            }
            return new List<Dictionary<string, object>>();
        }

        public List<DeptPermissionSchemeEntryInfo> ListPermissionDetailByRoleId(Guid SchemeId, Guid RoleId, int userNum, DbTransaction dbTransaction)
        {
            try
            {
                string strSQL = @"select aaa.* from 
(
select  b.recid,b.schemeid ,b.authorized_roleid,b.authorized_userid,b.authorized_type ,
			2 pmobject_type,a.deptid pmobject_deptid,
			a.deptname PMObject_DeptName,b.pmobject_userid,'' PMObj_UserName,b.permissiontype ,b.subdeptpermission,b.subuserpermission, a.pdeptid parentid  
from crm_sys_department a
	left outer join crm_sys_pm_orgschemeentry b on 
	( a.deptid = b.pmobject_deptid and b.pmobject_type = 2)
where a.recstatus = 1  
union all 
select  b.recid,b.schemeid ,b.authorized_roleid,b.authorized_userid,b.authorized_type ,
			1  pmobject_type,b.pmobject_deptid pmobject_deptid,
			'' PMObject_DeptName,a.userid pmobj_userid,a.username PMObj_UserName,b.permissiontype ,b.subdeptpermission,b.subuserpermission,c.deptid parentid
from crm_sys_userinfo  a
	left outer join crm_sys_pm_orgschemeentry b on 
	( a.userid = b.pmobject_userid and b.pmobject_type = 2)
	left outer join  (select * from crm_sys_account_userinfo_relate where recstatus =1 ) c on c.userid = a.userid 
where a.recstatus = 1 
) aaa
where (aaa.schemeid is null or aaa.schemeid=@schemeid) and (aaa.authorized_roleid is null or aaa.authorized_roleid = @authorized_roleid)";
                Npgsql.NpgsqlParameter [] p = new Npgsql.NpgsqlParameter[] {
                    new Npgsql.NpgsqlParameter("@schemeid",SchemeId),
                    new Npgsql.NpgsqlParameter("@authorized_roleid",RoleId)
                };
                if (dbTransaction!= null )
                    return DBHelper.ExecuteQuery<DeptPermissionSchemeEntryInfo>(dbTransaction, strSQL, p);
                else 
                    return DBHelper.ExecuteQuery<DeptPermissionSchemeEntryInfo>("", strSQL, p);
            }
            catch (Exception ex) {
                
            }
            return new List<DeptPermissionSchemeEntryInfo>();
        }

        public List<DeptPermissionSchemeEntryInfo> ListPermissionDetailByUserId(Guid SchemeId, int authed_userid, int userNum, DbTransaction dbTransaction)
        {
            try
            {
                string strSQL = @"select aaa.* from 
(
select  b.recid,b.schemeid ,b.authorized_roleid,b.authorized_userid,b.authorized_type ,
			2 pmobject_type,a.deptid pmobject_deptid,
			a.deptname PMObject_DeptName,b.pmobject_userid,'' PMObj_UserName,b.permissiontype ,b.subdeptpermission,b.subuserpermission, a.pdeptid parentid  
from crm_sys_department a
	left outer join crm_sys_pm_orgschemeentry b on 
	( a.deptid = b.pmobject_deptid and b.pmobject_type = 2)
where a.recstatus = 1  
union all 
select  b.recid,b.schemeid ,b.authorized_roleid,b.authorized_userid,b.authorized_type ,
			1  pmobject_type,b.pmobject_deptid pmobject_deptid,
			'' PMObject_DeptName,a.userid pmobj_userid,a.username PMObj_UserName,b.permissiontype ,b.subdeptpermission,b.subuserpermission,c.deptid parentid
from crm_sys_userinfo  a
	left outer join crm_sys_pm_orgschemeentry b on 
	( a.userid = b.pmobject_userid and b.pmobject_type = 2)
	left outer join  (select * from crm_sys_account_userinfo_relate where recstatus =1 ) c on c.userid = a.userid 
where a.recstatus = 1 
) aaa
where (aaa.schemeid is null or aaa.schemeid=@schemeid) and (aaa.authorized_userid is null or aaa.authorized_userid = @authorized_userid)";
                Npgsql.NpgsqlParameter[] p = new Npgsql.NpgsqlParameter[] {
                    new Npgsql.NpgsqlParameter("@schemeid",SchemeId),
                    new Npgsql.NpgsqlParameter("@authorized_userid",authed_userid)
                };
                if (dbTransaction != null)
                    return DBHelper.ExecuteQuery<DeptPermissionSchemeEntryInfo>(dbTransaction, strSQL, p);
                else
                    return DBHelper.ExecuteQuery<DeptPermissionSchemeEntryInfo>("", strSQL, p);
            }
            catch (Exception ex)
            {

            }
            return new List<DeptPermissionSchemeEntryInfo>();
        }

        public void SavePermissionItem(DbTransaction tran, List<DeptPermissionSchemeEntryInfo> items, int userId)
        {
            try
            {
                string strSQL = @"INSERT INTO  crm_sys_pm_orgschemeentry (recid, schemeid, authorized_userid, authorized_roleid, authorized_type, pmobject_userid, pmobject_deptid, pmobject_type, permissiontype, subdeptpermission, subuserpermission)
select * from json_populate_recordset(null::crm_sys_pm_orgschemeentry,@data::json)";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@data",Newtonsoft.Json.JsonConvert.SerializeObject(items,Formatting.Indented,new JsonSerializerSettings(){ ContractResolver = new CamelCasePropertyNamesContractResolver() }))
                };
                if (tran == null) DBHelper.ExecuteNonQuery("", strSQL, p);
                else DBHelper.ExecuteNonQuery(tran, strSQL, p);
            }
            catch (Exception ex) {
                
            }
        }
    }
}
