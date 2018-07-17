CREATE OR REPLACE FUNCTION "public"."crm_func_entity_field_add"("_entityid" varchar, "_fieldlable" varchar, "_displayname" varchar, "_fieldname" varchar, "_fieldconfig" varchar, "_fieldtype" int4, "_status" int4, "_controltype" int4, "_userno" int4, "_fieldlanguage" jsonb, "_displaylanguage" jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$ -- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE _orderby int4 ; _entitytable TEXT ; _fieldid TEXT ; _modeltype INT4 ; _relentityid uuid ; _relfieldid uuid ; _relfieldname TEXT ; _r record ; _field_rules_sql TEXT ; --标准返回参数
_codeid TEXT ; _codeflag INT := 0 ; _codemsg TEXT ; _codestack TEXT ; _codestatus TEXT ;
BEGIN

BEGIN
	--数据逻辑
IF _entityid IS NULL
OR _entityid = '' THEN
	Raise EXCEPTION '实体Id不能为空' ;
END
IF ;
IF EXISTS (
	SELECT
		1
	FROM
		crm_sys_entity_fields
	WHERE
		fieldlabel = _fieldlable
	AND entityid = _entityid :: uuid
	AND recstatus = 1
	LIMIT 1
) THEN
	Raise EXCEPTION '字段名称不能重复' ;
END
IF ;
IF EXISTS (
	SELECT
		1
	FROM
		crm_sys_entity_fields
	WHERE
		fieldname = _fieldname
	AND entityid = _entityid :: uuid
	AND recstatus = 1
	LIMIT 1
) THEN
	Raise EXCEPTION '字段列明不能重复' ;
END
IF ; SELECT
	entitytable INTO _entitytable
FROM
	crm_sys_entity
WHERE
	entityid = _entityid :: uuid
LIMIT 1 ;
IF EXISTS (
	SELECT
		1
	FROM
		information_schema. COLUMNS
	WHERE
		table_schema = 'public'
	AND TABLE_NAME = _entitytable
	AND TABLE_NAME NOT IN (
		SELECT
			entitytable
		FROM
			crm_sys_entity_special_table
	)
	AND COLUMN_NAME = _fieldname
) THEN
	Raise EXCEPTION '该字段已经存在数据库表中,不能重复添加' ;
END
IF ; SELECT
	modeltype,
	relentityid,
	relfieldid,
	relfieldname INTO _modeltype,
	_relentityid,
	_relfieldid,
	_relfieldname
FROM
	crm_sys_entity
WHERE
	entityid = _entityid :: uuid
LIMIT 1 ;
IF _modeltype = 3
AND _relentityid IS NOT NULL
AND _relfieldid IS NOT NULL THEN

IF _relfieldname = _fieldname THEN
	Raise EXCEPTION '该字段名称不能与关联实体的字段重复' ;
END
IF ;
END
IF ; SELECT
	COALESCE (MAX(recorder), 0) + 1 INTO _orderby
FROM
	crm_sys_entity_fields
WHERE
	entityid = _entityid :: uuid ; INSERT INTO crm_sys_entity_fields (
		fieldname,
		entityid,
		fieldlabel,
		displayname,
		controltype,
		fieldtype,
		fieldconfig,
		recorder,
		recstatus,
		reccreator,
		recupdator,
		fieldname_lang,
		displayname_lang
	)
VALUES
	(
		_fieldname,
		_entityid :: uuid,
		_fieldlable,
		_displayname,
		_controltype,
		_fieldtype,
		_fieldconfig :: jsonb,
		_orderby,
		_status,
		_userno,
		_userno,
		_fieldlanguage,
		_displaylanguage
	) RETURNING fieldid INTO _fieldid ; SELECT
		entitytable INTO _entitytable
	FROM
		crm_sys_entity
	WHERE
		entityid = _entityid :: uuid ; perform crm_func_entity_add_column (
			_entitytable,
			_fieldname,
			_fieldlable,
			_controltype,
			_userno
		) ;
	IF _controltype != 2
	AND _controltype != 20 THEN
		FOR _r IN SELECT
			categoryid
		FROM
			crm_sys_entity_category
		WHERE
			entityid = _entityid :: uuid LOOP _field_rules_sql := 'INSERT INTO crm_sys_entity_field_rules (typeid,fieldid, operatetype, isrequire, isvisible, isreadonly, viewrules, validrules, relaterules, recorder, reccreator, recupdator) 
																	 VALUES ($1,$2, ''0'', ''0'', ''1'', ''0'', ''{"style":0,"isVisible":1,"isReadOnly":0}'', ''{"isRequired":0}'', ''{}'', ''1'', $3, $3);
																	 INSERT INTO crm_sys_entity_field_rules (typeid,fieldid, operatetype, isrequire, isvisible, isreadonly, viewrules, validrules, relaterules, recorder, reccreator, recupdator) 
																	 VALUES ($1,$2, ''1'', ''0'', ''1'', ''0'', ''{"style":0,"isVisible":1,"isReadOnly":0}'', ''{"isRequired":0}'', ''{}'', ''2'', $3, $3);
																	 INSERT INTO crm_sys_entity_field_rules (typeid,fieldid, operatetype, isrequire, isvisible, isreadonly, viewrules, validrules, relaterules, recorder, reccreator, recupdator) 
																	 VALUES ($1,$2, ''2'', ''0'', ''1'', ''1'', ''{"style":0,"isVisible":1,"isReadOnly":1}'', ''{"isRequired":0}'', ''{}'', ''3'', $3, $3);' ; EXECUTE _field_rules_sql USING _r.categoryid,
			_fieldid :: uuid,
			_userno ;
		END LOOP ;
		ELSE
			FOR _r IN SELECT
				categoryid
			FROM
				crm_sys_entity_category
			WHERE
				entityid = _entityid :: uuid LOOP _field_rules_sql := 'INSERT INTO crm_sys_entity_field_rules (typeid,fieldid, operatetype, isrequire, isvisible, isreadonly, viewrules, validrules, relaterules, recorder, reccreator, recupdator) 
																 VALUES ($1,$2, ''0'', ''0'', ''1'', ''0'', ''{"style":0,"isVisible":1,"isReadOnly":0}'', ''{"isRequired":0}'', ''{}'', ''1'', $3, $3);
																 INSERT INTO crm_sys_entity_field_rules (typeid,fieldid, operatetype, isrequire, isvisible, isreadonly, viewrules, validrules, relaterules, recorder, reccreator, recupdator) 
																 VALUES ($1,$2, ''1'', ''0'', ''1'', ''0'', ''{"style":0,"isVisible":1,"isReadOnly":0}'', ''{"isRequired":0}'', ''{}'', ''2'', $3, $3);' ; EXECUTE _field_rules_sql USING _r.categoryid,
				_fieldid :: uuid,
				_userno ;
			END LOOP ;
			END
			IF ; _codeid := _fieldid :: TEXT ; _codeflag := 1 ; _codemsg := '新增字段成功' ; EXCEPTION
			WHEN OTHERS THEN
				GET STACKED DIAGNOSTICS _codestack = PG_EXCEPTION_CONTEXT ; _codemsg := SQLERRM ; _codestatus := SQLSTATE ;
			END ; --RETURN RESULT
			RETURN QUERY EXECUTE format ('SELECT $1,$2,$3,$4,$5') USING _codeid,
			_codeflag,
			_codemsg,
			_codestack,
			_codestatus ;
		END $BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;

