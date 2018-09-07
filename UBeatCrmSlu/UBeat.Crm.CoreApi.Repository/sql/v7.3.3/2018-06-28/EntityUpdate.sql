CREATE OR REPLACE FUNCTION "public"."crm_func_entity_edit"("_entityid" varchar, "_entityname" varchar, "_typeid" int4, "_icons" text, "_remark" varchar, "_relentityid" text, "_relfieldid" uuid, "_relaudit" int4, "_userno" int4,"_entitylanguage" jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE
  _old_entityname TEXT;
  _r record;
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

BEGIN

   BEGIN


				 IF NOT EXISTS (select 1 from crm_sys_entity where entityid=_entityid::uuid limit 1) THEN
								Raise EXCEPTION '实体不存在';
				 END IF;


				 --数据逻辑
				 IF  EXISTS (select 1 from crm_sys_entity where entityid<>_entityid::uuid AND entityname=_entityname limit 1) THEN
								Raise EXCEPTION '实体名称不能重复';
				 END IF;

         SELECT entityname INTO _old_entityname FROM crm_sys_entity WHERE entityid=_entityid::uuid;


    		 UPDATE crm_sys_entity SET 
    		 entityname=_entityname,
    		 relfieldid=_relfieldid,
         icons=_icons::uuid,
    		 modeltype=_typeid,
    		 remark=_remark,
         relaudit=_relaudit,
         relentityid=_relentityid::uuid,
				 entitylanguage=_entitylanguage,
    		 recupdator=_userno WHERE entityid=_entityid::uuid;

         --UPDATE crm_sys_entity_version SET updatetime=now(),status=1 WHERE entityid=_entityid::uuid;

         IF EXISTS(SELECT 1 FROM crm_sys_entrance WHERE entityid=_entityid::uuid LIMIT 1) THEN
             UPDATE crm_sys_entrance SET entryname=_entityname WHERE entityid=_entityid::uuid;
         END IF;
         IF _typeid=0 OR _typeid=2 THEN
							FOR _r IN  SELECT funcid FROM crm_sys_function WHERE entityid=_entityid::uuid AND rectype=1 LOOP 
								WITH RECURSIVE T1 as
								(
								SELECT f.funcid,f.funcname,f.parentid from crm_sys_function f WHERE f.funcid=_r.funcid
								UNION ALL
								SELECT func.funcid,(T1.funcname||'>'||func.funcname) as funcname,func.parentid   from crm_sys_function func INNER JOIN T1  ON T1.funcid = func.parentid
								)
								UPDATE crm_sys_function SET funcname=replace(funcname,_old_entityname,_entityname) WHERE funcid IN (SELECT funcid from T1);
              END LOOP;
         ELSEIF  _typeid=3 THEN
              IF EXISTS(SELECT 1 FROM crm_sys_function WHERE entityid=_entityid::uuid LIMIT 1) THEN
                  UPDATE crm_sys_function SET funcname=replace(funcname,_old_entityname,_entityname) WHERE entityid=_entityid::uuid;
              END IF;
              IF EXISTS(SELECT 1 FROM crm_sys_function WHERE relationvalue=_entityid LIMIT 1) THEN
                  UPDATE crm_sys_function SET funcname=replace(funcname,_old_entityname,_entityname) WHERE relationvalue=_entityid;
              END IF;
         END IF;
         

					_codeid:= _entityid::TEXT;
					_codeflag:= 1;
					_codemsg:= '编辑实体成功';
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
