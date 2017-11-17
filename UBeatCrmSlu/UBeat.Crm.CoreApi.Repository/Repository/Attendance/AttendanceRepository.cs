using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Attendance;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.Attendance
{
    public class AttendanceRepository : RepositoryBase, IAttendanceRepository
    {
        public OperateResult Sign(AttendanceSignMapper signEntity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_attendance_add(@signimg,@locations,@signtype,@signmark,@signTime,@userNo)
            ";

            //pwd salt security

            var param = new
            {
                SignImg = signEntity.SignImg,
                Locations = JsonHelper.ToJson(signEntity.Locations),
                SignType = signEntity.SignType,
                SignMark = signEntity.SignMark,
                SignTime = signEntity.SignTime,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public Dictionary<string, List<Dictionary<string, object>>> SignList(PageParam pageParam, AttendanceSignListMapper searchParm, int userNumber)
        {
            var procName =
              "SELECT crm_func_attendance_list(@monthType,@listtype,@searchName,@startdate,@enddate,@type,@deptid,@pageIndex,@pageSize,@userNo)";

            var dataNames = new List<string> { "PageData", "PageCount" };

            //var param = new
            //{
            //    MonthType = searchParm.MonthType,
            //    ListType = searchParm.ListType,
            //    SearchName = searchParm.SearchName,
            //    StartDate = searchParm.StartDate,
            //    EndDate = searchParm.EndDate,
            //    Type = searchParm.Type,
            //    DeptId = searchParm.DeptId,
            //    PageIndex = pageParam.PageIndex,
            //    PageSize = pageParam.PageSize,
            //    UserNo = userNumber
            //};

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("MonthType", searchParm.MonthType));
            sqlParameters.Add(new NpgsqlParameter("ListType", searchParm.ListType));
            sqlParameters.Add(new NpgsqlParameter("SearchName", searchParm.SearchName));
            sqlParameters.Add(new NpgsqlParameter("StartDate", (searchParm.ListType == 0|| searchParm.ListType == 1) ? string.Empty : searchParm.StartDate));
            sqlParameters.Add(new NpgsqlParameter("EndDate", (searchParm.ListType == 0 || searchParm.ListType == 1) ? string.Empty : searchParm.EndDate));
            sqlParameters.Add(new NpgsqlParameter("Type", searchParm.Type));
            sqlParameters.Add(new NpgsqlParameter("DeptId", searchParm.DeptId));
            sqlParameters.Add(new NpgsqlParameter("PageIndex", pageParam.PageIndex));
            sqlParameters.Add(new NpgsqlParameter("PageSize", pageParam.PageSize));
            sqlParameters.Add(new NpgsqlParameter("UserNo", userNumber));

            var result = DBHelper.ExecuteQueryRefCursor("", procName, sqlParameters.ToArray());
            //var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
    }
}
