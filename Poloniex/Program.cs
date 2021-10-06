using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Poloniex
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                string key = ReadFromConfig("accountKey");
                string secret = ReadFromConfig("accountSecret");
                string url = ReadFromConfig("apiUrl");

                using var httpClient = new HttpClient();

                httpClient.BaseAddress = new Uri(url);
                httpClient.DefaultRequestHeaders.Add("Key", key);

                KeyValuePair<string, string>[] param = {
                    new("command", "returnBalances"),
                    new("nonce", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()),
                };
                FormUrlEncodedContent content = new FormUrlEncodedContent(param);
                string body = await content.ReadAsStringAsync();
                string sign = Sign(secret, body);
                httpClient.DefaultRequestHeaders.Add("Sign", sign);

                var response = await httpClient.PostAsync("", content);
                response.EnsureSuccessStatusCode();

                string result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static string ReadFromConfig(string settingKey)
        {
            string setting = ConfigurationManager.AppSettings[settingKey];
            if (string.IsNullOrWhiteSpace(setting))
            {
                throw new ArgumentNullException(nameof(setting), "не заполнены параметры конфигурации");
            }

            return setting;
        }

        private static string Sign(string secret, string body)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
            using HMACSHA512 hmac = new HMACSHA512(keyBytes);

            byte[] data = Encoding.UTF8.GetBytes(body);
            byte[] hash = hmac.ComputeHash(data);
            string sign = BinaryToHex(hash);

            return sign;
        }

        private static string BinaryToHex(byte[] data)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in data)
            {
                stringBuilder.Append(b.ToString("x2"));
            }

            return stringBuilder.ToString();
        }
    }
}
