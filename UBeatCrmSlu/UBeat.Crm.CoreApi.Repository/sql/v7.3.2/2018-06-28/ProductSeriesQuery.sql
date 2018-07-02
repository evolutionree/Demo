CREATE OR REPLACE FUNCTION "public"."crm_func_product_series_select_improve_bak"(IN "_productsetid" varchar, IN "_direction" varchar, IN "_isgetdisable" int4, IN "_userno" int4)
  RETURNS TABLE("productsetid" uuid, "pproductsetid" uuid, "productsetname" text, "productsetcode" text, "recorder" int4, "nodepath" int4, "nodes" int4, "reccreator" int4, "reccreatorname" text, "reccreated" timestamp, "recstatus" int4)  AS $BODY$
DECLARE

  _productsetidtemp uuid:=NULL;
  _sql TEXT:='';
	_recstatus_sql TEXT:='c.recstatus = 1';

  --select * from crm_func_product_series_select('00000000-0000-0000-0000-000000000000','UPPER',11);

BEGIN
      IF _productsetid IS NOT NULL AND _productsetid!='' THEN
          _productsetidtemp:=_productsetid::uuid;
     END IF;

		IF(_isgetdisable=1) THEN
					_recstatus_sql:='1 = 1';
		END IF;

   
	--如果没有文件目录id，则获取所有根目录
	IF _productsetidtemp IS NULL THEN
		 _sql = 'SELECT c.productsetid,c.pproductsetid,c.productsetname,c.productsetcode,c.recorder,t.nodepath,
		                (SELECT COUNT(1)-1 FROM crm_sys_products_series_treepaths AS s WHERE  s.ancestor = t.descendant)::INT AS nodes ,
			        c.reccreator,u.username AS reccreatorname,c.reccreated ,c.recstatus 
			 FROM crm_sys_products_series AS c
			 INNER JOIN crm_sys_products_series_treepaths t on c.productsetid = t.descendant  
			 LEFT JOIN crm_sys_userinfo AS u ON u.userid=c.reccreator 
			 WHERE '||_recstatus_sql||'
			 AND t.ancestor IN (       
						 SELECT productsetid 
						 FROM crm_sys_products_series
						 WHERE recstatus = 1  
						 AND  pproductsetid=''00000000-0000-0000-0000-000000000000''
					    )';

					    
    
	ELSE
		 CASE _direction
			WHEN 'UPPER' THEN
				_sql = 'SELECT c.productsetid,c.pproductsetid,c.productsetname,c.productsetcode,c.recorder,t.nodepath,
					(SELECT COUNT(1)-1 FROM crm_sys_products_series_treepaths AS s WHERE  s.ancestor = t.descendant)::INT AS nodes ,
					c.reccreator,u.username AS reccreatorname,c.reccreated ,c.recstatus 
					FROM crm_sys_products_series AS c
					INNER JOIN crm_sys_products_series_treepaths t on c.productsetid = t.ancestor  
					LEFT JOIN crm_sys_userinfo AS u ON u.userid=c.reccreator 
					WHERE '||_recstatus_sql||' AND  t.descendant = $1  ';
			WHEN 'DOWNER' THEN
				    _sql = 'SELECT c.productsetid,c.pproductsetid,c.productsetname,c.productsetcode,c.recorder,t.nodepath,
					       (SELECT COUNT(1)-1 FROM crm_sys_products_series_treepaths AS s WHERE  s.ancestor = t.descendant)::INT AS nodes ,
						c.reccreator,u.username AS reccreatorname,c.reccreated ,c.recstatus 
					FROM crm_sys_products_series AS c
					INNER JOIN crm_sys_products_series_treepaths t on c.productsetid = t.descendant 
					LEFT JOIN crm_sys_userinfo AS u ON u.userid=c.reccreator 
					WHERE '||_recstatus_sql||'  AND t.ancestor = $1';

			ELSE 
			     SELECT func_raise('EXCEPTION', '未匹配的Direction'); 
		END CASE;
	END IF;

	Raise Notice 'the sql:%',_sql;
	 --标准返回
	 RETURN QUERY EXECUTE format(_sql)
	 USING  _productsetidtemp;
	 
END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;




drop FUNCTION "crm_func_product_series_select_improve"(IN "_productsetid" varchar, IN "_direction" varchar, IN "_isgetdisable" int4, IN "_userno" int4);





CREATE OR REPLACE FUNCTION "public"."crm_func_product_series_select_improve"(IN "_productsetid" varchar, IN "_direction" varchar, IN "_isgetdisable" int4, IN "_userno" int4)
  RETURNS TABLE("productsetid" uuid, "pproductsetid" uuid, "productsetname" text, "productsetcode" text, "recorder" int4, "nodepath" int4, "nodes" int4, "reccreator" int4, "reccreatorname" text, "reccreated" timestamp, "recstatus" int4, "serieslanguage" jsonb)  AS $BODY$
DECLARE

  _productsetidtemp uuid:=NULL;
  _sql TEXT:='';
	_recstatus_sql TEXT:='c.recstatus = 1';

  --select * from crm_func_product_series_select('00000000-0000-0000-0000-000000000000','UPPER',11);

BEGIN
      IF _productsetid IS NOT NULL AND _productsetid!='' THEN
          _productsetidtemp:=_productsetid::uuid;
     END IF;

		IF(_isgetdisable=1) THEN
					_recstatus_sql:='1 = 1';
		END IF;

   
	--如果没有文件目录id，则获取所有根目录
	IF _productsetidtemp IS NULL THEN
		 _sql = 'SELECT c.productsetid,c.pproductsetid,c.productsetname,c.productsetcode,c.recorder,t.nodepath,
		                (SELECT COUNT(1)-1 FROM crm_sys_products_series_treepaths AS s WHERE  s.ancestor = t.descendant)::INT AS nodes ,
			        c.reccreator,u.username AS reccreatorname,c.reccreated ,c.recstatus,c.serieslanguage 
			 FROM crm_sys_products_series AS c
			 INNER JOIN crm_sys_products_series_treepaths t on c.productsetid = t.descendant  
			 LEFT JOIN crm_sys_userinfo AS u ON u.userid=c.reccreator 
			 WHERE '||_recstatus_sql||'
			 AND t.ancestor IN (       
						 SELECT productsetid 
						 FROM crm_sys_products_series
						 WHERE recstatus = 1  
						 AND  pproductsetid=''00000000-0000-0000-0000-000000000000''
					    )';

					    
    
	ELSE
		 CASE _direction
			WHEN 'UPPER' THEN
				_sql = 'SELECT c.productsetid,c.pproductsetid,c.productsetname,c.productsetcode,c.recorder,t.nodepath,c.serieslanguage,
					(SELECT COUNT(1)-1 FROM crm_sys_products_series_treepaths AS s WHERE  s.ancestor = t.descendant)::INT AS nodes ,
					c.reccreator,u.username AS reccreatorname,c.reccreated ,c.recstatus 
					FROM crm_sys_products_series AS c
					INNER JOIN crm_sys_products_series_treepaths t on c.productsetid = t.ancestor  
					LEFT JOIN crm_sys_userinfo AS u ON u.userid=c.reccreator 
					WHERE '||_recstatus_sql||' AND  t.descendant = $1  ';
			WHEN 'DOWNER' THEN
				    _sql = 'SELECT c.productsetid,c.pproductsetid,c.productsetname,c.productsetcode,c.recorder,t.nodepath,c.serieslanguage,
					       (SELECT COUNT(1)-1 FROM crm_sys_products_series_treepaths AS s WHERE  s.ancestor = t.descendant)::INT AS nodes ,
						c.reccreator,u.username AS reccreatorname,c.reccreated ,c.recstatus 
					FROM crm_sys_products_series AS c
					INNER JOIN crm_sys_products_series_treepaths t on c.productsetid = t.descendant 
					LEFT JOIN crm_sys_userinfo AS u ON u.userid=c.reccreator 
					WHERE '||_recstatus_sql||'  AND t.ancestor = $1';

			ELSE 
			     SELECT func_raise('EXCEPTION', '未匹配的Direction'); 
		END CASE;
	END IF;

	Raise Notice 'the sql:%',_sql;
	 --标准返回
	 RETURN QUERY EXECUTE format(_sql)
	 USING  _productsetidtemp;
	 
END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;
