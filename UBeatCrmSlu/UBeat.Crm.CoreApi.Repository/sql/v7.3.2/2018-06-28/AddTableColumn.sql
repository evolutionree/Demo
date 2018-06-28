alter table crm_sys_entity add entitylanguage jsonb;

alter table crm_sys_entity_fields add fieldlanguage jsonb;
alter table crm_sys_entity_fields add displaylanguage jsonb;

alter table crm_sys_entity_rel_tab add reltablanguage jsonb;

alter table crm_sys_workflow add flowlanguage jsonb;

alter table crm_sys_entity add funcbtnlanguage jsonb;

alter table crm_sys_dictionary_type add dictypelanguage jsonb;

alter table crm_sys_dictionary add datalanguage jsonb;

alter table crm_sys_department add deptlanguage jsonb;

alter table crm_sys_products_series add serieslanguage jsonb;

alter table crm_sys_entity_category add categorylanguage jsonb;

alter table crm_sys_entity_datasource add datasourcelanguage jsonb;

alter table crm_sys_role_group add grouplanguage jsonb;

alter table crm_sys_role add rolelanguage jsonb;

alter table crm_sys_vocation add vocationlanguage jsonb;

alter table crm_sys_qrtz_triggerdefine add reclanguage jsonb;

alter table crm_sys_sales_target_norm_type add reclanguage jsonb;

alter table crm_sys_reminder add reminderlanguage jsonb;

alter table crm_sys_reportdefine add reclanguage jsonb;

alter table crm_sys_reportdatasource add reclanguage jsonb;

alter table crm_sys_mainpageitem add reclanguage jsonb;

alter table crm_sys_mainpagedefine add reclanguage jsonb;
