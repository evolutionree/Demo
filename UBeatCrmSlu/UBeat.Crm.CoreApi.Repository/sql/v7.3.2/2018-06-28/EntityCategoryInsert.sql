CREATE OR REPLACE FUNCTION "public"."crm_func_entity_type_add"("_categoryname" varchar, "_entityid" varchar, "_userno" int4,_categorylanguage jsonb)
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
categorylanguage
         ) values 
         (
          _categoryname,
          _entityid::uuid,
          _orderby,
          _userno,
          _userno,
          1,
_categorylanguage
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

