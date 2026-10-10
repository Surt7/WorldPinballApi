namespace WorldPinball.Logistics.Domain.Enums;

public enum ShipmentStatus
{
    PendingAllocation = 1,
    MachineAllocated = 2,
    InTransit = 3,
    Delivered = 4,
    Cancelled = 5
}