CREATE OR REPLACE FUNCTION "public"."crm_func_init_entity_function"("_entityid" uuid, "_userno" int4)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
DECLARE
   _entityname TEXT;
   _funcid uuid;
   _modeltype int4;
   _menuid TEXT;
   _relentityname TEXT;
   _relentityid uuid;
   _tmpid uuid;
   _r record;
   _web_parent_funcid uuid;
   _mob_parent_funcid uuid;
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;
BEGIN

	BEGIN
		    --获取表名
    SELECT entityname,modeltype INTO _entityname,_modeltype FROM crm_sys_entity WHERE entityid = _entityid LIMIT 1;
    IF _entityname IS NULL OR _entityname = '' THEN
           Raise EXCEPTION '实体名称不能为空';
    END IF;
    SELECT funcid INTO _web_parent_funcid FROM crm_sys_function WHERE entityid='11111111-0000-0000-0000-000000000000' AND devicetype=0 LIMIT 1;
    SELECT funcid INTO _mob_parent_funcid FROM crm_sys_function WHERE entityid='11111111-0000-0000-0000-000000000000' AND devicetype=1 LIMIT 1;
    IF _web_parent_funcid IS NULL OR _mob_parent_funcid IS NULL THEN
        Raise EXCEPTION '缺失职能树节点';
    END IF;
    CASE  _modeltype
    WHEN 0  THEN
					--初始化实体职能权限码
						--实体根节点
          --web
					SELECT id,flag,msg,stacks,codes INTO _codeid,_codeflag,_codemsg,_codestack,_codestatus FROM crm_func_function_insert(0,NULL,_web_parent_funcid,_entityname,'Entity',_entityid,0,1,'',_userno);
 
					SELECT id::uuid,flag,msg,stacks,codes INTO _funcid,_codeflag,_codemsg,_codestack,_codestatus FROM crm_func_function_insert(0,'',_codeid::uuid,(_entityname||'功能'),'EntityFunc',_entityid,0,3,'',_userno);

					PERFORM crm_func_function_insert(-1,'api/dynamicentity/add',_funcid,'新增','EntityDataAdd',_entityid,0,0,'',_userno);
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/edit',_funcid,'编辑','EntityDataEdit',_entityid,0,0,'',_userno);
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/delete',_funcid,'删除','EntityDataDelete',_entityid,0,0,'',_userno);
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/transfer',_funcid,'转移','EntityDataTransfer',_entityid,0,0,'',_userno);
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/exportdata',_funcid,'导出','EntityDataExport',_entityid,0,0,'',_userno);
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/importdata',_funcid,'导入','EntityDataImport',_entityid,0,0,'',_userno);

					SELECT id::uuid,flag,msg,stacks,codes INTO _funcid,_codeflag,_codemsg,_codestack,_codestatus FROM crm_func_function_insert(0,null,_codeid::uuid,_entityname||'菜单','EntityMenu',_entityid,0,2,'',_userno);

          SELECT menuid::TEXT INTO _menuid FROM crm_sys_entity_menu WHERE entityid=_entityid AND menutype=0;
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/list',_funcid,'全部数据','AllEntityData',_entityid,0,0,_menuid,_userno);

          SELECT menuid::TEXT INTO _menuid FROM crm_sys_entity_menu WHERE entityid=_entityid AND menutype=1;
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/list',_funcid,'待转移数据','TransferEntityData',_entityid,0,0,_menuid,_userno);
					
					SELECT id::uuid,flag,msg,stacks,codes INTO _funcid,_codeflag,_codemsg,_codestack,_codestatus FROM crm_func_function_insert(0,null,_codeid::uuid,_entityname||'主页Tab','EntityTab',_entityid,0,4,'',_userno);

  				--INSERT INTO "public"."crm_sys_entity_rel_tab" ("relid", "entityid", "relentityid", "relname", "icon","web","mob","recorder", "reccreator", "recupdator","fieldid", "entitytaburl")
          --VALUES (uuid_generate_v1()::uuid, _entityid, NULL, '基础信息', '6a973cf6-4ecb-42de-9f9c-a9f22e7de83f',1,0, '2',_userno,_userno, NULL, 'info') RETURNING relid INTO _tmpid;
          SELECT relid INTO _tmpid FROM crm_sys_entity_rel_tab WHERE entityid=_entityid AND entitytaburl='info' AND recstatus=1 LIMIT 1;

					PERFORM crm_func_function_insert(-1,'api/dynamicentity/detial',_funcid,'基础信息','EntityDataDetail',_entityid,0,0,_tmpid::TEXT,_userno);
 
					--INSERT INTO "public"."crm_sys_entity_rel_tab" ("relid", "entityid", "relentityid", "relname", "icon","web","mob", "recorder", "reccreator", "recupdator","fieldid", "entitytaburl")
          --VALUES (uuid_generate_v1()::uuid, _entityid, NULL, '动态', '6a973cf6-4ecb-42de-9f9c-a9f22e7de83f', 1,0,'1', _userno,_userno, NULL, 'activities')  RETURNING relid INTO _tmpid;
          SELECT relid INTO _tmpid FROM crm_sys_entity_rel_tab WHERE entityid=_entityid AND entitytaburl='activities' AND recstatus=1 LIMIT 1;
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/getdynamiclist',_funcid,'动态','EntityDataDynamicList',_entityid,0,0,_tmpid::TEXT,_userno);
 

					--INSERT INTO "public"."crm_sys_entity_rel_tab" ("relid", "entityid", "relentityid", "relname", "icon","web","mob",  "recorder","reccreator", "recupdator","fieldid", "entitytaburl")
          --VALUES (uuid_generate_v1()::uuid,_entityid, NULL, '文档', '6a973cf6-4ecb-42de-9f9c-a9f22e7de83f',1,0, '3', _userno, _userno, NULL, 'docs') RETURNING relid INTO _tmpid;
          SELECT relid INTO _tmpid FROM crm_sys_entity_rel_tab WHERE entityid=_entityid AND entitytaburl='docs' AND recstatus=1 LIMIT 1;
					SELECT id::uuid,flag,msg,stacks,codes INTO _funcid,_codeflag,_codemsg,_codestack,_codestatus FROM  crm_func_function_insert(0,'api/dynamicentity/documentlist',_funcid,'文档','EntityDataDocmentList',_entityid,0,9,_tmpid::TEXT,_userno);

					PERFORM crm_func_function_insert(-1,'api/documents/adddocument',_funcid,'文档上传','DocumentUpload',_entityid,0,0,'',_userno);
					PERFORM crm_func_function_insert(-1,'api/documents/deletedocument',_funcid,'文档删除','DocumentDelete',_entityid,0,0,'',_userno);

					PERFORM crm_func_function_insert(0,null,_codeid::uuid,_entityname||'主页动态Tab','EntityDynamicTab',_entityid,0,5,'',_userno);

					--初始化实体职能权限码
						--实体根节点
          --mob
					SELECT id::uuid,flag,msg,stacks,codes INTO _codeid,_codeflag,_codemsg,_codestack,_codestatus FROM crm_func_function_insert(0,null,_mob_parent_funcid,_entityname,'EntityFunc',_entityid,1,1,'',_userno);

					SELECT id::uuid,flag,msg,stacks,codes INTO _funcid,_codeflag,_codemsg,_codestack,_codestatus FROM crm_func_function_insert(0,null,_codeid::uuid,_entityname||'功能','EntityFunc',_entityid,1,3,'',_userno);
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/add',_funcid,'新增','EntityDataAdd',_entityid,1,0,'',_userno);
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/edit',_funcid,'编辑','EntityDataEdit',_entityid,1,0,'',_userno);
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/delete',_funcid,'删除','EntityDataDelete',_entityid,1,0,'',_userno);
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/transfer',_funcid,'转移','EntityDataTransfer',_entityid,1,0,'',_userno);

					SELECT id::uuid,flag,msg,stacks,codes INTO _funcid,_codeflag,_codemsg,_codestack,_codestatus FROM crm_func_function_insert(0,null,_codeid::uuid,_entityname||'菜单','EntityMenu',_entityid,1,2,'',_userno);
          SELECT menuid::TEXT INTO _menuid FROM crm_sys_entity_menu WHERE entityid=_entityid AND menutype=0;
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/list',_funcid,'全部数据','AllEntityData',_entityid,1,0,_menuid,_userno);
          SELECT menuid::TEXT INTO _menuid FROM crm_sys_entity_menu WHERE entityid=_entityid AND menutype=1;
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/list',_funcid,'待转移数据','TransferEntityData',_entityid,1,0,_menuid,_userno);
					
					SELECT id::uuid,flag,msg,stacks,codes INTO _funcid,_codeflag,_codemsg,_codestack,_codestatus FROM crm_func_function_insert(0,null,_codeid::uuid,_entityname||'主页Tab','EntityTab',_entityid,1,4,'',_userno);
          SELECT relid INTO _tmpid FROM crm_sys_entity_rel_tab WHERE entityid=_entityid AND entitytaburl='chat' AND recstatus=1 LIMIT 1;
					PERFORM crm_func_function_insert(-1,'api/chat/send',_funcid,'沟通','EntityDataChat',_entityid,1,0,_tmpid::TEXT,_userno);

          SELECT relid INTO _tmpid FROM crm_sys_entity_rel_tab WHERE entityid=_entityid AND entitytaburl='docs' AND recstatus=1 LIMIT 1;
					SELECT id::uuid,flag,msg,stacks,codes INTO _funcid,_codeflag,_codemsg,_codestack,_codestatus FROM  crm_func_function_insert(0,'api/dynamicentity/documentlist',_funcid,'文档','EntityDataDocmentList',_entityid,1,9,_tmpid::TEXT,_userno);

					PERFORM crm_func_function_insert(0,null,_codeid::uuid,_entityname||'主页动态Tab','EntityDynamicTab',_entityid,1,5,'',_userno);

					---处理其他动态页签
		
					select crm_func_init_entity_tab_function(entityid,relid,1) into _codemsg
					from crm_sys_entity_rel_tab 
					where entityid = _entityid and  relname not in('沟通','动态','基础信息','文档');
	  WHEN 2 THEN
          --web
					SELECT id,flag,msg,stacks,codes INTO _codeid,_codeflag,_codemsg,_codestack,_codestatus FROM crm_func_function_insert(0,null,_web_parent_funcid,_entityname,'Entity',_entityid,0,1,'',_userno);

					SELECT id,flag,msg,stacks,codes INTO _funcid,_codeflag,_codemsg,_codestack,_codestatus FROM crm_func_function_insert(0,null,_codeid::uuid,_entityname||'功能','EntityFunc',_entityid,0,3,'',_userno);
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/add',_funcid,'新增','EntityDataAdd',_entityid,0,0,'',_userno);
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/edit',_funcid,'编辑','EntityDataEdit',_entityid,0,0,'',_userno);
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/delete',_funcid,'删除','EntityDataDelete',_entityid,0,0,'',_userno);
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/exportdata',_funcid,'导出','EntityDataExport',_entityid,0,0,'',_userno);
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/importdata',_funcid,'导入','EntityDataImport',_entityid,0,0,'',_userno);

					SELECT id::uuid,flag,msg,stacks,codes INTO _funcid,_codeflag,_codemsg,_codestack,_codestatus FROM crm_func_function_insert(0,null,_codeid::uuid,_entityname||'菜单','EntityMenu',_entityid,0,2,'',_userno);

          SELECT menuid::TEXT INTO _menuid FROM crm_sys_entity_menu WHERE entityid=_entityid AND menutype=0;
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/list',_funcid,'全部数据','AllEntityData',_entityid,0,0,_menuid,_userno);


          --mob
					SELECT id,flag,msg,stacks,codes INTO _codeid,_codeflag,_codemsg,_codestack,_codestatus FROM crm_func_function_insert(0,null,_mob_parent_funcid,_entityname,'EntityFunc',_entityid,1,1,'',_userno);

					SELECT id,flag,msg,stacks,codes INTO _funcid,_codeflag,_codemsg,_codestack,_codestatus FROM crm_func_function_insert(0,null,_codeid::uuid,_entityname||'功能','EntityFunc',_entityid,1,3,'',_userno);
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/add',_funcid,'新增','EntityDataAdd',_entityid,1,0,'',_userno);
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/edit',_funcid,'编辑','EntityDataEdit',_entityid,1,0,'',_userno);
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/delete',_funcid,'删除','EntityDataDelete',_entityid,1,0,'',_userno);

					SELECT id::uuid,flag,msg,stacks,codes INTO _funcid,_codeflag,_codemsg,_codestack,_codestatus FROM crm_func_function_insert(0,null,_codeid::uuid,_entityname||'菜单','EntityMenu',_entityid,1,2,'',_userno);

          SELECT menuid::TEXT INTO _menuid FROM crm_sys_entity_menu WHERE entityid=_entityid AND menutype=0;
					PERFORM crm_func_function_insert(-1,'api/dynamicentity/list',_funcid,'全部数据','AllEntityData',_entityid,1,0,_menuid,_userno);

	
    WHEN 3 THEN
          SELECT entityname,entityid INTO _relentityname,_relentityid FROM crm_sys_entity WHERE entityid =(SELECT relentityid  FROM crm_sys_entity WHERE entityid=_entityid);

          --web
-- 					SELECT id,flag,msg,stacks,codes INTO _funcid,_codeflag,_codemsg,_codestack,_codestatus FROM crm_func_function_insert(0,null,'b3bd4972-23cd-4dc6-a4c8-4f1ee023fe14'::uuid,_entityname||'('||_relentityname||')','Entity',_entityid,0,3,'',_userno);
--  
-- 					PERFORM crm_func_function_insert(-1,'api/dynamicentity/add',_funcid,'新增','EntityDataAdd',_entityid,0,0,'',_userno);
-- 					PERFORM crm_func_function_insert(-1,'api/dynamicentity/edit',_funcid,'编辑','EntityDataEdit',_entityid,0,0,'',_userno);
-- 					PERFORM crm_func_function_insert(-1,'api/dynamicentity/delete',_funcid,'删除','EntityDataDelete',_entityid,0,0,'',_userno);
            IF EXISTS(SELECT 1 FROM crm_sys_entity WHERE relaudit=0 AND recstatus=1 AND entityid=_entityid) THEN
								FOR _r IN SELECT funcid , devicetype FROM crm_sys_function WHERE entityid=_relentityid AND rectype=5 LOOP
											SELECT id,flag,msg,stacks,codes INTO _funcid,_codeflag,_codemsg,_codestack,_codestatus FROM 
											crm_func_function_insert(-1,'api/dynamicentity/add',_r.funcid,_entityname,'EntityDynamic',_entityid,_r.devicetype,0,_entityid::TEXT,_userno);
											IF _codeflag=0 THEN
												 Raise EXCEPTION '%','初始化职能树失败';
											END IF;
                      IF _r.devicetype=0 THEN
													SELECT id,flag,msg,stacks,codes INTO _funcid,_codeflag,_codemsg,_codestack,_codestatus FROM 
													crm_func_function_insert(-1,'api/dynamicentity/exportdata',_funcid::uuid,'导出','EntityDataExport',_entityid,0,0,NULL,_userno);
													IF _codeflag=0 THEN
														 Raise EXCEPTION '%','初始化职能树失败';
													END IF;
                      END IF;
								END LOOP;
            END IF;
    END CASE;

		_codeid:= _funcid::TEXT;
		_codeflag:= 1;
		_codemsg:= '初始化实体职能';
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

ALTER FUNCTION "public"."crm_func_init_entity_function"("_entityid" uuid, "_userno" int4) OWNER TO "postgres";