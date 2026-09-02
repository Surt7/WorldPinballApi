using WorldPinball.Domain.Enums;

namespace WorldPinball.Domain.Entities;

public class Machine
{
    private static class ErrorMessages
    {
        public const string EmptyIpId = "The IP license identifier cannot be empty.";
        public const string EmptySerialNumber = "The WorldPinball serial number cannot be empty.";
        public const string EmptyDisplayName = "The display name cannot be empty.";
        public const string CannotReturnToPendingEnrollment =
            "A machine cannot return to pending enrollment.";
        public const string CannotCompleteEnrollment =
            "Only a machine pending enrollment can complete enrollment.";
        public const string RevokedMachineCannotBeReactivated =
            "A revoked machine cannot be reactivated.";
        public const string PendingEnrollmentRequiresCompletion =
            "A machine pending enrollment must complete enrollment before activation.";
    }

    private Machine() { } // Utilisé par EF Core

    public Machine(
        Guid ipId,
        string serialNumber,
        string displayName,
        DateTime registeredAtUtc)
    {

        if (ipId == Guid.Empty)
            throw new ArgumentException(ErrorMessages.EmptyIpId, nameof(ipId));

        if (string.IsNullOrWhiteSpace(serialNumber))
            throw new ArgumentException(ErrorMessages.EmptySerialNumber, nameof(serialNumber));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException(ErrorMessages.EmptyDisplayName, nameof(displayName));
        
        DisplayName = displayName.Trim();
        WorldPinballSerialNumber = serialNumber.Trim();
        MachineId = Guid.NewGuid();
        IpId = ipId;
        Status = MachineStatus.PendingEnrollment;
        RegisteredAtUtc = registeredAtUtc;
    }

    public Guid MachineId { get; private set; }

    public Guid IpId { get; private set; }
    public IpLicense IpLicense { get; set; } = null!;


    //Identifiant interne WorldPinball
    public string WorldPinballSerialNumber { get; private set; } = null!;

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


    public string DisplayName { get; private set; }

    public MachineStatus Status { get; private set; } = MachineStatus.PendingEnrollment;

    public DateTime RegisteredAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public ICollection<MachineDevice> MachineDevices { get; private set; } = [];
    public ICollection<EnrollmentToken> EnrollmentTokens { get; private set; } = [];



    public void SetStatus(MachineStatus status)
    {
        if (status == MachineStatus.Active)
        {
            Activate();
            return;
        }

        if (status == MachineStatus.PendingEnrollment && Status != MachineStatus.PendingEnrollment)
        {
            throw new InvalidOperationException(ErrorMessages.CannotReturnToPendingEnrollment);
        }

        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void CompleteEnrollment()
    {
        if (Status != MachineStatus.PendingEnrollment)
        {
            throw new InvalidOperationException(ErrorMessages.CannotCompleteEnrollment);
        }

        Status = MachineStatus.Active;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status == MachineStatus.Revoked)
        {
            throw new InvalidOperationException(ErrorMessages.RevokedMachineCannotBeReactivated);
        }

        if (Status == MachineStatus.PendingEnrollment)
        {
            throw new InvalidOperationException(ErrorMessages.PendingEnrollmentRequiresCompletion);
        }

        Status = MachineStatus.Active;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
