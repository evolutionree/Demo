using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Department;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class DeptPermissionServices:EntityBaseServices
    {
        private IDeptPermissionRepository _deptPermissionRepository;
        public DeptPermissionServices(IDeptPermissionRepository deptPermissionRepository)  {
            this._deptPermissionRepository = deptPermissionRepository;
        }
        public List<DeptPermissionSchemeEntryInfo> ListPermissionDetailByRoleId(Guid schemeId, Guid roleid, int userNum) {
            List<DeptPermissionSchemeEntryInfo> tmpResult = _deptPermissionRepository.ListPermissionDetailByRoleId(schemeId, roleid, userNum, null);
            if (tmpResult == null) return null;
            #region 树状处理
            Dictionary<Guid, DeptPermissionSchemeEntryInfo> dictResult = new Dictionary<Guid, DeptPermissionSchemeEntryInfo>();
            foreach (DeptPermissionSchemeEntryInfo item in tmpResult) {
                if (item.RecId == null || item.RecId.Equals(Guid.Empty))
                {
                    item.RecId = Guid.NewGuid();
                    item.SchemeId = schemeId;
                    item.Authorized_RoleId = roleid;
                    item.Authorized_Type = DeptPermissionObjectTypeEnum.Role;
                    item.PermissionType = DeptPermissionAuthTypeEnum.NotDefined;
                    item.SubDeptPermission = DeptPermissionSubPolicyEnum.Auto_Gain;
                    item.SubUserPermission = DeptPermissionSubPolicyEnum.Auto_Gain;
                }
                if (item.PMObject_Type == DeptPermissionObjectTypeEnum.Department) {//只需要加入部门的就可以了，其他不用处理
                    if (item.PMObject_DeptId != null && item.PMObject_DeptId.Equals(Guid.Empty) == false) {
                        if (dictResult.ContainsKey(item.PMObject_DeptId) == false) {
                            dictResult.Add(item.PMObject_DeptId, item);
                        }
                    }
                }
            }
            List<DeptPermissionSchemeEntryInfo> rootList = new List<DeptPermissionSchemeEntryInfo>();
            foreach (DeptPermissionSchemeEntryInfo item in tmpResult) {
                if (item.ParentId != null&&!item.ParentId.Equals(Guid.Empty)) {
                    if (dictResult.ContainsKey(item.ParentId))
                    {
                        DeptPermissionSchemeEntryInfo pItem = dictResult[item.ParentId];
                        if (pItem.SubDepts == null) pItem.SubDepts = new List<DeptPermissionSchemeEntryInfo>();
                        pItem.SubDepts.Add(item);
                    }
                }
                else
                {
                    rootList.Add( item);
                }
            }
            List<DeptPermissionSchemeEntryInfo> result = new List<DeptPermissionSchemeEntryInfo>();
            ChangeList2List(rootList, result);
            #endregion
            return result;
        }

        public List<DeptPermissionSchemeEntryInfo> ListPermissionDetailByUserId(Guid schemeId, int userId, int userNum)
        {
            List<DeptPermissionSchemeEntryInfo> tmpResult = _deptPermissionRepository.ListPermissionDetailByUserId(schemeId, userId, userNum, null);
            if (tmpResult == null) return null;
            #region 树状处理
            Dictionary<Guid, DeptPermissionSchemeEntryInfo> dictResult = new Dictionary<Guid, DeptPermissionSchemeEntryInfo>();
            foreach (DeptPermissionSchemeEntryInfo item in tmpResult)
            {
                if (item.RecId == null || item.RecId.Equals(Guid.Empty))
                {
                    item.RecId = Guid.NewGuid();
                    item.SchemeId = schemeId;
                    item.Authorized_UserId = userId;
                    item.Authorized_Type = DeptPermissionObjectTypeEnum.User;
                    item.PermissionType = DeptPermissionAuthTypeEnum.NotDefined;
                    item.SubDeptPermission = DeptPermissionSubPolicyEnum.Auto_Gain;
                    item.SubUserPermission = DeptPermissionSubPolicyEnum.Auto_Gain;
                }
                if (item.PMObject_Type == DeptPermissionObjectTypeEnum.Department)
                {//只需要加入部门的就可以了，其他不用处理
                    if (item.PMObject_DeptId != null && item.PMObject_DeptId.Equals(Guid.Empty) == false)
                    {
                        if (dictResult.ContainsKey(item.PMObject_DeptId) == false)
                        {
                            dictResult.Add(item.PMObject_DeptId, item);
                        }
                    }
                }
            }
            List<DeptPermissionSchemeEntryInfo> rootList = new List<DeptPermissionSchemeEntryInfo>();
            foreach (DeptPermissionSchemeEntryInfo item in tmpResult)
            {
                if (item.ParentId != null && !item.ParentId.Equals(Guid.Empty))
                {
                    if (dictResult.ContainsKey(item.ParentId))
                    {
                        DeptPermissionSchemeEntryInfo pItem = dictResult[item.ParentId];
                        if (pItem.SubDepts == null) pItem.SubDepts = new List<DeptPermissionSchemeEntryInfo>();
                        pItem.SubDepts.Add(item);
                    }
                }
                else
                {
                    rootList.Add(item);
                }
            }
            List<DeptPermissionSchemeEntryInfo> result = new List<DeptPermissionSchemeEntryInfo>();
            ChangeList2List(rootList, result);
            #endregion
            return result;
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="schemeId"></param>
       /// <param name="searchResultUserId"></param>
       /// <param name="optimize"></param>
       /// <param name="isCombined"></param>
       /// <param name="userId"></param>
       /// <returns></returns>
        public List<DeptPermissionSchemeEntryInfo> FetchOrgPermissionByUser(Guid schemeId, int searchResultUserId, int optimize, int isCombined, int userId)
        {
            List<DeptPermissionSchemeEntryInfo> tmpResult = this._deptPermissionRepository.ListPermissionDetailByUserId(schemeId, searchResultUserId, userId, null);
            if (tmpResult == null) tmpResult = new List<DeptPermissionSchemeEntryInfo>();
            Dictionary<Guid, DeptPermissionSchemeEntryInfo> dictResult = new Dictionary<Guid, DeptPermissionSchemeEntryInfo>();
            foreach (DeptPermissionSchemeEntryInfo item in tmpResult)
            {
                if (item.PMObject_Type == DeptPermissionObjectTypeEnum.Department)
                {
                    dictResult.Add(item.PMObject_DeptId, item);
                }
            }
            List<DeptPermissionSchemeEntryInfo> rootList = new List<DeptPermissionSchemeEntryInfo>();
            Dictionary<Guid, Guid> rejectDepts = new Dictionary<Guid, Guid>();
            Dictionary<int, int> rejectUsers = new Dictionary<int, int>();
            foreach (DeptPermissionSchemeEntryInfo item in tmpResult)
            {
                if (item.ParentId != null && item.ParentId.Equals(Guid.Empty) == false)
                {
                    if (dictResult.ContainsKey(item.ParentId))
                    {
                        DeptPermissionSchemeEntryInfo pItem = dictResult[item.ParentId];
                        if (pItem.SubDepts == null) pItem.SubDepts = new List<DeptPermissionSchemeEntryInfo>();
                        pItem.SubDepts.Add(item);
                    }
                }
                else
                {
                    rootList.Add(item);
                }
                if (item.PermissionType == DeptPermissionAuthTypeEnum.Reject)
                {
                    if (item.PMObject_Type == DeptPermissionObjectTypeEnum.Department)
                    {
                        if (rejectDepts.ContainsKey(item.PMObject_DeptId) == false)
                        {
                            rejectDepts.Add(item.PMObject_DeptId, item.PMObject_DeptId);
                        }
                    }
                    else if (item.PMObject_Type == DeptPermissionObjectTypeEnum.User)
                    {
                        rejectUsers.Add(item.PMObject_UserId, item.PMObject_UserId);
                    }
                }
            }
            ConstructDeptPermissionTree(rootList, DeptPermissionSubPolicyEnum.Auto_NoGain, DeptPermissionSubPolicyEnum.Auto_NoGain);
            if (isCombined == 1)
            {
                //需要获取用户归属的角色，并且把角色的权限处理完毕
                List<Guid> roles = new List<Guid>();
                roles = this._deptPermissionRepository.GetRolesByUser(null, searchResultUserId, userId);
                foreach(Guid roleid in roles)
                {
                    List<DeptPermissionSchemeEntryInfo> rolePermission = this.FetchOrgPermissionByRole(schemeId, roleid, 0, userId,rejectDepts,rejectUsers);
                    CombineTreeList(ref rolePermission, ref rootList);
                }
                RejectPermissionTree(ref rootList, rejectDepts, rejectUsers);

            }
            if (optimize == 1)//如果采用优化模式，则提升只有一个儿子的节点
                rootList = OptimizePermissionTree(rootList);
            List<DeptPermissionSchemeEntryInfo> returnList = new List<DeptPermissionSchemeEntryInfo>();
            ChangeList2List(rootList, returnList);
            return returnList;
        }
        /// <summary>
        /// 合并两个树形列表
        /// </summary>
        /// <param name="srcList"></param>
        /// <param name="dstList"></param>
        private void CombineTreeList(ref List<DeptPermissionSchemeEntryInfo> srcList,ref  List<DeptPermissionSchemeEntryInfo> dstList)
        {
            if (srcList == null || dstList == null) return;
            List<DeptPermissionSchemeEntryInfo> needAdd = new List<DeptPermissionSchemeEntryInfo>();
            foreach(DeptPermissionSchemeEntryInfo srcItem in srcList)
            {
                bool isFound = false;
                DeptPermissionSchemeEntryInfo matchItem = null;
                foreach (DeptPermissionSchemeEntryInfo dstItem in dstList) {
                    if (dstItem.PMObject_Type == srcItem.PMObject_Type) {
                        if (dstItem.PMObject_Type == DeptPermissionObjectTypeEnum.Department
                            && dstItem.PMObject_DeptId == srcItem.PMObject_DeptId)
                        {
                            isFound = true;
                        }else if(dstItem.PMObject_Type == DeptPermissionObjectTypeEnum.User
                            && dstItem.PMObject_UserId == srcItem.PMObject_UserId)
                        {
                            isFound = true;
                        }
                        if (isFound)
                        {
                            matchItem = dstItem;
                            break;
                        }
                    }
                }
                if (isFound)
                {
                    if (srcItem.PMObject_Type == DeptPermissionObjectTypeEnum.Department)
                    {
                        //只有部门才需要增加进入下一步处理
                        if (srcItem.SubDepts == null) srcItem.SubDepts = new List<DeptPermissionSchemeEntryInfo>();
                        if (matchItem.SubDepts == null) matchItem.SubDepts = new List<DeptPermissionSchemeEntryInfo>();
                        CombineTreeList(ref srcItem.SubDepts, ref matchItem.SubDepts);
                    }
                }
                else
                {
                    needAdd.Add(srcItem);
                }
            }
            foreach(DeptPermissionSchemeEntryInfo item in needAdd)
            {
                dstList.Add(item);
            }
        }

        /// <summary>
        /// 全局拒绝权限
        /// </summary>
        /// <param name="srcList"></param>
        /// <param name="rejectDepts"></param>
        /// <param name="rejectUsers"></param>
        private void RejectPermissionTree(ref List<DeptPermissionSchemeEntryInfo> srcList, Dictionary<Guid, Guid> rejectDepts, Dictionary<int, int> rejectUsers) {
            List<DeptPermissionSchemeEntryInfo> needRemove = new List<DeptPermissionSchemeEntryInfo>();
            foreach (DeptPermissionSchemeEntryInfo item in srcList) {
                if (item.PMObject_Type == DeptPermissionObjectTypeEnum.Department) {
                    if (rejectDepts.ContainsKey(item.PMObject_DeptId)) {
                        needRemove.Add(item);
                    }
                }else if (item.PMObject_Type == DeptPermissionObjectTypeEnum.User)
                {
                    if (rejectUsers.ContainsKey(item.PMObject_UserId)) {
                        needRemove.Add(item);
                    }
                }
            }
            foreach (DeptPermissionSchemeEntryInfo item in needRemove) {
                srcList.Remove(item);
            }
        }
        public List<DeptPermissionSchemeEntryInfo> FetchOrgPermissionByRole(Guid schemeId, Guid roleId, int optimize, int userId) {
            return FetchOrgPermissionByRole(schemeId, roleId, optimize, userId, null, null);
        }
        /// <summary>
        /// 根据角色，获取实际的授权情况(排除未授权、未设置或者已拒绝）
        /// </summary>
        /// <param name="schemeId"></param>
        /// <param name="roleId"></param>
        /// <param name="optimize"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private  List<DeptPermissionSchemeEntryInfo> FetchOrgPermissionByRole(Guid schemeId, Guid roleId, int optimize, int userId, Dictionary<Guid, Guid> rejectDepts, Dictionary<int, int> rejectUsers)
        {
            List<DeptPermissionSchemeEntryInfo> tmpResult = this._deptPermissionRepository.ListPermissionDetailByRoleId(schemeId, roleId, userId, null);
            if (tmpResult == null) tmpResult = new List<DeptPermissionSchemeEntryInfo>();
            #region 处理树状
            Dictionary<Guid, DeptPermissionSchemeEntryInfo> dictResult = new Dictionary<Guid, DeptPermissionSchemeEntryInfo>();
            foreach (DeptPermissionSchemeEntryInfo item in tmpResult) {
                if (item.PMObject_Type == DeptPermissionObjectTypeEnum.Department) {
                    dictResult.Add(item.PMObject_DeptId, item);
                }
            }
            List<DeptPermissionSchemeEntryInfo> rootList = new List<DeptPermissionSchemeEntryInfo>();
            if (rejectDepts == null ) rejectDepts = new Dictionary<Guid, Guid>();
            if (rejectUsers == null) rejectUsers = new Dictionary<int, int>();
            foreach (DeptPermissionSchemeEntryInfo item in tmpResult) {
                if (item.ParentId != null && item.ParentId.Equals(Guid.Empty) == false)
                {
                    if (dictResult.ContainsKey(item.ParentId)) {
                        DeptPermissionSchemeEntryInfo pItem = dictResult[item.ParentId];
                        if (pItem.SubDepts == null) pItem.SubDepts = new List<DeptPermissionSchemeEntryInfo>();
                        pItem.SubDepts.Add(item);
                    }
                }
                else {
                    rootList.Add(item);
                }
                if (item.PermissionType == DeptPermissionAuthTypeEnum.Reject) {
                    if (item.PMObject_Type == DeptPermissionObjectTypeEnum.Department) {
                        if(rejectDepts.ContainsKey(item.PMObject_DeptId) ==false) {
                            rejectDepts.Add(item.PMObject_DeptId, item.PMObject_DeptId);
                        }
                    }else if (item.PMObject_Type == DeptPermissionObjectTypeEnum.User)
                    {
                        rejectUsers.Add(item.PMObject_UserId, item.PMObject_UserId);
                    }
                }
            }
            ConstructDeptPermissionTree(rootList, DeptPermissionSubPolicyEnum.Auto_NoGain, DeptPermissionSubPolicyEnum.Auto_NoGain);
            if (optimize == 1)//如果采用优化模式，则提升只有一个儿子的节点
                rootList = OptimizePermissionTree(rootList);
            List<DeptPermissionSchemeEntryInfo> returnList = new List<DeptPermissionSchemeEntryInfo>();
            ChangeList2List(rootList, returnList);
            #endregion
            return returnList;
        }
        /// <summary>
        /// 处理树状节点，删除没有权限的节点，自动匹配没有配置的节点
        /// </summary>
        /// <param name="list"></param>
        /// <param name="subFolderPolicy"></param>
        /// <param name="userPolicy"></param>
        private void ConstructDeptPermissionTree(List<DeptPermissionSchemeEntryInfo> list, DeptPermissionSubPolicyEnum subFolderPolicy, DeptPermissionSubPolicyEnum userPolicy) {
            List<DeptPermissionSchemeEntryInfo> needRemove = new List<DeptPermissionSchemeEntryInfo>();
            foreach (DeptPermissionSchemeEntryInfo item in list) {
                bool isNeedRemove = false;
                if (item.PermissionType == DeptPermissionAuthTypeEnum.Reject)
                {
                    needRemove.Add(item);
                    isNeedRemove = true;
                }
                else if (item.PermissionType == DeptPermissionAuthTypeEnum.NotAuthed) {
                    needRemove.Add(item);
                    isNeedRemove = true;
                }
                else if (item.PermissionType == DeptPermissionAuthTypeEnum.NotDefined)
                {
                    //如果是没有定义，则需要根据类型，获取当前的默认权限处理规则
                    if (item.PMObject_Type == DeptPermissionObjectTypeEnum.Department)
                    {
                        if (subFolderPolicy != DeptPermissionSubPolicyEnum.Auto_Gain)
                        {
                            needRemove.Add(item);
                            isNeedRemove = true;
                        }
                    }
                    else {
                        if (userPolicy != DeptPermissionSubPolicyEnum.Auto_Gain) {
                            needRemove.Add(item);
                            isNeedRemove = true;
                        }
                    }
                }
                if (isNeedRemove == false)
                {
                    //尝试处理下级
                    if (item.SubDepts != null && item.SubDepts.Count > 0)
                    {
                        ConstructDeptPermissionTree(item.SubDepts, item.SubDeptPermission, item.SubUserPermission);
                    }
                }
            }
            foreach (DeptPermissionSchemeEntryInfo item in needRemove) {
                list.Remove(item);
            }
        }
        /// <summary>
        /// 把只有一个儿子的节点上提
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<DeptPermissionSchemeEntryInfo> OptimizePermissionTree(List<DeptPermissionSchemeEntryInfo> list) {
            if (list == null || list.Count != 1) return list;
            if (list[0].SubDepts != null) return OptimizePermissionTree(list[0].SubDepts);
            return list;
        }
            
        /// <summary>
        /// 根据权限方案，获取该权限方案下的所有处理过的被授权对象
        /// </summary>
        /// <param name="schemeId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<Dictionary<string,object>> ListAuthorizedObjects(Guid schemeId, int userId)
        {
            DbTransaction tran = null;
            return this._deptPermissionRepository.ListAuthorizedObjects(tran,schemeId, userId);
        }

        private void ChangeList2List(List< DeptPermissionSchemeEntryInfo> dict, List<DeptPermissionSchemeEntryInfo> list)
        {
            foreach (DeptPermissionSchemeEntryInfo item in dict)
            {
                list.Add(item);
                if (item.SubDepts != null && item.SubDepts.Count > 0)
                {
                    ChangeList2List(item.SubDepts, list);
                }
                item.SubDepts = null;
            }
        }

        public object SaveSchemeDetail(Guid SchemeId,int authorized_userid,Guid authorized_roleid, DeptPermissionObjectTypeEnum authorized_type,List<DeptPermissionSchemeEntryInfo> items, int userId)
        {
            foreach (DeptPermissionSchemeEntryInfo item in items) {
                item.SchemeId = SchemeId;
                item.Authorized_RoleId = authorized_roleid;
                item.Authorized_Type = authorized_type;
                item.Authorized_UserId = authorized_userid;
                if (item.RecId == null || item.RecId.Equals(Guid.Empty)) item.RecId = Guid.NewGuid();
            }
            if (authorized_type == DeptPermissionObjectTypeEnum.User)
            {
                this._deptPermissionRepository.DeletePermissionItemByUser(null,SchemeId, authorized_userid, userId);
            }
            else {
                this._deptPermissionRepository.DeletePermissionItemByRole(null, SchemeId, authorized_roleid, userId);
            }
            this._deptPermissionRepository.SavePermissionItem(null, items,userId);
            return null;
        }
    }
}
