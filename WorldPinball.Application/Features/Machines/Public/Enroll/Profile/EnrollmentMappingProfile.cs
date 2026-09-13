using WorldPinball.Application.Features.Machines.Public.Enroll.Dto;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Application.Features.Machines.Public.Enroll.Profile;

public class EnrollmentMappingProfile : AutoMapper.Profile
{
    public EnrollmentMappingProfile()
    {
        CreateMap<EnrollmentToken, EnrollmentTokenDto>();
    }
}
