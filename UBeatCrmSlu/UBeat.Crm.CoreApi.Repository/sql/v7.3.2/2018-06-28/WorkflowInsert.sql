CREATE OR REPLACE FUNCTION "public"."crm_func_workflow_add"("_entityid" uuid, "_flowname" text, "_flowtype" int4, "_backflag" int4, "_resetflag" int4, "_expireday" int4, "_remark" text, "_skipflag" int4, "_userno" int4,_flowlanguage jsonb)
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
 
          INSERT INTO crm_sys_workflow (flowname, flowtype, backflag, resetflag, expireday, remark, entityid, vernum, recorder, reccreator, recupdator, skipflag ,flowlanguage) 
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
