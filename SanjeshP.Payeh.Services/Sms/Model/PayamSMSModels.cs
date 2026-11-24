using System;
using System.Collections.Generic;
using System.Text;

namespace SanjeshP.Payeh.Services.Sms.Model
{
    public class PayamSMSModels
    {
        public class SmsApi_Authentication
        {
            public string UserName { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class VerifyResul
        {
            public bool result { get; set; }
            public VerifyResulData? data { get; set; }
        }

        public class VerifyResulData
        {
            public string tokenValue { get; set; } = string.Empty;
            public string expires { get; set; } = string.Empty;
            public string tokenType { get; set; } = string.Empty;
        }

        public class SendSMS
        {
            public string sender_number { get; set; } = string.Empty;
            public string receiver_number { get; set; } = string.Empty;
            public string note_arr { get; set; } = string.Empty;
            public string date { get; set; } = string.Empty;
            public string flash { get; set; } = string.Empty;
            public string onlysend { get; set; } = string.Empty;
        }

        public class ResultSendsmsModel
        {
            public string Version { get; set; } = string.Empty;
            public int StatusCode { get; set; }
            public bool IsError { get; set; }
            public string Message { get; set; } = string.Empty;
            public ResultModel? Result { get; set; }
        }

        public class ResultModel
        {
            public bool result { get; set; }
            public String? failureReason { get; set; }
            public int status { get; set; }
            public string sender_number { get; set; } = string.Empty;
            public string receiver_number { get; set; } = string.Empty;
            public string[]? trackings { get; set; }
        }
    }
}
