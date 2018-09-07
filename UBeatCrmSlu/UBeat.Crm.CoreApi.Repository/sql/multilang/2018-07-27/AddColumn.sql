alter TABLE crm_sys_rule add column relid uuid;
alter TABLE crm_sys_rule_item add column relid uuid;
alter TABLE crm_sys_rule_set add column relid uuid;
alter TABLE crm_sys_rule_item_relation add column relid uuid;