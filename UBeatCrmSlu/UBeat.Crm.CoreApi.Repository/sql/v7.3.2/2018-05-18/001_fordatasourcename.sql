/**
处理数据源时，在详情中显示最新的引用对象的名称
*/
CREATE OR REPLACE FUNCTION "public"."crm_func_entity_protocol_extrainfo_fetch_fordetail"("_entityid" uuid, "_viewtype" int4)
  RETURNS "pg_catalog"."text" AS $BODY$
--SELECT crm_func_entity_protocol_extrainfo_fetch('e0771780-9883-456a-98b9-372d9888e0ac',-1)

/*
控件类型：
  1 文本 (输入任何字符 正则校验 长度校验区分最短最长 非空 只读 新增 编辑规则)
  2 提示文本（只读 跳转读取内容）
  3 单选(本地字典表)
  4 多选(本地字典表)
  5 大文本(输入任何字符 正则校验 长度校验区分最短最长 非空 只读 新增 编辑规则)
  6 整数文本(输入任何字符 正则校验 长度校验区分最短最长 非空 只读 新增 编辑规则 范围)
  7 小数文本（小数点）
  8 日期（年月日）（年  年月  年月日 月日  时间范围）
  9 日期时间（显示规则）
  10 手机号(11位 正则 点击拨号)
  11 邮箱
  12 电话(正则 点击拨号)
  13 地址(地图选择  经纬度中文)
  14 定位(经纬度中文)
  15 头像(圆方 修改 删除 压缩大小)
  16 行政区域
  17 团队组织
  18 数据源单选（entityid   relaid  通用接口 联动规则 关联id 显示隐藏其他字段）
  19数据源多选（entityid   relaid  通用接口 联动规则 关联id 显示隐藏其他字段）
  20 分组
  21 树形控件
  22 图片控件(拍照 选择图片 数量可变)
  23 附件控件（web可上传 大小  格式 数量  其他端下载查看）
  24 表格控件
  25 单选人控件
  26 多选人控件
27  树形控件多选
28产品
29产品系列
*/

DECLARE
   _entity_tablename VARCHAR:='';

  _para_col_format TEXT:='';
  _para_col_value TEXT:='';
  _para_col_split_flag TEXT:=',';
  _para_col_new_name TEXT:='';

  _datasource_type TEXT;
  _datasource_sourceid TEXT;

   _r record; 
   _condition_array TEXT[];
   _execute_sql TEXT:='';
		_ds_multi int4:=0;
		_ds_id text;

BEGIN

  --Routine body goes here...
     IF _entityid IS NULL THEN
          RETURN _execute_sql;
          --Raise EXCEPTION '实体ID不能为空';
     END IF;

     --获取该实体表的表名
      SELECT entitytable INTO _entity_tablename FROM crm_sys_entity WHERE entityid=_entityid LIMIT 1;
      IF _entity_tablename IS NULL OR _entity_tablename='' THEN
          Raise EXCEPTION '找不到该实体';
      END IF;

			--获取实体特定的字段
      FOR _r IN SELECT fieldid,fieldname,fieldconfig,controltype FROM crm_sys_entity_fields WHERE entityid = _entityid AND recstatus = 1 LOOP
              -- Raise Notice 'FieldName %',_r.fieldname;
							_para_col_new_name:=_r.fieldname || '_name';
							_para_col_format:='';
							if( _r.controltype  = 18 )THEN
								_ds_multi:=_r.fieldconfig#>>'{multiple}';
                _ds_id:=_r.fieldconfig#>>'{dataSource,sourceId}';
							end if;
							CASE _r.controltype 
                     WHEN 31 THEN

                         IF _r.fieldname='predeptgroup' THEN
														_para_col_format:='crm_func_entity_protocol_format_predepartment(e.recmanager) AS ' || _para_col_new_name;
                         ELSEIF _r.fieldname='deptgroup' THEN
														_para_col_format:='crm_func_entity_protocol_format_belongdepartment(e.recmanager) AS ' || _para_col_new_name;
                         ELSE
                          	_para_col_format:='crm_func_entity_protocol_format_quote_control(row_to_json(e)::json,'''||_entityid||''','''||_r.fieldid||''') AS ' || _para_col_new_name;
                         END IF;
                     WHEN 1002 THEN
													_para_col_format:='crm_func_entity_protocol_format_userinfo(e.' || _r.fieldname || ') AS ' || _para_col_new_name;  
                     WHEN 1013 THEN
													_para_col_format:='crm_func_entity_protocol_format_salesstage(e.' || _r.fieldname || ') AS ' || _para_col_new_name;    
                     WHEN  1003 THEN
													_para_col_format:='crm_func_entity_protocol_format_userinfo(e.' || _r.fieldname || ') AS ' || _para_col_new_name;    
                     WHEN 1006 THEN
													_para_col_format:='crm_func_entity_protocol_format_userinfo(e.' || _r.fieldname || ') AS ' || _para_col_new_name; 
                     WHEN 18 THEN
													_para_col_format:='crm_func_entity_protocol_format_ds_fordetail(e.' || _r.fieldname || ','|| _ds_multi ||','''|| _ds_id ||''') AS ' || _para_col_new_name;    
                     WHEN 25 THEN
                           --创建人
                           --更新人
													 --负责人
														_para_col_format:='crm_func_entity_protocol_format_userinfo_multi(e.' || _r.fieldname || ') AS ' || _para_col_new_name;    
                     WHEN 26 THEN
                           --多选人
													_para_col_format:='crm_func_entity_protocol_format_userinfo_multi(e.' || _r.fieldname || ') AS ' || _para_col_new_name;    
                     WHEN 1008 THEN
													--记录状态
													 _para_col_format:='crm_func_entity_protocol_format_recstatus(e.' || _r.fieldname || ') AS ' || _para_col_new_name;    
                     WHEN 8 THEN
														_para_col_format:='crm_func_entity_protocol_format_time(e.' || _r.fieldname || ',''YYYY-MM-DD'') AS ' || _para_col_new_name;
                     WHEN 9 THEN
														_para_col_format:='crm_func_entity_protocol_format_time(e.' || _r.fieldname || ',''YYYY-MM-DD HH24:MI:SS'') AS ' || _para_col_new_name;
                     WHEN 1004 THEN
													  _para_col_format:='crm_func_entity_protocol_format_time(e.' || _r.fieldname || ',''YYYY-MM-DD HH24:MI:SS'') AS ' || _para_col_new_name;
                     WHEN 1005 THEN
													--更新时间	1005
													--创建时间	1004
													--9 日期时间（显示规则)
													_para_col_format:='crm_func_entity_protocol_format_time(e.' || _r.fieldname || ',''YYYY-MM-DD HH24:MI:SS'') AS ' || _para_col_new_name;
                     WHEN 16 THEN
                          --行政区域
												  _para_col_format:='crm_func_entity_protocol_format_region(e.' || _r.fieldname || ') AS ' || _para_col_new_name;    
										WHEN 28 THEN
                          --产品
												  _para_col_format:='crm_func_entity_protocol_format_product_multi(e.' || _r.fieldname || ') AS ' || _para_col_new_name;  
										WHEN 29 THEN
                          --产品系列
												  _para_col_format:='crm_func_entity_protocol_format_productserial_multi(e.' || _r.fieldname || ') AS ' || _para_col_new_name;    
                     WHEN 1007 THEN
                            --审批状态
														_para_col_format:='crm_func_entity_protocol_format_workflow_auditstatus(e.' || _r.fieldname || ') AS ' || _para_col_new_name;    
                     WHEN 17 THEN
                          --部门
													_para_col_format:='crm_func_entity_protocol_format_dept_multi(e.' || _r.fieldname || '::TEXT) AS ' || _para_col_new_name;    
                     WHEN 13 THEN
														_para_col_format:='crm_func_entity_protocol_format_address(e.' || _r.fieldname || ') AS ' || _para_col_new_name;    
                     WHEN 14 THEN
                     --    --定位、地址
										 		  _para_col_format:='crm_func_entity_protocol_format_address(e.' || _r.fieldname || ') AS ' || _para_col_new_name;    
                     WHEN 1009 THEN
													_para_col_format:='crm_func_entity_protocol_format_rectype(e.' || _r.fieldname || ') AS ' || _para_col_new_name;    
                     WHEN 3 THEN
                              --单选
                            _datasource_type:=_r.fieldconfig#>>'{dataSource,type}';
                            _datasource_sourceid:=_r.fieldconfig#>>'{dataSource,sourceId}';
                            CASE _datasource_type
                                  WHEN 'local' THEN
																				_para_col_format:='crm_func_entity_protocol_format_dictionary(''' || _datasource_sourceid || ''',e.' || _r.fieldname || '::TEXT) AS ' || _para_col_new_name;    
                                  WHEN 'network' THEN
                                  ELSE
                            END CASE;
                     WHEN 4 THEN
                              --多选
														_datasource_type:=_r.fieldconfig#>>'{dataSource,type}';
                            _datasource_sourceid:=_r.fieldconfig#>>'{dataSource,sourceId}';
                            CASE _datasource_type
                                  WHEN 'local' THEN
																				_para_col_format:='crm_func_entity_protocol_format_dictionary(''' || _datasource_sourceid || ''',e.' || _r.fieldname || '::TEXT) AS ' || _para_col_new_name;    
                                  WHEN 'network' THEN
                                  ELSE
                            END CASE;
                     ELSE
              END CASE;

							IF _para_col_format!='' THEN
												 _condition_array:=array_append(_condition_array,_para_col_format);
              END IF;
      END LOOP;

			IF array_length(_condition_array, 1) > 0 THEN
							_execute_sql:=_execute_sql || array_to_string(_condition_array, _para_col_split_flag);
			END IF;

     Raise Notice '_entity_sql %',_execute_sql;
     IF _execute_sql IS NOT NULL AND _execute_sql!='' THEN
             _execute_sql:=format(' ,%s',_execute_sql);
     END IF;
     RETURN _execute_sql;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
;


CREATE OR REPLACE FUNCTION "public"."crm_func_entity_protocol_format_ds_fordetail"(IN "_dsjson" jsonb, IN "_ismulti" int4, IN "_datasourceid" text, OUT "formatname" text)
  RETURNS "pg_catalog"."text" AS $BODY$
declare 
	_rulesql text;
	_did text;
	_name text;
rec record;
BEGIN
		/***
	来源于crm_func_entity_protocol_format_ds，主要为了解决数据源以json格式存储，导致无法更新最新的名称的问题，
	这里通过新增函数，而不是改原来的函数，主要为了避免如果是获取列表时调用，就会严重影响性能。
	
***/
			_did:=_dsjson->>'id';
		if (_ismulti =1 ) then 
			formatname:=_dsjson->>'name';
		else 
			select rulesql into _rulesql  from crm_sys_entity_datasource where datasrcid::text = _datasourceid;
			if(_rulesql is null ) then 
				_rulesql := '';
			end if;
			_rulesql:=replace(_rulesql,'{querydata}','''''');
		--raise notice '%',_rulesql;
			_rulesql:=replace(_rulesql,'{currentUser}','1');
			
			--raise notice '%',_rulesql;
		
			_rulesql:='select total.name from ('|| _rulesql|| ') total where total.id::text=''' || _did||'''';
			for rec in EXECUTE _rulesql loop
					formatname :=rec.name;
			end loop;
			if formatname is null THEN
				formatname:=_dsjson->>'name';
			end if;
		end if;
	EXCEPTION WHEN OTHERS THEN
			raise notice '%',SQLERRM;
			formatname:=_dsjson->>'name';
END;
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
;


CREATE OR REPLACE FUNCTION "public"."crm_func_entity_protocol_data_detail"("_entityid" uuid, "_recid" uuid, "_needpower" int4, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_entity_protocol_data_detail('e0771780-9883-456a-98b9-372d9888e0ac', '6a62ac9b-210f-4247-9b1e-42f5e5c5103c', 1);
--FETCH ALL FROM datacursor;

DECLARE

  _execute_sql TEXT;
  _table_name TEXT;
  _role_power_sql TEXT;
  _field_extra_format_sql TEXT:='';

  _extra_func TEXT;
  _isfollowed_sql TEXT;
  _follow_user_sql TEXT;

  --分页标准参数
  _datacursor refcursor:= 'datacursor';

BEGIN

 
   IF _recid IS NULL THEN
         Raise EXCEPTION '数据ID不能为空';
   END IF;

   SELECT funcname INTO _extra_func FROM crm_sys_entity_func_event WHERE typeid = _entityid AND operatetype = 2 LIMIT 1;
   IF _extra_func IS NOT NULL AND _extra_func!='' THEN
         _execute_sql:='SELECT * FROM %s($1,$2,$3,$4)';
         RETURN QUERY EXECUTE format(_execute_sql,_extra_func)
         USING  _entityid,_recid,_needpower,_userno;
         RETURN;
   END IF;

   SELECT entitytable INTO _table_name FROM crm_sys_entity WHERE entityid = _entityid LIMIT 1;
   IF _table_name IS NULL OR _table_name = '' THEN
         Raise EXCEPTION '未找到相关的表';
   END IF;



   --获取数据权限
    IF _needpower = 0 THEN
           _role_power_sql:=format('
                    SELECT * FROM %s AS e
                    WHERE 1 = 1
              ',_table_name);
    ELSE
          _role_power_sql:=crm_func_role_rule_fetch_sql(_entityid,_userno);
    END IF;

   --获取额外字段语句
   _field_extra_format_sql:=crm_func_entity_protocol_extrainfo_fetch_fordetail(_entityid,-1);

   Raise Notice '_field_extra_format_sql %',_field_extra_format_sql;
   Raise Notice '_role_power_sql %',_role_power_sql;

   _isfollowed_sql:=format('(select count(1) from crm_sys_entity_record_follow where entityid=''%s'' and followdataid=''%s'' and userid=%s) as isfollowed',_entityid,_recid,_userno);
   _follow_user_sql:=format('(select string_agg(userid::text,'','') from crm_sys_entity_record_follow where entityid=''%s'' and followdataid=''%s'' ) as followusers',_entityid,_recid);

  _execute_sql:=format('
    SELECT t.*
    FROM (
          SELECT * %s,%s,%s FROM (
                  %s
          ) AS e
          WHERE recid = $1
    ) AS t  
  ',_field_extra_format_sql,_isfollowed_sql,_follow_user_sql,_role_power_sql);
   Raise Notice '_execute_sql %',_execute_sql;

  _execute_sql:=crm_func_role_rule_param_format(_execute_sql,_userno);
   Raise Notice '_execute_sql %',_execute_sql;
  OPEN _datacursor FOR EXECUTE _execute_sql USING _recid;
  RETURN NEXT _datacursor;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;

CREATE OR REPLACE FUNCTION "public"."crm_func_entity_protocol_data_detail_custcommon"("_entityid" uuid, "_recid" uuid, "_needpower" int4, "_userno" int4)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_entity_protocol_data_detail('e0771780-9883-456a-98b9-372d9888e0ac', '6a62ac9b-210f-4247-9b1e-42f5e5c5103c', 1);
--FETCH ALL FROM datacursor;

DECLARE

  _execute_sql TEXT;
  _table_name TEXT;
  _role_power_sql TEXT;
  _field_extra_format_sql TEXT:='';

  _extra_func TEXT;

  _isfollowed_sql TEXT;
  _follow_user_sql TEXT;


  --分页标准参数
  _datacursor refcursor:= 'datacursor';

BEGIN
   
   IF _recid IS NULL THEN
         Raise EXCEPTION '数据ID不能为空';
   END IF;
   IF NOT EXISTS(SELECT 1 FROM crm_sys_customer WHERE recstatus=1 AND recid=_recid) THEN
        Raise EXCEPTION '%','该客户已删除';
   END IF;

   SELECT entitytable INTO _table_name FROM crm_sys_entity WHERE entityid = _entityid LIMIT 1;
   IF _table_name IS NULL OR _table_name = '' THEN
         Raise EXCEPTION '未找到相关的表';
   END IF;

   --获取数据权限
    IF _needpower = 0 THEN
					 _role_power_sql:=format('
										SELECT * FROM %s AS e
										WHERE 1 = 1
							',_table_name);
    ELSE
					_role_power_sql:=crm_func_role_rule_fetch_sql(_entityid,_userno);
    END IF;

   --获取额外字段语句
   _field_extra_format_sql:=crm_func_entity_protocol_extrainfo_fetch_fordetail(_entityid,-1);

   Raise Notice '_field_extra_format_sql %',_field_extra_format_sql;
   Raise Notice '_role_power_sql %',_role_power_sql;


   _isfollowed_sql:=format('(select count(1) from crm_sys_entity_record_follow where entityid=''%s'' and followdataid=''%s'' and userid=%s) as isfollowed',_entityid,_recid,_userno);
   _follow_user_sql:=format('(select string_agg(userid::text,'','') from crm_sys_entity_record_follow where entityid=''%s'' and followdataid=''%s'' ) as followusers',_entityid,_recid);


  _execute_sql:=format('
    SELECT t.*,
    case when exists(SELECT 1 FROM crm_sys_custcommon_customer_relate WHERE commonid IN (SELECT commonid FROM crm_sys_custcommon_customer_relate WHERE custid=$1) HAVING(count(1)>1)) 
         then 
						(select array_to_string(ARRAY(SELECT unnest(array_agg(fieldname))),'','') 
						from crm_sys_entity_fields WHERE entityid=''ac051b46-7a20-4848-9072-3b108f1de9b0'' AND recstatus=1)
         else
          null
         end as commmon_fields
    FROM (
					SELECT * %s,%s,%s FROM (
									%s
					) AS e
          WHERE recid = $1
    ) AS t  
  ',_field_extra_format_sql,_isfollowed_sql,_follow_user_sql,_role_power_sql);
   Raise Notice '_execute_sql %',_execute_sql;


/*
  _execute_sql:=format('
    SELECT t.*
    FROM (
          SELECT * %s,%s,%s FROM (
                  %s
          ) AS e
          WHERE recid = $1
    ) AS t  
  ',_field_extra_format_sql,_isfollowed_sql,_follow_user_sql,_role_power_sql);

 */
	_execute_sql:=crm_func_role_rule_param_format(_execute_sql,_userno);
   Raise Notice '_execute_sql %',_execute_sql;
  OPEN _datacursor FOR EXECUTE _execute_sql USING _recid;
	RETURN NEXT _datacursor;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;