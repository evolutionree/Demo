CREATE OR REPLACE FUNCTION "public"."crm_func_workflow_case_finishfunc_salesstage"("_caseid" uuid, "_nodenum" int4, "_choicestatus" int4, "_userno" int4)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
	-- select * from 
	-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
	DECLARE
		_relate_status_sql TEXT;
		_check_exist_sql TEXT;
		_reportstatus INT;
		_relate_entitytable TEXT;

		_relentityid uuid;
		_relrecid uuid; 

		_flow_entityid uuid;
		_entityid uuid;
		_tmpentityid uuid;
		_entity_name TEXT;
		_flowid uuid;
		_flow_vernum INT;
		_audituserids TEXT;
		_flowname TEXT;
		_flowcode TEXT;
		_flowcase_creator INT;
		_flow_notice_content TEXT;
		_nodenum_name TEXT;

		_default_tmpdata jsonb;
		_default_tmpformat jsonb;

		_need_send_dynamic INT:=0;
		
		--动态参数
		_dynamictype INT;
		_dynamic_entityid uuid;
		_dynamic_businessid uuid;
		_dynamic_typeid uuid;

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
		---销售阶段信息
		_winrate numeric;
		_username TEXT;
		_opportunityname TEXT;

		--标准返回参数
		_codeid TEXT;
		_codeflag INT:=0;
		_codemsg TEXT;
		_codestacks TEXT;
		_codestatus TEXT;
		 
		_casedata json;
		_salesstageids TEXT:='';
		_salesstageid TEXT:='';
		_salestageuuid uuid:=null;
		_stagename TEXT;
		_presalesstageid uuid:=null;
		_isskip BOOLEAN:=TRUE;
		_ispassevent INT4:=1;
		_ispassinfo INT4:=0;
		_ispassflow INT4:=0;
		_recid uuid:=null;
		_auditstatus int4=0;
		_dynrecid uuid:=null;
		_arr_length int4=0;
		_arr TEXT[];
		_arr_tmp TEXT[];
		r1 record;
		r2 record;
		r3 record;
		_sql TEXT;
		_count int4=0;
	BEGIN

		 BEGIN

			SELECT relentityid,relrecid INTO _relentityid,_relrecid FROM crm_sys_workflow_case_entity_relation WHERE caseid = _caseid LIMIT 1;--获取关联审批实体的 实体id和数据id 如实体1关联了商机实体 即是拿到商机的实体和数据id

			IF _relentityid IS NOT NULL AND _relrecid IS NOT NULL THEN
						SELECT recid INTO _recid FROM crm_sys_workflow_case WHERE caseid=_caseid LIMIT 1;--获取审批实体id

						SELECT casedata INTO _casedata FROM crm_sys_workflow_case_item WHERE caseid=_caseid AND nodenum=0 and stepnum = 0 LIMIT 1;--获取推进的销售阶段id
						_salesstageids:=(_casedata->>'salesstageids')::TEXT;

						SELECT array_length(
						ARRAY(SELECT UNNEST(string_to_array((_salesstageids),',')))::text[],1) INTO _arr_length;
				 
						SELECT ARRAY(SELECT UNNEST(string_to_array((_salesstageids),',')))::text[] INTO _arr;
						SELECT _arr[_arr_length] INTO _salesstageid;
						SELECT _arr INTO _arr_tmp;
						IF EXISTS(SELECT 1 FROM crm_sys_salesstage_setting WHERE salesstageid=_salesstageid::uuid AND stagename='输单') THEN
										 _isskip=FALSE;
						ELSE
									SELECT array_remove(_arr,_salesstageid) INTO _arr;
						END IF;

						IF _isskip THEN

									FOR r1 in SELECT unnest(_arr)::uuid AS salesstageid loop
										_count=_count+1;
										IF EXISTS(SELECT 1 FROM crm_sys_salesstage_setting where salesstageid=r1.salesstageid) THEN
						 --------------------------------------------------关键事件状态判断--------------------------------------------------
											IF EXISTS(SELECT 1 FROM crm_sys_salesstage_event_setting WHERE salesstageid=r1.salesstageid AND recstatus=1) THEN
													IF EXISTS(SELECT 1 FROM crm_sys_salesstage_event WHERE salesstageid=r1.salesstageid AND recid=_relrecid ) THEN
															FOR r2 IN  SELECT isfinish,eventsetid FROM crm_sys_salesstage_event WHERE recid=_relrecid LOOP

																	IF r2.isfinish=1 THEN
																			IF _ispassevent=0 THEN
																				 _ispassevent=0;
																			ELSE
																				_ispassevent=1;
																			END IF;
																	ELSE
																		 _ispassevent=0;
																	END IF;
															END LOOP; 
													 ELSE
																 _ispassevent=0;
													 END IF;
											 ELSE
											 
											 END IF;

						 --------------------------------------------------关键信息状态判断--------------------------------------------------
											IF  EXISTS(SELECT 1 FROM crm_sys_salesstage_info_setting WHERE salesstageid=r1.salesstageid LIMIT 1) AND
													EXISTS(SELECT 1 FROM crm_sys_salesstage_info WHERE salesstageid=r1.salesstageid AND recid=_relrecid LIMIT 1)   THEN
														_ispassinfo=1;
											ELSEIF NOT EXISTS(SELECT 1 FROM crm_sys_salesstage_info_setting WHERE salesstageid=r1.salesstageid  LIMIT 1) THEN
														_ispassinfo=1;
                      ELSE
														_ispassinfo=0;
											END IF;

						 --------------------------------------------------流程状态判断--------------------------------------------------

											SELECT relentityid INTO _entityid FROM crm_sys_salesstage_dynentity_setting WHERE  salesstageid=r1.salesstageid;

											IF EXISTS(SELECT 1 FROM crm_sys_entity WHERE entityid=_entityid AND relaudit=1 LIMIT 1) THEN--如果关联了审批的实体
													SELECT flowid INTO _flowid FROM crm_sys_workflow WHERE  entityid=_entityid AND recstatus=1 ORDER BY reccreated DESC LIMIT 1 ;

													IF _flowid IS NOT NULL OR CAST(_flowid AS TEXT)<>'' THEN
															SELECT dynrecid INTO _dynrecid FROM crm_sys_salesstage_dynentity WHERE recid=_relrecid AND salesstageid=r1.salesstageid;
															IF _dynrecid IS NOT NULL AND CAST(_dynrecid AS TEXT)<>'' THEN
																	 SELECT auditstatus INTO _auditstatus FROM crm_sys_workflow_case WHERE flowid=_flowid AND recid=_dynrecid ORDER BY reccreated DESC LIMIT 1;
																	 IF _auditstatus=1 THEN
																			_ispassflow=1;
																	 ELSE
																			_ispassflow=0;
																	 END IF;
															ELSE
																	_ispassflow=0;
															END IF;
													 ELSE
															 _ispassflow=1;
													 END IF;
											ELSE 
													_ispassflow=1;
											END IF;
 
											IF _ispassflow=1 AND _ispassevent=1 AND _ispassinfo=1   THEN

													SELECT salesstageid INTO _presalesstageid FROM crm_sys_salesstage_status WHERE  recid=_relrecid;

													IF  _presalesstageid IS NOT NULL OR CAST(_presalesstageid AS TEXT)<>'' THEN

															 DELETE FROM crm_sys_salesstage_status WHERE recid=_relrecid::uuid;

															 INSERT INTO crm_sys_salesstage_status(isfinish,salesstageid,recid,reccreator,recupdator,entityid,presalesstageid) VALUES
																																			(1,_arr_tmp[_count+1]::uuid,_relrecid::uuid,_userno,_userno,_relentityid,_presalesstageid);
															 _need_send_dynamic=1;
													END IF;
											END IF; 
									 END IF;
								 END loop;

						ELSE
								SELECT salesstageid,entityid INTO _presalesstageid,_tmpentityid FROM crm_sys_salesstage_status WHERE  recid=_relrecid;
								IF  _presalesstageid IS NOT NULL OR CAST(_presalesstageid AS TEXT)<>'' THEN
										 DELETE FROM crm_sys_salesstage_status WHERE recid=_relrecid::uuid;
										_entityid = _tmpentityid;
										 raise EXCEPTION '_entityid=%',_entityid;
										 INSERT INTO crm_sys_salesstage_status(isfinish,salesstageid,recid,reccreator,recupdator,entityid,presalesstageid) VALUES
																														(_ispassinfo,_salesstageid::uuid,_relrecid::uuid,_userno,_userno,_entityid,_presalesstageid);
										 _need_send_dynamic=1;
								END IF;
						END IF;
				ELSE
							Raise EXCEPTION '该流程必须关联实体';
				END IF;

					--发送动态
					IF _need_send_dynamic = 1 THEN
								 
								 --独立实体，如果配了模版,则发模版动态，没有的话，发系统消息
								 --插入推送消息
								_notify_msggroupid=1006;
								_notify_msgdataid:=_relrecid;
								_notify_entityid:=_relentityid;
								_notify_msgtype:=1;
								_notify_msgtitle:='销售阶段推进';
								SELECT stagename,winrate INTO _stagename,_winrate FROM crm_sys_salesstage_setting 
								WHERE salesstageid =(SELECT salesstageid FROM crm_sys_salesstage_status WHERE recid=_relrecid AND entityid=_relentityid LIMIT 1);
								SELECT username INTO _username FROM crm_sys_userinfo WHERE userid=_userno LIMIT 1;
								SELECT recname INTO _opportunityname FROM crm_sys_opportunity WHERE recid=_relrecid LIMIT 1;
								_notify_msgcontent:=format('%s把销售阶段修改为%s(赢率%s%s)',_username,_stagename,(_winrate*100)::TEXT,'%');
								_sql:='select string_agg(userid,'','') from (select recmanager::text as userid from crm_sys_opportunity where recid=$1
	 union  select userid::text as userid from crm_sys_opportunity_receiver where recid=$1 union select %s::text) as t';

								EXECUTE format(_sql,_userno) USING _relrecid INTO _notify_receiver;

								_notify_creator:=_userno;
							 _dynamictype:=1;
							 _dynamic_typeid:='00000000-0000-0000-0000-000000000001';
							 _dynamic_businessid:=_relrecid;
							 _dynamic_entityid:=_relentityid;
							 _notify_msgcontent:=_notify_msgcontent;
							 raise notice '%',_dynamictype;
							 raise notice '%',_dynamic_entityid;
							 raise notice '%',_dynamic_businessid;
							 raise notice '%',_dynamic_typeid;
							 raise notice '%',_caseid;
							 raise notice '%',_default_tmpdata;
							 raise notice '%',_notify_msgcontent;
							 raise notice '%',_dynamictype;

							 --插入动态消息
							 SELECT id,flag,msg,stacks INTO _codeid,_codeflag,_codemsg,_codestacks FROM crm_func_dynamics_detail_insert(_dynamictype, _dynamic_entityid,_dynamic_businessid,_dynamic_typeid,_caseid,_default_tmpdata::TEXT,_notify_msgcontent,_userno);
							 Raise Notice 'Detail Insert % % % %',_codeid,_codeflag,_codemsg,_codestacks;
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

						_codeid:= _caseid::TEXT;
						_codeflag:= 1;
						_codemsg:= '初始化完成';
		 EXCEPTION WHEN OTHERS THEN
							 GET STACKED DIAGNOSTICS _codestacks = PG_EXCEPTION_CONTEXT;
							 _codemsg:=SQLERRM;
							 _codestatus:=SQLSTATE;
			END;
		 
				--RETURN RESULT
			RETURN QUERY EXECUTE format('SELECT $1,$2,$3,$4,$5')
			USING  _codeid,_codeflag,_codemsg,_codestacks,_codestatus;

	END
	$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;