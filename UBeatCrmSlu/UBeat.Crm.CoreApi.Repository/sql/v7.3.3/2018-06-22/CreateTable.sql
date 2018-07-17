CREATE TABLE "public"."crm_sys_temporary_entity" (
"recmanager" int,
"createdtime" timestamp(6),
"datajson" jsonb,
"fieldjson" jsonb,
"typeid" uuid,
"cacheid" uuid NOT NULL,
"inputjson" jsonb,
"title" text,
"fieldname" text,
"recrelateid" text,
"relateentityid" text,
"relatetypeid" text,
CONSTRAINT "cacheid" UNIQUE ("cacheid")
)