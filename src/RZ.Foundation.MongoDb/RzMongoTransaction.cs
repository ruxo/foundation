using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Search;

namespace RZ.Foundation.MongoDb;

[PublicAPI]
public interface IRzMongoTransaction : IRzMongoDatabase, IAsyncDisposable
{
    Guid Id { get; }

    /// <summary>
    /// I hope you know what you are doing :)
    /// </summary>
    IClientSessionHandle Session { get; }

    Task Commit();
    Task Rollback();
}

[PublicAPI]
public class RzMongoTransaction(Guid id,IMongoDatabase db, IClientSessionHandle session) : IRzMongoTransaction
{
    bool? commit;

    [ExcludeFromCodeCoverage]
    public Guid Id => id;

    [ExcludeFromCodeCoverage]
    public IClientSessionHandle Session => session;

    public async Task Commit() {
        await session.CommitTransactionAsync();

        // no exception trap is needed.. if commit failed, just leave commit = false to abort this transaction
        commit = true;
    }

    public async Task Rollback() {
        await session.AbortTransactionAsync();
        commit = false;
    }

    public IMongoCollection<T> GetCollection<T>()
    => new Wrapper<T>(db.GetCollection<T>(MongoHelper.GetCollectionName<T>()), session);

    public async ValueTask DisposeAsync() {
        try{
            if (commit is null)
                await Rollback();
        }
        catch (Exception ex){
            Trace.WriteLine($"WARN: Session {Id} cannot be rolled back: {ex}");
        }
        finally {
            session.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    #region Collection with session

    [ExcludeFromCodeCoverage]
    sealed class Wrapper<T>(IMongoCollection<T> collection, IClientSessionHandle current) : IMongoCollection<T>
    {
        public IAsyncCursor<TResult> Aggregate<TResult>(PipelineDefinition<T, TResult> pipeline, AggregateOptions? options = null, CancellationToken cancellationToken = new())
            => collection.Aggregate(current, pipeline, options, cancellationToken);

        public IAsyncCursor<TResult> Aggregate<TResult>(IClientSessionHandle session, PipelineDefinition<T, TResult> pipeline, AggregateOptions? options = null, CancellationToken cancellationToken = new())
            => collection.Aggregate(session, pipeline, options, cancellationToken);

        public Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(PipelineDefinition<T, TResult> pipeline, AggregateOptions? options = null, CancellationToken cancellationToken = new())
        => collection.AggregateAsync(current, pipeline, options, cancellationToken);

        public Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(IClientSessionHandle session, PipelineDefinition<T, TResult> pipeline, AggregateOptions? options = null, CancellationToken cancellationToken = new())
        => collection.AggregateAsync(session, pipeline, options, cancellationToken);

        public void AggregateToCollection<TResult>(PipelineDefinition<T, TResult> pipeline, AggregateOptions? options = null, CancellationToken cancellationToken = new())
        => collection.AggregateToCollection(current, pipeline, options, cancellationToken);

        public void AggregateToCollection<TResult>(IClientSessionHandle session, PipelineDefinition<T, TResult> pipeline, AggregateOptions? options = null, CancellationToken cancellationToken = new())
        => collection.AggregateToCollection(session, pipeline, options, cancellationToken);

        public Task AggregateToCollectionAsync<TResult>(PipelineDefinition<T, TResult> pipeline, AggregateOptions? options = null, CancellationToken cancellationToken = new())
        => collection.AggregateToCollectionAsync(current, pipeline, options, cancellationToken);

        public Task AggregateToCollectionAsync<TResult>(IClientSessionHandle session, PipelineDefinition<T, TResult> pipeline, AggregateOptions? options = null, CancellationToken cancellationToken = new())
        => collection.AggregateToCollectionAsync(session, pipeline, options, cancellationToken);

        public BulkWriteResult<T> BulkWrite(IEnumerable<WriteModel<T>> requests, BulkWriteOptions? options = null, CancellationToken cancellationToken = new())
        => collection.BulkWrite(current, requests, options, cancellationToken);

        public BulkWriteResult<T> BulkWrite(IClientSessionHandle session, IEnumerable<WriteModel<T>> requests, BulkWriteOptions? options = null, CancellationToken cancellationToken = new())
        => collection.BulkWrite(session, requests, options, cancellationToken);

        public  Task<BulkWriteResult<T>> BulkWriteAsync(IEnumerable<WriteModel<T>> requests, BulkWriteOptions? options = null, CancellationToken cancellationToken = new())
        => collection.BulkWriteAsync(current, requests, options, cancellationToken);

        public  Task<BulkWriteResult<T>> BulkWriteAsync(IClientSessionHandle session, IEnumerable<WriteModel<T>> requests, BulkWriteOptions? options = null, CancellationToken cancellationToken = new())
        => collection.BulkWriteAsync(session, requests, options, cancellationToken);

        public long Count(FilterDefinition<T> filter, CountOptions? options = null, CancellationToken cancellationToken = new())
            => throw new NotSupportedException();

        public long Count(IClientSessionHandle session, FilterDefinition<T> filter, CountOptions? options = null, CancellationToken cancellationToken = new())
            => throw new NotSupportedException();

        public  Task<long> CountAsync(FilterDefinition<T> filter, CountOptions? options = null, CancellationToken cancellationToken = new())
            => throw new NotSupportedException();

        public  Task<long> CountAsync(IClientSessionHandle session, FilterDefinition<T> filter, CountOptions? options = null, CancellationToken cancellationToken = new())
            => throw new NotSupportedException();

        public long CountDocuments(FilterDefinition<T> filter, CountOptions? options = null, CancellationToken cancellationToken = new())
        => collection.CountDocuments(current, filter, options, cancellationToken);

        public long CountDocuments(IClientSessionHandle session, FilterDefinition<T> filter, CountOptions? options = null, CancellationToken cancellationToken = new())
        => collection.CountDocuments(session, filter, options, cancellationToken);

        public  Task<long> CountDocumentsAsync(FilterDefinition<T> filter, CountOptions? options = null, CancellationToken cancellationToken = new())
        => collection.CountDocumentsAsync(current, filter, options, cancellationToken);

        public  Task<long> CountDocumentsAsync(IClientSessionHandle session, FilterDefinition<T> filter, CountOptions? options = null, CancellationToken cancellationToken = new())
        => collection.CountDocumentsAsync(session, filter, options, cancellationToken);

        public DeleteResult DeleteMany(FilterDefinition<T> filter, CancellationToken cancellationToken = new())
        => collection.DeleteMany(current, filter, options: null, cancellationToken);

        public DeleteResult DeleteMany(FilterDefinition<T> filter, DeleteOptions options, CancellationToken cancellationToken = new())
        => collection.DeleteMany(current, filter, options, cancellationToken);

        public DeleteResult DeleteMany(IClientSessionHandle session, FilterDefinition<T> filter, DeleteOptions? options = null, CancellationToken cancellationToken = new())
        => collection.DeleteMany(session, filter, options, cancellationToken);

        public  Task<DeleteResult> DeleteManyAsync(FilterDefinition<T> filter, CancellationToken cancellationToken = new())
        => collection.DeleteManyAsync(current, filter, options: null, cancellationToken);

        public  Task<DeleteResult> DeleteManyAsync(FilterDefinition<T> filter, DeleteOptions options, CancellationToken cancellationToken = new())
        => collection.DeleteManyAsync(current, filter, options, cancellationToken);

        public  Task<DeleteResult> DeleteManyAsync(IClientSessionHandle session, FilterDefinition<T> filter, DeleteOptions? options = null, CancellationToken cancellationToken = new())
        => collection.DeleteManyAsync(session, filter, options, cancellationToken);

        public DeleteResult DeleteOne(FilterDefinition<T> filter, CancellationToken cancellationToken = new())
        => collection.DeleteOne(current, filter, options: null, cancellationToken);

        public DeleteResult DeleteOne(FilterDefinition<T> filter, DeleteOptions options, CancellationToken cancellationToken = new())
        => collection.DeleteOne(current, filter, options, cancellationToken);

        public DeleteResult DeleteOne(IClientSessionHandle session, FilterDefinition<T> filter, DeleteOptions? options = null, CancellationToken cancellationToken = new())
        => collection.DeleteOne(session, filter, options, cancellationToken);

        public  Task<DeleteResult> DeleteOneAsync(FilterDefinition<T> filter, CancellationToken cancellationToken = new())
        => collection.DeleteOneAsync(current, filter, options: null, cancellationToken);

        public  Task<DeleteResult> DeleteOneAsync(FilterDefinition<T> filter, DeleteOptions options, CancellationToken cancellationToken = new())
        => collection.DeleteOneAsync(current, filter, options, cancellationToken);

        public  Task<DeleteResult> DeleteOneAsync(IClientSessionHandle session, FilterDefinition<T> filter, DeleteOptions? options = null, CancellationToken cancellationToken = new())
        => collection.DeleteOneAsync(session, filter, options, cancellationToken);

        public IAsyncCursor<TField> Distinct<TField>(FieldDefinition<T, TField> field, FilterDefinition<T> filter, DistinctOptions? options = null, CancellationToken cancellationToken = new())
        => collection.Distinct(current, field, filter, options, cancellationToken);

        public IAsyncCursor<TField> Distinct<TField>(IClientSessionHandle session, FieldDefinition<T, TField> field, FilterDefinition<T> filter, DistinctOptions? options = null,
                                                     CancellationToken cancellationToken = new())
        => collection.Distinct(session, field, filter, options, cancellationToken);

        public  Task<IAsyncCursor<TField>> DistinctAsync<TField>(FieldDefinition<T, TField> field, FilterDefinition<T> filter, DistinctOptions? options = null, CancellationToken cancellationToken = new())
        => collection.DistinctAsync(current, field, filter, options, cancellationToken);

        public  Task<IAsyncCursor<TField>> DistinctAsync<TField>(IClientSessionHandle session, FieldDefinition<T, TField> field, FilterDefinition<T> filter, DistinctOptions? options = null, CancellationToken cancellationToken = new())
        => collection.DistinctAsync(session, field, filter, options, cancellationToken);

        public IAsyncCursor<TItem> DistinctMany<TItem>(FieldDefinition<T, IEnumerable<TItem>> field, FilterDefinition<T> filter, DistinctOptions? options = null, CancellationToken cancellationToken = new())
        => collection.DistinctMany(current, field, filter, options, cancellationToken);

        public IAsyncCursor<TItem> DistinctMany<TItem>(IClientSessionHandle session, FieldDefinition<T, IEnumerable<TItem>> field, FilterDefinition<T> filter, DistinctOptions? options = null, CancellationToken cancellationToken = new())
        => collection.DistinctMany(session, field, filter, options, cancellationToken);

        public  Task<IAsyncCursor<TItem>> DistinctManyAsync<TItem>(FieldDefinition<T, IEnumerable<TItem>> field, FilterDefinition<T> filter, DistinctOptions? options = null, CancellationToken cancellationToken = new())
        => collection.DistinctManyAsync(current, field, filter, options, cancellationToken);

        public  Task<IAsyncCursor<TItem>> DistinctManyAsync<TItem>(IClientSessionHandle session, FieldDefinition<T, IEnumerable<TItem>> field, FilterDefinition<T> filter, DistinctOptions? options = null, CancellationToken cancellationToken = new())
        => collection.DistinctManyAsync(session, field, filter, options, cancellationToken);

        public long EstimatedDocumentCount(EstimatedDocumentCountOptions? options = null, CancellationToken cancellationToken = new())
        => collection.EstimatedDocumentCount(options, cancellationToken);

        public  Task<long> EstimatedDocumentCountAsync(EstimatedDocumentCountOptions? options = null, CancellationToken cancellationToken = new())
        => collection.EstimatedDocumentCountAsync(options, cancellationToken);

        public IAsyncCursor<TProjection> FindSync<TProjection>(FilterDefinition<T> filter, FindOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => collection.FindSync(current, filter, options, cancellationToken);

        public IAsyncCursor<TProjection> FindSync<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, FindOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => collection.FindSync(session, filter, options, cancellationToken);

        public  Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(FilterDefinition<T> filter, FindOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => collection.FindAsync(current, filter, options, cancellationToken);

        public  Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, FindOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => collection.FindAsync(session, filter, options, cancellationToken);

        public TProjection FindOneAndDelete<TProjection>(FilterDefinition<T> filter, FindOneAndDeleteOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => collection.FindOneAndDelete(current, filter, options, cancellationToken);

        public TProjection FindOneAndDelete<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, FindOneAndDeleteOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => collection.FindOneAndDelete(session, filter, options, cancellationToken);

        public  Task<TProjection> FindOneAndDeleteAsync<TProjection>(FilterDefinition<T> filter, FindOneAndDeleteOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => collection.FindOneAndDeleteAsync(current, filter, options, cancellationToken);

        public  Task<TProjection> FindOneAndDeleteAsync<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, FindOneAndDeleteOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => collection.FindOneAndDeleteAsync(session, filter, options, cancellationToken);

        public TProjection FindOneAndReplace<TProjection>(FilterDefinition<T> filter, T replacement, FindOneAndReplaceOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => collection.FindOneAndReplace(current, filter, replacement, options, cancellationToken);

        public TProjection FindOneAndReplace<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, FindOneAndReplaceOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => collection.FindOneAndReplace(session, filter, replacement, options, cancellationToken);

        public  Task<TProjection> FindOneAndReplaceAsync<TProjection>(FilterDefinition<T> filter, T replacement, FindOneAndReplaceOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => collection.FindOneAndReplaceAsync(current, filter, replacement, options, cancellationToken);

        public  Task<TProjection> FindOneAndReplaceAsync<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, FindOneAndReplaceOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => collection.FindOneAndReplaceAsync(session, filter, replacement, options, cancellationToken);

        public TProjection FindOneAndUpdate<TProjection>(FilterDefinition<T> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => collection.FindOneAndUpdate(current, filter, update, options, cancellationToken);

        public TProjection FindOneAndUpdate<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => collection.FindOneAndUpdate(session, filter, update, options, cancellationToken);

        public  Task<TProjection> FindOneAndUpdateAsync<TProjection>(FilterDefinition<T> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => collection.FindOneAndUpdateAsync(current, filter, update, options, cancellationToken);

        public  Task<TProjection> FindOneAndUpdateAsync<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => collection.FindOneAndUpdateAsync(session, filter, update, options, cancellationToken);

        public void InsertOne(T document, InsertOneOptions? options = null, CancellationToken cancellationToken = new())
        => collection.InsertOne(current, document, options, cancellationToken);

        public void InsertOne(IClientSessionHandle session, T document, InsertOneOptions? options = null, CancellationToken cancellationToken = new())
        => collection.InsertOne(session, document, options, cancellationToken);

        public  Task InsertOneAsync(T document, CancellationToken cancellationToken)
        => collection.InsertOneAsync(current, document, options: null, cancellationToken);

        public  Task InsertOneAsync(T document, InsertOneOptions? options = null, CancellationToken cancellationToken = new())
        => collection.InsertOneAsync(current, document, options, cancellationToken);

        public  Task InsertOneAsync(IClientSessionHandle session, T document, InsertOneOptions? options = null, CancellationToken cancellationToken = new())
        => collection.InsertOneAsync(session, document, options, cancellationToken);

        public void InsertMany(IEnumerable<T> documents, InsertManyOptions? options = null, CancellationToken cancellationToken = new())
        => collection.InsertMany(current, documents, options, cancellationToken);

        public void InsertMany(IClientSessionHandle session, IEnumerable<T> documents, InsertManyOptions? options = null, CancellationToken cancellationToken = new())
        => collection.InsertMany(session, documents, options, cancellationToken);

        public  Task InsertManyAsync(IEnumerable<T> documents, InsertManyOptions? options = null, CancellationToken cancellationToken = new())
        => collection.InsertManyAsync(current, documents, options, cancellationToken);

        public  Task InsertManyAsync(IClientSessionHandle session, IEnumerable<T> documents, InsertManyOptions? options = null, CancellationToken cancellationToken = new())
        => collection.InsertManyAsync(session, documents, options, cancellationToken);

        [Obsolete]
        public IAsyncCursor<TResult> MapReduce<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<T, TResult>? options = null, CancellationToken cancellationToken = new())
            => throw new NotSupportedException();

        [Obsolete]
        public IAsyncCursor<TResult> MapReduce<TResult>(IClientSessionHandle session, BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<T, TResult>? options = null, CancellationToken cancellationToken = new())
            => throw new NotSupportedException();

        [Obsolete]
        public  Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<T, TResult>? options = null, CancellationToken cancellationToken = new())
            => throw new NotSupportedException();

        [Obsolete]
        public  Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(IClientSessionHandle session, BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<T, TResult>? options = null, CancellationToken cancellationToken = new())
            => throw new NotSupportedException();

        public IFilteredMongoCollection<TDerivedDocument> OfType<TDerivedDocument>() where TDerivedDocument : T
            => collection.OfType<TDerivedDocument>();

        public ReplaceOneResult ReplaceOne(FilterDefinition<T> filter, T replacement, ReplaceOptions? options = null, CancellationToken cancellationToken = new())
        => collection.ReplaceOne(current, filter, replacement, options, cancellationToken);

        public ReplaceOneResult ReplaceOne(FilterDefinition<T> filter, T replacement, UpdateOptions options, CancellationToken cancellationToken = new())
            => throw new NotSupportedException();

        public ReplaceOneResult ReplaceOne(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, ReplaceOptions? options = null, CancellationToken cancellationToken = new())
        => collection.ReplaceOne(session, filter, replacement, options, cancellationToken);

        public ReplaceOneResult ReplaceOne(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, UpdateOptions options, CancellationToken cancellationToken = new())
            => throw new NotSupportedException();

        public  Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<T> filter, T replacement, ReplaceOptions? options = null, CancellationToken cancellationToken = new())
        => collection.ReplaceOneAsync(current, filter, replacement, options, cancellationToken);

        public  Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<T> filter, T replacement, UpdateOptions options, CancellationToken cancellationToken = new())
            => throw new NotSupportedException();

        public  Task<ReplaceOneResult> ReplaceOneAsync(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, ReplaceOptions? options = null, CancellationToken cancellationToken = new())
        => collection.ReplaceOneAsync(session, filter, replacement, options, cancellationToken);

        public  Task<ReplaceOneResult> ReplaceOneAsync(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, UpdateOptions options, CancellationToken cancellationToken = new())
            => throw new NotSupportedException();

        public UpdateResult UpdateMany(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions? options = null, CancellationToken cancellationToken = new())
        => collection.UpdateMany(current, filter, update, options, cancellationToken);

        public UpdateResult UpdateMany(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions? options = null, CancellationToken cancellationToken = new())
        => collection.UpdateMany(session, filter, update, options, cancellationToken);

        public  Task<UpdateResult> UpdateManyAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions? options = null, CancellationToken cancellationToken = new())
        => collection.UpdateManyAsync(current, filter, update, options, cancellationToken);

        public  Task<UpdateResult> UpdateManyAsync(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions? options = null, CancellationToken cancellationToken = new())
        => collection.UpdateManyAsync(session, filter, update, options, cancellationToken);

        public UpdateResult UpdateOne(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions? options = null, CancellationToken cancellationToken = new())
        => collection.UpdateOne(current, filter, update, options, cancellationToken);

        public UpdateResult UpdateOne(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions? options = null, CancellationToken cancellationToken = new())
        => collection.UpdateOne(session, filter, update, options, cancellationToken);

        public  Task<UpdateResult> UpdateOneAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions? options = null, CancellationToken cancellationToken = new())
        => collection.UpdateOneAsync(current, filter, update, options, cancellationToken);

        public  Task<UpdateResult> UpdateOneAsync(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions? options = null, CancellationToken cancellationToken = new())
        => collection.UpdateOneAsync(session, filter, update, options, cancellationToken);

        public IChangeStreamCursor<TResult> Watch<TResult>(PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline, ChangeStreamOptions? options = null, CancellationToken cancellationToken = new())
        => collection.Watch(current, pipeline, options, cancellationToken);

        public IChangeStreamCursor<TResult> Watch<TResult>(IClientSessionHandle session, PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline, ChangeStreamOptions? options = null, CancellationToken cancellationToken = new())
        => collection.Watch(session, pipeline, options, cancellationToken);

        public  Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline, ChangeStreamOptions? options = null, CancellationToken cancellationToken = new())
        => collection.WatchAsync(current, pipeline, options, cancellationToken);

        public  Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(IClientSessionHandle session, PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline, ChangeStreamOptions? options = null, CancellationToken cancellationToken = new())
        => collection.WatchAsync(session, pipeline, options, cancellationToken);

        public IMongoCollection<T> WithReadConcern(ReadConcern readConcern)
            => new Wrapper<T>(collection.WithReadConcern(readConcern), current);

        public IMongoCollection<T> WithReadPreference(ReadPreference readPreference)
        => new Wrapper<T>(collection.WithReadPreference(readPreference), current);

        public IMongoCollection<T> WithWriteConcern(WriteConcern writeConcern)
        => new Wrapper<T>(collection.WithWriteConcern(writeConcern), current);

        public CollectionNamespace CollectionNamespace => collection.CollectionNamespace;
        public IMongoDatabase Database => collection.Database;
        public IBsonSerializer<T> DocumentSerializer => collection.DocumentSerializer;
        public IMongoIndexManager<T> Indexes => collection.Indexes;
        public IMongoSearchIndexManager SearchIndexes => collection.SearchIndexes;
        public MongoCollectionSettings Settings => collection.Settings;
    }

    #endregion
}