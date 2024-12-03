using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using JetBrains.Annotations;
using MongoDB.Driver;

namespace RZ.Foundation.MongoDb.Migration;

[PublicAPI]
public static class Migration
{
    [PublicAPI]
    public sealed class Validation
    {
        readonly string[] names;
        public static Validation Requires(params string[] names) => new(names);
        public static Validation Requires(IEnumerable<string> names) => new(names.AsArray());

        public static Validation Requires<T>(string[]? excepts = null) {
            var finalExcepts = excepts ?? [];
            return Requires(from prop in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            where prop.Name != "Id" && !finalExcepts.Contains(prop.Name)
                            select prop.Name);
        }

        Validation(string[] names) {
            this.names = names;
        }

        public object WithProperties(object specs) =>
            new{
                bsonType = "object",
                required = names,
                properties = specs
            };
    }

    public static void CreateCollection<T>(this IMongoDatabase db, IClientSessionHandle session, object specs, string? dbName = null)
        => db.CreateCollection(session, dbName ?? MongoHelper.GetCollectionName<T>(),
                               new CreateCollectionOptions<T> {
                                   ValidationAction = DocumentValidationAction.Error,
                                   ValidationLevel = DocumentValidationLevel.Strict,
                                   Validator = new JsonFilterDefinition<T>(MongoValidation(specs).ToString())
                               });

    #region Mongo Collection Extension

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IMongoCollection<T> Collection<T>(this IMongoDatabase db)
        => db.GetCollection<T>(MongoHelper.GetCollectionName<T>());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IMongoCollection<T> CreateUniqueIndex<T>(this IMongoCollection<T> collection,
                                                     string name,
                                                     Func<IndexKeysDefinitionBuilder<T>, IndexKeysDefinition<T>> builder)
        => CreateIndex(collection, name, builder, unique: true);

    public static IMongoCollection<T> CreateIndex<T>(this IMongoCollection<T> collection,
                                                     string name,
                                                     Func<IndexKeysDefinitionBuilder<T>, IndexKeysDefinition<T>> builder,
                                                     bool unique = false) {
        var opts = new CreateIndexModel<T>(builder(Builders<T>.IndexKeys), new(){ Unique = unique, Name = name });
        collection.Indexes.CreateOne(opts);
        return collection;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IMongoCollection<T> DropIndex<T>(this IMongoCollection<T> collection, string name) {
        collection.Indexes.DropOne(name);
        return collection;
    }

    #endregion

    [PublicAPI]
    public sealed class MigrationMongoBuilder<T>(IMongoDatabase database)
    {
        readonly List<CreateIndexModel<T>> indexModels = new();
        object? validation;
        readonly string dbName = MongoHelper.GetCollectionName<T>();

        public MigrationMongoBuilder<T> WithSchema(object specs) {
            Debug.Assert(validation is null);
            validation = specs;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MigrationMongoBuilder<T> UniqueIndex(string name, Func<IndexKeysDefinitionBuilder<T>, IndexKeysDefinition<T>> builder) => Index(name, builder, unique: true);

        public MigrationMongoBuilder<T> Index(string name, Func<IndexKeysDefinitionBuilder<T>, IndexKeysDefinition<T>> builder, bool unique = false) {
            indexModels.Add(new(builder(Builders<T>.IndexKeys), new(){ Unique = unique, Name = name }));
            return this;
        }

        public void Run(IClientSessionHandle session) {
            if (validation is not null)
                database.CreateCollection<T>(session, validation, dbName);
            if (indexModels.Count != 0)
                database.Collection<T>().Indexes.CreateMany(session, indexModels);
        }

        public void Run() {
            using var session = database.Client.StartSession();
            // if the using MongoDB doesn't support transaction, ignore it.
            var transactionSupported = TryCatch(() => {
                    session.StartTransaction();
                    return Unit.Default;
                })
               .ToOption();
            Run(session);
            if (transactionSupported.IsSome)
                session.CommitTransaction();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MigrationMongoBuilder<T> Build<T>(this IMongoDatabase db) => new(db);

    static JsonObject MongoValidation(object specs) => new(){ { "$jsonSchema", JsonSerializer.SerializeToNode(specs) } };
}