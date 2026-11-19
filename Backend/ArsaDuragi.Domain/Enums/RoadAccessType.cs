namespace ArsaDuragi.Domain.Enums;

public enum RoadAccessType
{
    RoadFrontage = 1,         // Yola cepheli
    AsphaltFrontage = 2,      // Asfalt yola cepheli
    DirtRoadFrontage = 3,     // Toprak yola cepheli
    VillageRoadFrontage = 4,  // Köy yoluna cepheli
    MainRoadNearby = 5,       // Ana yola {n} m   (needs RoadDistanceMeters)
    AsphaltRoadNearby = 6     // Asfalt yola {n} m (needs RoadDistanceMeters)
}
