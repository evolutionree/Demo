CREATE OR REPLACE FUNCTION "public"."crm_func_salesstage_nextstep_check"("_recid" text, "_typeid" text, "_salesstageids" text, "_relrecid" text, "_userno" int4)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
DECLARE
 
  _opp_sql TEXT;
  r1 record;
  r2 record;
  _entitytable TEXT;
  _recaudits int4:=1;
  _entity_sql TEXT;
  _entityid uuid;
  _msg TEXT:='';
  _stagestatus TEXT:='';
  _id TEXT;
  _laststatus int4:=1;
  _arr_length int4;
  _arr TEXT [];
  _salesstageid TEXT;
  _presalesstageid uuid;
  _orderby int4:=0;
  _isopenhighset BOOLEAN;

  _isfinish int4:=0;
  _resecsalesstageid TEXT;
  _isskip BOOLEAN:=TRUE;
  _relentityid uuid:=null;
  _flowid uuid:=null;
  _dynrecid uuid:=null;
	_auditstatus int4;

  _power_entityid uuid:='00000000-0000-0000-0000-000000000001';
  _power_pagecode TEXT:='EntityStagePage';
  _pagecursor refcursor;
  _datacursor refcursor;
  _crm_func_entity_page_check_visible TEXT;
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

	_recstageid TEXT;

  _fieldconfig json;
  _statusid TEXT;
  _controltype int4;
  _datasource_type TEXT;
  _datasource_sourceid TEXT;
  _status_val TEXT:='';

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

  ---销售阶段信息
  _winrate numeric;
  _username TEXT;
  _opportunityname TEXT;
  _stagename TEXT;
  _sql TEXT;
  _count int4=0;
  _arr_tmp TEXT[];
  _arr_tmp2 TEXT[];
	_tmppresalesstageid text;
BEGIN
	BEGIN	
		--如果是即信，则需要检查是否有进行商机报备通过了，否则不予与推进
		IF _typeid::uuid='a9b17ddd-62a1-4347-8f05-e10d3be5c4f8'::uuid THEN
				IF NOT EXISTS (SELECT 1 FROM crm_sys_workflow_case WHERE flowid='ff9ceec8-20c6-471c-a1a7-fbfcfaa356e1' AND recid=_recid::uuid AND auditstatus=1) THEN
						Raise EXCEPTION '%','请先通过商机报备，再进行商机推进';
				END IF;
		END IF;

     SELECT entityid INTO _entityid FROM crm_sys_entity_category WHERE categoryid=_typeid::uuid;

		 OPEN _pagecursor for execute format('select *  from crm_func_entity_page_check_visible(''%s''::uuid,''%s'',%s,''%s''::uuid,%s)',_entityid::uuid,_power_pagecode,-1,_recid,_userno);  
		 LOOP  
					FETCH  _pagecursor INTO _datacursor ;  

					FETCH  _datacursor INTO _crm_func_entity_page_check_visible;
					IF  FOUND THEN  
							exit;
					ELSE
							Raise EXCEPTION '%','没有权限删除该数据';
					END IF;  
		 END LOOP; 

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
			 _msg:='该阶段已处于'||_stagestatus||'阶段不允许推进';
			 Raise EXCEPTION '%',_msg;
   END IF;

		

    SELECT  (isopenhighsetting=1) INTO _isopenhighset FROM crm_sys_salesstage_type_setting WHERE salesstagetypeid=_typeid::uuid;
		SELECT array_length(
		ARRAY(SELECT UNNEST(string_to_array((_salesstageids),',')))::text[],1) INTO _arr_length;

		SELECT ARRAY(SELECT UNNEST(string_to_array((_salesstageids),',')))::text[] INTO _arr;
		SELECT _arr[_arr_length] INTO _salesstageid;
		if not (  _stagestatus='输单' OR _stagestatus='弃单')then 
					

				SELECT array(
								SELECT salesstageid 
								FROM crm_sys_salesstage_setting 
								WHERE  salesstagetypeid=_typeid::uuid
								and recorder < (Select recorder from crm_sys_salesstage_setting where salesstageid = _salesstageid::uuid limit 1)
								And recstatus = 1
								order by recorder ) into _arr_tmp2;
				
				SELECT array_cat(_arr_tmp2,_arr) INTO _arr;
				raise notice '%',_arr;
		end if;
    IF _isopenhighset=TRUE THEN
						--raise exception '_isopenhighset=1';
						SELECT _arr INTO _arr_tmp;
						IF EXISTS(SELECT 1 FROM crm_sys_salesstage_setting WHERE salesstageid=_salesstageid::uuid AND stagename='输单') THEN
										 _isskip=FALSE;
						ELSE
									SELECT array_remove(_arr,_salesstageid) INTO _arr;
						END IF;
						IF _isskip THEN
									FOR r1 in SELECT unnest(_arr)::uuid AS salesstageid loop
									    _count=_count+1;
											IF NOT EXISTS(SELECT 1 FROM crm_sys_salesstage_setting where salesstageid=r1.salesstageid) THEN
													 Raise EXCEPTION '该销售阶段不存在';
											END IF;

											--关键事件状态判断
											IF EXISTS(SELECT 1 FROM crm_sys_salesstage_event_setting WHERE salesstageid=r1.salesstageid AND recstatus=1) THEN
														IF EXISTS(SELECT 1 FROM crm_sys_salesstage_event WHERE salesstageid=r1.salesstageid AND recid=_recid::uuid ) THEN
																	FOR r2 IN  SELECT isfinish,eventsetid,fileid,isuploadfile FROM crm_sys_salesstage_event WHERE recid=_recid::uuid LOOP

																			IF r2.isfinish=0 THEN
																							_msg:='不满足阶段推进条件，请完善后再推进';
																							Raise EXCEPTION '%',_msg;
																			ELSEIF r2.isfinish IS NULL THEN
																				 IF EXISTS(SELECT 1 FROM crm_sys_salesstage_event_setting WHERE eventsetid=r2.eventsetid::uuid) THEN
																							_msg:='不满足阶段推进条件，请完善后再推进';
																							Raise EXCEPTION '%',_msg;
																				 END IF;
																			ELSE	
																				 if (r2.isuploadfile = 0 and  EXISTS (
																									select * from crm_sys_salesstage_event_setting where eventsetid=r2.eventsetid::uuid and isneedupfile=1)) THEN
																							_msg:='不满足阶段推进条件(文件未上传)，请完善后再推进';
																							Raise EXCEPTION '%',_msg;
																					end if ;
																			END IF;
																END LOOP;
														 ELSE
																	 _msg:='不满足阶段推进条件，请完善后再推进';
																	 Raise EXCEPTION '%',_msg;	  
														 END IF;
											 END IF;
 
											SELECT relentityid INTO _relentityid FROM crm_sys_salesstage_dynentity_setting WHERE  salesstageid=r1.salesstageid;
											IF EXISTS(SELECT 1 FROM crm_sys_entity WHERE entityid=_relentityid AND relaudit=1 LIMIT 1) THEN--如果关联了审批的实体
													SELECT flowid INTO _flowid FROM crm_sys_workflow WHERE  entityid=_relentityid AND recstatus=1;

													IF _flowid IS NOT NULL OR CAST(_flowid AS TEXT)<>'' THEN
															SELECT dynrecid INTO _dynrecid FROM crm_sys_salesstage_dynentity WHERE recid=_recid::uuid AND salesstageid=r1.salesstageid;
															IF _dynrecid IS NOT NULL AND CAST(_dynrecid AS TEXT)<>'' THEN
																	 SELECT auditstatus INTO _auditstatus FROM crm_sys_workflow_case WHERE flowid=_flowid AND recid=_dynrecid ORDER BY reccreated DESC LIMIT 1;

																	 IF _auditstatus=0 OR _auditstatus=3  THEN
																				_msg:='阶段流程正在审批中,不能推进';
																				Raise EXCEPTION '%',_msg;
																	 ELSEIF _auditstatus=2 THEN
																				_msg:='阶段流程审批不通过,请重新发起流程';
																				Raise EXCEPTION '%',_msg;
																	 ELSEIF _auditstatus IS NULL THEN
																			  _msg:='阶段流程还没发起审批,不能推进';
																			  Raise EXCEPTION '%',_msg;
																	 END IF;

															ELSE
																	_msg:='阶段流程还没发起审批,不能推进';
																	Raise EXCEPTION '%',_msg;
															END IF;
													END IF;
											END IF;

											SELECT salesstageid INTO _presalesstageid FROM crm_sys_salesstage_status WHERE  recid=_recid::uuid;

											IF  _presalesstageid IS NOT NULL OR CAST(_presalesstageid AS TEXT)<>'' THEN
													 raise notice 'insert new sale status';
													 DELETE FROM crm_sys_salesstage_status WHERE recid=_recid::uuid;

													 INSERT INTO crm_sys_salesstage_status(isfinish,salesstageid,recid,reccreator,recupdator,entityid,presalesstageid) VALUES
																																	(1,_arr_tmp[_count+1]::uuid,_recid::uuid,_userno,_userno,_entityid,_presalesstageid);

											END IF;
								 END loop;
            ELSE
									SELECT salesstageid INTO _presalesstageid FROM crm_sys_salesstage_status WHERE  recid=_recid::uuid;
									FOR r1 in SELECT unnest(_arr)::uuid AS salesstageid loop
									      _count=_count+1;
												SELECT relentityid INTO _relentityid FROM crm_sys_salesstage_dynentity_setting WHERE  salesstageid=r1.salesstageid;
												IF EXISTS(SELECT 1 FROM crm_sys_entity WHERE entityid=_relentityid AND relaudit=1 LIMIT 1) THEN--如果关联了审批的实体
														SELECT flowid INTO _flowid FROM crm_sys_workflow WHERE  entityid=_relentityid AND recstatus=1;
														IF _flowid IS NOT NULL OR CAST(_flowid AS TEXT)<>'' THEN
																SELECT dynrecid INTO _dynrecid FROM crm_sys_salesstage_dynentity WHERE recid=_recid::uuid AND salesstageid=r1.salesstageid;

																IF _dynrecid IS NOT NULL AND CAST(_dynrecid AS TEXT)<>'' THEN
																		 SELECT auditstatus INTO _auditstatus FROM crm_sys_workflow_case WHERE flowid=_flowid AND recid=_dynrecid ORDER BY reccreated DESC LIMIT 1;
																		 IF _auditstatus=3 OR _auditstatus=0 THEN
																					_msg:='阶段流程正在审批中,不能推进';
																					Raise EXCEPTION '%',_msg;
																		 ELSEIF _auditstatus=2 THEN
																					_msg:='阶段流程未通过审批,不能推进';
																					Raise EXCEPTION '%',_msg;
																		 END IF;
																END IF;
														END IF;
												END IF;

												IF  _presalesstageid IS NOT NULL OR CAST(_presalesstageid AS TEXT)<>'' THEN

														 DELETE FROM crm_sys_salesstage_status WHERE recid=_recid::uuid;

														 INSERT INTO crm_sys_salesstage_status(isfinish,salesstageid,recid,reccreator,recupdator,entityid,presalesstageid) VALUES
																																		(1,_arr_tmp[_count]::uuid,_recid::uuid,_userno,_userno,_entityid,_presalesstageid);

                        ELSE
      
														_msg:='输单阶段信息缺失';
														Raise EXCEPTION '%',_msg;
												END IF;
									END loop;
						END IF;
    ELSE
				
			 IF _isopenhighset=FALSE  AND EXISTS(SELECT 1 FROM crm_sys_salesstage_setting WHERE salesstagetypeid=_typeid::uuid
																						 AND salesstageid=_salesstageid::uuid AND (stagename='赢单' OR stagename='输单')) THEN
					IF _relrecid IS NOT NULL OR _relrecid<>'' THEN
								DELETE FROM crm_sys_salesstage_dynentity WHERE dynrecid=_relrecid::uuid AND recid=_recid::uuid AND salesstageid=_salesstageid::uuid;
								
								INSERT INTO  crm_sys_salesstage_dynentity
													 (salesstageid,
														reccreator,
														recupdator,
														recid,
														dynrecid)
														VALUES
														(_salesstageid::uuid,
														 _userno,
														 _userno,
														 _recid::uuid,
														 _relrecid::uuid
														);
				 ELSE 
						 Raise EXCEPTION '%','非高级模式下,关联销售阶段表单Id不能为空';
				 END IF;
					SELECT salesstageid INTO _presalesstageid FROM crm_sys_salesstage_status WHERE  recid=_recid::uuid;
					raise notice 'test';
					IF  _presalesstageid IS NOT NULL OR CAST(_presalesstageid AS TEXT)<>'' THEN
								raise notice 'insert new sale status';
							 DELETE FROM crm_sys_salesstage_status WHERE recid=_recid::uuid;

							 INSERT INTO crm_sys_salesstage_status(isfinish,salesstageid,recid,reccreator,recupdator,entityid,presalesstageid) VALUES
																											(1,_salesstageid::uuid,_recid::uuid,_userno,_userno,_entityid,_presalesstageid);

					END IF;
			ELSEIF _isopenhighset=FALSE THEN
							SELECT salesstageid INTO _presalesstageid FROM crm_sys_salesstage_status WHERE  recid=_recid::uuid;

							IF  _presalesstageid IS NOT NULL OR CAST(_presalesstageid AS TEXT)<>'' THEN

									 DELETE FROM crm_sys_salesstage_status WHERE recid=_recid::uuid;

									 INSERT INTO crm_sys_salesstage_status(isfinish,salesstageid,recid,reccreator,recupdator,entityid,presalesstageid) VALUES
																													(1,_salesstageid::uuid,_recid::uuid,_userno,_userno,_entityid,_presalesstageid);

							END IF;
			 END IF;
    END IF;


		_opp_sql:='select recstageid::text FROM '||_entitytable||'  WHERE recid='''||_recid||'''::uuid limit 1';

		EXECUTE _opp_sql INTO _recstageid; 
		--如果是即信
		IF _typeid::uuid='a9b17ddd-62a1-4347-8f05-e10d3be5c4f8'::uuid THEN
				--需求方案,这里不更新需求转化时间，该字段由商机报备逻辑更新
				IF _recstageid='6edc4390-016b-4e6f-bdd5-3da84fed0568' THEN
						--UPDATE crm_sys_opportunity SET requtime=now() WHERE recid=_recid::uuid;
				--商务谈判
				ELSEIF _recstageid='b50d2069-bb7e-4a5e-8bc6-df2fb7e8619d' THEN
						UPDATE crm_sys_opportunity SET busstime=now() WHERE recid=_recid::uuid;
				--赢单
				ELSEIF _recstageid='82bc6dfd-0e6d-4376-a50e-042ac1d7909b' THEN
						UPDATE crm_sys_opportunity SET winordertime=now() WHERE recid=_recid::uuid;
				END IF;

		ELSE --如果是玄讯
				--需求调研
				IF _recstageid='2fa3bd97-5e0f-4078-aa8e-a0408054ff74' THEN
						UPDATE crm_sys_opportunity SET requtime=now() WHERE recid=_recid::uuid;
				--商务谈判
				ELSEIF _recstageid='b30dc50c-36e5-4846-b2a2-f5dfed4d20ed' THEN
						UPDATE crm_sys_opportunity SET busstime=now() WHERE recid=_recid::uuid;
				--赢单
				ELSEIF _recstageid='c410243d-6fb5-4c3e-a5bd-4f13bea75ee6' THEN
						UPDATE crm_sys_opportunity SET winordertime=now() WHERE recid=_recid::uuid;
				END IF;
		END IF;

	IF _need_send_dynamic = 1 THEN
					 
					 --独立实体，如果配了模版,则发模版动态，没有的话，发系统消息
					 --插入推送消息
					_notify_msggroupid=1001;
					_notify_msgdataid:=_recid::uuid;
					_notify_entityid:=_entityid::uuid;
					_notify_msgtype:=1;
					_notify_msgtitle:='销售阶段推进';
					SELECT stagename,winrate INTO _stagename,_winrate FROM crm_sys_salesstage_setting 
					WHERE salesstageid =(SELECT salesstageid FROM crm_sys_salesstage_status WHERE recid=_recid::uuid AND entityid=_entityid::uuid LIMIT 1);

					SELECT username INTO _username FROM crm_sys_userinfo WHERE userid=_userno LIMIT 1;
					SELECT recname INTO _opportunityname FROM crm_sys_opportunity WHERE recid=_recid::uuid LIMIT 1;

					_notify_msgcontent:=format('%s把销售阶段修改为%s(赢率%s%s)',_username,_stagename,(_winrate*100)::TEXT,'%');
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


	  _codeid:= _salesstageid;
		_codeflag:= 1;--推进成功
		_codemsg:= '推进销售阶段成功';
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