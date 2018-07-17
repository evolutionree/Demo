CREATE OR REPLACE FUNCTION "public"."crm_func_productseries_insert"("_topseriesid" uuid, "_seriesname" text, "_seriescode" text, "_userno" int4,_serieslanguage jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
DECLARE

/*
	select crm_func_productseries_insert('2a762299-e455-4537-9c90-070c81f58a54','hgcTest','001',11);
	select *from crm_sys_products_series order by reccreated desc limit 10;
*/

  _productsetid uuid;
   
  --��׼���ز���
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;
BEGIN
	BEGIN

		IF _seriesname IS  NULL OR _seriesname='' THEN
				 Raise EXCEPTION '��Ʒϵ�����Ʋ���Ϊ��';
		END IF;

		--���ͬ���Ĳ�Ʒϵ�����Ƿ��Ѿ�����
		IF EXISTS (SELECT 1 FROM crm_sys_products_series WHERE recstatus=1 AND  productsetname = _seriesname and pproductsetid = _topseriesid LIMIT 1) THEN
				Raise EXCEPTION '��Ʒϵ�������Ѵ���';
		END IF;

		IF _seriescode IS  NULL OR _seriescode='' THEN
				 Raise EXCEPTION '��Ʒϵ�б��벻��Ϊ��';
		END IF;

		--���ͬ���Ĳ�Ʒϵ�б����Ƿ��Ѿ�����
		IF EXISTS (SELECT 1 FROM crm_sys_products_series WHERE recstatus=1 AND  productsetcode = _seriescode LIMIT 1) THEN
				Raise EXCEPTION '��Ʒϵ�б����Ѵ���';
		END IF;

	   --�����߼�
	   INSERT INTO crm_sys_products_series (productsetname, pproductsetid, productsetcode, reccreator, recupdator,serieslanguage) 
	   VALUES (_seriesname,_topseriesid,_seriescode,_userno,_userno,_serieslanguage) RETURNING productsetid INTO _productsetid;

		INSERT INTO crm_sys_products_series_treepaths(ancestor,descendant,nodepath)
		SELECT t.ancestor,_productsetid,t.nodepath+1
		FROM crm_sys_products_series_treepaths AS t
		WHERE t.descendant = _topseriesid
		UNION ALL
		SELECT _productsetid,_productsetid,0;

		_codeid:= _productsetid::TEXT;
		_codeflag:= 1;
		_codemsg:= '������Ʒϵ�гɹ�';
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
