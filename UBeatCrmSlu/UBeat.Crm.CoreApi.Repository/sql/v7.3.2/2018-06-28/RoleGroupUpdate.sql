CREATE OR REPLACE FUNCTION "public"."crm_func_role_group_edit"("_rolegroupid" text, "_groupname" text, "_userno" int4,_grouplanguage jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE


  --��׼���ز���
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;
BEGIN

   BEGIN
				 --�����߼�
				 IF _groupname IS NULL OR _groupname = '' THEN
								Raise EXCEPTION '��ɫ�������Ʋ���Ϊ��';
				 END IF;
				 IF _rolegroupid IS NULL OR _rolegroupid = '' THEN
								Raise EXCEPTION '��ɫ����Id����Ϊ��';
				 END IF;
				 IF NOT EXISTS(SELECT 1 FROM crm_sys_role_group WHERE rolegroupid=_rolegroupid::uuid ) THEN
								Raise EXCEPTION '�ý�ɫ���鲻����';
				 END IF;
				 IF  EXISTS(SELECT 1 FROM crm_sys_role_group WHERE rolegroupid<>_rolegroupid::uuid AND rolegroupname=_groupname AND recstatus=1 ) THEN
								Raise EXCEPTION '�ý�ɫ�����Ѵ���';
				 END IF;
 
				 UPDATE crm_sys_role_group SET rolegroupname=_groupname,grouplanguage=_grouplanguage WHERE rolegroupid=_rolegroupid::uuid;
 
					_codeid:= _rolegroupid::TEXT;
					_codeflag:= 1;
					_codemsg:= '���½�ɫ����ɹ�';
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
