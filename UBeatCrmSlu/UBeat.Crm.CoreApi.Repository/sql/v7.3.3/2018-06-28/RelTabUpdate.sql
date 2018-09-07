CREATE OR REPLACE FUNCTION "public"."crm_func_rel_tab_update"("_relid" uuid, "_relentityid" uuid, "_fieldid" uuid, "_relname" text, "_icon" text, "_ismanytomany" int4, "_srcsql" text, "_srctitle" text, "_userno" int4,_reltablanguage jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE
 
  --_relid uuid:=null;
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
 
				 IF  NOT EXISTS (SELECT 1 FROM crm_sys_entity WHERE entityid=_relentityid AND recstatus=1 LIMIT 1)  THEN  
								Raise EXCEPTION '该实体不存在';
				 END IF; 
         IF NOT EXISTS (SELECT 1 FROM crm_sys_entity_fields WHERE fieldid=_fieldid AND recstatus=1 LIMIT 1) THEN
								Raise EXCEPTION '该实体字段不存在';
         END IF;
				 IF  NOT EXISTS (SELECT 1 FROM crm_sys_entity_rel_tab WHERE relid=_relid AND recstatus=1 LIMIT 1)  THEN  
								Raise EXCEPTION '该页签不存在';
				 END IF; 
        IF EXISTS(SELECT 1 FROM crm_sys_entity_rel_tab WHERE relid<>_relid AND recstatus=1 AND relname=_relname  LIMIT 1 ) THEN
             Raise EXCEPTION '该页签名称已存在';
        END IF;
 
        UPDATE crm_sys_entity_rel_tab SET
        "relentityid"=_relentityid,
        "relname"=_relname,
        "fieldid"=_fieldid,
        "icon"=_icon::uuid,
        "recupdator"=_userno,recupdated=now(),ismanytomany=_ismanytomany,srcsql=_srcsql,srctitle=_srctitle,
				reltablanguage = _reltablanguage
				WHERE relid=_relid;

        UPDATE crm_sys_function SET funcname=_relname WHERE relationvalue=_relid::TEXT;
		_codeid:= _relid::TEXT;
		_codeflag:= 1;
		_codemsg:= '更新页签配置成功';
 
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
