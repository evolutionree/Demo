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
_entityid uuid:=NULL;
BEGIN
	
   SELECT relentityid,fieldid,ismanytomany,tabtype,entityid INTO _relentityid,_fieldid,_ismanytomany,_tabtype,_entityid FROM crm_sys_entity_rel_tab WHERE relid=_relid LIMIT 1;
   IF _ismanytomany=1 THEN
		 _execute_sql:=format('and recid in ( 
																 select t.recid from crm_sys_entity_datasrc_relation AS t where t.entityid=''%s'' AND t.fieldid=''%s'' AND
																 t.datasrcrecid=''%s''
																)   ',_relentityid,_fieldid,_recid);
   ELSE
			 if _tabtype = 10 then 
					_execute_sql:='and (recid =''' || _recid || '''::uuid or caseid  in (select caseid from  crm_sys_workflow_case_entity_relation  where relrecid ='''|| _recid ||''' )) ';
			 ELSE
				 SELECT entitytable INTO _entitytable FROM crm_sys_entity WHERE entityid=_relentityid;
				 _execute_sql:=format('select fieldname from crm_sys_entity_fields where entityid=''%s''::uuid and fieldid=''%s''::uuid',_relentityid,_fieldid);
				 EXECUTE _execute_sql INTO _fieldname;
				 if('f9db9d79-e94b-4678-a5cc-aa6e281c1246' = _entityid::text) 
						and exists(
										select entityid 
										from crm_sys_entity_datasource 
										where datasrcid::text  in (select (fieldconfig->>'dataSource')::jsonb->>'sourceId' 
																								from crm_sys_entity_fields where fieldid = _fieldid)	
													and entityid = 'ac051b46-7a20-4848-9072-3b108f1de9b0')
						then 
						
						_execute_sql:=format('and recid in ( 
																 select t.recid from (
																		 select recid,regexp_split_to_table((%s->>''id''),'','')::uuid as relrecid from %s where COALESCE((%s->>''id''),'''')<>''''
																			 ) as t where t.relrecid in (select commonid  from crm_sys_custcommon_customer_relate where custid =  ''%s'')
																		)',_fieldname,_entitytable,_fieldname,_recid);
					else 
						_execute_sql:=format('and recid in ( 
																 select t.recid from (
																		 select recid,regexp_split_to_table((%s->>''id''),'','')::uuid as relrecid from %s where COALESCE((%s->>''id''),'''')<>''''
																			 ) as t where t.relrecid=''%s''
																		)',_fieldname,_entitytable,_fieldname,_recid);
				end if;
			end if;
   END IF;
   RETURN _execute_sql;
 
END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
;
