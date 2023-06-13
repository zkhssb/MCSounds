using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using MCSounds.Utils;
using RestSharp;
using System.Net;

namespace MCSounds.Fandom
{
    public enum FandomLanguage
    {
        Zh,
        En
    }
    public class FandomClient
    {
        public FandomLanguage Language { get; }
        public string SearchKey { get; }
        public string SoundKey { get; }

        private readonly Uri _baseAddress;
        private readonly RestClient _client;

        public FandomClient(FandomLanguage language)
        {
            Language = language;
            string url = string.Empty;
            switch (language)
            {
                case FandomLanguage.Zh:
                    url = "https://minecraft.fandom.com/zh/wiki/";
                    SearchKey = "搜索";
                    SoundKey = "音效";
                    break;
                default:
                    url = "https://minecraft.fandom.com/wiki/";
                    SearchKey = "Search";
                    SoundKey = "Sound";
                    break;
            }
            _baseAddress = new Uri(url);
            _client = new RestClient(_baseAddress);
            _client.AddDefaultHeader("Referrer", "https://minecraft.fandom.com/");
        }
        public FandomClient() : this(FandomLanguage.Zh)
        {
        }
        public async Task<List<QueryResult>> Query(string key)
        {
            List<QueryResult> result = new();
            string path = string.Format("{0}Special:{1}", _baseAddress.ToString(), SearchKey);
            RestRequest request = new(path, Method.Get);
            request.AddQueryParameter("query", key);
            request.AddQueryParameter("scope", "internal");
            request.AddQueryParameter("navigationSearch", true);
            try
            {
                RestResponse response = await _client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    HtmlParser parser = new();
                    IHtmlDocument document = parser.ParseDocument(response.Content ?? string.Empty);
                    IElement? root = document.QuerySelector("ul.unified-search__results");
                    if (root != null)
                    {
                        IHtmlCollection<IElement> elements = root.QuerySelectorAll("li.unified-search__result");
                        foreach (IElement element in elements)
                        {
                            IElement? a = element.QuerySelector("a");
                            if (a != null)
                            {
                                string? title = a.GetAttribute("data-title");
                                string? url = a.GetAttribute("href");
                                if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(url))
                                {
                                    result.Add(new(title, url));
                                }
                            }
                        }
                    }

                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    string errorString = string.Format("找不到{0},请重新搜索!", key);
                    ErrorUtil.Error("搜索请求后", errorString);
                }
                else
                {
                    string errorString = string.Format("网页返回了错误的状态码{0}", response.StatusCode);
                    ErrorUtil.Error("搜索请求后", errorString);
                }
            }
            catch (Exception ex)
            {
                ErrorUtil.Error("搜索", ex.ToString());
            }
            return result;
        }
        public async Task<List<SoundInfo>> Sound(QueryResult query)
        {
            List<SoundInfo> result = new();
            List<IHtmlAudioElement> cmpedAudio = new();
            RestRequest request = new(query.Url, Method.Get);
            try
            {
                RestResponse response = await _client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    HtmlParser parser = new();
                    IHtmlDocument document = parser.ParseDocument(response.Content ?? string.Empty);
                    var audios = document.QuerySelectorAll("audio.sound-audio");

                    foreach (var audio in audios)
                    {
                        if (cmpedAudio.Contains(audio))
                            continue;
                        var td = Language == FandomLanguage.Zh ? audio?.ParentElement?.ParentElement as IHtmlTableDataCellElement : audio?.ParentElement?.ParentElement?.ParentElement as IHtmlTableDataCellElement;
                        if (td != null && td.TagName.ToLower() == "td")
                        {
                            var tr = td.ParentElement as IHtmlTableRowElement;
                            if (tr != null && tr.TagName.ToLower() == "tr")
                            {
                                var tds = tr.QuerySelectorAll("td");
                                if (tds.Count() == 6)
                                {
                                    SoundInfo sound = new();

                                    var soundTd = tds[0];
                                    sound.Text = tds[1].TextContent;
                                    sound.Type = tds[2].TextContent;
                                    sound.Description = tds[3].TextContent;


                                    var soundSpans = soundTd.QuerySelectorAll("span.sound");

                                    foreach (var soundSpan in soundSpans)
                                    {
                                        IHtmlAudioElement? audio_table = soundSpan.QuerySelector("audio.sound-audio") as IHtmlAudioElement;
                                        if (audio_table != null)
                                        {
                                            cmpedAudio.Add(audio_table);
                                            string? url = audio_table.GetAttribute("src");
                                            if (!string.IsNullOrEmpty(url))
                                                sound.Urls.Add(url);
                                        }
                                    }

                                    result.Add(sound);
                                }
                            }
                        }
                    }
                }
                else
                {
                    ErrorUtil.Error("请求页面", string.Format("网页返回了{0}", response.StatusCode));
                }
            }
            catch (Exception ex)
            {
                ErrorUtil.Error("请求页面", ex.ToString());
            }
            return result;
        }

        public async Task<MemoryStream> GetStreamAsync(string url)
        {
            RestRequest request = new(url, Method.Get);
            try
            {
                RestResponse response = await _client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return new MemoryStream(response.RawBytes ?? Array.Empty<byte>());
                }
                else
                {
                    ErrorUtil.Error("请求页面", string.Format("网页返回了{0}", response.StatusCode));
                }
            }
            catch (Exception ex)
            {
                ErrorUtil.Error("请求页面", ex.ToString());
            }
            return new MemoryStream();
        }

        /*
        public async Task<List<SoundInfo>> Sound(QueryResult query)
        {

            List<SoundInfo> result = new();
            RestRequest request = new (query.Url, Method.Get);
            try
            {
                RestResponse response = await _client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    HtmlParser parser = new();
                    IHtmlDocument document = parser.ParseDocument(response.Content ?? string.Empty);
                    IHtmlCollection<IElement> tables = document.QuerySelectorAll("table.wikitable");
                    IElement? target = null;
                    foreach (IElement table in tables)
                    {
                        IElement? tBody = table.QuerySelector("tbody");
                        if(tBody != null)
                        {
                            IHtmlCollection<IElement> trs = tBody.QuerySelectorAll("tr");
                            if (trs.Count() != 0)
                            {
                                IHtmlCollection<IElement> ths = trs[0].QuerySelectorAll("th");
                                if (trs.Count() != 0)
                                {
                                    string title = ths[0].TextContent;
                                    if (ths[0].TextContent == SoundKey)
                                    {
                                        target = tBody;
                                    }
                                }
                            }
                        }
                    }
                    if(null != target)
                    {
                        var trs = target.QuerySelectorAll("tr");
                        foreach (IElement tr in trs.Skip(1))
                        {
                            var tds = tr.QuerySelectorAll("td");
                            if(tds.Count() >= 6)
                            {
                                SoundInfo sound = new();

                                var soundTd         = tds[0];
                                sound.Type          = tds[1].TextContent;
                                sound.Description   = tds[2].TextContent;
                                sound.Text          = tds[3].TextContent;

                                var soundSpans = soundTd.QuerySelectorAll("span.sound");

                                foreach (var soundSpan in soundSpans)
                                {
                                    var audio = soundSpan.QuerySelector("audio.sound-audio");
                                    if(audio != null)
                                    {
                                        string? url = audio.GetAttribute("src");
                                        if(!string.IsNullOrEmpty(url))
                                            sound.Urls.Add(url);
                                    }
                                }

                                result.Add(sound);
                            }
                        }
                    }
                }
                else
                {
                    ErrorUtil.Error("请求页面", string.Format("网页返回了{0}", response.StatusCode));
                }
            }catch(Exception ex)
            {
                ErrorUtil.Error("请求页面", ex.ToString());
            }
            return result;
        }
        */
    }
}
