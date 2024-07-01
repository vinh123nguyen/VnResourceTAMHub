using System.Text.RegularExpressions;

namespace TAMHubApp.Infrastructures.TAMUtilities
{
    public class GlobalUtilities
    {
        public static bool IsCorrenctIP(string ip)
        {
            bool result = false;
            if (Regex.IsMatch(ip, "[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}"))
            {
                string[] ips = ip.Split('.');
                if (4 == ips.Length)
                {
                    if (int.Parse(ips[0]) < 256 && int.Parse(ips[1]) < 256
                        && int.Parse(ips[2]) < 256 && int.Parse(ips[3]) < 256)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }
    }
}
