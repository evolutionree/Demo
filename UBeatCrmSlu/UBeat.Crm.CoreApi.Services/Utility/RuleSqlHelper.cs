using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Utility
{
    public class RuleSqlHelper
    {

        public static string FormatRuleSql(string sql, int userNumber,Guid deptid)
        {
            return sql.Replace("{UserNo}", userNumber.ToString())
                .Replace("{currentUser}", userNumber.ToString())
                .Replace("{UserDeptPeople}", string.Format("(SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid = '{0}')", deptid.ToString()))
                .Replace("{currentDepartment}", string.Format("(SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid = '{0}')", deptid.ToString()))
                .Replace("{UserDeptPeopleWithSub}", string.Format("(SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid IN (SELECT deptid FROM crm_func_department_tree( '{0}', 1)) )", deptid.ToString()))
                .Replace("{subDepartment}", string.Format("(SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid IN (SELECT deptid FROM crm_func_department_tree('{0}', 1)) )", deptid.ToString()))
            ;
            
        }

    }
}
