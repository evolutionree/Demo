CREATE OR REPLACE FUNCTION "public"."crm_func_datasourcedetail_list"("_datasrcid" text, "_userno" int4)
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
    

   _execute_sql:='
                  SELECT 
                  ds.datasrcid,
                  fig.dataconfid,
                  ds.datasrcname,
                  ds.srctype,
                  ds.srcmark,
                  ds.rulesql,
                  ds.recstatus,
                  ds.ispro,
                  fig.viewstyleid,
                  fig.fieldkeys,
                  fig.fonts,
                  fig.colors,
									ds.datasourcename_lang
                  FROM crm_sys_entity_datasource ds 
                  LEFT JOIN crm_sys_entity_datasource_config fig on ds.datasrcid=fig.datasrcid
                  WHERE ds.datasrcid='''||_datasrcid||'''::uuid;';

    RAISE NOTICE '%',_execute_sql;


  OPEN _datacursor FOR EXECUTE _execute_sql;
	RETURN NEXT _datacursor;



END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;