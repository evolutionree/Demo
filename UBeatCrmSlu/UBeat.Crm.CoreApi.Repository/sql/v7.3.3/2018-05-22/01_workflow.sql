
/***
工作流增加标题查找
***/
alter table crm_sys_workflow add titleconfig text null;
alter table crm_sys_workflow_case add title text null ;
update crm_sys_workflow_case a set title = (select flowname from crm_sys_workflow  where flowid = a.flowid )
where a.title is null ;
INSERT INTO "public"."crm_sys_entity_fields" ( "fieldname", "entityid", "fieldlabel", "displayname", "controltype", "fieldtype", "fieldconfig", "recorder", "recstatus", "reccreator", "recupdator", "reccreated", "recupdated",  "expandjs", "filterjs") 
VALUES ('title', '00000000-0000-0000-0000-000000000001', '审批主题', '审批主题', '1', '0', '{}', '0', '1', '14', '14', now(), now(), '', '');

CREATE OR REPLACE FUNCTION "public"."crm_func_workflow_detail"("_flowid" uuid, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_account_userinfo_contact_list(0,1, 10,1);
--FETCH ALL FROM datacursor;
--FETCH ALL FROM pagecursor;
--FETCH ALL FROM versioncursor;

DECLARE
  _condition_sql TEXT;

  _execute_sql TEXT;
  _condition_array TEXT[];
  _datacursor refcursor:= 'datacursor';

BEGIN

   IF _flowid IS NULL THEN
         Raise EXCEPTION '流程ID不能为空';
   END IF;
    
   _execute_sql:='
		SELECT w.flowid,w.flowname,w.flowtype,w.backflag,w.resetflag,w.expireday,w.remark,
		w.entityid,w.vernum,w.skipflag,e.entityname ,w.titleconfig
		FROM crm_sys_workflow AS w
		LEFT JOIN crm_sys_entity AS e ON w.entityid = e.entityid
    WHERE flowid = $1
    ';

    RAISE NOTICE '%',_execute_sql;

  OPEN _datacursor FOR EXECUTE _execute_sql USING _flowid;
	RETURN NEXT _datacursor;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;