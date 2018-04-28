CREATE TABLE "public"."crm_sys_pm_orgscheme" (
"recid" uuid DEFAULT uuid_generate_v4() NOT NULL,
"schemename" text COLLATE "default" NOT NULL,
"recstatus" int4 DEFAULT 1 NOT NULL,
"remark" text COLLATE "default",
"isdefault" int4 DEFAULT 0 NOT NULL,
CONSTRAINT "crm_sys_pm_orgscheme_pkey" PRIMARY KEY ("recid")
)
WITH (OIDS=FALSE)
;

ALTER TABLE "public"."crm_sys_pm_orgscheme" OWNER TO "postgres";

COMMENT ON COLUMN "public"."crm_sys_pm_orgscheme"."recid" IS '方案id';

COMMENT ON COLUMN "public"."crm_sys_pm_orgscheme"."schemename" IS '组织人员授权方案名称';

COMMENT ON COLUMN "public"."crm_sys_pm_orgscheme"."recstatus" IS '方案状态，默认为1=启用，0=禁用';

COMMENT ON COLUMN "public"."crm_sys_pm_orgscheme"."remark" IS '方案备注';

COMMENT ON COLUMN "public"."crm_sys_pm_orgscheme"."isdefault" IS '是否默认方案，1=默认方案，如果是默认方案，则表示如果没有其他说明情况，获取组织和用户方案时按此方案执行';



CREATE TABLE "public"."crm_sys_pm_orgschemeentry" (
"recid" uuid DEFAULT uuid_generate_v4() NOT NULL,
"schemeid" uuid NOT NULL,
"authorized_userid" int4,
"authorized_roleid" uuid,
"authorized_type" int4 DEFAULT 1,
"pmobject_userid" int4 DEFAULT 1 NOT NULL,
"pmobject_deptid" uuid,
"pmobject_type" int4 DEFAULT 1 NOT NULL,
"permissiontype" int4 DEFAULT 0,
"subdeptpermission" int4 DEFAULT 1,
"subuserpermission" int4 DEFAULT 1,
CONSTRAINT "crm_sys_pm_orgschemeentry_pkey" PRIMARY KEY ("recid")
)
WITH (OIDS=FALSE)
;

ALTER TABLE "public"."crm_sys_pm_orgschemeentry" OWNER TO "postgres";

COMMENT ON COLUMN "public"."crm_sys_pm_orgschemeentry"."schemeid" IS '对应的方案id';

COMMENT ON COLUMN "public"."crm_sys_pm_orgschemeentry"."authorized_type" IS '获取权限人员类型，1=用户，2=部门';

COMMENT ON COLUMN "public"."crm_sys_pm_orgschemeentry"."pmobject_type" IS '授权对象类型，1=人员，2=部门';

COMMENT ON COLUMN "public"."crm_sys_pm_orgschemeentry"."permissiontype" IS '授权类型，0=未设置，1=授予拥有权，2=未授予拥有权，3=拒绝授予权限，';

COMMENT ON COLUMN "public"."crm_sys_pm_orgschemeentry"."subdeptpermission" IS '自动拥有下级部门权限，1=自动拥有，0=自动未拥有';

COMMENT ON COLUMN "public"."crm_sys_pm_orgschemeentry"."subuserpermission" IS '自动拥有所有员工权限，0=未自动拥有，1=自动未拥有';