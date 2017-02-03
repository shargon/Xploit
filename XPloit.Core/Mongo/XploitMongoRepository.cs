using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XPloit.Helpers.Interfaces;

namespace XPloit.Core.Mongo
{
    /// <summary>
    /// Repository for mongo
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    public class XploitMongoRepository<T> : IMongoCollection<T>, IConvertibleFromString
    {
        MongoClient _MongoClient;
        IMongoDatabase _DB;
        IMongoCollection<T> _Repository;

        /// <summary>
        /// Mongo url
        ///     mongodb://root:pwd@127.0.0.1:27017/tor
        /// </summary>
        public MongoUrl Url { get; private set; }
        /// <summary>
        /// Repository
        /// </summary>
        public IMongoCollection<T> Repository { get { return _Repository; } }
        /// <summary>
        /// Database
        /// </summary>
        public IMongoDatabase DB { get { return _DB; } }
        /// <summary>
        /// Database
        /// </summary>
        public MongoClient Client { get { return _MongoClient; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public XploitMongoRepository() { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="url">Url</param>
        public XploitMongoRepository(string url) { LoadFromString(url); }

        #region Conversions
        public static implicit operator string(XploitMongoRepository<T> repo) { return repo.Url.ToString(); }
        public static implicit operator XploitMongoRepository<T>(string url) { return new XploitMongoRepository<T>(url); }
        public void LoadFromString(string url)
        {
            Url = new MongoUrl(url);

            _MongoClient = new MongoClient(Url);
            _DB = _MongoClient.GetDatabase(Url.DatabaseName, new MongoDatabaseSettings()
            {
                GuidRepresentation = GuidRepresentation.CSharpLegacy,
                ReadPreference = new ReadPreference(ReadPreferenceMode.Primary),
            });

            if (_DB == null) throw new Exception("Database must be exits");

            ConventionPack pack = new ConventionPack { new EnumRepresentationConvention(BsonType.String) };
            ConventionRegistry.Register("EnumStringConvention", pack, t => true);

            _Repository = DB.GetCollection<T>(typeof(T).Name);
        }

        public void Check()
        {
            if (_DB != null)
                _DB.ListCollections();
        }

        public override string ToString() { return Url.ToString(); }
        #endregion

        #region Interface
        public CollectionNamespace CollectionNamespace { get { return _Repository.CollectionNamespace; } }
        public IMongoDatabase Database { get { return _Repository.Database; } }
        public IBsonSerializer<T> DocumentSerializer { get { return _Repository.DocumentSerializer; } }
        public IMongoIndexManager<T> Indexes { get { return _Repository.Indexes; } }
        public MongoCollectionSettings Settings { get { return _Repository.Settings; } }

        public IAsyncCursor<TResult> Aggregate<TResult>(PipelineDefinition<T, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.Aggregate(pipeline, options, cancellationToken); }
        public Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(PipelineDefinition<T, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.AggregateAsync(pipeline, options, cancellationToken); }
        public BulkWriteResult<T> BulkWrite(IEnumerable<WriteModel<T>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.BulkWrite(requests, options, cancellationToken); }
        public Task<BulkWriteResult<T>> BulkWriteAsync(IEnumerable<WriteModel<T>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.BulkWriteAsync(requests, options, cancellationToken); }
        public long Count(FilterDefinition<T> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.Count(filter, options, cancellationToken); }
        public Task<long> CountAsync(FilterDefinition<T> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.CountAsync(filter, options, cancellationToken); }
        public DeleteResult DeleteMany(FilterDefinition<T> filter, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.DeleteMany(filter, cancellationToken); }
        public DeleteResult DeleteMany(FilterDefinition<T> filter, DeleteOptions options, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.DeleteMany(filter, options, cancellationToken); }
        public Task<DeleteResult> DeleteManyAsync(FilterDefinition<T> filter, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.DeleteManyAsync(filter, cancellationToken); }
        public Task<DeleteResult> DeleteManyAsync(FilterDefinition<T> filter, DeleteOptions options, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.DeleteManyAsync(filter, options, cancellationToken); }
        public DeleteResult DeleteOne(FilterDefinition<T> filter, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.DeleteOne(filter, cancellationToken); }
        public DeleteResult DeleteOne(FilterDefinition<T> filter, DeleteOptions options, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.DeleteOne(filter, options, cancellationToken); }
        public Task<DeleteResult> DeleteOneAsync(FilterDefinition<T> filter, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.DeleteOneAsync(filter, cancellationToken); }
        public Task<DeleteResult> DeleteOneAsync(FilterDefinition<T> filter, DeleteOptions options, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.DeleteOneAsync(filter, options, cancellationToken); }
        public IAsyncCursor<TField> Distinct<TField>(FieldDefinition<T, TField> field, FilterDefinition<T> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.Distinct(field, filter, options, cancellationToken); }
        public Task<IAsyncCursor<TField>> DistinctAsync<TField>(FieldDefinition<T, TField> field, FilterDefinition<T> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.DistinctAsync(field, filter, options, cancellationToken); }
        public IAsyncCursor<TProjection> FindSync<TProjection>(FilterDefinition<T> filter, FindOptions<T, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.FindSync(filter, options, cancellationToken); }
        public Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(FilterDefinition<T> filter, FindOptions<T, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.FindAsync(filter, options, cancellationToken); }
        public TProjection FindOneAndDelete<TProjection>(FilterDefinition<T> filter, FindOneAndDeleteOptions<T, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.FindOneAndDelete(filter, options, cancellationToken); }
        public Task<TProjection> FindOneAndDeleteAsync<TProjection>(FilterDefinition<T> filter, FindOneAndDeleteOptions<T, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.FindOneAndDeleteAsync(filter, options, cancellationToken); }
        public TProjection FindOneAndReplace<TProjection>(FilterDefinition<T> filter, T replacement, FindOneAndReplaceOptions<T, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.FindOneAndReplace(filter, replacement, options, cancellationToken); }
        public Task<TProjection> FindOneAndReplaceAsync<TProjection>(FilterDefinition<T> filter, T replacement, FindOneAndReplaceOptions<T, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.FindOneAndReplaceAsync(filter, replacement, options, cancellationToken); }
        public TProjection FindOneAndUpdate<TProjection>(FilterDefinition<T> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.FindOneAndUpdate(filter, update, options, cancellationToken); }
        public Task<TProjection> FindOneAndUpdateAsync<TProjection>(FilterDefinition<T> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.FindOneAndUpdateAsync(filter, update, options, cancellationToken); }
        public void InsertOne(T document, InsertOneOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        { _Repository.InsertOne(document, options, cancellationToken); }
        [Obsolete]
        public Task InsertOneAsync(T document, CancellationToken cancellationToken)
        { return _Repository.InsertOneAsync(document, cancellationToken); }
        public Task InsertOneAsync(T document, InsertOneOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.InsertOneAsync(document, options, cancellationToken); }
        public void InsertMany(IEnumerable<T> documents, InsertManyOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        { _Repository.InsertMany(documents, options, cancellationToken); }
        public Task InsertManyAsync(IEnumerable<T> documents, InsertManyOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.InsertManyAsync(documents, options, cancellationToken); }
        public IAsyncCursor<TResult> MapReduce<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<T, TResult> options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.MapReduce(map, reduce, options, cancellationToken); }
        public Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<T, TResult> options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.MapReduceAsync(map, reduce, options, cancellationToken); }
        public IFilteredMongoCollection<TDerivedDocument> OfType<TDerivedDocument>() where TDerivedDocument : T
        { return _Repository.OfType<TDerivedDocument>(); }
        public ReplaceOneResult ReplaceOne(FilterDefinition<T> filter, T replacement, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.ReplaceOne(filter, replacement, options, cancellationToken); }
        public Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<T> filter, T replacement, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.ReplaceOneAsync(filter, replacement, options, cancellationToken); }
        public UpdateResult UpdateMany(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.UpdateMany(filter, update, options, cancellationToken); }
        public Task<UpdateResult> UpdateManyAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.UpdateManyAsync(filter, update, options, cancellationToken); }
        public UpdateResult UpdateOne(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.UpdateOne(filter, update, options, cancellationToken); }
        public Task<UpdateResult> UpdateOneAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        { return _Repository.UpdateOneAsync(filter, update, options, cancellationToken); }
        public IMongoCollection<T> WithReadConcern(ReadConcern readConcern)
        { return _Repository.WithReadConcern(readConcern); }
        public IMongoCollection<T> WithReadPreference(ReadPreference readPreference)
        { return _Repository.WithReadPreference(readPreference); }
        public IMongoCollection<T> WithWriteConcern(WriteConcern writeConcern)
        { return _Repository.WithWriteConcern(writeConcern); }
        #endregion
    }
}