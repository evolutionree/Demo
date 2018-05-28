CREATE OR REPLACE FUNCTION "public"."crm_func_return_rel_tab_sql"("_relid" uuid, "_recid" uuid, "_userno" int4)
  RETURNS "pg_catalog"."text" AS $BODY$
--SELECT crm_func_attendance_list(0,0, '',1,10, 5);
--FETCH ALL FROM datacursor;
--FETCH ALL FROM pagecursor;

DECLARE
  
  _relentityid uuid:=NULL;
  _entitytable TEXT:='';
  _fieldid uuid:=NULL;
  _fieldname TEXT;
  _sql_where TEXT;
  _ismanytomany int4;
	_tabtype int4;
  _execute_sql TEXT;
BEGIN
	
   SELECT relentityid,fieldid,ismanytomany,tabtype INTO _relentityid,_fieldid,_ismanytomany,_tabtype FROM crm_sys_entity_rel_tab WHERE relid=_relid LIMIT 1;
   IF _ismanytomany=1 THEN
		 _execute_sql:=format('and recid in ( 
																 select t.recid from crm_sys_entity_datasrc_relation AS t where t.entityid=''%s'' AND t.fieldid=''%s'' AND
																 t.datasrcrecid=''%s''
																)   ',_relentityid,_fieldid,_recid);
   ELSE
			 if _tabtype = 10 then 
					_execute_sql:='and recid =''' || _recid || '''::uuid';
			 ELSE
				 SELECT entitytable INTO _entitytable FROM crm_sys_entity WHERE entityid=_relentityid;
				 _execute_sql:=format('select fieldname from crm_sys_entity_fields where entityid=''%s''::uuid and fieldid=''%s''::uuid',_relentityid,_fieldid);
				 EXECUTE _execute_sql INTO _fieldname;
				 _execute_sql:=format('and recid in ( 
																 select t.recid from (
																		 select recid,regexp_split_to_table((%s->>''id''),'','')::uuid as relrecid from %s where COALESCE((%s->>''id''),'''')<>''''
																			 ) as t where t.relrecid=''%s''
																		)',_fieldname,_entitytable,_fieldname,_recid);
			end if;
   END IF;
   RETURN _execute_sql;
 
END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
;



CREATE OR REPLACE FUNCTION "public"."crm_func_business_ds_detail"("_datasrckey" text, "_recid" text, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
DECLARE
  _rulesql TEXT;
  _execute_sql TEXT;
  _needpower int4;
  _ispro int4;
  _entityid uuid;
  _fieldkeys TEXT:='';
  _role_power_sql TEXT;
  _rule_power_sql TEXT:='';
  _field_sql TEXT:='';
  _entitytable TEXT;
  _datacursor refcursor:= 'datacursor';
	_field_keys TEXT;
_fieldname text;
BEGIN
    
   _execute_sql:='
									select rulesql,entityid,isrelatepower,ispro from crm_sys_entity_datasource where datasrcid='''||_datasrckey||'''';
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
		--raise notice '%',_keyword_where_sql;
	 IF _ispro=0 THEN
				_rulesql:=replace(_rulesql,'{needpower}','0');
			 _rulesql:=replace(_rulesql,'{querydata}','''''');
	     _rulesql:=format('select * from ( %s ) as t  where t.id::text = ''%s''',_rulesql,_recid);
   ELSE
			 _rulesql:=replace(_rulesql,'{querydata}','''''');
			 _rulesql:=format('select * from ( %s ) as t where  t.id::text = ''%s'' ',_rulesql,_recid);

	 END IF;
 raise notice '%',_rulesql;
 
			OPEN _datacursor FOR EXECUTE _rulesql;
			RETURN NEXT _datacursor;
END

$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;
