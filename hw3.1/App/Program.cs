using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace App
{
    class Program
    {
        private static readonly MongoClient _client = new MongoClient();
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting work");
                Work().Wait();
                Console.WriteLine("Done!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception!");
                Console.Write(e);
                Console.ReadLine();
            }
        }

        private static async Task Work()
        {
            var db = _client.GetDatabase("school");
            var collection = db.GetCollection<Item>("students");

            // pull all items
            var items = await collection.Find(new BsonDocument()).ToListAsync();

            foreach (var item in items)
            {
                // find the lowest homework grade
                Score lowest = item.Scores.OrderBy(x => x.ScoreValue).First(x => x.Type == "homework");
                
                item.Scores.Remove(lowest);
                
                Console.WriteLine("Removing " + lowest.Type + "with grade of " + lowest.ScoreValue + " from " + item.Name);

                // save the item with the lowest grade removed
                await collection.ReplaceOneAsync(x => x.Id == item.Id, item);
            }
        }
        
        // [BsonIgnoreExtraElements]
        private class Item
        {
            public int Id { get; set; }

            [BsonElement("scores")]
            public List<Score> Scores { get; set; }

            [BsonElement("name")]
            public string Name { get; set; }

            public override string ToString()
            {
                return string.Format("Id: {0}, Scores: {1}", Id, Scores);
            }
        }

        private sealed class Score
        {
            [BsonElement("type")]
            public string Type { get; set; }

            [BsonElement("score")]
            public double ScoreValue { get; set; }
        }
    }
}
