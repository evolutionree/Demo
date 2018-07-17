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
        select e.datasrcid as datasourceid,e.datasrcname as datasourcename,e.srctype,e.srcmark,e.recstatus,e.entityid,e.isrelatepower ,e.ispro ,f.entityname,e.datasourcelanguage
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

