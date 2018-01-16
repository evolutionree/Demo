using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using System.Linq;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;

namespace UBeat.Crm.CoreApi.Services.Utility
{
    public static class DynamicProtocolHelper
    {
        public static Dictionary<string, DynamicProtocolValidResult> ValidData(
            List<DynamicEntityDataFieldMapper> fields,
            Dictionary<string, object> fieldDatas, DynamicProtocolOperateType operateType, bool isMobile = false)
        {
            //验证字段必填，正则
            //验证字段数据是否符合规则，数据有效性
            var validResultDic = new Dictionary<string, DynamicProtocolValidResult>();

            foreach (var field in fields)
            {
                //字段过滤器，过滤一些只读字段等
                if (!ValidFieldFilter(field, operateType)) continue;

                object fieldData;
                fieldDatas.TryGetValue(field.FieldName, out fieldData);

                //验证字段配置
                var validResult = ValidFieldConfig(field, fieldData, isMobile);
                if (validResult == null)
                {
                    if (!field.IsVisible && !string.IsNullOrWhiteSpace(field.DefaultValue) && operateType == DynamicProtocolOperateType.Add)
                    {
                        validResult = ValidDefaultValue(field);
                        if (validResult != null)
                        {
                            validResultDic.Add(field.FieldName, validResult);
                        }
                    }
                    continue;
                }
                if (!validResult.IsValid)
                {
                    validResultDic.Add(field.FieldName, validResult);
                    continue;
                }

                //特殊的字段值转换
                //validResult = ValidFieldStruct(field, fieldData);
                validResultDic.Add(field.FieldName, validResult);
            }

            return validResultDic;
        }

        public static DynamicProtocolValidResult ValidDefaultValue(DynamicEntityDataFieldMapper field)
        {
            var result = new DynamicProtocolValidResult();
            result.FieldName = field.FieldName;
            if (string.IsNullOrWhiteSpace(field.DefaultValue))
            {
                return null;
            }

            switch ((DynamicProtocolControlType)field.ControlType)
            {
                case DynamicProtocolControlType.RecOnlive:
                case DynamicProtocolControlType.RecCreated:
                case DynamicProtocolControlType.RecUpdated:
                case DynamicProtocolControlType.NumberInt:
                case DynamicProtocolControlType.NumberDecimal:
                case DynamicProtocolControlType.TimeDate:
                case DynamicProtocolControlType.TimeStamp:
                    {
                        result.FieldData = field.DefaultValue.Contains("now") ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : field.DefaultValue;
                        break;
                    }
                default:
                    {
                        result.FieldData = field.DefaultValue;
                        break;
                    }
            }

            result.IsValid = true;

            return result;
        }

        public static DynamicProtocolValidResult ValidFieldConfig(DynamicEntityDataFieldMapper field, object data, bool isMobile)
        {
            var result = new DynamicProtocolValidResult();
            result.FieldName = field.FieldName;

            //字段值为空时，如果是非必填，则忽略该键值  必填+隐藏 跳过校验
            if (field.IsRequire && field.IsVisible)
            {
                if (string.IsNullOrWhiteSpace(data?.ToString().Trim()))
                {
                    if (!(isMobile && field.ControlType == (int)DynamicProtocolControlType.FileAttach))
                    {
                        result.Tips = string.Format("{0}必填，不能为空", field.DisplayName);
                        return result;
                    }
                }
            }

            if (field.IsRequire && field.IsReadOnly)
            {
                if (string.IsNullOrWhiteSpace(data?.ToString().Trim()))
                {
                    if (!(isMobile && field.ControlType == (int)DynamicProtocolControlType.FileAttach))
                    {
                        result.Tips = string.Format("{0}字段规则配置冲突，请重新修改配置", field.DisplayName);
                        return result;
                    }
                }
            }

            if (data == null) return null;

            var fieldConfig = JsonHelper.ToObject<DynamicProtocolFieldConfig>(field.FieldConfig);
            if (fieldConfig == null)
            {
                result.Tips = string.Format("{0}配置有误，FieldConfig不能为空", field.DisplayName);
                return result;
            }

            var dataString = data.ToString().Trim();
            if (string.IsNullOrEmpty(dataString))
            {
                result.IsValid = true;
                result.FieldData = null;
                return result;
            }

            //验证字段长度
            if (fieldConfig.MinLength.HasValue)
            {
                if (dataString.Length < fieldConfig.MinLength.Value)
                {
                    result.Tips = string.Format("{0}填写有误,最小长度为{1}", field.DisplayName, fieldConfig.MinLength.Value);
                    return result;
                }
            }
            if (fieldConfig.MaxLength.HasValue)
            {
                if (dataString.Length > fieldConfig.MaxLength.Value)
                {
                    result.Tips = string.Format("{0}填写有误,最大长度为{1}", field.DisplayName, fieldConfig.MaxLength.Value);
                    return result;
                }
            }

            //验证正则表达式
            if (!string.IsNullOrWhiteSpace(fieldConfig.ValidRegex))
            {
                var match = Regex.Match(dataString, fieldConfig.ValidRegex);
                if (!match.Success)
                {
                    result.Tips = string.Format("{0}填写有误,不符合正则要求{1}", field.DisplayName, fieldConfig.ValidRegex);
                    return result;
                }
            }

            result.IsValid = true;
            result.FieldData = dataString;

            return result;
        }

        public static bool ValidFieldFilter(DynamicEntityDataFieldMapper field, DynamicProtocolOperateType operateType)
        {
            switch (operateType)
            {
                case DynamicProtocolOperateType.Add:
                case DynamicProtocolOperateType.Edit:
                    {
                        //只读字段直接过滤掉
                        //if (field.IsReadOnly && !field.IsRequire)
                        //{
                        //    return false;
                        //}

                        //不可见控件也过滤掉
                        if (!field.IsVisible)
                        {
                            //新增的时候，如果一个控件不可见，可是却有默认值，则需要返回
                            if (operateType == DynamicProtocolOperateType.Add && !string.IsNullOrWhiteSpace(field.DefaultValue))
                            {
                                return true;
                            }
                            return false;
                        }

                        //只读控件也是需要过滤的
                        var unusedControl = new List<DynamicProtocolControlType>
                    {
                        DynamicProtocolControlType.AreaGroup,
                        DynamicProtocolControlType.TipText
                    };
                        if (unusedControl.Contains((DynamicProtocolControlType)field.ControlType))
                        {
                            return false;
                        }

                        var sysControls = new List<int>
                    {
                        1001,
                        1002,
                        1003,
                        1004,
                        1005,
                        1007,
                        1008,
                        1009
                    };

                        if (sysControls.Contains(field.ControlType))
                        {
                            return false;
                        }

                        break;
                    }
                case DynamicProtocolOperateType.List:
                    {
                        //只读控件也是需要过滤的
                        var unusedControl = new List<DynamicProtocolControlType>
                    {
                        DynamicProtocolControlType.AreaGroup,
                        DynamicProtocolControlType.TipText
                    };
                        if (unusedControl.Contains((DynamicProtocolControlType)field.ControlType))
                        {
                            return false;
                        }
                        break;
                    }
            }

            return true;
        }

        public static DynamicProtocolValidResult ValidFieldStruct(DynamicEntityDataFieldMapper field, object data)
        {
            var result = new DynamicProtocolValidResult { FieldData = data.ToString().Trim() };

            //这里需要遵循一个原则，数据验证，尽量用正则完成验证，这里只对特殊的数据进行处理
            //例如数据导入时，因为不在前端输入数据，很容易造成数据错误
            switch ((DynamicProtocolControlType)field.ControlType)
            {
                case DynamicProtocolControlType.TimeDate:
                    {
                        result.FieldData = DateTime.Parse(result.FieldData.ToString()).ToString("yyyy-MM-dd");
                        break;
                    }
                case DynamicProtocolControlType.TimeStamp:
                    {
                        result.FieldData = DateTime.Parse(result.FieldData.ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                        break;
                    }
            }

            result.IsValid = true;
            result.FieldName = field.FieldName;
            return result;
        }
        /// <summary>
        /// 用于新版的查询条件整理，以后会改名为正常的名称
        /// </summary>
        /// <param name="searchFields"></param>
        /// <param name="fieldDatas"></param>
        /// <returns></returns>
        public static Dictionary<string, DynamicProtocolValidResult> AdvanceQuery2(
            List<DynamicEntityFieldSearch> searchFields, Dictionary<string, object> fieldDatas)
        {
            var validResultDic = new Dictionary<string, DynamicProtocolValidResult>();


            foreach (var field in searchFields)
            {
                var checkType = CheckSpecialField(field);
                bool isNameField = false;

                //周报的reportdate控件比较特殊
                var columnKey = field.FieldName;
                #region 已经没有意义了
                //if (checkType != field.NewType && field.NewType != 0)
                //{
                //    columnKey = columnKey + "_name";
                //    isNameField = true;
                //}
                #endregion 

                if (field.NewType == 0)
                {
                    field.NewType = checkType;
                }

                object fieldData;
                fieldDatas.TryGetValue(field.FieldName, out fieldData);

                if (string.IsNullOrWhiteSpace(fieldData?.ToString().Trim()))
                {
                    continue;
                }

                var result = new DynamicProtocolValidResult();
                result.FieldName = field.FieldName;

                var dataStr = fieldData.ToString().Trim();

                //根据控件处理不同的数据格式
                //数字，时间这2种才有范围，其余的是文本ilike
                switch ((DynamicProtocolControlType)field.NewType)
                {
                    case DynamicProtocolControlType.RecOnlive:
                    case DynamicProtocolControlType.RecCreated:
                    case DynamicProtocolControlType.RecUpdated:
                    case DynamicProtocolControlType.NumberInt:
                    case DynamicProtocolControlType.NumberDecimal:
                    case DynamicProtocolControlType.TimeDate:
                    case DynamicProtocolControlType.TimeStamp:
                        {
                            //一个是等于，两个是between
                            var dataArr = dataStr.Split(',');
                            if (dataArr.Length != 2 && dataArr.Length != 1)
                            {
                                result.Tips = "数据格式不对,应为逗号分隔";
                                validResultDic.Add(field.FieldName, result);
                                continue;
                            }
                            if (dataArr.Length == 1)
                            {
                                var conditions = new List<string>();
                                if (!string.IsNullOrWhiteSpace(dataArr[0].Trim()))
                                {
                                    conditions.Add(string.Format("e.{0} = '{1}' ", columnKey, dataArr[0].Trim()));
                                }
                                if (conditions.Count > 0)
                                {
                                    result.FieldData = string.Join(" AND ", conditions.ToArray());
                                }
                            }
                            else
                            {
                                var conditions = new List<string>();
                                if (!string.IsNullOrWhiteSpace(dataArr[0].Trim()))
                                {
                                    conditions.Add(string.Format("e.{0} >= '{1}' ", columnKey, dataArr[0].Trim()));
                                }
                                if (!string.IsNullOrWhiteSpace(dataArr[1].Trim()))
                                {
                                    conditions.Add(string.Format("e.{0} <= '{1}' ", columnKey, dataArr[1].Trim()));
                                }

                                if (conditions.Count > 0)
                                {
                                    result.FieldData = string.Join(" AND ", conditions.ToArray());
                                }
                            }


                            break;
                        }
                    case DynamicProtocolControlType.RecName:
                    case DynamicProtocolControlType.Telephone:
                    case DynamicProtocolControlType.PhoneNum:
                    case DynamicProtocolControlType.Text:
                    case DynamicProtocolControlType.TextArea:
                    case DynamicProtocolControlType.EmailAddr:
                        {
                            var _operator = string.Empty;
                            if (field.IsLike == 0)//是否走模糊查询 1 支持模糊 0不支持
                            {
                                if (isNameField == false)
                                {
                                    result.FieldData = string.Format("e.{0}='{1}'", columnKey, dataStr);
                                }
                                else
                                {
                                    result.FieldData = string.Format("e.{0}='{1}'", columnKey, dataStr);//应该不可能在这里

                                }
                            }
                            else
                            {
                                if (isNameField == false)
                                {
                                    result.FieldData = string.Format("e.{0} ilike '%{1}%'", columnKey, dataStr);
                                }
                                else
                                {
                                    result.FieldData = string.Format("e.{0} ilike '%{1}%'", columnKey, dataStr);//应该不可能在这里

                                }
                            }
                            break;
                        }
                    case DynamicProtocolControlType.Address:
                    case DynamicProtocolControlType.Location:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("jsonb_extract_path_text(e.{0}, 'address')='%{1}%'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format("jsonb_extract_path_text(e.{0}, 'address') ilike '%{1}%'", columnKey, dataStr);
                            }
                            break;
                        }
                    case DynamicProtocolControlType.RecCreator:
                    case DynamicProtocolControlType.RecUpdator:
                    case DynamicProtocolControlType.RecManager:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("e.{0} = '{1}'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0}_t.username  ilike '%{1}%'", columnKey, dataStr);
                            }
                            break;
                        }
                    case DynamicProtocolControlType.AreaRegion:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("e.{0} = '{1}'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0}_t.fullname  ilike '%{1}%'", columnKey, dataStr);
                            }
                            break;
                        }
                    case DynamicProtocolControlType.Department:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("e.{0} = '{1}'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0} '%{1}%'", tryParseFieldSearchString(field, "e"), dataStr);//这里应该有问题，要判断多选还是单选
                            }
                            break;
                        }
                    case DynamicProtocolControlType.SelectSingle:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("e.{0} = '{1}'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0}_t.dataval ilike '%{1}%'", columnKey, dataStr);
                            }
                            break;
                        }
                    case DynamicProtocolControlType.SelectMulti:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("e.{0} = '{1}'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0} ilike '%{1}%'", tryParseFieldSearchString(field, "e"), dataStr);//多选有问题
                            }
                            break;
                        }
                    case DynamicProtocolControlType.RecType:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("e.{0} = '{1}'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0}_t.catalogname ilike '%{1}%'", tryParseFieldSearchString(field, "e"), dataStr);
                            }
                            break;
                        }
                    case DynamicProtocolControlType.DataSourceSingle:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("(e.{0}->>'id')='{1}'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0} ilike '%{1}%'", tryParseFieldSearchString(field, "e"), dataStr);
                            }
                            break;
                        }
                    case DynamicProtocolControlType.DataSourceMulti:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("(e.{0}->>'id') in ('{1}')", columnKey, dataStr.Replace(",", "','"));
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0} ilike '%{1}%'", tryParseFieldSearchString(field, "e"), dataStr);
                            }
                            break;
                        }
                    case DynamicProtocolControlType.QuoteControl:
                        {
                            if (field.IsLike == 0)
                            {
                                if (columnKey == "deptgroup")
                                {
                                    result.FieldData = string.Format("crm_func_entity_protocol_format_belongdepartment(e.recmanager) = '{0}'", dataStr);
                                }
                                else if (columnKey == "predeptgroup")
                                {
                                    result.FieldData = string.Format("crm_func_entity_protocol_format_predepartment(e.recmanager) = '{0}'", dataStr);
                                }
                                else
                                {
                                    result.FieldData = string.Format("{0} = '{1}'", tryParseFieldSearchString(field, "e"), dataStr);
                                }
                            }
                            else
                            {
                                if (columnKey == "deptgroup")
                                {
                                    result.FieldData = string.Format("crm_func_entity_protocol_format_belongdepartment(e.recmanager) ilike '%{0}%'", dataStr);
                                }
                                else if (columnKey == "predeptgroup")
                                {
                                    result.FieldData = string.Format("crm_func_entity_protocol_format_predepartment(e.recmanager) ilike '%{0}%'", dataStr);
                                }
                                else
                                {
                                    result.FieldData = string.Format("{0} ilike '%{1}%'", tryParseFieldSearchString(field, "e"), dataStr);
                                }
                            }
                            break;
                        }
                    case DynamicProtocolControlType.ProductSet:
                    case DynamicProtocolControlType.Product:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("e.{0} = '{1}'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0} ilike '%{1}%'", tryParseFieldSearchString(field, "e"), dataStr);
                            }
                            break;
                        }
                    case DynamicProtocolControlType.PersonSelectSingle:
                    case DynamicProtocolControlType.PersonSelectMulti:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("e.{0} = '{1}'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0} ilike '%{1}%'", tryParseFieldSearchString(field, "e"), dataStr);
                            }
                            break;
                        }
                    default:
                        {
                            result.FieldData = " 1=1 ";
                            break;
                        }
                }

                result.IsValid = true;
                validResultDic.Add(field.FieldName, result);
            }
            return validResultDic;
        }
        public static Dictionary<string, DynamicProtocolValidResult> AdvanceQuery(
            List<DynamicEntityFieldSearch> searchFields, Dictionary<string, object> fieldDatas)
        {
            var validResultDic = new Dictionary<string, DynamicProtocolValidResult>();


            foreach (var field in searchFields)
            {
                var checkType = CheckSpecialField(field);
                bool isNameField = false;

                //周报的reportdate控件比较特殊
                var columnKey = field.FieldName;
                if (checkType != field.NewType && field.NewType != 0)
                {
                    columnKey = columnKey + "_name";
                    isNameField = true;
                }

                if (field.NewType == 0)
                {
                    field.NewType = checkType;
                }

                object fieldData;
                fieldDatas.TryGetValue(field.FieldName, out fieldData);

                if (string.IsNullOrWhiteSpace(fieldData?.ToString().Trim()))
                {
                    continue;
                }

                var result = new DynamicProtocolValidResult();
                result.FieldName = field.FieldName;

                var dataStr = fieldData.ToString().Trim();

                //根据控件处理不同的数据格式
                //数字，时间这2种才有范围，其余的是文本ilike
                switch ((DynamicProtocolControlType)field.NewType)
                {
                    case DynamicProtocolControlType.RecOnlive:
                    case DynamicProtocolControlType.RecCreated:
                    case DynamicProtocolControlType.RecUpdated:
                    case DynamicProtocolControlType.NumberInt:
                    case DynamicProtocolControlType.NumberDecimal:
                    case DynamicProtocolControlType.TimeDate:
                    case DynamicProtocolControlType.TimeStamp:
                        {
                            //一个是等于，两个是between
                            var dataArr = dataStr.Split(',');
                            if (dataArr.Length != 2 && dataArr.Length != 1)
                            {
                                result.Tips = "数据格式不对,应为逗号分隔";
                                validResultDic.Add(field.FieldName, result);
                                continue;
                            }
                            if (dataArr.Length == 1)
                            {
                                var conditions = new List<string>();
                                if (!string.IsNullOrWhiteSpace(dataArr[0].Trim()))
                                {
                                    conditions.Add(string.Format("t.{0} = '{1}' ", columnKey, dataArr[0].Trim()));
                                }
                                if (conditions.Count > 0)
                                {
                                    result.FieldData = string.Join(" AND ", conditions.ToArray());
                                }
                            }
                            else {
                                var conditions = new List<string>();
                                if (!string.IsNullOrWhiteSpace(dataArr[0].Trim()))
                                {
                                    conditions.Add(string.Format("t.{0} >= '{1}' ", columnKey, dataArr[0].Trim()));
                                }
                                if (!string.IsNullOrWhiteSpace(dataArr[1].Trim()))
                                {
                                    conditions.Add(string.Format("t.{0} <= '{1}' ", columnKey, dataArr[1].Trim()));
                                }

                                if (conditions.Count > 0)
                                {
                                    result.FieldData = string.Join(" AND ", conditions.ToArray());
                                }
                            }


                            break;
                        }
                    case DynamicProtocolControlType.RecName:
                    case DynamicProtocolControlType.Telephone:
                    case DynamicProtocolControlType.PhoneNum:
                    case DynamicProtocolControlType.Text:
                    case DynamicProtocolControlType.TextArea:
                    case DynamicProtocolControlType.EmailAddr:
                        {
                            var _operator = string.Empty;
                            if (field.IsLike == 0)//是否走模糊查询 1 支持模糊 0不支持
                            {
                                if (isNameField == false)
                                {
                                    result.FieldData = string.Format("t.{0}='{1}'", columnKey, dataStr);
                                }
                                else
                                {
                                    result.FieldData = string.Format("{0}='{1}'", tryParseFieldSearchString(field), dataStr);

                                }
                            }
                            else
                            {
                                if (isNameField == false)
                                {
                                    result.FieldData = string.Format("t.{0} ilike '%{1}%'", columnKey, dataStr);
                                }
                                else
                                {
                                    result.FieldData = string.Format("{0} ilike '%{1}%'", tryParseFieldSearchString(field), dataStr);

                                }
                            }
                            break;
                        }
                    case DynamicProtocolControlType.Address:
                    case DynamicProtocolControlType.Location:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("jsonb_extract_path_text(t.{0}, 'address')='%{1}%'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format("jsonb_extract_path_text(t.{0}, 'address') ilike '%{1}%'", columnKey, dataStr);
                            }
                            break;
                        }
                    case DynamicProtocolControlType.RecCreator:
                    case DynamicProtocolControlType.RecUpdator:
                    case DynamicProtocolControlType.RecManager:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("t.{0} = '{1}'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0} ilike '%{1}%'", tryParseFieldSearchString(field), dataStr);
                            }
                            break;
                        }
                    case DynamicProtocolControlType.AreaRegion:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("t.{0} = '{1}'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0} ilike '%{1}%'", tryParseFieldSearchString(field), dataStr);
                            }
                            break;
                        }
                    case DynamicProtocolControlType.Department:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("t.{0} = '{1}'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0} ilike '%{1}%'", tryParseFieldSearchString(field), dataStr);
                            }
                            break;
                        }
                    case DynamicProtocolControlType.SelectSingle:
                    case DynamicProtocolControlType.SelectMulti:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("t.{0} = '{1}'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0} ilike '%{1}%'", tryParseFieldSearchString(field), dataStr);
                            }
                            break;
                        }
                    case DynamicProtocolControlType.RecType:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("t.{0} = '{1}'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0} ilike '%{1}%'", tryParseFieldSearchString(field), dataStr);
                            }
                            break;
                        }
                    case DynamicProtocolControlType.DataSourceSingle:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("(t.{0}->>'id')='{1}'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0} ilike '%{1}%'", tryParseFieldSearchString(field), dataStr);
                            }
                            break;
                        }
                    case DynamicProtocolControlType.DataSourceMulti:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("(t.{0}->>'id') in ('{1}')", columnKey, dataStr.Replace(",", "','"));
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0} ilike '%{1}%'", tryParseFieldSearchString(field), dataStr);
                            }
                            break;
                        }
                    case DynamicProtocolControlType.QuoteControl:
                        {
                            if (field.IsLike == 0)
                            {
                                if (columnKey == "deptgroup")
                                {
                                    result.FieldData = string.Format("crm_func_entity_protocol_format_belongdepartment(t.recmanager) = '{0}'", dataStr);
                                }
                                else if (columnKey == "predeptgroup")
                                {
                                    result.FieldData = string.Format("crm_func_entity_protocol_format_predepartment(t.recmanager) = '{0}'", dataStr);
                                }
                                else
                                {
                                    result.FieldData = string.Format("{0} = '{1}'", tryParseFieldSearchString(field), dataStr);
                                }
                            }
                            else
                            {
                                if (columnKey == "deptgroup")
                                {
                                    result.FieldData = string.Format("crm_func_entity_protocol_format_belongdepartment(t.recmanager) ilike '%{0}%'", dataStr);
                                }
                                else if (columnKey == "predeptgroup")
                                {
                                    result.FieldData = string.Format("crm_func_entity_protocol_format_predepartment(t.recmanager) ilike '%{0}%'", dataStr);
                                }
                                else
                                {
                                    result.FieldData = string.Format("{0} ilike '%{1}%'", tryParseFieldSearchString(field), dataStr);
                                }
                            }
                            break;
                        }
                    case DynamicProtocolControlType.ProductSet:
                    case DynamicProtocolControlType.Product:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("t.{0} = '{1}'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0} ilike '%{1}%'", tryParseFieldSearchString(field), dataStr);
                            }
                            break;
                        }
                    case DynamicProtocolControlType.PersonSelectSingle:
                    case DynamicProtocolControlType.PersonSelectMulti:
                        {
                            if (field.IsLike == 0)
                            {
                                result.FieldData = string.Format("t.{0} = '{1}'", columnKey, dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format(" {0} ilike '%{1}%'", tryParseFieldSearchString(field), dataStr);
                            }
                            break;
                        }
                    default:
                        {
                            result.FieldData = " 1=1 ";
                            break;
                        }
                }

                result.IsValid = true;
                validResultDic.Add(field.FieldName, result);
            }
            return validResultDic;
        }

        public static Dictionary<string, DynamicProtocolValidResult> SimpleQuery(
    List<DynamicEntityFieldSearch> searchFields, Dictionary<string, object> fieldDatas)
        {
            var validResultDic = new Dictionary<string, DynamicProtocolValidResult>();


            foreach (var field in searchFields.Where(t => fieldDatas.Keys.Contains(t.FieldName)))
            {

                var columnKey = field.FieldName;

                object fieldData;
                fieldDatas.TryGetValue(field.FieldName, out fieldData);

                if (string.IsNullOrWhiteSpace(fieldData?.ToString().Trim()))
                {
                    continue;
                }

                var result = new DynamicProtocolValidResult();
                result.FieldName = field.FieldName;

                var dataStr = fieldData.ToString().Trim();

                //根据控件处理不同的数据格式
                //数字，时间这2种才有范围，其余的是文本ilike
                switch ((DynamicProtocolControlType)field.NewType)
                {
                    case DynamicProtocolControlType.RecName:
                    case DynamicProtocolControlType.Text:
                    case DynamicProtocolControlType.TextArea:
                        {
                            result.FieldData = string.Format("e.{0} ilike '%{1}%'", columnKey, dataStr);
                            break;
                        }
                    case DynamicProtocolControlType.Address:
                    case DynamicProtocolControlType.Location:
                        {
                            result.FieldData = string.Format("jsonb_extract_path_text(e.{0}, 'address') ilike '%{1}%'", columnKey, dataStr);
                            break;
                        }
                    case DynamicProtocolControlType.RecCreator:
                    case DynamicProtocolControlType.PersonSelectMulti:
                    case DynamicProtocolControlType.PersonSelectSingle:
                    case DynamicProtocolControlType.RecUpdator:
                    case DynamicProtocolControlType.RecManager:
                    case DynamicProtocolControlType.AreaRegion:
                    case DynamicProtocolControlType.Department:
                    case DynamicProtocolControlType.SelectSingle:
                    case DynamicProtocolControlType.SelectMulti:
                    case DynamicProtocolControlType.RecType:
                    case DynamicProtocolControlType.DataSourceSingle:
                    case DynamicProtocolControlType.DataSourceMulti:
                    case DynamicProtocolControlType.Product:
                    case DynamicProtocolControlType.ProductSet:
                        {
                            result.FieldData = string.Format("{0} ilike '%{1}%'", tryParseFieldSearchString(field), dataStr);
                            break;
                        }
                    case DynamicProtocolControlType.QuoteControl:
                        {
                            if (columnKey == "deptgroup")
                            {
                                result.FieldData = string.Format("crm_func_entity_protocol_format_belongdepartment(e.recmanager) ilike '%{0}%'", dataStr);
                            }
                            else if (columnKey == "")
                            {
                                result.FieldData = string.Format("crm_func_entity_protocol_format_predepartment(e.recmanager) ilike '%{0}%'", dataStr);
                            }
                            else
                            {
                                result.FieldData = string.Format("{0} ilike '%{1}%'", tryParseFieldSearchString(field), dataStr);
                            }
                            break;
                        }
                    default:
                        {
                            result.FieldData = string.Format("e.{0} ilike '%{1}%'", columnKey, dataStr);
                            break;
                        }
                }

                result.IsValid = true;
                validResultDic.Add(field.FieldName, result);
            }
            return validResultDic;
        }

        public static int CheckSpecialField(DynamicEntityFieldSearch field)
        {
            if (field.FieldName.Equals("reportdate"))
            {
                if (field.FieldId.ToString().Equals("65a2a107-68c0-438c-a6f8-ffe09454a4fc"))
                {
                    field.NewType = 0;
                    return (int)DynamicProtocolControlType.TimeDate;
                }
            }

            return field.ControlType;
        }
        public static string tryParseFieldSearchString(DynamicEntityFieldSearch field, string tablealias = "t")
        {
            if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.QuoteControl)
            {
                if (field.FieldName == "deptgroup")
                {
                    return string.Format("crm_func_entity_protocol_format_belongdepartment({0}.recmanager) ", tablealias);
                }
                else if (field.FieldName == "predeptgroup")
                {
                    return string.Format("crm_func_entity_protocol_format_predepartment({0}.recmanager) ", tablealias);
                }
                else
                {
                    return string.Format("crm_func_entity_protocol_format_quote_control(row_to_json({2}),'{0}','{1}') ", field.EntityId, field.FieldId, tablealias);
                }
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.RecCreator)
            {
                return string.Format("crm_func_entity_protocol_format_userinfo({1}.{0}) ", field.FieldName, tablealias);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.SalesStage)
            {
                return string.Format("crm_func_entity_protocol_format_salesstage({1}.{0}) ", field.FieldName, tablealias);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.RecUpdator)
            {
                return string.Format("crm_func_entity_protocol_format_userinfo({1}.{0}) ", field.FieldName, tablealias);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.RecManager)
            {
                return string.Format("crm_func_entity_protocol_format_userinfo({1}.{0}) ", field.FieldName, tablealias);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.DataSourceSingle)
            {
                return string.Format("crm_func_entity_protocol_format_ds({1}.{0}) ", field.FieldName, tablealias);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.PersonSelectSingle)
            {
                return string.Format("crm_func_entity_protocol_format_userinfo_multi({1}.{0}) ", field.FieldName, tablealias);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.PersonSelectMulti)
            {
                return string.Format("crm_func_entity_protocol_format_userinfo_multi({1}.{0}) ", field.FieldName, tablealias);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.RecStatus)
            {
                return string.Format("crm_func_entity_protocol_format_recstatus({1}.{0}) ", field.FieldName, tablealias);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.TimeDate)
            {
                return string.Format("crm_func_entity_protocol_format_time({1}.{0},'YYYY-MM-DD') ", field.FieldName, tablealias);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.TimeStamp)
            {
                return string.Format("crm_func_entity_protocol_format_time({1}.{0},'YYYY-MM-DD HH24:MI:SS') ", field.FieldName, tablealias);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.RecCreated)
            {
                return string.Format("crm_func_entity_protocol_format_time({1}.{0},'YYYY-MM-DD HH24:MI:SS') ", field.FieldName, tablealias);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.RecUpdated)
            {
                return string.Format("crm_func_entity_protocol_format_time({1}.{0},'YYYY-MM-DD HH24:MI:SS') ", field.FieldName, tablealias);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.Department)
            {
                return string.Format("crm_func_entity_protocol_format_dept_multi({1}.{0}::TEXT) ", field.FieldName, tablealias);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.Address)
            {
                return string.Format("crm_func_entity_protocol_format_address({1}.{0}) ", field.FieldName, tablealias);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.Location)
            {
                return string.Format("crm_func_entity_protocol_format_address({1}.{0}) ", field.FieldName, tablealias);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.RecType)
            {
                return string.Format("crm_func_entity_protocol_format_rectype({1}.{0}) ", field.FieldName, tablealias);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.Product)
            {
                return string.Format("crm_func_entity_protocol_format_product_multi({1}.{0}) ", field.FieldName, tablealias);
            }

            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.ProductSet)
            {
                return string.Format("crm_func_entity_protocol_format_productserial_multi(t.{0}) ", field.FieldName, tablealias);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.SelectSingle || (DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.SelectMulti)
            {
                if (!string.IsNullOrEmpty(field.FieldConfig))
                {
                    JObject jo = JObject.Parse(field.FieldConfig);
                    if (jo["dataSource"] == null) throw new Exception("字段FieldConfig异常");
                    jo = JObject.Parse(jo["dataSource"].ToString());
                    int dataId = Convert.ToInt32(jo["sourceId"].ToString());
                    return string.Format("crm_func_entity_protocol_format_dictionary({0},{2}.{1}::text) ", dataId, field.FieldName, tablealias);
                }

                return field.FieldName;
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.AreaRegion)
            {
                return string.Format("crm_func_entity_protocol_format_region({1}.{0}) ", field.FieldName, tablealias);
            }
            return string.Format("{1}.{0}", field.FieldName, tablealias);
        }
        public static void FormatDateTimeFieldInList(List<Dictionary<string, object>> datas, DynamicEntityFieldSearch fieldInfo)
        {
            if (fieldInfo.ControlType != (int)EntityFieldControlType.TimeDate
                && fieldInfo.ControlType != (int)EntityFieldControlType.TimeStamp
                && fieldInfo.ControlType != (int)EntityFieldControlType.RecCreated
                && fieldInfo.ControlType != (int)EntityFieldControlType.RecUpdated)
                return;
            bool isNeedFormat = false;
            string formatString = "";
            Dictionary<string, object> fieldConfigDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(fieldInfo.FieldConfig);
            if (fieldConfigDict != null
                && fieldConfigDict.ContainsKey("format")
                && fieldConfigDict["format"] != null)
            {
                isNeedFormat = true;
                formatString = fieldConfigDict["format"].ToString();
            }
            if (!isNeedFormat) return;
            if (formatString == null || formatString.Length == 0) return;
            DateTime tmpDateTime;
            foreach (Dictionary<string, object> data in datas)
            {
                if (data.ContainsKey(fieldInfo.FieldName) && data[fieldInfo.FieldName] != null)
                {
                    if (DateTime.TryParse(data[fieldInfo.FieldName].ToString(), out tmpDateTime))
                    {
                        data[fieldInfo.FieldName + "_name"] = tmpDateTime.ToString(formatString);
                    }
                }
            }
        }
        /// <summary>
        /// 格式化数字类型的返回值
        /// </summary>
        /// <param name="dynamicModel"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public static void FormatNumericFieldInList(List<Dictionary<string, object>> datas, DynamicEntityFieldSearch fieldInfo)
        {
            if (fieldInfo.ControlType != (int)EntityFieldControlType.NumberDecimal
                && fieldInfo.ControlType != (int)EntityFieldControlType.NumberInt)
                return;
            bool isNeedFormat = false;
            Dictionary<string, object> fieldConfigDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(fieldInfo.FieldConfig);
            if (fieldConfigDict != null
                && fieldConfigDict.ContainsKey("separator")
                && fieldConfigDict["separator"] != null
                && fieldConfigDict["separator"].ToString().Equals("1"))
            {
                isNeedFormat = true;
            }
            if (!isNeedFormat) return;
            int numCount = 0;
            if (fieldInfo.ControlType == (int)EntityFieldControlType.NumberDecimal
                && fieldConfigDict != null
                && fieldConfigDict.ContainsKey("decimalsLength")
                && fieldConfigDict["decimalsLength"] != null)
            {
                Int32.TryParse(fieldConfigDict["decimalsLength"].ToString(), out numCount);
            }
            Decimal tmpResult = -1;
            foreach (Dictionary<string, object> data in datas)
            {
                if (data.ContainsKey(fieldInfo.FieldName) && data[fieldInfo.FieldName] != null)
                {
                    if (Decimal.TryParse(data[fieldInfo.FieldName].ToString(), out tmpResult))
                    {
                        data[fieldInfo.FieldName] = String.Format("{0:N" + numCount.ToString() + "}", tmpResult);
                    }
                }
            }
        }
        public static void FormatNumericFieldInList(List<IDictionary<string, object>> datas, DynamicEntityFieldSearch fieldInfo)
        {
            if (fieldInfo.ControlType != (int)EntityFieldControlType.NumberDecimal
                && fieldInfo.ControlType != (int)EntityFieldControlType.NumberInt)
                return;
            bool isNeedFormat = false;
            Dictionary<string, object> fieldConfigDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(fieldInfo.FieldConfig);
            if (fieldConfigDict != null
                && fieldConfigDict.ContainsKey("separator")
                && fieldConfigDict["separator"] != null
                && fieldConfigDict["separator"].ToString().Equals("1"))
            {
                isNeedFormat = true;
            }
            if (!isNeedFormat) return;
            int numCount = 0;
            if (fieldInfo.ControlType == (int)EntityFieldControlType.NumberDecimal
                && fieldConfigDict != null
                && fieldConfigDict.ContainsKey("decimalsLength")
                && fieldConfigDict["decimalsLength"] != null)
            {
                Int32.TryParse(fieldConfigDict["decimalsLength"].ToString(), out numCount);
            }
            Decimal tmpResult = -1;
            foreach (IDictionary<string, object> data in datas)
            {
                if (data.ContainsKey(fieldInfo.FieldName) == false  || data[fieldInfo.FieldName] == null) continue;
                if (Decimal.TryParse(data[fieldInfo.FieldName].ToString(), out tmpResult))
                {
                    data[fieldInfo.FieldName] = String.Format("{0:N" + numCount.ToString() + "}", tmpResult);
                }
            }
        }
    }
}