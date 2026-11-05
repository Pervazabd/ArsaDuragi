using ArsaDuragi.Domain.Enums;

namespace ArsaDuragi.Application.Common;

/// <summary>
/// Display texts. The existing frontend shows Turkish property texts ("Konut imarlı", "Asfalt yola cepheli"...),
/// so the API returns exactly these strings next to the machine-readable enum codes.
/// Changing language later means changing only this class.
/// </summary>
public static class PropertyLabels
{
    public const string DefaultTransaction = "Satılık";

    public static string Type(PropertyType type) => type switch
    {
        PropertyType.Land => "Arsa",
        PropertyType.Field => "Tarla",
        PropertyType.VineyardOrchard => "Bağ / Bahçe",
        _ => type.ToString()
    };

    public static string Zoning(PropertyZoning zoning) => zoning switch
    {
        PropertyZoning.Residential => "Konut imarlı",
        PropertyZoning.Agricultural => "Tarım arazisi",
        PropertyZoning.Investment => "Yatırımlık arsa",
        _ => zoning.ToString()
    };

    public static string Terrain(TerrainType terrain) => terrain switch
    {
        TerrainType.Flat => "Düz arazi",
        TerrainType.SlightlySloped => "Hafif eğimli arazi",
        TerrainType.Sloped => "Eğimli arazi",
        TerrainType.FlatFertile => "Düz ve verimli toprak",
        _ => terrain.ToString()
    };

    public static string RoadAccess(RoadAccessType access, int? distanceMeters) => access switch
    {
        RoadAccessType.RoadFrontage => "Yola cepheli",
        RoadAccessType.AsphaltFrontage => "Asfalt yola cepheli",
        RoadAccessType.DirtRoadFrontage => "Toprak yola cepheli",
        RoadAccessType.VillageRoadFrontage => "Köy yoluna cepheli",
        RoadAccessType.MainRoadNearby => $"Ana yola {distanceMeters} m",
        RoadAccessType.AsphaltRoadNearby => $"Asfalt yola {distanceMeters} m",
        _ => access.ToString()
    };

    public static bool RequiresDistance(RoadAccessType access)
        => access is RoadAccessType.MainRoadNearby or RoadAccessType.AsphaltRoadNearby;

    public static string Utility(UtilityType utility) => utility switch
    {
        UtilityType.Electricity => "Elektrik",
        UtilityType.Water => "Su",
        UtilityType.NaturalGas => "Doğalgaz",
        UtilityType.IrrigationWater => "Sulama suyu",
        _ => utility.ToString()
    };

    private static IEnumerable<UtilityType> Split(UtilityType utilities)
        => Enum.GetValues<UtilityType>().Where(u => u != UtilityType.None && utilities.HasFlag(u));

    public static List<string> UtilityLabels(UtilityType utilities) => Split(utilities).Select(Utility).ToList();

    public static List<string> UtilityCodes(UtilityType utilities) => Split(utilities).Select(u => u.ToString()).ToList();

    public static UtilityType ToFlags(IEnumerable<UtilityType> utilities)
    {
        var result = UtilityType.None;
        foreach (var u in utilities) result |= u;
        return result;
    }
}
