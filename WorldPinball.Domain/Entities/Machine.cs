using WorldPinball.Domain.Enums;

namespace WorldPinball.Domain.Entities;

public class Machine
{
    public Guid MachineId { get; set; }


    public Guid IpId { get; set; }
    public IpLicense IpLicense { get; set; } = null!;


    //Identifiant interne WorldPinball
    public string WorldPinballSerialNumber { get; set; } = null!;

    public string? Manufacturer { get; set; }
    public string? ManufacturerModelCode { get; set; }
    public string? ManufacturerModelName { get; set; }
    public string? ManufacturerModelYear { get; set; }
    public string? ManufacturerSerialNumber { get; set; }
    public string? ManufacturerNotes { get; set; }
    public string? MachineNotes { get; set; }
    public string? Location { get; set; }

    // Infos techniques étiquettes
    public int? Voltage { get; set; }
    public int? FrequencyHz { get; set; }
    public decimal? Amperage { get; set; }
    public DateOnly? ManufacturedDate { get; set; }


    public string? Name { get; set; }

    public MachineStatus Status { get; set; } = MachineStatus.Active; //Statut possible: Active , Inactive, Maintenance, Retired

    public DateTime RegisteredAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<MachineDevice> MachineDevices { get; set; } = [];
    public ICollection<EnrollmentToken> EnrollmentTokens { get; private set; } = [];



    public void SetStatus(MachineStatus status)
    {
        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status == MachineStatus.Revoked)
        {
            throw new InvalidOperationException("A retired machine cannot be reactivated.");
        }

    }
}