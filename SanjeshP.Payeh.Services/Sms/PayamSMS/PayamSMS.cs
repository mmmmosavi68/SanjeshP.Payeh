using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SanjeshP.Payeh.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using static SanjeshP.Payeh.Services.Sms.Model.PayamSMSModels;

namespace SanjeshP.Payeh.Services.Sms.PayamSMS
{
    public class PayamSMS : IPayamSMS, IScopedDependency
    {
        private readonly SiteSettings _siteSetting;
        public PayamSMS(IOptionsSnapshot<SiteSettings> settings)
        {
            _siteSetting = settings.Value;
        }

        public async Task<int> Send(string _number, string _message)
        {
            try
            {
                // اطلاعات احراز هویت برای دریافت توکن
                //SmsApi_Authentication smsApi_Authentication = new SmsApi_Authentication
                //{
                //    UserName = "AmoozeshModavem",
                //    Password = "hZg7B2C4S!jh"
                //};
                //var res_SMSAuth = await GetToken<VerifyResul>("https://smsapi.behdasht.gov.ir/auth", smsApi_Authentication);

                var res_SMSAuth = await GetToken<VerifyResul>("https://smsapi.behdasht.gov.ir/auth", _siteSetting.payamSMS);

                if (res_SMSAuth == null || res_SMSAuth.data == null)
                    throw new Exception("دریافت توکن با خطا مواجه شد.");

                // ساخت پیامک
                SendSMS sendSMS = new SendSMS
                {
                    sender_number = "98200019018",
                    receiver_number = _number,
                    note_arr = _message,
                    date = "",
                    flash = "",
                    onlysend = ""
                };

                string URL_SendSMS = "https://smsapi.behdasht.gov.ir/api/v1/gwsmsapi/sendsms";
                var res_SendSMS = await SendSMSRequest<ResultSendsmsModel>(URL_SendSMS, sendSMS, res_SMSAuth.data.tokenValue);

                return 0; // موفق
            }
            catch (Exception ex)
            {
                // اینجا می‌تونی لاگ هم بزاری
                return 1; // خطا
            }
        }

        private static async Task<T> GetToken<T>(string apiUrl, object value)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(value);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<T>(responseBody);
                    }

                    return default;
                }
            }
            catch
            {
                throw;
            }
        }

        private static async Task<T> SendSMSRequest<T>(string apiUrl, object value, string token)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var json = JsonConvert.SerializeObject(value);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<T>(responseBody);
                    }

                    return default;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
