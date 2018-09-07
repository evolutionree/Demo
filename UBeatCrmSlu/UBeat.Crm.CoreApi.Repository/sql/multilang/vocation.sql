drop FUNCTION "public"."crm_func_vocation_update"("_vocationid" uuid, "_vocationname" text, "_description" text, "_userno" int4);
drop FUNCTION "public"."crm_func_vocation_update"("_vocationid" uuid, "_vocationname" text, "_description" text, "_userno" int4, "_vocationlanguage" jsonb);
alter table crm_sys_vocation add vocationname_lang jsonb;
alter table crm_sys_vocation drop column vocationlanguage;
drop   FUNCTION "public"."crm_func_vocation_insert"("_vocationname" text, "_description" text, "_userno" int4);
CREATE OR REPLACE FUNCTION "public"."crm_func_vocation_update"("_vocationid" uuid, "_vocationname" text, "_description" text, "_userno" int4, "_vocationname_lang" jsonb)
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
					vocationname_lang=_vocationname_lang
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

CREATE OR REPLACE FUNCTION "public"."crm_func_vocation_insert"("_vocationname" text, "_description" text, "_vocationname_lang" jsonb,"_userno" int4)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$

/*


--编辑某个职能的功能

SELECT crm_func_vocation_insert('test night','description content',7);

select* from crm_sys_vocation;
 vocationname,description,recorder,reccreator,recupdator,

 alter table crm_sys_vocation  RENAME COLUMN desciption TO description;
*/

DECLARE
  _vocationid uuid;
  _recorder integer:=0;
  
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

BEGIN
    
	 BEGIN

		IF _vocationname IS NULL OR _vocationname='' THEN
		    Raise EXCEPTION '职能名称不能为空';
		END IF;

		IF EXISTS(SELECT 1 FROM crm_sys_vocation where vocationname=_vocationname AND recstatus=1) THEN
        RAISE EXCEPTION '职能名称已经存在';
		END IF;
		
	        SELECT coalesce(max(recorder),0)+1 INTO _recorder FROM crm_sys_vocation;

		INSERT INTO crm_sys_vocation (vocationname,description,recorder,reccreator,recupdator,vocationname_lang)
		VALUES(_vocationname,_description,_recorder,_userno,_userno,_vocationname_lang) RETURNING vocationid  INTO _vocationid;

		--为职能添加默认的功能
		/*INSERT INTO crm_sys_vocation_function_relation(vocationid,functionid)
		SELECT _vocationid,functionid FROM crm_sys_vocation_function_relation 
		WHERE vocationid='00000000-0000-0000-0000-000000000000';
                */
		

		_codeid:= _vocationid::TEXT;
		_codeflag:= 1;
		_codemsg:= '添加职能成功';    

		   
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
CREATE OR REPLACE FUNCTION "public"."crm_func_vocation_select"("_vocationname" text, "_userno" int4, "_pageindex" int4, "_pagesize" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$

/*
DROP FUNCTION public.crm_func_vocation_select2(text, integer, integer, integer);


--编辑某个职能的功能

SELECT crm_func_vocation_select2('first',7,1,10);
fetch all from datacursor;
fetch all from pagecursor;

SELECT * FROM crm_sys_vocation
 
*/

DECLARE

  _sql TEXT:='';

  --分页标准参数
  _page_sql TEXT;
  _count_sql TEXT;
  _datacursor refcursor:= 'datacursor';
  _pagecursor refcursor:= 'pagecursor';
  

BEGIN
    
	_sql:='  SELECT * FROM crm_sys_vocation  WHERE recstatus=1 ';

	IF _vocationname IS NOT NULL AND _vocationname!='' THEN
	_sql:=_sql||' AND (vocationname ilike ''%'||_vocationname||'%''  or vocationname_lang::text ilike ''%' || _vocationname|| '%'' ) ';
	END IF;

	_sql:=_sql||' ORDER BY reccreated desc';


	Raise Notice 'the sql:%',_sql;


	--查询分页数据
	SELECT * FROM crm_func_paging_sql_fetch(_sql, _pageindex, _pagesize) INTO _page_sql,_count_sql;

	OPEN _datacursor FOR EXECUTE _page_sql ;
	RETURN NEXT _datacursor;

	OPEN _pagecursor FOR EXECUTE _count_sql ;
	RETURN NEXT _pagecursor;
	

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;

ALTER FUNCTION "public"."crm_func_vocation_select"("_vocationname" text, "_userno" int4, "_pageindex" int4, "_pagesize" int4) OWNER TO "postgres";