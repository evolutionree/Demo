alter table crm_sys_analyse_func_active add column groupmark_lang jsonb;


UPDATE crm_sys_analyse_func_active set groupmark_lang = '{"cn":"{NOW}����ͳ��","en":"{NOW}Monthly Statistics","tw":"{NOW}���½yӋ"}'::jsonb where groupmark='{NOW}����ͳ��';
UPDATE crm_sys_analyse_func_active set groupmark_lang = '{"cn":"����ͳ��","en":"Sub month statistics","tw":"���½yӋ"}'::jsonb where groupmark='����ͳ��';