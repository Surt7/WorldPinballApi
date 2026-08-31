namespace WorldPinball.Domain.Errors;

public static class MachineErrorCodes
{
    public const string EmptyIpId = "Machine.EmptyIpId";
    public const string EmptySerialNumber = "Machine.EmptySerialNumber";
    public const string CannotReturnToPendingEnrollment =
        "Machine.CannotReturnToPendingEnrollment";
    public const string CannotCompleteEnrollment = "Machine.CannotCompleteEnrollment";
    public const string RevokedMachineCannotBeReactivated =
        "Machine.RevokedMachineCannotBeReactivated";
    public const string PendingEnrollmentRequiresCompletion =
        "Machine.PendingEnrollmentRequiresCompletion";
}
