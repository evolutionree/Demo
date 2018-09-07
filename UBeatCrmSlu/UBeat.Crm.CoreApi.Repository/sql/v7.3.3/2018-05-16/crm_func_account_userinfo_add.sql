
/**
增加注册用户时，根据密码策略，设定账号表的下次是否需要更改密码
*/
CREATE OR REPLACE FUNCTION "public"."crm_func_account_userinfo_add"("_accountname" text, "_accountpwd" text, "_accesstype" text, "_username" text, "_usericon" text, "_userphone" text, "_userjob" text, "_deptid" uuid, "_namepinyin" text, "_email" text, "_joineddate" timestamp, "_birthday" timestamp, "_remark" text, "_sex" int4, "_tel" text, "_status" int4, "_workcode" text, "_nextchangedpwd" int4,"_userno" int4)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE
   _accountid int4;
   _userid int4;

  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

BEGIN
   BEGIN 
				 --数据逻辑
				 IF _accountname IS NULL OR _accountname = '' THEN
								Raise EXCEPTION '账号不能为空';
				 END IF;

				 IF _accountpwd IS NULL OR _accountpwd = '' THEN
								Raise EXCEPTION '密码不能为空';
				 END IF;

				 IF EXISTS(SELECT 1 FROM crm_sys_account WHERE accountname=_accountname AND recstatus=1 LIMIT 1) THEN
								Raise EXCEPTION '账号不能重复';
				 END IF;

         --默认照片
         IF _usericon IS NULL OR _usericon = '' OR _usericon = 'ICON' THEN
                 _usericon = 'a24201ce-04a9-11e7-a7a4-005056ae7f49';
         END IF;

				 INSERT INTO crm_sys_account (accountname, accountpwd,accesstype,lastchangedpwdtime,nextmustchangepwd,reccreator,recupdator) 
				 VALUES (_accountname,_accountpwd,_accesstype,now(),_nextchangedpwd,_userno,_userno)  RETURNING accountid INTO _accountid;

				 INSERT INTO crm_sys_userinfo (username, usericon,userphone,userjob,reccreator,recupdator,namepinyin,useremail,joineddate,birthday,remark,usersex,usertel,recstatus,workcode) 
				 VALUES (_username,_usericon,_userphone,_userjob,_userno,_userno,_namepinyin,_email,_joineddate::date,_birthday::date,_remark,_sex,_tel,_status,COALESCE(_workcode,''))  RETURNING userid INTO _userid;

				 INSERT INTO crm_sys_account_userinfo_relate (accountid,userid,deptid) 
				 VALUES (_accountid,_userid,_deptid);

				 INSERT INTO crm_sys_userinfo_role_relate (userid,roleid) 
				 VALUES (_userid,'1e568fe0-389a-4943-b1b6-d3b0099d9e98'::uuid);

				 INSERT INTO crm_sys_userinfo_vocation_relate (userid,vocationid) 
				 VALUES (_userid,'8e1771c8-d173-486a-bd64-b0b774e9de92'::uuid);
					_codeid:= _accountid::TEXT;
					_codeflag:= 1;
					_codemsg:= '新增账户成功';
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
