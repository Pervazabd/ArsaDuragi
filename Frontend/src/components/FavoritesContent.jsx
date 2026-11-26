import { Link } from "react-router-dom";
import { Heart } from "lucide-react";
import PropertyCard from "./PropertyCard.jsx";
import { useI18n } from "../i18n/I18nContext.jsx";

function FavoritesContent({ favorites, isLoading, error }) {
    const { t } = useI18n();

    return (
        <section className="min-h-[65vh] bg-[#f7f6f2] px-4 py-12 sm:px-6 sm:py-16 lg:px-10">
            <div className="mx-auto max-w-7xl">
                <div className="mb-9 flex items-end justify-between gap-5">
                    <div>
                        <span className="inline-flex items-center gap-2 text-xs font-semibold tracking-[0.16em] text-[#9a794f] uppercase">
                            <Heart size={15} />
                            {t("favoritesEyebrow")}
                        </span>
                        <h1 className="mt-3 font-display text-4xl font-medium text-[#292725] sm:text-5xl">
                            {t("favoritesTitle")}
                        </h1>
                        <p className="mt-3 max-w-xl text-sm leading-6 text-[#77726b]">
                            {t("favoritesDescription")}
                        </p>
                    </div>
                    <span className="hidden size-14 place-items-center rounded-2xl bg-[#efe8dc] text-[#9a794f] sm:grid">
                        <Heart size={24} />
                    </span>
                </div>

                {isLoading ? (
                    <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
                        {[1, 2, 3].map((item) => (
                            <div
                                key={item}
                                className="h-[390px] animate-pulse rounded-2xl bg-[#e7e3db]"
                            />
                        ))}
                    </div>
                ) : error ? (
                    <p
                        role="alert"
                        className="rounded-2xl border border-red-200 bg-red-50 px-5 py-4 text-sm text-red-700"
                    >
                        {error}
                    </p>
                ) : favorites.length ? (
                    <div className="grid items-stretch gap-5 sm:grid-cols-2 lg:grid-cols-3 lg:gap-6">
                        {favorites.map((favorite) => (
                            <PropertyCard
                                key={favorite.id}
                                property={favorite.property}
                            />
                        ))}
                    </div>
                ) : (
                    <div className="rounded-2xl border border-dashed border-[#dcd8ce] bg-white px-5 py-16 text-center">
                        <span className="mx-auto grid size-14 place-items-center rounded-2xl bg-[#f5f0e6] text-[#a48454]">
                            <Heart size={24} />
                        </span>
                        <h2 className="mt-5 text-lg font-semibold text-[#252720]">
                            {t("favoritesEmptyTitle")}
                        </h2>
                        <p className="mx-auto mt-2 max-w-md text-sm leading-6 text-[#77776f]">
                            {t("favoritesEmptyDescription")}
                        </p>
                        <Link
                            to="/properties"
                            className="mt-6 inline-flex rounded-xl bg-[#252720] px-5 py-3 text-sm font-semibold text-white transition hover:bg-[#3b3d34]"
                        >
                            {t("browseListings")}
                        </Link>
                    </div>
                )}
            </div>
        </section>
    );
}

export default FavoritesContent;
