ALTER table crm_sys_entity_compoment_config add COLUMN comptname_lang jsonb;



----------------------------



CREATE OR REPLACE FUNCTION "public"."crm_fun_entrance_save"("_entrance" text, "_userno" int4)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE 
  _entranceid uuid:=null;
  _entrancejsonarr json;
  _r record;
  _r1 record;
  _modeltype INT4;
  _funcid uuid;
  _parentid uuid;
  _entityname TEXT;
  _sql TEXT;
  _remove_entityid TEXT;

  --��׼���ز���
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;
	_WEB_CRM_FuncID uuid;
	_WEB_Office_FuncID uuid;
	_WEB_FuncID uuid;
	_MOB_FuncID uuid;
	_Root_FuncID  uuid;
	_MOB_CRM_FuncID  uuid;
	_MOB_Office_FuncID  uuid;
	_MOB_OtherEntity_FuncID  uuid;
	_WEB_OtherEntity_FuncID  uuid;
BEGIN

   BEGIN
		_Root_FuncID = '1fc3a304-9e5c-4f8e-852b-ef947645aa98'::uuid;
		_MOB_FuncID = 'd90680f9-5cf3-49c2-a83e-8ab267ff094a'::uuid;
		_WEB_FuncID = '1f9a7c10-0a22-4ef0-825e-c98d4503c600'::uuid;
		_WEB_CRM_FuncID='c84ff512-fe7d-4d7e-8cf2-f0dc72ea9cb3'::uuid;
		_WEB_Office_FuncID = '7c927ecd-bdf9-424f-bed8-e577783c3922'::uuid;
		_MOB_CRM_FuncID='da5fbce8-8c52-4d3c-8e44-8aa5bfdfaae1'::uuid;
		_MOB_Office_FuncID = '39287afa-8254-446b-960d-2924cebfc84b'::uuid;
    SELECT funcid INTO _WEB_OtherEntity_FuncID FROM crm_sys_function WHERE entityid='11111111-0000-0000-0000-000000000000' AND devicetype=0 LIMIT 1;
    SELECT funcid INTO _MOB_OtherEntity_FuncID FROM crm_sys_function WHERE entityid='11111111-0000-0000-0000-000000000000' AND devicetype=1 LIMIT 1;
    IF _WEB_OtherEntity_FuncID IS NULL OR _MOB_OtherEntity_FuncID IS NULL THEN
        Raise EXCEPTION 'ȱʧְ�����ڵ�';
    END IF;
	 SELECT _entrance::json INTO _entrancejsonarr;
		SELECT  array_to_string(ARRAY(SELECT unnest(array_agg(entityid))),',') INTO _remove_entityid  FROM crm_sys_entrance WHERE entityid NOT IN (
													SELECT entityid::uuid FROM json_populate_recordset(null::crm_sys_entrance, _entrancejsonarr)
													);
 
		DELETE FROM crm_sys_entrance;
		INSERT INTO crm_sys_entrance(
							 entryname,
							 entrytype,
							 entityid,
							 isgroup,
							 recorder,
							 reccreator,
							 recupdator,web,mob)
	 SELECT entryname,entrytype,entityid::uuid,isgroup,recorder,_userno,_userno,
   1 AS web,
   (CASE WHEN (entityid='00000000-0000-0000-0000-000000000002' OR entityid='00000000-0000-0000-0000-000000000004') THEN 0 ELSE 1 END) AS mob
   FROM json_populate_recordset(null::crm_sys_entrance, _entrancejsonarr);
  
   FOR _r IN  SELECT entityid::uuid,entrytype FROM json_populate_recordset(null::crm_sys_entrance, _entrancejsonarr) LOOP
       IF EXISTS(SELECT 1 FROM crm_sys_function WHERE entityid=_r.entityid AND rectype=1 AND recstatus=1 GROUP BY funcid HAVING(count(1))>2) THEN
            Raise EXCEPTION '%','ְ������쳣';
       END IF;

       SELECT entityname,modeltype INTO _entityname,_modeltype FROM crm_sys_entity WHERE entityid=_r.entityid LIMIT 1;
				IF NOT EXISTS(SELECT 1 FROM crm_sys_entity_compoment_config WHERE entityid=_r.entityid AND comptaction='EntityDataAdd' LIMIT 1 ) THEN
						 INSERT INTO "public"."crm_sys_entity_compoment_config" ("comptid", "entityid", "comptname", "comptaction","icon", "recorder","reccreator","recupdator",comptname_lang)
						 VALUES (uuid_generate_v1()::uuid, _r.entityid, '����', 'EntityDataAdd','00000000-0000-0000-0000-400000000001', '1',_userno,_userno,'{"cn":"����","en":"Add","tw":"����"}'::jsonb);
				ELSE
						 UPDATE crm_sys_entity_compoment_config SET recstatus=1 WHERE entityid=_r.entityid;
        END IF;
				IF NOT EXISTS(SELECT 1 FROM crm_sys_entity_compoment_config WHERE entityid=_r.entityid AND comptaction='EntityDataEdit' LIMIT 1 ) THEN
						 INSERT INTO "public"."crm_sys_entity_compoment_config" ("comptid", "entityid", "comptname", "comptaction","icon", "recorder","reccreator","recupdator",comptname_lang)
						 VALUES (uuid_generate_v1()::uuid, _r.entityid, '�༭', 'EntityDataEdit','00000000-0000-0000-0000-400000000002', '2',_userno,_userno,'{"cn":"�༭","en":"edit","tw":"��݋"}'::jsonb);
				ELSE
						 UPDATE crm_sys_entity_compoment_config SET recstatus=1 WHERE entityid=_r.entityid;
        END IF;
				IF NOT EXISTS(SELECT 1 FROM crm_sys_entity_compoment_config WHERE entityid=_r.entityid AND comptaction='EntityDataDelete' LIMIT 1 ) THEN
						 INSERT INTO "public"."crm_sys_entity_compoment_config" ("comptid", "entityid", "comptname", "comptaction","icon", "recorder","reccreator","recupdator",comptname_lang)
						 VALUES (uuid_generate_v1()::uuid, _r.entityid, 'ɾ��', 'EntityDataDelete','00000000-0000-0000-0000-400000000003', '3',_userno,_userno,'{"cn":"ɾ��","en":"delete","tw":"�h��"}'::jsonb);
				ELSE
						 UPDATE crm_sys_entity_compoment_config SET recstatus=1 WHERE entityid=_r.entityid;
        END IF;
				IF NOT EXISTS(SELECT 1 FROM crm_sys_entity_compoment_config WHERE entityid=_r.entityid AND comptaction='EntityDataImport' LIMIT 1 ) THEN
						 INSERT INTO "public"."crm_sys_entity_compoment_config" ("comptid", "entityid", "comptname", "comptaction","icon", "recorder","reccreator","recupdator",comptname_lang)
						 VALUES (uuid_generate_v1()::uuid, _r.entityid, '����', 'EntityDataImport','00000000-0000-0000-0000-400000000004', '4',_userno,_userno,'{"cn":"����","en":"import","tw":"����"}'::jsonb);
				ELSE
						 UPDATE crm_sys_entity_compoment_config SET recstatus=1 WHERE entityid=_r.entityid;
        END IF;
				IF NOT EXISTS(SELECT 1 FROM crm_sys_entity_compoment_config WHERE entityid=_r.entityid AND comptaction='EntityDataExport' LIMIT 1 ) THEN
						 INSERT INTO "public"."crm_sys_entity_compoment_config" ("comptid", "entityid", "comptname", "comptaction","icon", "recorder","reccreator","recupdator",comptname_lang)
						 VALUES (uuid_generate_v1()::uuid, _r.entityid, '����', 'EntityDataExport','00000000-0000-0000-0000-400000000005', '5',_userno,_userno,'{"cn":"����","en":"report","tw":"����"}'::jsonb);
				ELSE
						 UPDATE crm_sys_entity_compoment_config SET recstatus=1 WHERE entityid=_r.entityid;
        END IF;
				IF _modeltype=0 THEN
							IF NOT EXISTS(SELECT 1 FROM crm_sys_entity_compoment_config WHERE entityid=_r.entityid AND comptaction='EntityDataTransfer' LIMIT 1 ) THEN
										INSERT INTO "public"."crm_sys_entity_compoment_config" ("comptid", "entityid", "comptname", "comptaction","icon", "recorder","reccreator","recupdator",comptname_lang)
										VALUES (uuid_generate_v1()::uuid, _r.entityid, 'ת��', 'EntityDataTransfer','00000000-0000-0000-0000-400000000003', '6',_userno,_userno,'{"cn":"ת��","en":"remove","tw":"�D��"}'::jsonb);
				      ELSE
						        UPDATE crm_sys_entity_compoment_config SET recstatus=1 WHERE entityid=_r.entityid;
							END IF;
        END IF;
   
        IF _modeltype=0 OR _modeltype=2 THEN

					 IF EXISTS(SELECT 1 FROM crm_sys_function  WHERE entityid=_r.entityid AND rectype=1 AND recstatus=1 AND devicetype=0 AND parentid=_WEB_OtherEntity_FuncID LIMIT 1) THEN
							 IF _r.entrytype=0 THEN
									 UPDATE crm_sys_function SET parentid=_WEB_CRM_FuncID WHERE entityid=_r.entityid AND rectype=1 AND recstatus=1 AND devicetype=0;
 
							 ELSEIF _r.entrytype=1 THEN
									 UPDATE crm_sys_function SET parentid=_WEB_Office_FuncID WHERE entityid=_r.entityid AND rectype=1 AND recstatus=1 AND devicetype=0;  
							 END IF;
           ELSEIF EXISTS(SELECT 1 FROM crm_sys_function  WHERE entityid=_r.entityid AND rectype=1 AND recstatus=1 AND devicetype=0 AND parentid IS NOT NULL LIMIT 1) THEN
                SELECT funcid,parentid INTO _funcid,_parentid FROM crm_sys_function WHERE entityid=_r.entityid AND rectype=1 AND recstatus=1 AND devicetype=0 LIMIT 1;
							 IF _r.entrytype=0 THEN
 
                   UPDATE crm_sys_function SET parentid=_WEB_CRM_FuncID  WHERE funcid=_funcid;

							 ELSEIF _r.entrytype=1 THEN
                   UPDATE crm_sys_function SET parentid=_WEB_Office_FuncID  WHERE funcid=_funcid;
							 END IF;

					 END IF;
					 IF EXISTS(SELECT 1 FROM crm_sys_function  WHERE entityid=_r.entityid AND rectype=1 AND recstatus=1 AND devicetype=1 AND parentid=_MOB_OtherEntity_FuncID LIMIT 1) THEN
							 IF _r.entrytype=0 THEN
									 UPDATE crm_sys_function SET parentid=_MOB_CRM_FuncID WHERE entityid=_r.entityid AND rectype=1 AND recstatus=1 AND devicetype=1;
 
							 ELSEIF _r.entrytype=1 THEN
									 UPDATE crm_sys_function SET parentid=_MOB_Office_FuncID WHERE entityid=_r.entityid AND rectype=1 AND recstatus=1 AND devicetype=1;  

							 END IF;
           ELSEIF EXISTS(SELECT 1 FROM crm_sys_function  WHERE entityid=_r.entityid AND rectype=1 AND recstatus=1 AND devicetype=0 AND parentid IS NOT NULL LIMIT 1) THEN
                SELECT funcid,parentid INTO _funcid,_parentid FROM crm_sys_function WHERE entityid=_r.entityid AND rectype=1 AND recstatus=1 AND devicetype=1 LIMIT 1;
							 IF _r.entrytype=0 THEN
 
                   UPDATE crm_sys_function SET parentid=_MOB_CRM_FuncID  WHERE funcid=_funcid;

							 ELSEIF _r.entrytype=1 THEN
                   UPDATE crm_sys_function SET parentid=_MOB_Office_FuncID  WHERE funcid=_funcid;

							 END IF;
					 END IF;
       END IF;
    
			 FOR _r1 IN SELECT funcid FROM crm_sys_function WHERE entityid=_r.entityid AND rectype=1 LOOP
			        PerForm crm_func_init_treepath(_r1.funcid);
       END LOOP;

   END LOOP;
    
   --�Ƴ�ְ�����ڵ�
   FOR _r IN SELECT UNNEST(string_to_array(_remove_entityid,','))::uuid AS entityid LOOP
        --�Ƴ�web�˵Ľڵ���
       SELECT funcid INTO _funcid FROM crm_sys_function WHERE entityid=_r.entityid AND rectype=1 and devicetype = 0   AND recstatus=1 LIMIT 1;
       DELETE FROM crm_sys_function_treepaths WHERE descendant=_funcid AND ancestor=_WEB_FuncID;
       DELETE FROM crm_sys_function_treepaths WHERE descendant=_funcid AND ancestor=_WEB_CRM_FuncID;
       DELETE FROM crm_sys_function_treepaths WHERE descendant=_funcid AND ancestor=_WEB_Office_FuncID;
       UPDATE crm_sys_function SET parentid=_WEB_OtherEntity_FuncID WHERE funcid=_funcid;

       --�Ƴ�mob�˵Ľڵ���
       SELECT funcid INTO _funcid FROM crm_sys_function WHERE entityid=_r.entityid AND rectype=1 AND devicetype=1 AND recstatus=1 LIMIT 1;
       DELETE FROM crm_sys_function_treepaths WHERE descendant=_funcid AND ancestor=_MOB_FuncID;
       DELETE FROM crm_sys_function_treepaths WHERE descendant=_funcid AND ancestor=_MOB_CRM_FuncID;
       DELETE FROM crm_sys_function_treepaths WHERE descendant=_funcid AND ancestor=_MOB_Office_FuncID;
       UPDATE crm_sys_function SET parentid=_MOB_OtherEntity_FuncID WHERE funcid=_funcid;
       --�����б�˵���ť
       UPDATE crm_sys_entity_compoment_config SET recstatus=0 WHERE entityid=_r.entityid;
				PerForm crm_func_init_treepath(_funcid);
   END LOOP;
   
 
					_codeid:= uuid_generate_v1();
					_codeflag:= 1;
					_codemsg:= '������ڷ���ɹ�';
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




------------------------------------------
