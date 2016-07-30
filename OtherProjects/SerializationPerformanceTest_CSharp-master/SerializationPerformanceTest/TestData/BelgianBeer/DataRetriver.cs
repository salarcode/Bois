using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Serialization;
using HtmlAgilityPack;

namespace SerializationPerformanceTest.TestData.BelgianBeer
{
    public static class BelgianBeerDataRetriever
    {
        public static List<Beer> GetDataFromXML()
        {
            var serializer = new XmlSerializer(typeof(List<Beer>));
            using (var fs = new FileStream(@".\..\..\TestData\BelgianBeer\Data\beers.xml", FileMode.Open))
            {
                var deserialize = serializer.Deserialize(fs);
                return (List<Beer>) deserialize;
            }
        }


        public static List<Beer> GetDataFromWikipedia()
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            var html = new WebClient().DownloadString(new Uri("http://en.wikipedia.org/wiki/List_of_Belgian_beer"));

            htmlDocument.LoadHtml(html);

            var rows = htmlDocument.DocumentNode.Descendants("tr").ToArray();

            List<Beer> list = new List<Beer>();

            foreach (var row in rows)
            {
                string[] columns = row.Descendants("td").Select(item => item.InnerText).ToArray();

                float alcohol;

                bool hasFourColumns = columns.Length == 4;
                if (hasFourColumns && float.TryParse(columns[2].TrimEnd('%'), out alcohol))
                {
                    var sort = columns[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    Beer beer = new Beer()
                    {
                        Brand = columns[0],
                        Sort = sort,
                        Alcohol = alcohol,
                        Brewery = columns[3]
                    };

                    list.Add(beer);
                }
            }

            //FileStream fs = new FileStream("beers.xml", FileMode.Create);
            //new XmlSerializer(typeof(List<Beer>)).Serialize(fs, list);
            //fs.Close();

            return list;

        }
    }
}