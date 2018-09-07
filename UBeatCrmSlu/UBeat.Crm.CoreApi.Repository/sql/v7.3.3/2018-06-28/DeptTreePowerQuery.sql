CREATE OR REPLACE FUNCTION "public"."crm_func_department_tree_power_bak"(IN "_deptid" uuid, IN "_status" int4, IN "_direction" int4, IN "_userno" int4)
  RETURNS TABLE("recstatus" int4, "deptid" uuid, "deptname" text, "recorder" int4, "ancestor" uuid, "descendant" uuid, "nodepath" int4, "nodes" int4)  AS $BODY$
DECLARE

_sql TEXT:='';
_entityid uuid:='3d77dfd2-60bb-4552-bb69-1c3e73cf4095'::uuid;
_role_power_sql TEXT;
_rule_power_sql TEXT;
_where TEXT;
BEGIN
	  IF EXISTS(SELECT 1 FROM crm_sys_entity WHERE entityid=_entityid LIMIT 1) THEN
				-- _role_power_sql:=crm_func_role_rule_fetch_sql(_entityid,_userno);
			--	 _rule_power_sql:=crm_func_role_rule_param_format(_role_power_sql,_userno);
        -- _rule_power_sql:=replace(_rule_power_sql,'AND recstatus = 1','');
         _rule_power_sql:='select * from crm_sys_department';--这里这样改动主要考虑禁用后的数据的处理情况
    ELSE
         _rule_power_sql:='select * from crm_sys_department';
    END IF;
    raise notice '%',_rule_power_sql;
    IF _status=1 THEN
       _where:=' and recstatus=1';
    END IF;
	  CASE _direction
					WHEN 0 THEN
                --UPPER
						    _sql =format('SELECT c.recstatus,c.deptid,c.deptname,c.recorder,c.pdeptid AS ancestor,t.descendant,t.nodepath,
                           (SELECT COUNT(1)-1 FROM crm_sys_department_treepaths AS s WHERE s.ancestor = t.descendant AND EXISTS(SELECT 1 FROM crm_sys_department AS n WHERE n.deptid = s.descendant AND n.recstatus = 1 LIMIT 1))::INT AS nodes
                           FROM (
                                   %s
                                ) AS c
													 INNER JOIN crm_sys_department_treepaths t on c.deptid = t.ancestor
													 WHERE  t.descendant = $1  %s  Order by c.recorder',_rule_power_sql,_where);
          WHEN 1 THEN
                 --DOWNER
								 _sql = format('SELECT c.recstatus,c.deptid,c.deptname,c.recorder,c.pdeptid AS ancestor,t.descendant,t.nodepath,
													(SELECT COUNT(1)-1 FROM crm_sys_department_treepaths AS s WHERE s.ancestor = t.descendant AND EXISTS(SELECT 1 FROM crm_sys_department AS n WHERE n.deptid = s.descendant AND n.recstatus = 1 LIMIT 1))::INT AS nodes
                            FROM (
                                   %s
                                ) AS c
														INNER JOIN crm_sys_department_treepaths t on c.deptid = t.descendant
														WHERE  t.ancestor = $1   Order by c.recorder',_rule_power_sql,_where);
					ELSE 
                Raise EXCEPTION '未知的排序类型';
    END CASE;
	raise notice '%',_sql;
		 --标准返回
		 RETURN QUERY EXECUTE format(_sql)
		 USING  _deptid;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;




drop function "crm_func_department_tree_power"(IN "_deptid" uuid, IN "_status" int4, IN "_direction" int4, IN "_userno" int4);





CREATE OR REPLACE FUNCTION "public"."crm_func_department_tree_power"(IN "_deptid" uuid, IN "_status" int4, IN "_direction" int4, IN "_userno" int4)
  RETURNS TABLE("recstatus" int4, "deptid" uuid, "deptname" text, "recorder" int4, "ancestor" uuid, "descendant" uuid, "nodepath" int4, "nodes" int4,deptlanguage jsonb)  AS $BODY$
DECLARE

_sql TEXT:='';
_entityid uuid:='3d77dfd2-60bb-4552-bb69-1c3e73cf4095'::uuid;
_role_power_sql TEXT;
_rule_power_sql TEXT;
_where TEXT;
BEGIN
	  IF EXISTS(SELECT 1 FROM crm_sys_entity WHERE entityid=_entityid LIMIT 1) THEN
				-- _role_power_sql:=crm_func_role_rule_fetch_sql(_entityid,_userno);
			--	 _rule_power_sql:=crm_func_role_rule_param_format(_role_power_sql,_userno);
        -- _rule_power_sql:=replace(_rule_power_sql,'AND recstatus = 1','');
         _rule_power_sql:='select * from crm_sys_department';--这里这样改动主要考虑禁用后的数据的处理情况
    ELSE
         _rule_power_sql:='select * from crm_sys_department';
    END IF;
    raise notice '%',_rule_power_sql;
    IF _status=1 THEN
       _where:=' and recstatus=1';
    END IF;
	  CASE _direction
					WHEN 0 THEN
                --UPPER
						    _sql =format('SELECT c.recstatus,c.deptid,c.deptname,c.recorder,c.pdeptid AS ancestor,t.descendant,t.nodepath,
                           (SELECT COUNT(1)-1 FROM crm_sys_department_treepaths AS s WHERE s.ancestor = t.descendant AND EXISTS(SELECT 1 FROM crm_sys_department AS n WHERE n.deptid = s.descendant AND n.recstatus = 1 LIMIT 1))::INT AS nodes,
c.deptlanguage
                           FROM (
                                   %s
                                ) AS c
													 INNER JOIN crm_sys_department_treepaths t on c.deptid = t.ancestor
													 WHERE  t.descendant = $1  %s  Order by c.recorder',_rule_power_sql,_where);
          WHEN 1 THEN
                 --DOWNER
								 _sql = format('SELECT c.recstatus,c.deptid,c.deptname,c.recorder,c.pdeptid AS ancestor,t.descendant,t.nodepath,
													(SELECT COUNT(1)-1 FROM crm_sys_department_treepaths AS s WHERE s.ancestor = t.descendant AND EXISTS(SELECT 1 FROM crm_sys_department AS n WHERE n.deptid = s.descendant AND n.recstatus = 1 LIMIT 1))::INT AS nodes,
c.deptlanguage
                            FROM (
                                   %s
                                ) AS c
														INNER JOIN crm_sys_department_treepaths t on c.deptid = t.descendant
														WHERE  t.ancestor = $1   Order by c.recorder',_rule_power_sql,_where);
					ELSE 
                Raise EXCEPTION '未知的排序类型';
    END CASE;
	raise notice '%',_sql;
		 --标准返回
		 RETURN QUERY EXECUTE format(_sql)
		 USING  _deptid;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;

