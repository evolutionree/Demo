
alter table crm_sys_notify_group add column msggroupname_lang jsonb;


update crm_sys_notify_group set msggroupname_lang = '{"cn":"��������","en":"Work Report","tw":"�������"}'::jsonb where msggroupname='��������';
update crm_sys_notify_group set msggroupname_lang = '{"cn":"��������","en":"Task Reminding","tw":"�΄�����"}'::jsonb where msggroupname='��������';
update crm_sys_notify_group set msggroupname_lang = '{"cn":"���ۼ�¼","en":"Sales Record","tw":"�N��ӛ�"}'::jsonb where msggroupname='���ۼ�¼';
update crm_sys_notify_group set msggroupname_lang = '{"cn":"����֪ͨ","en":"Approval Notice","tw":"����֪ͨ"}'::jsonb where msggroupname='����֪ͨ';
update crm_sys_notify_group set msggroupname_lang = '{"cn":"����֪ͨ","en":"Announcement","tw":"����֪ͨ"}'::jsonb where msggroupname='����֪ͨ';
update crm_sys_notify_group set msggroupname_lang = '{"cn":"ʵʱ��̬","en":"Real-Time Dynamic","tw":"���r�ӑB"}'::jsonb where msggroupname='ʵʱ��̬';
update crm_sys_notify_group set msggroupname_lang = '{"cn":"ʵʱ����","en":"Live Chat","tw":"���r����"}'::jsonb where msggroupname='ʵʱ����';
update crm_sys_notify_group set msggroupname_lang = '{"cn":"�ձ��ܱ�","en":"Daily, weekly","tw":"�Ո��L��"}'::jsonb where msggroupname='�ձ��ܱ�';