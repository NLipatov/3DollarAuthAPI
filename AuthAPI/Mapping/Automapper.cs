using AuthAPI.DTOs.User;
using AuthAPI.Models;
using AutoMapper;

namespace AuthAPI.Mapping
{
    public static class Automapper
    {
        private static IMapper CreateUserMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<User, UserDTO>()
            .ForMember(src => src.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(src => src.Password, opt => opt.Ignore())
            .ForMember(src => src.Claims, opt => opt.Ignore())
            );

            return config.CreateMapper();
        }

        public static UserDTO ToDTO(this User user)
        {
            var mapper = CreateUserMapper();

            return mapper.Map<UserDTO>(user);
        }
    }
}
