alter table crm_sys_analyse_func add column anafuncname_lang jsonb;



UPDATE crm_sys_analyse_func set anafuncname_lang = '{"cn":"用户总数3","en":"Total Number Of Users3","tw":"用戶總數3"}'::jsonb where anafuncname='用户总数3';
UPDATE crm_sys_analyse_func set anafuncname_lang = '{"cn":"用户总数4","en":"Total Number Of Users4","tw":"用戶總數3"}'::jsonb where anafuncname='用户总数4';
UPDATE crm_sys_analyse_func set anafuncname_lang = '{"cn":"用户总数5","en":"Total Number Of Users5","tw":"用戶總數3"}'::jsonb where anafuncname='用户总数5';
UPDATE crm_sys_analyse_func set anafuncname_lang = '{"cn":"用户总数6","en":"Total Number Of Users6","tw":"用戶總數3"}'::jsonb where anafuncname='用户总数6';
UPDATE crm_sys_analyse_func set anafuncname_lang = '{"cn":"用户总数7","en":"Total Number Of Users7","tw":"用戶總數3"}'::jsonb where anafuncname='用户总数7';
UPDATE crm_sys_analyse_func set anafuncname_lang = '{"cn":"用户总数8","en":"Total Number Of Users8","tw":"用戶總數3"}'::jsonb where anafuncname='用户总数8';
UPDATE crm_sys_analyse_func set anafuncname_lang = '{"cn":"用户总数9","en":"Total Number Of Users9","tw":"用戶總數3"}'::jsonb where anafuncname='用户总数9';
UPDATE crm_sys_analyse_func set anafuncname_lang = '{"cn":"用户总数10","en":"Total Number Of Users10","tw":"用戶總數3"}'::jsonb where anafuncname='用户总数10';
UPDATE crm_sys_analyse_func set anafuncname_lang = '{"cn":"新增客户数","en":"New Customers","tw":"新增客戶數"}'::jsonb where anafuncname='新增客户数';
UPDATE crm_sys_analyse_func set anafuncname_lang = '{"cn":"新增商机数","en":"New Business Opportunities","tw":"新增商機數"}'::jsonb where anafuncname='新增商机数';
UPDATE crm_sys_analyse_func set anafuncname_lang = '{"cn":"跟进客户数","en":"Follow Up Customer","tw":"跟進客戶數"}'::jsonb where anafuncname='跟进客户数';