CREATE OR REPLACE FUNCTION "public"."crm_func_role_add"("_rolegroupid" text, "_rolename" varchar, "_roletype" int4, "_rolepriority" int4, "_roleremark" varchar, "_userno" int4,_rolelanguage jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE
   _roleid uuid;

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

				 IF EXISTS(select 1 from crm_sys_role where rolename=_rolename and recstatus=1)THEN
								Raise EXCEPTION '角色名称已存在';
				 END IF;

         select COALESCE((MAX(recorder)+1),0) into _orderby from crm_sys_role;
         raise notice '%',_orderby;
				 INSERT INTO crm_sys_role (rolename, roletype,rolepriority,roleremark,recorder,reccreator,recupdator,rolelanguage) 
				 VALUES (_rolename,_roletype,_rolepriority,_roleremark,_orderby,_userno,_userno,_rolelanguage)  returning roleid into _roleid;
				 INSERT INTO crm_sys_role_group_relate (rolegroupid, roleid) 
				 VALUES (_rolegroupid::uuid,_roleid);

					_codeid:= _roleid::TEXT;
					_codeflag:= 1;
					_codemsg:= '新增角色成功';
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
