using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.QRCode;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IQRCodeRepository
    {
        /// <summary>
        /// 新增QRCode检查规则
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="recName"></param>
        /// <param name="remark"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        Guid Add(DbTransaction tran, string recName, string remark ,int userid);

        /// <summary>
        /// 获取最大的序号
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        int GetMaxOrderId(DbTransaction tran, int userid);
        /// <summary>
        /// 修改规则基本信息
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="recId"></param>
        /// <param name="recName"></param>
        /// <param name="remark"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        bool Save(DbTransaction tran, Guid recId, string recName, string remark, int userid);

        /// <summary>
        /// 获取匹配规则
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="recId"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        Dictionary<string, object> GetMatchParamInfo(DbTransaction tran, Guid recId, int userid);

        /// <summary>
        /// 更新匹配规则
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="recId"></param>
        /// <param name="checkType"></param>
        /// <param name="checkParam"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        bool UpdateMatchParamInfo(DbTransaction tran, Guid recId, QRCodeCheckTypeEnum checkType, QRCodeCheckMatchParamInfo checkParam, int userid);

        /// <summary>
        /// 获取处理规则
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="recId"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        Dictionary<string, object> GetDealParamInfo(DbTransaction tran, Guid recId, int userid);

        /// <summary>
        /// 保存处理规则
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="recid"></param>
        /// <param name="dealType"></param>
        /// <param name="dealParmInfo"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        bool UpdateDealParamInfo(DbTransaction tran, Guid recid, QRCodeCheckTypeEnum dealType, QRCodeDealParamInfo dealParmInfo, int userid);


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="recId"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        QRCodeEntryItemInfo GetFullInfo(DbTransaction tran, Guid recId, int userid);

        /// <summary>
        /// 列表
        /// 必须全部返回
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="showDisabled"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        List<QRCodeEntryItemInfo> ListRules(DbTransaction tran, bool showDisabled, int userid);
        /// <summary>
        /// 删除规则，真正物理删除
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="recId"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        bool Delete(DbTransaction tran, List<Guid> recId, int userid);

        /// <summary>
        /// 修改状态，1=启用，0=禁用
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="recId"></param>
        /// <param name="status"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        bool SetStatus(DbTransaction tran, List<Guid> recIds, int status, int userid);

        bool OrderRules(DbTransaction tran, List<Guid> recids, int userid);

    }
}
