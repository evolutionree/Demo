using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using UBeat.Crm.CoreApi.Core.Utility;

/// <summary>
/// 读取配置文件
/// </summary>
public class AppConfigurtaionServices
{
    public static IConfigurationRoot configuration = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
}