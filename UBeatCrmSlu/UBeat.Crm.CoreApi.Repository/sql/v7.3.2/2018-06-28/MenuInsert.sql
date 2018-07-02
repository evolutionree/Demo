CREATE OR REPLACE FUNCTION "public"."crm_func_rule_add"("_isdefaultrule" int4, "_typeid" int4, "_rule" text, "_ruleitem" text, "_ruleset" text, "_rulerelation" text, "_userno" int4)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
--SELECT crm_func_account_userinfo_dept_list('', '', '7f74192d-b937-403f-ac2a-8be34714278b', 1,1,-1);
--FETCH ALL FROM datacursor;
--FETCH ALL FROM pagecursor;

DECLARE
  _orderby int4:=0;
  _rulejson text;
  _rulesetjson text;
  _rolename TEXT;

  _ruleid uuid:=null;
  _tmpruleid uuid:=null;
  _roleid uuid:=null;
  _rulejsonarr json;
  _rulesetjsonarr json;
  _ruleitemjsonarr json;
  _ruleitemrelaarr json;
  _rulejsonobj json;
  _menuname text;
  _entityid uuid;
  _flowid uuid;
  _r record;
  _menuid uuid;
	_menulanguage jsonb;
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

BEGIN
    
 BEGIN
     SELECT _rule::json into _rulejsonobj;
    _rulejson:='['||_rule||']';
    _rulesetjson:='['||_ruleset||']';
    SELECT _rulejson::json INTO _rulejsonarr;
    SELECT _rulesetjson::json INTO _rulesetjsonarr;
    SELECT _ruleitem::json INTO _ruleitemjsonarr;
    SELECT _rulerelation::json INTO _ruleitemrelaarr;
    INSERT INTO crm_sys_rule(
               rulename,
               entityid,
               rulesql,
               reccreator,
               recupdator)
    SELECT rulename,entityid,COALESCE(rulesql,'') rulesql,_userno,_userno FROM json_populate_recordset(null::crm_sys_rule, _rulejsonarr) 
    RETURNING ruleid INTO _ruleid;
    CASE _typeid --ROLE
    WHEN 0 THEN
      SELECT (_rulejsonobj->>'roleid')::uuid INTO _roleid;
      IF NOT EXISTS(SELECT 1 FROM crm_sys_userinfo_role_relate WHERE userid=_userno AND roleid='63dd2a9d-7f75-42ff-a696-7cc841e884e7'::uuid LIMIT 1) THEN
					 IF EXISTS(SELECT 1 FROM crm_sys_role WHERE roleid=_roleid AND roletype=0 LIMIT 1) THEN
								 SELECT rolename INTO _rolename FROM  crm_sys_role WHERE roleid=_roleid AND roletype=0 LIMIT 1;
								 Raise EXCEPTION '%',_rolename||'为系统角色,不允许编辑规则';
					 END IF;
      END IF;
      SELECT (_rulejsonobj->>'entityid')::uuid INTO _entityid;
      IF NOT EXISTS(
									 SELECT 1
									 FROM crm_sys_role_rule_relation ro 
									 INNER JOIN  crm_sys_rule r  on ro.ruleid=r.ruleid
									 INNER JOIN crm_sys_rule_item_relation ir ON r.ruleid=ir.ruleid where
									 r.recstatus=1 and ro.roleid=_roleid AND r.entityid=_entityid LIMIT 1
                   ) THEN
					INSERT INTO crm_sys_role_rule_relation(
									 roleid,
									 ruleid)
					SELECT _roleid,_ruleid;
      ELSE
          --Raise EXCEPTION '%','该角色已存在规则';
					 SELECT ro.ruleid INTO _tmpruleid
					 FROM crm_sys_role_rule_relation ro 
					 INNER JOIN  crm_sys_rule r  on ro.ruleid=r.ruleid
					 INNER JOIN crm_sys_rule_item_relation ir ON r.ruleid=ir.ruleid where
					 r.recstatus=1 and ro.roleid=_roleid AND r.entityid=_entityid LIMIT 1;
           UPDATE crm_sys_role_rule_relation SET ruleid=_ruleid WHERE ruleid=_tmpruleid AND roleid=_roleid;
      END IF;
    WHEN 1 THEN -- menu
      SELECT (COALESCE((MAX(recorder)),0)+1) INTO _orderby FROM crm_sys_entity_menu;
      SELECT (_rulejsonobj->>'menuname') into _menuname;
      SELECT (_rulejsonobj->>'entityid')::uuid into _entityid;
      SELECT (_rulejsonobj->>'menulanguage')::jsonb into _menulanguage;
      IF _isdefaultrule=0 THEN
          _menuid:=uuid_generate_v1()::uuid;
					INSERT INTO crm_sys_entity_menu(
                   menuid,
									 menuname,
									 entityid,
									 ruleid,
									 recorder,
									 reccreator,
									 recupdator,
									 menulanguage)
					SELECT _menuid,_menuname,_entityid,_ruleid,_orderby,_userno,_userno,_menulanguage;
          FOR _r IN SELECT funcid,devicetype FROM crm_sys_function WHERE entityid=_entityid AND rectype=2 AND recstatus=1 LOOP
							SELECT  flag,msg,stacks,codes INTO _codeflag,_codemsg,_codestack,_codestatus
              FROM crm_func_function_insert(-1,'api/dynamicentity/list',_r.funcid,_menuname, 
							'Menu',_entityid,_r.devicetype,0,_menuid::TEXT,_userno);
              IF _codeflag=0 THEN
                  Raise EXCEPTION '%','同步实体职能菜单异常';
              END IF;
          END LOOP;

      END IF;
   WHEN 2 THEN
      SELECT (_rulejsonobj->>'entityid')::uuid into _entityid;
      INSERT INTO crm_sys_entity_plugin_rule_relation(
               entityid,
               ruleid)
      SELECT _entityid,_ruleid;
   WHEN 4 THEN
      SELECT (_rulejsonobj->>'flowid')::uuid into _flowid;
      INSERT INTO crm_sys_workflow_rule_relation(
               flowid,
               ruleid)
      SELECT _flowid,_ruleid;
   ELSE
   END CASE;
 

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
                                   _userno,_userno ;
 
	 END loop;

   FOR _r IN SELECT ruleid,itemid,userid,rolesub,paramindex FROM json_populate_recordset(null::crm_sys_rule_item_relation,_ruleitemrelaarr)  loop
          raise notice '%',_r.userid;
          INSERT INTO crm_sys_rule_item_relation(
                     ruleid,
                     itemid,
                     userid,
                     rolesub,
                     paramindex)
                     SELECT _ruleid,_r.itemid,_r.userid,_r.rolesub,_r.paramindex;
 
	 END loop;
 
   INSERT INTO "public"."crm_sys_rule_set" 
   ("ruleid", "ruleset", "userid", "ruleformat")
    SELECT _ruleid,ruleset,userid,ruleformat FROM json_populate_recordset
    (null::crm_sys_rule_set,_rulesetjsonarr);

   UPDATE crm_sys_rule SET rulesql=crm_func_role_rule_fetch_single_sql(_ruleid,_userno) WHERE ruleid=_ruleid;

 		_codeid:= _ruleid::TEXT;
		_codeflag:= 1;
		_codemsg:= '新增筛选规则成功';               
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
