namespace HopInBE.DataAccess.IDataProvider
{
    public class Pagination
    {
        /// <summary>
        /// gets and sets SortColumn
        /// </summary>
        public string? SortColumn { get; set; }

        /// <summary>
        /// gets and sets SortDirection
        /// </summary>
        public string? SortDirection { get; set; }

        /// <summary>
        /// gets and sets PageSize
        /// </summary>
        public int? PageSize { get; set; } = 10;

        /// <summary>
        /// gets and sets PageNumber
        /// </summary>
        public int? PageNumber { get; set; } = 1;

        /// <summary>
        /// gets and sets Action
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// gets and sets Id
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// gets and sets SortColumnValue
        /// </summary>
        public object? SortColumnValue { get; set; }

    }

}
