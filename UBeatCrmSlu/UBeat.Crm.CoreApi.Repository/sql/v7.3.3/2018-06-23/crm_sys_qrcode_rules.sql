CREATE TABLE "public"."crm_sys_qrcode_rules" (
"recid" uuid DEFAULT uuid_generate_v4() NOT NULL,
"recname" text,
"remark" text,
"checktype" int4,
"checkparam" jsonb,
"dealtype" int4,
"dealparam" varchar(255),
"recorder" int4,
"recstatus" int4,
"reccreator" int4,
"reccreated" timestamp(255),
"recupdator" int4,
"recupdated" timestamp,
PRIMARY KEY ("recid")
)
WITH (OIDS=FALSE)
;