namespace TAMHubApp.Infrastructures.Enums
{
    public class Global
    {
        private Global() { }

        /// <summary>
        /// 设备处于繁忙状态
        /// </summary>
        public const long DeviceBusy = 1;

        /// <summary>
        /// 设备处于空闲状态
        /// </summary>
        public const long DeviceIdle = 0;
    }

    public class RealandObjectConvert
    {
        public static string IOMode(int mode)
        {
            string message = string.Empty;

            switch (mode)
            {
                case 0:
                    message = "Check in";
                    break;
                case 1:
                    message = "Clock in";
                    break;
                case 2:
                    message = "Clock out";
                    break;
                case 3:
                    message = "Customer in";
                    break;
                case 4:
                    message = "Customer out";
                    break;
                case 5:
                    message = "Out";
                    break;
                case 6:
                    message = "In";
                    break;
                case 7:
                    message = "User defined 1";
                    break;
                case 8:
                    message = "User defined 2";
                    break;
                case 12:
                    message = "Button open";
                    break;
                case 13:
                    message = "Software open";
                    break;
                case 14:
                    message = "Keep open";
                    break;
                case 15:
                    message = "Keep close";
                    break;
                case 16:
                    message = "Auto mode";
                    break;
                case 17:
                    message = "Open in";
                    break;
                case 18:
                    message = "Open out";
                    break;
                case 19:
                    message = "Overtime open alarm";
                    break;
                case 20:
                    message = "Forced open alarm";
                    break;
                case 21:
                    message = "Antihijack alarm";
                    break;
                case 22:
                    message = "Input action 1";
                    break;
                case 23:
                    message = "Input action 2";
                    break;
                case 24:
                    message = "Output action 1";
                    break;
                case 25:
                    message = "Output action 2";
                    break;
                case 26:
                    message = "Invalid time";
                    break;
                case 27:
                    message = "Invalid date";
                    break;
                case 31:
                    message = "Illegal operation";
                    break;
            }
            return message;
        }
    }
}
