using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Repository.Repository;
using UBeat.Crm.CoreApi.Repository.Utility;
using System.Linq;
namespace UBeat.Crm.CoreApi.Desktop
{
    public class DesktopRepository : RepositoryBase, IDesktopRepository
    {
        public DesktopRepository(IConfigurationRoot config)
        {


        }

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
