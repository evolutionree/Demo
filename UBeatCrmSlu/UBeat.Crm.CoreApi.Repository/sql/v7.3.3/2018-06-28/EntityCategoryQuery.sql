CREATE OR REPLACE FUNCTION "public"."crm_func_entity_type_list"("_entityid" text, "_status" int4, "_userno" int4)
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
  _reluuid TEXT ;

BEGIN
 

    IF _entityid IS NOT NULL AND _entityid<>'' THEN
			
				_sql_where:=_sql_where || ' and entityid='''||_entityid||'''::uuid';
    END IF;

    RAISE NOTICE '%',_sql_where;
   _execute_sql:='
                  select  entityid ,categoryname,categoryid,recstatus,relcategoryid,categorylanguage from 
                  crm_sys_entity_category where recstatus<>2 ' || _sql_where||' order by recorder asc';

    RAISE NOTICE '%',_execute_sql;
 
  OPEN _datacursor FOR EXECUTE _execute_sql;
	RETURN NEXT _datacursor;
 

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;
