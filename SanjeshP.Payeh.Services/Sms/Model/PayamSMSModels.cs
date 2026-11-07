using System;
using System.Collections.Generic;
using System.Text;

namespace SanjeshP.Payeh.Services.Sms.Model
{
    public class PayamSMSModels
    {
        public class SmsApi_Authentication
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        public class VerifyResul
        {
            public bool result { get; set; }
            public VerifyResulData data { get; set; }
        }

        public class VerifyResulData
        {
            public string tokenValue { get; set; }
            public string expires { get; set; }
            public string tokenType { get; set; }
        }

        public class SendSMS
        {
            public string sender_number { get; set; }
            public string receiver_number { get; set; }
            public string note_arr { get; set; }
            public string date { get; set; }
            public string flash { get; set; }
            public string onlysend { get; set; }
        }

        public class ResultSendsmsModel
        {
            public string Version { get; set; }
            public int StatusCode { get; set; }
            public bool IsError { get; set; }
            public string Message { get; set; }
            public ResultModel Result { get; set; }
        }

        public class ResultModel
        {
            public bool result { get; set; }
            public String failureReason { get; set; }
            public int status { get; set; }
            public string sender_number { get; set; }
            public string receiver_number { get; set; }
            public string[] trackings { get; set; }
        }
    }
}
