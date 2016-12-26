using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Codeplex.Data;

namespace hapControlApp
{
	class Program
	{
        public static int nowVolume = 0;
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

		static void connectHap(dynamic json,string url,int isNeedParse)//帰ってきたJSONが意味をなさないやつはこれ
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
			string result = client.PostAsync(Url, theContent).Result.Content.ReadAsStringAsync().Result;//送信はここ
			Console.WriteLine(result);
			if (isNeedParse == 1)//音量取り出し後に戻るやつ
			{
				string str = @"""volume"":";
				int ind = result.IndexOf(str);
				string volumeParam = result.Substring(ind,12);
                int leng = volumeParam.Length;
                string volume;
                if (0 <= volumeParam.IndexOf(","))
                {//,が含まれている=ボリュームが一桁
                    volume = volumeParam.Substring(leng - 2,1);
                    Console.WriteLine(volume);
                }
                else
                {//含まれていない=ボリュームが二桁
                    volume = volumeParam.Substring(leng - 2, 2);
                    Console.WriteLine(volume);
                }
                int ivol = Int32.Parse(volume);
                nowVolume = ivol;
            }
			else
			{
				inputCmd();//parseが必要ない場合のみinputへ戻る
			}
		}

		static void serializeJson(dynamic obje,string url,int isNeedParse)
		{
			var json = DynamicJson.Serialize(obje);
			connectHap(json,url,isNeedParse);
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
				serializeJson(obj,url,0);
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
				serializeJson(obj,url,0);
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
				serializeJson(obj,url,0);
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
				serializeJson(obj,url,0);
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
				serializeJson(obj,url,0);
			}
			if (mode == "volumeup")
			{
                dynamic getVolumeObj = getVolumeInfo();
                url = "audio";
                serializeJson(getVolumeObj, url, 1);//ここまで現情報取得
                nowVolume += 1;//ここから現ボリュームに1足した値をJSONで送信
                string upvolume = nowVolume.ToString();
				var obj = new
				{
					@params = new[] {
						new {
							volume = upvolume,
						}
					},
					method = "setAudioVolume",
					version = "1.0",
					id = 1,
				};
				serializeJson(obj,url,0);
			}
			if (mode == "volumedown")
			{
                dynamic getVolumeObj = getVolumeInfo();
                url = "audio";
                serializeJson(getVolumeObj, url, 1);
                nowVolume -= 1;
                string downvolume = nowVolume.ToString();
                var obj = new
				{
					@params = new[] {
						new {
							volume = downvolume,
						}
					},
					method = "setAudioVolume",
					version = "1.0",
					id = 1,
				};
				serializeJson(obj, url, 0);
			}
		}

        static dynamic getVolumeInfo()
        {//音量調節の下準備のための
            var getVolumeObj = new
            {
                @params = new[] { new { } },
                method = "getVolumeInformation",
                version = "1.1",
                id = 4,
            };
            return getVolumeObj;
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