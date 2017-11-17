using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.DynamicEntity
{
    public class EntityMemberModel
    {
        private string recName;
        public int RecCreateUserId { set; get; }
        public string RecName
        {
            set { recName = value; }
            get
            {
                if (string.IsNullOrEmpty(recName))
                {
                    return RecCode;
                }
                else return recName;
            }
        }

        public string RecCode { set; get; }

        public int RecManager { set; get; }
        public List<int> ViewUsers { set; get; } = new List<int>();
        /// <summary>
        /// 抄送人
        /// </summary>
        public List<int> CopyUsers { set; get; } = new List<int>();


        public List<int> FollowUsers { set; get; } = new List<int>();

        public List<int> Members
        {
            get
            {
                var viewUsers = new List<int>(ViewUsers);
                if (RecManager > 0)
                    viewUsers.Add(RecManager);

                if (FollowUsers.Count > 0) {

                    viewUsers.AddRange(FollowUsers);
                }
                
                return viewUsers.Distinct().ToList();
            }
        }
    }
}
