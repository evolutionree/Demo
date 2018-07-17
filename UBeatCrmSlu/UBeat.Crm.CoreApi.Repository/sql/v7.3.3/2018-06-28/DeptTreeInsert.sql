CREATE OR REPLACE FUNCTION "public"."crm_func_department_add"("_topdeptid" uuid, "_deptname" text, "_oglevel" int4, "_userno" int4, _deptlanguage jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
DECLARE
   _deptid uuid;
   _orderby int4;
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;
BEGIN

   BEGIN
				 --数据逻辑
				 IF _deptname IS NULL OR _deptname = '' THEN
								Raise EXCEPTION '部门名称不能为空';
				 END IF;
         IF EXISTS(SELECT 1 FROM crm_sys_department where pdeptid=_topdeptid AND deptname=_deptname LIMIT 1) THEN
								Raise EXCEPTION '同一级的部门名称不能重复';
         END IF;
         SELECT (COALESCE(MAX(recorder),0)+1) INTO _orderby FROM crm_sys_department WHERE pdeptid=_topdeptid;
				 INSERT INTO crm_sys_department (deptname, oglevel,recorder,reccreator,recupdator,pdeptid,deptlanguage) 
				 VALUES (_deptname,_oglevel,_orderby,_userno,_userno,_topdeptid,_deptlanguage) RETURNING deptid INTO _deptid;
 
					INSERT INTO crm_sys_department_treepaths(ancestor,descendant,nodepath)
					SELECT t.ancestor,_deptid,nodepath+1
					FROM crm_sys_department_treepaths AS t
					WHERE t.descendant = _topdeptid
					UNION ALL
					SELECT _deptid,_deptid,0;

					_codeid:= _deptid::TEXT;
					_codeflag:= 1;
					_codemsg:= '新增部门成功';
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

