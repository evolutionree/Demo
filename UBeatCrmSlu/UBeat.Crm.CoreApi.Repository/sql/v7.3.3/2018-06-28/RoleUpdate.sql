CREATE OR REPLACE FUNCTION "public"."crm_func_role_edit"("_rolegroupid" text, "_roleid" text, "_rolename" text, "_roletype" int4, "_rolepriority" int4, "_roleremark" text, "_userno" int4, _rolelanguage jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE

  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;
  _orderby int4:=0;
BEGIN

   BEGIN
				 --数据逻辑
				 IF _rolename IS NULL OR _rolename = '' THEN
								Raise EXCEPTION '角色名称不能为空';
				 END IF;
				 IF NOT EXISTS(SELECT 1 FROM crm_sys_role WHERE roleid=_roleid::uuid AND recstatus=1 LIMIT 1)THEN
								Raise EXCEPTION '该角色记录不存在';
				 END IF;
				 IF NOT EXISTS(SELECT 1 FROM crm_sys_role_group_relate WHERE roleid=_roleid::uuid  LIMIT 1)THEN
								Raise EXCEPTION '该角色分组关系已存在';
				 END IF;
				 IF EXISTS(SELECT 1 FROM crm_sys_role WHERE roleid<>_roleid::uuid AND rolename=_rolename AND recstatus=1 LIMIT 1)THEN
								Raise EXCEPTION '角色名称已存在';
				 END IF;
         IF NOT EXISTS(SELECT 1 FROM crm_sys_userinfo_role_relate WHERE userid=_userno AND roleid='63dd2a9d-7f75-42ff-a696-7cc841e884e7'::uuid LIMIT 1) THEN
						 IF EXISTS(SELECT 1 FROM crm_sys_role WHERE roleid=_roleid::uuid AND roletype=0 LIMIT 1) THEN
									 SELECT rolename INTO _rolename FROM  crm_sys_role WHERE roleid=_roleid::uuid AND roletype=0 LIMIT 1;
									 Raise EXCEPTION '%',_rolename||'为系统角色,不允许编辑';
						 END IF;
         END IF;
 
         UPDATE crm_sys_role SET
                rolename=_rolename,
                roletype=_roletype,
                rolepriority=_rolepriority,
                roleremark=_roleremark,
                recupdated=now(),
rolelanguage=_rolelanguage,
                recupdator=_userno where roleid=_roleid::uuid;
         UPDATE crm_sys_role_group_relate SET
                rolegroupid=_rolegroupid::uuid where roleid=_roleid::uuid;
					_codeid:= _roleid;
					_codeflag:= 1;
					_codemsg:= '更新角色成功';
	 EXCEPTION WHEN OTHERS THEN
						 GET STACKED DIAGNOSTICS _codestack = PG_EXCEPTION_CONTEXT;
						 _codemsg:=SQLERRM;
						 _codestatus:=SQLSTATE;
		END;
   
   		--RETURN RESULT
	  RETURN QUERY EXECUTE format('SELECT $1,$2,$3,$4,$5')
	  USING  _codeid,_codeflag,_codemsg,_codestack,_codestatus;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;
