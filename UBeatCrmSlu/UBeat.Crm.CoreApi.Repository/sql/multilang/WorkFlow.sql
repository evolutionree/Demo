drop function "crm_func_workflow_add"("_entityid" uuid, "_flowname" text, "_flowtype" int4, "_backflag" int4, "_resetflag" int4, "_expireday" int4, "_remark" text, "_skipflag" int4, "_userno" int4);

-------------------------

alter table crm_sys_workflow rename column flowlanguage to flowname_lang;

----------------------------------

CREATE OR REPLACE FUNCTION "public"."crm_func_workflow_add"("_entityid" uuid, "_flowname" text, "_flowtype" int4, "_backflag" int4, "_resetflag" int4, "_expireday" int4, "_remark" text, "_skipflag" int4, "_userno" int4, "_flowlanguage" jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE

  _flowid uuid;
  _vernum INT;
  _r record;
  _funcid uuid;
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

BEGIN

   BEGIN
	
          IF _entityid IS NULL THEN
                 Raise EXCEPTION '流程关联的实体ID不能为空';
          END IF; 	

          IF _flowname IS NULL OR _flowname = '' THEN
                Raise EXCEPTION '流程名称不能为空';
          END IF;



          --判断流程名称是否存在
          PERFORM 1 FROM crm_sys_workflow WHERE flowname = _flowname  LIMIT 1;
          IF FOUND THEN
                 Raise EXCEPTION '流程名称已经存在';
          END IF;
          IF _remark IS NULL THEN
							_remark:='';
          END IF;

          IF _flowtype = 0 THEN
               _vernum:=1;
          ELSE
							 _vernum:=0;
          END IF;
 
          INSERT INTO crm_sys_workflow (flowname, flowtype, backflag, resetflag, expireday, remark, entityid, vernum, recorder, reccreator, recupdator, skipflag ,flowname_lang) 
          VALUES (_flowname, _flowtype, _backflag, _resetflag, _expireday, _remark, _entityid, _vernum, '0', _userno, _userno,  _skipflag,_flowlanguage) RETURNING flowid INTO _flowid;



				 FOR _r IN SELECT funcid,devicetype  FROM crm_sys_function WHERE entityid=(SELECT relentityid FROM crm_sys_entity WHERE entityid=_entityid AND recstatus=1 LIMIT 1) AND rectype=5 LOOP
						 
						 SELECT id,flag,msg,stacks,codes INTO _codeid,_codeflag,_codemsg,_codestack,_codestatus FROM 
						 crm_func_function_insert(-1,'api/dynamicentity/add',_r.funcid,_flowname,'Flow',_entityid,_r.devicetype,0,_flowid::TEXT,_userno);
 
						 IF _codeflag=0 THEN
								Raise EXCEPTION '%','初始化职能树异常';
						 END IF;
				 END LOOP;
				 FOR _r IN SELECT funcid,devicetype FROM crm_sys_function WHERE entityid=_entityid AND rectype=5  LOOP
             
						 SELECT id,flag,msg,stacks,codes INTO _codeid,_codeflag,_codemsg,_codestack,_codestatus FROM 
						 crm_func_function_insert(-1,'api/workflow/addcase',_r.funcid,_flowname,'Flow',_entityid,_r.devicetype,0,_flowid::TEXT,_userno);
 
						 IF _codeflag=0 THEN
								Raise EXCEPTION '%','初始化职能树异常';
						 END IF;
				 END LOOP;

--  				 FOR _r IN SELECT * FROM crm_sys_function WHERE entityid=_entityid and rectype=5 AND recstatus=1  LOOP
--                UPDATE crm_sys_function SET relationvalue=_flowid::TEXT WHERE funcid=_r.funcid;
-- 				 END LOOP;
					_codeid:= _flowid::TEXT;
					_codeflag:= 1;
					_codemsg:= '新增流程成功';
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

---------------------------------------------------


drop function "crm_func_workflow_update"("_flowid" uuid, "_flowname" text, "_backflag" int4, "_resetflag" int4, "_expireday" int4, "_remark" text, "_skipflag" int4, "_userno" int4);


------------------------------------------------

CREATE OR REPLACE FUNCTION "public"."crm_func_workflow_update"("_flowid" uuid, "_flowname" text, "_backflag" int4, "_resetflag" int4, "_expireday" int4, "_remark" text, "_skipflag" int4, "_userno" int4, "_flowlanguage" jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE

  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

BEGIN

   BEGIN
	
          IF _flowname IS NULL OR _flowname = '' THEN
                Raise EXCEPTION '流程名称不能为空';
          END IF;

          --判断流程名称是否存在
          PERFORM 1 FROM crm_sys_workflow WHERE flowid!=_flowid AND flowname = _flowname AND recstatus = 1 LIMIT 1;
          IF FOUND THEN
                 Raise EXCEPTION '流程名称已经存在';
          END IF;
          IF _remark IS NULL THEN
							_remark:='';
          END IF;
 
          UPDATE crm_sys_workflow SET flowname = _flowname,backflag = _backflag,resetflag = _resetflag,
          expireday = _expireday,remark = _remark,skipflag = _skipflag,recupdator = _userno,
					flowname_lang = _flowlanguage
          WHERE flowid = _flowid;

					_codeid:= _flowid::TEXT;
					_codeflag:= 1;
					_codemsg:= '修改流程成功';
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



--------------------------------

CREATE OR REPLACE FUNCTION "public"."crm_func_workflow_list"("_flowstatus" int4, "_searchname" text, "_pageindex" int4, "_pagesize" int4, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_account_userinfo_contact_list(0,1, 10,1);
--FETCH ALL FROM datacursor;
--FETCH ALL FROM pagecursor;
--FETCH ALL FROM versioncursor;

DECLARE
  _condition_sql TEXT:='';

  _execute_sql TEXT;
  _condition_array TEXT[];

  --分页标准参数
  _page_sql TEXT;
  _count_sql TEXT;
  _datacursor refcursor:= 'datacursor';
  _pagecursor refcursor:= 'pagecursor';

BEGIN

   IF _flowstatus IS NOT NULL AND _flowstatus <>-1 THEN
						_condition_array:=array_append(_condition_array,'w.recstatus = ''' || _flowstatus || ''' ');
   END IF;

   IF _searchname IS NOT NULL AND _searchname !='' THEN
           Raise Notice 'Enter searchname %',_searchname;
						_condition_array:=array_append(_condition_array,'w.flowname ilike ''%' || _searchname || '%'' ');
   END IF;

    IF array_length(_condition_array, 1) > 0 THEN
          _condition_sql:=_condition_sql || format(' AND (%s)',array_to_string(_condition_array, ' AND '));
    END IF;
    Raise Notice '_condition_sql,%',_condition_sql;

   _execute_sql:=format('
		SELECT w.flowid,w.flowname,w.flowtype,w.backflag,w.resetflag,w.expireday,w.remark,
		w.entityid,w.vernum,w.skipflag,e.entityname,w.recstatus,e.modeltype AS entitymodeltype,e.relentityid,re.modeltype AS relentitymodeltype,
		w.flowname_lang
		FROM crm_sys_workflow AS w
		LEFT JOIN crm_sys_entity AS e ON w.entityid = e.entityid
		LEFT JOIN crm_sys_entity AS re ON re.entityid = e.relentityid
    WHERE 1=1
    %s
    ORDER BY w.reccreated DESC
    ',_condition_sql);

    RAISE NOTICE '%',_execute_sql;

  --查询分页数据
	SELECT * FROM crm_func_paging_sql_fetch(_execute_sql, _pageindex, _pagesize) INTO _page_sql,_count_sql;

  OPEN _datacursor FOR EXECUTE _page_sql;
	RETURN NEXT _datacursor;

	OPEN _pagecursor FOR EXECUTE _count_sql;
	RETURN NEXT _pagecursor;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;



-----------------

