using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Dapper;
using Npgsql;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.GL.Model;
using UBeat.Crm.CoreApi.GL.Repository;
using UBeat.Crm.CoreApi.Repository.Repository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.FHSJ.Repository
{
    public class CustomerRepository: RepositoryBase, ICustomerRepository
    {
        public List<string> getAddCode(List<string> codeList)
        {
            List<string> list = new List<string>();

            var sql = string.Format(@"
                select erpcode from crm_sys_customer where recstatus = 1 and erpcode in ('{0}');", string.Join("','", codeList));

            var result = DataBaseHelper.Query(sql, null, CommandType.Text);
            if (result.Count > 0)
            {
                foreach (var item in result)
                {
                    var code = string.Concat(item["erpcode"]);
                    if (string.IsNullOrEmpty(code)) continue;

                    if (codeList.Contains(code))
                        codeList.Remove(code);
                }
            }

            return codeList;
        }

        public List<string> getModifyCode(List<string> codeList)
        {
            List<string> list = new List<string>();

            var sql = string.Format(@"
                select erpcode from crm_sys_customer where recstatus = 1 and erpcode in ('{0}');", string.Join("','", codeList));

            var result = DataBaseHelper.Query(sql, null, CommandType.Text);
            if (result.Count > 0)
            {
                foreach (var item in result)
                {
                    var code = string.Concat(item["erpcode"]);
                    if (string.IsNullOrEmpty(code)) continue;

                    list.Add(string.Concat(code));
                }
            }

            return list;
        }

        public List<string> getDeleteCode(List<string> codeList)
        {
            List<string> list = new List<string>();

            var sql = string.Format(@"
                select erpcode from crm_sys_customer where recstatus = 1 and erpcode in ('{0}');", string.Join("','", codeList), Guid.Empty);

            var result = DataBaseHelper.Query(sql, null, CommandType.Text);
            if (result.Count > 0)
            {
                foreach (var item in result)
                {
                    var code = string.Concat(item["erpcode"]);
                    if (string.IsNullOrEmpty(code)) continue;

                    list.Add(code);
                }
            }

            return list;
        }

        public List<string> getCrmLostCode(List<string> codeList)
        {
            List<string> list = new List<string>();

            var sql = string.Format(@"
                select reccode from crm_sys_customer where erpcode is null and  recstatus = 1 and reccode in ('{0}');", string.Join("','", codeList), Guid.Empty);

            var result = DataBaseHelper.Query(sql, null, CommandType.Text);
            if (result.Count > 0)
            {
                foreach (var item in result)
                {
                    var code = string.Concat(item["reccode"]);
                    if (string.IsNullOrEmpty(code)) continue;

                    list.Add(code);
                }
            }

            return list;
        }

        public bool AddList(List<SaveCustomerMainView> dataList, int userId)
        {
            var result = false;

            StringBuilder sql = new StringBuilder();
            var i = 0;
            var index_key = "XXaddXX";
            var addParameters = new DynamicParameters();
            foreach (var item in dataList)
            {
                var parameters = new List<string>();
                var addSingleParameters = new DynamicParameters();

                var index = i++;
                var index_str = string.Concat(index, index_key);

                var sqlExist = @"select *  from crm_sys_customer where erpcode=@erpcode and recstatus=1 limit 1;";
                var param = new DynamicParameters();
                param.Add("erpcode", item.companyone);
                var custExistResult = DataBaseHelper.Query(sqlExist, param, CommandType.Text);
                if (custExistResult.Count > 0)
                {
                    continue;
                };

                //recid, rectype, recstatus, reccreator, issynchrosap, createfrom
                addSingleParameters.Add(string.Concat("@recid", index_str), item.id);
                addSingleParameters.Add(string.Concat("@rectype", index_str), item.rectype);
                addSingleParameters.Add(string.Concat("@recstatus", index_str), (int)item.status);
                addSingleParameters.Add(string.Concat("@reccreator", index_str), userId);
                addSingleParameters.Add(string.Concat("@issynchrosap", index_str), (int)SynchrosapStatus.Yes);
                addSingleParameters.Add(string.Concat("@flowstatus", index_str), 3);
                addSingleParameters.Add(string.Concat("@createfrom", index_str), (int)item.createfrom);
                 
                //recupdator, recmanager, reccreated, recupdated, reconlive,
                addSingleParameters.Add(string.Concat("@recupdator", index_str), userId);
                addSingleParameters.Add(string.Concat("@recmanager", index_str), item.manager);
                addSingleParameters.Add(string.Concat("@reccreated", index_str), DateTime.Now);
                addSingleParameters.Add(string.Concat("@recupdated", index_str), DateTime.Now);
                addSingleParameters.Add(string.Concat("@reconlive", index_str), DateTime.Now);

                //customertype,erpcode, customertype, recname,contacts,contactnumber
                addSingleParameters.Add(string.Concat("@customertype", index_str), item.customertype_crmid);//客户账户组
                addSingleParameters.Add(string.Concat("@erpcode", index_str), item.companyone);
                //addSingleParameters.Add(string.Concat("@appellation", index_str), item.appellation_crmid);
                addSingleParameters.Add(string.Concat("@recname", index_str), item.recname);
                addSingleParameters.Add(string.Concat("@customername", index_str), item.searchone);
                addSingleParameters.Add(string.Concat("@contacts", index_str), item.contacts);
                addSingleParameters.Add(string.Concat("@contactnumber", index_str), item.taxphone);

                //customercompanyaddress, region, country, region, postcode,
                addSingleParameters.Add(string.Concat("@customercompanyaddress", index_str), item.address);
                addSingleParameters.Add(string.Concat("@region", index_str), item.city_crmid);//城市
                addSingleParameters.Add(string.Concat("@country", index_str), item.country_crmid);
                addSingleParameters.Add(string.Concat("@saparea", index_str), item.region_crmid);//地区
                //addSingleParameters.Add(string.Concat("@postcode", index_str), item.postcode);

                //language, taxphone, mobilephone, fax,
                addSingleParameters.Add(string.Concat("@language", index_str), item.language);
                //addSingleParameters.Add(string.Concat("@taxphone", index_str), item.taxphone);
                //addSingleParameters.Add(string.Concat("@mobilephone", index_str), item.mobilephone);
                //addSingleParameters.Add(string.Concat("@fax", index_str), item.fax);

                //email, taxno, opencode, openname, accountcode, salesorganization,
                //addSingleParameters.Add(string.Concat("@email", index_str), item.email);
                addSingleParameters.Add(string.Concat("@taxno", index_str), item.valueadd);
                addSingleParameters.Add(string.Concat("@bank", index_str), item.opencode);
                addSingleParameters.Add(string.Concat("@account", index_str), item.accountcode);
                addSingleParameters.Add(string.Concat("@salesorganization", index_str), item.salesorganization_crmid);

                //saledistribution, productgroup, sapcusttype,industry,salesarea,
                addSingleParameters.Add(string.Concat("@saledistribution", index_str), item.distribution_crmid);
                addSingleParameters.Add(string.Concat("@productgroup", index_str), item.productgroup_crmid);
                addSingleParameters.Add(string.Concat("@sapcusttype", index_str), item.custgpone_crmid);
                addSingleParameters.Add(string.Concat("@industry", index_str), item.custgptwo_crmid);
                addSingleParameters.Add(string.Concat("@area", index_str), item.salesarea_crmid);

                //salesoffice, pricingpro, delivery, shipment, payment,
                addSingleParameters.Add(string.Concat("@salesoffice", index_str), item.salesoffice_crmid);
                //addSingleParameters.Add(string.Concat("@pricingpro", index_str), item.pricingpro_crmid);
                //addSingleParameters.Add(string.Concat("@delivery", index_str), item.delivery_crmid);
                addSingleParameters.Add(string.Concat("@shipment", index_str), item.shipment_crmid);
                addSingleParameters.Add(string.Concat("@payrequirement", index_str), item.payment_crmid);

                //accountgp, taxgp, currency, creditperiod, rules, 
                addSingleParameters.Add(string.Concat("@accountgp", index_str), item.accountgp_crmid);
                addSingleParameters.Add(string.Concat("@taxgp", index_str), item.taxgp_crmid);
                addSingleParameters.Add(string.Concat("@currency", index_str), item.currency_crmid);
                //addSingleParameters.Add(string.Concat("@creditperiod", index_str), item.creditperiod);
                //addSingleParameters.Add(string.Concat("@rules", index_str), item.rules_crmid);

                //risklimit, companycode, accountantsub
                addSingleParameters.Add(string.Concat("@credit", index_str), item.risklimit);
                addSingleParameters.Add(string.Concat("@companycode", index_str), item.companycode);//公司代码，核心
                addSingleParameters.Add(string.Concat("@accountantsub", index_str), item.accountantsub);
                addSingleParameters.Add(string.Concat("@updatefrom", index_str), 0);
                addSingleParameters.Add(string.Concat("@customerstatus", index_str), 2);

                foreach (var p in addSingleParameters.ParameterNames)
                { 
                    if (p.Contains("customercompanyaddress"))
                        parameters.Add(string.Concat(@"@", p, "::jsonb"));
                    else
                        parameters.Add(string.Concat(@"@", p)); 
                }

                addParameters.AddDynamicParams(addSingleParameters); 
                var insertSql = string.Format(@"INSERT INTO crm_sys_customer(
												recid, rectype, recstatus, reccreator, issynchrosap,flowstatus,createfrom,
                                                recupdator, recmanager, reccreated, recupdated, reconlive,
                                                customertype,erpcode, recname, customername,contacts,contactnumber 
                                                customercompanyaddress, region, country, saparea,
                                                language,
                                                taxno, bank,account, salesorganization,
                                                saledistribution, productgroup, sapcusttype,industry,area,
                                                salesoffice, shipment, payrequirement,
                                                accountgp,taxgp, currency,
                                                credit,companycode,accountantsub,updatefrom,customerstatus
                                                ) VALUES ({0});",
                                                string.Join(",", parameters));

                sql.Append(insertSql);
                var comSql = insertCustomerCommon(item, string.Concat(index_str, "_com"), addParameters, userId);
                if(!string.IsNullOrEmpty(comSql))
                {
                    sql.Append(comSql);
                }
            }

            var finalSql = sql.ToString();
            if (!string.IsNullOrEmpty(finalSql))
            {
                result = DataBaseHelper.ExecuteNonQuery(finalSql, addParameters, CommandType.Text) > 0;
            }

            return result;
        }
        public bool ModifyLostList(List<SaveCustomerMainView> dataList, int userId)
        {
            var result = false;

            StringBuilder sql = new StringBuilder();
            var index = 0;
            var index_key = "XXupdateXX";
            var parameters = new DynamicParameters();
            foreach (var item in dataList)
            {
                index++;
                var index_str = string.Concat(index, index_key);
                var setters = new List<string>();

                //parameters
                //recupdator, recmanager, recupdated, reconlive, recstatus, issynchrosap, createfrom
                parameters.Add(string.Concat("@recupdator", index_str), userId);
                // parameters.Add(string.Concat("@recmanager", index_str), item.manager);
                parameters.Add(string.Concat("@recupdated", index_str), DateTime.Now);
                parameters.Add(string.Concat("@reconlive", index_str), DateTime.Now);
                parameters.Add(string.Concat("@recstatus", index_str), (int)item.status);
                parameters.Add(string.Concat("@issynchrosap", index_str), (int)SynchrosapStatus.Yes);
                parameters.Add(string.Concat("@flowstatus", index_str), 3);
                //修改不修改来源
                //parameters.Add(string.Concat("@createfrom", index_str), (int)item.createfrom);

                //customertype, companyone, recname, searchone, 
                parameters.Add(string.Concat("@customertype", index_str), item.customertype_crmid);//客户账户组
                parameters.Add(string.Concat("@erpcode", index_str), item.companyone);
                //parameters.Add(string.Concat("@appellation", index_str), item.appellation_crmid);
                parameters.Add(string.Concat("@recname", index_str), item.recname);
                parameters.Add(string.Concat("@reccode", index_str), item.reccode);
                parameters.Add(string.Concat("@customername", index_str), item.searchone);
                parameters.Add(string.Concat("@contacts", index_str), item.contacts);
                parameters.Add(string.Concat("@contactnumber", index_str), item.taxphone);

                //customercompanyaddress, city, country, saparea, postcode,
                parameters.Add(string.Concat("@customercompanyaddress", index_str), item.address);
                parameters.Add(string.Concat("@region", index_str), item.city_crmid);//城市
                parameters.Add(string.Concat("@country", index_str), item.country_crmid);
                parameters.Add(string.Concat("@saparea", index_str), item.region_crmid);//地区
                //parameters.Add(string.Concat("@postcode", index_str), item.postcode);


                //language, taxphone, extension, mobilephone, fax,
                parameters.Add(string.Concat("@language", index_str), item.language);
                //parameters.Add(string.Concat("@taxphone", index_str), item.taxphone);
                //parameters.Add(string.Concat("@extension", index_str), item.extension);
                //parameters.Add(string.Concat("@mobilephone", index_str), item.mobilephone);
                //parameters.Add(string.Concat("@fax", index_str), item.fax);

                //email, valueadd, opencode, accountcode, salesorganization,
                //parameters.Add(string.Concat("@email", index_str), item.email);
                if (!string.IsNullOrEmpty(item.valueadd))
                {
                    parameters.Add(string.Concat("@taxno", index_str), item.valueadd);
                }
                parameters.Add(string.Concat("@bank", index_str), item.opencode);
                parameters.Add(string.Concat("@account", index_str), item.accountcode);
                parameters.Add(string.Concat("@salesorganization", index_str), item.salesorganization_crmid);


                //distribution, productgroup, sapcusttype,industry,salesarea,
                parameters.Add(string.Concat("@saledistribution", index_str), item.distribution_crmid);
                parameters.Add(string.Concat("@productgroup", index_str), item.productgroup_crmid);
                parameters.Add(string.Concat("@sapcusttype", index_str), item.custgpone_crmid);
                parameters.Add(string.Concat("@industry", index_str), item.custgptwo_crmid);
                parameters.Add(string.Concat("@area", index_str), item.salesarea_crmid);

                //salesoffice, pricingpro, delivery, shipment, payment,
                parameters.Add(string.Concat("@salesoffice", index_str), item.salesoffice_crmid);
                //parameters.Add(string.Concat("@pricingpro", index_str), item.pricingpro_crmid);
                //parameters.Add(string.Concat("@delivery", index_str), item.delivery_crmid);
                parameters.Add(string.Concat("@shipment", index_str), item.shipment_crmid);
                parameters.Add(string.Concat("@payrequirement", index_str), item.payment_crmid);

                //accountgp, taxgp, currency, creditperiod, rules, 
                parameters.Add(string.Concat("@accountgp", index_str), item.accountgp_crmid);
                parameters.Add(string.Concat("@taxgp", index_str), item.taxgp_crmid);
                parameters.Add(string.Concat("@currency", index_str), item.currency_crmid);
                //parameters.Add(string.Concat("@creditperiod", index_str), item.creditperiod);


                //risktype, checkrules, risklimit, companycode, accountantsub
                //parameters.Add(string.Concat("@risktype", index_str), item.risktype_crmid);
                parameters.Add(string.Concat("@credit", index_str), item.risklimit);
                parameters.Add(string.Concat("@companycode", index_str), item.companycode);
                parameters.Add(string.Concat("@accountantsub", index_str), item.accountantsub);
                //0sap 1crm
                parameters.Add(string.Concat("@updatefrom", index_str), 0);
                parameters.Add(string.Concat("@customerstatus", index_str), 2);

                //setters
                foreach (var p in parameters.ParameterNames)
                {
                    if (p.Contains(index_str))
                    {
                        var para = p.Replace(index_str, "");
                        if (p.Contains("customercompanyaddress"))
                            setters.Add(string.Format("{0} = {1}", para, string.Concat("@", p, "::jsonb")));
                        else
                            setters.Add(string.Format("{0} = {1}", para, string.Concat("@", p)));
                    }
                }
               var updateSql = string.Format(@"update crm_sys_customer set {1} where reccode = '{0}';",
                    item.reccode,
                    string.Join(",", setters));

                sql.Append(updateSql);

                var comSql = updateCustomerCommon(item, string.Concat(index_str, "_com"), parameters, userId);
                if (!string.IsNullOrEmpty(comSql))
                {
                    sql.Append(comSql);
                }
            }

            var finalSql = sql.ToString();
            if (!string.IsNullOrEmpty(finalSql))
            {
                result = DataBaseHelper.ExecuteNonQuery(finalSql, parameters, CommandType.Text) > 0;
            }

            return result;
        }

        public bool ModifyList(List<SaveCustomerMainView> dataList, int userId)
        {
            var result = false;

            StringBuilder sql = new StringBuilder();
            var index = 0;
            var index_key = "XXupdateXX";
            var parameters = new DynamicParameters();
            foreach (var item in dataList)
            {
                index++;
                var index_str = string.Concat(index, index_key);
                var setters = new List<string>();

                //parameters
                //parameters
                //recupdator, recmanager, recupdated, reconlive, recstatus, issynchrosap, createfrom
                parameters.Add(string.Concat("@recupdator", index_str), userId);
                // parameters.Add(string.Concat("@recmanager", index_str), item.manager);
                parameters.Add(string.Concat("@recupdated", index_str), DateTime.Now);
                parameters.Add(string.Concat("@reconlive", index_str), DateTime.Now);
                parameters.Add(string.Concat("@recstatus", index_str), (int)item.status);
                parameters.Add(string.Concat("@issynchrosap", index_str), (int)SynchrosapStatus.Yes);
                parameters.Add(string.Concat("@flowstatus", index_str), 3);
                //修改不修改来源
                //parameters.Add(string.Concat("@createfrom", index_str), (int)item.createfrom);

                //customertype, companyone, recname, searchone, 
                parameters.Add(string.Concat("@customertype", index_str), item.customertype_crmid);//客户账户组
                parameters.Add(string.Concat("@erpcode", index_str), item.companyone);
                //parameters.Add(string.Concat("@appellation", index_str), item.appellation_crmid);
                parameters.Add(string.Concat("@recname", index_str), item.recname);
                parameters.Add(string.Concat("@customername", index_str), item.searchone);
                parameters.Add(string.Concat("@contacts", index_str), item.contacts);
                parameters.Add(string.Concat("@contactnumber", index_str), item.taxphone);

                //customercompanyaddress, city, country, saparea, postcode,
                parameters.Add(string.Concat("@customercompanyaddress", index_str), item.address);
                parameters.Add(string.Concat("@region", index_str), item.city_crmid);//城市
                parameters.Add(string.Concat("@country", index_str), item.country_crmid);
                parameters.Add(string.Concat("@saparea", index_str), item.region_crmid);//地区
                //parameters.Add(string.Concat("@postcode", index_str), item.postcode);


                //language, taxphone, extension, mobilephone, fax,
                parameters.Add(string.Concat("@language", index_str), item.language);
                //parameters.Add(string.Concat("@taxphone", index_str), item.taxphone);
                //parameters.Add(string.Concat("@extension", index_str), item.extension);
                //parameters.Add(string.Concat("@mobilephone", index_str), item.mobilephone);
                //parameters.Add(string.Concat("@fax", index_str), item.fax);

                //email, valueadd, opencode, accountcode, salesorganization,
                //parameters.Add(string.Concat("@email", index_str), item.email);
                if (!string.IsNullOrEmpty(item.valueadd))
                {
                    parameters.Add(string.Concat("@taxno", index_str), item.valueadd);
                }
                parameters.Add(string.Concat("@bank", index_str), item.opencode);
                parameters.Add(string.Concat("@account", index_str), item.accountcode);
                parameters.Add(string.Concat("@salesorganization", index_str), item.salesorganization_crmid);


                //distribution, productgroup, sapcusttype,industry, salesarea,
                parameters.Add(string.Concat("@saledistribution", index_str), item.distribution_crmid);
                parameters.Add(string.Concat("@productgroup", index_str), item.productgroup_crmid);
                parameters.Add(string.Concat("@sapcusttype", index_str), item.custgpone_crmid);
                parameters.Add(string.Concat("@industry", index_str), item.custgptwo_crmid);
                parameters.Add(string.Concat("@area", index_str), item.salesarea_crmid);

                //salesoffice, pricingpro, delivery, shipment, payment,
                parameters.Add(string.Concat("@salesoffice", index_str), item.salesoffice_crmid);
                //parameters.Add(string.Concat("@pricingpro", index_str), item.pricingpro_crmid);
                //parameters.Add(string.Concat("@delivery", index_str), item.delivery_crmid);
                parameters.Add(string.Concat("@shipment", index_str), item.shipment_crmid);
                parameters.Add(string.Concat("@payrequirement", index_str), item.payment_crmid);

                //accountgp, taxgp, currency, creditperiod, rules, 
                parameters.Add(string.Concat("@accountgp", index_str), item.accountgp_crmid);
                parameters.Add(string.Concat("@taxgp", index_str), item.taxgp_crmid);
                parameters.Add(string.Concat("@currency", index_str), item.currency_crmid);
                //parameters.Add(string.Concat("@creditperiod", index_str), item.creditperiod);


                //risktype, checkrules, risklimit, companycode, accountantsub
                //parameters.Add(string.Concat("@risktype", index_str), item.risktype_crmid);
                parameters.Add(string.Concat("@credit", index_str), item.risklimit);
                parameters.Add(string.Concat("@companycode", index_str), item.companycode);
                parameters.Add(string.Concat("@accountantsub", index_str), item.accountantsub);
                //0sap 1crm
                parameters.Add(string.Concat("@updatefrom", index_str), 0);
                parameters.Add(string.Concat("@customerstatus", index_str), 2);

                //setters
                foreach (var p in parameters.ParameterNames)
                {
                    if (p.Contains(index_str))
                    {
                        var para = p.Replace(index_str, "");
                        if (p.Contains("customercompanyaddress"))
                            setters.Add(string.Format("{0} = {1}", para, string.Concat("@", p, "::jsonb")));
                        else
                            setters.Add(string.Format("{0} = {1}", para, string.Concat("@", p)));
                    }
                }
                var updateSql = string.Format(@"update crm_sys_customer set {1} where erpcode = '{0}';",
                    item.companyone,
                    string.Join(",", setters));
                 
                sql.Append(updateSql);

                var comSql = updateCustomerCommon(item, string.Concat(index_str, "_com"), parameters, userId);
                if (!string.IsNullOrEmpty(comSql))
                {
                    sql.Append(comSql);
                }
            }

            var finalSql = sql.ToString();
            if (!string.IsNullOrEmpty(finalSql))
            {
                result = DataBaseHelper.ExecuteNonQuery(finalSql, parameters, CommandType.Text) > 0;
            }

            return result;
        }

        public bool DeleteList(List<string> codeList, int userId)
        {
            var result = false;

            StringBuilder sql = new StringBuilder();
            foreach (var item in codeList)
            {
                var setters = new List<string>();
                setters.Add(string.Format("{0} = {1}", "recstatus", 0));
                setters.Add(string.Format("{0} = {1}", "recupdator", userId));
                setters.Add(string.Format("{0} = '{1}'", "recupdated", DateTime.Now));
                setters.Add(string.Format("{0} = '{1}'", "reconlive", DateTime.Now));

                var updateSql = string.Format(@"update crm_sys_customer set {0} where erpcode = '{1}';", string.Join(",", setters), item);

                sql.Append(updateSql);
            }

            var finalSql = sql.ToString();
            if (!string.IsNullOrEmpty(finalSql))
            {
                result = DataBaseHelper.ExecuteNonQuery(finalSql, null, CommandType.Text) > 0;
            }
            return result;
        }

        public string insertCustomerCommon(SaveCustomerMainView item, string index, DynamicParameters param, int userId)
        {
            StringBuilder sql = new StringBuilder();

            var parameters = new List<string>();
            var addSingleParameters = new DynamicParameters();

            //recid, recname, recstatus, reccreator, recupdator,
            addSingleParameters.Add(string.Concat("@recid", index), item.id);  
            addSingleParameters.Add(string.Concat("@recname", index), item.recname); 
            addSingleParameters.Add(string.Concat("@recstatus", index), (int)item.status);
            addSingleParameters.Add(string.Concat("@reccreator", index), userId); 
            addSingleParameters.Add(string.Concat("@recupdator", index), userId);

            //recmanager, reccreated, recupdated, rectype,
            addSingleParameters.Add(string.Concat("@recmanager", index), item.manager);
            addSingleParameters.Add(string.Concat("@reccreated", index), DateTime.Now);
            addSingleParameters.Add(string.Concat("@recupdated", index), DateTime.Now);
            addSingleParameters.Add(string.Concat("@rectype", index), item.rectype);

            foreach (var p in addSingleParameters.ParameterNames)
            { 
                parameters.Add(string.Concat(@"@", p));
            }

            param.AddDynamicParams(addSingleParameters);

            var insertSql = string.Format(@"INSERT INTO crm_sys_custcommon(
											recid, recname, recstatus, reccreator, recupdator,
                                            recmanager, reccreated, recupdated, rectype) VALUES ({0});
                                            INSERT INTO crm_sys_custcommon_customer_relate(
                                            commonid, custid, relateindex) VALUES('{1}', '{2}', 1);
                                            ",
                                            string.Join(",", parameters), item.id, item.id);

            sql.Append(string.Format(@"DELETE FROM crm_sys_custcommon WHERE recid = '{0}';",item.id)); 
            sql.Append(string.Format(@"DELETE FROM crm_sys_custcommon_customer_relate WHERE custid = '{0}';", item.id));
            sql.Append(insertSql);

            return sql.ToString();
        }

        public string updateCustomerCommon(SaveCustomerMainView item, string index_str, DynamicParameters param, int userId)
        {
            StringBuilder sql = new StringBuilder();
             
            var parameters = new DynamicParameters();
            var setters = new List<string>();

            parameters.Add(string.Concat("@recid", index_str), item.id);
            parameters.Add(string.Concat("@recname", index_str), item.recname);
            parameters.Add(string.Concat("@recstatus", index_str), (int)item.status);
             
            param.AddDynamicParams(parameters);

            foreach (var p in parameters.ParameterNames)
            {
                var para = p.Replace(index_str, "");
                setters.Add(string.Format("{0} = {1}", para, string.Concat("@", p)));
            }
            var updateSql = string.Format(@"update crm_sys_custcommon set {1} where recid = '{0}';",
                item.id,
                string.Join(",", setters));

            sql.Append(updateSql);

            return sql.ToString();
        }

        public string insertCustomerSalesView(SaveCustomerSalesView item, string index_str, DynamicParameters param, int userId)
        {
            StringBuilder sql = new StringBuilder();

            var parameters = new List<string>();
            var addSingleParameters = new DynamicParameters();

            //recid, rectype, recstatus, reccreator, 
            addSingleParameters.Add(string.Concat("@recid", index_str), item.id);
            addSingleParameters.Add(string.Concat("@rectype", index_str), item.rectype);
            addSingleParameters.Add(string.Concat("@recstatus", index_str), (int)item.status);
            addSingleParameters.Add(string.Concat("@reccreator", index_str), userId);

            //recupdator, recmanager, reccreated, recupdated, reconlive,
            addSingleParameters.Add(string.Concat("@recupdator", index_str), userId);
            addSingleParameters.Add(string.Concat("@recmanager", index_str), item.manager);
            addSingleParameters.Add(string.Concat("@reccreated", index_str), DateTime.Now);
            addSingleParameters.Add(string.Concat("@recupdated", index_str), DateTime.Now);
            addSingleParameters.Add(string.Concat("@reconlive", index_str), DateTime.Now);

            //salesorganization, distribution, productgroup, custgpone, custgptwo,
            addSingleParameters.Add(string.Concat("@salesorganization", index_str), item.salesorganization_crmid);
            addSingleParameters.Add(string.Concat("@distribution", index_str), item.distribution_crmid);
            addSingleParameters.Add(string.Concat("@productgroup", index_str), item.productgroup_crmid);
            addSingleParameters.Add(string.Concat("@custgpone", index_str), item.custgpone_crmid);
            addSingleParameters.Add(string.Concat("@custgptwo", index_str), item.custgptwo_crmid);

            //salesarea, salesoffice, pricingpro, delivery, shipment,  
            addSingleParameters.Add(string.Concat("@salesarea", index_str), item.salesarea_crmid);
            addSingleParameters.Add(string.Concat("@salesoffice", index_str), item.salesoffice_crmid);
            addSingleParameters.Add(string.Concat("@pricingpro", index_str), item.pricingpro_crmid);
            addSingleParameters.Add(string.Concat("@delivery", index_str), item.delivery_crmid);
            addSingleParameters.Add(string.Concat("@shipment", index_str), item.shipment_crmid);

            //payment, accountgp, taxgp, currency,
            addSingleParameters.Add(string.Concat("@payment", index_str), item.payment_crmid); 
            addSingleParameters.Add(string.Concat("@accountgp", index_str), item.accountgp_crmid);
            addSingleParameters.Add(string.Concat("@taxgp", index_str), item.taxgp_crmid);
            addSingleParameters.Add(string.Concat("@currency", index_str), item.currency_crmid);

            //customer
            addSingleParameters.Add(string.Concat("@customer", index_str), item.customer);

            foreach (var p in addSingleParameters.ParameterNames)
            {
                if (p.Contains("customer"))
                    parameters.Add(string.Concat(@"@", p, "::jsonb"));
                else
                    parameters.Add(string.Concat(@"@", p)); 
            }

            param.AddDynamicParams(addSingleParameters);

            var insertSql = string.Format(@"INSERT INTO crm_sys_sales(
												recid, rectype, recstatus, reccreator, 
                                                recupdator, recmanager, reccreated, recupdated, reconlive,

                                                salesorganization, distribution, productgroup, custgpone, custgptwo,
                                                salesarea, salesoffice, pricingpro, delivery, shipment,
                                                payment, accountgp, taxgp, currency,
                                                customer
                                                ) VALUES ({0});",
                                                string.Join(",", parameters)); 
            sql.Append(insertSql);

            return sql.ToString();
        }

        public string insertCustomerBurkView(SaveCustomerBurkView item, string index_str, DynamicParameters param, int userId)
        {
            StringBuilder sql = new StringBuilder();

            var parameters = new List<string>();
            var addSingleParameters = new DynamicParameters();

            //recid, recstatus, reccreator, recupdator,
            addSingleParameters.Add(string.Concat("@recid", index_str), item.id); 
            addSingleParameters.Add(string.Concat("@recstatus", index_str), (int)item.status);
            addSingleParameters.Add(string.Concat("@reccreator", index_str), userId);
            addSingleParameters.Add(string.Concat("@recupdator", index_str), userId);

            //recmanager, reccreated, recupdated,
            addSingleParameters.Add(string.Concat("@recmanager", index_str), item.manager);
            addSingleParameters.Add(string.Concat("@reccreated", index_str), DateTime.Now);
            addSingleParameters.Add(string.Concat("@recupdated", index_str), DateTime.Now);

            //recname, accountingsubjects
            addSingleParameters.Add(string.Concat("@recname", index_str), item.companycode);
            addSingleParameters.Add(string.Concat("@accountingsubjects", index_str), item.accountantsub);

            //customer
            addSingleParameters.Add(string.Concat("@customer", index_str), item.customer);

            foreach (var p in addSingleParameters.ParameterNames)
            {
                if (p.Contains("customer"))
                    parameters.Add(string.Concat(@"@", p, "::jsonb"));
                else
                    parameters.Add(string.Concat(@"@", p));
            }

            param.AddDynamicParams(addSingleParameters);

            var insertSql = string.Format(@"INSERT INTO crm_fhsj_cust_finance(
											recid, recstatus, reccreator, recupdator,
                                            recmanager, reccreated, recupdated,
                                            recname, accountingsubjects,
                                            customer) VALUES ({0});
                                            ",
                                            string.Join(",", parameters)); 
            sql.Append(insertSql);

            return sql.ToString();
        }

        public int UpdateCustomerSapCode(Guid recId, string sapCode, DbTransaction tran = null)
        { 
            var updateSql = string.Format("update crm_sys_customer set erpcode = @sapCode, recupdated = now() where recid = @recId;"); 
			var param = new DbParameter[]
			{
				new NpgsqlParameter("recId",recId),
				new NpgsqlParameter("sapCode", sapCode),
			};

			if (tran == null)
				return DBHelper.ExecuteNonQuery("", updateSql, param);

			var result = DBHelper.ExecuteNonQuery(tran, updateSql, param); 
            return result;
        }

		public bool ModifyFetchList(List<SaveCustomerMainView> dataList, int userId)
		{
			var result = false;

			StringBuilder sql = new StringBuilder();
			var index = 0;
			var index_key = "XXupdateXX";
			var parameters = new DynamicParameters();
			foreach (var item in dataList)
			{
				index++;
				var index_str = string.Concat(index, index_key);
				var setters = new List<string>();

				//parameters
				//recupdator, recmanager, recupdated, reconlive, recstatus,
				parameters.Add(string.Concat("@recupdator", index_str), userId);
				parameters.Add(string.Concat("@recmanager", index_str), item.manager);
				parameters.Add(string.Concat("@recupdated", index_str), DateTime.Now);
				parameters.Add(string.Concat("@reconlive", index_str), DateTime.Now);
				parameters.Add(string.Concat("@recstatus", index_str), (int)item.status); 
				 
				//salesorganization,
				parameters.Add(string.Concat("@salesorganization", index_str), item.salesorganization_crmid);

				//distribution, productgroup, custgpone, custgptwo, salesarea,
				parameters.Add(string.Concat("@distribution", index_str), item.distribution_crmid);
				parameters.Add(string.Concat("@productgroup", index_str), item.productgroup_crmid);
				parameters.Add(string.Concat("@custgpone", index_str), item.custgpone_crmid);
				parameters.Add(string.Concat("@custgptwo", index_str), item.custgptwo_crmid);
				parameters.Add(string.Concat("@salesarea", index_str), item.salesarea_crmid);

				//salesoffice, pricingpro, delivery, shipment, payment,
				parameters.Add(string.Concat("@salesoffice", index_str), item.salesoffice_crmid);
				parameters.Add(string.Concat("@pricingpro", index_str), item.pricingpro_crmid);
				parameters.Add(string.Concat("@delivery", index_str), item.delivery_crmid);
				parameters.Add(string.Concat("@shipment", index_str), item.shipment_crmid);
				parameters.Add(string.Concat("@payment", index_str), item.payment_crmid);

				//accountgp, taxgp, currency, creditperiod, rules, 
				parameters.Add(string.Concat("@accountgp", index_str), item.accountgp_crmid);
				parameters.Add(string.Concat("@taxgp", index_str), item.taxgp_crmid);
				parameters.Add(string.Concat("@currency", index_str), item.currency_crmid); 

				//setters
				foreach (var p in parameters.ParameterNames)
				{
					if (p.Contains(index_str))
					{
						var para = p.Replace(index_str, "");
						if (p.Contains("address"))
							setters.Add(string.Format("{0} = {1}", para, string.Concat("@", p, "::jsonb")));
						else
							setters.Add(string.Format("{0} = {1}", para, string.Concat("@", p)));
					}
				}
				var updateSql = string.Format(@"update crm_sys_customer set {1} where companyone = '{0}';",
					item.companyone,
					string.Join(",", setters));

				sql.Append(updateSql);

				var comSql = updateCustomerCommon(item, string.Concat(index_str, "_com"), parameters, userId);
				if (!string.IsNullOrEmpty(comSql))
				{
					sql.Append(comSql);
				}

				if (item.salesView.Count > 0)
				{
					var indexDetail = 0;
					sql.Append(string.Format(@"DELETE FROM crm_sys_sales where (customer->>'id')::text = '{0}';", item.id));
					foreach (var detail in item.salesView)
					{
						indexDetail++;
						var detailSql = insertCustomerSalesView(detail, string.Concat(index, "sales", indexDetail), parameters, userId);
						if (!string.IsNullOrEmpty(detailSql))
						{
							sql.Append(detailSql);
						}
					}
				}

				if (item.burkView.Count > 0)
				{
					var indexDetail = 0;
					sql.Append(string.Format(@"DELETE FROM crm_fhsj_cust_finance where (customer->>'id')::text = '{0}';", item.id));
					foreach (var detail in item.burkView)
					{
						indexDetail++;
						var detailSql = insertCustomerBurkView(detail, string.Concat(index, "burk", indexDetail), parameters, userId);
						if (!string.IsNullOrEmpty(detailSql))
						{
							sql.Append(detailSql);
						}
					}
				}
			}

			var finalSql = sql.ToString();
			if (!string.IsNullOrEmpty(finalSql))
			{
				result = DataBaseHelper.ExecuteNonQuery(finalSql, parameters, CommandType.Text) > 0;
			}

			return result;
		}

        #region 同步银行信息
        public List<Dictionary<string, object>> GetCRMBankInfoList()
        {
            string sql = @"select recid,banks,bankl,erdat,ernam,banka,provz,ort01,bnklz
                            from crm_gl_bankinfo where recstatus = 1";
            return ExecuteQuery(sql, new DbParameter[] { });
        }
        #endregion

        public Guid IsExistsDelivnote(string code)
        {
            string sql = @"select recid from crm_glsc_deliveryorder 
                    where code = @code and recstatus = 1  limit 1";
            var p = new DbParameter[]
            {
                new NpgsqlParameter("code",code)
            };
            var res = ExecuteScalar(sql, p);
            Guid g;
            Guid.TryParse(res?.ToString(), out g);
            return g;
        }

        public Dictionary<string, object> getUserInfo(string workCode)
        {
            string sql = @"select * from crm_sys_userinfo where recstatus = 1 and workcode = @code";
            var p = new DbParameter[]
            {
                new NpgsqlParameter("code",workCode)
            };
            return ExecuteQuery(sql, p).FirstOrDefault();
        }
        public Dictionary<string, object> GetOrderInfo(string orderCode)
        {
            string sql = @"select customer,salesdepartments,salesterritory,jsonb_build_object('id',recid,'name',orderid) as orderjson
                        from crm_sys_order where recstatus = 1 and  orderid = @code ";
            var p = new DbParameter[]
            {
                new NpgsqlParameter("code",orderCode)
            };
            return ExecuteQuery(sql, p).FirstOrDefault();
        }
    }
}
