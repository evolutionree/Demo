CREATE OR REPLACE FUNCTION "public"."crm_func_entity_dynamic_check_visible"("_entityid" uuid, "_recid" uuid, "_deviceclassic" int4, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_account_userinfo_dept_list('', '', '7f74192d-b937-403f-ac2a-8be34714278b', 1,1,-1);
--FETCH ALL FROM datacursor;
--FETCH ALL FROM pagecursor;

DECLARE
   _execute_sql TEXT:='SELECT *,0 ::TEXT AS pluginid,0 AS validflag FROM crm_sys_empty WHERE 1=2';
   _check_sql TEXT:='';
   _rule_sql TEXT:='';
   _entity_table TEXT:='';
   _pluginflow_sql TEXT:='';
   _entity_audit_sql TEXT:='';

   _r record; 
   _condition_array TEXT[];
   _caseid uuid;
   _relrecid uuid;
   _flowid uuid;

   _mapperentityid uuid;


   _check_sql_flow TEXT:='';
   _condition_array_flow TEXT[];
   _condition_array_flow_entity TEXT[];

   _execute_sql_flow TEXT:='SELECT *,0 AS validflag FROM crm_sys_workflow WHERE 1=2';


   _execute_sql_rel_flow TEXT:='SELECT *,0 AS validflag FROM crm_sys_workflow WHERE 1=2';
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

  _datacursor refcursor:= 'datacursor';
  _flowcursor refcursor:= 'flowcursor';
  _entityauditcursor refcursor:= 'entityauditcursor';
  _relflowcursor refcursor:= 'relflowcursor';
BEGIN

  BEGIN
 
					SELECT entitytable INTO _entity_table FROM crm_sys_entity WHERE entityid = _entityid LIMIT 1;
 
					IF _entity_table IS NULL OR _entity_table ='' THEN
								 Raise EXCEPTION '不存在的实体ID';
					END IF;

					 --获取所有相关的插件
					FOR _r IN 
					SELECT e.entityid,e.entityname,e.entitytable,e.relentityid,r.ruleid FROM crm_sys_entity AS e
					LEFT JOIN crm_sys_entity_plugin_rule_relation AS r ON e.entityid = r.entityid
					WHERE e.relentityid = _entityid AND e.modeltype = 3 AND e.recstatus = 1
					LOOP
										--Raise Notice 'entityname %',_r.entityname;
                    IF _r.ruleid IS NULL THEN
													_rule_sql:='1=1';
                    ELSE
													_rule_sql:=crm_func_role_rule_fetch_single_sql(_r.ruleid, _userno);
                    END IF;

										_check_sql:=format('
													SELECT
													''' || _r.entityid || '''::TEXT  AS pluginid,
													COALESCE((
																	SELECT 1 FROM %s AS e
																	WHERE e.recid = ''' || _recid || '''
																	AND %s LIMIT 1
													),0) AS validflag
										',_entity_table,_rule_sql);
								
									_condition_array:=array_append(_condition_array,_check_sql);
					 END LOOP;

						IF array_length(_condition_array, 1) > 0 THEN
									_execute_sql:=array_to_string(_condition_array, ' UNION ');
						END IF;

			    	_execute_sql:=REPLACE(crm_func_role_rule_param_format(_execute_sql,_userno),'{recid}',''''||_recid::TEXT||'''');--特殊规则 处理

						_execute_sql:=format('with T1  As  (
                                         %s
																			)
                                      SELECT string_agg(pluginid::TEXT,'','') AS pluginids FROM (
																			SELECT * FROM T1 WHERE validflag=0
																			UNION
																			SELECT * FROM (SELECT * FROM T1 WHERE validflag=1 ) AS tmp  WHERE 
                                      (NOT EXISTS(SELECT 1 FROM crm_sys_workflow WHERE entityid=tmp.pluginid::uuid AND recstatus=1) 
                                       AND  EXISTS(SELECT 1 FROM crm_sys_entity where entityid=tmp.pluginid::uuid AND modeltype=3 AND relaudit=1))
                                       AND tmp.pluginid  IN (

 																			  SELECT tmp1.relationvalue FROM (
																					SELECT relationvalue::TEXT  FROM crm_sys_function WHERE  (SELECT COUNT(1) FROM (
 																					SELECT vocationid FROM crm_sys_userinfo_vocation_relate WHERE userid='||_userno||'
																					EXCEPT
 																					SELECT DISTINCT vocationid FROM crm_sys_vocation_function_relation WHERE functionid=funcid
 																					) t)=0 AND recstatus=1  AND funcid IN (SELECT funcid FROM crm_sys_function WHERE parentid IN (
                                        SELECT funcid from crm_sys_function where entityid=$1 AND rectype=5 AND devicetype='||_deviceclassic||'
                                        ))) AS tmp1
                                        UNION 
                                        SELECT entityid::TEXT FROM crm_sys_entity WHERE  (NOT EXISTS(SELECT 1 FROM crm_sys_workflow WHERE entityid=tmp.pluginid::uuid AND recstatus=1) 
                                       AND  EXISTS(SELECT 1 FROM crm_sys_entity where entityid=tmp.pluginid::uuid AND modeltype=3 AND relaudit=1))))  AS t
						',_execute_sql);
 
            --实体是否关联实体
            _pluginflow_sql:='
						SELECT e.entityid,f.flowid,f.flowname_lang,e.entityname_lang FROM crm_sys_entity AS e
						INNER JOIN crm_sys_workflow AS f ON f.entityid = e.entityid
						WHERE e.relentityid = $1 AND e.modeltype = 3 AND e.recstatus = 1 AND flowid::text NOT IN 
            (
									SELECT relationvalue  FROM crm_sys_function WHERE relationvalue is not null and (SELECT COUNT(1) FROM (
SELECT vocationid FROM crm_sys_userinfo_vocation_relate WHERE userid='||_userno||'
EXCEPT
SELECT DISTINCT vocationid FROM crm_sys_vocation_function_relation WHERE functionid=funcid
) t)=0 AND recstatus=1  AND funcid IN (SELECT funcid FROM crm_sys_function WHERE parentid IN (
                  SELECT funcid from crm_sys_function where entityid=$1 AND rectype=5 AND devicetype='||_deviceclassic||' ))
            )
            ';


            --是否有自身的审批
            --_entity_audit_sql:='SELECT flowid,flowname,''00000000-0000-0000-0000-100000000005'' AS icons FROM crm_sys_workflow WHERE entityid = $1';
						FOR _r IN 
						SELECT w.flowid,w.flowname,r.ruleid FROM crm_sys_workflow AS w 
						LEFT JOIN crm_sys_workflow_rule_relation AS r ON w.flowid = r.flowid
            WHERE w.entityid = _entityid AND recstatus=1
						LOOP
										--Raise Notice 'flowname %',_r.flowname;
                    IF _r.ruleid IS NULL THEN
													_rule_sql:='1=1';
                    ELSE
													_rule_sql:=crm_func_role_rule_fetch_single_sql(_r.ruleid, _userno);
                    END IF;

										_check_sql_flow:=format('
													SELECT
													''' || _r.flowid || '''::uuid AS flowid,
													COALESCE((
																	SELECT 1 FROM %s
																	WHERE recid = ''' || _recid || '''
																	AND %s  LIMIT 1
													),0) AS validflag
										',_entity_table,_rule_sql);
								
									_condition_array_flow:=array_append(_condition_array_flow,_check_sql_flow);
            END LOOP;

						IF array_length(_condition_array_flow, 1) > 0 THEN
									_execute_sql_flow:=array_to_string(_condition_array_flow, ' UNION ');
						END IF;
            _execute_sql_flow:=crm_func_role_rule_param_format(_execute_sql_flow,_userno);

			    	_execute_sql_flow:=REPLACE(_execute_sql_flow,'{recid}',''''||_recid::TEXT||'''');--特殊规则 处理

						_execute_sql_flow:=format('
									SELECT flowid,flowname,''00000000-0000-0000-0000-100000000005'' AS icons,flowname_lang FROM crm_sys_workflow WHERE flowid IN (
                         SELECT flowid FROM  (
                            SELECT * FROM ( %s ) as tmp WHERE tmp.flowid::text NOT IN 
                            (
																SELECT COALESCE(relationvalue,'''') as relationvalue   FROM crm_sys_function WHERE (SELECT COUNT(1) FROM (
																SELECT vocationid FROM crm_sys_userinfo_vocation_relate WHERE userid='||_userno||'
																EXCEPT
																SELECT DISTINCT vocationid FROM crm_sys_vocation_function_relation WHERE functionid=funcid
																) t)=0 AND recstatus=1  AND devicetype='||_deviceclassic||'  AND relationvalue<>''''
                             )
                                            ) AS t WHERE validflag = 1
                  )
						',_execute_sql_flow);
 
            SELECT commonid INTO _relrecid FROM crm_sys_custcommon_customer_relate WHERE custid=_recid;
            IF _relrecid IS  NULL OR CAST(_relrecid AS TEXT)='' THEN
               _relrecid:='00000000-0000-0000-0000-000000000000'::uuid;
            END IF;

						_execute_sql_rel_flow:=format('select entityid,
 flowname_lang,
																					 flowid,flowname,''00000000-0000-0000-0000-100000000005'' AS icons,
																					 ''%s'' as recid
																					 FROM crm_sys_workflow WHERE flowid=''d23a99d0-d8bc-4d6c-a52b-75e4988c9852''
                                           AND flowid NOT IN 
																					(
																							SELECT relationvalue::uuid  FROM crm_sys_function WHERE devicetype='||_deviceclassic||' AND funcid IN (
																							SELECT functionid FROM crm_sys_vocation_function_relation WHERE vocationid IN (

																							SELECT vocationid FROM crm_sys_userinfo_vocation_relate WHERE userid='||_userno||'
																							)) AND recstatus=1  AND relationvalue=''d23a99d0-d8bc-4d6c-a52b-75e4988c9852''
																					 ) and EXISTS (select 1 from crm_sys_custcommon_customer_relate WHERE commonid=''%s'' HAVING(count(1))>1)'
                                           ,_relrecid,_relrecid);

   Raise notice '_execute_sql_flow: %',_execute_sql_flow;
						OPEN _datacursor FOR EXECUTE _execute_sql  USING _entityid;
						RETURN NEXT _datacursor;

						OPEN _flowcursor FOR EXECUTE _pluginflow_sql USING _entityid;
						RETURN NEXT _flowcursor;

	          OPEN _entityauditcursor FOR EXECUTE _execute_sql_flow;
						RETURN NEXT _entityauditcursor;

	          OPEN _relflowcursor FOR EXECUTE _execute_sql_rel_flow USING _entityid; 
						RETURN NEXT _relflowcursor;
 
	 EXCEPTION WHEN OTHERS THEN
						 GET STACKED DIAGNOSTICS _codestack = PG_EXCEPTION_CONTEXT;
						 _codemsg:=SQLERRM;
						 _codestatus:=SQLSTATE;

             Raise Notice '_codemsg %',_codemsg;
             Raise Notice '_codestack %',_codestack;
						_execute_sql:='SELECT *,0 AS pluginid,0 AS validflag FROM crm_sys_empty WHERE 1=2';
						_pluginflow_sql:='SELECT *,0 AS pluginid,0 AS validflag FROM crm_sys_empty WHERE 1=2';
						_entity_audit_sql:='SELECT *,0 AS pluginid,0 AS validflag FROM crm_sys_empty WHERE 1=2';
						_execute_sql_rel_flow:='SELECT *,0 AS pluginid,0 AS validflag FROM crm_sys_empty WHERE 1=2';
 
						
						OPEN _datacursor FOR EXECUTE _execute_sql;
						RETURN NEXT _datacursor;

						OPEN _flowcursor FOR EXECUTE _pluginflow_sql USING _entityid;
						RETURN NEXT _flowcursor;

			      OPEN _entityauditcursor FOR EXECUTE _execute_sql_flow USING _entityid;
						RETURN NEXT _entityauditcursor;

			      OPEN _relflowcursor FOR EXECUTE _execute_sql_rel_flow USING _flowid;
						RETURN NEXT _relflowcursor;

	END;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;
