alter table crm_sys_role_group drop column grouplanguage ;
alter table crm_sys_role_group add rolegroupname_lang jsonb;

alter table crm_sys_role drop column rolelanguage;
alter table crm_sys_role add rolename_lang jsonb;

drop FUNCTION "public"."crm_func_role_group_add"("_groupname" text, "_grouptype" int4, "_userno" int4);
drop FUNCTION "public"."crm_func_role_group_add"("_groupname" text, "_grouptype" int4, "_userno" int4, "_grouplanguage" jsonb);
drop FUNCTION "public"."crm_func_role_group_edit"("_rolegroupid" text, "_groupname" text, "_userno" int4);
drop FUNCTION "public"."crm_func_role_group_edit"("_rolegroupid" text, "_groupname" text, "_userno" int4, "_grouplanguage" jsonb);

drop FUNCTION "public"."crm_func_role_add"("_rolegroupid" text, "_rolename" varchar, "_roletype" int4, "_rolepriority" int4, "_roleremark" varchar, "_userno" int4);
drop FUNCTION "public"."crm_func_role_add"("_rolegroupid" text, "_rolename" varchar, "_roletype" int4, "_rolepriority" int4, "_roleremark" varchar, "_userno" int4, "_rolelanguage" jsonb);
drop FUNCTION "public"."crm_func_role_edit"("_rolegroupid" text, "_roleid" text, "_rolename" text, "_roletype" int4, "_rolepriority" int4, "_roleremark" text, "_userno" int4);
drop FUNCTION "public"."crm_func_role_edit"("_rolegroupid" text, "_roleid" text, "_rolename" text, "_roletype" int4, "_rolepriority" int4, "_roleremark" text, "_userno" int4, "_rolelanguage" jsonb);
drop FUNCTION "public"."crm_func_role_copy"("_roleid" text, "_rolegroupid" text, "_rolename" varchar, "_rolepriority" int4, "_roletype" int4, "_roleremark" varchar, "_userno" int4);


CREATE OR REPLACE FUNCTION "public"."crm_func_role_group_list"("_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
 

DECLARE
  
  _sql_where TEXT:='';
  _execute_sql TEXT;

  _datacursor refcursor:= 'datacursor';
 
BEGIN
 
   _execute_sql:='
        select  
				g.rolegroupid, 
				g.rolegroupname, 
				g.grouptype,
				g.rolegroupname_lang
        from crm_sys_role_group g where recstatus=1;';

    RAISE NOTICE '%',_execute_sql;
 
  OPEN _datacursor FOR EXECUTE _execute_sql;
	RETURN NEXT _datacursor;

 

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;

ALTER FUNCTION "public"."crm_func_role_group_list"("_userno" int4) OWNER TO "postgres";


CREATE OR REPLACE FUNCTION "public"."crm_func_role_group_add"("_groupname" text, "_grouptype" int4, "_userno" int4, "_rolegroupname_lang" jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE
   _rolegroupid uuid;

  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;
  _orderby int4:=0;
BEGIN

   BEGIN
				 --数据逻辑
				 IF _groupname IS NULL OR _groupname = '' THEN
								Raise EXCEPTION '角色分组名称不能为空';
				 END IF;
				 IF EXISTS(SELECT 1 FROM crm_sys_role_group WHERE rolegroupname=_groupname AND recstatus=1 LIMIT 1) THEN
								Raise EXCEPTION '角色分类名称已存在,请修正';
				 END IF;


         select (COALESCE((MAX(recorder)),0)+1) into _orderby from crm_sys_role;
         raise notice '%',_orderby;
				 INSERT INTO crm_sys_role_group (rolegroupname, grouptype,recorder,reccreator,recupdator,rolegroupname_lang) 
				 VALUES (_groupname,_grouptype,_orderby,_userno,_userno,_rolegroupname_lang)  returning rolegroupid into _rolegroupid;

					_codeid:= _rolegroupid::TEXT;
					_codeflag:= 1;
					_codemsg:= '新增角色分组成功';
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


CREATE OR REPLACE FUNCTION "public"."crm_func_role_add"("_rolegroupid" text, "_rolename" varchar, "_roletype" int4, "_rolepriority" int4, "_roleremark" varchar, "_userno" int4, "_rolename_lang" jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE
   _roleid uuid;

  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;
  _orderby int4:=0;
BEGIN

   BEGIN
				 --数据逻辑
				 IF _rolename IS NULL OR _rolename = '' THEN
								Raise EXCEPTION '角色名称不能为空';
				 END IF;

				 IF EXISTS(select 1 from crm_sys_role where rolename=_rolename and recstatus=1)THEN
								Raise EXCEPTION '角色名称已存在';
				 END IF;

         select COALESCE((MAX(recorder)+1),0) into _orderby from crm_sys_role;
         raise notice '%',_orderby;
				 INSERT INTO crm_sys_role (rolename, roletype,rolepriority,roleremark,recorder,reccreator,recupdator,rolename_lang) 
				 VALUES (_rolename,_roletype,_rolepriority,_roleremark,_orderby,_userno,_userno,_rolename_lang)  returning roleid into _roleid;
				 INSERT INTO crm_sys_role_group_relate (rolegroupid, roleid) 
				 VALUES (_rolegroupid::uuid,_roleid);

					_codeid:= _roleid::TEXT;
					_codeflag:= 1;
					_codemsg:= '新增角色成功';
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

ALTER FUNCTION "public"."crm_func_role_add"("_rolegroupid" text, "_rolename" varchar, "_roletype" int4, "_rolepriority" int4, "_roleremark" varchar, "_userno" int4, "_rolelanguage" jsonb) OWNER TO "postgres";


CREATE OR REPLACE FUNCTION "public"."crm_func_role_group_edit"("_rolegroupid" text, "_groupname" text, "_userno" int4, "_groupname_lang" jsonb)
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
				 --数据逻辑
				 IF _groupname IS NULL OR _groupname = '' THEN
								Raise EXCEPTION '角色分组名称不能为空';
				 END IF;
				 IF _rolegroupid IS NULL OR _rolegroupid = '' THEN
								Raise EXCEPTION '角色分组Id不能为空';
				 END IF;
				 IF NOT EXISTS(SELECT 1 FROM crm_sys_role_group WHERE rolegroupid=_rolegroupid::uuid ) THEN
								Raise EXCEPTION '该角色分组不存在';
				 END IF;
				 IF  EXISTS(SELECT 1 FROM crm_sys_role_group WHERE rolegroupid<>_rolegroupid::uuid AND rolegroupname=_groupname AND recstatus=1 ) THEN
								Raise EXCEPTION '该角色分组已存在';
				 END IF;
 
				 UPDATE crm_sys_role_group SET rolegroupname=_groupname,rolegroupname_lang=_groupname_lang WHERE rolegroupid=_rolegroupid::uuid;
 
					_codeid:= _rolegroupid::TEXT;
					_codeflag:= 1;
					_codemsg:= '更新角色分组成功';
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


CREATE OR REPLACE FUNCTION "public"."crm_func_role_edit"("_rolegroupid" text, "_roleid" text, "_rolename" text, "_roletype" int4, "_rolepriority" int4, "_roleremark" text, "_userno" int4, "_rolename_lang" jsonb)
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
  _orderby int4:=0;
BEGIN

   BEGIN
				 --数据逻辑
				 IF _rolename IS NULL OR _rolename = '' THEN
								Raise EXCEPTION '角色名称不能为空';
				 END IF;
				 IF NOT EXISTS(SELECT 1 FROM crm_sys_role WHERE roleid=_roleid::uuid AND recstatus=1 LIMIT 1)THEN
								Raise EXCEPTION '该角色记录不存在';
				 END IF;
				 IF NOT EXISTS(SELECT 1 FROM crm_sys_role_group_relate WHERE roleid=_roleid::uuid  LIMIT 1)THEN
								Raise EXCEPTION '该角色分组关系已存在';
				 END IF;
				 IF EXISTS(SELECT 1 FROM crm_sys_role WHERE roleid<>_roleid::uuid AND rolename=_rolename AND recstatus=1 LIMIT 1)THEN
								Raise EXCEPTION '角色名称已存在';
				 END IF;
         IF NOT EXISTS(SELECT 1 FROM crm_sys_userinfo_role_relate WHERE userid=_userno AND roleid='63dd2a9d-7f75-42ff-a696-7cc841e884e7'::uuid LIMIT 1) THEN
						 IF EXISTS(SELECT 1 FROM crm_sys_role WHERE roleid=_roleid::uuid AND roletype=0 LIMIT 1) THEN
									 SELECT rolename INTO _rolename FROM  crm_sys_role WHERE roleid=_roleid::uuid AND roletype=0 LIMIT 1;
									 Raise EXCEPTION '%',_rolename||'为系统角色,不允许编辑';
						 END IF;
         END IF;
 
         UPDATE crm_sys_role SET
                rolename=_rolename,
                roletype=_roletype,
                rolepriority=_rolepriority,
                roleremark=_roleremark,
                recupdated=now(),
								rolename_lang=_rolename_lang,
                recupdator=_userno where roleid=_roleid::uuid;
         UPDATE crm_sys_role_group_relate SET
                rolegroupid=_rolegroupid::uuid where roleid=_roleid::uuid;
					_codeid:= _roleid;
					_codeflag:= 1;
					_codemsg:= '更新角色成功';
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


CREATE OR REPLACE FUNCTION "public"."crm_func_role_copy"("_roleid" text, "_rolegroupid" text, "_rolename" varchar,"_rolename_lang" jsonb, "_rolepriority" int4, "_roletype" int4, "_roleremark" varchar, "_userno" int4)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE
   _ruleid uuid;
   _newroleid uuid;
   _newruleid uuid;
   _newitemid uuid;
   _r record;
   _r1 record;
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;
  _orderby int4:=0;
BEGIN

   BEGIN
				 --数据逻辑
				 IF _rolename IS NULL OR _rolename = '' THEN
								Raise EXCEPTION '角色名称不能为空';
				 END IF;

				 IF EXISTS(select 1 from crm_sys_role where rolename=_rolename and recstatus=1)THEN
								Raise EXCEPTION '角色名称已存在';
				 END IF;


				 IF NOT EXISTS(select 1 from crm_sys_role where roleid=_roleid::uuid and recstatus=1)THEN
								Raise EXCEPTION '该角色不存在';
				 END IF;

         select (COALESCE(MAX(recorder),0)+1) into _orderby from crm_sys_role;
         raise notice '%',_orderby;

         --复制角色
				 INSERT INTO crm_sys_role (rolename,rolename_lang, roletype,rolepriority,roleremark,recorder,reccreator,recupdator) 
				 SELECT _rolename, _rolename_lang,_roletype,_rolepriority,_roleremark,_orderby,_userno,_userno  RETURNING roleid INTO _newroleid;
         -- 复制角色分组关系
				 INSERT INTO crm_sys_role_group_relate (rolegroupid, roleid) 
				 VALUES (_rolegroupid::uuid,_newroleid);
         --用户角色关系
--          INSERT INTO crm_sys_userinfo_role_relate(userid,roleid)
--          SELECT userid,_newroleid  FROM crm_sys_userinfo_role_relate WHERE roleid=_roleid::uuid;

         FOR _r1 IN  SELECT rr.ruleid  FROM crm_sys_role_rule_relation AS rr
										 INNER JOIN crm_sys_rule AS r ON r.ruleid=rr.ruleid AND r.recstatus=1
										 WHERE roleid=_roleid::uuid LOOP
               _ruleid:=_r1.ruleid;
							 IF _ruleid IS NOT NULL OR COALESCE(cast(_ruleid as TEXT),'')<>'' THEN

										 --插入规则
										 INSERT INTO crm_sys_rule(
													 rulename,
													 entityid,
													 rulesql,
													 reccreator,
													 recupdator)
										 SELECT rulename,entityid,COALESCE(rulesql,'') rulesql,_userno,_userno FROM crm_sys_rule WHERE ruleid=_ruleid RETURNING ruleid INTO _newruleid ;
										 
										 INSERT INTO crm_sys_role_rule_relation(roleid,ruleid)
										 SELECT _newroleid,_newruleid; 

											
											-- 规则明细
											select (COALESCE(MAX(recorder),0)+1) into _orderby from crm_sys_rule_item;
											FOR _r IN SELECT itemid FROM crm_sys_rule_item_relation WHERE ruleid=_ruleid  loop
														_newitemid:=uuid_generate_v1()::uuid;
														INSERT INTO crm_sys_rule_item(
														itemid,
														itemname,
														fieldid,
														operate,
														ruledata,
														ruletype,
														rulesql,
														usetype,
														recorder,
														reccreator,
														recupdator) SELECT _newitemid,itemname,fieldid,operate,ruledata,
																							 ruletype,rulesql,usetype,_orderby,
																							 _userno,_userno FROM crm_sys_rule_item WHERE itemid=_r.itemid ;
													 -- 规则和明细关系

														INSERT INTO crm_sys_rule_item_relation(
														ruleid,
														itemid,
														userid,
														rolesub,
														paramindex)
														SELECT _newruleid,_newitemid,0,rolesub,paramindex FROM crm_sys_rule_item_relation WHERE ruleid=_ruleid AND itemid=_r.itemid;
											 END loop;
											 INSERT INTO "public"."crm_sys_rule_set" 
											 ("ruleid", "ruleset", "userid", "ruleformat")
											 SELECT _newruleid,ruleset,0,ruleformat FROM  crm_sys_rule_set WHERE ruleid=_ruleid;
                       UPDATE crm_sys_rule SET rulesql=crm_func_role_rule_fetch_single_sql(_newruleid,_userno) WHERE ruleid=_newruleid;
								 END IF;
         END LOOP;
					 _codeid:= _newroleid::TEXT;
					 _codeflag:= 1;
					 _codemsg:= '复制角色成功';

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

ALTER FUNCTION "public"."crm_func_role_copy"("_roleid" text, "_rolegroupid" text, "_rolename" varchar, "_rolepriority" int4, "_roletype" int4, "_roleremark" varchar, "_userno" int4) OWNER TO "postgres";


CREATE OR REPLACE FUNCTION "public"."crm_func_role_list"("_groupid" text, "_rolename" text, "_pageindex" int4, "_pagesize" int4, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_attendance_list(0,0, '',1,10, 5);
--FETCH ALL FROM datacursor;
--FETCH ALL FROM pagecursor;

DECLARE
  
  _sql_where TEXT:='';
  _execute_sql TEXT;
 
 
  --分页标准参数
  _page_sql TEXT;
  _count_sql TEXT;
  _datacursor refcursor:= 'datacursor';
  _pagecursor refcursor:= 'pagecursor';

BEGIN
 

   IF _groupid IS NOT NULL AND _groupid<>'' THEN
          _sql_where:=' and gr.rolegroupid= '''||_groupid||'''::uuid';   
   END IF;

   IF _rolename IS NOT NULL AND _rolename<>'' THEN
          _sql_where:=_sql_where||' and r.rolename ilike ''%'||_rolename||'%''';
   END IF;
 
   _execute_sql:='
        select  
				r.roleid, 
				r.rolename, 
				case when r.roletype=0 then ''系统角色'' when r.roletype=1 then ''自定义角色'' else '''' end roletypename, 
        
        r.roletype,
        r.rolepriority,
        r.roleremark,
        r.recorder,
        gr.rolegroupname,
        gr.rolegroupid,r.rolename_lang
        from crm_sys_role r left join crm_sys_role_group_relate re on r.roleid=re.roleid 
        left join crm_sys_role_group gr  on gr.rolegroupid=re.rolegroupid where r.recstatus=1 '||_sql_where||' order by r.recupdated desc';

    RAISE NOTICE '%',_execute_sql;

   	--查询分页数据
	SELECT * FROM crm_func_paging_sql_fetch(_execute_sql, _pageindex, _pagesize) INTO _page_sql,_count_sql;

  OPEN _datacursor FOR EXECUTE _page_sql  ;
	RETURN NEXT _datacursor;

	OPEN _pagecursor FOR EXECUTE _count_sql  ;
	RETURN NEXT _pagecursor;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;

ALTER FUNCTION "public"."crm_func_role_list"("_groupid" text, "_rolename" text, "_pageindex" int4, "_pagesize" int4, "_userno" int4) OWNER TO "postgres";
