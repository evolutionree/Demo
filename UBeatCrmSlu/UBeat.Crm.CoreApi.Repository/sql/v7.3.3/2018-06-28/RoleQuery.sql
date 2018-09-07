CREATE OR REPLACE FUNCTION "public"."crm_func_role_list"("_groupid" text, "_rolename" text, "_pageindex" int4, "_pagesize" int4, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_attendance_list(0,0, '',1,10, 5);
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
 

   IF _groupid IS NOT NULL AND _groupid<>'' THEN
          _sql_where:=' and gr.rolegroupid= '''||_groupid||'''::uuid';   
   END IF;

   IF _rolename IS NOT NULL AND _rolename<>'' THEN
          _sql_where:=_sql_where||' and r.rolename ilike ''%'||_rolename||'%''';
   END IF;
 
   _execute_sql:='
        select  
				r.roleid, 
				r.rolename, 
				case when r.roletype=0 then ''系统角色'' when r.roletype=1 then ''自定义角色'' else '''' end roletypename, 
        
        r.roletype,
        r.rolepriority,
        r.roleremark,
        r.recorder,
        gr.rolegroupname,
        gr.rolegroupid,r.rolelanguage
        from crm_sys_role r left join crm_sys_role_group_relate re on r.roleid=re.roleid 
        left join crm_sys_role_group gr  on gr.rolegroupid=re.rolegroupid where r.recstatus=1 '||_sql_where||' order by r.recupdated desc';

    RAISE NOTICE '%',_execute_sql;

   	--查询分页数据
	SELECT * FROM crm_func_paging_sql_fetch(_execute_sql, _pageindex, _pagesize) INTO _page_sql,_count_sql;

  OPEN _datacursor FOR EXECUTE _page_sql  ;
	RETURN NEXT _datacursor;

	OPEN _pagecursor FOR EXECUTE _count_sql  ;
	RETURN NEXT _pagecursor;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;
