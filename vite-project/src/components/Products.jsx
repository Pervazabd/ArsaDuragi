
import { useMemo, useState } from "react";
import {
    ArrowDownWideNarrow,
    ArrowRight,
    ChevronDown,
    LandPlot,
    MapPin,
    Search,
    SlidersHorizontal,
    X,
} from "lucide-react";
import { Link } from "react-router-dom";
import { properties } from "../data/properties.js";
import DropdownSelect from "./DropdownSelect.jsx";
import PropertyCard from "./PropertyCard.jsx";

const districts = [
    "Bilecik Merkez",
    "Bozüyük",
    "Gölpazarı",
    "İnhisar",
    "Osmaneli",
    "Pazaryeri",
    "Söğüt",
    "Yenipazar",
];

const landTypes = [
    { value: "all", label: "Tüm arazi türleri" },
    { value: "arsa", label: "Arsa" },
    { value: "tarla", label: "Tarla" },
    { value: "bağ", label: "Bağ / Bahçe" },
    { value: "yatırım", label: "Yatırımlık arazi" },
];

function Products() {
    const [search, setSearch] = useState("");
    const [district, setDistrict] = useState("all");
    const [landType, setLandType] = useState("all");
    const [sort, setSort] = useState("featured");
    const [filtersOpen, setFiltersOpen] = useState(false);

    const featuredProperties = useMemo(() => {
        let result = properties.filter((property) => property.featured);

        if (search.trim()) {
            const query = search.trim().toLocaleLowerCase("tr-TR");

            result = result.filter((property) =>
                [
                    property.title,
                    property.address,
                    property.type,
                    property.transaction,
                ]
                    .filter(Boolean)
                    .some((value) =>
                        String(value)
                            .toLocaleLowerCase("tr-TR")
                            .includes(query),
                    ),
            );
        }

        if (district !== "all") {
            result = result.filter((property) => {
                const address = String(property.address || "")
                    .toLocaleLowerCase("tr-TR")
                    .replaceAll("ı", "i");

                const selectedDistrict = district
                    .toLocaleLowerCase("tr-TR")
                    .replaceAll("ı", "i");

                return address.includes(selectedDistrict);
            });
        }

        if (landType !== "all") {
            result = result.filter((property) => {
                const type = String(property.type || "")
                    .toLocaleLowerCase("tr-TR")
                    .replaceAll("ı", "i");

                const selectedType = landType
                    .toLocaleLowerCase("tr-TR")
                    .replaceAll("ı", "i");

                if (selectedType === "yatirim") {
                    return (
                        type.includes("yatırım") ||
                        type.includes("yatirim") ||
                        property.investment === true
                    );
                }

                if (selectedType === "bağ") {
                    return (
                        type.includes("bağ") ||
                        type.includes("bahçe") ||
                        type.includes("bag") ||
                        type.includes("bahce")
                    );
                }

                return type.includes(selectedType);
            });
        }

        if (sort === "priceAsc") {
            result.sort((a, b) => Number(a.price) - Number(b.price));
        } else if (sort === "priceDesc") {
            result.sort((a, b) => Number(b.price) - Number(a.price));
        } else if (sort === "areaDesc") {
            result.sort((a, b) => Number(b.area) - Number(a.area));
        }

        return result;
    }, [search, district, landType, sort]);

    const activeFilterCount = [
        district !== "all",
        landType !== "all",
    ].filter(Boolean).length;

    const resetFilters = () => {
        setSearch("");
        setDistrict("all");
        setLandType("all");
        setSort("featured");
    };

    return (
        <section
            id="properties"
            className="relative overflow-hidden bg-[#f7f6f2] px-4 py-16 sm:px-6 sm:py-20 lg:px-10 lg:py-28"
        >
            <div className="mx-auto max-w-7xl">
                {/* Section heading */}
                <div className="mb-9 flex flex-col gap-5 sm:mb-12 sm:flex-row sm:items-end sm:justify-between">
                    <div>
                        <span className="mb-4 inline-flex items-center gap-2 text-[10px] font-bold tracking-[0.22em] text-[#a48454] uppercase sm:text-xs">
                            <span className="h-px w-7 bg-[#b99561]" />
                            Arsa Durağı seçkisi
                        </span>

                        <h2 className="max-w-2xl text-3xl leading-tight font-medium tracking-[-0.045em] text-[#22251f] sm:text-4xl lg:text-5xl">
                            Geleceğe yatırım,
                            <span className="font-serif italic text-[#a48454]">
                                {" "}doğru arazide.
                            </span>
                        </h2>

                        <p className="mt-4 max-w-xl text-sm leading-7 text-[#77776f] sm:text-base">
                            Bilecik ve çevresindeki arsa, tarla ve yatırım
                            fırsatlarını keşfedin.
                        </p>
                    </div>

                    <Link
                        to="/properties"
                        className="group inline-flex min-h-11 items-center gap-3 self-start rounded-xl border border-[#dedbd2] bg-white px-4 py-3 text-sm font-semibold text-[#292b25] transition duration-300 hover:border-[#b99561] hover:bg-[#fbf8f1] sm:self-auto"
                    >
                        Tüm ilanlar
                        <ArrowRight
                            size={16}
                            className="transition-transform group-hover:translate-x-1"
                        />
                    </Link>
                </div>

                {/* Search and filter toolbar */}
                <div className="mb-8 rounded-2xl border border-[#e8e5dc] bg-white p-3 shadow-[0_8px_35px_rgba(30,30,20,0.035)] sm:p-4">
                    <div className="flex flex-col gap-3 md:flex-row">
                        <label className="flex min-h-12 min-w-0 flex-1 items-center gap-3 rounded-xl border border-[#eeece5] bg-[#faf9f6] px-4 transition focus-within:border-[#b99561] focus-within:bg-white">
                            <Search
                                size={18}
                                className="shrink-0 text-[#a48454]"
                            />
                            <input
                                type="search"
                                value={search}
                                onChange={(event) => setSearch(event.target.value)}
                                placeholder="İlan, arsa türü veya konum ara..."
                                aria-label="İlanlarda ara"
                                className="min-w-0 flex-1 bg-transparent py-3 text-sm text-[#252720] outline-none placeholder:text-[#aaa99f]"
                            />

                            {search && (
                                <button
                                    type="button"
                                    onClick={() => setSearch("")}
                                    aria-label="Aramayı temizle"
                                    className="rounded-full p-1 text-[#77776f] hover:bg-black/5"
                                >
                                    <X size={15} />
                                </button>
                            )}
                        </label>

                        <button
                            type="button"
                            aria-expanded={filtersOpen}
                            aria-controls="property-filters"
                            onClick={() => setFiltersOpen((open) => !open)}
                            className={`flex min-h-12 items-center justify-center gap-2 rounded-xl border px-5 text-sm font-semibold transition duration-200 ${filtersOpen || activeFilterCount > 0
                                    ? "border-[#b99561] bg-[#f8f3e9] text-[#775c35]"
                                    : "border-[#e8e5dc] bg-white text-[#393a33] hover:border-[#c9b18b]"
                                }`}
                        >
                            <SlidersHorizontal size={17} />
                            Filtrele
                            {activeFilterCount > 0 && (
                                <span className="grid size-5 place-items-center rounded-full bg-[#b99561] text-[10px] text-white">
                                    {activeFilterCount}
                                </span>
                            )}
                            <ChevronDown
                                size={15}
                                className={`transition-transform duration-300 ${filtersOpen ? "rotate-180" : ""
                                    }`}
                            />
                        </button>

                        <div className="flex min-h-12 items-center gap-2 rounded-xl border border-[#e8e5dc] px-3">
                            <ArrowDownWideNarrow
                                size={16}
                                className="shrink-0 text-[#a48454]"
                            />
                            <DropdownSelect
                                className="flex-1"
                                variant="compact"
                                value={sort}
                                onChange={setSort}
                                aria-label="İlanları sırala"
                                options={[
                                    {
                                        value: "featured",
                                        label: "Öne çıkanlar",
                                    },
                                    {
                                        value: "priceAsc",
                                        label: "Fiyat: Artan",
                                    },
                                    {
                                        value: "priceDesc",
                                        label: "Fiyat: Azalan",
                                    },
                                    {
                                        value: "areaDesc",
                                        label: "Alan: Büyükten küçüğe",
                                    },
                                ]}
                            />
                        </div>
                    </div>

                    {/* Animated filters */}
                    <div
                        id="property-filters"
                        aria-hidden={!filtersOpen}
                        className={`grid transition-[grid-template-rows,opacity,margin] duration-300 ease-out ${filtersOpen
                                ? "mt-4 grid-rows-[1fr] opacity-100"
                                : "mt-0 grid-rows-[0fr] opacity-0"
                            }`}
                    >
                        <div className="overflow-hidden">
                            <div className="grid gap-4 border-t border-[#eeece5] pt-4 sm:grid-cols-2">
                                <div className="block min-w-0">
                                    <span className="mb-2 block text-xs font-semibold text-[#55564e]">
                                        <MapPin
                                            size={14}
                                            className="mr-1 inline text-[#a48454]"
                                        />
                                        İlçe / Bölge
                                    </span>
                                    <DropdownSelect
                                        className="mt-2"
                                        value={district}
                                        onChange={setDistrict}
                                        ariaLabel="Bölgeye göre filtrele"
                                        options={[
                                            {
                                                value: "all",
                                                label: "Tüm bölgeler",
                                            },
                                            ...districts.map((item) => ({
                                                value: item,
                                                label: item,
                                            })),
                                        ]}
                                    />
                                </div>

                                <div className="block min-w-0">
                                    <span className="mb-2 block text-xs font-semibold text-[#55564e]">
                                        <LandPlot
                                            size={14}
                                            className="mr-1 inline text-[#a48454]"
                                        />
                                        Arazi türü
                                    </span>
                                    <DropdownSelect
                                        className="mt-2"
                                        value={landType}
                                        onChange={setLandType}
                                        ariaLabel="Arazi türüne göre filtrele"
                                        options={landTypes}
                                    />
                                </div>
                            </div>

                            {activeFilterCount > 0 && (
                                <button
                                    type="button"
                                    onClick={resetFilters}
                                    className="mt-4 inline-flex items-center gap-2 text-xs font-semibold text-[#8b6c41] transition hover:text-[#49371f]"
                                >
                                    <X size={14} />
                                    Filtreleri temizle
                                </button>
                            )}
                        </div>
                    </div>
                </div>

                {/* Results count */}
                <div className="mb-5 flex flex-wrap items-center justify-between gap-3">
                    <p className="text-sm text-[#77776f]">
                        <span className="font-semibold text-[#252720]">
                            {featuredProperties.length}
                        </span>{" "}
                        öne çıkan ilan
                    </p>

                    <span className="inline-flex items-center gap-2 text-[11px] font-medium text-[#89877e]">
                        <span className="size-1.5 rounded-full bg-[#9aab7b]" />
                        Bilecik ve çevresi
                    </span>
                </div>

                {/* Property cards */}
                {featuredProperties.length > 0 ? (
                    <div className="grid items-stretch gap-5 sm:grid-cols-2 lg:grid-cols-3 lg:gap-6">
                        {featuredProperties.map((property) => (
                            <PropertyCard
                                key={property.id}
                                property={property}
                            />
                        ))}
                    </div>
                ) : (
                    <div className="rounded-2xl border border-dashed border-[#dcd8ce] bg-white px-5 py-14 text-center sm:py-20">
                        <span className="mx-auto grid size-14 place-items-center rounded-2xl bg-[#f5f0e6] text-[#a48454]">
                            <Search size={24} />
                        </span>

                        <h3 className="mt-5 text-lg font-semibold text-[#252720]">
                            Aramanıza uygun ilan bulunamadı
                        </h3>

                        <p className="mx-auto mt-2 max-w-sm text-sm leading-6 text-[#77776f]">
                            Farklı bir ilçe veya arazi türü seçerek yeniden
                            deneyebilirsiniz.
                        </p>

                        <button
                            type="button"
                            onClick={resetFilters}
                            className="mt-5 inline-flex items-center gap-2 rounded-xl bg-[#252720] px-5 py-3 text-sm font-semibold text-white transition hover:bg-[#3b3d34]"
                        >
                            Filtreleri temizle
                            <ArrowRight size={15} />
                        </button>
                    </div>
                )}
            </div>
        </section>
    );
}

export default Products;
