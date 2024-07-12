using Riss.Devices;
using System.Text;
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

        public static string VerifyType(int opertationType)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                if (0 != Zd2911Utils.BitCheck(opertationType, i))
                {
                    switch (i)
                    {
                        case 0:
                            sb.Append("F ");
                            break;
                        case 1:
                            sb.Append("P ");
                            break;
                        case 2:
                            sb.Append("C ");
                            break;
                        case 3:
                            sb.Append("I ");
                            break;
                    }
                }
            }
            return sb.ToString();
        }
    }
}
