using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.DbManage;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface  IDbManageRepository
    {
        /// <summary>
        /// 获取表的信息 
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        Dictionary<string, object> getTableInfo(string tablename, int userid,DbTransaction tran);
        /// <summary>
        /// 获取字段信息
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> getFieldList(string tablename, int userid, DbTransaction tran);
        List<Dictionary<string, object>> getConstraints(string tablename, int userid, DbTransaction tran);
        List<Dictionary<string, object>> getTriggers(string tablename, int userid, DbTransaction tran);
        List<Dictionary<string, object>> getIndexes(string tablename, int userid, DbTransaction tran);
        Dictionary<string, object> getProcInfo(string procname, string param, int userid, DbTransaction tran);
        List<SQLObjectModel> getSQLObjects(string searchKey, int userid, DbTransaction tran);
        SQLObjectModel querySQLObject(string id, int userid, DbTransaction tran);
        void deleteSQLObject(string id, int userid, DbTransaction tran);
        void saveSQLObject(SQLObjectModel model, int userid, DbTransaction tran);
        void saveSQLText(SQLTextModel model, int userid, DbTransaction tran);
        List<SQLTextModel> getSQLTextList(string objid, InitOrUpdate initorupdate, StructOrData structordata,int userid, DbTransaction tran);
        void deleteSQLTextByObjId(string objid, InitOrUpdate initorupdate, StructOrData structordata, int userid, DbTransaction tran);
        void deleteSQLText(string id, int userid, DbTransaction tran);
        List<SQLTextModel> ListInitSQLForFunc(SQLObjectBelongSysEnum exportSys, StructOrData isStruct, int userId,DbTransaction tran);
        List<SQLTextModel> ListInitSQLForTable(SQLObjectBelongSysEnum exportSys, StructOrData isStruct, int userId ,DbTransaction tran);
        bool checkHasPreProName(string proname, int userid, DbTransaction tran);
        List<SQLTextModel> ListInitSQLForType(SQLObjectBelongSysEnum exportSys, StructOrData isStruct, int userId, DbTransaction tran);
        Dictionary<string, object> getTypeInfo(string typename, int userid, DbTransaction tran);
    }
}
