using HopInBE.DataAccess.IDataProvider;
using MongoDB.Driver;

namespace HopInBE.DataAccess.MongoDB
{
    public class MongoDbBaseRepository<TDocument> : IBaseRepository<TDocument>
        where TDocument : IDocument
    {
        /// <summary>
        /// This collection is used to interact with Database collection.
        /// </summary>
        public IMongoCollection<TDocument> collection { get; set; }
        private readonly string userId;

        private static readonly object lockMe = new object();
        private static IDbSettings dbSettings;
        private static MongoClient mongoClient = null;

        public static MongoClient Instance
        {
            get
            {
                if (mongoClient == null)
                {
                    lock (lockMe)
                    {
                        if (mongoClient == null)
                        {
                            mongoClient = new MongoClient(dbSettings.ConnectionString);
                        }
                    }
                }
                return mongoClient;
            }
        }

        /// <summary>
        /// MongoDbBaseRepository Constructor. 
        /// </summary>
        /// <param name="settings">DB setting object</param>
        public MongoDbBaseRepository(IMongoDatabase database, string userId = "")
        {
            this.userId = userId;
            ///var database = new MongoClient(settings.ConnectionString).GetDatabase(settings.DatabaseName);
            collection = database.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
        }

        /// <summary>
        /// This is contructor of MongoDbBaseRepository.
        /// </summary>
        /// <param name="settings"></param>
        public MongoDbBaseRepository(IDbSettings settings)
        {
            this.userId = settings.DatabaseAccessedBy;
            var database = new MongoClient(settings.ConnectionString).GetDatabase(settings.DatabaseName);
            collection = database.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
        }

        /// <summary>
        /// This is contructor of MongoDbBaseRepository.
        /// </summary>
        /// <param name="settings"></param>
        public MongoDbBaseRepository(IDbSettings settings, bool useSingleTon)
        {
            dbSettings = settings;
            this.userId = settings.DatabaseAccessedBy;
            var database = Instance.GetDatabase(settings.DatabaseName);
            collection = database.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
        }

        /// <summary>
        /// Get MongoDB collection name using document type.
        /// </summary>
        /// <param name="documentType">Document type.</param>
        /// <returns>Returns collection name</returns>
        private protected string GetCollectionName(Type documentType)
        {
            return ((BsonCollectionAttribute)documentType.GetCustomAttributes(
                    typeof(BsonCollectionAttribute),
                    true)
                .FirstOrDefault())?.CollectionName;
        }

        /// <summary>
        /// Creates a queryable source of documents.
        /// </summary>
        /// <returns>A queryable source of documents.</returns>
        public IQueryable<TDocument> AsQueryable()
        {
            return collection.AsQueryable();
        }

        /// <summary>
        /// Get List of documents.
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns> List of documents.</returns>
        public async Task<List<TDocument>> GetAllAsync(CancellationToken ct = default)
        {
            return await collection.Find(_ => true).ToListAsync(ct);
        }

        /// <summary>
        /// Get Single document by Id.
        /// </summary>
        /// <param name="id">Id of document.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>A Single document</returns>
        public async Task<TDocument> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
            return await GetAsync(filter); // collection.Find(filter).SingleOrDefault();
        }

        /// <summary>
        /// Get single document using filter criteria.
        /// </summary>
        /// <param name="filter">Filter criteria.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>A Single document.</returns>
        public async Task<TDocument> GetAsync(FilterDefinition<TDocument> filter, CancellationToken ct = default)
        {
            return await collection.Find(filter).SingleOrDefaultAsync(ct);
        }

        /// <summary>
        /// Get single document using filter criteria.
        /// </summary>
        /// <param name="filter">Filter criteria.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>A Single document.</returns>
        public async Task<List<TDocument>> GetFilterListAsync(FilterDefinition<TDocument> filter, CancellationToken ct = default)
        {
            return await collection.Find(filter).ToListAsync();
        }

        /// <summary>
        /// Get Mulitple document using filter, sort and find options criteria.
        /// </summary>
        /// <param name="filter">Filter criteria.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>A Single document.</returns>
        public async Task<List<TDocument>> GetKeySetPaginatedAsync(Pagination pagination, FilterDefinition<TDocument> customFilter, ProjectionDefinition<TDocument> projection, CancellationToken ct = default)
        {
            var filterBuilder = Builders<TDocument>.Filter;
            var filter = customFilter;

            var sortBuilder = Builders<TDocument>.Sort;
            SortDefinition<TDocument> sort;

            if (!string.IsNullOrEmpty(pagination.SortColumn))
            {
                pagination.SortColumn = !string.IsNullOrEmpty(pagination.SortColumn) ? pagination.SortColumn : "asc";
                if (pagination.SortDirection.ToLower() == "asc")
                    sort = sortBuilder.Ascending(pagination.SortColumn).Ascending(x => x.Id);
                else
                    sort = sortBuilder.Descending(pagination.SortColumn).Descending(x => x.Id);
            }
            else
            {
                sort = sortBuilder.Ascending(x => x.Id);
            }

            if (!string.IsNullOrEmpty(pagination.Id) && !string.IsNullOrEmpty(pagination.Action))
            {
                if (string.IsNullOrEmpty(pagination.SortColumn))
                {
                    if (pagination.Action.ToLower() == "next")
                        filter &= filterBuilder.Gt(e => e.Id, pagination.Id);
                    else if (pagination.Action.ToLower() == "previous")
                        filter &= filterBuilder.Lt(e => e.Id, pagination.Id);
                }
                else
                {
                    var sortFilterBuilder = Builders<TDocument>.Filter;
                    var sortFilter = sortFilterBuilder.Empty;
                    if (pagination.Action.ToLower() == "next")
                    {
                        sortFilter = sortFilterBuilder.Gt(pagination.SortColumn, pagination.SortColumnValue);
                        sortFilter |= sortFilterBuilder.Eq(pagination.SortColumn, pagination.SortColumnValue) & sortFilterBuilder.Gt(x => x.Id, pagination.Id);
                    }
                    else if (pagination.Action.ToLower() == "previous")
                    {
                        sortFilter = sortFilterBuilder.Lt(pagination.SortColumn, pagination.SortColumnValue);
                        sortFilter |= sortFilterBuilder.Eq(pagination.SortColumn, pagination.SortColumnValue) & sortFilterBuilder.Lt(x => x.Id, pagination.Id);
                    }

                    filter &= sortFilter;
                }
            }

            var findOption = new FindOptions<TDocument>
            {
                Sort = sort,
                Limit = pagination.PageSize,
                Projection = projection
            };
            return (await collection.FindAsync(filter, findOption, ct)).ToList();
        }

        /// <summary>
        /// Get Mulitple document using filter, sort and find options criteria.
        /// </summary>
        /// <param name="filter">Filter criteria.</param>
        /// <param name="sortDefinition">Sort criteria.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>List of document.</returns>
        public async Task<(int totalPages, IReadOnlyList<TDocument> data)> GetPaginatedAsync(FilterDefinition<TDocument> filterDefinition, SortDefinition<TDocument> sortDefinition, int page, int pageSize, ProjectionDefinition<TDocument> projection, CancellationToken ct = default)
        {
            var filterBuilder = Builders<TDocument>.Filter;
            var filter = filterDefinition;

            var countFacet = AggregateFacet.Create("count",
                PipelineDefinition<TDocument, AggregateCountResult>.Create(new[]
                {
                PipelineStageDefinitionBuilder.Count<TDocument>()
                }));

            var dataFacet = AggregateFacet.Create("data",
                PipelineDefinition<TDocument, TDocument>.Create(new[]
                {
                PipelineStageDefinitionBuilder.Sort(sortDefinition),
                PipelineStageDefinitionBuilder.Skip<TDocument>((page - 1) * pageSize),
                PipelineStageDefinitionBuilder.Limit<TDocument>(pageSize),
                PipelineStageDefinitionBuilder.Project<TDocument,TDocument>(projection),
                }));


            var aggregation = await collection.Aggregate()
                .Match(filter)
                .Facet(countFacet, dataFacet)
                .ToListAsync();

            var count = aggregation.First()
                .Facets.First(x => x.Name == "count")
                .Output<AggregateCountResult>()
                ?.FirstOrDefault()
                ?.Count;

            var totalPages = count == null ? 0 : (int)Math.Ceiling((double)count / pageSize);

            var data = aggregation.First()
                .Facets.First(x => x.Name == "data")
                .Output<TDocument>();

            return (totalPages, data);
        }

        /// <summary>
        /// Get Mulitple document using filter, sort and find options criteria.
        /// </summary>
        /// <param name="filter">Filter criteria.</param>
        /// <param name="sortDefinition">Sort criteria.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>List of document.</returns>
        public async Task<(int totalRecords, IReadOnlyList<TDocument> data)> GetPaginateAsync(FilterDefinition<TDocument> filterDefinition, SortDefinition<TDocument> sortDefinition, Pagination pagination, ProjectionDefinition<TDocument> projection = null, CancellationToken ct = default)
        {
            var filterBuilder = Builders<TDocument>.Filter;
            var filter = filterDefinition;

            var countFacet = AggregateFacet.Create("count",
                PipelineDefinition<TDocument, AggregateCountResult>.Create(new[]
                {
                PipelineStageDefinitionBuilder.Count<TDocument>()
                }));

            AggregateFacet<TDocument, TDocument> dataFacet;

            if (projection == null)
            {
                dataFacet = AggregateFacet.Create("data",
                PipelineDefinition<TDocument, TDocument>.Create(new[]
                {
                PipelineStageDefinitionBuilder.Sort(sortDefinition),
                PipelineStageDefinitionBuilder.Skip<TDocument>(((int)pagination.PageNumber - 1) * (int)pagination.PageSize),
                PipelineStageDefinitionBuilder.Limit<TDocument>((int)pagination.PageSize)
                }));
            }
            else
            {
                dataFacet = AggregateFacet.Create("data",
                PipelineDefinition<TDocument, TDocument>.Create(new[]
                {
                PipelineStageDefinitionBuilder.Sort(sortDefinition),
                PipelineStageDefinitionBuilder.Skip<TDocument>(((int)pagination.PageNumber - 1) * (int)pagination.PageSize),
                PipelineStageDefinitionBuilder.Limit<TDocument>((int)pagination.PageSize),
                PipelineStageDefinitionBuilder.Project<TDocument,TDocument>(projection),
                }));
            }

            var aggregation = await collection.Aggregate()
                .Match(filter)
                .Facet(countFacet, dataFacet)
                .ToListAsync();

            var count = aggregation.First()
                .Facets.First(x => x.Name == "count")
                .Output<AggregateCountResult>()
                ?.FirstOrDefault()
                ?.Count;

            var totalRecords = count == null ? 0 : (int)count;

            var data = aggregation.First()
                .Facets.First(x => x.Name == "data")
                .Output<TDocument>();

            return (totalRecords, data);
        }

        /// <summary>
        /// Insert a document.
        /// </summary>
        /// <param name="entity">documnet object</param>
        /// <param name="options">Insertion options</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Boolean representation of success.</returns>
        public async Task<bool> InsertAsync(TDocument entity, InsertOneOptions options = null, CancellationToken ct = default)
        {
            if (entity.createdAt != DateTime.MinValue)
            {
                entity.createdAt = entity.createdAt;
                entity.modifiedAt = entity.modifiedAt;
            }
            else
            {
                entity.createdAt = DateTime.UtcNow;
                entity.modifiedAt = DateTime.UtcNow;
            }

            await collection.InsertOneAsync(entity, options, ct);
            return true;
        }

        /// <summary>
        /// This method is used to Insert List
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="options"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<bool> InsertListAsync(List<TDocument> entity, InsertManyOptions options, CancellationToken ct = default)
        {
            entity.ForEach(
                p =>
                {
                    p.createdAt = DateTime.UtcNow;
                    p.modifiedAt = DateTime.UtcNow;
                });

            await collection.InsertManyAsync(entity, options, ct);
            return true;
        }

        /// <summary>
        /// Update a document by Id.
        /// </summary>
        /// <param name="document">document object</param>
        /// <param name="id">Id of document which needs to be updated</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Boolean representation of success.</returns>
        public async Task<bool> UpdateByIdAsync(TDocument document, string id, CancellationToken ct = default)
        {
            document.modifiedAt = DateTime.UtcNow;

            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
            return await UpdateAsync(document, filter, ct);
        }

        /// <summary>
        /// UpdateManyAsync a documents
        /// </summary>
        /// <param name="document">document object</param>
        /// <param name="id">Id of document which needs to be updated</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Boolean representation of success.</returns>
        public async Task<bool> UpdateManyListAsync(List<TDocument> document, CancellationToken ct = default)
        {
            document.ForEach(
                p =>
                {
                    p.modifiedAt = DateTime.UtcNow;
                });

            // var updates = new List<WriteModel<TDocument>>();

            foreach (var doc in document)
            {
                await UpdateByIdAsync(doc, doc.Id);
                // updates.Add(new ReplaceOneModel<TDocument>(doc.Id, doc));
                //var upsertOne = new ReplaceOneModel<TDocument>(doc.Id, doc)
                //{ IsUpsert = true };
                //updates.Add(upsertOne);
            }
            //try
            //{
            //    await collection.BulkWriteAsync(updates);

            //}
            //catch (Exception ex)
            //{

            //}
            return true;
        }

        /// <summary>
        /// Update document based on filter criteria.
        /// </summary>
        /// <param name="entity">docuemnt object</param>
        /// <param name="filter">filter criteria.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Boolean representation of success.</returns>
        public async Task<bool> UpdateAsync(TDocument entity, FilterDefinition<TDocument> filter, CancellationToken ct = default)
        {
            entity.modifiedAt = DateTime.UtcNow;
            return await collection.FindOneAndReplaceAsync(filter, entity, new FindOneAndReplaceOptions<TDocument>() { MaxTime = new TimeSpan(0, 5, 0) }, ct) != null;
            // return true;
        }

        public bool ReplaceOneById(TDocument document, string id, CancellationToken ct = default)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
            return ReplaceOne(document, filter, ct);
        }

        public bool ReplaceOne(TDocument entity, FilterDefinition<TDocument> filter, CancellationToken ct = default)
        {
            entity.modifiedAt = DateTime.UtcNow;
            return collection.ReplaceOne(filter, entity, new ReplaceOptions() { }, cancellationToken: ct).IsAcknowledged;
        }

        /// <summary>
        /// This method is used to update record by using filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="update"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<UpdateResult> UpdateAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var data = await collection.UpdateManyAsync(filter, update, options, cancellationToken);
            return data;
        }

        /// <summary>
        /// Delete document by Id.
        /// </summary>
        /// <param name="id">Id of documnet which needs to be deleted.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Boolean representation of success.</returns>
        public async Task<bool> DeleteByIdAsync(string id, CancellationToken ct = default)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
            return await DeleteAsync(filter, ct);
        }

        /// <summary>
        /// Delete document based on filter criteria.
        /// </summary>
        /// <param name="filter">filter criteria</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Boolean representation of success.</returns>
        public async Task<bool> DeleteAsync(FilterDefinition<TDocument> filter, CancellationToken ct = default)
        {
            await collection.DeleteManyAsync(filter, null, ct);
            return true;
        }

        /// <summary>
        /// Gets count of the documents on filter criteria
        /// </summary>
        /// <param name="filter">filter criteria</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>The count of documents</returns>
        public async Task<long> GetCountAsync(FilterDefinition<TDocument> filter, CancellationToken ct = default(CancellationToken))
        {
            return await collection.CountDocumentsAsync(filter, null, ct);
        }

        /// <summary>
        /// Checks if any document exists on filter criteria
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="ct"></param>
        /// <returns>
        /// Boolean
        /// </returns>
        public async Task<bool> AnyAsync(FilterDefinition<TDocument> filter, CancellationToken ct = default(CancellationToken))
        {
            return await collection.Find(filter, null).AnyAsync();
        }
    }

}
