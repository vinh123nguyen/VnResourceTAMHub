using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAMHubApp.Infrastructures.Enums
{
    public enum ZKTecoINOUTMode
    {
        E_IN = 0,
        E_OUT = 1,
        E_BREAK_OUT = 2,
        E_BREAK_IN = 3,
        E_OT_IN = 4,
        E_OT_OUT = 5
    }
    public enum ZKTecoVerifyMode
    {
        FP_OR_PW_OR_RF = 0, //password verification
        FP = 1,  //fingerprint verification
        PIN = 2,  //card verification
        PW = 3,
        RF = 4,
        FP_OR_PW = 5,
        FP_OR_RF = 6,
        PW_OR_RF = 7,
        PIN_AND_FP = 8,
        FP_AND_PW = 9,
        FP_AND_RF = 10,
        PW_AND_RF = 11,
        FP_AND_PW_AND_RF = 12,
        PIN_AND_FP_AND_PW = 13,
        FP_AND_RF_OR_PIN = 14
    }
}
