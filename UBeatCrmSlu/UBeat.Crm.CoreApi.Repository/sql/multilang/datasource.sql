
alter table crm_sys_entity_datasource drop column datasourcelanguage;
alter table crm_sys_entity_datasource add datasourcename_lang jsonb;

drop FUNCTION "public"."crm_func_datasource_add"("_datasourcename" varchar, "_srctype" int4, "_entityid" text, "_srcmark" varchar, "_isrelatepower" int4, "_status" int4, "_ispro" int4, "_userno" int4);
drop FUNCTION "public"."crm_func_datasource_add"("_datasourcename" varchar, "_srctype" int4, "_entityid" text, "_srcmark" varchar, "_isrelatepower" int4, "_status" int4, "_ispro" int4, "_userno" int4, "_datasourcelanguage" jsonb);
drop FUNCTION "public"."crm_func_datasource_edit"("_datasourceid" text, "_datasourcename" text, "_srctype" int4, "_entityid" text, "_srcmark" text, "_status" int4, "_isrelatepower" int4, "_ispro" int4, "_userno" int4);
drop FUNCTION "public"."crm_func_datasource_edit"("_datasourceid" text, "_datasourcename" text, "_srctype" int4, "_entityid" text, "_srcmark" text, "_status" int4, "_isrelatepower" int4, "_ispro" int4, "_userno" int4, "_datasourcelanguage" jsonb);



CREATE OR REPLACE FUNCTION "public"."crm_func_datasource_list"("_datasourcename" text, "_status" int4, "_pageindex" int4, "_pagesize" int4, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_account_userinfo_dept_list('', '', '7f74192d-b937-403f-ac2a-8be34714278b', 1,1,-1);
--FETCH ALL FROM datacursor;
--FETCH ALL FROM pagecursor;

DECLARE
  
  _sql_where TEXT:='';
  _execute_sql TEXT;

  --分页标准参数
  _page_sql TEXT;
  _count_sql TEXT;
  _datacursor refcursor:= 'datacursor';
  _pagecursor refcursor:= 'pagecursor';
BEGIN
    

    IF _datasourcename IS NOT NULL AND _datasourcename<>'' THEN
         _sql_where:=_sql_where||' and  e.datasrcname ilike ''%' ||_datasourcename|| '%''';
    END IF;
    
    IF _status IS NOT NULL THEN
         _sql_where:=_sql_where||' and e.recstatus='||_status;
    ELSE
         _sql_where:=_sql_where||' and e.recstatus=1';
    END IF;

   _execute_sql:='
        select e.datasrcid as datasourceid,e.datasrcname as datasourcename,e.srctype,e.srcmark,e.recstatus,e.entityid,e.isrelatepower ,e.ispro ,f.entityname,e.datasourcename_lang
				from crm_sys_entity_datasource e left  outer join crm_sys_entity f on f.entityid = e.entityid  where 1=1'||_sql_where||' order by e.recupdated desc ';
		
    RAISE NOTICE '%',_execute_sql;

	SELECT * FROM crm_func_paging_sql_fetch(_execute_sql, _pageindex, _pagesize) INTO _page_sql,_count_sql;

  OPEN _datacursor FOR EXECUTE _page_sql;
	RETURN NEXT _datacursor;

  OPEN _pagecursor FOR EXECUTE _count_sql;
	RETURN NEXT _pagecursor;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;
CREATE OR REPLACE FUNCTION "public"."crm_func_datasource_add"("_datasourcename" varchar, "_srctype" int4, "_entityid" text, "_srcmark" varchar, "_isrelatepower" int4, "_status" int4, "_ispro" int4, "_userno" int4, "_datasourcename_lang" jsonb)
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
														"datasourcename_lang") 
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
														_datasourcename_lang) returning datasrcid into _datasrcid;
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
CREATE OR REPLACE FUNCTION "public"."crm_func_datasource_edit"("_datasourceid" text, "_datasourcename" text, "_srctype" int4, "_entityid" text, "_srcmark" text, "_status" int4, "_isrelatepower" int4, "_ispro" int4, "_userno" int4, "_datasourcename_lang" jsonb)
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
					datasourcename_lang=_datasourcename_lang
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


CREATE OR REPLACE FUNCTION "public"."crm_func_datasource_list"("_datasourcename" text, "_status" int4, "_pageindex" int4, "_pagesize" int4, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_account_userinfo_dept_list('', '', '7f74192d-b937-403f-ac2a-8be34714278b', 1,1,-1);
--FETCH ALL FROM datacursor;
--FETCH ALL FROM pagecursor;

DECLARE
  
  _sql_where TEXT:='';
  _execute_sql TEXT;

  --分页标准参数
  _page_sql TEXT;
  _count_sql TEXT;
  _datacursor refcursor:= 'datacursor';
  _pagecursor refcursor:= 'pagecursor';
BEGIN
    

    IF _datasourcename IS NOT NULL AND _datasourcename<>'' THEN
         _sql_where:=_sql_where||' and  e.datasrcname ilike ''%' ||_datasourcename|| '%''';
    END IF;
    
    IF _status IS NOT NULL THEN
         _sql_where:=_sql_where||' and e.recstatus='||_status;
    ELSE
         _sql_where:=_sql_where||' and e.recstatus=1';
    END IF;

   _execute_sql:='
        select e.datasrcid as datasourceid,e.datasrcname as datasourcename,e.srctype,e.srcmark,e.recstatus,e.entityid,e.isrelatepower ,e.ispro ,f.entityname,e.datasourcename_lang
				from crm_sys_entity_datasource e left  outer join crm_sys_entity f on f.entityid = e.entityid  where 1=1'||_sql_where||' order by e.recupdated desc ';
		
    RAISE NOTICE '%',_execute_sql;

	SELECT * FROM crm_func_paging_sql_fetch(_execute_sql, _pageindex, _pagesize) INTO _page_sql,_count_sql;

  OPEN _datacursor FOR EXECUTE _page_sql;
	RETURN NEXT _datacursor;

  OPEN _pagecursor FOR EXECUTE _count_sql;
	RETURN NEXT _pagecursor;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;


