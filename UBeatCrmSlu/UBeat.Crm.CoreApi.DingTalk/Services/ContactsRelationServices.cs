using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DingTalk.Models;
using UBeat.Crm.CoreApi.DingTalk.Repository;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.DingTalk.Services
{
    public class ContactsRelationServices: EntityBaseServices
    {
        private readonly IJYFContactsRepository _iJYFContactsRepository;
        public ContactsRelationServices(IJYFContactsRepository iJYFContactsRepository)
        {
            _iJYFContactsRepository = iJYFContactsRepository;
        }
        public Dictionary<string,object> GetContactRelations(Guid contactId, int level, int userid) {
            List<ContactsRelationInfo> retList = new List<ContactsRelationInfo>();
            Dictionary<string, ContactsRelationInfo> AllRelations = new Dictionary<string, ContactsRelationInfo>();
            Dictionary<string, string> AllContacts = new Dictionary<string, string>();
            Dictionary<string, string> NewContacts = new Dictionary<string, string>();
            Dictionary<string, string> NextNewContacts = new Dictionary<string, string>();
            AllContacts.Add(contactId.ToString(),contactId.ToString());
            NewContacts.Add(contactId.ToString(), contactId.ToString());
            for (int i = 0; i < level; i++) {
                NextNewContacts = new Dictionary<string, string>();
                //获取newcontacts内的所有公司信息
                #region 开始处理公司
                List< Dictionary<string, object>> companys = _iJYFContactsRepository.GetAllCompanys(NewContacts.Keys.GetEnumerator());
                //获取这些公司的所有历史员工
                if (companys.Count > 0) {
                    string companyids = "";
                    foreach (Dictionary<string, object>  company in companys) {
                        companyids = companyids + ",'" + company["recid"].ToString() + "'";
                    }
                    if (companyids.Length > 0) companyids = companyids.Substring(1);
                    List<ContactPositionInfo> ContactPosition = _iJYFContactsRepository.getContactsByCompanys(companyids);
                    Dictionary<string,List<ContactPositionInfo>> NextContactPosition = new Dictionary<string, List<ContactPositionInfo>>();
                    foreach (ContactPositionInfo item in ContactPosition) {
                        if (NewContacts.ContainsKey(item.ContactId.ToString()) || !AllContacts.ContainsKey(item.ContactId.ToString()))
                        {
                            if (NextContactPosition.ContainsKey(item.ContactId.ToString())) {
                                List<ContactPositionInfo> tmp = NextContactPosition[item.ContactId.ToString()];
                                tmp.Add(item);
                            }
                            else
                            {
                                List<ContactPositionInfo> tmp = new List<ContactPositionInfo>();
                                tmp.Add(item);
                                NextContactPosition.Add(item.ContactId.ToString(),tmp);
                            }
                           
                        }
                    }
                    //把已经存在与allContacts的员工排除掉
                    foreach (string newContact in NewContacts.Keys) {
                        if (NextContactPosition.ContainsKey(newContact) == false) continue;
                        List<ContactPositionInfo> newContactPositionItem = NextContactPosition[newContact];
                        foreach (string nextContact in NextContactPosition.Keys) {
                            if (newContact == nextContact) continue;
                            //检验关系
                            ContactRelationEnum relation = CheckRelation(newContactPositionItem, NextContactPosition[nextContact]);
                            if (relation != ContactRelationEnum.None) {
                                string c1 = newContact.ToString();
                                string c2 = nextContact.ToString();
                                if (c1.CompareTo(c2) <0 ) {
                                    string tmp = c1;
                                    c1 = c2;
                                    c2 = tmp;
                                }
                                ContactsRelationInfo relationInfo = new ContactsRelationInfo() {
                                    Contact1 = Guid.Parse(c1),
                                    Contact2 = Guid.Parse(c2),
                                    Id = c1 + ":" + c2,
                                    RelationType = relation
                                };
                                if(AllRelations.ContainsKey(relationInfo.Id) == false)
                                {
                                    AllRelations.Add(relationInfo.Id, relationInfo);
                                    if (NextNewContacts.ContainsKey(nextContact) == false)
                                        NextNewContacts.Add(nextContact, nextContact);
                                    if (AllContacts.ContainsKey(nextContact) ==false) {
                                        AllContacts.Add(nextContact, nextContact);
                                    }
                                }
                                
                            }

                        }
                    }

                }
                #endregion

                #region 处理其他关系
                List<ContactsRelationInfo> otherRelations = _iJYFContactsRepository.GetOtherContactRelations(NewContacts.Keys.GetEnumerator());
                foreach (ContactsRelationInfo r in otherRelations) {
                    foreach (string c in NewContacts.Keys) {
                        if (c == r.Contact1.ToString())
                        {
                            if (AllContacts.ContainsKey(r.Contact2.ToString()) || NextNewContacts.ContainsKey(r.Contact2.ToString()) || NewContacts.ContainsKey(r.Contact2.ToString())) continue;
                            AllRelations.Add(r.Id,r);
                            AllContacts.Add(r.Contact2.ToString(), r.Contact2.ToString());
                            NextNewContacts.Add(r.Contact2.ToString(), r.Contact2.ToString());

                        }
                        else if (c == r.Contact2.ToString()) {
                            if (AllContacts.ContainsKey(r.Contact1.ToString()) || NextNewContacts.ContainsKey(r.Contact1.ToString()) || NewContacts.ContainsKey(r.Contact1.ToString())) continue;
                            AllRelations.Add(r.Id, r);
                            NextNewContacts.Add(r.Contact1.ToString(), r.Contact1.ToString());
                            AllContacts.Add(r.Contact1.ToString(), r.Contact1.ToString());
                        }
                    }
                }
                

                #endregion 
                NewContacts = new Dictionary<string, string>();
                foreach (string key in NextNewContacts.Keys) {
                    NewContacts.Add(key, key);
                }
            }
            #region 处理联系人姓名
            Dictionary<Guid, string> ContactsName = _iJYFContactsRepository.GetContactsName(AllContacts.Keys);
            List<Dictionary<string, string>> RetData = new List<Dictionary<string, string>>();
            foreach (string name in ContactsName.Values) {
                Dictionary<string, string> nameDict = new Dictionary<string, string>();
                nameDict.Add("name", name);
                RetData.Add(nameDict);
            }
            #endregion 
            foreach (ContactsRelationInfo item in AllRelations.Values) {
                if (ContactsName.ContainsKey(item.Contact1)) {
                    item.Contact1Name = ContactsName[item.Contact1];
                }
                if (ContactsName.ContainsKey(item.Contact2))
                {
                    item.Contact2Name = ContactsName[item.Contact2];
                }
                retList.Add(item);
            }
            Dictionary<string, object> retDict = new Dictionary<string, object>();
            retDict.Add("links", retList);
            retDict.Add("data", RetData);
            return retDict;
        }

        private ContactRelationEnum CheckRelation(List<ContactPositionInfo> newContactPositionItem, List<ContactPositionInfo> list)
        {
            ContactRelationEnum relation = ContactRelationEnum.None;
            bool isOK = false;
            foreach (ContactPositionInfo p1 in newContactPositionItem)
            {
                if (p1.JobStart == DateTime.MinValue) continue;
                foreach (ContactPositionInfo p2 in list) {
                    if (p2.JobStart == DateTime.MinValue) continue;
                    if (p1.CustId != p2.CustId) continue;
                    if (p1.JobEnd == DateTime.MinValue)
                    {
                        if (p2.JobEnd == DateTime.MinValue) {
                            relation = ContactRelationEnum.Workmate;
                            isOK = true;
                            break;
                        }
                        else if(p1.JobStart  < p2.JobEnd)
                        {
                            relation = ContactRelationEnum.PreWorkmate;
                        }
                    }
                    else {
                        if (p2.JobEnd == DateTime.MinValue)
                        {
                            if (p2.JobStart < p1.JobEnd)
                            {
                                relation = ContactRelationEnum.PreWorkmate;
                            }
                        }
                        else {
                            if (!(p1.JobStart > p2.JobEnd || p1.JobEnd < p2.JobStart)) {
                                relation = ContactRelationEnum.PreWorkmate;
                            }
                        }
                    }

                }
                if (isOK) break;
            }
            return relation;
        }
    }
}
