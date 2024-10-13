﻿using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using RZ.Foundation.Types;

namespace RZ.Foundation.MongoDb;

[PublicAPI]
public static class MongoHelper
{
    public static void SetupMongoStandardMappings() {
        BsonSerializer.RegisterSerializer(new MongoDB.Bson.Serialization.Serializers.DateTimeOffsetSerializer(BsonType.DateTime));

        var pack = new ConventionPack{ new EnumRepresentationConvention(BsonType.String) };
        ConventionRegistry.Register("EnumString", pack, _ => true);
    }

    [Pure]
    public static ErrorInfo? TryInterpretDatabaseError(Exception e)
        => e is MongoWriteException mongoException && mongoException.WriteError.Category == ServerErrorCategory.DuplicateKey
               ? new ErrorInfo(StandardErrorCodes.Duplication, "Either data identity, name, or both are already existed", e.ToString())
               : e is MongoException
                   ? new ErrorInfo(StandardErrorCodes.DatabaseTransactionError, e.Message, e.ToString())
                   : null;

    [Pure]
    public static ErrorInfo InterpretDatabaseError(Exception e)
        => TryInterpretDatabaseError(e) ?? ErrorFrom.Exception(e);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<T> Try<T>(Func<T> f) {
        try{
            return f();
        }
        catch (Exception e){
            return InterpretDatabaseError(e);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Outcome<T>> TryAsync<T>(Func<Task<T>> f) {
        try{
            return await f();
        }
        catch (Exception e){
            return InterpretDatabaseError(e);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Outcome<Unit>> TryAsync(Func<Task> f) {
        try{
            await f();
            return unit;
        }
        catch (Exception e){
            return InterpretDatabaseError(e);
        }
    }

    public static async Task Execute(Func<Task> f) {
        try{
            await f();
        }
        catch (MongoException e){
            throw new ErrorInfoException(InterpretDatabaseError(e));
        }
    }

    public static async Task<T?> ExecuteNullable<T>(Func<Task<T?>> f) {
        try{
            return await f();
        }
        catch (MongoException e){
            throw new ErrorInfoException(InterpretDatabaseError(e));
        }
    }

    public static async Task<T> Execute<T>(Func<Task<T>> f) {
        try{
            return await f();
        }
        catch (MongoException e){
            throw new ErrorInfoException(InterpretDatabaseError(e));
        }
    }

    public static async Task<Outcome<T>> TryExecute<T>(Func<Task<Outcome<T>>> f) {
        try{
            return await f();
        }
        catch (MongoException e){
            return InterpretDatabaseError(e);
        }
    }

    public static async Task<Outcome<T>> TryExecute<T>(Func<Task<T>> f) {
        try{
            return await f();
        }
        catch (MongoException e){
            return InterpretDatabaseError(e);
        }
    }
}