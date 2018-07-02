CREATE OR REPLACE FUNCTION "public"."crm_func_role_group_add"("_groupname" text, "_grouptype" int4, "_userno" int4,_grouplanguage jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE
   _rolegroupid uuid;

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
				 IF _groupname IS NULL OR _groupname = '' THEN
								Raise EXCEPTION '角色分组名称不能为空';
				 END IF;
				 IF EXISTS(SELECT 1 FROM crm_sys_role_group WHERE rolegroupname=_groupname AND recstatus=1 LIMIT 1) THEN
								Raise EXCEPTION '角色分类名称已存在,请修正';
				 END IF;


         select (COALESCE((MAX(recorder)),0)+1) into _orderby from crm_sys_role;
         raise notice '%',_orderby;
				 INSERT INTO crm_sys_role_group (rolegroupname, grouptype,recorder,reccreator,recupdator,grouplanguage) 
				 VALUES (_groupname,_grouptype,_orderby,_userno,_userno,_grouplanguage)  returning rolegroupid into _rolegroupid;

					_codeid:= _rolegroupid::TEXT;
					_codeflag:= 1;
					_codemsg:= '新增角色分组成功';
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

