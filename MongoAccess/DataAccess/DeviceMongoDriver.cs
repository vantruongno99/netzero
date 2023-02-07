using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MongoAccess.DataAccess
{
    public class DeviceMongoDriver<T>
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;
        public IMongoCollection<T> Collection { get; private set; }

        public DeviceMongoDriver(MongoDbSet mongoDbSet_init)
        {
            _client = new MongoClient(mongoDbSet_init.ConnectionString);
            _database = _client.GetDatabase(mongoDbSet_init.DatabaseName);
            Collection = _database.GetCollection<T>(mongoDbSet_init.CollectionName);
        }

        public async Task<List<T>> ReadObjectsAsync(Expression<Func<T, bool>> filter)
        {
            // List<DeviceObject> objects = await Collection.Find(filter).ToListAsync();
            // if (objects.Count > 0) return objects;
            // return null;

            return await Collection.Aggregate().Match(filter).ToListAsync();
        }

        public async Task<List<T>> ReadAllObjectsAsync()
        {
            // List<DeviceObject> objects = await Collection.Find(filter).ToListAsync();
            // if (objects.Count > 0) return objects;
            // return null;

            return await Collection.Find( _ => true).ToListAsync();
        }

        public async Task<T> ReadSingleObjectAsync(Expression<Func<T, bool>> filter)
        {

            return await Collection.Aggregate().Match(filter).FirstOrDefaultAsync();
        }

        public async Task<T> ReadSingleObjectAsync(ObjectId Id1, ObjectId Id2)
        {
            return await Collection.Find(T => Id1 == Id2).Skip(10).FirstOrDefaultAsync();
        }

        public async Task InsertObjectAsync(T deviceObject)
        {

            await Collection.InsertOneAsync(deviceObject);
        }


        public async Task InsertObjectManyAsync(IEnumerable<T> documents)
        {
            await Collection.InsertManyAsync(documents);
        }

        public async Task UpdateObjectAsync(T deviceObject, ObjectId Id)
        {
            var filter = new BsonDocument("_id", Id);
            await Collection.ReplaceOneAsync(filter, deviceObject);
        }

        public async Task DeleteObjectAsync(T deviceObject, ObjectId Id)
        {
            var filter = new BsonDocument("_id", Id);
            await Collection.DeleteOneAsync(filter);
        }

        public async Task DeleteObjectAsync(ObjectId Id)
        {
            var filter = new BsonDocument("_id", Id);
            await Collection.DeleteOneAsync(filter);
        }
    }
}
