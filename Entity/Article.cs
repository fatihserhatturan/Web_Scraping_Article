using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace scrapingWeb.Entity
{
    public class Article
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Type { get; set; }
        public string Date { get; set; }
        public string Publisher { get; set; }
        public string KeyWords { get; set; }
        public string Summary { get; set; }
        public string References { get; set; }
        public string Quotiation { get; set; }
        public string UrlAdress { get; set; }
    }
}
