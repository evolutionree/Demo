alter table crm_sys_webmenu add name_lang jsonb;
alter table crm_sys_reportfolder add name_lang jsonb;



-------------------------


CREATE OR REPLACE FUNCTION "public"."crm_func_menu_rule_info"("_menuid" text, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_attendance_list(0,0, '',1,10, 5);
--FETCH ALL FROM datacursor;
--FETCH ALL FROM pagecursor;

DECLARE
  
  _sql_where TEXT:='';
  _execute_sql TEXT;
 
 
  _datacursor refcursor:= 'datacursor';
 

BEGIN
 
    IF _menuid='' OR _menuid IS NULL THEN
         Raise EXCEPTION '菜单Id不能为空';
    END IF;

    _sql_where:=' and me.menuid='''||_menuid||'''::uuid';
 
   _execute_sql:='SELECT me.menuname, r.ruleid::text,r.rulename,r.entityid::text,r.recstatus,
   i.rulesql,i.itemid::text,i.fieldid::text,i.itemname,i.operate,i.usetype,i.ruletype,i.ruledata,s.ruleset,me.menuname_lang
   FROM crm_sys_entity_menu me 
   left join  crm_sys_rule r  on me.ruleid=r.ruleid
   INNER JOIN crm_sys_rule_item_relation ir ON r.ruleid=ir.ruleid 
   INNER JOIN crm_sys_rule_item  i on i.itemid=ir.itemid
   INNER JOIN crm_sys_rule_set  s on s.ruleid=r.ruleid where r.recstatus=1'||_sql_where;

    RAISE NOTICE '%',_execute_sql;
 
  OPEN _datacursor FOR EXECUTE _execute_sql  ;
	RETURN NEXT _datacursor;
 

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;

