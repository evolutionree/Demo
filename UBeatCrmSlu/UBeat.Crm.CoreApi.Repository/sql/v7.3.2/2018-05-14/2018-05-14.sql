-- ��crm_sys_account ������nextmustchangepwd���ֶ� ����

ALTER TABLE crm_sys_account ADD COLUMN nextmustchangepwd  int default 0;
alter table crm_sys_account add COLUMN lastchangedpwdtime date default now();//����޸�����ʱ��



-- ������Ա���crm_sys_security_pwdpolicy��


CREATE TABLE crm_sys_security_pwdpolicy(
"RecId" uuid PRIMARY KEY not NULL,
"Policy" VARCHAR(1000),
"RecUpdated" date,
"RecUpdator" INT
);


-- ������ʷ��


CREATE TABLE crm_sys_security_historyPwd(
"recid" uuid PRIMARY KEY not null,
"userid" int not null,
"oldpwd" VARCHAR(50) ,
"newpwd" VARCHAR(50),
"changetype" int,
"reccreator" INT,
"reccreated" date
);

