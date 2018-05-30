CREATE OR REPLACE FUNCTION "public"."crm_func_dynamics_detail_insert"("_dynamictype" int4, "_entityid" uuid, "_businessid" uuid, "_typeid" uuid, "_typerecid" uuid, "_jsonstring" text, "_content" text, "_userno" int4)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
DECLARE
  _dynamicid uuid:=NULL;
	_typeentityid uuid:=NULL;
	_templateid uuid:=NULL;
  _typeidtemp uuid:=cast('00000000-0000-0000-0000-000000000000' as uuid);
  _jsondata jsonb:=NULL;

  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;
BEGIN
	BEGIN
					--默认类型的动态
					IF _dynamictype=0 OR _dynamictype=1 THEN
							IF _content IS NULL OR _content='' THEN
										Raise EXCEPTION 'content不可为空';
							END IF;
							--_entityid:=cast('00000000-0000-0000-0000-000000000000' as uuid);
					ELSE
							IF _entityid IS NULL THEN
										Raise EXCEPTION '实体ID不可为空';
							END IF;

							IF _jsonstring IS NULL OR _jsonstring='' THEN
										Raise EXCEPTION 'jsondata不可为空';
							ELSE
										_jsondata:=_jsonstring::jsonb;
							END IF;

              --Raise Notice 'Find Template:%  %',_entityid,_typeid;
              SELECT templateid INTO _templateid FROM crm_sys_dynamic_template where recstatus=1 AND  entityid=_typeid ORDER BY reccreated DESC LIMIT 1;
              IF _templateid IS NULL THEN
                  Raise EXCEPTION '没有配置该实体的动态模板';
              END IF;
					END IF;


					 --数据逻辑
					 INSERT INTO crm_sys_dynamics (entityid,businessid,templateid,tempdata,dynamictype,content,typeid,recstatus,reccreator,recupdator) 
					 VALUES (_entityid,_businessid,_templateid,_jsondata,_dynamictype,_content,_typeid,1 ,_userno,_userno) RETURNING dynamicid INTO _dynamicid;

					_codeid:= _dynamicid::TEXT;
					_codeflag:= 1;
					_codemsg:= '发布动态成功';
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
