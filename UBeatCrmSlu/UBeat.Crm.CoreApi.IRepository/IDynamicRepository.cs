using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Dynamics;
using UBeat.Crm.CoreApi.DomainModel.Message;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IDynamicRepository
    {
        //OperateResult SaveDynamicTemplate(DynamicTemplateInsert data);

        //dynamic SelectDynamicTemplate(DynamicTemplateSelect data);

        //OperateResult DeleteDynamicTemplate(Guid entityid, int usrno);

        OperateResult InsertDynamicAbstract(DynamicAbstractInsert data);

        dynamic SelectDynamicAbstract(DynamicAbstractSelect data);

        /// <summary>
        /// 获取动态摘要的详情数据
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="typeID"></param>
        /// <param name="userNo"></param>
        /// <returns></returns>
        List<DynamicAbstractInfo> GetDynamicAbstract(Guid entityID, Guid typeID);

        /// <summary>
        /// 获取动态模板内容
        /// </summary>
        /// <param name="entityid"></param>
        /// <param name="typeid"></param>
        /// <returns></returns>
        string GetDynamicTemplate(Guid entityid, Guid typeid);
        /// <summary>
        /// 获取动态模板生成的数据
        /// </summary>
        /// <param name="recid"></param>
        /// <param name="entityID"></param>
        /// <param name="typeID"></param>
        /// <param name="userno"></param>
        /// <returns></returns>
        string GetDynamicTemplateData(Guid recid, Guid entityID, Guid typeID, int userno);

        OperateResult InsertDynamic(DynamicInsert data);

        bool InsertDynamic(DbTransaction tran, DynamicInsertInfo data,int userNumber, out MsgParamInfo tempcontent);

        OperateResult DeleteDynamic(DynamicDelete data);

        dynamic SelectDynamic(PageParam pageParam, DynamicSelect data);
        DynamicInfoModel SelectDynamic(Guid dynamicid, int usernumber);

        /// <summary>
        /// 增量获取动态列表
        /// </summary>
        /// <param name="param"></param>
        /// <param name="incrementPage"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        IncrementPageDataInfo<DynamicInfoExt> GetDynamicInfoList(DynamicListParameter param,IncrementPageParameter incrementPage, int userNumber);
        /// <summary>
        /// 分页获取动态列表
        /// </summary>
        /// <param name="param"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        PageDataInfo<DynamicInfoExt> GetDynamicInfoList(DynamicListParameter param, int pageIndex, int pageSize, int userNumber);

        DynamicInfo GetDynamicInfo(Guid dynamicid);

        OperateResult AddDynamicComments(DynamicCommentsInsert data);

        Guid AddDynamicComments(DynamicCommentParameter data,int userNumber);

        //dynamic SelectDynamicComments(DynamicCommentsSelect data);

        OperateResult DeleteDynamicComments(Guid commentsid, int usrno);

        bool AddDynamicPraise(Guid dynamicid, int userno);
        OperateResult AddDynamicPraise(DynamicPraiseMapper data);

        //dynamic SelectDynamicPraise(DynamicPraise data);

        OperateResult DeleteDynamicPraise(DynamicPraiseMapper data);


        bool MergeDynamic(DbTransaction tran, Guid entityid, Guid businessid, List<Guid> beMergeBusinessids, int usernumber);
        bool TransferEntityData(DbTransaction tran, string tableName, List<string> fieldNames, int newUserId,Guid recId, int userId);
    }
}

