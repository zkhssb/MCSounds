namespace MCSounds.Fandom
{
    public class QueryResult
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public QueryResult(string title, string url)
        {
            Title = title;
            Url = url;
        }
    }
}
