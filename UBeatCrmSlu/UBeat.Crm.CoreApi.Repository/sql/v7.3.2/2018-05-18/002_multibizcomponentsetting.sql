CREATE OR REPLACE FUNCTION "public"."crm_func_entity_add_column"("_tablename" varchar, "_columnname" varchar, "_columncomment" varchar, "_controltype" int4)
  RETURNS "pg_catalog"."bool" AS $BODY$
DECLARE
_execute_sql TEXT;
_column_type TEXT;

_extra_sql TEXT:='';

_affected INT:=0;
_field_rules_sql TEXT:='';
_typeid uuid:=null;
_entityid uuid:=null;
_fieldid uuid:=null;
_r record;
BEGIN

   CASE _controltype
   WHEN 1 THEN
      --文本
      _column_type:='TEXT';
   WHEN 2 THEN
				--提示文本（不需要建字段）
			 RETURN TRUE;
   WHEN 3 THEN
      --本地字段单选
      _column_type:='INT';
    WHEN 4 THEN
      --本地字段多选
      _column_type:='TEXT';
    WHEN 5 THEN
      --大文本
      _column_type:='TEXT';
    WHEN 6 THEN
      --整数
      _column_type:='INT';
    WHEN 7 THEN
      --小数
      _column_type:='DECIMAL';
   WHEN 8 THEN
      --日期
      _column_type:='DATE';
   WHEN 9 THEN
      --时间
      _column_type:='TIMESTAMP';
   WHEN 10 THEN
      --手机号
      _column_type:='TEXT';
   WHEN 11 THEN
      --邮箱地址
      _column_type:='TEXT';
   WHEN 12 THEN
      --电话
      _column_type:='TEXT';
   WHEN 13 THEN
      --地址
      _column_type:='JSONB';
   WHEN 14 THEN
      --定位
      _column_type:='JSONB';
   WHEN 15 THEN
      --头像
      _column_type:='TEXT';
   WHEN 16 THEN
      --行政区域
      _column_type:='INT';
   WHEN 17 THEN
      --团队组织
      _column_type:='TEXT';
   WHEN 18 THEN
      --数据源单选
      _column_type:='JSONB';
   WHEN 19 THEN
      --数据源多选
      _column_type:='JSONB';
   WHEN 20 THEN
      --分组（不需要创建字段）
      RETURN TRUE;
   WHEN 21 THEN
      --树形
      _column_type:='INT';
   WHEN 22 THEN
      --拍照
      _column_type:='TEXT';
   WHEN 23 THEN
      --附件
      _column_type:='TEXT';
   WHEN 24 THEN
      --表格
      _column_type:='TEXT';
   WHEN 25 THEN
      --单选人
      _column_type:='TEXT';
   WHEN 26 THEN
      --多选人
      _column_type:='TEXT';
  WHEN 27 THEN
      --树形多选
      _column_type:='TEXT';
  WHEN 28 THEN
      --产品
      _column_type:='TEXT';
  WHEN 29 THEN
      --产品系列
      _column_type:='TEXT';
  WHEN 30 THEN
      --引用对象
      _column_type:='UUID';
	when 32 then 
			_column_type:='JSONB';
   ELSE
      Raise EXCEPTION '暂未实现的字段类型';
   END CASE;   

    IF _columnname = 'reccode' AND _controltype = 1 THEN
           --生成流水号
           EXECUTE format('CREATE SEQUENCE %s_serial_code',_tablename);
          _extra_sql:=format('
					NOT NULL DEFAULT (((date_part(''year''::text, (''now''::text)::date) || ''0000000''::text))::bigint + nextval(''%s_serial_code''::regclass))
           ',_tablename);
    END IF;

   _execute_sql:='
        ALTER TABLE %s ADD COLUMN %s %s %s;
				COMMENT ON COLUMN %s.%s IS ''%s'';
    ';
    Raise Notice '%',_execute_sql;
    
    EXECUTE format(_execute_sql,_tablename,_columnname,_column_type,_extra_sql,_tablename,_columnname,_columncomment);
    GET DIAGNOSTICS _affected = ROW_COUNT;
    SELECT entityid INTO _entityid FROM crm_sys_entity WHERE entitytable=_tablename LIMIT 1;
    SELECT fieldid INTO _fieldid FROM crm_sys_entity_fields WHERE entityid=_entityid AND fieldname=_columnname LIMIT 1;
raise EXCEPTION '%','asd';
    FOR _r IN SELECT categoryid FROM crm_sys_entity_category WHERE entityid=_entityid LOOP
				_field_rules_sql:='INSERT INTO crm_sys_entity_field_rules (typeid,fieldid, operatetype, isrequire, isvisible, isreadonly, viewrules, validrules, relaterules, recorder, reccreator, recupdator) 
													 VALUES ($1,$2, ''0'', ''0'', ''1'', ''0'', ''{"style":0,"isVisible":0,"isReadOnly":0}'', ''{"isRequired":0}'', ''{}'', ''1'', $2, $2);
													 INSERT INTO crm_sys_entity_field_rules (typeid,fieldid, operatetype, isrequire, isvisible, isreadonly, viewrules, validrules, relaterules, recorder, reccreator, recupdator) 
													 VALUES ($1,$2, ''1'', ''0'', ''1'', ''0'', ''{"style":0,"isVisible":0,"isReadOnly":0}'', ''{"isRequired":0}'', ''{}'', ''1'', $2, $2);
													 INSERT INTO crm_sys_entity_field_rules (typeid,fieldid, operatetype, isrequire, isvisible, isreadonly, viewrules, validrules, relaterules, recorder, reccreator, recupdator) 
													 VALUES ($1,$2, ''2'', ''0'', ''1'', ''1'', ''{"style":0,"isVisible":1,"isReadOnly":1}'', ''{"isRequired":0}'', ''{}'', ''3'', $2, $2);';
				EXECUTE _field_rules_sql
				USING _r.categoryid,_fieldid,_userno;
    END LOOP;
    RETURN _affected > 0;
END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
;

CREATE OR REPLACE FUNCTION "public"."crm_func_entity_add_column"("_tablename" varchar, "_columnname" varchar, "_columncomment" varchar, "_controltype" int4, "_userno" int4)
  RETURNS "pg_catalog"."bool" AS $BODY$
DECLARE
_execute_sql TEXT;
_column_type TEXT;

_extra_sql TEXT:='';

_affected INT:=0;
_field_rules_sql TEXT:='';
_typeid uuid:=null;
_entityid uuid:=null;
_fieldid uuid:=null;
BEGIN

   CASE _controltype
   WHEN 1 THEN
      --文本
      _column_type:='TEXT';
   WHEN 2 THEN
      --文本
      _column_type:='TEXT';
   WHEN 3 THEN
      --本地字段单选
      _column_type:='INT';
    WHEN 4 THEN
      --本地字段多选
      _column_type:='TEXT';
    WHEN 5 THEN
      --大文本
      _column_type:='TEXT';
    WHEN 6 THEN
      --整数
      _column_type:='INT8';
    WHEN 7 THEN
      --小数
      _column_type:='DECIMAL';
   WHEN 8 THEN
      --日期
      _column_type:='DATE';
   WHEN 9 THEN
      --时间
      _column_type:='TIMESTAMP';
   WHEN 10 THEN
      --手机号
      _column_type:='TEXT';
   WHEN 11 THEN
      --邮箱地址
      _column_type:='TEXT';
   WHEN 12 THEN
      --电话
      _column_type:='TEXT';
   WHEN 13 THEN
      --地址
      _column_type:='JSONB';
   WHEN 14 THEN
      --定位
      _column_type:='JSONB';
   WHEN 15 THEN
      --头像
      _column_type:='TEXT';
   WHEN 16 THEN
      --行政区域
      _column_type:='INT';
   WHEN 17 THEN
      --团队组织
      _column_type:='TEXT';
   WHEN 18 THEN
      --数据源单选
      _column_type:='JSONB';
   WHEN 19 THEN
      --数据源多选
      _column_type:='JSONB';
   WHEN 20 THEN
      --树形
      _column_type:='INT';
   WHEN 21 THEN
      --树形
      _column_type:='INT';
   WHEN 22 THEN
      --拍照
      _column_type:='TEXT';
   WHEN 23 THEN
      --附件
      _column_type:='TEXT';
   WHEN 24 THEN
      --表格
      _column_type:='TEXT';
   WHEN 25 THEN
      --单选人
      _column_type:='TEXT';
   WHEN 26 THEN
      --多选人
      _column_type:='TEXT';
  WHEN 27 THEN
      --树形多选
      _column_type:='TEXT';
  WHEN 28 THEN
      --产品
      _column_type:='TEXT';
  WHEN 29 THEN
      --产品系列
      _column_type:='TEXT';
  WHEN 31 THEN
      --引用对象
      _column_type:='UUID';
	when 32 then 
			_column_type:='JSONB';
   ELSE
      Raise EXCEPTION '暂未实现的字段类型';
   END CASE;   

    IF _columnname = 'reccode' AND _controltype = 1 THEN
           --生成流水号
           EXECUTE format('CREATE SEQUENCE %s_serial_code',_tablename);
          _extra_sql:=format('
					NOT NULL DEFAULT (((date_part(''year''::text, (''now''::text)::date) || ''0000000''::text))::bigint + nextval(''%s_serial_code''::regclass))
           ',_tablename);
    END IF;
   
		 _execute_sql:='
					ALTER TABLE %s ADD COLUMN %s %s %s;
					COMMENT ON COLUMN %s.%s IS ''%s'';
			';
			SELECT entityid INTO _entityid FROM crm_sys_entity WHERE entitytable=_tablename LIMIT 1;
			SELECT fieldid INTO _fieldid FROM crm_sys_entity_fields WHERE entityid=_entityid AND fieldname=_columnname LIMIT 1;
			IF NOT EXISTS  (SELECT 1
								FROM pg_attribute AS a 
								WHERE a.attnum > 0
								AND NOT a.attisdropped
								AND a.attrelid = _tablename::regclass AND a.attname=_columnname) THEN
 
							EXECUTE format(_execute_sql,_tablename,_columnname,_column_type,_extra_sql,_tablename,_columnname,_columncomment);

							GET DIAGNOSTICS _affected = ROW_COUNT;
			 ELSE
					 _affected=1;
			 END IF;

    RETURN _affected > 0;
END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
;


