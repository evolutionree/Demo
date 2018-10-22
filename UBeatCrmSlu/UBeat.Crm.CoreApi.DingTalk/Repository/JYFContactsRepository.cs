using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DingTalk.Models;
using UBeat.Crm.CoreApi.Repository.Repository;

namespace UBeat.Crm.CoreApi.DingTalk.Repository
{
    public class JYFContactsRepository : RepositoryBase, IJYFContactsRepository
    {
        public List<Dictionary<string, object>> GetAllCompanys(Dictionary<string, string>.KeyCollection.Enumerator enumerator)
        {
            string ids = "";
            while (enumerator.MoveNext())
            {
                string id = enumerator.Current;
                ids = ids + ",'" + id + "'";
            }
            if (ids.Length > 0) ids = ids.Substring(1);
            if (ids.Length == 0) return new List<Dictionary<string, object>>();
            string strSQL = @"select distinct b.recid,b.recname 
	                            from crm_jyf_contacts_position a  
			                            inner join crm_sys_customer b on a.customer->>'id' = b.recid::text
                            where  a.contact ->>'id' in  (" + ids + ")";
            return ExecuteQuery(strSQL, new System.Data.Common.DbParameter[] { });

        }

        public List<ContactPositionInfo> getContactsByCompanys(string companyids)
        {
            string strSQL = @"select  (a.contact ->>'id')::uuid contactid,
			                            (a.customer->>'id')::uuid custid,
			                            a.positionstart jobstart,
			                            a.positionstop jobend
                            from crm_jyf_contacts_position a
                            where (a.customer->>'id') in  (" + companyids + ")";
            return ExecuteQuery<ContactPositionInfo>(strSQL, new System.Data.Common.DbParameter[] { });
        }

        public Dictionary<Guid, string> GetContactsName(Dictionary<string, string>.KeyCollection keys)
        {
            string ids = "";
            foreach (string id in keys)
            {
                ids = ids + ",'" + id + "'";
            }
            if (ids.Length == 0) return new Dictionary<Guid, string>();
            ids = ids.Substring(1);
            string strSQL = "select recid ,recname from crm_jyf_contacts where recid::text in(" + ids + ")";
            List<Dictionary<string, object>> tmpList = ExecuteQuery(strSQL, new System.Data.Common.DbParameter[] { });
            Dictionary<Guid, string> retDict = new Dictionary<Guid, string>();
            foreach (Dictionary<string, object> item in tmpList)
            {
                Guid recid = Guid.Parse(item["recid"].ToString());
                retDict.Add(recid, item["recname"].ToString());
            }
            return retDict;
        }


        public Dictionary<string, object> GetDeptInfo(Guid parentId, string deptname)
        {
            try
            {
                string strSQL = "Select * from crm_sys_department where parentid = @parentid and deptname = @deptname ";
                DbParameter[] p = new DbParameter[]{
                    new Npgsql.NpgsqlParameter("@parentid",parentId),
                };


            }
            catch (Exception ex)
            {


            }
            return new Dictionary<string, object>();

        }

        public List<ContactsRelationInfo> GetOtherContactRelations(Dictionary<string, string>.KeyCollection.Enumerator enumerator)
        {
            string ids = "";
            while (enumerator.MoveNext())
            {
                string id = enumerator.Current;
                ids = ids + ",'" + id + "'";
            }
            if (ids.Length == 0) return new List<ContactsRelationInfo>();
            ids = ids.Substring(1);
            string strSQL = @"select (contact1->>'id')::uuid contact1,(contact2->>'id')::uuid contact2,contactsrelation relationtype
                                from crm_jyf_contactsrelation 
                                where contact1->>'id' in (" + ids + ") or contact2->>'id' in (" + ids + ")";
            List<ContactsRelationInfo> retList = ExecuteQuery<ContactsRelationInfo>(strSQL, new System.Data.Common.DbParameter[] { });
            List<ContactsRelationInfo> realRetList = new List<ContactsRelationInfo>();
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (ContactsRelationInfo item in retList)
            {
                if (item.Contact1.CompareTo(item.Contact2) < 0)
                {
                    Guid tmpGuid = item.Contact2;
                    item.Contact2 = item.Contact1;
                    item.Contact1 = tmpGuid;
                }
                item.Id = item.Contact1.ToString() + ":" + item.Contact2.ToString();
                if (!dict.ContainsKey(item.Id))
                {
                    dict.Add(item.Id, item.Id);
                    realRetList.Add(item);
                }
            }
            return realRetList;
        }
    }
}
