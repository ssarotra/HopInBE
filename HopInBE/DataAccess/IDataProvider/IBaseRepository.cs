using MongoDB.Driver;

namespace HopInBE.DataAccess.IDataProvider
{
    public interface IBaseRepository<TDocument> where TDocument : IDocument
    {
        IMongoCollection<TDocument> collection { get; set; }
        /// <summary>
        /// Creates a queryable source of documents.
        /// </summary>
        /// <returns>A queryable source of documents.</returns>
        IQueryable<TDocument> AsQueryable();

        /// <summary>
        /// Get Single document by Id.
        /// </summary>
        /// <param name="id">Id of document.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>A Single document</returns>
        Task<TDocument> GetByIdAsync(string id, CancellationToken ct = default(CancellationToken));

        /// <summary>
        /// Get List of documents.
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns> List of documents.</returns>
        Task<List<TDocument>> GetAllAsync(CancellationToken ct = default(CancellationToken));

        /// <summary>
        /// Get single document using filter criteria.
        /// </summary>
        /// <param name="filter">Filter criteria.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>A Single document.</returns>
        Task<TDocument> GetAsync(FilterDefinition<TDocument> filter, CancellationToken ct = default(CancellationToken));

        /// <summary>
        /// Get Mulitple document using filter, sort and find options criteria.
        /// </summary>
        /// <param name="filter">Filter criteria.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>A Single document.</returns>
        Task<List<TDocument>> GetKeySetPaginatedAsync(Pagination pagination, FilterDefinition<TDocument> customFilter, ProjectionDefinition<TDocument> projection, CancellationToken ct = default);

        /// <summary>
        /// Get list of document by filter Id
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<List<TDocument>> GetFilterListAsync(FilterDefinition<TDocument> filter, CancellationToken ct = default);

        /// <summary>
        /// Get Mulitple document using filter, sort and find options criteria.
        /// </summary>
        /// <param name="filter">Filter criteria.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>A Single document.</returns>
        Task<(int totalPages, IReadOnlyList<TDocument> data)> GetPaginatedAsync(FilterDefinition<TDocument> filterDefinition, SortDefinition<TDocument> sortDefinition, int page, int pageSize, ProjectionDefinition<TDocument> projection, CancellationToken ct = default);

        /// <summary>
        /// Get Mulitple document using filter, sort and find options criteria.
        /// </summary>
        /// <param name="filter">Filter criteria.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>A Single document.</returns>
        Task<(int totalRecords, IReadOnlyList<TDocument> data)> GetPaginateAsync(FilterDefinition<TDocument> filterDefinition, SortDefinition<TDocument> sortDefinition, Pagination pagination, ProjectionDefinition<TDocument> projection = null, CancellationToken ct = default);

        /// <summary>
        /// Insert a document.
        /// </summary>
        /// <param name="entity">documnet object</param>
        /// <param name="options">Insertion options</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Boolean representation of success.</returns>
        Task<bool> InsertAsync(TDocument entity, InsertOneOptions options = null, CancellationToken ct = default(CancellationToken));


        /// <summary>
        /// Insert many a document.
        /// </summary>
        /// <param name="entity">documnet object</param>
        /// <param name="options">Insertion options</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Boolean representation of success.</returns>
        Task<bool> InsertListAsync(List<TDocument> entity, InsertManyOptions options, CancellationToken ct = default(CancellationToken));

        /// <summary>
        /// Update a document by Id.
        /// </summary>
        /// <param name="document">document object</param>
        /// <param name="id">Id of document which needs to be updated</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Boolean representation of success.</returns>
        Task<bool> UpdateByIdAsync(TDocument entity, string id, CancellationToken ct = default(CancellationToken));

        /// <summary>
        /// Update document based on filter criteria.
        /// </summary>
        /// <param name="entity">docuemnt object</param>
        /// <param name="filter">filter criteria.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Boolean representation of success.</returns>
        Task<bool> UpdateAsync(TDocument entity, FilterDefinition<TDocument> filter, CancellationToken ct = default(CancellationToken));

        /// <summary>
        /// Update document based on filter criteria.
        /// </summary>
        /// <param name="entity">docuemnt object</param>
        /// <param name="filter">filter criteria.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Boolean representation of success.</returns>
        Task<UpdateResult> UpdateAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// this method is used for bulk update
        /// </summary>
        /// <param name="document"></param>
        /// <param name="filter"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<bool> UpdateManyListAsync(List<TDocument> document, CancellationToken ct = default);

        /// <summary>
        /// Replace a document by id. Method uses ReplaceOne sub method. 
        /// </summary>
        /// <param name="document"></param>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        bool ReplaceOneById(TDocument document, string id, CancellationToken ct = default);

        /// <summary>
        /// This method uses ReplaceOneAsync method.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="filter"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        bool ReplaceOne(TDocument entity, FilterDefinition<TDocument> filter, CancellationToken ct = default);

        /// <summary>
        /// Delete document by Id.
        /// </summary>
        /// <param name="id">Id of documnet which needs to be deleted.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Boolean representation of success.</returns>
        Task<bool> DeleteByIdAsync(string id, CancellationToken ct = default(CancellationToken));

        /// <summary>
        /// Delete document based on filter criteria.
        /// </summary>
        /// <param name="filter">filter criteria</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Boolean representation of success.</returns>
        Task<bool> DeleteAsync(FilterDefinition<TDocument> filter, CancellationToken ct = default(CancellationToken));

        /// <summary>
        /// Gets count of the documents on filter criteria
        /// </summary>
        /// <param name="filter">filter criteria</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Boolean representation of success.</returns>
        Task<long> GetCountAsync(FilterDefinition<TDocument> filter, CancellationToken ct = default(CancellationToken));

        Task<bool> AnyAsync(FilterDefinition<TDocument> filter, CancellationToken ct = default(CancellationToken));
    }

}
