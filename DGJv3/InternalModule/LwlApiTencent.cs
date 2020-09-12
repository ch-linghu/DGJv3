using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DGJv3.InternalModule
{
    sealed class LwlApiTencent : LwlApiBaseModule
    {
        internal LwlApiTencent()
        {
            SetServiceName("tencent");
            SetInfo("QQ音乐", INFO_AUTHOR, INFO_EMAIL, INFO_VERSION, "搜索QQ音乐的歌曲");
        }

        protected override string GetDownloadUrl(SongItem songInfo)
        {
            string prot = "https://";
            string host = "u.y.qq.com";
            string path = $"/cgi-bin/musicu.fcg?format=json&data=%7B%22req_0%22%3A%7B%22module%22%3A%22vkey.GetVkeyServer%22%2C%22method%22%3A%22CgiGetVkey%22%2C%22param%22%3A%7B%22guid%22%3A%22358840384%22%2C%22songmid%22%3A%5B%22{songInfo.SongId}%22%5D%2C%22songtype%22%3A%5B0%5D%2C%22uin%22%3A%221443481947%22%2C%22loginflag%22%3A1%2C%22platform%22%3A%2220%22%7D%7D%2C%22comm%22%3A%7B%22uin%22%3A%2218585073516%22%2C%22format%22%3A%22json%22%2C%22ct%22%3A24%2C%22cv%22%3A0%7D%7D";

            string result_str;
            try
            {
                result_str = Fetch(prot, host, path);
            }
            catch (Exception ex)
            {
                Log("下载歌曲时网络错误：" + ex.Message);
                return null;
            }

            try
            {
                JObject info = JObject.Parse(result_str);
                if (info["code"].ToString() == "0")
                {
                    string purl, sip;

                    var urlinfo = (info["req_0"]["data"]["midurlinfo"] as JArray)?[0];
                    if (urlinfo != null)
                    {
                        purl = urlinfo["purl"].ToString();
                    }
                    else
                    {
                        Log("找不到歌曲下载链接");
                        return null;
                    }

                    sip = (info["req_0"]["data"]["sip"])?[0].ToString();

                    string down_url = sip + purl;
                    return down_url;
                }
                else
                {
                    Log("获取歌词下载链接错误：code = " + info["code"].ToString());
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log("获取歌词解析数据错误：" + ex.Message);
                return null;
            }
        }

        protected override string GetLyricById(string Id)
        {
            string prot = "https://";
            string host = "c.y.qq.com";
            string path = $"/lyric/fcgi-bin/fcg_query_lyric_new.fcg?songmid={Id}&format=json&nobase64=1";
            string referer = "https://y.qq.com/portal/player.html,%E4%B8%8D%E7%84%B6%E8%AF%B7%E6%B1%82%E4%BC%9A%E8%BF%94%E5%9B%9E-1310";

            string result_str;
            try
            {
                result_str = Fetch(prot, host, path, null, referer);
            }
            catch (Exception ex)
            {
                Log("获取歌词时网络错误：" + ex.Message);
                return null;
            }

            try
            {
                JObject info = JObject.Parse(result_str);
                if (info["code"].ToString() == "0" && info["retcode"].ToString() == "0")
                {
                    return info["lyric"].ToString();
                }
                else
                {
                    Log("获取歌词数据错误：code = " + info["code"].ToString() + ", retcode = " + info["retcode"].ToString());
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log("获取歌词解析数据错误：" + ex.Message);
                return null;
            }
        }

        protected override List<SongInfo> GetPlaylist(string keyword)
        {
            return base.GetPlaylist(keyword);
        }

        protected override SongInfo Search(string keyword)
        {
            string prot = "https://";
            string host = "c.y.qq.com";
            string path = $"/soso/fcgi-bin/client_search_cp?p=1&n=2&w={HttpUtility.UrlEncode(keyword)}&format=json";

            string result_str;
            try
            {
                result_str = Fetch(prot, host, path);
             }
            catch (Exception ex)
            {
                Log("搜索歌曲时网络错误：" + ex.Message);
                return null;
            }

            JObject song = null;
            try
            {
                JObject info = JObject.Parse(result_str);
                if (info["code"].ToString() == "0")
                {
                    song = (info["data"]["song"]["list"] as JArray)?[0] as JObject;
                }
            }
            catch (Exception ex)
            {
                Log("搜索歌曲解析数据错误：" + ex.Message);
                return null;
            }

            SongInfo songInfo ;

            try
            {
                songInfo = new SongInfo (
                    this,
                    song["songmid"].ToString(),
                    song["songname"].ToString(),
                    (song["singer"] as JArray).Select(x => x["name"].ToString()).ToArray()
                );
            }
            catch (Exception ex)
            {
                Log("歌曲信息获取结果错误：" + ex.Message);
                return null;
            }

            songInfo.Lyric = GetLyricById(songInfo.Id);

            return songInfo;
         }
    }
}
