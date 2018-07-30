CREATE OR REPLACE FUNCTION "public"."crm_func_field_filter_list"("_entityid" text, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_account_userinfo_dept_list('', '', '7f74192d-b937-403f-ac2a-8be34714278b', 1,1,-1);
--FETCH ALL FROM datacursor;
--FETCH ALL FROM pagecursor;

DECLARE
  
 
  _fields_sql TEXT;
  _fieldssearch_sql TEXT;
  _simpl_sql TEXT;
  --分页标准参数
  _page_sql TEXT;
  _count_sql TEXT;
  _fieldscursor refcursor:= 'fieldscursor';
  _searchcursor refcursor:= 'searchcursor';
  _simplecursor refcursor:='simplecursor';
BEGIN
    
 
   _fields_sql:='select fields.fieldlabel,fields.fieldid,fields.controltype,fields.displayname,fields.fieldlabel_lang,fields.displayname_lang from crm_sys_entity_fields fields where fields.controltype not in (2, 15, 20, 22, 23, 24) and  fields.entityid='''||_entityid||'''::uuid and fields.recstatus=1;';

   _fieldssearch_sql='select fields.fieldlabel,fields.fieldid,sea.controltype,fields.displayname,sea.islike,fields.fieldlabel_lang,fields.displayname_lang from crm_sys_entity_search sea LEFT JOIN  crm_sys_entity_fields fields
                  on fields.fieldid = sea.fieldid where  sea.entityid='''||_entityid||'''::uuid and sea is not null 
                   and sea.recstatus=1 and fields.recstatus=1 order by sea.recorder asc;';
   _simpl_sql:='select fields.fieldlabel,fields.fieldname,fields.fieldid,fields.controltype,fields.displayname,fields.fieldlabel_lang,fields.displayname_lang from crm_sys_entity_listview_search sea left join crm_sys_entity_fields fields on sea.searchfield=fields.fieldid   where fields.controltype not in (2, 15, 20, 22, 23, 24) and  sea.entityid='''||_entityid||'''::uuid and fields.recstatus=1 and sea.viewtype=0;';
    RAISE NOTICE '%',_fields_sql;

     RAISE NOTICE '%',_fieldssearch_sql;

  OPEN _fieldscursor FOR EXECUTE _fields_sql;
	RETURN NEXT _fieldscursor;

   OPEN _searchcursor FOR EXECUTE _fieldssearch_sql;
	RETURN NEXT _searchcursor;

   OPEN _simplecursor FOR EXECUTE _simpl_sql;
	RETURN NEXT _simplecursor;
END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;

