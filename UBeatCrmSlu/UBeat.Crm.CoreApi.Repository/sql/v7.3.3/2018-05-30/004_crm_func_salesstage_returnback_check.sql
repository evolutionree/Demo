CREATE OR REPLACE FUNCTION "public"."crm_func_salesstage_returnback_check"("_recid" text, "_salesstageid" text, "_typeid" text, "_userno" int4)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
DECLARE
 
   
  r1 record;
  _entitytable TEXT;
  _recaudits int4:=1;
  _entityid uuid;
  _entity_sql TEXT:='';
  _msg TEXT:='';
 
  _stagestatus TEXT:='';
  _id TEXT;
  _arr TEXT[];
  _arr_length int4;
  _tmpsalesstageid TEXT;
  _cursalesstageid TEXT;
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

  --消息推送参数
  _notify_operate_username TEXT;
  _notify_msggroupid INT; 
  _notify_msgdataid uuid;
  _notify_entityid uuid;
  _notify_msgtype INT;
  _notify_msgtitle TEXT; 
  _notify_msgcontent TEXT:=''; 
  _notify_msgstatus TEXT:='';
  _notify_msgparam jsonb:='{}';
  _notify_sendtime timestamp:=now();
  _notify_receiver TEXT; 
  _notify_creator INT;
  _notify_msgid INT;
  _need_send_dynamic INT:=1;

  --动态参数
  _dynamictype INT;
  _dynamic_entityid uuid;
  _dynamic_businessid uuid;
  _dynamic_typeid uuid;

  _winrate numeric;
  _username TEXT;
  _opportunityname TEXT;
  _sql TEXT;

  _fieldconfig json;
  _statusid TEXT;
  _controltype int4;
  _datasource_type TEXT;
  _datasource_sourceid TEXT;
  _status_val TEXT:='';

  _relentityid uuid:=null;
  _flowid uuid:=null;
  _dynrecid uuid:=null;
	_auditstatus int4;
BEGIN
	BEGIN	
     SELECT entityid INTO _entityid FROM crm_sys_entity_category WHERE categoryid=_typeid::uuid;
     SELECT entitytable INTO _entitytable FROM crm_sys_entity WHERE entityid=_entityid::uuid;

     _entity_sql:='select recid::text,opporstatus::text FROM '||_entitytable||'  WHERE recid='''||_recid||'''::uuid limit 1';

     EXECUTE _entity_sql INTO _id,_statusid; 

     IF _id IS NULL OR _id='' THEN
         Raise EXCEPTION '该商机不存在';
     END IF;
     SELECT controltype,fieldconfig INTO _controltype,_fieldconfig FROM crm_sys_entity_fields WHERE entityid=_entityid::uuid AND fieldname='opporstatus' AND recstatus=1 LIMIT 1;
     IF _controltype IS NULL THEN
        Raise EXCEPTION '%','缺少商机状态字段';
     END IF;

     CASE _controltype
				 WHEN 3 THEN
									--单选
								_datasource_type:=_fieldconfig#>>'{dataSource,type}';
								_datasource_sourceid:=_fieldconfig#>>'{dataSource,sourceId}';
								CASE _datasource_type
											WHEN 'local' THEN
														_status_val:=' select crm_func_entity_protocol_format_dictionary(''' || _datasource_sourceid || ''',' || _statusid || '::TEXT) ';    
											WHEN 'network' THEN
											ELSE
								END CASE;
				 WHEN 4 THEN
									--多选
								_datasource_type:=_fieldconfig#>>'{dataSource,type}';
								_datasource_sourceid:=_fieldconfig#>>'{dataSource,sourceId}';
								CASE _datasource_type
											WHEN 'local' THEN
														_status_val:='select crm_func_entity_protocol_format_dictionary(''' || _datasource_sourceid || ''',' || _statusid || '::TEXT) ' ;    
											WHEN 'network' THEN
											ELSE
								END CASE;
				 ELSE
         Raise EXCEPTION '%','字段类型异常';
		END CASE;

    EXECUTE _status_val INTO _stagestatus;

   IF _stagestatus='赢单' OR _stagestatus='输单' OR _stagestatus='弃单' THEN
			 _msg:='该阶段已处于'||_stagestatus||'阶段不允许回退';
			 Raise EXCEPTION '%',_msg;
   END IF;

	 SELECT array_length(
	 ARRAY(SELECT UNNEST(string_to_array((_salesstageid),',')))::text[],1) INTO _arr_length;

	 SELECT ARRAY(SELECT UNNEST(string_to_array((_salesstageid),',')))::text[] INTO _arr;
	 SELECT _arr[1] INTO _tmpsalesstageid;
	 SELECT _arr[_arr_length] INTO _cursalesstageid;
	 SELECT array_remove(_arr,_tmpsalesstageid) INTO _arr;

		SELECT relentityid INTO _relentityid FROM crm_sys_salesstage_dynentity_setting WHERE  salesstageid=_cursalesstageid::uuid;
		IF EXISTS(SELECT 1 FROM crm_sys_entity WHERE entityid=_relentityid AND relaudit=1 LIMIT 1) THEN--如果关联了审批的实体
				SELECT flowid INTO _flowid FROM crm_sys_workflow WHERE  entityid=_relentityid AND recstatus=1;
				IF _flowid IS NOT NULL OR CAST(_flowid AS TEXT)<>'' THEN
			    	SELECT dynrecid INTO _dynrecid FROM crm_sys_salesstage_dynentity WHERE recid=_recid::uuid AND salesstageid=_cursalesstageid::uuid;
 
						IF _dynrecid IS NOT NULL AND CAST(_dynrecid AS TEXT)<>'' THEN
								 SELECT auditstatus INTO _auditstatus FROM crm_sys_workflow_case WHERE flowid=_flowid AND recid=_dynrecid ORDER BY reccreated DESC LIMIT 1;
								 IF _auditstatus!=1 AND _auditstatus!=2 AND _auditstatus!=(-1)  THEN
											_msg:='阶段流程正在审批中,不能回退';
											Raise EXCEPTION '%',_msg;
								 ELSEIF _auditstatus IS NULL THEN
										--	_msg:='阶段流程还没发起审批,不能推进';
										--	Raise EXCEPTION '%',_msg;
								 END IF;
						END IF;
				END IF;
		END IF;

	 DELETE FROM crm_sys_salesstage_status WHERE recid=_recid::uuid;
	 --raise EXCEPTION '%',_entityid;
	 INSERT INTO crm_sys_salesstage_status(isfinish,salesstageid,recid,reccreator,recupdator,entityid,presalesstageid) VALUES
																					(1,_tmpsalesstageid::uuid,_recid::uuid,_userno,_userno,_entityid,null);

		--发送动态
		IF _need_send_dynamic = 1 THEN
					 
					 --独立实体，如果配了模版,则发模版动态，没有的话，发系统消息
					 --插入推送消息
					_notify_msggroupid=1001;
					_notify_msgdataid:=_recid::uuid;
					_notify_entityid:=_entityid::uuid;
					_notify_msgtype:=1;
					_notify_msgtitle:='销售阶段推进';
					SELECT stagename,winrate INTO _stagestatus,_winrate FROM crm_sys_salesstage_setting 
					WHERE salesstageid =(SELECT salesstageid FROM crm_sys_salesstage_status WHERE recid=_recid::uuid AND entityid=_entityid::uuid LIMIT 1);

					SELECT username INTO _username FROM crm_sys_userinfo WHERE userid=_userno LIMIT 1;
					SELECT recname INTO _opportunityname FROM crm_sys_opportunity WHERE recid=_recid::uuid LIMIT 1;

					_notify_msgcontent:=format('%s把销售阶段修改为%s(赢率%s%s)',_username,_stagestatus,(_winrate*100)::TEXT,'%');

					_sql:='select string_agg(userid,'','') from (select recmanager::text as userid from crm_sys_opportunity where recid=$1
union  select userid::text as userid from crm_sys_opportunity_receiver where recid=$1 union select %s::text) as t';

					EXECUTE format(_sql,_userno) USING _recid::uuid INTO _notify_receiver;

					_notify_creator:=_userno;
				 _dynamictype:=1;
				 _dynamic_typeid:='00000000-0000-0000-0000-000000000001';
				 _dynamic_businessid:=_recid::uuid;
				 _dynamic_entityid:=_entityid::uuid;
				 _notify_msgcontent:=_notify_msgcontent;

				 --插入动态消息
				 SELECT id,flag,msg,stacks INTO _codeid,_codeflag,_codemsg,_codestack FROM crm_func_dynamics_detail_insert(_dynamictype, _dynamic_entityid,_dynamic_businessid,_dynamic_typeid,_recid::uuid,null,_notify_msgcontent,_userno);
				 Raise Notice 'Detail Insert % % % %',_codeid,_codeflag,_codemsg,_codestack;
				 IF _codeflag!=1 THEN
							 Raise EXCEPTION '%',_codemsg;
				 END IF;
					 --写离线消息
					SELECT crm_func_notify_message_insert(
											_notify_msggroupid, _notify_msgdataid, 
											_notify_entityid, _notify_msgtype, _notify_msgtitle, _notify_msgcontent, _notify_msgstatus,
											_notify_msgparam, _notify_sendtime, _notify_receiver, _notify_creator) 
					 INTO _notify_msgid;

					 --主动推消息
					 IF _notify_msgid IS NOT NULL AND _notify_msgid > 0 THEN
									 INSERT INTO crm_sys_notify_sync (notifyid, synctype, syncparam, reccreator) VALUES (_notify_msgid, '0', '{}', _notify_creator);
					 END IF;
		END IF;
 
	  _codeid:=_tmpsalesstageid;
		_codeflag:= 1;
		_codemsg:= '回退销售阶段成功';
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
