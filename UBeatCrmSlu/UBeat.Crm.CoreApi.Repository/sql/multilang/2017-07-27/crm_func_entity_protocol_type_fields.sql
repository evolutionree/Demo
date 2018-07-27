CREATE OR REPLACE FUNCTION "public"."crm_func_entity_protocol_type_fields"("_typeid" uuid, "_operatetype" int4, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_entity_protocol_type_fields('e1d67c20-a648-4aec-8064-2c27df720e1b', 0, 1);
--FETCH ALL FROM fieldcursor;

DECLARE 
  _field_sql TEXT;
	_entityid uuid;
  _fieldcursor refcursor:= 'fieldcursor';

BEGIN

    IF _typeid IS NULL THEN
          Raise EXCEPTION '实体类型ID不能为空';
    END IF;    
		SELECT entityid INTO _entityid FROM crm_sys_entity_category WHERE categoryid = _typeid LIMIT 1;
    _field_sql:='
							WITH
							TUserVocation AS (
										SELECT * FROM crm_sys_userinfo_vocation_relate WHERE userid = $1
							),
							TTypeField AS (
									 SELECT r.fieldid,f.fieldlabel,f.fieldname,f.displayname,f.controltype,f.fieldtype,f.recorder,r.typeid,r.isrequire,r.isvisible,r.isreadonly,(r.viewrules || r.relaterules || f.fieldconfig) AS fieldconfig
									 ,f.expandjs,f.filterjs,f.fieldlabel_lang,f.displayname_lang FROM crm_sys_entity_field_rules AS r
									 INNER JOIN crm_sys_entity_fields AS f ON r.fieldid = f.fieldid
									 WHERE f.recstatus = 1 AND
									 r.recstatus = 1 AND r.typeid = $2 and f.entityid = $4 AND r.operatetype = $3
							),
							TFieldVocation AS 
							(
									 SELECT * FROM crm_sys_entity_field_rules_vocation 
									 WHERE entityid = $4  AND vocationid IN (
													SELECT vocationid FROM TUserVocation
									 ) AND  operatetype =$3 
							),
							TFieldCalulate AS (
										SELECT fieldid,recstatus,
										(CASE WHEN isvisible > 0 THEN 1 ELSE 0 END) AS isvisible, 
										(CASE WHEN isreadonly > 0 THEN 1 ELSE 0 END) AS isreadonly
										FROM (
												SELECT fieldid,recstatus,
												COUNT(CASE WHEN isvisible = 1 THEN 1 ELSE NULL END) AS isvisible,
												COUNT(CASE WHEN isreadonly = 1 THEN 1 ELSE NULL END) AS isreadonly
												FROM TFieldVocation
												GROUP BY fieldid,recstatus
										) AS e
							)
							SELECT t.fieldid,t.typeid,t.fieldlabel,t.fieldname,t.displayname,t.controltype,t.fieldtype,t.isrequire,t.isvisible,t.isreadonly,json_object_update_key(json_object_update_key(t.fieldconfig::json,''isVisible'',t.isvisible::int4),''isReadOnly'',t.isreadonly::int4) AS fieldconfig,t.recorder,t.expandjs,t.filterjs,jsonb_extract_path_text(fieldconfig,''defaultValue'') AS defaultvalue FROM(
							SELECT f.fieldid,f.typeid,f.fieldlabel,f.fieldname,f.displayname,f.controltype,f.fieldtype,f.isrequire,CASE WHEN f.isvisible=1 THEN c.isvisible ELSE f.isvisible END isvisible,CASE WHEN f.isreadonly=1 THEN f.isreadonly ELSE c.isreadonly END isreadonly ,f.fieldconfig,f.recorder,f.expandjs,f.filterjs,f.fieldlabel_lang,f.displayname_lang FROM TTypeField AS f
							INNER JOIN TFieldCalulate AS c ON c.fieldid = f.fieldid  AND c.recstatus=1
							UNION
							SELECT f.fieldid,f.typeid,f.fieldlabel,f.fieldname,f.displayname,f.controltype,f.fieldtype,f.isrequire,f.isvisible,f.isreadonly,f.fieldconfig,f.recorder,f.expandjs,f.filterjs,f.fieldlabel_lang,f.displayname_lang FROM TTypeField AS f
							WHERE f.fieldid NOT IN (SELECT fieldid FROM TFieldCalulate)
							) AS t  ORDER BY t.recorder 
    ';
	raise notice '%',_field_sql;
  OPEN _fieldcursor FOR EXECUTE _field_sql USING _userno,_typeid,_operatetype,_entityid;
	RETURN NEXT _fieldcursor;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;