using System.Collections.Generic;

namespace QuoteApi.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<QuoteTag> QuoteTags { get; set; } = new();
    }
}
