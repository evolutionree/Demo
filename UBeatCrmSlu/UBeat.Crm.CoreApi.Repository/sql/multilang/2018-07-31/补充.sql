CREATE OR REPLACE FUNCTION "public"."crm_func_menu_rule_list"("_entityid" text, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_attendance_list(0,0, '',1,10, 5);
--FETCH ALL FROM datacursor;
--FETCH ALL FROM pagecursor;

DECLARE
  
  _sql_where TEXT:='';
  _execute_sql TEXT;
 
  _datacursor refcursor:= 'datacursor';
 

BEGIN
 
   IF _entityid<>'' OR _entityid IS NOT NULL THEN
         _sql_where:=' and me.entityid='''||_entityid||'''::uuid';
   END IF;
   _execute_sql:='select me.menuname,me.recorder,me.menuid,me.menuname_lang from crm_sys_entity_menu me LEFT JOIN  crm_sys_rule r on me.ruleid=r.ruleid where me.recstatus=1'||_sql_where||' order by me.recorder desc;';

    RAISE NOTICE '%',_execute_sql;

  
  OPEN _datacursor FOR EXECUTE _execute_sql ;
	RETURN NEXT _datacursor;
 

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;

-------------------------------------


ALTER TABLE crm_sys_entity_fields rename fieldname_lang to fieldlabel_lang;

--------------------------

ALTER TABLE crm_sys_role rename rolelanguage to rolename_lang;