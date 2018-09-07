CREATE OR REPLACE FUNCTION "public"."crm_func_sales_target_norm_type_save"("_normtypeid" uuid, "_normtypename" text, "_entityid" uuid, "_fieldname" text, "_calcutetype" int4, "_userno" int4,_reclanguage jsonb)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$

/*

	--编辑销售目标

	SELECT crm_func_sales_target_norm_type_save(
	'00000000-0000-0000-0000-000000000000',
	'helo',
	'c658968b-f959-469f-b602-4925cd291292',
	'aa',
	0,
	7);


	select * from crm_sys_entity
	select * from crm_sys_sales_target_norm_type

*/

DECLARE
  _recorder integer:=0;
  _order integer:=0;
  
  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

BEGIN
	 BEGIN

	    IF _normtypename IS NOT  NULL AND  _normtypename!='' THEN

	      IF _normtypeid IS NULL OR _normtypeid='00000000-0000-0000-0000-000000000000'::uuid THEN  --新增一条数据  

	           IF NOT EXISTS(SELECT normtypename FROM crm_sys_sales_target_norm_type WHERE normtypename=_normtypename AND recstatus=1) THEN
	              
			--获取排序
			SELECT coalesce(max(recorder),0)+1 into _order  
			FROM crm_sys_sales_target_norm_type;
		 
			--插入数据
			INSERT INTO crm_sys_sales_target_norm_type (normtypename,entityid,fieldname,calcutetype,recorder,reccreator,recupdator,reclanguage)
			VALUES(_normtypename,_entityid,_fieldname,_calcutetype,_order,_userno,_userno,_reclanguage)
			RETURNING normtypeid INTO _normtypeid;

			_codeflag:= 1;

	         ELSE
		    _codeflag:= 0;
	            
	         END IF;

	      ELSE --编辑数据

	        IF NOT EXISTS(SELECT normtypename FROM crm_sys_sales_target_norm_type WHERE normtypename=_normtypename AND recstatus=1 AND normtypeid<>_normtypeid) THEN
	        
			UPDATE crm_sys_sales_target_norm_type 
			SET normtypename=_normtypename,
			    reccreator=_userno,
			    recupdator=_userno,
			    recupdated=now(),
reclanguage=_reclanguage
			WHERE normtypeid=_normtypeid;

			_codeflag:= 1;

	        ELSE 
		    _codeflag:= 0;
	        END IF;
	        
              END IF;

                    --设置返回提示
		     IF _codeflag=1 THEN
			    _codemsg:= '保存销售目标成功';
		     ELSE 
			_codemsg:= '销售指标名称已存在';
		     END IF;


            ELSE
              
                  _codeflag:= 0;
                  _codemsg:= '销售目标名称不能为空';
  
             END IF;

           
		   
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

