-- 表crm_sys_account 新增《nextmustchangepwd》字段 （）

ALTER TABLE crm_sys_account ADD COLUMN nextmustchangepwd  int default 0



-- 密码策略表：（crm_sys_security_pwdpolicy）


CREATE TABLE crm_sys_security_pwdpolicy(
"RecId" uuid PRIMARY KEY not NULL,
"Policy" VARCHAR(1000),
"RecUpdated" date,
"RecUpdator" INT
)


-- 密码历史表


CREATE TABLE crm_sys_security_historyPwd(
"RecId" uuid PRIMARY KEY not null,
"UserId" int not null,
"oldPwd" VARCHAR(50) ,
"newpwd" VARCHAR(50),
"changeype" int,
"reccreator" INT,
"reccreated" date
)

