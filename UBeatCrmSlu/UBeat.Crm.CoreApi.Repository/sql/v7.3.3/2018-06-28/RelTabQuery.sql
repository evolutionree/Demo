CREATE OR REPLACE FUNCTION "public"."crm_func_rel_tab_list"("_entityid" uuid, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_account_userinfo_dept_list('', '', '7f74192d-b937-403f-ac2a-8be34714278b', 1,1,-1);
--FETCH ALL FROM datacursor;
--FETCH ALL FROM pagecursor;

DECLARE
  
  _sql_where TEXT:='';
  _execute_sql TEXT;

 
  _datacursor refcursor:= 'datacursor';
 

BEGIN
    
   IF NOT EXISTS(SELECT 1 FROM crm_sys_entity WHERE entityid=_entityid) THEN
       Raise EXCEPTION '%','该实体不存在';
   END IF;
   _sql_where:=' and ta.entityid='''||_entityid||'''';
   _execute_sql:=format('select ta.relid,fi.fieldid,fi.fieldname ,ta.relentityid,ta.relname,en.entityname,fi.fieldlabel,ta.recstatus,ta.entitytaburl,ta.recorder,ta.tabtype,ta.icon,ta.web,ta.mob,ta.ismanytomany,ta.srcsql,ta.srctitle,(select count(1) from crm_sys_entity_rel_config co where co.relid=ta.relid )::int confitems,ta.reltablanguage from crm_sys_entity_rel_tab ta 
                         left join crm_sys_entity en on ta.relentityid=en.entityid 
                         left join crm_sys_entity_fields fi on fi.fieldid=ta.fieldid and fi.recstatus=1 
												 where 1=1 and ta.recstatus=1 %s order by ta.recorder',_sql_where);

    RAISE NOTICE '%',_execute_sql;

 
	OPEN _datacursor FOR EXECUTE _execute_sql;
	RETURN NEXT _datacursor;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;
