CREATE OR REPLACE FUNCTION "public"."crm_func_role_rule_param_format_for_job"("_paramsql" text, "_userno" int4)
  RETURNS "pg_catalog"."text" AS $BODY$
DECLARE
  _user_deptid uuid;
  _format_sql TEXT:='';

BEGIN
			IF _paramsql IS NULL OR _paramsql='' THEN
            RETURN _format_sql;
      END IF;

			_format_sql:=REPLACE(_paramsql,'{UserNo}','u_tmp.userid');
		
			_format_sql:=REPLACE(_format_sql,'{currentUser}','u_tmp.userid');

			_format_sql:=REPLACE(_format_sql,'{UserDeptPeople}','(SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid = (select crm_func_userinfo_udeptcode(u_tmp.userid))');
			_format_sql:=REPLACE(_format_sql,'{currentDepartment}','(SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid = (select crm_func_userinfo_udeptcode(u_tmp.userid))');

			_format_sql:=REPLACE(_format_sql,'{UserDeptPeopleWithSub}','(SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid IN (SELECT deptid FROM crm_func_department_tree((select crm_func_userinfo_udeptcode(u_tmp.userid)), 1)) )');
			_format_sql:=REPLACE(_format_sql,'{subDepartment}','(SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid IN (SELECT deptid FROM crm_func_department_tree((select crm_func_userinfo_udeptcode(u_tmp.userid)), 1)) )');

			--取部门id
			_format_sql:=REPLACE(_format_sql,'{currentDeptId}','(select crm_func_userinfo_udeptcode(u_tmp.userid))');
			_format_sql:=REPLACE(_format_sql,'{subDirectDeptId}','(SELECT deptid::text FROM crm_sys_department WHERE recstatus = 1 AND deptid IN (SELECT deptid FROM crm_func_department_tree((select crm_func_userinfo_udeptcode(u_tmp.userid)), 1)) )');
			_format_sql:=REPLACE(_format_sql,'{noLeaderDepartment}','(select userid  from crm_sys_account_userinfo_relate  
                where recstatus= 1 and 
	                deptid not in (
	                select a.deptid
	                from crm_sys_account_userinfo_relate a
				                inner join crm_sys_userinfo b on a.userid= b.userid 
	                where a.recstatus =1  and b.recstatus =1 and b.isleader = 1 
                ))');
 
      RETURN _format_sql;
 
END
 $BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
;



CREATE OR REPLACE FUNCTION "public"."crm_func_role_rule_param_format"("_paramsql" text, "_userno" int4)
  RETURNS "pg_catalog"."text" AS $BODY$
DECLARE
  _user_deptid uuid;
  _format_sql TEXT:='';

BEGIN
    
			IF _paramsql IS NULL OR _paramsql='' THEN
            RETURN _format_sql;
      END IF;

			_format_sql:=REPLACE(_paramsql,'{UserNo}',_userno::TEXT);

			_format_sql:=REPLACE(_format_sql,'{currentUser}',_userno::TEXT);

			--_format_sql:=REPLACE(_format_sql,'{querydata}','''''');
			_format_sql:=REPLACE(_format_sql,'{querydata}',' AND 1=1 ');

			SELECT udeptid INTO _user_deptid FROM crm_func_userinfo_udeptcode(_userno);
			_format_sql:=REPLACE(_format_sql,'{UserDeptPeople}','(SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid = ''' || _user_deptid || ''')');
			_format_sql:=REPLACE(_format_sql,'{currentDepartment}','(SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid = ''' || _user_deptid || ''')');

			_format_sql:=REPLACE(_format_sql,'{UserDeptPeopleWithSub}','(SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid IN (SELECT deptid FROM crm_func_department_tree(''' || _user_deptid || ''', 1)) )');
			_format_sql:=REPLACE(_format_sql,'{subDepartment}','(SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid IN (SELECT deptid FROM crm_func_department_tree(''' || _user_deptid || ''', 1)) )');

			--取部门id
			_format_sql:=REPLACE(_format_sql,'{currentDeptId}',''''||_user_deptid::TEXT||'''');
			_format_sql:=REPLACE(_format_sql,'{subDirectDeptId}','(SELECT deptid::text FROM crm_sys_department WHERE recstatus = 1 AND deptid IN (SELECT deptid FROM crm_func_department_tree(''' || _user_deptid || ''', 1)) )');
			_format_sql:=REPLACE(_format_sql,'{noLeaderDepartment}','(select userid  from crm_sys_account_userinfo_relate  
                where recstatus= 1 and 
	                deptid not in (
	                select a.deptid
	                from crm_sys_account_userinfo_relate a
				                inner join crm_sys_userinfo b on a.userid= b.userid 
	                where a.recstatus =1  and b.recstatus =1 and b.isleader = 1 
                ))');
 
      RETURN _format_sql;
 
END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
;

