
alter table crm_sys_notify_group add column msggroupname_lang jsonb;


update crm_sys_notify_group set msggroupname_lang = '{"cn":"工作报告","en":"Work Report","tw":"工作報告"}'::jsonb where msggroupname='工作报告';
update crm_sys_notify_group set msggroupname_lang = '{"cn":"任务提醒","en":"Task Reminding","tw":"任務提醒"}'::jsonb where msggroupname='任务提醒';
update crm_sys_notify_group set msggroupname_lang = '{"cn":"销售记录","en":"Sales Record","tw":"銷售記錄"}'::jsonb where msggroupname='销售记录';
update crm_sys_notify_group set msggroupname_lang = '{"cn":"审批通知","en":"Approval Notice","tw":"審批通知"}'::jsonb where msggroupname='审批通知';
update crm_sys_notify_group set msggroupname_lang = '{"cn":"公告通知","en":"Announcement","tw":"公告通知"}'::jsonb where msggroupname='公告通知';
update crm_sys_notify_group set msggroupname_lang = '{"cn":"实时动态","en":"Real-Time Dynamic","tw":"實時動態"}'::jsonb where msggroupname='实时动态';
update crm_sys_notify_group set msggroupname_lang = '{"cn":"实时聊天","en":"Live Chat","tw":"實時聊天"}'::jsonb where msggroupname='实时聊天';
update crm_sys_notify_group set msggroupname_lang = '{"cn":"日报周报","en":"Daily, weekly","tw":"日報週報"}'::jsonb where msggroupname='日报周报';