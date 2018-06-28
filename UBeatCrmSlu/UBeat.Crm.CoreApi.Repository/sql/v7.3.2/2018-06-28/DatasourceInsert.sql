CREATE OR REPLACE FUNCTION "public"."crm_func_datasource_add"("_datasourcename" varchar, "_srctype" int4, "_entityid" text, "_srcmark" varchar, "_isrelatepower" int4, "_status" int4, "_ispro" int4, "_userno" int4 , _datasourcelanguage jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE
   _datasrcid uuid; 
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

BEGIN

   BEGIN


          IF  EXISTS(select 1 from crm_sys_entity_datasource where datasrcname=_datasourcename AND recstatus=1 LIMIT 1) THEN
                RAISE EXCEPTION '数据源名称不能重复';
          END IF;

          INSERT INTO "public"."crm_sys_entity_datasource" (
                            "datasrcname", 
                            "srctype",
                            "entityid", 
                            "srcmark",
                            "ispro",
                            "isrelatepower", 
                            "recstatus", 
                            "reccreator", 
                            "recupdator",
datasourcelanguage) 
                            VALUES
                            (_datasourcename,
                            _srctype,
                            _entityid::uuid,
                            _srcmark,
                            _ispro,
                            _isrelatepower, 
                            _status,
                            _userno,
                            _userno,
_datasourcelanguage) returning datasrcid into _datasrcid;
					_codeid:= _datasrcid::TEXT;
					_codeflag:= 1;
					_codemsg:= '新增数据源成功';
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

