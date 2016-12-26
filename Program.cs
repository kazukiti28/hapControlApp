using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Codeplex.Data;

namespace hapControlApp
{
	class Program
	{
		static void Main(string[] args)
		{
            Console.Title = "HAPControleApp";
            inputCmd();
		}

        static void inputCmd()
        {
            string cmd = Console.ReadLine();
            setJunbi(cmd);
        }

		static void connectHap(dynamic json,string url)//帰ってきたJSONが意味をなさないやつはこれ
		{
			HttpClient client = new HttpClient();
			string setUrl = "http://192.168.0.2:60200/sony/" + url;
			Uri Url = new Uri(setUrl);
			client.DefaultRequestHeaders.Host = Url.Host;//ここから
			client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
			client.DefaultRequestHeaders.Add("Referer", "http://192.168.0.2:60100/HAP_app.html");
			client.DefaultRequestHeaders.Add("Accept-Language", "gzip, deflate");
			client.DefaultRequestHeaders.Add("Accept-Encoding", "ja,en-US;q=0.8,en;q=0.6");
			client.DefaultRequestHeaders.Add("Origin", "http://192.168.0.2:60100");//ここまで要らない
			client.DefaultRequestHeaders.ExpectContinue = false;//これは絶対いる
			StringContent theContent = new StringContent(json, Encoding.UTF8, "application/x-www-form-urlencoded");
			var result = client.PostAsync(Url, theContent).Result.Content.ReadAsStringAsync().Result;//送信はここ
			Console.WriteLine(result);
            inputCmd();
		}

		static void serializeJson(dynamic obje,string url)
		{
			var json = DynamicJson.Serialize(obje);
			connectHap(json,url);
		}


		static void setJunbi(string mode)
		{
			string url = null;
			if (mode == "previous")//前の曲へ
			{
				var obj = new//JSON生成
				{
					@params = new[] { new { } },
					method = "setPlayPreviousContent",
					version = "1.0",
					id = 1,
				};

				url = "avContent";
				serializeJson(obj,url);
			}
			if (mode == "poweron")//電源オン
			{
				var obj = new//JSON生成
				{
					@params = new[] {
						new {
							status = "active",
							standbyDetail = "",
						} 
					},
					method = "setPowerStatus",
					version = "1.1",
					id = 1,
				};
				url = "system";
				serializeJson(obj,url);
			}
			if (mode == "poweroff")//電源オフ
			{
				var obj = new//JSON生成
				{
					@params = new[] {
						new {
							status = "off",
							standbyDetail = "",
						}
					},
					method = "setPowerStatus",
					version = "1.1",
					id = 1,
				};
				url = "system";
				serializeJson(obj,url);
			}
			if (mode == "start" || mode == "stop")//再生停止制御
			{
				var obj = new//JSON生成
				{
					@params = new[] { new { } },
					method = "pausePlayingContent",
					version = "1.0",
					id = 1,
				};
				url = "avContent";
				serializeJson(obj,url);
			}
			if (mode == "next")//次の曲へ
			{
				var obj = new//JSON生成
				{
					@params = new[] { new { } },
					method = "setPlayNextContent",
					version = "1.0",
					id = 1,
				};
				url = "avContent";
				serializeJson(obj,url);
			}
			if (mode == "volumeup")
			{
				var obj = new//JSON生成
				{
					@params = new[] {
						new {
							volume = "+1",
						}
					},
					method = "setAudioVolume",
					version = "1.0",
					id = 1,
				};
				url = "audio";
				serializeJson(obj,url);
			}
			if (mode == "volumedown")
			{
				var obj = new//JSON生成
				{
					@params = new[] {
						new {
							volume = "-1",
						}
					},
					method = "setAudioVolume",
					version = "1.0",
					id = 1,
				};
				url = "audio";
				serializeJson(obj, url);
			}
		}
	}

	public class setJson
	{
		public string meth;
		public string ver;
		public int i;
		public int param;
		public string url;
		public string paramStatus;
		public string paramDetail;
		public string paramVolume;
	}
}