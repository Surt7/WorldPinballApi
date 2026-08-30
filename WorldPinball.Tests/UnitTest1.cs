using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests
{
    public class UnitTest1
    {
        [Theory]
        [InlineData(MachineStatus.Active)]
        [InlineData(MachineStatus.Inactive)]
        [InlineData(MachineStatus.Revoked)]
        [InlineData(MachineStatus.Maintenance)]
        public void SetStatus_ShouldUpdateStatusAndUpdatedAt(MachineStatus status)
        {
            var machine = new Machine();

            machine.SetStatus(status);

            Assert.Equal(status, machine.Status);
            Assert.NotNull(machine.UpdatedAtUtc);
        }
    }
}
