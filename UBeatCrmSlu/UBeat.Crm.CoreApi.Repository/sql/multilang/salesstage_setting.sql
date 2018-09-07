alter table crm_sys_salesstage_setting add stagename_lang jsonb;

----------------------

drop function "crm_func_stage_setting_add"("_stagename" text, "_winrate" numeric, "_typeid" text, "_userno" int4);

--------------------------

CREATE OR REPLACE FUNCTION "public"."crm_func_stage_setting_add"("_stagename" text, "_winrate" numeric, "_typeid" text, "_userno" int4,_stagename_lang text)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE
 
    _salesstageid uuid:=null;
    _orderby int4;
  --��׼���ز���
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

BEGIN

   BEGIN

				 --�����߼�
				 IF  EXISTS (select 1 from crm_sys_salesstage_setting where stagename=_stagename and salesstagetypeid =_typeid::uuid and recstatus in (0,1)  limit 1) THEN  
								Raise EXCEPTION '���۽׶����Ʋ����ظ�';
				 END IF;
 
        SELECT (COALESCE(MAX(recorder),0)+1) into _orderby from crm_sys_salesstage_setting where salesstagetypeid =_typeid::uuid  and stagename <>'Ӯ��' and  
					stagename <>'�䵥';

         IF _stagename='Ӯ��' THEN
              _orderby=100;  
         END IF;

         IF _stagename='�䵥' THEN
              _orderby=101;  
         END IF;
        INSERT INTO "public"."crm_sys_salesstage_setting" (
        stagename,
        winrate,
        salesstagetypeid,
        "recorder", 
        "reccreator", 
        "recupdator",
stagename_lang) 
        VALUES(
        _stagename,
        _winrate,
        _typeid::uuid,
        _orderby,
        _userno,
        _userno,
_stagename_lang::jsonb
        )  returning salesstageid into _salesstageid;
				--�䵥��Ӯ���Զ����Ӽ�����
				if (_stagename <> 'Ӯ��' and _stagename <> '�䵥') then 
					update crm_sys_salesstage_setting set recorder = _orderby+1 where stagename = 'Ӯ��'  and  salesstagetypeid =_typeid::uuid  ;
					update crm_sys_salesstage_setting set recorder = _orderby+2 where stagename = '�䵥'  and  salesstagetypeid =_typeid::uuid  ;
				end if;
				_codeid:= _salesstageid::TEXT;
				_codeflag:= 1;
				_codemsg:= '�������۽׶γɹ�';
 
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


------------------------------------

drop function "crm_func_stage_setting_edit"("_salesstageid" text, "_stagename" text, "_winrate" numeric, "_userno" int4);



----------------------------------



CREATE OR REPLACE FUNCTION "public"."crm_func_stage_setting_edit"("_salesstageid" text, "_stagename" text, "_winrate" numeric, "_userno" int4,_stagename_lang text)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE

    _orderby int4;
  --��׼���ز���
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;
	_salesstagetypeid uuid;

BEGIN

   BEGIN
				select salesstagetypeid into _salesstagetypeid   from  crm_sys_salesstage_setting where  salesstageid = _salesstageid::uuid limit 1  ;
				 --�����߼�
				 IF  EXISTS (SELECT 1 FROM crm_sys_salesstage_setting WHERE salesstagetypeid =_salesstagetypeid  and salesstageid<>_salesstageid::uuid AND stagename=_stagename  and recstatus in (0,1) LIMIT 1) THEN  
								Raise EXCEPTION '���۽׶����Ʋ����ظ�';
				 END IF;
 

 
        UPDATE "public"."crm_sys_salesstage_setting" SET
        stagename=_stagename,
        winrate=_winrate,
        "recupdator"=_userno,
stagename_lang=_stagename_lang::jsonb
        WHERE salesstageid=_salesstageid::uuid;

				_codeid:= _salesstageid::TEXT;
				_codeflag:= 1;
				_codemsg:= '�������۽׶γɹ�';
 
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



