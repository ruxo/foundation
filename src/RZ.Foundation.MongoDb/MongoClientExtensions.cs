using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MongoDB.Driver;
using RZ.Foundation.Types;
using static RZ.Foundation.MongoDb.MongoHelper;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;
// ReSharper disable SuspiciousTypeConversion.Global

namespace RZ.Foundation.MongoDb;

[PublicAPI]
public static class MongoClientExtensions
{
    #region Retrieval

    /// <summary>
    /// Get the first element that satisfies the predicate.
    /// </summary>
    [Pure]
    public static Task<T?> Get<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> predicate, CancellationToken cancel = default)
        => ExecuteNullable(async () => {
            using var cursor = await collection.FindAsync(predicate, cancellationToken: cancel);
            return await cursor.FirstOrDefaultAsync(cancellationToken: cancel);
        });

    [Pure]
    public static Task<T?> GetById<T, TKey>(this IMongoCollection<T> collection, TKey id, CancellationToken cancel = default)
        => ExecuteNullable(async () => {
            var filter = Builders<T>.Filter.Eq(new StringFieldDefinition<T, TKey>("Id"), id);
            using var cursor = await collection.FindAsync(filter, cancellationToken: cancel);
            return await cursor.FirstOrDefaultAsync(cancellationToken: cancel);
        });

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<TResult> Retrieve<T, TResult>(this IAsyncCursor<T> cursor, Func<IAsyncCursor<T>, Task<TResult>> chain)
        => Execute(async () => {
            using var c = cursor;
            return await chain(c);
        });

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<TResult> Retrieve<T, TResult>(this Task<IAsyncCursor<T>> cursor, Func<IAsyncCursor<T>, Task<TResult>> chain)
        => Execute(async () => {
            using var c = await cursor;
            return await chain(c);
        });

    #endregion

    #region Add new

    public static Task<Outcome<T>> TryAdd<T>(this IMongoCollection<T> collection, T data, CancellationToken cancel = default)
        => TryExecute(async () => {
            await collection.InsertOneAsync(data, cancellationToken: cancel);
            return data;
        });

    public static Task<T> Add<T>(this IMongoCollection<T> collection, T data, CancellationToken cancel = default)
        => Execute(async () => {
            await collection.InsertOneAsync(data, cancellationToken: cancel);
            return data;
        });

    #endregion

    #region Updates

    static readonly ReplaceOptions ReplaceUpsertOption = new() { IsUpsert = true };

    static async Task<Outcome<T>> PureUpdate<T>(this IMongoCollection<T> collection, T data, FilterDefinition<T> predicate,
                                                bool upsert, CancellationToken cancel) {
        var option = upsert ? ReplaceUpsertOption : null;
        var result = await collection.ReplaceOneAsync(predicate, data, option, cancel);
        return InterpretUpdateResult(data, result);
    }

    static (T, FilterDefinition<T>) GetUpdateCondition<T, TKey>(T data, TimeProvider? clock) where T : IHaveKey<TKey>
        => data is ICanUpdateVersion<T> duv
               ? (GetFinal(duv, clock), Build<T>.Predicate(data.Id, duv.Version))
               : (data, Build<T>.Predicate(data.Id));

    public static Task<Outcome<T>> TryUpdate<T>(this IMongoCollection<T> collection,
                                                T data,
                                                Expression<Func<T, bool>> predicate,
                                                bool upsert = false,
                                                CancellationToken cancel = default)
        => TryExecute(() => collection.PureUpdate(data, predicate, upsert, cancel));

    public static Task<Outcome<T>> TryUpdate<T, TKey>(this IMongoCollection<T> collection, TKey key, T data, VersionType? current = null,
                                                      bool upsert = false, CancellationToken cancel = default)
        => TryExecute(() => collection.PureUpdate(data,
                                                  current is null? Build<T>.Predicate(key) : Build<T>.Predicate(key, current.Value),
                                                  upsert, cancel));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<Outcome<T>> TryUpdate<T, TKey>(this IMongoCollection<T> collection, T data,
                                                      bool upsert = false,
                                                      TimeProvider? clock = null,
                                                      CancellationToken cancel = default)
        where T : IHaveKey<TKey>
        => TryExecute(() => {
            var (final, predicate) = GetUpdateCondition<T, TKey>(data, clock);
            return collection.PureUpdate(final, predicate, upsert, cancel);
        });

    public static Task<T> Update<T>(this IMongoCollection<T> collection,
                                    T data,
                                    Expression<Func<T, bool>> predicate,
                                    bool upsert = false,
                                    CancellationToken cancel = default)
        => Execute(async () => {
            var result = await collection.PureUpdate(data, predicate, upsert, cancel);
            return result.IfSuccess(out var v, out var e) ? v : throw new ErrorInfoException(e);
        });

    public static Task<T> Update<T, TKey>(this IMongoCollection<T> collection, TKey key, T data, VersionType? current = null,
                                          bool upsert = false,
                                          CancellationToken cancel = default)
        => Execute(async () => {
            var result = await collection.PureUpdate(data,
                                                     current is null ? Build<T>.Predicate(key) : Build<T>.Predicate(key, current.Value),
                                                     upsert, cancel);
            return result.IfSuccess(out var v, out var e) ? v : throw new ErrorInfoException(e);
        });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<T> Update<T, TKey>(this IMongoCollection<T> collection, T data,
                                          bool upsert = false,
                                          TimeProvider? clock = null,
                                          CancellationToken cancel = default)
        where T : IHaveKey<TKey>
        => Execute(async () => {
            var (final, predicate) = GetUpdateCondition<T, TKey>(data, clock);
            var result = await collection.PureUpdate(final, predicate, upsert, cancel);
            return result.IfSuccess(out var v, out var e) ? v : throw new ErrorInfoException(e);
        });

    #endregion

    #region Upsert

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<Outcome<T>> TryUpsert<T>(this IMongoCollection<T> collection, T data, Expression<Func<T, bool>> predicate, CancellationToken cancel = default)
        => collection.TryUpdate(data, predicate, upsert: true, cancel: cancel);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<Outcome<T>> TryUpsert<T, TKey>(this IMongoCollection<T> collection, TKey key, T data, VersionType? current = null,
                                                      CancellationToken cancel = default)
        => collection.TryUpdate(key, data, current, upsert: true, cancel: cancel);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<Outcome<T>> TryUpsert<T, TKey>(this IMongoCollection<T> collection, T data,
                                                      TimeProvider? clock = null,
                                                      CancellationToken cancel = default)
        where T : IHaveKey<TKey>
        => collection.TryUpdate<T, TKey>(data, upsert: true, clock, cancel);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<T> Upsert<T>(this IMongoCollection<T> collection, T data, Expression<Func<T, bool>> predicate, CancellationToken cancel = default)
        => collection.Update(data, predicate, upsert: true, cancel: cancel);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<T> Upsert<T, TKey>(this IMongoCollection<T> collection, TKey key, T data, VersionType? current = null,
                                          CancellationToken cancel = default)
        => collection.Update(key, data, current, upsert: true, cancel: cancel);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<T> Upsert<T, TKey>(this IMongoCollection<T> collection, T data,
                                          TimeProvider? clock = null,
                                          CancellationToken cancel = default)
        where T : IHaveKey<TKey>
        => collection.Update<T, TKey>(data, upsert: true, clock, cancel);

    #endregion

    #region Deletion

    public static Task<Outcome<Unit>> TryDeleteAll<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> predicate, CancellationToken cancel = default)
        => TryExecute(async () => {
            await collection.DeleteManyAsync(predicate, cancel);
            return unit;
        });

    public static Task<Outcome<Unit>> TryDelete<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> predicate, CancellationToken cancel = default)
        => TryExecute(async () => {
            await collection.DeleteOneAsync(predicate, cancel);
            return unit;
        });

    public static Task<Outcome<Unit>> TryDelete<T, TKey>(this IMongoCollection<T> collection, TKey key, VersionType? current = null, CancellationToken cancel = default)
        => TryExecute(async () => {
            var filter = current is null? Build<T>.Predicate(key) : Build<T>.Predicate(key, current.Value);
            await collection.DeleteOneAsync(filter, cancel);
            return unit;
        });

    public static Task<Outcome<Unit>> TryDelete<T, TKey>(this IMongoCollection<T> collection, T data, CancellationToken cancel = default) where T : IHaveKey<TKey>
        => collection.TryDelete(data.Id, (data as IHaveVersion)?.Version, cancel);

    public static Task DeleteAll<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> predicate, CancellationToken cancel = default)
        => Execute(() => collection.DeleteManyAsync(predicate, cancel));

    public static Task Delete<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> predicate, CancellationToken cancel = default)
        => Execute(() => collection.DeleteOneAsync(predicate, cancel));

    public static Task Delete<T, TKey>(this IMongoCollection<T> collection, TKey key, VersionType? current = null, CancellationToken cancel = default)
        => Execute(async () => {
            var filter = current is null? Build<T>.Predicate(key) : Build<T>.Predicate(key, current.Value);
            await collection.DeleteOneAsync(filter, cancel);
        });

    public static Task Delete<T, TKey>(this IMongoCollection<T> collection, T data, CancellationToken cancel = default) where T : IHaveKey<TKey>
        => collection.Delete(data.Id, (data as IHaveVersion)?.Version, cancel);

    #endregion

    static T GetFinal<T>(ICanUpdateVersion<T> data, TimeProvider? clock)
        => data.WithVersion(clock?.GetUtcNow() ?? DateTimeOffset.UtcNow, data.Version + 1);

    static class Build<T>
    {
        public static FilterDefinition<T> Predicate<TKey>(TKey id)
            => Builders<T>.Filter.Eq(new StringFieldDefinition<T, TKey>("Id"), id);

        public static FilterDefinition<T> Predicate<TKey>(TKey id, VersionType version)
            => Builders<T>.Filter.And(Predicate(id),
                                      Builders<T>.Filter.Eq(new StringFieldDefinition<T, VersionType>(nameof(IHaveVersion.Version)), version));
    }

    static ErrorInfo? InterpretReplaceResult(ReplaceOneResult result) =>
        result.IsAcknowledged
            ? result.UpsertedId is null && result is { ModifiedCount: 0, MatchedCount: 0 }
                  ? new ErrorInfo(StandardErrorCodes.RaceCondition, "Data has changed externally")
                  : null
            : new ErrorInfo(StandardErrorCodes.DatabaseTransactionError, "Failed to update the data", result.ToString());

    static Outcome<T> InterpretUpdateResult<T>(T data, ReplaceOneResult result) {
        var error = InterpretReplaceResult(result);
        return error is null ? data : error;
    }
}