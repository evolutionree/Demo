-- ��crm_sys_account ������nextmustchangepwd���ֶ� ����

ALTER TABLE crm_sys_account ADD COLUMN nextmustchangepwd  int default 0



-- ������Ա���crm_sys_security_pwdpolicy��


CREATE TABLE crm_sys_security_pwdpolicy(
"RecId" uuid PRIMARY KEY not NULL,
"Policy" VARCHAR(1000),
"RecUpdated" date,
"RecUpdator" INT
)


-- ������ʷ��


CREATE TABLE crm_sys_security_historyPwd(
"RecId" uuid PRIMARY KEY not null,
"UserId" int not null,
"oldPwd" VARCHAR(50) ,
"newpwd" VARCHAR(50),
"changeype" int,
"reccreator" INT,
"reccreated" date
)

