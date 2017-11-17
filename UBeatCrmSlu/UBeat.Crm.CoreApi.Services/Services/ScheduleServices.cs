using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using UBeat.Crm.CoreApi.Services.Models.Reminder;
using UBeat.Crm.CoreApi.Core.Utility;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;


namespace UBeat.Crm.CoreApi.Services.Services
{

    public static class ScheduleServices
    {

        public static string ScheduleServerHost;
        public static ScheduleSetting _scheduleSetting;

        public const string DefaultAssemblyName = "UBeat.Crm.Schedule.Job";
        public const string CustomerTipsJobType = "RemindEventMessageJob";
        public const string CustomerTipsJobName = "RemindEventMessageJob,{Default},{JobName}-{ClientId}-0-{MessageId},{ClientId}";

        static ScheduleServices()
        {
            IConfigurationRoot config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
            _scheduleSetting = config.GetSection("ScheduleSetting").Get<ScheduleSetting>();

            //读取作业调度服务的配置
            ScheduleServerHost = _scheduleSetting.TaskJobServer;

        }


        public static List<string> JobNameSplitor(string jobName)
        {
            return jobName.Split(',').ToList();
        }

        public static TaskJobModel CreateCustomReminder(string jobName, string eventTitle, string enterpriseNo, string eventId,string cronString)
        {
            var jobNameArr = JobNameSplitor(jobName);

            var model = new TaskJobModel()
            {
                Assembly = jobNameArr[1].Equals("{Default}") ? DefaultAssemblyName : jobNameArr[1],
                Type = jobNameArr[0],
                ClientId = enterpriseNo,
                CronStrings = new List<string>() { cronString },
                JobData = new Dictionary<string, object>() { { "eventId", eventId } },
                MessageId = eventId,
                UserId = 0,
                ReMark = "自定义事件提醒:" + eventTitle,

            };
            return model;
        }


        public static TaskJobFullNameModel Creator(string jobName, string enterpriseNo, string eventid, string userNumber)
        {
            var jobNameArr = JobNameSplitor(jobName);
            TaskJobFullNameModel model = new TaskJobFullNameModel()
            {
                Assembly = jobNameArr[1].Equals("{Default}") ? DefaultAssemblyName : jobNameArr[1],
                Type = jobNameArr[0],
                JobName = jobNameArr[2]
                .Replace("{ClientId}", enterpriseNo)
                .Replace("{JobName}", jobNameArr[0])
                .Replace("{MessageId}", eventid)
                .Replace("{UserId}", userNumber),
                JobGroup = jobNameArr[3].Replace("{ClientId}", enterpriseNo)

            };
            return model;
        }


        public static RemindResultWrap AddSchedule(TaskJobModel model)
        {
            var jobApiUrl = "api/job/add";
            var result = ApiPostData(ScheduleServerHost, jobApiUrl, JsonConvert.SerializeObject(model));
            return JsonConvert.DeserializeObject<RemindResultWrap>(result);
        }

        public static RemindResultWrap DelSchedule(TaskJobRemoveModel model)
        {
            var jobApiUrl = "api/job/remove";
            var result = ApiPostData(ScheduleServerHost, jobApiUrl, JsonConvert.SerializeObject(model));
            return JsonConvert.DeserializeObject<RemindResultWrap>(result);
        }

        public static RemindResultWrap DelScheduleWithFullName(List<TaskJobFullNameModel> modelList)
        {
            var jobApiUrl = "api/job/removebyname";
            var result = ApiPostData(ScheduleServerHost, jobApiUrl, JsonConvert.SerializeObject(modelList));
            return JsonConvert.DeserializeObject<RemindResultWrap>(result);
        }

        public static RemindResultWrap ExecuteNow(TaskJobFullNameModel model)
        {
            var jobApiUrl = "api/job/executebyname";
            var result = ApiPostData(ScheduleServerHost, jobApiUrl, JsonConvert.SerializeObject(model));
            return JsonConvert.DeserializeObject<RemindResultWrap>(result);
        }

        public static RemindResultWrap StopJobsWithFullName(List<TaskJobFullNameModel> modelList)
        {
            var jobApiUrl = "api/job/pausebyname";
            var result = ApiPostData(ScheduleServerHost, jobApiUrl, JsonConvert.SerializeObject(modelList));
            return JsonConvert.DeserializeObject<RemindResultWrap>(result);
        }

        public static RemindResultWrap ResumeJobsWithFullName(List<TaskJobFullNameModel> modelList)
        {
            var jobApiUrl = "api/job/resumebyname";
            var result = ApiPostData(ScheduleServerHost, jobApiUrl, JsonConvert.SerializeObject(modelList));
            return JsonConvert.DeserializeObject<RemindResultWrap>(result);
        }

        public static RemindResultWrap ResumeJobsWithAdd(List<TaskJobModel> modelList)
        {
            var jobApiUrl = "api/job/resumewithadd";
            var result = ApiPostData(ScheduleServerHost, jobApiUrl, JsonConvert.SerializeObject(modelList));
            return JsonConvert.DeserializeObject<RemindResultWrap>(result);
        }


        public static RemindResultWrap AddRunOnceSchedule(TaskJobModel model)
        {
            var jobApiUrl = "api/job/addrunonce";
            var result = ApiPostData(ScheduleServerHost, jobApiUrl, JsonConvert.SerializeObject(model));
            return JsonConvert.DeserializeObject<RemindResultWrap>(result);
        }

    
        public static RemindResultWrap DeleteJob(TaskJobFullNameModel model)
        {
            var jobApiUrl = "api/job/delete";
            var result = ApiPostData(ScheduleServerHost, jobApiUrl, JsonConvert.SerializeObject(model));
            return JsonConvert.DeserializeObject<RemindResultWrap>(result);

        }


        


        public static string ApiPostData(string host, string apiUrl, string postData)
        {
            var baseUri = new Uri(host);
            var url = new Uri(baseUri, apiUrl);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";


            //TODO:use asyn method
            if (!string.IsNullOrEmpty(postData))
            {
                using (var streamWriter = new StreamWriter(request.GetRequestStreamAsync().GetAwaiter().GetResult()))
                {
                    streamWriter.Write(postData);
                    streamWriter.Flush();


                    //streamWriter.Close();
                }
            }

            var response = (HttpWebResponse)request.GetResponseAsync().GetAwaiter().GetResult();

            var data = string.Empty;
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        data = streamReader.ReadToEnd();
                    }
                }
            }

            return data;
        }
    }
}
