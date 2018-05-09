/***
修改日期：2018-05-09
修改人：钟冠辉
修改内容：
	解决keyword参数不生效问题
	截止目前改为：
		keyword对所有的可显示字段（数据源配置）生效
***/
CREATE OR REPLACE FUNCTION "public"."crm_func_business_ds_list"("_datasrckey" text, "_keyword" text, "_sqlwhere" text, "_pageindex" int4, "_pagesize" int4, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_account_userinfo_dept_list('', '', '7f74192d-b937-403f-ac2a-8be34714278b', 1,1,-1);
--FETCH ALL FROM datacursor;
--FETCH ALL FROM pagecursor;
--    _field_sql:='select fieldkeys from crm_sys_entity_datasource_config where datasrcid='''||_datasrckey||'''';
--    EXECUTE _field_sql INTO _fieldkeys;
-- 
--  
--    SELECT array_length(
-- 	 ARRAY(SELECT UNNEST(string_to_array((_fieldkeys),',')))::text[],1) INTO _count;
--                 raise notice '%',_count;
--    IF _keyword IS NOT NULL AND _keyword<>'' THEN
--        _isappend=TRUE;
--    END IF;
-- 
--    FOR r1 IN SELECT UNNEST(string_to_array((_fieldkeys),',')) as field loop
--           _num=_num+1;
--           IF _isappend THEN
--              IF _count=_num THEN
--                 _where_sql:=_where_sql||'t.'||r1.field||' ilike ''%'||_keyword||'%'' ';
--              ELSE 
--                 _where_sql:=_where_sql||'t.'||r1.field||' ilike ''%'||_keyword||'%'' or ';
--                 raise notice '%',_where_sql;
--              END IF;
--        END IF;
--    END loop;
--    IF _where_sql<>'' THEN
-- 	    _where_sql:=format(' and ( %s )',_where_sql);
--    END IF;btrim(_rulesql,';'),
DECLARE
  r1 record;
  _rulesql TEXT;
  _config_sql TEXT;
  _execute_sql TEXT;
  _needpower int4;
  _ispro int4;
  _entityid uuid;
  _fieldkeys TEXT:='';
--   _fieldkeys TEXT;
--   _field_sql TEXT;

--   _count int4:=0;
--   _num int4:=0;
--   _isappend BOOLEAN:=FALSE;
  _role_power_sql TEXT;
  _rule_power_sql TEXT:='';
  _field_sql TEXT:='';
  _entitytable TEXT;
  _datacursor refcursor:= 'datacursor';
  _configcursor refcursor:= 'configcursor';
 
_keyword_where_sql text;

  _page_sql TEXT;
  _count_sql TEXT;
	_field_keys TEXT;
	_field_keys_r refcursor;
_fieldname text;
  _pagecursor refcursor:= 'pagecursor';
BEGIN
    
   _execute_sql:='
									select rulesql,entityid,isrelatepower,ispro from crm_sys_entity_datasource where datasrcid='''||_datasrckey||'''';
   _config_sql:='
									select datasrcid, viewstyleid,fieldkeys,fonts,colors from crm_sys_entity_datasource_config where datasrcid='''||_datasrckey||'''';
    RAISE NOTICE '%',_execute_sql;
		_field_sql:='
									select  fieldkeys from crm_sys_entity_datasource_config where datasrcid='''||_datasrckey||'''';
    
   EXECUTE _execute_sql INTO _rulesql,_entityid,_needpower,_ispro;
   
   IF _entityid IS NULL AND _needpower=1 THEN
      Raise EXCEPTION '%','该实体Id不能为空';
   END IF;


   IF (_rulesql IS NULL OR _rulesql='')  THEN
      Raise EXCEPTION '数据源sql不能为空';
   ELSE
      _rulesql:=replace(_rulesql,'{currentUser}',_userno::VARCHAR);
   END IF;
	EXECUTE _field_sql INTO _field_keys;
	_keyword_where_sql:=' 1<> 1 ';
	if _keyword is null THEN
		_keyword :='';	
	end if;
		_keyword := replace(_keyword,'''','''''');
		if _field_keys is not null THEN
			open _field_keys_r for EXECUTE 'select UNNEST(string_to_array('''|| _field_keys ||''', '','')) fieldname  ' ;
				loop
					fetch _field_keys_r into _fieldname;  
					if found then 
						_keyword_where_sql:=_keyword_where_sql|| ' or  t.'|| _fieldname|| '::text like ''%' || _keyword|| '%'' ';
					else 
						exit;
					end if;
			end loop;
			close _field_keys_r;
		end if;
	 _sqlwhere:=COALESCE(_sqlwhere,'');
		--raise notice '%',_keyword_where_sql;
	 IF _ispro=0 THEN
			 IF _needpower=1 THEN 
					_rulesql:=replace(_rulesql,'{needpower}',_needpower::text);
			 ELSE
					_rulesql:=replace(_rulesql,'{needpower}','0');
			 END IF;
			 _rulesql:=replace(_rulesql,'{querydata}',quote_literal(_sqlwhere));
	     _rulesql:=format('select * from ( %s ) as t  where %s',_rulesql,_keyword_where_sql);
   ELSE
       IF _needpower=1 THEN
				 _role_power_sql:=crm_func_role_rule_fetch_sql(_entityid::uuid,_userno);
				 _rule_power_sql:=crm_func_role_rule_param_format(_role_power_sql,_userno);
				 _rulesql:=replace(_rulesql,'{querydata}',_sqlwhere);
				 _rulesql:=format('select t.* from ( %s ) as t inner join (%s) as e on t.id=e.recid where %s',_rulesql,_rule_power_sql,_keyword_where_sql);
       ELSE
					 _rulesql:=replace(_rulesql,'{querydata}',_sqlwhere);
					 _rulesql:=format('select * from ( %s ) as t where %s ',_rulesql,_keyword_where_sql);
       END IF;

	 END IF;
 raise notice '%',_rulesql;
 
	IF _pageindex IS NOT NULL AND _pageindex=1 THEN
			OPEN _configcursor FOR EXECUTE _config_sql;
			RETURN NEXT _configcursor;
  END IF;
			SELECT * FROM crm_func_paging_sql_fetch(_rulesql, _pageindex, _pagesize) INTO _page_sql,_count_sql;

			OPEN _datacursor FOR EXECUTE _page_sql;
			RETURN NEXT _datacursor;
			OPEN _pagecursor FOR EXECUTE _count_sql;
			RETURN NEXT _pagecursor;
END

$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;


