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
