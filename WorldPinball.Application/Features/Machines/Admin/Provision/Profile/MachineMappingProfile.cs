using AutoMapper;
using WorldPinball.Application.Features.Machines.Admin.Provision.Dtos;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Application.Features.Machines.Admin.Provision.Profile;

public sealed class MachineMappingProfile : AutoMapper.Profile
{
    public MachineMappingProfile()
    {
        CreateMap<Machine, MachineRowDto>()
            .ForCtorParam(nameof(MachineRowDto.MachineId), opt => opt.MapFrom(src => src.MachineId))
            .ForCtorParam(nameof(MachineRowDto.MachineName), opt => opt.MapFrom(src => src.ManufacturerModelName ?? src.DisplayName))
            .ForCtorParam(nameof(MachineRowDto.DisplayName), opt => opt.MapFrom(src => src.DisplayName))
            .ForCtorParam(nameof(MachineRowDto.MachineStatus), opt => opt.MapFrom(src => src.Status))
            .ForCtorParam(nameof(MachineRowDto.MachineNotes), opt => opt.MapFrom(src => src.MachineNotes))
            .ForCtorParam(nameof(MachineRowDto.RegistredAtUtc), opt => opt.MapFrom(src => src.RegisteredAtUtc))
            .ForCtorParam(nameof(MachineRowDto.LastUpdated), opt => opt.MapFrom(src => src.UpdatedAtUtc))
            .ForCtorParam(nameof(MachineRowDto.SerialNumber), opt => opt.MapFrom(src => src.WorldPinballSerialNumber));
    }
}
