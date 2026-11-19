const referenceLandImage =
    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ1PjYayqh6d0JVx4GTAPtP7sKohx8EzgaOtrmf5bNhc1jlbKzARLpxhH2J&s=10";

export const properties = [
    {
        id: "merdakan-konut-arsasi",
        title: "Merdakan'da konut imarlı arsa",
        district: "Merdakan",
        address: "Merdakan, Bakü",
        type: "Arsa",
        zoning: "Konut imarlı",
        area: 600,
        price: 180000,
        featured: true,
        listedAt: "2026-10-24",
        terrain: "Düz arazi",
        roadAccess: "Asfalt yola cepheli",
        utilities: ["Elektrik", "Su", "Doğalgaz"],
        description:
            "Merdakan'da yerleşim alanına yakın, müstakil konut yapımına uygun arsa. Düz yapısı ve yola cepheli konumuyla kullanıma hazır durumdadır.",
        image: referenceLandImage,
    },
    {
        id: "bilgah-yatirim-arsasi",
        title: "Bilgah'ta yatırıma uygun arsa",
        district: "Bilgah",
        address: "Bilgah, Bakü",
        type: "Arsa",
        zoning: "Yatırımlık arsa",
        area: 850,
        price: 212500,
        featured: true,
        listedAt: "2026-10-21",
        terrain: "Hafif eğimli arazi",
        roadAccess: "Ana yola 300 m",
        utilities: ["Elektrik", "Su"],
        description:
            "Bilgah bölgesinde gelişen yerleşim alanlarının yakınında bulunan geniş yatırım arsası. Arazi sınırları belirgindir ve bölge yoluna kolay erişim sunar.",
        image:
            "https://images.unsplash.com/photo-1500382017468-9049fed747ef?auto=format&fit=crop&w=1200&q=85",
    },
    {
        id: "sabuncu-tarim-arazisi",
        title: "Sabuncu'da verimli tarım arazisi",
        district: "Sabuncu",
        address: "Sabuncu, Bakü",
        type: "Arsa",
        zoning: "Tarım arazisi",
        area: 2400,
        price: 96000,
        featured: true,
        listedAt: "2026-10-19",
        terrain: "Düz ve verimli toprak",
        roadAccess: "Toprak yola cepheli",
        utilities: ["Sulama suyu"],
        description:
            "Sabuncu'da tarımsal kullanım için uygun, düz ve geniş arazi. Sulama suyu bağlantısı ve ulaşım yolu mevcuttur.",
        image:
            "https://images.unsplash.com/photo-1472396961693-142e6e269027?auto=format&fit=crop&w=1200&q=85",
    },
    {
        id: "hovsan-konut-arsasi",
        title: "Hövsan'da yola cepheli arsa",
        district: "Hövsan",
        address: "Hövsan, Bakü",
        type: "Arsa",
        zoning: "Konut imarlı",
        area: 500,
        price: 115000,
        featured: false,
        listedAt: "2026-10-16",
        terrain: "Düz arazi",
        roadAccess: "Asfalt yola cepheli",
        utilities: ["Elektrik", "Su"],
        description:
            "Hövsan yerleşim alanında konut yapımına uygun, düz ve yola cepheli arsa. Elektrik ve su hatları parselin yakınından geçmektedir.",
        image:
            "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=1200&q=85",
    },
    {
        id: "mashtaga-yatirim-arazisi",
        title: "Maştağa'da geniş yatırım arazisi",
        district: "Maştağa",
        address: "Maştağa, Bakü",
        type: "Arsa",
        zoning: "Yatırımlık arsa",
        area: 1200,
        price: 168000,
        featured: false,
        listedAt: "2026-10-12",
        terrain: "Düz arazi",
        roadAccess: "Ana yola 500 m",
        utilities: ["Elektrik", "Su", "Doğalgaz"],
        description:
            "Maştağa'da gelişime açık bölgede bulunan geniş yatırım arazisi. Düz topografyası farklı arazi projeleri için elverişlidir.",
        image:
            "https://images.unsplash.com/photo-1500382017468-9049fed747ef?auto=format&fit=crop&w=1200&q=85",
    },
    {
        id: "nardaran-tarim-arazisi",
        title: "Nardaran'da geniş tarım arazisi",
        district: "Nardaran",
        address: "Nardaran, Bakü",
        type: "Arsa",
        zoning: "Tarım arazisi",
        area: 3200,
        price: 128000,
        featured: false,
        listedAt: "2026-10-09",
        terrain: "Düz arazi",
        roadAccess: "Köy yoluna cepheli",
        utilities: ["Sulama suyu", "Elektrik"],
        description:
            "Nardaran çevresinde tarım ve açık alan kullanımı için uygun geniş parsel. Araziye köy yolu üzerinden ulaşılabilir.",
        image:
            "https://images.unsplash.com/photo-1472396961693-142e6e269027?auto=format&fit=crop&w=1200&q=85",
    },
    {
        id: "zabrat-konut-arsasi",
        title: "Zabrat'ta konut yapımına uygun arsa",
        district: "Zabrat",
        address: "Zabrat, Bakü",
        type: "Arsa",
        zoning: "Konut imarlı",
        area: 400,
        price: 88000,
        featured: false,
        listedAt: "2026-10-06",
        terrain: "Düz arazi",
        roadAccess: "Yola cepheli",
        utilities: ["Elektrik", "Su", "Doğalgaz"],
        description:
            "Zabrat'ta yerleşim alanı içinde bulunan, konut yapımı için uygun arsa. Altyapı hatlarına ve ana ulaşım güzergahına yakındır.",
        image:
            "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=1200&q=85",
    },
    {
        id: "pirshagi-yatirim-arsasi",
        title: "Pirşağı'da sahile yakın arsa",
        district: "Pirşağı",
        address: "Pirşağı, Bakü",
        type: "Arsa",
        zoning: "Yatırımlık arsa",
        area: 700,
        price: 154000,
        featured: false,
        listedAt: "2026-10-02",
        terrain: "Düz arazi",
        roadAccess: "Asfalt yola 200 m",
        utilities: ["Elektrik", "Su"],
        description:
            "Pirşağı'da sahil bölgesine ve yerleşim noktalarına yakın konumda arsa. Yatırım veya gelecekteki arazi projeniz için değerlendirilebilir.",
        image:
            "https://images.unsplash.com/photo-1500382017468-9049fed747ef?auto=format&fit=crop&w=1200&q=85",
    },
    {
        id: "ramana-tarim-arazisi",
        title: "Ramana'da tarıma uygun arazi",
        district: "Ramana",
        address: "Ramana, Bakü",
        type: "Arsa",
        zoning: "Tarım arazisi",
        area: 1800,
        price: 72000,
        featured: false,
        listedAt: "2026-09-28",
        terrain: "Hafif eğimli arazi",
        roadAccess: "Toprak yola cepheli",
        utilities: ["Sulama suyu"],
        description:
            "Ramana'da tarımsal kullanıma elverişli, geniş ve ulaşılabilir arazi. Sulama imkanı bulunan parsel farklı tarım faaliyetleri için uygundur.",
        image:
            "https://images.unsplash.com/photo-1472396961693-142e6e269027?auto=format&fit=crop&w=1200&q=85",
    },
];

export function formatPrice(price) {
    return new Intl.NumberFormat("tr-TR").format(price);
}
