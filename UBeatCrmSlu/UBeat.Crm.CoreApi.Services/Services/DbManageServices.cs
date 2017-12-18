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
        public string  GenerateProcSQL(string procname, string param,int userid) {
            DbTransaction tran = null;
            Dictionary<string, object> item = _dbManageRepository.getProcInfo(procname, param, userid, tran);
            if (item == null) return null;
            string sql = (string)item["textsql"]+";";
            return sql;
        }
        public string GenerateTypeCreateSQL(string typename, int userid) {
            DbTransaction tran = null;
            Dictionary<string, object> tableInfo = _dbManageRepository.getTypeInfo(typename, userid, tran);
            if (tableInfo == null) return null;
            #region 字段信息
            List<Dictionary<string, object>> fieldsList = _dbManageRepository.getFieldList(typename, userid, tran);
            Dictionary<string, Dictionary<string, object>> fieldMap = new Dictionary<string, Dictionary<string, object>>();
            foreach (Dictionary<string, object> item in fieldsList)
            {
                int tmp = ((Int16)item["attnum"]);
                fieldMap.Add(tmp.ToString(), item);
            }
            string fieldSQL = _generateFieldsSQL(fieldsList);
            string createTableSQL = string.Format(@"
create type  ""public"".""{0}"" as (
{1});", typename, fieldSQL);
            #endregion
            
            return createTableSQL + "\r\n" ;
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
WITH (OIDS=FALSE);", tablename, fieldSQL);
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
                int index = src.IndexOf("nextval");
                if (index < 0) continue;
                src = src.Substring(index);
                if (src.StartsWith("nextval")) {
                    string seqname = src.Substring("nextval('".Length);
                    index = seqname.IndexOf("'::regclass)");
                    if (index < 0) continue;
                    seqname = seqname.Substring(0, index);
                   // seqname = seqname.Substring(0, seqname.Length - "'::regclass)".Length);
                    SeqSQL = SeqSQL + string.Format("CREATE SEQUENCE \"public\".\"{0}\";", seqname);
                    SeqSQL = SeqSQL + string.Format("ALTER SEQUENCE \"public\".\"{0}\" OWNER TO \"postgres\";", seqname);
                }
            }
            #endregion 

            #region _
            string ConstraintSQL = "";
            List<Dictionary<string, object>> constraintList = _dbManageRepository.getConstraints(tablename, userid, tran);
            if(constraintList != null)
            {
                foreach (Dictionary<string, object> item in constraintList)
                {
                    string tmp = _generateConstraintSQL(item, tablename);
                    if (tmp != null && tmp.Length > 0)
                    {
                        ConstraintSQL = ConstraintSQL + tmp + ";\r\n";
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
            Dictionary<string, string> needTriggerProc = new Dictionary<string, string>();
            List<Dictionary<string, object>> triggerList = _dbManageRepository.getTriggers(tablename, userid, tran);
            if (triggerList != null)
            {
                foreach (Dictionary<string, object> item in triggerList)
                {
                    TriggerSQL = TriggerSQL + (string)item["sqltext"] + ";\r\n";
                    if (item["proname"] != null && ((string)item["proname"]).Length > 0) {
                        string proname = (string)item["proname"];
                        if (needTriggerProc.ContainsKey(proname) == false) {
                            needTriggerProc.Add(proname, proname);
                        }
                    }
                }
                //检查触发器函数是否加在列表内
                foreach (string proname in needTriggerProc.Keys) {
                    if (this._dbManageRepository.checkHasPreProName(proname, userid, tran) == false) {
                        throw (new Exception("函数[" + proname + "]必须在定义列表内，请更新定义"));
                    }
                }
            }
            
            #endregion
            #region rules 
            #endregion
            return SeqSQL+"\r\n"+createTableSQL + "\r\n" + ConstraintSQL +"\r\n" + IndexSQL +"\r\n" + TriggerSQL  +"\r\n";
        }

        public OutputResult<object> SaveObjectSQL(DbSaveSQLParamInfo paramInfo, int userId)
        {
            DbTransaction tran = null;
            List<SQLTextModel> models = this._dbManageRepository.getSQLTextList(paramInfo.ObjId.ToString(), InitOrUpdate.Init, paramInfo.StructOrData, userId, tran);
            if (models == null || models.Count == 0)
            {
                //新增模式
                SQLTextModel model = new SQLTextModel();
                model.Id = Guid.NewGuid();
                model.initOrUpdate = InitOrUpdate.Init;
                model.isRun = 1;
                model.SqlObjId = paramInfo.ObjId;
                model.sqlOrJson = SqlOrJsonEnum.SQL;
                model.SqlText = paramInfo.SqlText;
                model.structOrData = paramInfo.StructOrData;
                this._dbManageRepository.saveSQLText(model, userId, tran);
            }
            else {
                //修改模式
                SQLTextModel model = models[0];
                model.SqlText = paramInfo.SqlText;
                this._dbManageRepository.saveSQLText(model, userId, tran);
            }
            return new OutputResult<object>("ok");
        }

        public string getObjectSQL(DbGetSQLParamInfo paramInfo, int userId)
        {
            DbTransaction tran = null;
            List<SQLTextModel> models  = this._dbManageRepository.getSQLTextList(paramInfo.ObjId.ToString(), InitOrUpdate.Init, paramInfo.StructOrData, userId, tran);
            if (models == null || models.Count == 0) return null;
            return models[0].SqlText;
        }

        public OutputResult<object> ListObjects(DbListObjectsParamInfo paramInfo, int userId)
        {
            DbTransaction tran = null;
            try
            {
                List<SQLObjectModel> list = this._dbManageRepository.SearchSQLObjects(paramInfo, userId, tran);
                foreach (SQLObjectModel item in list) {
                    if (item.ProcParam != null) {
                        item.ProcParam = item.ProcParam.Replace("{", "(").Replace("}", ")");
                        
                    }
                    if (item.ObjType == SQLObjectTypeEnum.Func) item.ObjType_Name = "函数";
                    else if (item.ObjType == SQLObjectTypeEnum.PGType) item.ObjType_Name = "类型";
                    else if (item.ObjType == SQLObjectTypeEnum.Table) item.ObjType_Name = "表格";
                    else item.ObjType_Name = "未设置";
                    if (item.NeedInitSQL == 1) item.NeedInitSQL_Name = "是"; else item.NeedInitSQL_Name = "";
                }
                return new OutputResult<object>(list);
            }
            catch (Exception ex) {
                return new OutputResult<object>(null, ex.Message, -1);
            }
            
        }
        /// <summary>
        /// 获取数据库的大小信息，包括pg和mongo
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DbSizeStatInfo StatDbSize(int userId)
        {
            DbTransaction tran = null;
            DbSizeStatInfo ret = new DbSizeStatInfo();
            Dictionary<string, object> pgState = this._dbManageRepository.getPostgreStatInfo(userId,tran);
            if (pgState != null) {
                ret.DbName = (string)pgState["databasename"];
                long tmp = 0;
                long.TryParse(pgState["dbsize"].ToString(), out tmp);
                ret.PgSize = tmp;
                ret.PgSizeName = ChangeSizeToHuman(tmp);
            }
            return ret; 
        }
        private string ChangeSizeToHuman(long size) {
            if (size <= 1000) return size.ToString() + "B";
            else if (size < 1024 * 1024) return (size * 1.0 / 1024.0).ToString("N") + "KB";
            else if (size < 1024 * 1024 * 1024) return (size * 1.0 / 1024.0 / 1024.0).ToString("N") + "MB";
            else if (size < (long)1024*(long)1024*(long)1024*(long)1024) return (size * 1.0 / 1024.0 / 1024.0/1024.0).ToString("N") + "GB";
            else return  (size * 1.0 / 1024.0 / 1024.0 / 1024.0/1024.0).ToString("N") + "TB";

        }

        public OutputResult<object> ListDir(int userId)
        {
            DbTransaction tran = null;
            List<string> paths = this._dbManageRepository.ListAllDirs(userId,tran);
            if (paths == null) return new OutputResult<object>(null, "没有找到数据", -1);
            Dictionary<string, Dictionary<string, object>> allItems = new Dictionary<string, Dictionary<string, object>>();
            Dictionary<string, object> rootItem = new Dictionary<string, object>();
            rootItem.Add("name", "/");
            rootItem.Add("fullpath", "/");
            rootItem.Add("subitems", new List<Dictionary<string, object>>());
            allItems.Add("/", rootItem);
            foreach (string s in paths) {
                if (allItems.ContainsKey(s)) continue;
                string[] ss = s.Split('/');
                Dictionary<string, object> tmp = rootItem;
                string fullpath = "";
                foreach (string i in ss) {
                    if (i == null || i.Length == 0) continue;
                    fullpath = fullpath + "/" + i;
                    bool isFound = false;
                    foreach (Dictionary<string, object> subItem in (List<Dictionary<string, object>>)tmp["subitems"]) {
                        string name = (string)subItem["name"];
                        if (name.Equals(i)) {
                            isFound = true;
                            tmp = subItem;
                            break;
                        }
                    }
                    if (isFound == false) {
                        Dictionary<string, object> subItem = new Dictionary<string, object>();
                        subItem.Add("fullpath", fullpath);
                        subItem.Add("name", i);
                        subItem.Add("subitems", new List<Dictionary<string, object>>());
                        allItems.Add(fullpath, subItem);
                        ((List<Dictionary<string, object>>)tmp["subitems"]).Add(subItem);
                    }
                }
            }
            List<object> retList = new List<object>();
            retList.Add(rootItem);
            return new OutputResult<object>(retList);

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

        public OutputResult<object> SaveObject(SQLObjectModel paramInfo, int userId,int isForBase)
        {
            DbTransaction tran = null;
            try
            {
                SQLObjectModel oldObj = null;
                if (isForBase == 1)
                {
                    oldObj = this._dbManageRepository.getSQLObjectInfo(paramInfo.Id.ToString(), userId, tran);
                    if (oldObj == null) {
                        throw (new Exception("无法找到原对象，无法更新"));
                    }
                    oldObj.SqlPath = paramInfo.SqlPath;
                    oldObj.NeedInitSQL = paramInfo.NeedInitSQL;
                    oldObj.Name = paramInfo.Name;
                }
                else
                {
                    oldObj = paramInfo;
                }
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
                List<SQLTextModel> typeList = this._dbManageRepository.ListInitSQLForType(paramInfo.ExportSys, paramInfo.IsStruct, userId, null);
                List<SQLTextModel> funcsList = this._dbManageRepository.ListInitSQLForFunc(paramInfo.ExportSys, paramInfo.IsStruct, userId, null);
                List<SQLTextModel> tablesList = this._dbManageRepository.ListInitSQLForTable(paramInfo.ExportSys, paramInfo.IsStruct, userId, null);
                StringBuilder sb = new StringBuilder();
                foreach (SQLTextModel item in typeList)
                {
                    if (item.SqlText != null && item.SqlText.Length > 0)
                    {
                        sb.AppendLine(item.SqlText);
                    }
                }
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
            if (recIds == null || recIds.Length == 0) {
                //获取所有
                List<SQLObjectModel> allObject = this._dbManageRepository.getSQLObjects("", userId, tran);
                List<string> l = new List<string>();
                foreach (SQLObjectModel item in allObject) {
                    if (item.NeedInitSQL == 1) {
                        l.Add(item.Id.ToString());
                    }
                }
                recIds = l.ToArray();
            }
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
                    sql = this.GenerateProcSQL(item.ObjName, item.ProcParam, userId);
                }
                else if (item.ObjType == SQLObjectTypeEnum.PGType) {
                    sql = this.GenerateTypeCreateSQL(item.ObjName, userId);
                }
                else
                {
                    continue;
                }
                if (sql == null || sql.Length == 0)
                {
                    continue;
                }
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
            else if (fieldtype == "uuid")
            {
                fieldTypeString = "  uuid  ";
            }
            else if (fieldtype == "-") {
                return null;
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
