using System;
using System.Net.Http;
using System.Text;
using Codeplex.Data;

namespace hapControlApp
{
	class Program
	{
        public static int nowVolume = 0;
        public static string nowPlaying = null;
        public static string album = null;
        public static string artist = null;
        public static string codec = null;
        public static string bandwidth = null;
        public static double bitrate = 0;
        public static string freq = null;
        public static string uri = null;
        public static double positionSec = 0;
        public static int posSec = 0;
        public static int posMin = 0;
        public static string coverArtUrl = null;

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
			client.DefaultRequestHeaders.ExpectContinue = false;//これは絶対いる
			StringContent theContent = new StringContent(json, Encoding.UTF8, "application/x-www-form-urlencoded");
			dynamic result = client.PostAsync(Url, theContent).Result.Content.ReadAsStringAsync().Result;//送信部。帰ってくるJSONはresultに入る
			Console.WriteLine(result);
            dynamic data = DynamicJson.Parse(result);
            if (isNeedParse == 1)//現在音量取得
            {
                nowVolume = int.Parse(data.result.volume);
            }
            else if (isNeedParse == 2)//楽曲情報取得
            {
                nowPlaying = data.result[0].title;
                album = data.result[0].albumName;
                artist = data.result[0].artist;
                codec = data.result[0].audioInfo[0].codec;
                bandwidth = data.result[0].audioInfo[0].bandwidth;
                bitrate = (double.Parse(data.result[0].audioInfo[0].bitrate)) / 1000;
                freq = data.result[0].audioInfo[0].frequency;
                coverArtUrl = data.result[0].coverArtUrl;
                Console.WriteLine("曲名:" + nowPlaying);
                Console.WriteLine("アルバム名;" + album);
                Console.WriteLine("アーティスト:" + artist);
                Console.WriteLine("コーデック:" + codec);
                Console.WriteLine("ビットレート:" + bitrate + "kbps");
                Console.WriteLine("サンプリング周波数:" + freq);
                Console.WriteLine("ビット深度:" + bandwidth);

                positionSec = data.result[0].positionSec;
                posSec = (int)positionSec;
                posMin = posSec / 60;//分数
                posSec = posSec % 60;//秒数
                Console.WriteLine(posMin + "分" + posSec + "秒");
                Console.ReadLine();
            }
            else if (isNeedParse == 3)//ミュートチェック
            {
                if (data.result[0].mute == "on")
                {
                    makeMutejson(1);
                }
                if (data.result[0].mute == "off")
                {
                    makeMutejson(0);
                }
            }
            else
            {
                inputCmd();//parseが必要ない場合のみinputへ戻る
            }
		}

        static void makeMutejson(int mode)//ミュート状態により処理変更
        {
            string str = null;
            if (mode == 1)
            {
                str = "off";
            }
            else
            {
                str = "on";
            }
            var obj = new
            {
                @params = new[] {
                        new {
                            mute = str,
                            }
                    },
                method = "setAudioMute",
                version = "1.1",
                id = 1,
            };
            serializeJson(obj, "audio", 0);
        }

		static void serializeJson(dynamic obje,string url,int isNeedParse)//シリアライズして次に流すだけのメソッド
		{
			var json = DynamicJson.Serialize(obje);
			connectHap(json,url,isNeedParse);
		}

		static void setJunbi(string mode)//コマンドの場合分け
		{
			string url = null;
			if (mode == "previous")//前の曲(頭出し)
			{
				var obj = new
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
				var obj = new
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
				var obj = new
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
			if (mode == "start" || mode == "stop")//再生、停止
			{
				var obj = new
				{
					@params = new[] { new { } },
					method = "pausePlayingContent",
					version = "1.0",
					id = 1,
				};
				url = "avContent";
				serializeJson(obj,url,0);
			}
			if (mode == "next")//次の曲
			{
				var obj = new
				{
					@params = new[] { new { } },
					method = "setPlayNextContent",
					version = "1.0",
					id = 1,
				};
				url = "avContent";
				serializeJson(obj,url,0);
			}
			if (mode == "volumeup")//音量↑
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
			if (mode == "volumedown")//音量↓
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
            if (mode == "getmusicinfo")//再生中楽曲情報取得
            {
                var obj = new
                {
                    @params = new[] {
                        new {
                            level ="detail",
                        }
                    },
                    method = "getPlayingContentInfo",
                    version = "1.2",
                    id = 3,
                };
                url = "avContent";
                serializeJson(obj, url, 2);
            }
            if (mode == "mute")//ミュートオンオフ
            {
                dynamic setMuteObj = getVolumeInfo();
                url = "audio";
                serializeJson(setMuteObj, url, 3);
            }

		}

        static dynamic getVolumeInfo()//音量情報の取得用JSON生成
        {
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
}