CREATE OR REPLACE FUNCTION "public"."crm_func_entity_protocol_format_dictionary_lang"(IN "_dictypeid" int4, IN "_dataids" text, OUT "formatname" text)
  RETURNS "pg_catalog"."text" AS $BODY$

DECLARE
_formatname text;
_cnjson text;
_enjson text;
_twjson text;

rec record;

BEGIN

      SELECT '['||string_agg(dataval_lang::text,',')||']' INTO _formatname FROM crm_sys_dictionary 
      WHERE dictypeid = _dictypeid AND dataid IN (
							SELECT dataid::INT from (
										SELECT UNNEST( string_to_array(_dataids, ',')) as dataid 
							) as r WHERE dataid!=''
      );


	--raise exception '_formatname:%',_formatname;
	if(_formatname = '' or _formatname is null) then
		formatname:='{"cn":"","en":"","tw":""}';
	else
		select * into rec from (
			select string_agg(cnjson::text,',') as cnjson ,string_agg(enjson::text,',') as enjson,string_agg(twjson::text,',') as twjson
		from (
		select  btrim(cnjson::text,'"') cnjson , btrim(enjson::text,'"') enjson,btrim(twjson::text,'"') twjson from 
			(select json_array_elements(_formatname::json)->'CN' as cnjson,
				json_array_elements(_formatname::json)->'en' as enjson,
				json_array_elements(_formatname::json)->'tw' as twjson
			) as tab
		) as t 
) as t1;

		
		_cnjson := COALESCE(rec.cnjson,'');
		_enjson := COALESCE(rec.enjson,'');
		_twjson := COALESCE(rec.twjson,'');
		
	        --raise exception '_r:%',_enjson;
		formatname := '{"cn":"'|| _cnjson ||'", "en":"'|| _enjson || '","tw":"'||_twjson || '"}';
	end if;

 


END
$BODY$
  LANGUAGE 'plpgsql' VOLATILE COST 100
;