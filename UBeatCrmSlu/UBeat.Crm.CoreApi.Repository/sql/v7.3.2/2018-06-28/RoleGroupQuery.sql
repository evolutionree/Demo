CREATE OR REPLACE FUNCTION "public"."crm_func_role_group_list"("_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
 

DECLARE
  
  _sql_where TEXT:='';
  _execute_sql TEXT;

  _datacursor refcursor:= 'datacursor';
 
BEGIN
 
   _execute_sql:='
        select  
				g.rolegroupid, 
				g.rolegroupname, 
				g.grouptype,
g.grouplanguage
        from crm_sys_role_group g where recstatus=1;';

    RAISE NOTICE '%',_execute_sql;
 
  OPEN _datacursor FOR EXECUTE _execute_sql;
	RETURN NEXT _datacursor;

 

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;

