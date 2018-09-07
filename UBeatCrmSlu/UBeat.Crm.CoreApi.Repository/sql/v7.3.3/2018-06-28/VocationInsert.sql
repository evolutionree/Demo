CREATE OR REPLACE FUNCTION "public"."crm_func_vocation_copy"("_vocationid" uuid, "_vocationname" text, "_description" text, "_userno" int4,_vocationlanguage jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$


DECLARE
  _recorder integer:=0;
  _newvocationid uuid;
  _r record;
  _r1 record;
  _ruleid uuid;
  _itemid uuid;
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

BEGIN
    
	 BEGIN

 
				IF _vocationid IS NOT NULL THEN
								IF NOT EXISTS(SELECT 1 FROM crm_sys_vocation where vocationid=_vocationid) THEN
										 RAISE EXCEPTION '不存在该职能';
								END IF;
	             SELECT coalesce(max(recorder),0)+1 INTO _recorder FROM crm_sys_vocation;
 
								INSERT INTO crm_sys_vocation (vocationname,description,recorder,reccreator,recupdator,vocationlanguage)
								VALUES(_vocationname,_description,_recorder,_userno,_userno,_vocationlanguage) RETURNING vocationid  INTO _newvocationid;
                INSERT INTO "public"."crm_sys_vocation_function_relation" 
                ("vocationid", "functionid","recstaus")
                SELECT _newvocationid,functionid,1 FROM crm_sys_vocation_function_relation WHERE vocationid=_vocationid AND recstaus=1;
                FOR _r IN SELECT rr.* FROM crm_sys_vocation_function_rule_relation AS rr
													INNER JOIN crm_sys_rule AS r ON r.ruleid=rr.ruleid AND r.recstatus=1
													WHERE vocationid=_vocationid LOOP
                    INSERT INTO "public"."crm_sys_rule" ( "rulename", "entityid", "rulesql", "reccreator", "recupdator")
                           SELECT rulename,entityid,rulesql,_userno,_userno FROM crm_sys_rule WHERE ruleid=_r.ruleid RETURNING ruleid INTO _ruleid;
                    FOR _r1 IN SELECT * FROM crm_sys_rule_item WHERE itemid IN (SELECT itemid FROM crm_sys_rule_item_relation WHERE ruleid=_r.ruleid) LOOP
                        INSERT INTO "public"."crm_sys_rule_item" ("itemname", "fieldid", "operate", "ruledata", "ruletype", "rulesql", "usetype", "recorder", "reccreator", "recupdator")
                            SELECT _r1.itemname,_r1.fieldid,_r1.operate,_r1.ruledata,_r1.ruletype,_r1.rulesql,_r1.usetype,_r1.recorder,_userno,_userno RETURNING itemid INTO _itemid;
                        INSERT INTO crm_sys_rule_item_relation(ruleid,itemid,userid,rolesub,paramindex) SELECT _ruleid,_itemid,userid,rolesub,paramindex FROM crm_sys_rule_item_relation 
                            WHERE ruleid=_r.ruleid AND itemid=_r1.itemid;
                    END LOOP;
                    FOR _r1 IN SELECT * FROM crm_sys_rule_set WHERE ruleid=_r.ruleid LOOP
                          INSERT INTO "public"."crm_sys_rule_set" ("ruleid", "ruleset", "userid", "ruleformat")
                                 SELECT _ruleid,_r1.ruleset,_r1.userid,_r1.ruleformat;
                    END LOOP;
                    INSERT INTO crm_sys_vocation_function_rule_relation ("vocationid","functionid","ruleid")
                    SELECT _newvocationid,_r.functionid,_ruleid ;
                END LOOP;
				END IF;
 
		

		_codeid:= _vocationid::TEXT;
		_codeflag:= 1;
		_codemsg:= '复制职能成功';    

		   
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

