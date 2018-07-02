CREATE OR REPLACE FUNCTION "public"."crm_func_datasource_edit"("_datasourceid" text, "_datasourcename" text, "_srctype" int4, "_entityid" text, "_srcmark" text, "_status" int4, "_isrelatepower" int4, "_ispro" int4, "_userno" int4, _datasourcelanguage jsonb)
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

BEGIN

   BEGIN


          IF  EXISTS(select 1 from crm_sys_entity_datasource where datasrcid<>_datasourceid::uuid and  datasrcname=_datasourcename AND recstatus=1 LIMIT 1) THEN
                RAISE EXCEPTION '数据源名称不能重复';
          END IF;
--           IF _entityid IS NULL OR _entityid='' THEN
--               _entityid:='00000000-0000-0000-0000-000000000000';
--           END IF;
 
          UPDATE crm_sys_entity_datasource SET 
          datasrcname=_datasourcename,
					srctype=_srctype,
					srcmark=_srcmark,
					recupdator=_userno ,
					ispro=_ispro ,
          recupdated=now(),
					recstatus=_status,
					isrelatepower = _isrelatepower,
          entityid=_entityid::uuid,
datasourcelanguage=_datasourcelanguage
					WHERE datasrcid=_datasourceid::uuid;
 
					_codeid:= _datasourceid::TEXT;
					_codeflag:= 1;
					_codemsg:= '更新数据源成功';
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

