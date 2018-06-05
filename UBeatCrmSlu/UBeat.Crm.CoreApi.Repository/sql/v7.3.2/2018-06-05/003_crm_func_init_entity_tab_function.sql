CREATE OR REPLACE FUNCTION "public"."crm_func_init_entity_tab_function"("_entityid" uuid, "_relid" uuid, "_userno" int4)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
DECLARE
   _funcid uuid;
   _relname TEXT:='sss';
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;
BEGIN

	BEGIN
		    --获取表名
    IF NOT EXISTS(SELECT 1 FROM crm_sys_function WHERE rectype=4 AND entityid=_entityid ORDER BY reccreated ASC LIMIT 1) THEN
				 Raise EXCEPTION '该实体不存在主页Tab职能节点,请完善节点信息';
    END IF;
    SELECT relname INTO _relname  FROM crm_sys_entity_rel_tab WHERE relid=_relid LIMIT 1;
    SELECT funcid INTO _funcid FROM crm_sys_function WHERE rectype=4 AND entityid=_entityid and devicetype = 0  LIMIT 1;
		--raise notice '_relid:%',_relid;
    --web
		SELECT id,flag,msg,stacks,codes INTO _codeid,_codeflag,_codemsg,_codestack,_codestatus 
    FROM  crm_func_function_insert(1,'api/dynamicentity/reltablist',_funcid,_relname||'','EntityRelList',_entityid,0,null,_relid::TEXT,_userno);
		--raise notice '%',_codemsg;
    --mob

    SELECT funcid INTO _funcid FROM crm_sys_function WHERE rectype=4 AND entityid=_entityid and devicetype = 1  LIMIT 1;
		SELECT id,flag,msg,stacks,codes INTO _codeid,_codeflag,_codemsg,_codestack,_codestatus 
    FROM  crm_func_function_insert(1,'api/dynamicentity/reltablist',_funcid,_relname||'','EntityRelList',_entityid,1,null,_relid::TEXT,_userno);
		--raise notice '%',_codemsg;
		_codeid:= _funcid::TEXT;
		_codeflag:= 1;
		_codemsg:= '初始化主页Tab职能';
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

ALTER FUNCTION "public"."crm_func_init_entity_tab_function"("_entityid" uuid, "_relid" uuid, "_userno" int4) OWNER TO "postgres";