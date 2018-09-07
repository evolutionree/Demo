alter table crm_sys_analyse_func_active add column groupmark_lang jsonb;


UPDATE crm_sys_analyse_func_active set groupmark_lang = '{"cn":"{NOW}当月统计","en":"{NOW}Monthly Statistics","tw":"{NOW}當月統計"}'::jsonb where groupmark='{NOW}当月统计';
UPDATE crm_sys_analyse_func_active set groupmark_lang = '{"cn":"次月统计","en":"Sub month statistics","tw":"次月統計"}'::jsonb where groupmark='次月统计';