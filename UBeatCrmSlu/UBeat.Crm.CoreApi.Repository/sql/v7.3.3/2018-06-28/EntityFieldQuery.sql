CREATE OR REPLACE FUNCTION "public"."crm_func_entity_field_list"("_entityid" text, "_status" int4, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$

/*
SELECT crm_func_entity_field_list('157ec951-664c-4dfa-8f1d-a4173e5513f5', 2, 11);
FETCH ALL FROM datacursor;
FETCH ALL FROM pagecursor;
*/

DECLARE
  
  _sql_where TEXT:='';
  _execute_sql TEXT; 
  

  --分页标准参数
  _page_sql TEXT;
  _count_sql TEXT;
  _datacursor refcursor:= 'datacursor';

BEGIN

   _execute_sql:='
        select row_number() over(ORDER BY recorder asc),fieldid::varchar,fieldname,entityid::varchar,fieldlabel,displayname,controltype,fieldtype,
        fieldconfig,recstatus,expandjs,filterjs,fieldlanguage,displaylanguage from crm_sys_entity_fields 
        where fieldname <> ''recid'' and  entityid='''||_entityid||'''::uuid %s order by recorder asc';
   --经过和测试讨论 客户基础资料需特殊处理 隐藏字段
   IF EXISTS(SELECT 1 FROM crm_sys_entity_special_table WHERE entityid=_entityid::uuid) THEN

      _execute_sql:=format(_execute_sql,' and recstatus=1');
   ELSE
      _execute_sql:=format(_execute_sql,' and recstatus<>'||_status);
   END IF;
 
  OPEN _datacursor FOR EXECUTE _execute_sql;
	RETURN NEXT _datacursor;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;