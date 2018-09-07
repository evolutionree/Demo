CREATE OR REPLACE FUNCTION "public"."crm_func_reminder_select"("_rectype" int4, "_recstatus" int4, "_remindername" text, "_userno" int4, "_pageindex" int4, "_pagesize" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
DECLARE

  _sql TEXT:='';



  --分页标准参数
  _page_sql TEXT;
  _count_sql TEXT;
  _datacursor refcursor:= 'datacursor';
  _pagecursor refcursor:= 'pagecursor';

BEGIN

	/*
		select * from crm_func_reminder_select(0,1,'',118,1,10);
		fetch all from datacursor;
		fetch all from pagecursor;
		select entityid,repeattype,* from crm_sys_reminder
	*/


     _sql:='    SELECT  r.recid as  reminderid,
			r.remindername,
			r.entityid,
			e.entityname,
			r.isrepeat,
			r.repeattype,
			r.cronstring,
			r.recstatus,
			r.remark,
			r.reminderlanguage
			
                FROM crm_sys_reminder r
                INNER JOIN crm_sys_entity e on r.entityid=e.entityid
		WHERE r.rectype='||_rectype||'
		AND   r.recstatus='||_recstatus||'
		AND   r.remindername LIKE ''%'||_remindername||'%''
		ORDER BY r.recupdated desc ';

		

	
	       

	--查询分页数据
	SELECT * FROM crm_func_paging_sql_fetch(_sql, _pageindex, _pagesize) INTO _page_sql,_count_sql;

	Raise Notice 'the _page_sql:%',_page_sql;
        Raise Notice 'the _count_sql:%',_count_sql;

	OPEN _datacursor FOR EXECUTE _page_sql;
	RETURN NEXT _datacursor;

	OPEN _pagecursor FOR EXECUTE _count_sql;
	RETURN NEXT _pagecursor;

	
END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;

