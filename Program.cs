using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MongoDB.Driver;
using scrapingWeb.Entity;

class Program
{
    static void Main(string[] args)
    {
        string connectionString = "mongodb://localhost:27017";
        string databaseName = "Yazlab2_1";
        string collectionName = "Articles";

        var client = new MongoClient(connectionString);
        var db = client.GetDatabase(databaseName);
        var collection = db.GetCollection<Article>(collectionName);

        string url = "https://dergipark.org.tr/tr/search?q=&section=articles";

        List<string> pdfLinks = new List<string>();

        ScrapArticleLinks("https://dergipark.org.tr/tr/search?q=mustafa&section=articles", pdfLinks);

        List<Thread> threads = new List<Thread>();

        foreach (string link in pdfLinks)
        {
            Thread thread = new Thread(() =>
            {
                Article article = new Article();
                ScrapArticle(link, article);
                collection.InsertOne(article);
            });
            threads.Add(thread);
            thread.Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }
    }

    static void ScrapArticle(string url, Article article)
    {
        HtmlWeb web = new HtmlWeb();
        HtmlDocument document = web.Load(url);


        //Yayın Adı

        HtmlNode nameNode = document.DocumentNode.SelectSingleNode("//*[@id=\"article_en\"]/div[1]/h3");

        if (nameNode != null)
        {
            string content = nameNode.InnerText;
            article.Title = content;
            Console.WriteLine("Yayının Adı : " + content);
        }

        // Yazarların İsimleri

        HtmlNodeCollection authorNodes = document.DocumentNode.SelectNodes("//*[@id=\"article_en\"]/p");

        if (authorNodes != null)
        {
            StringBuilder allAuthorsBuilder = new StringBuilder();
            foreach (HtmlNode node in authorNodes)
            {
                string authors = node.InnerText;
                allAuthorsBuilder.Append(authors);
                Console.WriteLine("\n Yazarlar: " + authors);
            }
            string allAuthors = allAuthorsBuilder.ToString();
            article.Author = allAuthors;
        }

        // Yayın Türü

        HtmlNode articleTypeNode = document.DocumentNode.SelectSingleNode("//*[@id=\"article-main-portlet\"]/div[1]/div[1]/div/span");

        if (articleTypeNode != null)
        {
            string articleType = articleTypeNode.InnerText;
            article.Type = articleType;
            Console.WriteLine("\n Makale Türü :" + articleType);
        }

        //Yayınlanma Tarihi 

        HtmlNode articleDateNode = document.DocumentNode.SelectSingleNode("//*[@id=\"article_en\"]/span/a");

        if (articleDateNode != null)
        {
            string articleDate = Regex.Replace(articleDateNode.InnerText, "[^0-9]", "");
            article.Date = articleDate;
            Console.WriteLine("\nYayınlanma Tarihi: " + articleDate);
        }
        //Yayıncı Adı

        HtmlNode publisherNode = document.DocumentNode.SelectSingleNode("//*[@id=\"journal-title\"]");

        if (publisherNode != null)
        {
            string publisher = publisherNode.InnerText;
            article.Publisher = publisher;
            Console.WriteLine("\n Yayıncı Adı :" + publisher);
        }

        //Anahtar Kelimeler(Arama Motoru)

        //Anahtar Kelimeler(Makaleye Ait)

        HtmlNodeCollection keywordNodes = document.DocumentNode.SelectNodes("//*[@id=\"article_en\"]/div[3]/p");

        if (keywordNodes != null)
        {
            StringBuilder allKeywordsBuilder = new StringBuilder();
            foreach (HtmlNode node in keywordNodes)
            {

                string keyWords = node.InnerText;
                allKeywordsBuilder.Append(keyWords);
                Console.WriteLine("\n Anahtar Kelimeler Makale: " + keyWords);
            }
            string allKeywords = allKeywordsBuilder.ToString();
            article.KeyWords = allKeywords;
        }


        //Özet

        HtmlNode summaryNode = document.DocumentNode.SelectSingleNode("//*[@id=\"article_en\"]/div[2]/p");

        if (summaryNode != null)
        {
            string summary = summaryNode.InnerText;
            article.Summary = summary;
            Console.WriteLine("\n Makale Özeti : " + summary);
        }

        //Referanslar

        HtmlNodeCollection referencesNodes = document.DocumentNode.SelectNodes("//*[@id=\"article_en\"]/div[4]/div");

        if (referencesNodes != null)
        {
            StringBuilder allReferencesBuilder = new StringBuilder();
            foreach (HtmlNode node in referencesNodes)
            {
                string reference = node.InnerText;
                allReferencesBuilder.AppendLine(reference);
                Console.WriteLine("\n Referanslar :" + reference);
            }
            string allReferences = allReferencesBuilder.ToString();
            article.References = allReferences;
        }

        //Alıntı Sayısı

        HtmlNode quotationNode = document.DocumentNode.SelectSingleNode("//*[@id=\"j-stat-article-view\"]");

        if (quotationNode != null)
        {
            string quotation = quotationNode.InnerText;
            article.Quotiation = quotation;
            Console.WriteLine("\n Alıntı Sayısı :" + quotation);
        }

        //Url Adresi

        string urlAdress = url;
        article.UrlAdress = urlAdress;
        Console.WriteLine("\n Url Adresi : " + urlAdress);
    }

    static void ScrapArticleLinks(string url, List<string> pdfLinks)
    {

        Console.Write("SelectLinks fonksiyonuna giriş yaptı\n");
        string onLink = "https://dergipark.org.tr";
        HtmlWeb web = new HtmlWeb();
        HtmlDocument document = web.Load(url);

        var links = document.DocumentNode.SelectNodes("//a[@href]")
            .Select(n => n.Attributes["href"].Value)
            .Where(link => link.StartsWith("https://dergipark.org.tr/tr/pub", StringComparison.OrdinalIgnoreCase))
            .ToList();
        ///////////////////////////////////////////////////////////////////
        foreach (var link in links)
        {


            HtmlWeb web2 = new HtmlWeb();
            HtmlDocument document2 = web2.Load(link);

            // Belirli XPath'e sahip elementi seçin
            HtmlNode pdfLinkNode = document2.DocumentNode.SelectSingleNode("//*[@id=\"article-toolbar\"]/a[1]");


            if (pdfLinkNode != null && pdfLinkNode.Attributes["href"] != null)
            {

                string linkUrl = pdfLinkNode.Attributes["href"].Value;

                var fullLink = string.Concat(onLink, linkUrl);
                pdfLinks.Add(link);
                // URL'yi ekrana yazdırın
                Console.WriteLine("Seçilen Linkin URL'si:");
                Console.WriteLine(link);
            }
        }
    }

    static string ScrapPageLinks(string url, List<string> pageLinks)
    {

        pageLinks.Add(url);

        string onLink = "https://dergipark.org.tr";

        HtmlWeb web = new HtmlWeb();
        HtmlDocument document = web.Load(url);

        var nextHrefLinkNode = document.DocumentNode.SelectSingleNode("//*[@id='kt_content']/div[2]/div[2]/div[2]/div[3]");

        if (nextHrefLinkNode != null)
        {
            HtmlNode lastPageLink = nextHrefLinkNode.Descendants("a").LastOrDefault();

            if (lastPageLink == null)
            {
                return "end";
            }

            string linkUrl = lastPageLink.Attributes["href"].Value;
            string nextLink = string.Concat(onLink, linkUrl);

            Console.Write("Bir sonraki sayfaya Geçiş Yapılıyor \n");

            return ScrapPageLinks(nextLink, pageLinks);
        }
        else
        {
            return "end";
        }
    }
}
