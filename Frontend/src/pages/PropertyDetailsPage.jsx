import {
    ArrowLeft,
    Check,
    LandPlot,
    MapPin,
    Route,
    Ruler,
} from "lucide-react";
import { Link, useParams } from "react-router-dom";
import { formatPrice, properties } from "../data/properties.js";

function PropertyDetailsPage() {
    const { id } = useParams();
    const property = properties.find((item) => item.id === id);

    if (!property) {
        return (
            <section className="mx-auto flex min-h-[60vh] max-w-7xl flex-col items-center justify-center px-5 text-center">
                <span className="text-sm font-semibold tracking-[0.2em] text-ochre uppercase">
                    İlan bulunamadı
                </span>
                <h1 className="mt-3 font-display text-4xl text-ink">
                    Bu arsa ilanı artık mevcut değil.
                </h1>
                <Link
                    className="mt-6 rounded-xl bg-coal px-5 py-3 text-sm font-semibold text-white"
                    to="/properties"
                >
                    Arsa ilanlarına dön
                </Link>
            </section>
        );
    }

    return (
        <article className="mx-auto max-w-7xl px-5 py-8 sm:px-8 sm:py-12 lg:px-12">
            <Link
                className="mb-6 inline-flex items-center gap-2 text-sm font-semibold text-coal transition hover:text-ochre"
                to="/properties"
            >
                <ArrowLeft size={17} />
                Tüm arsalara dön
            </Link>

            <div className="grid gap-8 lg:grid-cols-[minmax(0,1.35fr)_minmax(320px,0.8fr)]">
                <div className="overflow-hidden rounded-2xl border border-ink/10 bg-surface">
                    <div className="relative aspect-[1.35] overflow-hidden bg-sand">
                        <img
                            className="size-full object-cover"
                            src={property.image}
                            alt={`${property.title} arazi görünümü`}
                            fetchPriority="high"
                        />
                        <span className="absolute top-5 left-5 inline-flex items-center gap-2 rounded-full bg-coal-dark/90 px-4 py-2 text-xs font-semibold text-white backdrop-blur">
                            <LandPlot size={15} className="text-[#d2b184]" />
                            Satılık arsa
                        </span>
                    </div>
                    <div className="grid gap-4 p-5 sm:grid-cols-3 sm:p-6">
                        <div className="rounded-xl bg-[#d9d5ce] p-4">
                            <p className="text-xs text-muted">Arazi alanı</p>
                            <p className="mt-2 flex items-center gap-2 font-semibold text-ink">
                                <Ruler size={17} className="text-ochre" />
                                {property.area.toLocaleString("tr-TR")} m²
                            </p>
                        </div>
                        <div className="rounded-xl bg-[#d9d5ce] p-4">
                            <p className="text-xs text-muted">İmar durumu</p>
                            <p className="mt-2 font-semibold text-ink">
                                {property.zoning}
                            </p>
                        </div>
                        <div className="rounded-xl bg-[#d9d5ce] p-4">
                            <p className="text-xs text-muted">Arazi yapısı</p>
                            <p className="mt-2 font-semibold text-ink">
                                {property.terrain}
                            </p>
                        </div>
                    </div>
                </div>

                <aside className="h-fit rounded-2xl border border-ink/10 bg-surface p-5 sm:p-7">
                    <span className="inline-flex items-center gap-2 text-xs font-semibold tracking-[0.12em] text-ochre uppercase">
                        <span className="size-2 rounded-full bg-ochre" />
                        Satılık arsa
                    </span>
                    <h1 className="mt-3 font-display text-3xl leading-tight font-medium text-ink sm:text-4xl">
                        {property.title}
                    </h1>
                    <p className="mt-4 flex items-center gap-2 text-sm text-muted">
                        <MapPin size={17} className="shrink-0 text-ochre" />
                        {property.address}
                    </p>

                    <div className="mt-6 border-y border-ink/10 py-5">
                        <p className="text-xs text-muted">Satış fiyatı</p>
                        <p className="mt-1 text-3xl font-bold tracking-[-0.03em] text-coal">
                            {formatPrice(property.price)}
                            <span className="ml-2 text-sm font-semibold tracking-normal text-muted">
                                AZN
                            </span>
                        </p>
                        <p className="mt-2 text-xs text-muted">
                            {formatPrice(Math.round(property.price / property.area))}{" "}
                            AZN / m²
                        </p>
                    </div>

                    <p className="mt-5 text-sm leading-7 text-muted">
                        {property.description}
                    </p>

                    <div className="mt-6 rounded-xl border border-ink/10 bg-[#d9d5ce] p-4">
                        <p className="text-xs font-semibold text-ink">
                            Ulaşım ve altyapı
                        </p>
                        <p className="mt-3 flex items-start gap-2 text-sm text-muted">
                            <Route
                                size={16}
                                className="mt-0.5 shrink-0 text-ochre"
                            />
                            {property.roadAccess}
                        </p>
                        <div className="mt-3 flex flex-wrap gap-2">
                            {property.utilities.map((utility) => (
                                <span
                                    className="inline-flex items-center gap-1.5 rounded-lg border border-ink/10 bg-surface px-2.5 py-1.5 text-xs text-coal"
                                    key={utility}
                                >
                                    <Check size={13} className="text-ochre" />
                                    {utility}
                                </span>
                            ))}
                        </div>
                    </div>

                    <a
                        className="mt-6 inline-flex w-full items-center justify-center rounded-xl bg-coal px-5 py-3.5 text-sm font-semibold text-white transition hover:bg-coal-dark"
                        href="tel:+994501234567"
                    >
                        Arsa hakkında bilgi alın
                    </a>
                </aside>
            </div>
        </article>
    );
}

export default PropertyDetailsPage;
