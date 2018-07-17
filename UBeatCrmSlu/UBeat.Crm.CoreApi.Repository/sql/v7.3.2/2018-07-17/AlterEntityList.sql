CREATE OR REPLACE FUNCTION "public"."crm_func_entity_list"("_entityname" text, "_typeid" int4, "_status" int4, "_pageindex" int4, "_pagesize" int4, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_account_userinfo_dept_list('', '', '7f74192d-b937-403f-ac2a-8be34714278b', 1,1,-1);
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
    
    IF _entityname IS NOT NULL AND _entityname<>'' THEN
				_sql_where:=_sql_where||' AND (entity.entityname ilike ''%' || _entityname || '%'' or entity.entityname_lang::text ilike ''%' || _entityname || '%'')';
    END IF;

    IF _status IS NOT NULL THEN
       _sql_where:=_sql_where||' AND entity.recstatus=' ||_status;
    ELSE
       _sql_where:=_sql_where||' AND entity.recstatus=1';
    END IF;

     IF _typeid IS NOT NULL AND _typeid<>-1 THEN
       _sql_where:=_sql_where||' AND entity.modeltype=' ||_typeid;
    END IF;
   _execute_sql:='
        select  
				entity.entityid, 
				entity.entityname, 
				entity.recstatus,
        entity.entitytable,
        entity.relentityid,
        entity.relfieldid,
        entity.icons,
        case when (entity.relentityid is null or relentityid::text='''') then '''' else (select entityname from crm_sys_entity where entityid=entity.relentityid limit 1) end relentityname,
        entity.relaudit,
        entity.modeltype,
				case when entity.modeltype=0 then ''独立实体'' when entity.modeltype=1 then ''嵌套实体'' 
        when entity.modeltype=2 then ''应用实体'' when entity.modeltype=3 then ''动态实体'' else '''' end as entitytype, 
				entity.remark,
        (select flowid from crm_sys_workflow where entityid=entity.entityid order by reccreated desc limit 1) as flowid,
				entity.entityname_lang
				from crm_sys_entity entity  where 1=1' || _sql_where||' order by entity.reccreated desc ';

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
