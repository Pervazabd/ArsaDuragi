
import {
    ArrowUpRight,
    LandPlot,
    MapPin,
    Ruler,
    ShieldCheck,
} from "lucide-react";
import { Link } from "react-router-dom";
import { formatPrice } from "../data/properties.js";

function PropertyCard({ property }) {
    const area = Number(property.area) || 0;

    return (
        <article className="group flex h-full flex-col overflow-hidden rounded-2xl border border-[#e8e5dc] bg-white transition duration-300 hover:-translate-y-1 hover:border-[#d4c3a6] hover:shadow-[0_20px_50px_rgba(38,38,26,0.09)]">
            {/* Arazi görseli */}
            <Link
                to={`/properties/${property.id}`}
                aria-label={`${property.title} ilanını incele`}
                className="relative block overflow-hidden bg-[#e9e7dc]"
            >
                <div className="aspect-[1.4] overflow-hidden">
                    <img
                        src={property.image}
                        alt={`${property.title} - arazi görünümü`}
                        loading="lazy"
                        className="size-full object-cover transition duration-700 ease-out group-hover:scale-[1.06]"
                    />
                </div>

                {/* Görsel overlay */}
                <div className="pointer-events-none absolute inset-0 bg-gradient-to-t from-black/45 via-transparent to-black/10" />

                <div className="absolute left-4 top-4 flex flex-wrap gap-2">
                    <span className="inline-flex items-center gap-1.5 rounded-full border border-white/20 bg-[#191b17]/85 px-3 py-1.5 text-[10px] font-semibold text-white shadow-lg backdrop-blur-md">
                        <LandPlot size={13} className="text-[#d2b184]" />
                        {property.type || "Arsa"}
                    </span>

                    {property.featured && (
                        <span className="rounded-full bg-[#d2b184] px-3 py-1.5 text-[10px] font-bold text-[#24251e]">
                            Öne Çıkan
                        </span>
                    )}
                </div>

                <span className="absolute bottom-4 left-4 inline-flex max-w-[85%] items-center gap-1.5 truncate text-xs font-medium text-white">
                    <MapPin size={14} className="shrink-0 text-[#e1c69a]" />
                    <span className="truncate">{property.address}</span>
                </span>

                <span className="absolute bottom-3 right-3 grid size-10 place-items-center rounded-full border border-white/30 bg-white/15 text-white backdrop-blur-md transition duration-300 group-hover:border-[#d2b184] group-hover:bg-[#d2b184] group-hover:text-[#202119]">
                    <ArrowUpRight size={19} />
                </span>
            </Link>

            {/* Kart içeriği */}
            <div className="flex flex-1 flex-col p-5 sm:p-6">
                <div className="flex items-center justify-between gap-3">
                    <span className="inline-flex items-center gap-1.5 text-[10px] font-semibold tracking-[0.12em] text-[#9a7b4d] uppercase">
                        <span className="size-1.5 rounded-full bg-[#b99561]" />
                        {property.transaction || "Satılık"}
                    </span>

                    {property.zoning && (
                        <span className="rounded-lg bg-[#f5f3ed] px-2.5 py-1 text-[10px] font-medium text-[#6e6e61]">
                            {property.zoning}
                        </span>
                    )}
                </div>

                <h3 className="mt-3 line-clamp-2 text-lg leading-7 font-semibold tracking-[-0.025em] text-[#272921] transition-colors group-hover:text-[#8c7046]">
                    {property.title}
                </h3>

                <div className="mt-5 flex items-center gap-3 rounded-xl bg-[#f8f7f3] px-3.5 py-3">
                    <span className="grid size-10 shrink-0 place-items-center rounded-xl border border-[#e8e0d1] bg-white text-[#a48454]">
                        <Ruler size={19} strokeWidth={1.7} />
                    </span>

                    <div className="min-w-0">
                        <p className="text-[10px] font-medium tracking-wide text-[#89877e]">
                            Toplam arazi alanı
                        </p>

                        <p className="mt-0.5 text-base font-semibold text-[#292b24]">
                            {area.toLocaleString("tr-TR")}{" "}
                            <span className="text-xs font-medium text-[#77776f]">
                                m²
                            </span>
                        </p>
                    </div>

                    <div className="ml-auto text-right">
                        <p className="text-[10px] text-[#89877e]">
                            Metrekare fiyatı
                        </p>

                        <p className="mt-0.5 text-sm font-semibold text-[#55564e]">
                            {area > 0
                                ? `${Math.round(
                                    Number(property.price) / area,
                                ).toLocaleString("tr-TR")} AZN`
                                : "—"}
                        </p>
                    </div>
                </div>

                {/* Fiyat */}
                <div className="mt-auto flex items-end justify-between gap-3 border-t border-[#eeece5] pt-5 mt-5">
                    <div className="min-w-0">
                        <p className="mb-1 text-[10px] font-medium tracking-wide text-[#89877e]">
                            Satış fiyatı
                        </p>

                        <p className="text-xl font-bold tracking-[-0.035em] text-[#262820] sm:text-2xl">
                            {formatPrice(property.price)}
                            <span className="ml-1.5 text-xs font-semibold tracking-normal text-[#8b8a80]">
                                AZN
                            </span>
                        </p>
                    </div>

                    <Link
                        to={`/properties/${property.id}`}
                        className="inline-flex shrink-0 items-center gap-1.5 rounded-xl bg-[#252720] px-3.5 py-3 text-xs font-semibold text-white transition hover:bg-[#a48454] sm:px-4"
                    >
                        İncele
                        <ArrowUpRight size={15} />
                    </Link>
                </div>

                {property.verified && (
                    <p className="mt-4 flex items-center gap-1.5 text-[10px] text-[#7b806b]">
                        <ShieldCheck size={14} />
                        Doğrulanmış ilan
                    </p>
                )}
            </div>
        </article>
    );
}

export default PropertyCard;