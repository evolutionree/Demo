using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Controllers;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.ZGQY.Model;
using UBeat.Crm.CoreApi.ZGQY.Services;

namespace UBeat.Crm.CoreApi.ZGQY.Controllers
{
	[Route("api/zgqy/[controller]")]
	public class DataPlatformController: BaseController
	{
		private static readonly Logger Logger = LogManager.GetLogger(typeof(AttendanceController).FullName);

		private readonly DataPlatformServices _dataPlatformServices;

		public DataPlatformController(DataPlatformServices dataPlatformServices)
		{
			_dataPlatformServices = dataPlatformServices;
		}

		[Route("test")]
		[HttpPost]
		[AllowAnonymous]
		public OutputResult<object> SapTest([FromBody] DataPlatformInfo paramInfo = null)
		{
			if (paramInfo == null)
			{
				return ResponseError<object>("参数异常");
			}

			var data = _dataPlatformServices.InitProjectData(true);
			return new OutputResult<object>("test");
		}
	}
}
