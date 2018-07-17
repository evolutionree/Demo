CREATE OR REPLACE FUNCTION "public"."crm_func_sales_target_norm_type_select"()
  RETURNS SETOF "pg_catalog"."refcursor" AS $BODY$
DECLARE

  _sql TEXT:='';
  _datacursor refcursor:= 'datacursor';

BEGIN

	/*
           select  from crm_func_sales_target_norm_type_select();
           fetch all from datacursor;

           select calcutetype, * from crm_sys_sales_target_norm_type
	*/

	OPEN _datacursor FOR
	SELECT normtypeid,normtypename,recorder,isdefault,calcutetype,reclanguage FROM  crm_sys_sales_target_norm_type 
	WHERE recstatus=1
	ORDER BY isdefault desc, recorder asc;
		  
	RETURN NEXT _datacursor;

END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
 ROWS 1000
;

