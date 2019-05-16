using Dapper;
using Npgsql;
using System;
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
                SELECT * FROM crm_func_attendance_add(@signimg,@locations,@signtype,@signmark,@signTime,@cardtype,@userNo)
            ";

            //pwd salt security

            var param = new
            {
                SignImg = signEntity.SignImg,
                Locations = JsonHelper.ToJson(signEntity.Locations),
                SignType = signEntity.SignType,
                SignMark = signEntity.SignMark,
                SignTime = signEntity.SignTime,
                UserNo = userNumber,
                CardType = signEntity.CardType
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        /// <summary>
        /// 手工加入打开记录
        /// </summary>
        public OperateResult Add(AttendanceAddMapper signEntity, int userNumber)
        {

            string sql = sql = @"INSERT INTO crm_sys_attendance(attendid,cardtype,signtype, signmark, udeptcode, reccreated, reccreator, recupdator,recordsource)  
                VALUES (@attendid,@cardtype,@signtype,@signmark,(SELECT ucode FROM crm_func_userinfo_udeptcode(@reccreator)),@reccreated,@reccreator,@recupdator,@recordsource)";

            Guid attendid = new Guid();
            var args = new
            {
                attendid= attendid,
                cardtype =signEntity.CardType,
                signtype = signEntity.SignType,
                signmark = signEntity.SignMark,
                reccreated = signEntity.SignTime,
                reccreator = signEntity.SelectUser,
                recupdator = userNumber,
                recordsource = signEntity.RecordSource
            };
            var result = DataBaseHelper.ExecuteNonQuery(sql, args, CommandType.Text);
            if (result == 1)
            {
                return new OperateResult()
                {
                    Id= attendid.ToString(),
                    Flag = 1,
                    Msg = "新增成功"
                };
            }
            else
            {
                return new OperateResult()
                {
                    Flag = 0,
                    Msg = "新增失败"
                };
            }
        }


        /// <summary>
        /// 检查是否存在该用户的班组绑定
        /// </summary>
        /// <typeparam name="selectUser">查询的用户</typeparam>
        public int ExistGroupUser(string selectUser) {
            var existSql = @"select count(1) from crm_sys_attendance_setting where person=@selectuser ";
            var param = new
            {
                selectuser = selectUser
            };
            var existCount = DataBaseHelper.QuerySingle<int>(existSql, param, CommandType.Text);
            return existCount;
        }

        public Dictionary<string, List<IDictionary<string, object>>> GroupUserQuery(GroupUserMapper groupUser, int userNumber)
        {
            var procName =
                "SELECT crm_func_schedule_user_list(@deptid,@username,@pageindex,@pagesize,@userno)";

            var dataNames = new List<string> { "Page", "PageCount" };
            var param = new DynamicParameters();
            param.Add("deptid", groupUser.DeptId);
            param.Add("username", groupUser.UserName);
            param.Add("pageindex", groupUser.PageIndex);
            param.Add("pagesize", groupUser.PageSize);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
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
