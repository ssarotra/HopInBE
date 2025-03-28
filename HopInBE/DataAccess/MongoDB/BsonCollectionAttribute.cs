namespace HopInBE.DataAccess.MongoDB
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class BsonCollectionAttribute : Attribute
    {
        /// <summary>
        ///  Collection (Document) name
        /// </summary>
        public string CollectionName { get; }

        /// <summary>
        /// BsonCollectionAttribute's constructor.
        /// </summary>
        /// <param name="collectionName">Collection Name</param>
        public BsonCollectionAttribute(string collectionName)
        {
            CollectionName = collectionName;
        }
    }
}
