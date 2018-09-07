--修改后台配置的func定义

--配置管理1f9a7c10-0a22-4ef0-825e-c98d4503c601
----基础管理 --ad8993e3-0655-4397-a252-004473a881f0
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('ad8993e3-0655-4397-a252-004473a881f0', '基础管理', 'SettingManager', '1f9a7c10-0a22-4ef0-825e-c98d4503c601', NULL, '0', '1', '1', now(), now(), '1', '1', NULL, NULL, NULL, NULL, NULL);
------团队管理
update crm_sys_function set parentid = 'ad8993e3-0655-4397-a252-004473a881f0' ,recorder = 1  where  funcid ='583d3c3f-276f-43f1-aa1b-ed6b2448a4c3';

--------数据权限设置
update crm_sys_function set parentid = 'ad8993e3-0655-4397-a252-004473a881f0' ,recorder = 2  where  funcid ='6a552ac8-8ecc-46e2-b549-23d98378e1f3';


--------产品管理
update crm_sys_function set parentid = 'ad8993e3-0655-4397-a252-004473a881f0' ,recorder = 3  where  funcid ='5c58de94-3ad8-4e72-8b0d-0d3568ce0a92';


----平台配置 --a7e2b11b-278c-406a-bee3-cdb66eaf6105
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('a7e2b11b-278c-406a-bee3-cdb66eaf6105', '平台配置', 'SettingManager', '1f9a7c10-0a22-4ef0-825e-c98d4503c601', NULL, '0', '2', '1', now(), now(), '1', '1', NULL, NULL, NULL, NULL, NULL);
--------实体配置
update crm_sys_function set parentid = 'a7e2b11b-278c-406a-bee3-cdb66eaf6105' ,recorder = 1  where  funcid ='a9ab89a7-f80d-45db-8e26-311556b10b61';
--------审批设置
update crm_sys_function set parentid = 'a7e2b11b-278c-406a-bee3-cdb66eaf6105' ,recorder = 2  where  funcid ='9bcc3da2-767a-4971-abdc-99e9d30ee017';
--------数据源配置
update crm_sys_function set parentid = 'a7e2b11b-278c-406a-bee3-cdb66eaf6105' ,recorder = 3  where  funcid ='ed72c6e6-81e6-4549-86ba-88c4461545cc';
--------字典配置
update crm_sys_function set parentid = 'a7e2b11b-278c-406a-bee3-cdb66eaf6105' ,recorder = 4  where  funcid ='2368ea20-f748-43ad-9a41-c3e5cbef51c7';
--套打配置
----新增
----修改
----删除
----停用/启用
----适用范围
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('c1b76232-f2c5-42b8-bc70-a761d354a413', '套打配置', 'SettingManager', 'a7e2b11b-278c-406a-bee3-cdb66eaf6105', NULL, '0', '5', '1', now(), now(), '1', '1', NULL, NULL, NULL, NULL, NULL);
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('bd96e44b-4e91-44ad-85f2-cd270588ef1a', '新增模板', 'SettingManager', 'c1b76232-f2c5-42b8-bc70-a761d354a413', NULL, '0', '1', '1', now(), now(), '1', '1', NULL, NULL, 'api/PrintForm/inserttemplate', NULL, NULL);
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('90247c16-a03f-4541-ba91-249b5b32edf7', '修改模板', 'SettingManager', 'c1b76232-f2c5-42b8-bc70-a761d354a413', NULL, '0', '2', '1', now(), now(), '1', '1', NULL, NULL, 'api/PrintForm/updatetemplate', NULL, NULL);
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('769c17ce-9d2f-4540-b5eb-c57d3d0d7b80', '删除模板', 'SettingManager', 'c1b76232-f2c5-42b8-bc70-a761d354a413', NULL, '0', '3', '1', now(), now(), '1', '1', NULL, NULL, 'api/PrintForm/deletetemplate', NULL, NULL);
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('71906be1-5170-4c71-ae3b-62c0ebaa1e40', '停用启用', 'SettingManager', 'c1b76232-f2c5-42b8-bc70-a761d354a413', NULL, '0', '4', '1', now(), now(), '1', '1', NULL, NULL, 'api/PrintForm/settemplatestatus', NULL, NULL);
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('195270a1-0083-47fd-a086-8946e9e28e9a', '适用范围', 'SettingManager', 'c1b76232-f2c5-42b8-bc70-a761d354a413', NULL, '0', '5', '1', now(), now(), '1', '1', NULL, NULL, 'api/PrintForm/userange', NULL, NULL);

---扫描设置
-----新增
-----编辑
-----删除
-----禁用启用
-----排序
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('99a831ea-d336-4be2-aeeb-83060269c5ca', '扫描设置', 'SettingManager', 'a7e2b11b-278c-406a-bee3-cdb66eaf6105', NULL, '0', '6', '1', now(), now(), '1', '1', NULL, NULL, NULL, NULL, NULL);
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('7da4cd10-cb67-4a08-91d0-4313611f7375', '新增', 'SettingManager', '99a831ea-d336-4be2-aeeb-83060269c5ca', NULL, '0', '1', '1', now(), now(), '1', '1', NULL, NULL, 'api/qrcode/add', NULL, NULL);
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('2037c21e-fc10-49a5-8602-071191e3fa8e', '修改', 'SettingManager', '99a831ea-d336-4be2-aeeb-83060269c5ca', NULL, '0', '2', '1', now(), now(), '1', '1', NULL, NULL, 'api/qrcode/edit', NULL, NULL);
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('e6a3e90d-5480-4c43-9085-9171844f5abf', '删除', 'SettingManager', '99a831ea-d336-4be2-aeeb-83060269c5ca', NULL, '0', '3', '1', now(), now(), '1', '1', NULL, NULL, 'api/qrcode/delete', NULL, NULL);
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('efbb30c8-1761-4683-ab02-d29733c17d88', '停用启用', 'SettingManager', '99a831ea-d336-4be2-aeeb-83060269c5ca', NULL, '0', '4', '1', now(), now(), '1', '1', NULL, NULL, 'api/qrcode/setstatus', NULL, NULL);
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('bd10e485-d31e-4609-b27c-3cb3fe3e2c17', '排序', 'SettingManager', '99a831ea-d336-4be2-aeeb-83060269c5ca', NULL, '0', '5', '1', now(), now(), '1', '1', NULL, NULL, 'api/qrcode/orderrule', NULL, NULL);

---后台事务
-----新增
-----编辑
-----删除
-----禁用启用
-----查看实例
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('040370fe-c298-4d24-8135-28e93de44779', '调度事务', 'SettingManager', 'a7e2b11b-278c-406a-bee3-cdb66eaf6105', NULL, '0', '7', '1', now(), now(), '1', '1', NULL, NULL, NULL, NULL, NULL);
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('d9814917-4ed1-4394-9e15-2562dde02db7', '新增', 'SettingManager', '040370fe-c298-4d24-8135-28e93de44779', NULL, '0', '1', '1', now(), now(), '1', '1', NULL, NULL, 'api/qrtz/add', NULL, NULL);
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('ad5c2126-1aef-4e0a-a79b-a525bde89c00', '修改', 'SettingManager', '040370fe-c298-4d24-8135-28e93de44779', NULL, '0', '2', '1', now(), now(), '1', '1', NULL, NULL, 'api/qrtz/edit', NULL, NULL);
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('24d686a1-51de-4439-a5d5-900f88a90828', '删除', 'SettingManager', '040370fe-c298-4d24-8135-28e93de44779', NULL, '0', '3', '1', now(), now(), '1', '1', NULL, NULL, 'api/qrtz/delete', NULL, NULL);
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('8056be4f-00ba-4f33-b260-ad71b7159460', '停用启用', 'SettingManager', '040370fe-c298-4d24-8135-28e93de44779', NULL, '0', '4', '1', now(), now(), '1', '1', NULL, NULL, 'api/qrtz/setstatus', NULL, NULL);
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('25f64534-ad65-450d-b165-050ca56bf903', '查看实例', 'SettingManager', '040370fe-c298-4d24-8135-28e93de44779', NULL, '0', '5', '1', now(), now(), '1', '1', NULL, NULL, 'api/qrtz/viewinstance', NULL, NULL);



--业务参数设置
update crm_sys_function set parentid = '1f9a7c10-0a22-4ef0-825e-c98d4503c601' ,recorder = 3 where  funcid ='7e765740-3eaa-44ea-96d2-5880d20a9d6b';

update crm_sys_function set parentid = '7e765740-3eaa-44ea-96d2-5880d20a9d6b' ,recorder = 1 where  funcid ='4a38b378-11f3-4617-ab35-b8bd015c62ce';
--销售阶段设置
update crm_sys_function set parentid = '7e765740-3eaa-44ea-96d2-5880d20a9d6b' ,recorder = 2 where  funcid ='f178efc4-d41d-4060-8381-198da1556122';


--智能提醒
update crm_sys_function set parentid = '7e765740-3eaa-44ea-96d2-5880d20a9d6b' ,recorder = 3 where  funcid ='ee6a6de9-34de-40eb-a6c0-c9339e340c2e';

--回收规则

update crm_sys_function set parentid = '7e765740-3eaa-44ea-96d2-5880d20a9d6b' ,recorder = 4 where  funcid ='e5f2bd91-008e-45a4-80d4-c9384c8816aa';
--系统管理
update crm_sys_function set parentid = '1f9a7c10-0a22-4ef0-825e-c98d4503c601' ,recorder = 4 where  funcid ='d1cd1b87-047e-4888-89b5-5e890ba72758';
--授权信息
update crm_sys_function set parentid = 'd1cd1b87-047e-4888-89b5-5e890ba72758' ,recorder = 1 where  funcid ='4f9c8e65-596e-47a7-8b35-f0cca4a7061f';
--操作日志
update crm_sys_function set parentid = 'd1cd1b87-047e-4888-89b5-5e890ba72758' ,recorder = 2 where  funcid ='f5169ec8-5181-4b55-b57d-5686694c6e36';
--账号安全
-----查看
-----编辑
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('bb950493-2189-4647-aed4-baaaf031c0ee', '账号安全', 'SettingManager', 'd1cd1b87-047e-4888-89b5-5e890ba72758', NULL, '0', '3', '1', now(), now(), '1', '1', NULL, NULL, NULL, NULL, NULL);
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('959a7a0e-b75e-4a37-954d-07816463ac28', '查看', 'SettingManager', 'bb950493-2189-4647-aed4-baaaf031c0ee', NULL, '0', '1', '1', now(), now(), '1', '1', NULL, NULL, 'api/qrcode/add', NULL, NULL);
INSERT INTO "public"."crm_sys_function" ("funcid", "funcname", "funccode", "parentid", "entityid", "devicetype", "recorder", "recstatus", "reccreated", "recupdated", "reccreator", "recupdator", "rectype", "relationvalue", "routepath", "islastchild", "childtype") 
VALUES ('dca40516-96a5-4a5f-a2d8-dfb715eb4628', '编辑', 'SettingManager', 'bb950493-2189-4647-aed4-baaaf031c0ee', NULL, '0', '2', '1', now(), now(), '1', '1', NULL, NULL, 'api/qrcode/edit', NULL, NULL);
delete from crm_sys_function_treepaths where ancestor = 'ec1732e2-0189-40e3-bd9d-3181391be7df' or descendant = 'ec1732e2-0189-40e3-bd9d-3181391be7df';
delete from crm_sys_function where funcid ='ec1732e2-0189-40e3-bd9d-3181391be7df';
select crm_func_repairallfunctiontreepath();



---更新权限与web菜单的关联关系

update crm_sys_webmenu set funcid ='c1b76232-f2c5-42b8-bc70-a761d354a413' where id = '56eef638-4ab6-11e8-8421-000c29832f0a';
update crm_sys_webmenu set funcid ='c1b76232-f2c5-42b8-bc70-a761d354a413' where id = '3157aefb-392a-4121-8fd8-3198e026e75c';
update crm_sys_webmenu set funcid ='959a7a0e-b75e-4a37-954d-07816463ac28' where id = 'be05f8c1-a341-4d59-a5c9-78a86045ea00';


--select * from crm_sys_webmenu where path <> '' and funcid = ''


