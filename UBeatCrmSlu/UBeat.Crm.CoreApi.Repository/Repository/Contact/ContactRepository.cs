using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.DomainModel.Contact;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel;
using System.Data.Common;
using Npgsql;

namespace UBeat.Crm.CoreApi.Repository.Repository.Contact
{
    public class ContactRepository : RepositoryBase, IContactRepository
    {
        public ContactRepository(IConfigurationRoot config)
        {

        }
        public PageDataInfo<LinkManMapper> GetFlagLinkman(LinkManMapper paramInfo, int userId)
        {
            string sql = @"SELECT u.userid,u.username,u.usericon,u.remark,u.joineddate,u.birthday,u.userphone,u.userjob,u.usertel,u.usersex,u.workcode,u.useremail,r.deptid,d.deptname
			FROM crm_sys_flaglinkman f 
            inner join crm_sys_userinfo AS u on f.userid=u.userid
			LEFT JOIN crm_sys_account_userinfo_relate AS r ON u.userid = r.userid AND r.recstatus = 1
			LEFT JOIN crm_sys_account AS account ON account.accountid = r.accountid 
			LEFT JOIN crm_sys_department AS d ON d.deptid = r.deptid where  1=1 and f.recmanager=@userId ";

            if (string.IsNullOrEmpty(paramInfo.SearchKey))
            {
                sql += " order by f.reccreated desc ";
            }
            else
            {
                sql += " and u.username like '%" + paramInfo.SearchKey + "%' order by f.reccreated desc ";
            }

            if (paramInfo.PageSize <= 0)
            {
                paramInfo.PageSize = 1000000;
            }
            if (paramInfo.PageIndex <= 1)
            {
                paramInfo.PageIndex = paramInfo.PageIndex - 1;
            }

            var param = new DbParameter[]
            {
                new NpgsqlParameter("userId", userId)
            };
            PageDataInfo<LinkManMapper> pageData = new PageDataInfo<LinkManMapper>();

            return ExecuteQueryByPaging<LinkManMapper>(sql, param, paramInfo.PageSize, paramInfo.PageIndex);
        }

        public PageDataInfo<LinkManMapper> GetRecentCall(LinkManMapper paramInfo, int userId)
        {
            string sql = @"SELECT COALESCE(f.userid,0)::BOOLEAN as flag,u.userid,u.username,u.usericon,u.remark,u.joineddate,u.birthday,u.userphone,u.userjob,u.usertel,u.usersex,u.workcode,u.useremail,r.deptid,d.deptname
			FROM crm_sys_recentcall rc 
              inner join crm_sys_userinfo AS u on rc.userid=u.userid
              LEFT join crm_sys_flaglinkman f  on f.userid=rc.userid
			        LEFT JOIN crm_sys_account_userinfo_relate AS r ON u.userid = r.userid AND r.recstatus = 1
			        LEFT JOIN crm_sys_account AS account ON account.accountid = r.accountid 
			        LEFT JOIN crm_sys_department AS d ON d.deptid = r.deptid 
              where  1=1  and rc.recmanager=@userId ";

            if (string.IsNullOrEmpty(paramInfo.SearchKey))
            {
                sql += " order by rc.reccreated desc ";
            }
            else
            {
                sql += " and u.username like '%" + paramInfo.SearchKey + "%' order by rc.reccreated desc ";
            }

            if (paramInfo.PageSize <= 0)
            {
                paramInfo.PageSize = 1000000;
            }
            if (paramInfo.PageIndex <= 1)
            {
                paramInfo.PageIndex = paramInfo.PageIndex - 1;
            }

            var param = new DbParameter[]
            {
                new NpgsqlParameter("userId", userId)
            };
            PageDataInfo<LinkManMapper> pageData = new PageDataInfo<LinkManMapper>();

            return ExecuteQueryByPaging<LinkManMapper>(sql, param, paramInfo.PageSize, paramInfo.PageIndex);
        }

        public OperateResult FlagLinkman(LinkManMapper paramInfo, int userId)
        {
            string sql = "";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("userId", userId),
                new NpgsqlParameter("flaguser", paramInfo.userid)
            };
            if (paramInfo.flag)
            {
                //移除再插入
                sql = @"delete from crm_sys_flaglinkman where recmanager=@userId and userid=@flaguser;
                        INSERT INTO crm_sys_flaglinkman (recmanager, reccreated,userid) 
                        VALUES (@userId,now(),@flaguser)";
            }
            else
            {
                sql = "delete from crm_sys_flaglinkman where recmanager = @userId and userid = @flaguser";
            }
            var result = ExecuteNonQuery(sql, param);
            if (result >0)
            {
                return new OperateResult()
                {
                    Flag = 1,
                    Msg = "操作成功"
                };
            }
            else{
                return new OperateResult()
                {
                    Flag = 1,
                    Msg = "操作失败"
                };
            }
        }

        public OperateResult AddRecentCall(LinkManMapper paramInfo, int userId)
        {
            var param = new DbParameter[]
            {
                new NpgsqlParameter("userId", userId),
                new NpgsqlParameter("recentTar", paramInfo.userid)
            };

                //移除再插入
             string sql = @"delete from crm_sys_recentcall where recmanager=@userId and userid=@recentTar;
                        INSERT INTO crm_sys_recentcall (recmanager, reccreated,userid) 
                        VALUES (@userId,now(),@recentTar)";

            var result = ExecuteNonQuery(sql, param);
            if (result > 0)
            {
                return new OperateResult()
                {
                    Flag = 1,
                    Msg = "操作成功"
                };
            }
            else
            {
                return new OperateResult()
                {
                    Flag = 1,
                    Msg = "操作失败"
                };
            }
        }
    }
}
