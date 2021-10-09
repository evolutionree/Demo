using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Repository;
using UBeat.Crm.CoreApi.Repository.Utility;
using static IRCS.DBUtility.DbHelperOra;

namespace UBeat.Crm.CoreApi.ZGQY.Repository
{
    public class OaDataRepository : RepositoryBase, IOaDataRepository
    {
        public int insertContract(DataRow list, int userId, DbTransaction tran = null)
        {
            var sql = "insert into crm_sys_contract (recname,reccode,rectype,recstatus,reccreator,recupdator,recmanager,reccreated,recupdated,customer,filestatus,contractid,contracttype,flowertime,contractamount,signdate,remark,filedate,otherinfo,deptgroup,predeptgroup,flowstatus,opportunity,commonid,signdept,contracttypemin,syncflag,createtime,contracttypeother,class1id,class2id,class3id,recmanagercode) values (@recname,@reccode,@rectype,@recstatus,@reccreator,@recupdator,@recmanager,now(),now(),@customer::jsonb,@filestatus,@contractid,@contracttype,@flowertime,@contractamount::numeric,@signdate::date ,@remark,@filedate::date ,@otherinfo,@deptgroup::uuid,@predeptgroup::uuid,@flowstatus,@opportunity::jsonb,@commonid::jsonb,@signdept,@contracttypemin,@syncflag,@createtime::timestamp,@contracttypeother,@class1id::int8,@class2id::int8,@class3id::int8,@recmanagercode)";
            //var now = new TimeSpan(0, 0, 0, 0);
            var now = new Timestamp();
            
            var recmanager = 1;
            var recmanagerSql = "select userid from crm_sys_userinfo where workcode=@param1";
            List<Dictionary<string ,object >> recmanagerList=ExecuteQuery(recmanagerSql, new DbParameter[] { new NpgsqlParameter("param1",list[1].ToString())} );
            
            if (recmanagerList!=null&&recmanagerList.Count>0)
            {
                recmanager=(int)recmanagerList[0]["userid"];
            }
            
            if(recmanager == 1)
            {
                return 0;
            }

            //暂时处理重复的合同号不做写入
            var repeatSql = "select * from crm_sys_contract where contractid=@billno";
            List<Dictionary<string ,object >> repeatList=ExecuteQuery(repeatSql, new DbParameter[] { new NpgsqlParameter("billno",list[5].ToString())} );
            if (repeatList!=null&&repeatList.Count>0)
            {
                return 0;
            }
            
            var customerStr = "";
            var customerSql = "select recid from crm_sys_custcommon  where recname=@param1";
            List<Dictionary<string ,object >> customerList=ExecuteQuery(customerSql, new DbParameter[] { new NpgsqlParameter("param1",list[7].ToString())} );
            if (customerList!=null&&customerList.Count>0)
            {
                customerStr = "{\"name\": \"" + list[7] + "\" ,\"id\": \"" + customerList[0]["recid"] + "\"}";
            }
            else
            {
                customerStr = null;
            }

            var today = DateTime.Today.ToString("yyyyMMdd");
            
            var param = new DbParameter[] {
                new NpgsqlParameter("recname",list[6].ToString()),
                new NpgsqlParameter("reccode",today),
                new NpgsqlParameter("rectype",new Guid("239a7c69-8238-413d-b1d9-a0d51651abfa")),
                new NpgsqlParameter("recstatus",1),
                new NpgsqlParameter("reccreator",userId),
                new NpgsqlParameter("recupdator",userId),
                new NpgsqlParameter("recmanager",recmanager),
                new NpgsqlParameter("customer",null),
                new NpgsqlParameter("filestatus",null),
                new NpgsqlParameter("contractid",list[5].ToString()),
                new NpgsqlParameter("contracttype",list[13].ToString()),
                new NpgsqlParameter("flowertime",null),
                new NpgsqlParameter("contractamount",list[9]),
                new NpgsqlParameter("signdate",list[8]),
                new NpgsqlParameter("remark",null),
                new NpgsqlParameter("filedate",null),
                new NpgsqlParameter("otherinfo",null),
                new NpgsqlParameter("deptgroup",null),
                new NpgsqlParameter("predeptgroup",null),
                new NpgsqlParameter("flowstatus",1),
                new NpgsqlParameter("opportunity",null),//TODO OA数据库没有提供商机字段
                new NpgsqlParameter("commonid",customerStr),
                new NpgsqlParameter("signdept",list[11].ToString()),
                new NpgsqlParameter("contracttypemin",list[14].ToString()),
                new NpgsqlParameter("syncflag",1),  //同步类型 1：同步；2：变更
                new NpgsqlParameter("createtime", list[16].ToString()),
                new NpgsqlParameter("contracttypeother",list[15].ToString()),
                new NpgsqlParameter("class1id", list[2]),
                new NpgsqlParameter("class2id", list[3]),
                new NpgsqlParameter("class3id", list[4]),
                new NpgsqlParameter("recmanagercode",list[1].ToString())
                
            };
            
           return  ExecuteNonQuery(sql, param);

        }
        
        public int changeContract(DataRow list, int userId, DbTransaction tran = null)
        {
            var dataSql = "select * from crm_sys_contract where contractid=@contractid and contractstat!=2";
            List<Dictionary<string ,object >> bill=ExecuteQuery(dataSql, new DbParameter[] { new NpgsqlParameter("contractid",list[3])} );
            if (bill!=null)
            {
                if (bill.Count!=1)
                {
                    return 0;
                    //throw new Exception("不存在原合同：" + list[3]);可改用日志的方式输出
                }
                
                if (bill[0]["contractid"]==list[4]&&bill[0]["contractamount"]==list[6])
                {
                    return 0;
                }
            }
            
            //标记为作废
            var updateBillSql = "update crm_sys_contract set contractstat=2 where contractid=@contractid ";
            var updateParam = new DbParameter[] {
                new NpgsqlParameter("contractid",list[3])
            };
            

            //变更逻辑
            if (list[10].ToString() == "7708276074882810143")
            {
                //复制生成一张单据，修改单号，原单号，合同金额
                var sql =
                    "insert into crm_sys_contract (recname,reccode,rectype,recstatus,reccreator,recupdator,recmanager,reccreated,recupdated,customer,filestatus,contractid,contracttype,flowertime,contractamount,signdate ,remark,filedate ,otherinfo,deptgroup,predeptgroup,flowstatus,opportunity,commonid,signdept,contracttypemin,syncflag,createtime,contracttypeother,class1id,class2id,class3id,recmanagercode) values (@recname,@reccode,@rectype,@recstatus,@reccreator,@recupdator,@recmanager,now(),now(),@customer::jsonb,@filestatus,@contractid,@contracttype,@flowertime,@contractamount,@signdate::date ,@remark,@filedate::date ,@otherinfo,@deptgroup,@predeptgroup,@flowstatus,@opportunity::jsonb,@commonid::jsonb,@signdept,@contracttypemin,@syncflag,@createtime::timestamp,@contracttypeother,@class1id::int8,@class2id::int8,@class3id::int8,@recmanagercode)";
                //var now = new TimeSpan(0, 0, 0, 0);
                var now = new Timestamp();
                var param = new DbParameter[]
                {
                    new NpgsqlParameter("recname", bill[0]["recname"]),
                    new NpgsqlParameter("reccode", bill[0]["reccode"]),
                    new NpgsqlParameter("rectype", bill[0]["rectype"]),
                    new NpgsqlParameter("recstatus", bill[0]["recstatus"]),
                    new NpgsqlParameter("reccreator", bill[0]["reccreator"]),
                    new NpgsqlParameter("recupdator", bill[0]["recupdator"]),
                    new NpgsqlParameter("recmanager", bill[0]["recmanager"]),
                    new NpgsqlParameter("customer", bill[0]["customer"]),
                    new NpgsqlParameter("filestatus", bill[0]["filestatus"]),
                    new NpgsqlParameter("contractid", list[4]),
                    new NpgsqlParameter("contracttype", bill[0]["contracttype"]),
                    new NpgsqlParameter("flowertime", bill[0]["flowertime"]),
                    new NpgsqlParameter("contractamount",list[6]),
                    new NpgsqlParameter("signdate", bill[0]["signdate"]),
                    new NpgsqlParameter("remark", bill[0]["remark"]),
                    new NpgsqlParameter("filedate", bill[0]["filedate"]),
                    new NpgsqlParameter("otherinfo", bill[0]["otherinfo"]),
                    new NpgsqlParameter("deptgroup", bill[0]["deptgroup"]),
                    new NpgsqlParameter("predeptgroup", bill[0]["predeptgroup"]),
                    new NpgsqlParameter("flowstatus", bill[0]["flowstatus"]),
                    new NpgsqlParameter("opportunity", bill[0]["opportunity"]), //TODO OA数据库没有提供商机字段
                    new NpgsqlParameter("commonid", bill[0]["commonid"]),
                    new NpgsqlParameter("signdept", bill[0]["signdept"]),
                    new NpgsqlParameter("contracttypemin", bill[0]["contracttypemin"]),
                    new NpgsqlParameter("syncflag", 2),
                    new NpgsqlParameter("createtime", list[18].ToString()),
                    new NpgsqlParameter("contracttypeother", bill[0]["contracttypeother"]),
                    new NpgsqlParameter("class1id",bill[0]["class1id"]),
                    new NpgsqlParameter("class2id",bill[0]["class2id"]),
                    new NpgsqlParameter("class3id", bill[0]["class3id"]),
                    new NpgsqlParameter("recmanagercode", bill[0]["recmanagercode"])
                };
                
                return ExecuteNonQuery(sql, param, tran);
            }

            return ExecuteNonQuery(updateBillSql, updateParam, tran);
        }
        
        
        
        public DataSet getContractFromOa()
        {
            var createTimeSql = "select max(createtime) createtime from crm_sys_contract where syncflag=1";
            
            string sqlString = "SELECT field0004     deptid,\n" +
                               "       om.code     userid,\n" + 
                               "       field0012     class1id,\n" + 
                               "       field0013     class2id,\n" + 
                               "       field0014     class3id,\n" + 
                               "       field0002     billno,\n" + 
                               "       field0007     billname,\n" + 
                               "       field0018     billobject,\n" + 
                               "       field0006     billdate,\n" + 
                               "       field0021     amt,\n" + 
                               "       FINISHEDFLAG  stat,\n" + 
                               "       ou.name       deptname,\n" + 
                               "       om.name       membername,\n" + 
                               "       it1.showvalue class1,\n" + 
                               "       it2.showvalue class2,\n" + 
                               "       it3.showvalue class3,\n" + 
                               "       fm.modify_date\n" + 
                               "  FROM OAADMIN.formmain_8680 fm\n" + 
                               "  left join OAADMIN.ORG_UNIT ou\n" + 
                               "    on fm.field0004 = ou.id\n" + 
                               "  left join OAADMIN.org_member om\n" + 
                               "    on fm.field0005 = om.id\n" + 
                               "  left join OAADMIN.ctp_enum_item it1\n" + 
                               "    on fm.field0012 = it1.id\n" + 
                               "  left join OAADMIN.ctp_enum_item it2\n" + 
                               "    on fm.field0013 = it2.id\n" + 
                               "  left join OAADMIN.ctp_enum_item it3\n" + 
                               "    on fm.field0014 = it3.id\n" + 
                               " WHERE FINISHEDFLAG = 1 and field0002 is not null  ";
            
            List<Dictionary<string ,object >> list=ExecuteQuery(createTimeSql, null);

            if (list[0]["createtime"]!=null)
            {
                var a =list[0]["createtime"].ToString();
                //sqlString += "and fm.modify_date > to_date('"+a+"','yyyy-mm-dd hh24:mi:ss')";
            }

            sqlString += "union all SELECT FIELD0003     deptid,\n" +
                                   "       om.code     userid,\n" + 
                                   "       FIELD0006     class1id,\n" + 
                                   "       FIELD0007     class2id,\n" + 
                                   "       null     class3id,\n" + 
                                   "       FIELD0005     billno,\n" + 
                                   "       FIELD0004     billname,\n" + 
                                   "       FIELD0010     billobject,\n" + 
                                   "       FIELD0019     billdate,\n" + 
                                   "       FIELD0013     amt,\n" + 
                                   "       FINISHEDFLAG  stat,\n" + 
                                   "       ou.name       deptname,\n" + 
                                   "       om.name       membername,\n" + 
                                   "       it1.showvalue class1,\n" + 
                                   "       it2.showvalue class2,\n" + 
                                   "       null class3,\n" + 
                                   "       fm.modify_date\n" + 
                                   "  FROM OAADMIN.FORMMAIN_8740 fm\n" + 
                                   "  left join OAADMIN.ORG_UNIT ou\n" + 
                                   "    on fm.FIELD0002 = ou.id\n" + 
                                   "  left join OAADMIN.org_member om\n" + 
                                   "    on fm.FIELD0003 = om.id\n" + 
                                   "  left join OAADMIN.ctp_enum_item it1\n" + 
                                   "    on fm.FIELD0006 = it1.id\n" + 
                                   "  left join OAADMIN.ctp_enum_item it2\n" + 
                                   "    on fm.FIELD0007 = it2.id\n" + 
                                   " WHERE FINISHEDFLAG = 1 and FIELD0005 is not null ";
            if (list[0]["createtime"]!=null)
            {
                var a =list[0]["createtime"].ToString();
                //sqlString += "and fm.modify_date > to_date('"+a+"','yyyy-mm-dd hh24:mi:ss')";
            }
            
            sqlString += "union all\n" +
                          " SELECT field0041     deptid,\n" + 
                          "      om.code     userid,\n" + 
                          "      field0005     class1id,\n" + 
                          "      null     class2id,\n" + 
                          "      null     class3id,\n" + 
                          "      field0001     billno,\n" + 
                          "      field0003     billname,\n" + 
                          "      field0009     billobject,\n" + 
                          "      field0042     billdate,\n" + 
                          "      field0014     amt,\n" + 
                          "      FINISHEDFLAG  stat,\n" + 
                          "      ou.name       deptname,\n" + 
                          "      om.name       membername,\n" + 
                          "      it1.showvalue class1,\n" + 
                          "      null class2,\n" + 
                          "      null class3,\n" + 
                          "      fm.modify_date\n" + 
                          " FROM OAADMIN.formmain_10003 fm\n" + 
                          " left join OAADMIN.ORG_UNIT ou\n" + 
                          "   on fm.field0041 = ou.id\n" + 
                          " left join OAADMIN.org_member om\n" + 
                          "   on fm.field0040 = om.id\n" + 
                          " left join OAADMIN.ctp_enum_item it1\n" + 
                          "   on fm.field0005 = it1.id\n" + 
                          "WHERE FINISHEDFLAG = 1 and field0001 is not null ";

            if (list[0]["createtime"]!=null)
            {
                var a =list[0]["createtime"].ToString();
                //sqlString += "and fm.modify_date > to_date('"+a+"','yyyy-mm-dd hh24:mi:ss')";
            }
            
            return  Query(sqlString);

        }
        
        public DataSet getContractFromOaChange()
        {
            
            var createTimeSql = "select max(createtime) createtime from crm_sys_contract where syncflag=2";
           
            string sqlString = " SELECT fm.ID,\n" +
                               "      om.code     userid,\n" + 
                               "      field0002     deptid,\n" + 
                               "      field0003     billno,\n" + 
                               "      field0020     billnochange,\n" + 
                               "      field0009     amt,\n" + 
                               "      field0010     amtchange,\n" + 
                               "      field0004     class1id,\n" + 
                               "      null     class2id,\n" + 
                               "      null     class3id,\n" + 
                               "      field0015     changetype,\n" + 
                               "      FINISHEDFLAG  stat,\n" + 
                               "      csm.CREATE_DATE changedate,\n" + 
                               "      ou.name       deptname,\n" + 
                               "      om.code       membername,\n" + 
                               "      it1.showvalue class1,\n" + 
                               "      null class2,\n" + 
                               "      null class3,\n" + 
                               "      fm.modify_date\n" + 
                               " FROM OAADMIN.formmain_4912 fm\n" + 
                               " left join OAADMIN.ORG_UNIT ou\n" + 
                               "   on fm.FIELD0002 = ou.id\n" + 
                               " left join OAADMIN.org_member om\n" + 
                               "   on fm.FIELD0001 = om.id\n" + 
                               " left join OAADMIN.ctp_enum_item it1\n" + 
                               "   on fm.field0004 = it1.id\n" + 
                               "   left join  OAADMIN.COL_SUMMARY csm\n" + 
                               "   on csm.FORM_RECORDID=fm.id\n" + 
                               "WHERE FINISHEDFLAG = 1 ";
            List<Dictionary<string ,object >> list=ExecuteQuery(createTimeSql, null);

            if (list[0]["createtime"]!=null)
            {
                var a =list[0]["createtime"].ToString();
                //sqlString += "and fm.modify_date > to_date('"+a+"','yyyy-mm-dd hh24:mi:ss')";
            }

            sqlString +=  " union all\n" +
                                   "SELECT fm.ID,\n" + 
                                   "       om.code     userid,\n" + 
                                   "       field0002     deptid,\n" + 
                                   "       FIELD0006     billno,\n" + 
                                   "       field0003     billnochange,\n" + 
                                   "       field0007     amt,\n" + 
                                   "       field0016     amtchange,\n" + 
                                   "       to_char(field0024)     class1id,\n" + 
                                   "       to_char(field0025)     class2id,\n" + 
                                   "       to_char(field0026)     class3id,\n" + 
                                   "       to_char(field0004)     changetype,\n" + 
                                   "       FINISHEDFLAG  stat,\n" + 
                                   "       csm.CREATE_DATE changedate,\n" + 
                                   "       ou.name       deptname,\n" + 
                                   "       om.code       membername,\n" + 
                                   "       it1.showvalue class1,\n" + 
                                   "       it2.showvalue class2,\n" + 
                                   "       it3.showvalue class3,\n" + 
                                   "      fm.modify_date\n" + 
                                   "  FROM OAADMIN.formmain_8679 fm\n" + 
                                   "  left join OAADMIN.ORG_UNIT ou\n" + 
                                   "    on fm.FIELD0002 = ou.id\n" + 
                                   "  left join OAADMIN.org_member om\n" + 
                                   "    on fm.FIELD0001 = om.id\n" + 
                                   "  left join OAADMIN.ctp_enum_item it1\n" + 
                                   "    on fm.field0024 = it1.id\n" + 
                                   "  left join OAADMIN.ctp_enum_item it2\n" + 
                                   "    on fm.field0025 = it2.id\n" + 
                                   "   left join OAADMIN.ctp_enum_item it3\n" + 
                                   "    on fm.field0026 = it3.id\n" + 
                                   "    left join  OAADMIN.COL_SUMMARY csm\n" + 
                                   "    on csm.FORM_RECORDID=fm.id\n" + 
                                   " WHERE FINISHEDFLAG = 1 ";

            if (list[0]["createtime"]!=null)
            {
                var a =list[0]["createtime"].ToString();
                //sqlString += "and fm.modify_date > to_date('"+a+"','yyyy-mm-dd hh24:mi:ss')";
            }
            sqlString += " union all\n" +
                         "SELECT fm.ID,\n" +
                         "                                     om.code     userid,\n" +
                         "                                     field0041     deptid,\n" +
                         "                                     field0047     billno,\n" +
                         "                                     field0001     billnochange,\n" +
                         "                                     null     amt,\n" +
                         "                                     field0014     amtchange,\n" +
                         "                                     to_char(field0005)     class1id,\n" +
                         "                                     null     class2id,\n" +
                         "                                     null     class3id,\n" +
                         "                                     case it2.showvalue when '变更' then '7708276074882810143' when '作废' then '-2546487215894409528' else null end   changetype,\n" +
                         "                                     FINISHEDFLAG  stat,\n" +
                         "                                     csm.CREATE_DATE changedate,\n" +
                         "                                     ou.name       deptname,\n" +
                         "                                     om.code       membername,\n" +
                         "                                     null class1,\n" +
                         "                                     null class2,\n" +
                         "                                     null class3,\n" +
                         "                                     fm.modify_date\n" + 
                         "                                FROM OAADMIN.formmain_10037 fm\n" +
                         "                                left join OAADMIN.ORG_UNIT ou\n" +
                         "                                  on fm.field0041 = ou.id\n" +
                         "                                left join OAADMIN.org_member om\n" +
                         "                                  on fm.field0040 = om.id\n" +
                         "                                left join OAADMIN.ctp_enum_item it1\n" +
                         "                                  on fm.field0005 = it1.id\n" +
                         "                                  left join  OAADMIN.COL_SUMMARY csm\n" +
                         "                                  on csm.FORM_RECORDID=fm.id\n" +
                         "																left join OAADMIN.ctp_enum_item it2\n" +
                         "																on fm.field0039=it2.id\n" +
                         "                               WHERE FINISHEDFLAG = 1";

            if (list[0]["createtime"]!=null)
            {
                var a =list[0]["createtime"].ToString();
                //sqlString += "and fm.modify_date > to_date('"+a+"','yyyy-mm-dd hh24:mi:ss')";
            }
            
            return  Query(sqlString);

        }

        public DataSet getCustomerRiskFromOa()
        {
            string sqlString = @" 
            SELECT A.field0001 customercode, A.field0002 customername, B.SHOWVALUE ishasrisk
            FROM OAADMIN.formmain_8511 A
            LEFT JOIN OAADMIN.ctp_enum_item B ON B.ID = A.field0071
            where 1 = 1-- A.field0002 = '重庆溯联汽车零部件有限公司'
            --AND A.field0074 = '-803485964791821589'
            --   9204212548207594459  启用
            -- - 803485964791821589 停用"; 

            return Query(sqlString);
        }

        public int updateCustomerRisk(DataRow list, int userId, DbTransaction tran = null)
        {
            var sql = @"update crm_sys_customer set ishasrisk = @ishasrisk where customercode = @customercode;
                        update crm_sys_custcommon set ishasrisk = @ishasrisk where customercode = @customercode;";
         
            var param = new DbParameter[] {
                new NpgsqlParameter("customercode",list[0].ToString()),
                new NpgsqlParameter("ishasrisk",list[1].ToString() == "是"?1:2)
            };

            return ExecuteNonQuery(sql, param);

        }
    }
}
