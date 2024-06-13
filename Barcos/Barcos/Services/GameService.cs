using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Barcos.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Barcos.Services
{
    public class GameService
    {
        private readonly IMongoCollection<BsonDocument> _games;

        public GameService()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("recu");
            _games = database.GetCollection<BsonDocument>("games");
        }

        public async Task SaveGame(Game game)
        {
            var gameDoc = game.ToBsonDocument();

            var gameState = new BsonDocument
            {
                { "game", gameDoc }
            };

            await _games.InsertOneAsync(gameState);
        }
        public async Task<List<BsonDocument>> GetUserGames(string username)
        {
            var filter = Builders<BsonDocument>.Filter.Or(
                Builders<BsonDocument>.Filter.Eq("game.Player1.Username", username),
                Builders<BsonDocument>.Filter.Eq("game.Player2.Username", username)
            );

            var gameDocs = await _games.Find(filter).ToListAsync();

            return gameDocs;
        }
        public async Task<BsonDocument> GetGameById(string id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
            var gameDoc = await _games.Find(filter).FirstOrDefaultAsync();

            return gameDoc;
        }
    }
}
