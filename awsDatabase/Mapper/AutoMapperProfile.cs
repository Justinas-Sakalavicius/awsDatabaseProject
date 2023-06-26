using AutoMapper;
using awsDatabase.DTO;
using awsDatabase.Models;

namespace awsDatabase.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() 
        {
            CreateMap<Image, ImageVM>()
                .ForMember(dest => dest.Bitmap, opt => opt.Ignore());

            CreateMap<ImageUploadVM, Image>();
            CreateMap<ImageVM, Image>();
        }
    }
}
