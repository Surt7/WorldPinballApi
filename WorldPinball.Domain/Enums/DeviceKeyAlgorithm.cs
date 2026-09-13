namespace WorldPinball.Domain.Enums
{
    public enum DeviceKeyAlgorithm
    {
        Ed25519 = 1,
        EcdsaP256 = 2
        // Rsa2048 if needed, but prefer elliptic curves if possible
    }
}
