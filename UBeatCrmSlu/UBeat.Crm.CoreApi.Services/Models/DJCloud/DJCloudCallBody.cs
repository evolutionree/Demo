using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.DJCloud
{
    public class DJCloudCallBody    {        public AppInfo Info { get; set; }        public Subject Subject { get; set; }        public string Timestamp { get; set; }    }    public class AppInfo    {        public string AppId { get; set; }    }    public class Subject    {        public string Caller { get; set; }        public string CallerDisplay { get; set; }        public string CallerFilename { get; set; }        public string Called { get; set; }        public string CalledDisplay { get; set; }        public string CalledFilename { get; set; }        public string Data { get; set; }    }
}
