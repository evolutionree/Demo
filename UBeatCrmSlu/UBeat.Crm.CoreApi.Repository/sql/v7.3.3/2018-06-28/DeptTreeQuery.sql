CREATE OR REPLACE FUNCTION "public"."crm_func_department_tree_bak"(IN "_deptid" uuid, IN "_direction" int4)
  RETURNS TABLE("deptid" uuid, "deptname" text, "recorder" int4, "ancestor" uuid, "descendant" uuid, "nodepath" int4, "nodes" int4)  AS $BODY$
DECLARE

_sql TEXT:='';
BEGIN
	  CASE _direction
					WHEN 0 THEN
                --UPPER
						    _sql = 'SELECT c.deptid,c.deptname,c.recorder,c.pdeptid AS ancestor,t.descendant,t.nodepath,
                           (SELECT COUNT(1)-1 FROM crm_sys_department_treepaths AS s WHERE s.ancestor = t.descendant AND EXISTS(SELECT 1 FROM crm_sys_department AS n WHERE n.deptid = s.descendant AND n.recstatus = 1 LIMIT 1))::INT AS nodes,
c.deptlanguage
                           FROM crm_sys_department AS c
													 INNER JOIN crm_sys_department_treepaths t on c.deptid = t.ancestor
													 WHERE c.recstatus = 1 AND t.descendant = $1 ';
          WHEN 1 THEN
                 --DOWNER
								 _sql = 'SELECT c.deptid,c.deptname,c.recorder,c.pdeptid AS ancestor,t.descendant,t.nodepath,(SELECT COUNT(1)-1 FROM crm_sys_department_treepaths AS s WHERE s.ancestor = t.descendant AND EXISTS(SELECT 1 FROM crm_sys_department AS n WHERE n.deptid = s.descendant AND n.recstatus = 1 LIMIT 1))::INT AS nodes,c.deptlanguage
                            FROM crm_sys_department AS c
														INNER JOIN crm_sys_department_treepaths t on c.deptid = t.descendant
														WHERE c.recstatus = 1 AND t.ancestor = $1 ';
	  WHEN 2 THEN
                 --直属下级
								 _sql = 'SELECT c.deptid,c.deptname,c.recorder,c.pdeptid AS ancestor,t.descendant,t.nodepath,(SELECT COUNT(1)-1 FROM crm_sys_department_treepaths AS s WHERE s.ancestor = t.descendant AND EXISTS(SELECT 1 FROM crm_sys_department AS n WHERE n.deptid = s.descendant AND n.recstatus = 1 LIMIT 1))::INT AS nodes,c.deptlanguage
                            FROM crm_sys_department AS c
														INNER JOIN crm_sys_department_treepaths t on c.deptid = t.descendant
														WHERE c.recstatus = 1 AND c.pdeptid = $1 ';
					ELSE 
                Raise EXCEPTION '未知的排序类型';
    END CASE;

		 --标准返回
		 RETURN QUERY EXECUTE format(_sql)
		 USING  _deptid;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;





drop function "crm_func_department_tree"(IN "_deptid" uuid, IN "_direction" int4);




CREATE OR REPLACE FUNCTION "public"."crm_func_department_tree"(IN "_deptid" uuid, IN "_direction" int4)
  RETURNS TABLE("deptid" uuid, "deptname" text, "recorder" int4, "ancestor" uuid, "descendant" uuid, "nodepath" int4, "nodes" int4, deptlanguage jsonb)  AS $BODY$
DECLARE

_sql TEXT:='';
BEGIN
	  CASE _direction
					WHEN 0 THEN
                --UPPER
						    _sql = 'SELECT c.deptid,c.deptname,c.recorder,c.pdeptid AS ancestor,t.descendant,t.nodepath,
                           (SELECT COUNT(1)-1 FROM crm_sys_department_treepaths AS s WHERE s.ancestor = t.descendant AND EXISTS(SELECT 1 FROM crm_sys_department AS n WHERE n.deptid = s.descendant AND n.recstatus = 1 LIMIT 1))::INT AS nodes,
c.deptlanguage
                           FROM crm_sys_department AS c
													 INNER JOIN crm_sys_department_treepaths t on c.deptid = t.ancestor
													 WHERE c.recstatus = 1 AND t.descendant = $1 ';
          WHEN 1 THEN
                 --DOWNER
								 _sql = 'SELECT c.deptid,c.deptname,c.recorder,c.pdeptid AS ancestor,t.descendant,t.nodepath,(SELECT COUNT(1)-1 FROM crm_sys_department_treepaths AS s WHERE s.ancestor = t.descendant AND EXISTS(SELECT 1 FROM crm_sys_department AS n WHERE n.deptid = s.descendant AND n.recstatus = 1 LIMIT 1))::INT AS nodes,c.deptlanguage
                            FROM crm_sys_department AS c
														INNER JOIN crm_sys_department_treepaths t on c.deptid = t.descendant
														WHERE c.recstatus = 1 AND t.ancestor = $1 ';
	  WHEN 2 THEN
                 --直属下级
								 _sql = 'SELECT c.deptid,c.deptname,c.recorder,c.pdeptid AS ancestor,t.descendant,t.nodepath,(SELECT COUNT(1)-1 FROM crm_sys_department_treepaths AS s WHERE s.ancestor = t.descendant AND EXISTS(SELECT 1 FROM crm_sys_department AS n WHERE n.deptid = s.descendant AND n.recstatus = 1 LIMIT 1))::INT AS nodes,c.deptlanguage
                            FROM crm_sys_department AS c
														INNER JOIN crm_sys_department_treepaths t on c.deptid = t.descendant
														WHERE c.recstatus = 1 AND c.pdeptid = $1 ';
					ELSE 
                Raise EXCEPTION '未知的排序类型';
    END CASE;

		 --标准返回
		 RETURN QUERY EXECUTE format(_sql)
		 USING  _deptid;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;

