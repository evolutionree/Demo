CREATE OR REPLACE FUNCTION "public"."crm_func_workflow_list"("_flowstatus" int4, "_searchname" text, "_pageindex" int4, "_pagesize" int4, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_account_userinfo_contact_list(0,1, 10,1);
--FETCH ALL FROM datacursor;
--FETCH ALL FROM pagecursor;
--FETCH ALL FROM versioncursor;

DECLARE
  _condition_sql TEXT:='';

  _execute_sql TEXT;
  _condition_array TEXT[];

  --分页标准参数
  _page_sql TEXT;
  _count_sql TEXT;
  _datacursor refcursor:= 'datacursor';
  _pagecursor refcursor:= 'pagecursor';

BEGIN

   IF _flowstatus IS NOT NULL AND _flowstatus <>-1 THEN
						_condition_array:=array_append(_condition_array,'w.recstatus = ''' || _flowstatus || ''' ');
   END IF;

   IF _searchname IS NOT NULL AND _searchname !='' THEN
           Raise Notice 'Enter searchname %',_searchname;
						_condition_array:=array_append(_condition_array,'w.flowname ilike ''%' || _searchname || '%'' ');
   END IF;

    IF array_length(_condition_array, 1) > 0 THEN
          _condition_sql:=_condition_sql || format(' AND (%s)',array_to_string(_condition_array, ' AND '));
    END IF;
    Raise Notice '_condition_sql,%',_condition_sql;

   _execute_sql:=format('
		SELECT w.flowid,w.flowname,w.flowtype,w.backflag,w.resetflag,w.expireday,w.remark,
		w.entityid,w.vernum,w.skipflag,e.entityname,w.recstatus,e.modeltype AS entitymodeltype,e.relentityid,re.modeltype AS relentitymodeltype,
		w.flowlanguage
		FROM crm_sys_workflow AS w
		LEFT JOIN crm_sys_entity AS e ON w.entityid = e.entityid
		LEFT JOIN crm_sys_entity AS re ON re.entityid = e.relentityid
    WHERE 1=1
    %s
    ORDER BY w.reccreated DESC
    ',_condition_sql);

    RAISE NOTICE '%',_execute_sql;

  --查询分页数据
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
