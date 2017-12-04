using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.DbManage;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class DbManageServices:EntityBaseServices
    {
        private IDbManageRepository _dbManageRepository;
        public DbManageServices(IDbManageRepository dbManageRepository)  {
            this._dbManageRepository = dbManageRepository;
        }
        /// <summary>
        /// 创建函数、存储过程的脚本
        /// </summary>
        /// <param name="procname"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public string  GenerateProcSQL(string procname, int userid) {
            DbTransaction tran = null;
            Dictionary<string, object> item = _dbManageRepository.getProcInfo(procname, userid, tran);
            if (item == null) return null;
            string sql = (string)item["textsql"];
            return sql;
        }
        /// <summary>
        /// 生成创建table的脚本
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public string  GenerateTableCreateSQL(string tablename,int userid) {
            DbTransaction tran = null;
            Dictionary<string, object> tableInfo = _dbManageRepository.getTableInfo(tablename, userid, tran);
            if (tableInfo == null) return null;
            #region 字段信息
            List<Dictionary<string, object>> fieldsList = _dbManageRepository.getFieldList(tablename, userid, tran);
            Dictionary<string, Dictionary<string, object>> fieldMap = new Dictionary<string, Dictionary<string, object>>();
            foreach (Dictionary<string, object> item in fieldsList) {
                int tmp =( (Int16)item["attnum"]);
                fieldMap.Add(tmp.ToString(), item);
            }
            string fieldSQL = _generateFieldsSQL(fieldsList);
            string createTableSQL = string.Format(@"
                        create table ""public"".""{0}""(
                        {1})
                        WITH (OIDS=FALSE)", tablename, fieldSQL);
            #endregion

            #region 处理备注信息
            string CommentSQL = "";
            foreach (Dictionary<string, object> item in fieldsList) {
                string fieldname = (string)item["name"];
                string comment = (string)item["comment"];
                if (comment != null && comment.Length > 0) {
                    CommentSQL = CommentSQL + string.Format("COMMENT ON COLUMN \"public\".\"{0}\".\"{1}\" IS '{2}';", tablename, fieldname, comment);
                }
            }
            #endregion
            #region 处理序列信息
            string SeqSQL = "";
            foreach (Dictionary<string, object> item in fieldsList) {
                string src = (string)item["adsrc"];
                if (src == null || src.Length == 0) continue;
                if (src.StartsWith("nextval")) {
                    string seqname = src.Substring("nextval('".Length);
                    seqname = seqname.Substring(0, seqname.Length - "'::regclass)".Length);
                    SeqSQL = SeqSQL + string.Format("CREATE SEQUENCE \"public\".\"{0}\";", seqname);
                    SeqSQL = SeqSQL + string.Format("ALTER TABLE \"public\".\"{0}\" OWNER TO \"postgres\";", seqname);
                }
            }
            #endregion 

            #region 处理constraints
            string ConstraintSQL = "";
            List<Dictionary<string, object>> constraintList = _dbManageRepository.getConstraints(tablename, userid, tran);
            if(constraintList != null)
            {
                foreach (Dictionary<string, object> item in constraintList)
                {
                    string tmp = _generateConstraintSQL(item, tablename);
                    if (tmp != null && tmp.Length > 0)
                    {
                        ConstraintSQL = ConstraintSQL + tmp + "\r\n";
                    }
                }
            }
            
            #endregion
            #region 处理indexes
            string IndexSQL = "";
            List<Dictionary<string, object>> indexesList = _dbManageRepository.getIndexes(tablename, userid, tran);
            if (indexesList != null)
            {
                foreach (Dictionary<string, object> index in indexesList)
                {
                    IndexSQL = IndexSQL + (string)index["sqltext"] + ";\r\n";

                }
            }
            
            #endregion
            #region triggers 
            string TriggerSQL = "";
            List<Dictionary<string, object>> triggerList = _dbManageRepository.getTriggers(tablename, userid, tran);
            if (triggerList != null)
            {
                foreach (Dictionary<string, object> item in triggerList)
                {
                    TriggerSQL = TriggerSQL + (string)item["sqltext"] + ";\r\n";
                }
            }
            
            #endregion
            #region rules 
            #endregion
            return createTableSQL + "\r\n" + ConstraintSQL +"\r\n" + IndexSQL +"\r\n" + TriggerSQL  +"\r\n";
        }

        public OutputResult<object> SaveUpgradeSQL(SQLTextModel paramInfo, int userId)
        {
            try
            {
                this._dbManageRepository.saveSQLText(paramInfo, userId, null);
                return new OutputResult<object>("ok");
            }
            catch (Exception ex) {
                return new OutputResult<object>(ex.Message, ex.Message, -1);
            }

        }

        public OutputResult<object> SaveObject(SQLObjectModel paramInfo, int userId)
        {
            DbTransaction tran = null;
            try
            {
                this._dbManageRepository.saveSQLObject(paramInfo, userId, tran);
                return new OutputResult<object>("ok");
            }
            catch (Exception ex) {
                return new OutputResult<object>("", ex.Message, -1);
            }
        }

        public string ExportInitSQL(SQLExportParamInfo paramInfo, int userId)
        {
            try
            {
                List<SQLTextModel> funcsList = this._dbManageRepository.ListInitSQLForFunc(paramInfo.ExportSys, paramInfo.IsStruct, userId, null);
                List<SQLTextModel> tablesList = this._dbManageRepository.ListInitSQLForTable(paramInfo.ExportSys, paramInfo.IsStruct, userId, null);
                StringBuilder sb = new StringBuilder();
                foreach (SQLTextModel item in funcsList) {
                    if (item.SqlText != null && item.SqlText.Length >0 ) {
                        sb.AppendLine(item.SqlText);
                    }
                }
                foreach (SQLTextModel item in tablesList) {
                    if (item.SqlText != null && item.SqlText.Length > 0) {
                        sb.AppendLine(item.SqlText);
                    }
                }
                return sb.ToString();
            }
            catch (Exception ex) {
            }
            return "";
        }

        public OutputResult<object> ReflectInitStructSQL(string[] recIds, int userId)
        {
            DbTransaction tran = null;
            foreach (string id in recIds) {
                SQLObjectModel item = this._dbManageRepository.querySQLObject(id, userId, tran);
                if (item == null) continue;
                this._dbManageRepository.deleteSQLTextByObjId(id, InitOrUpdate.Init, StructOrData.Struct, userId, tran);//删除没拥有的
                SQLTextModel newSQL = new SQLTextModel();
                string sql = "";
                if (item.ObjType == SQLObjectTypeEnum.Table)
                {
                    sql = this.GenerateTableCreateSQL(item.ObjName, userId);
                }
                else if (item.ObjType == SQLObjectTypeEnum.Func)
                {
                    sql = this.GenerateProcSQL(item.ObjName, userId);
                }
                else {
                    continue;
                }
                if (sql == null || sql.Length == 0) continue;
                newSQL.initOrUpdate = InitOrUpdate.Init;
                newSQL.structOrData = StructOrData.Struct;
                newSQL.sqlOrJson = SqlOrJsonEnum.SQL;
                newSQL.SqlObjId = item.Id;
                newSQL.SqlText = sql;
                newSQL.isRun = 0;
                newSQL.Version = "0";
                this._dbManageRepository.saveSQLText(newSQL, userId, tran);
            }
            return new OutputResult<object>("ok");
        }

        private string _generateConstraintSQL(Dictionary<string, object> constraint,string tableName) {
            string definedtext = (string)constraint["definedtext"];
            return string.Format("alter table \"public\".\"{0}\" add {1} ;", tableName, definedtext);
        }
        /// <summary>
        /// 生成CreateTable内部语句
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        private string _generateFieldsSQL(List<Dictionary<string, object>> fields) {
            string retSQL = "";
            foreach (Dictionary<string, object> fieldInfo  in fields ) {
                string tmp = _generateOneFieldSQL(fieldInfo);
                if (tmp != null && tmp.Length >0)
                {
                    retSQL = retSQL + tmp + ",";
                }
            }
            if (retSQL.Length > 0) {
                retSQL = retSQL.Substring(0, retSQL.Length - 1);
            }
            return retSQL;
        }
        /// <summary>
        /// 生成CreateTable中的一个字段的语句
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        private string _generateOneFieldSQL(Dictionary<string, object> fieldInfo) {
            string fieldtype = (string)fieldInfo["type"];
            string fieldTypeString = "";
            if (fieldtype == "bigint")
            {
                fieldTypeString = " int8 ";
            }
            else if (fieldtype == "boolean")
            {
                fieldTypeString = " bool ";
            }
            else if (fieldtype.StartsWith("character varying"))
            {
                string tmp = fieldtype;
                tmp = tmp.Replace("character varying", "varchar");
                fieldTypeString = " " + tmp + " COLLATE \"default\" ";
            }
            else if (fieldtype == "date")
            {
                fieldTypeString = " date ";
            }
            else if (fieldtype == "integer")
            {
                int len = 0;
                int.TryParse(fieldInfo["attlen"].ToString(), out len);
                fieldTypeString = string.Format(" int{0} ", len);
            }
            else if (fieldtype == "json")
            {
                fieldTypeString = "  json ";
            }
            else if (fieldtype == "jsonb")
            {
                fieldTypeString = "  jsonb ";
            }
            else if (fieldtype.StartsWith("numeric"))
            {
                fieldTypeString = " " + fieldtype + " ";
            }
            else if (fieldtype == "smallint")
            {
                fieldTypeString = "  smallint ";
            }
            else if (fieldtype == "text")
            {
                fieldTypeString = "  text  COLLATE \"default\" ";
            }
            else if (fieldtype.StartsWith("timestamp"))
            {
                fieldTypeString = " timestamp(8) ";
            }
            else if (fieldtype == "uuid") {
                fieldTypeString = "  uuid  "; 
            }
            else
            {
                throw (new Exception("未支持的数据类型"));
            }
            string retStr = "";
            retStr = "\"" + (string)fieldInfo["name"] + "\" ";
            retStr = retStr + fieldTypeString;
            string defaultValue =(string) fieldInfo["adsrc"];
            if (!(defaultValue == null || defaultValue.Length == 0)) {
                retStr = retStr + " DEFAULT  " + defaultValue + "  ";
            }
            bool isNotNull = (bool)fieldInfo["notnull"];
            if (isNotNull) {
                retStr = retStr + " not null ";
            }
            return retStr;

        }

        
    }
}
