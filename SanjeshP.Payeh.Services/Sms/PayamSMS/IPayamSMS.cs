using System;
using System.Collections.Generic;
using System.Text;

namespace SanjeshP.Payeh.Services.Sms.PayamSMS
{
    public interface IPayamSMS
    {
        Task<int> Send(string _number, string _message);
    }
}
