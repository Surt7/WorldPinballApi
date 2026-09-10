using WorldPinball.Domain.Entities;

namespace WorldPinball.Application.Interfaces.Repositories;

public interface IDeviceEnrollmentRepository
{
   Task<EnrollmentToken?> GetTokenByHashAsync(
       string tokenHash,
       CancellationToken cancellationToken = default);

   Task<bool> DeviceSerialExistsAsync(
       string serialPhysical,
       CancellationToken cancellationToken = default);

   Task SaveEnrollmentAsync(
       Device device,
       MachineDevice machineDevice,
       EnrollmentToken  enrollmentToken,
       CancellationToken cancellationToken = default);
}