INSERT INTO "public"."crm_sys_webmenu" ("id", "index", "name", "icon", "path", "funcid", "parentid", "isdynamic", "islogicmenu") VALUES ('ac70439d-9a83-4c78-b542-fb0cf4e19492', '1', '基础管理', 'setting', '', NULL, '00000000-0000-0000-0000-000000000000', '0', '1');
INSERT INTO "public"."crm_sys_webmenu" ("id", "index", "name", "icon", "path", "funcid", "parentid", "isdynamic", "islogicmenu") VALUES ('313a17f3-f327-4be7-aaa5-49ba6b2894de', '2', '平台配置', 'setting', '', NULL, '00000000-0000-0000-0000-000000000000', '0', '1');
INSERT INTO "public"."crm_sys_webmenu" ("id", "index", "name", "icon", "path", "funcid", "parentid", "isdynamic", "islogicmenu") VALUES ('24cad7e0-713f-4244-a915-16ea367f00ca', '3', '业务配置', 'setting', '', NULL, '00000000-0000-0000-0000-000000000000', '0', '1');

--团队组织
update crm_sys_webmenu set parentid = 'ac70439d-9a83-4c78-b542-fb0cf4e19492' ,index= 1  where id = '4c3b6a13-0a01-4eb7-b497-8b585841739f';
--数据权限设置
update crm_sys_webmenu set parentid = 'ac70439d-9a83-4c78-b542-fb0cf4e19492' ,index= 2  where id = '9cdbacdd-c533-405b-96a5-7df720cb08a4';
--产品管理
update crm_sys_webmenu set parentid = 'ac70439d-9a83-4c78-b542-fb0cf4e19492' ,index= 3 where id = '17e4f4d3-6267-4c84-b410-da847e78e179';

--实体配置
update crm_sys_webmenu set parentid = '313a17f3-f327-4be7-aaa5-49ba6b2894de' ,index= 1 where id = 'f26dc83c-6258-4ca3-bb3b-3f9c256171d4';

--审批设置
update crm_sys_webmenu set parentid = '313a17f3-f327-4be7-aaa5-49ba6b2894de' ,index= 2 where id = '7b459bda-1ebe-4ddf-a5b6-6d60c36ba915';
--数据源配置
update crm_sys_webmenu set parentid = '313a17f3-f327-4be7-aaa5-49ba6b2894de' ,index= 3 where id = '01a4e6e4-3227-4263-8f21-a9f88fea532a';
--字典配置
update crm_sys_webmenu set parentid = '313a17f3-f327-4be7-aaa5-49ba6b2894de' ,index= 4 where id = '71cf6024-70c2-4007-991f-d5c5aa2fbf24';

--套打设置
update crm_sys_webmenu set parentid = '313a17f3-f327-4be7-aaa5-49ba6b2894de' ,index= 5 where id = '56eef638-4ab6-11e8-8421-000c29832f0a';
--二维码入口
INSERT INTO "public"."crm_sys_webmenu" ("id", "index", "name", "icon", "path", "funcid", "parentid", "isdynamic", "islogicmenu", "isleaf")
VALUES ('3157aefb-392a-4121-8fd8-3198e026e75c', '6', '扫描设置', 'setting', '/qrcodeentrance', '', '313a17f3-f327-4be7-aaa5-49ba6b2894de', '0', '1', '1');

--调度事务管理
update crm_sys_webmenu set parentid = '313a17f3-f327-4be7-aaa5-49ba6b2894de' ,index= 6 where id = '163a0adf-c070-4442-9847-5fdd27111db3';



--指标设置
update crm_sys_webmenu set parentid = '24cad7e0-713f-4244-a915-16ea367f00ca' ,index= 1 where id = '03103f62-1adb-420f-9657-16c0aaa9a70a';
--销售阶段
update crm_sys_webmenu set parentid = '24cad7e0-713f-4244-a915-16ea367f00ca' ,index= 2 where id = '3e2c585e-e601-45de-95fc-d699bfd7674b';
--智能提醒
update crm_sys_webmenu set parentid = '24cad7e0-713f-4244-a915-16ea367f00ca' ,index= 3  where id = '3900c69c-6030-458b-9b74-c386e2965de9';
--智能提醒
update crm_sys_webmenu set parentid = '24cad7e0-713f-4244-a915-16ea367f00ca' ,index= 3  where id = '3900c69c-6030-458b-9b74-c386e2965de9';
--回收规则
update crm_sys_webmenu set parentid = '24cad7e0-713f-4244-a915-16ea367f00ca' ,index= 4 where id = '1c48b9ef-ecfa-42e4-bcde-55acbbab7229';




--操作日志
update crm_sys_webmenu set parentid = '2fa0a308-0b61-4e37-a188-7a46c7877b44' ,index= 2 where id = '236c2169-e4e8-4ebd-97fd-cc41ca932c8a';
--二维码入口
delete from crm_sys_webmenu where id= 'be05f8c1-a341-4d59-a5c9-78a86045ea00';
INSERT INTO "public"."crm_sys_webmenu" ("id", "index", "name", "icon", "path", "funcid", "parentid", "isdynamic", "islogicmenu", "isleaf")
VALUES ('be05f8c1-a341-4d59-a5c9-78a86045ea00', '3', '账号安全', 'setting', '/passwordstrategy', '', '2fa0a308-0b61-4e37-a188-7a46c7877b44', '0', '1', '1');



update crm_sys_webmenu set name ='报表定义' where id = 'cc710262-2270-4341-82c6-7411377fb5e1';
