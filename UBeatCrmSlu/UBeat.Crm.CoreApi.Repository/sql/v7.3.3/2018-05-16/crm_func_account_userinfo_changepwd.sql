/*
	修改密码策略，在修改密码时增加以下逻辑：
	1、往历史表插入记录
	2、把账号的下次修改密码状态变了、更新最新密码更新时间。
*/
CREATE OR REPLACE FUNCTION "public"."crm_func_account_userinfo_changepwd"("_accountid" int4, "_userid" int4, "_accountpwd" text, "_orginpwd" text, "_userno" int4)
  RETURNS SETOF "public"."crm_general_handle_result" AS $BODY$
-- select * from 
-- crm_func_role_add('afdassdas',1,1,'asdasd',10000000)
DECLARE

  --标准返回参数
  _codeid TEXT;
  _codeflag INT:=0;
  _codemsg TEXT;
  _codestack TEXT;
  _codestatus TEXT;

BEGIN

   BEGIN
				 --数据逻辑
         --因为多个帐号对应一个用户信息，所以这里用userid去处理，忽略accountid
         IF NOT EXISTS(SELECT 1 FROM crm_sys_account_userinfo_relate WHERE userid = _userid LIMIT 1) THEN
                Raise EXCEPTION '用户ID不存在';
         END IF;

				 IF _accountpwd IS NULL OR _accountpwd='' THEN
                Raise EXCEPTION '帐号新密码不能为空';
          END IF;

					IF _orginpwd IS NULL OR _orginpwd='' THEN
                Raise EXCEPTION '帐号旧密码不能为空';
          END IF;

         IF NOT EXISTS(SELECT 1 FROM crm_sys_account_userinfo_relate AS r LEFT JOIN crm_sys_account AS a ON r.accountid = a.accountid WHERE r.userid = _userid AND accountpwd =  _orginpwd LIMIT 1) THEN
                Raise EXCEPTION '原密码错误';
         END IF;

         UPDATE crm_sys_account SET passstatus=1, accountpwd = _accountpwd,recupdator=_userno,lastchangedpwdtime=now(),nextmustchangepwd=0
         WHERE accountid IN (SELECT accountid FROM crm_sys_account_userinfo_relate AS r WHERE r.userid = _userid);
					insert into crm_sys_security_historypwd(recid,userid,oldpwd,newpwd,reccreator,reccreated)
					select uuid_generate_v4() ,_userid,_orginpwd,_accountpwd,_userno,now();
					_codeid:= _accountid::TEXT;
					_codeflag:= 1;
					_codemsg:= '更新账户成功';
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
