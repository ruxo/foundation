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
    public static ValueTask<T?> Get<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> predicate, CancellationToken cancel = default)
        => ExecuteNullable(async () => {
            using var cursor = await collection.FindAsync(predicate, cancellationToken: cancel);
            return await cursor.FirstOrDefaultAsync(cancellationToken: cancel);
        });

    [Pure]
    public static ValueTask<T?> GetById<T, TKey>(this IMongoCollection<T> collection, TKey id, CancellationToken cancel = default)
        => ExecuteNullable(async () => {
            var filter = Builders<T>.Filter.Eq(new StringFieldDefinition<T, TKey>("Id"), id);
            using var cursor = await collection.FindAsync(filter, cancellationToken: cancel);
            return await cursor.FirstOrDefaultAsync(cancellationToken: cancel);
        });

    #endregion

    #region Add new

    public static ValueTask<Outcome<T>> TryAdd<T>(this IMongoCollection<T> collection, T data, TimeProvider? clock = null, CancellationToken cancel = default)
        => TryExecute(async () => {
            await collection.InsertOneAsync(GetFinal(data, clock), cancellationToken: cancel);
            return data;
        });

    public static ValueTask<T> Add<T>(this IMongoCollection<T> collection, T data, TimeProvider? clock = null, CancellationToken cancel = default)
        => Execute(async () => {
            await collection.InsertOneAsync(GetFinal(data, clock), cancellationToken: cancel);
            return data;
        });

    #endregion

    #region Updates

    static readonly ReplaceOptions ReplaceUpsertOption = new() { IsUpsert = true };

    public static ValueTask<Outcome<T>> TryUpdate<T>(this IMongoCollection<T> collection,
                                                     T data,
                                                     Expression<Func<T, bool>> predicate,
                                                     bool upsert = false,
                                                     TimeProvider? clock = null,
                                                     CancellationToken cancel = default)
        => TryExecute(async () => {
            var final = GetFinal(data, clock);
            var option = upsert? ReplaceUpsertOption : null;
            var result = await collection.ReplaceOneAsync(predicate, final, option, cancel);
            return InterpretUpdateResult(data, result);
        });

    public static ValueTask<Outcome<T>> TryUpdate<T, TKey>(this IMongoCollection<T> collection, TKey key, T data,
                                                           bool upsert = false,
                                                           TimeProvider? clock = null,
                                                           CancellationToken cancel = default)
        where T : IHaveVersion
        => TryExecute(async () => {
            var (final, predicate) = BuildPredicate(key, data, clock);
            var option = upsert ? ReplaceUpsertOption : null;
            var result = await collection.ReplaceOneAsync(predicate, final, option, cancel);
            return InterpretUpdateResult(data, result);
        });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Outcome<T>> TryUpdate<T, TKey>(this IMongoCollection<T> collection, T data,
                                                           bool upsert = false,
                                                           TimeProvider? clock = null,
                                                           CancellationToken cancel = default)
        where T : IHaveKey<TKey>, IHaveVersion
        => collection.TryUpdate(data.Id, data, upsert, clock, cancel);

    public static ValueTask<T> Update<T>(this IMongoCollection<T> collection,
                                         T data,
                                         Expression<Func<T, bool>> predicate,
                                         bool upsert = false,
                                         TimeProvider? clock = null,
                                         CancellationToken cancel = default)
        => Execute(async () => {
            var final = GetFinal(data, clock);
            var option = upsert ? ReplaceUpsertOption : null;
            var result = await collection.ReplaceOneAsync(predicate, final, option, cancel);
            return InterpretUpdateResult(data, result).IfSuccess(out var v, out var e) ? v : throw new ErrorInfoException(e);
        });

    public static ValueTask<T> Update<T, TKey>(this IMongoCollection<T> collection, TKey key, T data,
                                               bool upsert = false,
                                               TimeProvider? clock = null,
                                               CancellationToken cancel = default)
        where T : IHaveVersion
        => Execute(async () => {
            var (final, predicate) = BuildPredicate(key, data, clock);
            var option = upsert ? ReplaceUpsertOption : null;
            var result = await collection.ReplaceOneAsync(predicate, final, option, cancel);
            return InterpretUpdateResult(data, result).IfSuccess(out var v, out var e) ? v : throw new ErrorInfoException(e);
        });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<T> Update<T, TKey>(this IMongoCollection<T> collection, T data,
                                                           bool upsert = false,
                                                           TimeProvider? clock = null,
                                                           CancellationToken cancel = default)
        where T : IHaveKey<TKey>, IHaveVersion
        => collection.Update(data.Id, data, upsert, clock, cancel);

    #endregion

    #region Upsert

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Outcome<T>> TryUpsert<T>(this IMongoCollection<T> collection, T data, Expression<Func<T, bool>> predicate, CancellationToken cancel = default)
        => TryUpdate(collection, data, predicate, upsert: true, cancel: cancel);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Outcome<T>> TryUpsert<T, TKey>(this IMongoCollection<T> collection, TKey key, T data,
                                                           TimeProvider? clock = null,
                                                           CancellationToken cancel = default)
        where T : IHaveVersion
        => TryUpdate(collection, key, data, upsert: true, clock, cancel);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Outcome<T>> TryUpsert<T, TKey>(this IMongoCollection<T> collection, T data,
                                                           TimeProvider? clock = null,
                                                           CancellationToken cancel = default)
        where T : IHaveKey<TKey>, IHaveVersion
        => TryUpdate(collection, data.Id, data, upsert: true, clock, cancel);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<T> Upsert<T>(this IMongoCollection<T> collection, T data, Expression<Func<T, bool>> predicate, CancellationToken cancel = default)
        => Update(collection, data, predicate, upsert: true, cancel: cancel);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<T> Upsert<T, TKey>(this IMongoCollection<T> collection, TKey key, T data,
                                                           TimeProvider? clock = null,
                                                           CancellationToken cancel = default)
        where T : IHaveVersion
        => Update(collection, key, data, upsert: true, clock, cancel);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<T> Upsert<T, TKey>(this IMongoCollection<T> collection, T data,
                                                           TimeProvider? clock = null,
                                                           CancellationToken cancel = default)
        where T : IHaveKey<TKey>, IHaveVersion
        => Update(collection, data.Id, data, upsert: true, clock, cancel);

    #endregion

    #region Deletion

    public static ValueTask<Outcome<Unit>> TryDeleteAll<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> predicate, CancellationToken cancel = default)
        => TryExecute(async () => {
            await collection.DeleteManyAsync(predicate, cancel);
            return unit;
        });

    public static ValueTask<Outcome<Unit>> TryDelete<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> predicate, CancellationToken cancel = default)
        => TryExecute(async () => {
            await collection.DeleteOneAsync(predicate, cancel);
            return unit;
        });

    public static ValueTask<Outcome<Unit>> TryDelete<T, TKey>(this IMongoCollection<T> collection, TKey key, T data, CancellationToken cancel = default)
        where T : IHaveVersion
        => TryExecute(async () => {
            var filter = BuildPredicate<T, TKey>(key, data.Version);
            await collection.DeleteOneAsync(filter, cancel);
            return unit;
        });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Outcome<Unit>> TryDelete<T, TKey>(this IMongoCollection<T> collection, T data, CancellationToken cancel = default)
        where T : IHaveKey<TKey>, IHaveVersion
        => collection.TryDelete(data.Id, data, cancel);

    public static ValueTask DeleteAll<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> predicate, CancellationToken cancel = default)
        => Execute(async () => {
            await collection.DeleteManyAsync(predicate, cancel);
        });

    public static ValueTask Delete<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> predicate, CancellationToken cancel = default)
        => Execute(async () => {
            await collection.DeleteOneAsync(predicate, cancel);
        });

    public static ValueTask Delete<T, TKey>(this IMongoCollection<T> collection, TKey key, T data, CancellationToken cancel = default)
        where T : IHaveVersion
        => Execute(async () => {
            var filter = BuildPredicate<T, TKey>(key, data.Version);
            await collection.DeleteOneAsync(filter, cancel);
        });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask Delete<T, TKey>(this IMongoCollection<T> collection, T data, CancellationToken cancel = default)
        where T : IHaveKey<TKey>, IHaveVersion
        => collection.Delete(data.Id, data, cancel);

    #endregion

    static T GetFinal<T>(T data, TimeProvider? clock, out ICanUpdateVersion<T>? duv) {
        duv = data as ICanUpdateVersion<T>;
        return duv is null ? data : duv.WithVersion(clock?.GetUtcNow() ?? DateTimeOffset.UtcNow, duv.Version + 1);
    }

    static T GetFinal<T>(T data, TimeProvider? clock)
        => GetFinal(data, clock, out _);

    static FilterDefinition<T> BuildPredicate<T, TKey>(TKey id, ulong version)
        where T : IHaveVersion
        => Builders<T>.Filter.And(Builders<T>.Filter.Eq(new StringFieldDefinition<T,TKey>("Id"), id),
                                  Builders<T>.Filter.Eq(x => x.Version, version));

    static (T Updated, FilterDefinition<T> Predicate) BuildPredicate<T, TKey>(TKey key, T data, TimeProvider? clock)
        where T : IHaveVersion {
        var final = GetFinal(data, clock, out var duv);
        var current = duv is null ? final.Version - 1 : final.Version;
        var predicate = BuildPredicate<T,TKey>(key, current);
        return (final, predicate);
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