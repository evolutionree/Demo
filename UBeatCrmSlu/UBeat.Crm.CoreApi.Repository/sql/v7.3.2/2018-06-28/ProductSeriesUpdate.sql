CREATE OR REPLACE FUNCTION "public"."crm_func_productseries_update"("_productsetid" varchar, "_seriesname" varchar, "_seriescode" varchar, "_userno" int4, _serieslanguage jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
DECLARE

   _productsetidtemp uuid:=NULL;
   _pproductsetidtemp uuid; --�Ĳ�Ʒϵ�е��ϼ�id
   _ptmp uuid:=NULL;

  --��׼���ز���
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

BEGIN

  --select * from crm_func_productseries_update('cb221494-9269-45d5-ba74-725ae852cd39','for update',7);
  --select * from crm_sys_products_series;

	BEGIN

	      IF _productsetid IS NULL OR _productsetid='' THEN 
	         Raise EXCEPTION '��Ʒϵ��id����Ϊ��';
	      ELSE 
	         _productsetidtemp:=_productsetid::uuid;  
	         IF NOT EXISTS(SELECT 1 FROM crm_sys_products_series where recstatus=1 AND  productsetid=_productsetidtemp) THEN
		     Raise EXCEPTION '������Ʒϵ�в�����';
	 	 END IF;
	      END IF;
  
		--����Ʒϵ�����Ƿ�Ϊ��
		IF _seriesname IS  NULL OR _seriesname='' THEN
			Raise EXCEPTION '��Ʒϵ�����Ʋ���Ϊ��';
		END IF;

		--���ͬ���Ĳ�Ʒϵ�����Ƿ��Ѿ�����
		select pproductsetid into _pproductsetidtemp from crm_sys_products_series where productsetid = _productsetid::uuid;
		IF EXISTS (SELECT 1 FROM crm_sys_products_series WHERE recstatus=1 AND  productsetname=_seriesname AND productsetid!=_productsetidtemp and pproductsetid = _pproductsetidtemp) THEN
			Raise EXCEPTION '��Ʒϵ�������Ѵ���';
		END IF;
		
		--����Ʒϵ�б����Ƿ�Ϊ��
		IF _seriescode IS  NULL OR _seriescode='' THEN
			Raise EXCEPTION '��Ʒϵ�б��벻��Ϊ��';
		END IF;
		--���ͬ����Ʒϵ�б����Ƿ��Ѿ�����
		IF EXISTS (SELECT 1 FROM crm_sys_products_series WHERE recstatus=1 AND  productsetcode=_seriescode AND productsetid!=_productsetidtemp) THEN
			Raise EXCEPTION '��Ʒϵ�б����Ѵ���';
		END IF;


	       --�޸ò�Ʒϵ������
	       update crm_sys_products_series
	       set productsetname=_seriesname,
		   productsetcode=_seriescode,
	           recupdator=_userno,
						serieslanguage=_serieslanguage,
	           recupdated=now()
	       where productsetid=_productsetidtemp;
	       

		--��������
		_codeid:= _productsetid::TEXT;
		_codeflag:= 1;
		_codemsg:= '�޸Ĳ�Ʒϵ�гɹ�';
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

