drop function "crm_func_rel_tab_add"("_entityid" uuid, "_relentityid" uuid, "_fieldid" uuid, "_relname" text, "_icon" uuid, "_ismanytomany" int4, "_srcsql" text, "_srctitle" text, "_userno" int4);



CREATE OR REPLACE FUNCTION "public"."crm_func_rel_tab_add"("_entityid" uuid, "_relentityid" uuid, "_fieldid" uuid, "_relname" text, "_icon" uuid, "_ismanytomany" int4, "_srcsql" text, "_srctitle" text, "_userno" int4, "_reltablanguage" jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE
 
  _relid uuid:=null;
  _orderby int4;
  _funcid uuid;
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
				 IF  EXISTS (SELECT 1 FROM crm_sys_entity_rel_tab WHERE entityid=_entityid AND relentityid=_relentityid AND fieldid=_fieldid AND recstatus=1 LIMIT 1 ) THEN  
								Raise EXCEPTION '该页签配置已存在';
				 END IF;
         IF NOT EXISTS (SELECT 1 FROM crm_sys_entity_fields WHERE fieldid=_fieldid AND recstatus=1 LIMIT 1) THEN
								Raise EXCEPTION '该实体字段不存在';
         END IF;
				 IF  NOT EXISTS (SELECT 1 FROM crm_sys_entity WHERE entityid=_entityid AND recstatus=1 LIMIT 1) OR
						 NOT EXISTS (SELECT 1 FROM crm_sys_entity WHERE entityid=_relentityid AND recstatus=1 LIMIT 1)  THEN  
								Raise EXCEPTION '该实体不存在';
				 END IF;
        IF EXISTS(SELECT 1 FROM crm_sys_entity_rel_tab WHERE relname=_relname and  entityid=_entityid AND recstatus=1 LIMIT 1 ) THEN
             Raise EXCEPTION '该页签名称已存在';
        END IF;
        SELECT COALESCE(MAX(recorder),0)+1 into _orderby from crm_sys_entity_rel_tab  where entityid=_entityid and recstatus=1;

 

 
        INSERT INTO "public"."crm_sys_entity_rel_tab" (
        "entityid", 
        "relentityid", 
        "relname", 
        "icon",
        "recorder", 
        "reccreator", 
        "recupdator","fieldid","tabtype","web","mob","ismanytomany","srcsql","srctitle",relname_lang) 
        VALUES(
        _entityid,
        _relentityid,
				_relname,
        _icon,
				_orderby,
        _userno,
        _userno,_fieldid,0,1,1,_ismanytomany,_srcsql,_srctitle,_reltablanguage
        )  returning relid into _relid;
       FOR _r IN SELECT funcid,devicetype FROM crm_sys_function WHERE rectype=4 AND entityid=_entityid AND recstatus=1 LOOP
					 SELECT id::uuid,flag,msg,stacks,codes INTO _codeid,_codeflag,_codemsg,_codestack,_codestatus FROM 
					 crm_func_function_insert(-1,'api/dynamicentity/reltablist',_r.funcid,_relname,'RelTabList',_entityid,_r.devicetype,0,_relid::TEXT,_userno);
					 IF _codeflag=0 THEN
							Raise EXCEPTION '%','初始化职能树异常';
					 END IF;
       END LOOP;

		_codeid:= _relid::TEXT;
		_codeflag:= 1;
		_codemsg:= '新增页签配置成功';
 
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

