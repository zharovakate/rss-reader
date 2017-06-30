using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;


namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private void populateFeed()
        {
            var url = "https://blogs.technet.microsoft.com/cloudplatform/rssfeeds/devblogs";
            var url1 = "https://www.alvinashcraft.com/feed/atom/";
            var url2 = "https://www.asp.net/rss/dailyarticles";
            buildReferenceMapFromRSS(url);
            buildReferenceMapFromRSS(url1);
//            buildReferenceMapFromRSS(url2);
        }

        public ActionResult Blogs()
        {
            populateFeed();

            var blogsList = blogsReferences.ToList();
            ViewBag.Blogs = blogsList;
            
            blogsList.Sort((x, y) => y.Value - x.Value);

            return View();
        }

        public ActionResult Articles()
        {
            populateFeed();

            var articleList = articleReferences.ToList();
            ViewBag.Article = articleList;
            
            articleList.Sort((x, y) => y.Value - x.Value);

            return View();
        }

        public ActionResult Authors()
        {
            populateFeed();

            var authorsList = authorReferences.ToList();
            ViewBag.Authors = authorsList;

            return View();
        }

        private Dictionary<string, int> authorReferences = new Dictionary<string, int>();
        private Dictionary<string, int> blogsReferences = new Dictionary<string, int>();
        private Dictionary<string, int> articleReferences = new Dictionary<string, int>();

        Regex urlRegex =
            new Regex(@"((http|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)");

        public List<Uri> findAllUrls(string content)
        {
            var urls = new List<Uri>();

            foreach (var match in urlRegex.Matches(content))
            {
                var url = ((Match) match).Value;
                urls.Add(new Uri(url));
            }

            return urls;
        }

        public void buildReferenceMapFromRSS(string url)
        {
            var reader = new XmlTextReader(url);
            var feed = SyndicationFeed.Load(reader);

            if (feed == null)
                return;

            var authorsMap = new Dictionary<string, int>();
            var blogsMap = new Dictionary<string, int>();
            var articleMap = new Dictionary<string, int>();

            foreach (var item in feed.Items)
            {
                foreach (var author in item.Authors)
                {
                    var name = author.Name;
                    if (name == null)
                        continue;
                    if (!authorsMap.ContainsKey(name))
                        authorsMap[name] = 1;
                    else
                        authorsMap[name] = authorsMap[name] + 1;
                }

                var content = item.Content as TextSyndicationContent;
                if (content == null)
                    continue;

                foreach (var uri in findAllUrls(content.Text))
                {
                    var blogURL = uri.GetLeftPart(UriPartial.Authority);
                    if (!blogsMap.ContainsKey(blogURL))
                        blogsMap[blogURL] = 1;
                    else
                        blogsMap[blogURL] = blogsMap[blogURL] + 1;

                    var articleURL = uri.GetLeftPart(UriPartial.Path);
                    if (!articleMap.ContainsKey(articleURL))
                        articleMap[articleURL] = 1;
                    else
                        articleMap[articleURL] = articleMap[articleURL] + 1;

                    Console.WriteLine(blogURL, articleURL);
                }

            }

            authorReferences = merge(authorsMap, authorReferences);
            blogsReferences = merge(blogsMap, blogsReferences);
            articleReferences = merge(articleMap, articleReferences);
        }


        public Dictionary<string, int> merge(Dictionary<string, int> one, Dictionary<string, int> other)
        {
            var copy = new Dictionary<string, int>(one);
            foreach (var kv in other)
            {
                Console.WriteLine(kv.Key, kv.Value);
                if (copy.ContainsKey(kv.Key))
                    copy[kv.Key] = copy[kv.Key] + kv.Value;
                else
                    copy[kv.Key] = kv.Value;
            }

            return copy;
        }
    }
}