using System;
using System.Threading.Tasks;
using Elasticsearch.Net;

namespace ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new ConnectionConfiguration(new Uri("http://localhost:9200")).RequestTimeout(TimeSpan.FromMinutes(2));
            var lowlevelClient = new ElasticLowLevelClient(settings);
            IndexData(lowlevelClient).Wait();
            Console.WriteLine("Connection eshtablished");
            Console.ReadKey();
        }

        private static async Task IndexData(ElasticLowLevelClient client)
        {
            var people = new object[]
            {
                new { index = new { _index = "people", _type = "person", _id = "1"  }},
                new { first_name = "Martijn", last_name = "Laarman",email = "ronnie.nsu@gmail.com",age=22,married=false },
                new { index = new { _index = "people", _type = "person", _id = "2"  }},
                new { first_name = "Greg", last_name = "Marzouka" ,email = "ronnie.nsu@outlook.com",age=23,married=true },
                new { index = new { _index = "people", _type = "person", _id = "3"  }},
                new { first_name = "Russ", last_name = "Cam" ,email = "ronnie.nsu@yahoo.com",age=24,married=false },
            };

            var ret = await client.BulkAsync<StringResponse>(PostData.MultiJson(people));
        }
    }

    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
