CREATE OR REPLACE FUNCTION "public"."crm_func_vocation_update"("_vocationid" uuid, "_vocationname" text, "_description" text, "_userno" int4,_vocationlanguage jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$

/*


--编辑某个职能的功能

SELECT crm_func_vocation_update('db935117-d650-4bcf-995b-5553913400a8','dddd','description content',7);

select* from crm_sys_vocation;
 vocationname,description,recorder,reccreator,recupdator,

 alter table crm_sys_vocation  RENAME COLUMN desciption TO description;
*/

DECLARE
  _recorder integer:=0;
  
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

BEGIN
    
	 BEGIN

	  IF _vocationid IS NULL THEN
		   RAISE EXCEPTION '职能id不能为空';
		END IF;

		IF _vocationname IS NULL OR _vocationname='' THEN
		    Raise EXCEPTION '职能名称不能为空';
		END IF;

		IF EXISTS(SELECT 1 FROM crm_sys_vocation where vocationname=_vocationname AND vocationid!=_vocationid AND recstatus=1) THEN
        RAISE EXCEPTION '职能名称已经存在';
		END IF;
		

		UPDATE crm_sys_vocation
	        SET vocationname=_vocationname,
	        description=_description,
	        recupdator=_userno,
	        recupdated=now(),
vocationlanguage=_vocationlanguage
	       WHERE vocationid=_vocationid;


		_codeid:= _vocationid::TEXT;
		_codeflag:= 1;
		_codemsg:= '编辑职能成功';    

		   
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

