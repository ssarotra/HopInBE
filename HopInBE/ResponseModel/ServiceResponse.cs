using System.Net;

namespace HopInBE.ResponseModel
{
    public class ServiceResponse<T> where T : class
    {

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public T? Data { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public HttpStatusCode Status { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string? Message { get; set; }

        /// <summary>
        /// Pagination details
        /// </summary>
        public PaginationResponse? pagination { get; set; }

        /// <summary>
        /// Localization key 
        /// </summary>
        public string? LocalizeKey { get; set; }
    }

    /// <summary>
    /// Pagination related information for response.
    /// </summary>
    public class PaginationResponse
    {
        /// <summary>
        /// Default pagination value
        /// </summary>
        private int _pageNumber = 1;

        /// <summary>
        /// Default Page size
        /// </summary>
        private int _pageSize = 10;

        /// <summary>
        /// Current page number
        /// </summary>
        public int pageNumber { get { return _pageNumber; } set { _pageNumber = value <= 0 ? 1 : value; } }

        /// <summary>
        ///  Page size
        /// </summary>
        public int pageSize { get { return _pageSize; } set { _pageSize = value <= 0 ? 10 : value; } }

        /// <summary>
        /// Total number of records
        /// </summary>
        public int totalRecords { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int totalPages { get; set; }
    }
}
