using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Repository.Repository;
using UBeat.Crm.CoreApi.Repository.Utility;
using System.Linq;
using System.Data;
using UBeat.Crm.CoreApi.DomainModel;

namespace UBeat.Crm.CoreApi.Desktop
{
    public class DesktopRepository : RepositoryBase, IDesktopRepository
    {
        public DesktopRepository(IConfigurationRoot config)
        {


        }

        #region config
        public OperateResult SaveDesktopComponent(DesktopComponentMapper mapper, IDbTransaction trans = null)
        {
            var sqlDel = @"delete from crm_sys_desktop_component where dscomponetid=@dscomponetid";
            var sql = @"insert into crm_sys_desktop_component (comname,comtype,comwidth, comheighttype,mincomheight,maxcomheight,comurl,comargs,comdesciption) values (@comname,@comtype,@comwidth, @comheighttype,@mincomheight,@maxcomheight,@comurl,@comargs,@comdesciption)";
            var param = new DynamicParameters();
            param.Add("dscomponetid", mapper.DsComponetId);
            param.Add("comname", mapper.ComName);
            param.Add("comtype", mapper.ComType);
            param.Add("comwidth", mapper.ComWidth);
            param.Add("comheighttype", mapper.ComHeightType);
            param.Add("mincomheight", mapper.MinComHeight);
            param.Add("maxcomheight", mapper.MaxComHeight);
            param.Add("comurl", mapper.ComUrl);
            param.Add("comargs", mapper.ComArgs);
            param.Add("comdesciption", mapper.ComDesciption);

            DataBaseHelper.ExecuteNonQuery(sqlDel, trans.Connection, trans, param);
            var result = DataBaseHelper.ExecuteNonQuery(sql, trans.Connection, trans, param);
            if (result > 0)
            {
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "保存成功"
                };
            }
            else
            {
                return new OperateResult
                {
                    Msg = "保存成功"
                };
            }
        }

        public OperateResult EnableDesktopComponent(DesktopComponentMapper mapper, IDbTransaction trans = null)
        {
            var sql = @"update crm_sys_desktop_component set status=@status where  dscomponetid=@dscomponetid;";
            var param = new DynamicParameters();
            param.Add("dscomponetid", mapper.DsComponetId);
            param.Add("status", mapper.Status);
            var result = DataBaseHelper.ExecuteNonQuery(sql, trans.Connection, trans, param);
            if (result > 0)
            {
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "保存成功"
                };
            }
            else
            {
                return new OperateResult
                {
                    Msg = "保存成功"
                };
            }
        }


        #endregion

        public DesktopMapper GetDesktop(int userId)
        {
            var sql = @"select * from crm_sys_desktop where desktopid in (select desktopid from crm_sys_desktop_relation where userid=@userid) limit 1;";
            var sqlLeft = @"select * from crm_sys_desktop_component where dscomponetid in (
            select   regexp_split_to_table::uuid as leftid  from  regexp_split_to_table((select leftitems from crm_sys_desktop where desktopid in (select desktopid from crm_sys_desktop_relation where userid=@userid) limit 1),','))";
            var sqlRight = @"select * from crm_sys_desktop_component where dscomponetid in (
            select   regexp_split_to_table::uuid as rightid  from  regexp_split_to_table((select rightitems from crm_sys_desktop where desktopid in (select desktopid from crm_sys_desktop_relation where userid=@userid) limit 1),','))";
            var param = new DynamicParameters();
            param.Add("userid", userId);
            var result = DataBaseHelper.QuerySingle<DesktopMapper>(sql, param);
            result.LeftDesktopComponents = DataBaseHelper.Query<DesktopComponentMapper>(sqlLeft, param);
            result.RightDesktopComponents = DataBaseHelper.Query<DesktopComponentMapper>(sqlRight, param);
            return result;
        }
    }
}
