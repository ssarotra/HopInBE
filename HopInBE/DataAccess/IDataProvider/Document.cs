using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace HopInBE.DataAccess.IDataProvider
{
    public abstract class Document : IDocument
    {
        /// ID will be used by all the models that are inherited from Document
        ///public Document() { Id = new MongoDB.Bson.ObjectId().ToString(); }

        /// <summary>
        /// MongoDB Document unique Id.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        /// <summary>
        /// gets and sets Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// String representation of Id.
        /// </summary>
        public string IdString { get { return Id == null ? "" : Id.ToString(); } }

        /// <summary>
        /// gets and sets createdBy
        /// </summary>
        public string createdBy { get; set; } = string.Empty;

        /// <summary>
        /// gets and sets createdAt
        /// </summary>
        public DateTime createdAt { get; set; }

        /// <summary>
        /// gets and sets modifiedBy
        /// </summary>
        public string modifiedBy { get; set; } = string.Empty;

        /// <summary>
        /// gets and sets modifiedAt 
        /// </summary>
        public DateTime modifiedAt { get; set; }
    }

}
