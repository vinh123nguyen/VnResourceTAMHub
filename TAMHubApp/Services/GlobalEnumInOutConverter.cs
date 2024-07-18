using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Xaml;
using TAMHubApp.Data;
using TAMHubApp.Infrastructures.Enums;

namespace TAMHubApp.Services
{
    public class GlobalEnumInOutConverter
    {
        public GlobalEnumInOutConverter()
        {
            InitUpdateEnumConverter().Wait();
        }
        public static IDictionary<int, string> ZKEnumConverter { get; set; } = new Dictionary<int, string>();
        public static IDictionary<int, string> RealandEnumConverter { get; set; } = new Dictionary<int, string>();
        public static async Task InitUpdateEnumConverter(string enumtype = "")
        {
            ZKEnumConverter.Clear();
            var unitOfWork = ServiceLocator.Instance.GetRequiredService<IUnitOfWork>();

            var listData = await unitOfWork.AppSettingRepository.GetAllAsync();

            if(string.IsNullOrEmpty(enumtype) || enumtype == "ZKTeco")
            {
                #region ZKTeco
                var zkInKey = listData.FirstOrDefault(x => x.Name == ConfigEnumConstant.ENUM_INOUT_ZKTECO_IN_KEY.ToString());
                var zkOutKey = listData.FirstOrDefault(x => x.Name == ConfigEnumConstant.ENUM_INOUT_ZKTECO_OUT_KEY.ToString());

                var zkInItem = listData.Where(x => x.Name == ConfigEnumConstant.ENUM_INOUT_ZKTECO_IN_ITEM.ToString());
                var zkOutItem = listData.Where(x => x.Name == ConfigEnumConstant.ENUM_INOUT_ZKTECO_OUT_ITEM.ToString());

                if (zkInKey != null && !string.IsNullOrEmpty(zkInKey.Value) && zkInItem != null && zkInItem.Any())
                {
                    foreach (var item in zkInItem)
                    {
                        if (!int.TryParse(item.Value, out int num))
                            continue;
                        ZKEnumConverter.Add(num, zkInKey.Value);
                    }
                }
                if (zkOutKey != null && !string.IsNullOrEmpty(zkOutKey.Value) && zkOutItem != null && zkOutItem.Any())
                {
                    foreach (var item in zkOutItem)
                    {
                        if (!int.TryParse(item.Value, out int num))
                            continue;
                        ZKEnumConverter.Add(num, zkOutKey.Value);
                    }
                }
                #endregion
            }

            if (string.IsNullOrEmpty(enumtype) || enumtype == "Realand")
            {
                #region ZKTeco
                var zkInKey = listData.FirstOrDefault(x => x.Name == ConfigEnumConstant.ENUM_INOUT_REALAND_IN_KEY.ToString());
                var zkOutKey = listData.FirstOrDefault(x => x.Name == ConfigEnumConstant.ENUM_INOUT_REALAND_OUT_KEY.ToString());

                var zkInItem = listData.Where(x => x.Name == ConfigEnumConstant.ENUM_INOUT_REALAND_IN_ITEM.ToString());
                var zkOutItem = listData.Where(x => x.Name == ConfigEnumConstant.ENUM_INOUT_REALAND_OUT_ITEM.ToString());

                if (zkInKey != null && !string.IsNullOrEmpty(zkInKey.Value) && zkInItem != null && zkInItem.Any())
                {
                    foreach (var item in zkInItem)
                    {
                        if (!int.TryParse(item.Value, out int num))
                            continue;
                        ZKEnumConverter.Add(num, zkInKey.Value);
                    }
                }
                if (zkOutKey != null && !string.IsNullOrEmpty(zkOutKey.Value) && zkOutItem != null && zkOutItem.Any())
                {
                    foreach (var item in zkOutItem)
                    {
                        if (!int.TryParse(item.Value, out int num))
                            continue;
                        ZKEnumConverter.Add(num, zkOutKey.Value);
                    }
                }
                #endregion
            }


        }

    }
}
