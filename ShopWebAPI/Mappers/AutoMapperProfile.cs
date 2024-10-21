using AutoMapper;
using Microsoft.Identity.Client;
using ShopWebAPI.DTOs.User;
using ShopWebAPI.Models;
using System.Data;

namespace ShopWebAPI.Mappers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ApplicationUser, UserDto>().ReverseMap();
        }
    }
}
