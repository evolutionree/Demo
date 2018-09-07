CREATE
OR REPLACE FUNCTION "public"."crm_func_entity_field_edit" (
	"_entityid" VARCHAR,
	"_fieldid" VARCHAR,
	"_fieldlabel" VARCHAR,
	"_displayname" VARCHAR,
	"_fieldconfig" VARCHAR,
	"_fieldtype" int4,
	"_status" int4,
	"_controltype" int4,
	"_userno" int4,
	"_fieldlanguage" jsonb,
	"_displaylanguage" jsonb
) RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
DECLARE _orderby int4 ; _entitytable TEXT ; _fieldname TEXT ; --标准返回参数
_codeid TEXT ; _codeflag INT := 0 ; _codemsg TEXT ; _codestack TEXT ; _codestatus TEXT ;
BEGIN

BEGIN
	RAISE NOTICE '%',
	_fieldid ; --数据逻辑
IF _entityid IS NULL
OR _entityid = '' THEN
	Raise EXCEPTION '实体Id不能为空' ;
END
IF ;
IF NOT EXISTS (
	SELECT
		1
	FROM
		crm_sys_entity_fields
	WHERE
		fieldid = _fieldid :: uuid
	LIMIT 1
) THEN
	Raise EXCEPTION '字段不存在' ;
END
IF ;
IF EXISTS (
	SELECT
		1
	FROM
		crm_sys_entity_fields
	WHERE
		entityid = _entityid :: uuid
	AND fieldid <> _fieldid :: uuid
	AND fieldlabel = _fieldlabel
	AND recstatus = 1
	LIMIT 1
) THEN
	Raise EXCEPTION '字段名称不能重复' ;
END
IF ; --raise exception  '_fieldlabel:%',_fieldlabel;
UPDATE crm_sys_entity_fields
SET fieldlabel = _fieldlabel,
 displayname = _displayname,
 controltype = _controltype,
 fieldtype = _fieldtype,
 fieldconfig = _fieldconfig :: jsonb,
 recstatus = _status,
 recupdator = _userno,
fieldlanguage=_fieldlanguage,
displaylanguage=_displaylanguage
WHERE
	fieldid = _fieldid :: uuid ; _codeid := _fieldid :: TEXT ; _codeflag := 1 ; _codemsg := '新增实体成功' ; EXCEPTION
WHEN OTHERS THEN
	GET STACKED DIAGNOSTICS _codestack = PG_EXCEPTION_CONTEXT ; _codemsg := SQLERRM ; _codestatus := SQLSTATE ;
END ; --RETURN RESULT
RETURN QUERY EXECUTE format ('SELECT $1,$2,$3,$4,$5') USING _codeid,
 _codeflag,
 _codemsg,
 _codestack,
 _codestatus ;
END $BODY$ LANGUAGE 'plpgsql' VOLATILE COST 100 ROWS 1000;

