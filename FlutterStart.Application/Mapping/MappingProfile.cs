using AutoMapper;
using FlutterStart.Application.DTO;
using FlutterStart.Domain.Entities;
using FlutterStart.Application.DTO.Book;
using FlutterStart.Application.DTO.Movie;

namespace FlutterStart.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Loan, LoanResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.BookId))
            .ForMember(dest => dest.BookISBN, opt => opt.MapFrom(src => src.Book != null && src.Book.ISBN != null ? src.Book.ISBN : ""))
            .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book != null ? src.Book.Title : ""))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Nome : ""))
            .ForMember(dest => dest.LoanDate, opt => opt.MapFrom(src => src.LoanDate))
            .ForMember(dest => dest.ReturnDate, opt => opt.MapFrom(src => src.DueDate));
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
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
            .ForMember(dest => dest.IsRented, opt => opt.MapFrom(src => src.IsRented));
            
        CreateMap<Movie, MovieResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.IMDB, opt => opt.MapFrom(src => src.IMDB))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
            .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language))
            .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => src.DurationMinutes))
            .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => src.Genre))
            .ForMember(dest => dest.Director, opt => opt.MapFrom(src => src.Director))
            .ForMember(dest => dest.Cast, opt => opt.MapFrom(src => src.Cast))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.PosterUrl, opt => opt.MapFrom(src => src.PosterUrl))
            .ForMember(dest => dest.TrailerUrl, opt => opt.MapFrom(src => src.TrailerUrl))
            .ForMember(dest => dest.LoanDate, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.ReturnDate, opt => opt.MapFrom(src => src.UpdatedAt));

    }
}