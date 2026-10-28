import { useMemo, useState } from "react";
import {
    ArrowDownWideNarrow,
    MapPin,
    RotateCcw,
    Search,
    SlidersHorizontal,
} from "lucide-react";
import { properties } from "../data/properties.js";
import DropdownSelect from "../components/DropdownSelect.jsx";
import PropertyCard from "../components/PropertyCard.jsx";

const initialFilters = {
    query: "",
    zoning: "",
    district: "",
    minPrice: "",
    maxPrice: "",
    minArea: "",
    maxArea: "",
    sort: "newest",
};

const fieldClass =
    "mt-2 h-11 w-full rounded-xl border border-ink/10 bg-[#d9d5ce] px-3 text-sm text-ink outline-none transition placeholder:text-muted/70 focus:border-ochre focus:ring-2 focus:ring-ochre/15";
const labelClass = "text-xs font-semibold tracking-wide text-coal";

function FilterField({ label, children }) {
    return (
        <div>
            <span className={labelClass}>{label}</span>
            {children}
        </div>
    );
}

function PropertiesPage() {
    const [filters, setFilters] = useState(initialFilters);

    const updateFilter = (key, value) => {
        setFilters((current) => ({ ...current, [key]: value }));
    };

    const filteredProperties = useMemo(() => {
        const query = filters.query.trim().toLocaleLowerCase("tr-TR");
        const minPrice =
            filters.minPrice === "" ? null : Number(filters.minPrice);
        const maxPrice =
            filters.maxPrice === "" ? null : Number(filters.maxPrice);
        const minArea =
            filters.minArea === "" ? null : Number(filters.minArea);
        const maxArea =
            filters.maxArea === "" ? null : Number(filters.maxArea);

        const matches = properties.filter((property) => {
            const searchableText = [
                property.title,
                property.address,
                property.district,
                property.type,
                property.zoning,
                property.description,
            ]
                .join(" ")
                .toLocaleLowerCase("tr-TR");

            return (
                (!query || searchableText.includes(query)) &&
                (!filters.zoning || property.zoning === filters.zoning) &&
                (!filters.district || property.district === filters.district) &&
                (minPrice === null || property.price >= minPrice) &&
                (maxPrice === null || property.price <= maxPrice) &&
                (minArea === null || property.area >= minArea) &&
                (maxArea === null || property.area <= maxArea)
            );
        });

        return matches.sort((first, second) => {
            if (filters.sort === "price-asc") {
                return first.price - second.price;
            }
            if (filters.sort === "price-desc") {
                return second.price - first.price;
            }
            if (filters.sort === "area-desc") {
                return second.area - first.area;
            }
            return new Date(second.listedAt) - new Date(first.listedAt);
        });
    }, [filters]);

    const activeFilterCount = Object.entries(filters).filter(
        ([key, value]) => key !== "sort" && value !== "",
    ).length;

    const districts = [
        ...new Set(properties.map((property) => property.district)),
    ].sort((first, second) => first.localeCompare(second, "tr"));

    return (
        <div className="min-h-[70vh]">
            <section className="border-b border-ink/10 bg-[#cbc6bd]">
                <div className="mx-auto max-w-7xl px-5 py-12 sm:px-8 sm:py-16 lg:px-12">
                    <span className="mb-3 block text-[11px] font-semibold tracking-[0.22em] text-ochre uppercase">
                        Arsa Durağı portföyü
                    </span>
                    <div className="flex flex-col gap-6 md:flex-row md:items-end md:justify-between">
                        <div>
                            <h1 className="font-display text-4xl leading-tight font-medium tracking-[-0.025em] text-ink sm:text-5xl">
                                Satılık arsa ve araziler
                            </h1>
                            <p className="mt-3 max-w-2xl text-sm leading-6 text-muted sm:text-base">
                                Bakü ve çevresindeki arsa ve tarım arazilerini
                                konum, imar durumu, fiyat ve büyüklüğe göre
                                filtreleyin.
                            </p>
                        </div>
                        <div className="flex shrink-0 items-center gap-3 text-sm text-muted">
                            <span className="grid size-10 place-items-center rounded-xl bg-[#b9b2a7] text-coal">
                                <MapPin size={18} />
                            </span>
                            Bakü ve çevresi
                        </div>
                    </div>

                    <label className="mt-8 flex h-14 max-w-3xl items-center gap-3 rounded-2xl border border-ink/10 bg-[#e0ddd7] px-4 shadow-sm focus-within:border-ochre focus-within:ring-2 focus-within:ring-ochre/15">
                        <Search className="shrink-0 text-muted" size={20} />
                        <input
                            className="h-full min-w-0 flex-1 bg-transparent text-sm text-ink outline-none placeholder:text-muted/75 sm:text-base"
                            type="search"
                            value={filters.query}
                            onChange={(event) =>
                                updateFilter("query", event.target.value)
                            }
                            placeholder="Bölge, arsa adı veya imar durumu ara"
                            aria-label="Arsalarda ara"
                        />
                    </label>
                </div>
            </section>

            <section className="mx-auto max-w-7xl px-5 py-10 sm:px-8 sm:py-14 lg:px-12">
                <div className="grid items-start gap-8 lg:grid-cols-[270px_minmax(0,1fr)]">
                    <aside className="rounded-2xl border border-ink/10 bg-surface p-5 sm:p-6">
                        <div className="flex items-center justify-between border-b border-ink/10 pb-4">
                            <h2 className="flex items-center gap-2 font-semibold text-ink">
                                <SlidersHorizontal
                                    size={18}
                                    className="text-ochre"
                                />
                                Arsa filtreleri
                            </h2>
                            {activeFilterCount > 0 && (
                                <span className="grid size-6 place-items-center rounded-full bg-coal text-xs font-semibold text-white">
                                    {activeFilterCount}
                                </span>
                            )}
                        </div>

                        <div className="mt-5 space-y-5">
                            <FilterField label="İmar / arazi durumu">
                                <DropdownSelect
                                    ariaLabel="İmar veya arazi durumuna göre filtrele"
                                    value={filters.zoning}
                                    onChange={(value) =>
                                        updateFilter("zoning", value)
                                    }
                                    options={[
                                        {
                                            value: "",
                                            label: "Tüm arazi türleri",
                                        },
                                        {
                                            value: "Konut imarlı",
                                            label: "Konut imarlı",
                                        },
                                        {
                                            value: "Tarım arazisi",
                                            label: "Tarım arazisi",
                                        },
                                        {
                                            value: "Yatırımlık arsa",
                                            label: "Yatırımlık arsa",
                                        },
                                    ]}
                                />
                            </FilterField>

                            <FilterField label="Bölge">
                                <DropdownSelect
                                    ariaLabel="Bölgeye göre filtrele"
                                    value={filters.district}
                                    onChange={(value) =>
                                        updateFilter("district", value)
                                    }
                                    options={[
                                        {
                                            value: "",
                                            label: "Tüm bölgeler",
                                        },
                                        ...districts.map((district) => ({
                                            value: district,
                                            label: district,
                                        })),
                                    ]}
                                />
                            </FilterField>

                            <div>
                                <span className={labelClass}>
                                    Fiyat aralığı (AZN)
                                </span>
                                <div className="mt-2 grid grid-cols-2 gap-2">
                                    <input
                                        className={fieldClass.replace("mt-2 ", "")}
                                        type="number"
                                        min="0"
                                        inputMode="numeric"
                                        placeholder="En az"
                                        aria-label="En düşük fiyat"
                                        value={filters.minPrice}
                                        onChange={(event) =>
                                            updateFilter(
                                                "minPrice",
                                                event.target.value,
                                            )
                                        }
                                    />
                                    <input
                                        className={fieldClass.replace("mt-2 ", "")}
                                        type="number"
                                        min="0"
                                        inputMode="numeric"
                                        placeholder="En çok"
                                        aria-label="En yüksek fiyat"
                                        value={filters.maxPrice}
                                        onChange={(event) =>
                                            updateFilter(
                                                "maxPrice",
                                                event.target.value,
                                            )
                                        }
                                    />
                                </div>
                            </div>

                            <div>
                                <span className={labelClass}>Arazi alanı (m²)</span>
                                <div className="mt-2 grid grid-cols-2 gap-2">
                                    <input
                                        className={fieldClass.replace("mt-2 ", "")}
                                        type="number"
                                        min="0"
                                        inputMode="numeric"
                                        placeholder="En az"
                                        aria-label="En küçük arazi alanı"
                                        value={filters.minArea}
                                        onChange={(event) =>
                                            updateFilter(
                                                "minArea",
                                                event.target.value,
                                            )
                                        }
                                    />
                                    <input
                                        className={fieldClass.replace("mt-2 ", "")}
                                        type="number"
                                        min="0"
                                        inputMode="numeric"
                                        placeholder="En çok"
                                        aria-label="En büyük arazi alanı"
                                        value={filters.maxArea}
                                        onChange={(event) =>
                                            updateFilter(
                                                "maxArea",
                                                event.target.value,
                                            )
                                        }
                                    />
                                </div>
                            </div>
                        </div>

                        <button
                            className="mt-6 inline-flex w-full items-center justify-center gap-2 rounded-xl border border-ink/10 px-4 py-3 text-sm font-semibold text-coal transition hover:bg-sand"
                            type="button"
                            onClick={() => setFilters(initialFilters)}
                        >
                            <RotateCcw size={15} />
                            Filtreleri temizle
                        </button>
                    </aside>

                    <div className="min-w-0">
                        <div className="mb-6 flex flex-col gap-4 border-b border-ink/10 pb-5 sm:flex-row sm:items-center sm:justify-between">
                            <div>
                                <p className="font-semibold text-ink">
                                    {filteredProperties.length} arsa bulundu
                                </p>
                                <p className="mt-1 text-xs text-muted">
                                    Filtrelerinize uygun satılık araziler
                                </p>
                            </div>
                            <div className="flex w-full items-center gap-2 self-start sm:w-auto sm:self-auto">
                                <ArrowDownWideNarrow
                                    size={17}
                                    className="ml-1 shrink-0 text-muted"
                                />
                                <DropdownSelect
                                    className="sm:w-[245px]"
                                    variant="compact"
                                    value={filters.sort}
                                    ariaLabel="Arsaları sıralama ölçütü"
                                    onChange={(value) =>
                                        updateFilter("sort", value)
                                    }
                                    options={[
                                        {
                                            value: "newest",
                                            label: "En yeni",
                                            description:
                                                "Son eklenen ilanlar önce gösterilir",
                                        },
                                        {
                                            value: "price-asc",
                                            label: "Fiyat: düşükten yükseğe",
                                            description:
                                                "En uygun fiyatlı arsalar önce",
                                        },
                                        {
                                            value: "price-desc",
                                            label: "Fiyat: yüksekten düşüğe",
                                            description:
                                                "En yüksek fiyatlı arsalar önce",
                                        },
                                        {
                                            value: "area-desc",
                                            label: "En geniş arazi",
                                            description:
                                                "Arazi büyüklüğüne göre sıralanır",
                                        },
                                    ]}
                                />
                            </div>
                        </div>

                        {filteredProperties.length > 0 ? (
                            <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3">
                                {filteredProperties.map((property) => (
                                    <PropertyCard
                                        key={property.id}
                                        property={property}
                                    />
                                ))}
                            </div>
                        ) : (
                            <div className="flex min-h-72 flex-col items-center justify-center rounded-2xl border border-dashed border-ink/20 bg-surface/60 px-6 text-center">
                                <span className="grid size-12 place-items-center rounded-full bg-sand text-muted">
                                    <Search size={22} />
                                </span>
                                <h2 className="mt-4 text-lg font-semibold text-ink">
                                    Uygun arsa bulunamadı
                                </h2>
                                <p className="mt-2 max-w-sm text-sm leading-6 text-muted">
                                    Arama kelimenizi veya filtrelerinizi
                                    değiştirip tekrar deneyin.
                                </p>
                                <button
                                    className="mt-5 rounded-xl bg-coal px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-coal-dark"
                                    type="button"
                                    onClick={() => setFilters(initialFilters)}
                                >
                                    Tüm arsaları göster
                                </button>
                            </div>
                        )}
                    </div>
                </div>
            </section>
        </div>
    );
}

export default PropertiesPage;
