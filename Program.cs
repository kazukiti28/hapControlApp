using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Codeplex.Data;
using System.IO;

namespace hapControlApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri playUrl = new Uri("http://192.168.0.2:60200/sony/hap?target=keyevent&cmd=play");
            Uri backUrl = new Uri("http://192.168.0.2:60200/sony/avContent");//曲戻しはここ宛
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Host = backUrl.Host;//本当はここから
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            client.DefaultRequestHeaders.Add("Referer", "http://192.168.0.2:60100/HAP_app.html");
            client.DefaultRequestHeaders.Add("Accept-Language", "gzip, deflate");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "ja,en-US;q=0.8,en;q=0.6");
            client.DefaultRequestHeaders.Add("Origin", "http://192.168.0.2:60100");//ここまで要らない
            client.DefaultRequestHeaders.ExpectContinue = false;//これは絶対いる
            var obj = new//JSON生成
            {
                @params = new[] { new { } },
                method = "setPlayPreviousContent",
                version = "1.0",
                id = 1,
            };
            var json = DynamicJson.Serialize(obj);//シリアライズ
            var postData = new StringContent(json);
            StringContent theContent = new StringContent(json, Encoding.UTF8, "application/x-www-form-urlencoded");
            var result = client.PostAsync(backUrl, theContent).Result.Content.ReadAsStringAsync().Result;//送信はここ
            Console.WriteLine(result);
            Console.ReadKey();
        }
    }
}
