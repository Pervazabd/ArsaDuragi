namespace ArsaDuragi.Domain.Enums;

/// <summary>Stored as a single integer column; combine values with | (bit flags).</summary>
[Flags]
public enum UtilityType
{
    None = 0,
    Electricity = 1,      // Elektrik
    Water = 2,            // Su
    NaturalGas = 4,       // Doğalgaz
    IrrigationWater = 8   // Sulama suyu
}
