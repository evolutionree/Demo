alter table crm_sys_entity_extfunction add enginetype int4 not null default(1);

alter table crm_sys_entity_extfunction add uscript text;

update crm_sys_entity_extfunction set enginetype =1 ;