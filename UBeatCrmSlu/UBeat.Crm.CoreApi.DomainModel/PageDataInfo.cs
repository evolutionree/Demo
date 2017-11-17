using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel
{
    public class PageDataInfo<T>
    {
        public List<T> DataList { set; get; }

        public PageInfo PageInfo { set; get; } = new PageInfo();

    }

   
   

    public class PageInfo
    {
        public long TotalCount { set; get; }

        public int PageCount
        {
            get
            {
                return (int)((TotalCount - 1) / PageSize + 1);
            }
        }

        public int PageSize { get => pageSize <= 0 ? 1 : pageSize; set => pageSize = value; }

        int pageSize = 1;

    }
}
