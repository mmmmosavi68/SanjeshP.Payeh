
using AutoMapper;

namespace SanjeshP.Payeh.WebFramework.CustomMapping
{
    public sealed class CustomMappingProfile : Profile
    {
        public CustomMappingProfile(
            IEnumerable<IHaveCustomMapping> haveCustomMappings)
        {
            foreach (var item in haveCustomMappings)
            {
                item.CreateMappings(this);
            }
        }
    }
}
