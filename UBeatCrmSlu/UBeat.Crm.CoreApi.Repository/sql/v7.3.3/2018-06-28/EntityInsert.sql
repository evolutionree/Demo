CREATE OR REPLACE FUNCTION "public"."crm_func_entity_add"("_entityname" text, "_entitytable" text, "_typeid" int4, "_remark" text, "_styles" text, "_icons" text, "_relentityid" text, "_relfieldid" uuid, "_relaudit" int4, "_recstatus" int4, "_userno" int4,"_entitylanguage" jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE
    _relateentityid uuid:=null;
    _relentityname varchar;
    _entityid uuid:=null;
    _orderby int4;
    _relfieldname TEXT;
    _count INT4;
    _funcid uuid;
    _menupid uuid;
    _index INT4;
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

BEGIN

   BEGIN

 
				 --数据逻辑
				 IF  EXISTS (select 1 from crm_sys_entity where entityname=_entityname AND recstatus=1 limit 1 ) AND (_typeid<>1 AND _typeid<>3) THEN  
								Raise EXCEPTION '实体名称不能重复';
				 END IF;
				 IF  EXISTS (select 1 from crm_sys_entity where entitytable=_entitytable limit 1)  THEN  
								Raise EXCEPTION '实体表名不能重复';
				 END IF;
				 IF _relentityid IS NULL OR _relentityid='' THEN  
							 _relateentityid:=null;
               _relfieldid:=NULL;
         ELSE
							 _relateentityid:=_relentityid::uuid;
               IF EXISTS(SELECT  1  FROM crm_sys_entity WHERE modeltype in (1,3) AND relentityid=_relateentityid::uuid AND entityname=_entityname) THEN
							      	Raise EXCEPTION '关联同一实体的名称不能重复';
               END IF;
				 END IF;

         
        SELECT COALESCE(MAX(recorder),0)+1 into _orderby from crm_sys_entity;

 

        INSERT INTO "public"."crm_sys_entity" (
        "entityname", 
        "entitytable", 
        "modeltype", 
        "remark", 
        "styles", 
        "icons", 
        "relentityid", 
        "relfieldid",  
        "relaudit", 
        "recorder", 
        "recstatus", 
        "reccreator", 
        "recupdator",
	"entitylanguage") 
        VALUES(
        _entityname,
        _entitytable,
        _typeid,
        _remark,
        _styles,
        _icons,
        _relateentityid,
        _relfieldid,
        _relaudit,
        _orderby,
        _recstatus,
        _userno,
        _userno,
	_entitylanguage
        )  returning entityid into _entityid;
        INSERT INTO "public"."crm_sys_entity_version" 
				("entityid", "publishtime", "updatetime",  "reccreator", "recupdator", "descript")
        VALUES 
        (_entityid::uuid, NULL, NULL, _userno, _userno, NULL);
        SELECT flag,msg,stacks,codes INTO _codeflag,_codemsg,_codestack,_codestatus FROM crm_func_entity_init(_entityid,_userno);

        IF _codeflag= 1 THEN
							_codeid:= _entityid::TEXT;
							_codeflag:= 1;
							_codemsg:= '新增实体成功';
            IF _typeid=3 THEN
                --给动态列表的关联对象的关联字段更新一个别名
								WHILE 1=1 loop
									SELECT random_string(15) INTO _relfieldname;
									SELECT COUNT(1) INTO _count FROM crm_sys_entity_fields WHERE entityid=_entityid AND fieldname=_relfieldname;
									exit when _count=0;
								end loop; 
								UPDATE crm_sys_entity SET relfieldname=_relfieldname WHERE entityid=_entityid;

                SELECT funcid INTO _funcid FROM crm_sys_function WHERE entityid=_relateentityid AND rectype=1;
                IF _funcid is NOT NULL THEN
                     SELECT id INTO _menupid FROM crm_sys_webmenu WHERE parentid='10000000-0000-0000-0001-000000000005'::uuid AND funcid=_funcid::TEXT LIMIT 1;

                     IF _menupid IS NOT NULL THEN
                         SELECT funcid INTO _funcid FROM crm_sys_function where entityid=_entityid AND relationvalue=_entityid::TEXT LIMIT 1;
                         SELECT (COALESCE(index,0)+1) INTO _index FROM crm_sys_webmenu WHERE parentid=_menupid;
                         INSERT INTO "public"."crm_sys_webmenu" ( id,"index", "name", "icon", "path", "funcid", "parentid", "isdynamic", "islogicmenu", "isleaf")
                         VALUES (uuid_generate_v1()::uuid,_index, _entityname, '', '/entcomm-dynamic/'||_entityid::TEXT, _funcid, _menupid, '1', '0', NULL);
--                      ELSE
--                          INSERT INTO "public"."crm_sys_webmenu" ( id, "index", "name", "icon", "path", "funcid", "parentid", "isdynamic", "islogicmenu", "isleaf")
--                          VALUES (uuid_generate_v1()::uuid,_index, _entityname, '', NULL, _funcid, '10000000-0000-0000-0001-000000000005', '1', '0', NULL) RETURNING id::uuid INTO _menupid;
--                          SELECT funcid INTO _funcid FROM crm_sys_function where entityid=_entityid AND relationvalue=_entityid::TEXT LIMIT 1;
--                          INSERT INTO "public"."crm_sys_webmenu" ( id,"index", "name", "icon", "path", "funcid", "parentid", "isdynamic", "islogicmenu", "isleaf")
--                          VALUES (uuid_generate_v1()::uuid,_index, _entityname, '', '/entcomm-dynamic/'||_entityid::TEXT, _funcid, _menupid, '1', '0', NULL);
                     END IF;
                ELSE
                     Raise EXCEPTION '%','动态实体关联对象职能信息缺失';
                END IF;

            END IF;
        ELSE
               Raise EXCEPTION '%', _codemsg;
        END IF;
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
