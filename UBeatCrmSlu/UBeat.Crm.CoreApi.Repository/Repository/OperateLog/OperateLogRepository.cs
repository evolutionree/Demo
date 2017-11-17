using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.OpreateLog;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.OperateLog
{
    public class OperateLogRepository:IOperateLogRepository
    {
        public Dictionary<string, List<IDictionary<string, object>>> RecordList(PageParam pageParam, OperateLogRecordListMapper searchParam, int userNumber)
        {
            //这里提供两种方式
            var procName =
                "SELECT crm_func_operatelog_list(@deptId,@searchBegin,@searchEnd,@searchName,@pageIndex,@pageSize,@userNo)";

            var dataNames = new List<string> { "PageData", "PageCount" };
            var param = new
            {
                SearchName = searchParam.UserName,
                SearchBegin = searchParam.SearchBegin,
                SearchEnd = searchParam.SearchEnd,
                DeptId = searchParam.DeptId,
                PageIndex = pageParam.PageIndex,
                PageSize = pageParam.PageSize,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
    }
}
