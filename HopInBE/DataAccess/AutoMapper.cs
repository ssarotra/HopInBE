using AutoMapper;

namespace HopInBE.DataAccess
{
    public static class Mapping
    {
        private static readonly Lazy<IMapper> Lazy = new Lazy<IMapper>(() =>
        {
            // Configuration
            var config = new MapperConfiguration(cfg =>
            {
                // This line ensures that internal properties are also mapped over.
                cfg.ShouldMapProperty = p => p.GetMethod.IsPublic || p.GetMethod.IsAssembly;
                cfg.AddProfile<MappingProfile>();
            });

            // Call create mapper
            var mapper = config.CreateMapper();
            return mapper;
        });

        public static IMapper Mapper => Lazy.Value;
    }

    /// <summary>
    /// Main mapper class. 
    /// </summary>
    public class MappingProfile : Profile
    {
        // Map classes in constructor
        public MappingProfile()
        {
            // CreateMap<Source, Destination>();

            // DatabaseModels to ResponseModels
           
        }
    }
}
