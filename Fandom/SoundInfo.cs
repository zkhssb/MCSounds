namespace MCSounds.Fandom
{
    public class SoundInfo
    {
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Urls { get; set; } = new();

        public override string ToString()
        {
            return Text;
        }
    }
}
