CREATE OR REPLACE FUNCTION "public"."crm_func_entity_info"("_entityid" text, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_attendance_list(0,0, '',1,10, 5);
--FETCH ALL FROM datacursor;
--FETCH ALL FROM pagecursor;

DECLARE
  
  _sql_where TEXT:='';
  _execute_sql TEXT;
 
 
  _datacursor refcursor:= 'datacursor';
 

BEGIN
 
    IF _entityid='' OR _entityid IS NULL THEN
         Raise EXCEPTION 'ʵ��Id����Ϊ��';
    END IF;

    _sql_where:=' and entityid='''||_entityid||'''::uuid';
 
   _execute_sql:=' select entityid,
                          entityname,
                          modeltype,
                          icons,
                          relentityid,newload,editload,checkload,inputmethod,entityname_lang from crm_sys_entity
                          where recstatus=1'||_sql_where;

    RAISE NOTICE '%',_execute_sql;
 
  OPEN _datacursor FOR EXECUTE _execute_sql  ;
	RETURN NEXT _datacursor;
 

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;
