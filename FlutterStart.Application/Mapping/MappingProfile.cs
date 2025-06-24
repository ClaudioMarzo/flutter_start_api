using AutoMapper;
using FlutterStart.Application.DTO;
using FlutterStart.Domain.Entities;

namespace FlutterStart.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Book, BookDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Isbn, opt => opt.MapFrom(src => src.ISBN))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Summary, opt => opt.MapFrom(src => src.Summary))
            .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => src.Genre))
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author))
            .ForMember(dest => dest.PublicationYear, opt => opt.MapFrom(src => src.PublicationYear))
            .ForMember(dest => dest.PageCount, opt => opt.MapFrom(src => src.PageCount))
            .ForMember(dest => dest.Publisher, opt => opt.MapFrom(src => src.Publisher))
            .ForMember(dest => dest.Edition, opt => opt.MapFrom(src => src.Edition))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
            .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language))
            .ForMember(dest => dest.Format, opt => opt.MapFrom(src => src.Format))
            .ForMember(dest => dest.Dimensions, opt => opt.MapFrom(src => src.Dimensions))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location));
    }
}