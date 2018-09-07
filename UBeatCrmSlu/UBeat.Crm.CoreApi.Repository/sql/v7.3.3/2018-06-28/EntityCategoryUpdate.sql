CREATE OR REPLACE FUNCTION "public"."crm_func_entity_type_edit"("_entityid" text, "_categoryid" text, "_categoryname" text, "_userno" int4 ,_categorylanguage jsonb)
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
         SET categoryname=_categoryname,recupdator=_userno,categorylanguage=_categorylanguage
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

