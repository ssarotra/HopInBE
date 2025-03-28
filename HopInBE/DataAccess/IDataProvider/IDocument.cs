using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace HopInBE.DataAccess.IDataProvider
{
    public interface IDocument
    {
        /// <summary>
        /// ID will be used by all the models that are inherited from Document 
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        string Id { get; set; }

        /// <summary>
        /// String representation of Id.
        /// </summary>
        string IdString { get; }

        /// <summary>
        /// gets and sets createdBy
        /// </summary>
        public string createdBy { get; set; }

        /// <summary>
        /// gets and sets createdAt
        /// </summary>
        public DateTime createdAt { get; set; }

        /// <summary>
        /// gets and sets modifiedBy
        /// </summary>
        public string modifiedBy { get; set; }

        /// <summary>
        /// gets and sets modifiedAt
        /// </summary>
        public DateTime modifiedAt { get; set; }
    }

}
