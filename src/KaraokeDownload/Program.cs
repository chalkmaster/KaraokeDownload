using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;

namespace KaraokeDownload
{
    class Program
    {
        private const string UrlFormation = "http://cf.uol.com.br/cante/new/musicas.asp?D=1&P=0&O=&G=&A=1&B={0}&L=1";        
        private static List<string> LinksDownloaded = new List<string>();

        private const string DestinationPath = @"D:\KaraokeMusics\{0}";

        //Para baixar apenas as músicas Nacionais, descomente a linha abaixo e comente a acima
        //private const string DestinationPath = @"D:\KaraokeMusics\Portugues\{0}";
        


        static void Main(string[] args)
        {
            GetMusics("0");

            //Para baixar apenas as músicas Nacionais, comente o FOR abaixo e descomente a linha no método GetMusics
            for (var c = 'A'; c <= 'Z'; c++)
            {
                GetMusics(c.ToString());
            }
        }

        static void GetMusics(string artistNameFirtsChar)
        {
            Console.WriteLine(String.Format("[{0}] - Getting Musics For Artist {1}", DateTime.Now, artistNameFirtsChar));
            var url = String.Format(UrlFormation, artistNameFirtsChar);
            
            //Para baixar apenas as músicas Nacionais, descomente a linha abaixo e comente a acima
            //var url = "http://cf.uol.com.br/cante/new/musicas.asp?O=Nacionais&A=1&B=*&L=1"; 


            while (!string.IsNullOrWhiteSpace(url))
            {
                Console.WriteLine(String.Format("[{0}] - Getting From {1}", DateTime.Now, url));

                var links = GetMusicLinks(url);
                LinksDownloaded.Add(url);

                DownloadAllMusics(links);
                url = GetNextPageLinks(url);
 
            }
        }

        static List<string> GetMusicLinks(string url)
        {
            Console.WriteLine(String.Format("[{0}] - Load Links", DateTime.Now));

            using (var wc = new WebClient())
            {
                var htmlSource = wc.DownloadString(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlSource);
                var links = doc.DocumentNode.SelectNodes("//a[@href]").Select(n => n.Attributes["href"].Value).ToList();

                return links.Where(l => l.EndsWith(".mk1", StringComparison.InvariantCultureIgnoreCase)
                                        || l.EndsWith(".kar", StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

        }

        static string GetNextPageLinks(string url)
        {
            Console.WriteLine(String.Format("[{0}] - GoToNextPage", DateTime.Now));
            using (var wc = new WebClient())
            {
                var htmlSource = wc.DownloadString(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlSource);
                var links = doc.DocumentNode.SelectNodes("//a[@href]").Select(n => n.Attributes["href"].Value).ToList();

                return links.FirstOrDefault(l => l.StartsWith("http://cf.uol.com.br/cante/new/musicas.asp?D=1&P=",
                                                              StringComparison.InvariantCultureIgnoreCase)
                                                              && !LinksDownloaded.Contains(l));
            }

        }

        static void DownloadAllMusics(List<string> links)
        {
            links.ForEach(link =>
            {
                using (var wc = new WebClient())
                {
                    var fileName = link.Split('/').Last();
                    Console.WriteLine(String.Format("[{0}] - Getting File {1}", DateTime.Now, fileName));
                    try
                    {
                        wc.DownloadFile(link, String.Format(DestinationPath, fileName));
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("404"))
                        {
                            link = link.Replace(" ", "");
                        }
                        try
                        {
                            wc.DownloadFile(link, String.Format(DestinationPath, fileName));
                        }
                        catch {
                            Console.WriteLine(String.Format("[{0}] - Error on Getting File {1}", DateTime.Now, fileName));
                        }
                    }
                }
            });
        }

    }
}
