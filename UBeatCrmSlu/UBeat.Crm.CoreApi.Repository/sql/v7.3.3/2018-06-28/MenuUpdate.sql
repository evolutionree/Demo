CREATE OR REPLACE FUNCTION "public"."crm_func_rule_edit"("_typeid" int4, "_rule" text, "_ruleitem" text, "_ruleset" text, "_rulerelation" text, "_userno" int4)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
--SELECT crm_func_account_userinfo_dept_list('', '', '7f74192d-b937-403f-ac2a-8be34714278b', 1,1,-1);
--FETCH ALL FROM datacursor;
--FETCH ALL FROM pagecursor;

DECLARE
  _orderby int4:=0;

  _ruleid uuid:=null; 
  _entityid uuid=null;
  _flowid uuid=null;
  _rulesetjson text; 
  _rulejsonobj json;
  _rulesetjsonarr json;
  _ruleitemjsonarr json;
  _ruleitemrelaarr json;
  _roleid uuid;
  _menuid uuid;
  _menuname text;
	_menulanguage json;

  _r record;
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

BEGIN
    
 BEGIN
			SELECT _rule::json into _rulejsonobj;
			_rulesetjson:='['||_ruleset||']';

			SELECT _rulesetjson::json INTO _rulesetjsonarr;
			SELECT _ruleitem::json INTO _ruleitemjsonarr;
			SELECT _rulerelation::json INTO _ruleitemrelaarr;
			SELECT (_rulejsonobj->>'ruleid')::uuid into _ruleid;

      IF _ruleid!='00000000-0000-0000-0000-000000000000'::uuid AND
         _ruleid!='00000000-0000-0000-0000-000000000001'::uuid AND
         _ruleid!='00000000-0000-0000-0000-000000000002'::uuid AND
         _ruleid!='00000000-0000-0000-0000-000000000003'::uuid AND
         _ruleid!='00000000-0000-0000-0000-000000000004'::uuid THEN

						CASE _typeid -- ROLE
						WHEN 0 THEN
							 SELECT (_rulejsonobj->>'roleid')::uuid into _roleid;
							 IF NOT EXISTS(SELECT 1 FROM crm_sys_role_group_relate WHERE roleid=_roleid) THEN
									Raise EXCEPTION '该角色规则记录不存在';
							 END IF;
							 IF NOT EXISTS(SELECT 1 FROM crm_sys_rule WHERE ruleid=_ruleid) THEN
									Raise EXCEPTION '该筛选规则记录不存在';
							 END IF;
							 UPDATE crm_sys_role SET recupdated=now() WHERE roleid=_roleid;
						WHEN 1 THEN --menu
							 SELECT (_rulejsonobj->>'id')::uuid into _menuid;
							 SELECT (_rulejsonobj->>'menuname') into _menuname;
							 SELECT (_rulejsonobj->>'menulanguage')::json INTO _menulanguage;
							 IF NOT EXISTS(SELECT 1 FROM crm_sys_entity_menu WHERE menuid=_menuid) THEN
									Raise EXCEPTION '该菜单记录不存在';
							 END IF;
							 IF NOT EXISTS(SELECT 1 FROM crm_sys_rule WHERE ruleid=_ruleid) THEN
									Raise EXCEPTION '该筛选规则记录不存在';
							 END IF;
							 UPDATE crm_sys_entity_menu SET
											 menuname=_menuname,
											 recupdator=_userno,
											 menulanguage=_menulanguage WHERE menuid =_menuid;
               UPDATE crm_sys_function SET funcname=_menuname WHERE relationvalue=_menuid::TEXT;
						WHEN 2 THEN --动态实体
							 SELECT (_rulejsonobj->>'entityid')::uuid into _entityid;
							 IF NOT EXISTS(SELECT 1 FROM crm_sys_entity_plugin_rule_relation WHERE entityid=_entityid AND ruleid=_ruleid) THEN
									Raise EXCEPTION '该动态实体规则记录不存在';
							 END IF;
							 IF NOT EXISTS(SELECT 1 FROM crm_sys_rule WHERE ruleid=_ruleid) THEN
									Raise EXCEPTION '该筛选规则记录不存在';
							 END IF;
						WHEN 4 THEN --独立实体审批
							 SELECT (_rulejsonobj->>'flowid')::uuid into _flowid;
							 IF NOT EXISTS(SELECT 1 FROM crm_sys_workflow_rule_relation WHERE flowid=_flowid AND ruleid=_ruleid) THEN
									Raise EXCEPTION '该动态实体规则记录不存在';
							 END IF;
							 IF NOT EXISTS(SELECT 1 FROM crm_sys_rule WHERE ruleid=_ruleid) THEN
									Raise EXCEPTION '该筛选规则记录不存在';
							 END IF;
						ELSE
							 --Raise EXCEPTION '参数异常';
						END CASE;
						UPDATE crm_sys_rule SET
											 rulename=_rulejsonobj->>'rulename',
											 rulesql=COALESCE((_rulejsonobj->>'rulesql'),''),
											 recupdator=_userno WHERE ruleid =_ruleid;
				 
					 DELETE FROM crm_sys_rule_item WHERE   itemid in (SELECT itemid FROM crm_sys_rule_item_relation WHERE ruleid=_ruleid);
				 
					 FOR _r IN SELECT itemid, itemname,fieldid,operate,ruledata,ruletype,rulesql,usetype
					 FROM json_populate_recordset(null::crm_sys_rule_item,_ruleitemjsonarr)  loop
									_orderby:=_orderby+1;
									INSERT INTO crm_sys_rule_item(
												itemid,
												itemname,
												fieldid,
												operate,
												ruledata,
												ruletype,
												rulesql,
												usetype,
												recorder,
												reccreator,
												recupdator) SELECT _r.itemid,_r.itemname,_r.fieldid,_r.operate,_r.ruledata,
																					 _r.ruletype,_r.rulesql,_r.usetype,_orderby,
																					 _userno,_userno  ;
				 
					 END loop;

					 DELETE FROM crm_sys_rule_item_relation WHERE ruleid=_ruleid;

					 FOR _r IN SELECT itemid,userid,rolesub,paramindex FROM json_populate_recordset(null::crm_sys_rule_item_relation,_ruleitemrelaarr)  loop
 
									INSERT INTO crm_sys_rule_item_relation(
														 ruleid,
														 itemid,
														 userid,
														 rolesub,
														 paramindex)
														 SELECT _ruleid,_r.itemid,_r.userid,_r.rolesub,_r.paramindex;

					 END loop;
					 DELETE FROM crm_sys_rule_set WHERE ruleid=_ruleid;
					 INSERT INTO "public"."crm_sys_rule_set" 
					 ("ruleid", "ruleset", "userid", "ruleformat")
						SELECT _ruleid,ruleset,userid,ruleformat FROM json_populate_recordset
						(null::crm_sys_rule_set,_rulesetjsonarr);
						_codeflag= 1;
    ELSE

        SELECT id,flag,msg,stacks,codes INTO 
               _codeid,_codeflag,_codemsg,_codestack,_codestatus FROM crm_func_rule_add(1,_typeid,
																																							 _rule,
																																							 _ruleitem,
																																							 _ruleset,
																																							 _rulerelation,
			       																																	 _userno);
         IF _typeid=1 THEN
           SELECT (_rulejsonobj->>'id')::uuid into _menuid;
 
           UPDATE crm_sys_entity_menu  SET menuname=(_rulejsonobj->>'menuname'), menulanguage=(_rulejsonobj->>'menulanguage')::jsonb, ruleid=_codeid::uuid WHERE menuid=_menuid;
         END IF;
			  _codeflag= 1;
    END IF;

		IF _codeflag= 1 THEN
 
           UPDATE crm_sys_rule SET rulesql=crm_func_role_rule_fetch_single_sql(_ruleid,_userno) WHERE ruleid=_ruleid;
					_codeid:= _ruleid::TEXT;
					_codeflag:= 1;
					_codemsg:= '更新筛选规则成功';
		ELSE
					 Raise EXCEPTION '更新筛选规则失败';
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
