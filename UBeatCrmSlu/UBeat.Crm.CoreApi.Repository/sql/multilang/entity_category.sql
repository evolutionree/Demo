alter table crm_sys_entity_category drop column categorylanguage;
alter table crm_sys_entity_category add categoryname_lang jsonb;
drop FUNCTION "public"."crm_func_entity_type_add"("_categoryname" varchar, "_entityid" varchar, "_userno" int4);
drop FUNCTION "public"."crm_func_entity_type_add"("_categoryname" varchar, "_entityid" varchar, "_userno" int4, "_categorylanguage" jsonb);
drop FUNCTION "public"."crm_func_entity_type_edit"("_categoryid" text, "_categoryname" text, "_userno" int4);
drop FUNCTION "public"."crm_func_entity_type_edit"("_entityid" text, "_categoryid" text, "_categoryname" text, "_userno" int4);
drop FUNCTION "public"."crm_func_entity_type_edit"("_entityid" text, "_categoryid" text, "_categoryname" text, "_userno" int4, "_categorylanguage" jsonb);
CREATE OR REPLACE FUNCTION "public"."crm_func_entity_type_list"("_entityid" text, "_status" int4, "_userno" int4)
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
  _reluuid TEXT ;

BEGIN
 

    IF _entityid IS NOT NULL AND _entityid<>'' THEN
			
				_sql_where:=_sql_where || ' and entityid='''||_entityid||'''::uuid';
    END IF;

    RAISE NOTICE '%',_sql_where;
   _execute_sql:='
                  select  entityid ,categoryname,categoryid,recstatus,relcategoryid,categoryname_lang from 
                  crm_sys_entity_category where recstatus<>2 ' || _sql_where||' order by recorder asc';

    RAISE NOTICE '%',_execute_sql;
 
  OPEN _datacursor FOR EXECUTE _execute_sql;
	RETURN NEXT _datacursor;
 

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;

CREATE OR REPLACE FUNCTION "public"."crm_func_entity_type_add"("_categoryname" varchar, "_entityid" varchar, "_userno" int4, "_categoryname_lang" jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE
  _orderby int4;  
  _categoryid uuid:=null;
  _r record;
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

BEGIN

   BEGIN

				 --数据逻辑
         IF _categoryname IS NULL OR _categoryname=''  THEN 
								Raise EXCEPTION '协议类型名称不能为空';
         END IF;

				 IF  EXISTS (select 1 from crm_sys_entity_category where categoryname=_categoryname AND entityid=_entityid::uuid LIMIT 1) THEN  
								Raise EXCEPTION '协议类型名称不能重复';
				 END IF;
         SELECT COALESCE((MAX(recorder)+1),0) into _orderby from crm_sys_entity_category where entityid=_entityid::uuid;
         INSERT INTO crm_sys_entity_category
         (
         "categoryname",
         "entityid",
         "recorder",
         "reccreator",
         "recupdator",
         "recstatus",
				 "categoryname_lang"
         ) values 
         (
          _categoryname,
          _entityid::uuid,
          _orderby,
          _userno,
          _userno,
          1,
				_categoryname_lang
         ) returning categoryid into _categoryid;
         INSERT INTO crm_sys_entity_field_rules 
                     (typeid,fieldid, operatetype, isrequire, isvisible, isreadonly, viewrules,validrules, relaterules, recorder, reccreator, recupdator)
                      SELECT _categoryid,fieldid,operatetype, isrequire, isvisible, isreadonly, viewrules,validrules, relaterules, recorder, _userno, _userno
                      FROM crm_sys_entity_field_rules WHERE typeid=_entityid::uuid;

 
				  INSERT INTO crm_sys_entity_func_event(typeid,operatetype,funcname) SELECT _categoryid,operatetype,funcname FROM crm_sys_entity_func_event  WHERE typeid=_entityid::uuid;
					_codeid:= _categoryid::TEXT;
					_codeflag:= 1;
					_codemsg:= '新增协议类型成功';
	 EXCEPTION WHEN OTHERS THEN
						 GET STACKED DIAGNOSTICS _codestack = PG_EXCEPTION_CONTEXT;
						 _codemsg:=SQLERRM;
						 _codestatus:=SQLSTATE;
		END;
   
   		--RETURN RESULT
	  RETURN QUERY EXECUTE format('SELECT $1,$2,$3,$4,$5')
	  USING  _codeid,_codeflag,_codemsg,_codestack,_codestatus;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;


CREATE OR REPLACE FUNCTION "public"."crm_func_entity_type_edit"("_entityid" text, "_categoryid" text, "_categoryname" text, "_userno" int4, "_categoryname_lang" jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE
  _orderby int4;  
  
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

BEGIN

   BEGIN

				 --数据逻辑
         IF _categoryname IS NULL OR _categoryname=''  THEN 
								Raise EXCEPTION '协议类型名称不能为空';
         END IF;

				 IF  EXISTS (select 1 from crm_sys_entity_category where entityid=_entityid::uuid AND categoryid<>_categoryid::uuid and categoryname=_categoryname LIMIT 1) THEN  
								Raise EXCEPTION '协议类型名称不能重复';
				 END IF;
 
         UPDATE crm_sys_entity_category
         SET categoryname=_categoryname,recupdator=_userno,categoryname_lang=_categoryname_lang
         WHERE categoryid=_categoryid::uuid;
 
					_codeid:= _categoryid::TEXT;
					_codeflag:= 1;
					_codemsg:= '更新协议类型成功';
	 EXCEPTION WHEN OTHERS THEN
						 GET STACKED DIAGNOSTICS _codestack = PG_EXCEPTION_CONTEXT;
						 _codemsg:=SQLERRM;
						 _codestatus:=SQLSTATE;
		END;
   
   		--RETURN RESULT
	  RETURN QUERY EXECUTE format('SELECT $1,$2,$3,$4,$5')
	  USING  _codeid,_codeflag,_codemsg,_codestack,_codestatus;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;


