CREATE OR REPLACE FUNCTION "public"."crm_func_salesstage_list"("_salesstagetypeid" text, "_userno" int4, "_foradmin" int4=0)
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
--SELECT crm_func_account_userinfo_dept_list('', '', '7f74192d-b937-403f-ac2a-8be34714278b', 1,1,-1);
--FETCH ALL FROM datacursor;
--FETCH ALL FROM pagecursor;

DECLARE
  
  _sql_where TEXT:='';
  _execute_sql TEXT;
 _winstageid  uuid:=null;
	_losestageid uuid:=null;
  _datacursor refcursor:= 'datacursor';
 

BEGIN
		if not exists  (select 1 from crm_sys_entity_category  where categoryid=_salesstagetypeid::uuid and  entityid='2c63b681-1de9-41b7-9f98-4cf26fd37ef1'::uuid ) THEN
				_execute_sql = 'select  
        s.salesstageid,
				s.stagename, 
				COALESCE(s.winrate*100,''0'')||''%'' as winrate,
        c.categoryid reltypeid,
        d.relentityid,
        en.entityname relentityname,s.recstatus
				from crm_sys_salesstage_setting s left join crm_sys_salesstage_dynentity_setting d on s.salesstageid=d.salesstageid
        left join crm_sys_entity_category c on c.entityid=d.relentityid left join crm_sys_entity en on en.entityid=c.entityid
				where 1<>1';
				OPEN _datacursor FOR EXECUTE _execute_sql;
				RETURN NEXT _datacursor;
				return ;
		end if;
    IF ( not exists (select * from crm_sys_salesstage_setting where stagename = '输单' and salesstagetypeid =_salesstagetypeid::uuid )) THEN
			insert into crm_sys_salesstage_setting(stagename,winrate,recorder,recstatus,reccreated,recupdated,reccreator,recupdator,salesstagetypeid)
			values('输单',0,100,1,now(),now(),_userno,_userno,_salesstagetypeid::uuid);
			select salesstageid into _losestageid  from crm_sys_salesstage_setting where  stagename = '输单' and salesstagetypeid =_salesstagetypeid::uuid limit 1;
			insert into crm_sys_salesstage_dynentity_setting(relentityid,salesstageid,recorder,recstatus,reccreator,recupdator,reccreated,recupdated)
			values('aa3222ac-767b-4bbf-8c0e-deae42760f05'::uuid,_losestageid,0,1,_userno,_userno,now(),now());

		end if;

		IF ( not exists (select * from crm_sys_salesstage_setting where stagename = '赢单' and salesstagetypeid =_salesstagetypeid::uuid )) THEN
			insert into crm_sys_salesstage_setting(stagename,winrate,recorder,recstatus,reccreated,recupdated,reccreator,recupdator,salesstagetypeid)
			values('赢单',1,100,1,now(),now(),_userno,_userno,_salesstagetypeid::uuid);
			--select salesstageid into _winstageid  from crm_sys_salesstage_setting where  stagename = '赢单' and salesstagetypeid =_salesstagetypeid::uuid limit 1;
			--insert into crm_sys_salesstage_dynentity_setting(relentityid,salesstageid,recorder,recstatus,reccreator,recupdator,reccreated,recupdated)
			--values('7e9f92f4-8ca0-4c13-8c7b-6b8e40e0a695'::uuid,_winstageid,0,1,_userno,_userno,now(),now());
		end if;
   _sql_where:=' and salesstagetypeid='''||_salesstagetypeid||'''::uuid';
   _execute_sql:='
        select  
        s.salesstageid,
				s.stagename,
s.stagename_lang, 
				COALESCE(s.winrate*100,''0'')||''%'' as winrate,
        c.categoryid reltypeid,
        d.relentityid,
        en.entityname relentityname,s.recstatus,s.recorder
				from crm_sys_salesstage_setting s left join crm_sys_salesstage_dynentity_setting d on s.salesstageid=d.salesstageid
        left join crm_sys_entity_category c on c.entityid=d.relentityid left join crm_sys_entity en on en.entityid=c.entityid    where (s.recstatus=1 or( s.recstatus=0 and 1='||_foradmin||')) '||_sql_where||' order by s.recorder asc';

    RAISE NOTICE '%',_execute_sql;
 
  OPEN _datacursor FOR EXECUTE _execute_sql;
	RETURN NEXT _datacursor;
 

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;

