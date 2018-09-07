CREATE OR REPLACE FUNCTION "public"."crm_func_workflow_update"("_flowid" uuid, "_flowname" text, "_backflag" int4, "_resetflag" int4, "_expireday" int4, "_remark" text, "_skipflag" int4, "_userno" int4,_flowlanguage jsonb)
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
					flowlanguage = _flowlanguage
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

