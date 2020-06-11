using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository
{
    public class DockingAPIRepository : RepositoryBase, IDockingAPIRepository
    {
        public OperateResult InsertBussinessInfomation(BussinessInformation data, int userNumber)
        {
            var executeSql = @"insert into crm_sys_bussiness_infomation(basicinfo,yearreport,casedetail,lawsuit,courtnotice,breakpromise,companyname,reccreator,recupdator,idforengin) values (@basicinfo::jsonb,@yearreport::jsonb,@casedetail::jsonb,@lawsuit::jsonb,@courtnotice::jsonb,@breakpromise::jsonb,@companyname,@reccreator,@recupdator,@idforengin)";

            var existSql = " select count(1) from crm_sys_bussiness_infomation where companyname=@companyname;";
            var args = new
            {
                idforengin = data.Id,
                basicinfo = data.BasicInfo,
                yearreport = data.YearReport,
                lawsuit = data.LawSuit,
                courtnotice = data.CourtNotice,
                breakpromise = data.BreakPromise,
                casedetail = data.CaseDetail,
                companyname = data.CompanyName,
                reccreator = userNumber,
                recupdator = userNumber
            };
            var result = DataBaseHelper.QuerySingle<int>(existSql, args);
            if (result == 1)
            {
                this.UpdateBussinessInfomation(data, userNumber);
                return new OperateResult { Flag = 1 };
            }

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }
        public OperateResult InsertForeignBussinessInfomation(BussinessInformation data, int userNumber)
        {
            var executeSql = @"insert into crm_sys_bussiness_infomation(basicinfo,yearreport,casedetail,lawsuit,courtnotice,breakpromise,companyname,reccreator,recupdator) values (@basicinfo::jsonb,@yearreport::jsonb,@casedetail::jsonb,@lawsuit::jsonb,@courtnotice::jsonb,@breakpromise::jsonb,@companyname,@reccreator,@recupdator)";

            var existSql = " select count(1) from crm_sys_bussiness_infomation where idforengin=@idforengin;";
            var args = new
            {
                basicinfo = data.BasicInfo,
                yearreport = data.YearReport,
                lawsuit = data.LawSuit,
                courtnotice = data.CourtNotice,
                breakpromise = data.BreakPromise,
                casedetail = data.CaseDetail,
                companyname = data.CompanyName,
                reccreator = userNumber,
                recupdator = userNumber
            };
            var result = DataBaseHelper.QuerySingle<int>(existSql, args);
            if (result == 1)
            {
                this.UpdateBussinessInfomation(data, userNumber);
                return new OperateResult { Flag = 1 };
            }

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }
        public void UpdateBussinessInfomation(BussinessInformation data, int userNumber)
        {
            var executeSql = @"  update  crm_sys_bussiness_infomation set {0} recupdator=@recupdator,recupdated=now()   where companyname=@companyname;";
            DynamicParameters args = new DynamicParameters();
            args.Add("companyname", data.CompanyName);
            args.Add("recupdator", userNumber);
            string condition = string.Empty;
            if (!string.IsNullOrEmpty(data.BasicInfo))
            {
                args.Add("basicinfo", data.BasicInfo);
                condition = " basicinfo=@basicinfo::jsonb, ";
            }
            if (!string.IsNullOrEmpty(data.YearReport))
            {
                args.Add("yearreport", data.YearReport);
                condition += " yearreport=@yearreport::jsonb, ";
            }
            if (!string.IsNullOrEmpty(data.CaseDetail))
            {
                args.Add("casedetail", data.CaseDetail);
                condition += " casedetail=@casedetail::jsonb, ";
            }
            if (!string.IsNullOrEmpty(data.LawSuit))
            {
                args.Add("lawsuit", data.LawSuit);
                condition += " lawsuit=@lawsuit::jsonb, ";
            }
            if (!string.IsNullOrEmpty(data.CourtNotice))
            {
                args.Add("courtnotice", data.CourtNotice);
                condition += " courtnotice=@courtnotice::jsonb, ";
            }
            if (!string.IsNullOrEmpty(data.BreakPromise))
            {
                args.Add("breakpromise", data.BreakPromise);
                condition += " breakpromise=@breakpromise::jsonb, ";
            }

            DataBaseHelper.QuerySingle<OperateResult>(string.Format(executeSql, condition), args);
        }
        public void UpdateForeignBussinessInfomation(BussinessInformation data, int userNumber)
        {
            var executeSql = @"  update  crm_sys_bussiness_infomation set {0} recupdator=@recupdator,recupdated=now()   where idforengin=@idforengin;";
            DynamicParameters args = new DynamicParameters();
            args.Add("idforengin", data.Id);
            args.Add("recupdator", userNumber);
            string condition = string.Empty;
            if (!string.IsNullOrEmpty(data.BasicInfo))
            {
                args.Add("basicinfo", data.BasicInfo);
                condition = " basicinfo=@basicinfo::jsonb, ";
            }
            if (!string.IsNullOrEmpty(data.YearReport))
            {
                args.Add("yearreport", data.YearReport);
                condition += " yearreport=@yearreport::jsonb, ";
            }
            if (!string.IsNullOrEmpty(data.CaseDetail))
            {
                args.Add("casedetail", data.CaseDetail);
                condition += " casedetail=@casedetail::jsonb, ";
            }
            if (!string.IsNullOrEmpty(data.LawSuit))
            {
                args.Add("lawsuit", data.LawSuit);
                condition += " lawsuit=@lawsuit::jsonb, ";
            }
            if (!string.IsNullOrEmpty(data.CourtNotice))
            {
                args.Add("courtnotice", data.CourtNotice);
                condition += " courtnotice=@courtnotice::jsonb, ";
            }
            if (!string.IsNullOrEmpty(data.BreakPromise))
            {
                args.Add("breakpromise", data.BreakPromise);
                condition += " breakpromise=@breakpromise::jsonb, ";
            }

            DataBaseHelper.QuerySingle<OperateResult>(string.Format(executeSql, condition), args);
        }
        public List<BussinessInformation> GetBussinessInfomation(string selectField, int isLike, string companyName, int userNumber)
        {
            //ILIKE '%' || @keyword || '%' ESCAPE '`'
            var executeSql = @" select {0},recupdated::text from  crm_sys_bussiness_infomation where companyname {1} ";
            string condition = string.Empty;
            if (isLike == 0)
                condition = " ILIKE '%' || @keyword || '%' ESCAPE '`' ";
            else
                condition = " = @keyword";
            executeSql = string.Format(executeSql, selectField, condition);

            var args = new
            {
                keyword = companyName
            };

            return DataBaseHelper.Query<BussinessInformation>(executeSql, args);
        }
    }
}
