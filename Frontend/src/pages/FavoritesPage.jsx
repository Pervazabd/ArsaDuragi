import { useEffect, useState } from "react";
import { Navigate } from "react-router-dom";
import FavoritesContent from "../components/FavoritesContent.jsx";
import { useFavorites } from "../context/useFavorites.js";
import { getMyFavorites } from "../services/api.js";

function FavoritesPage() {
    const {
        session,
        favoriteIds,
        isLoading: favoriteIdsLoading,
        error: favoriteIdsError,
    } = useFavorites();
    const [favorites, setFavorites] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");
    const userId = session?.userId ?? null;

    useEffect(() => {
        if (!userId) return undefined;

        let isCurrent = true;
        getMyFavorites()
            .then((data) => {
                const items = data?.items ?? data?.Items;
                if (!Array.isArray(items)) {
                    throw new Error("Sevimli elan siyahısı düzgün formatda deyil.");
                }
                if (isCurrent) setFavorites(items);
            })
            .catch((requestError) => {
                if (isCurrent) setError(requestError.message);
            })
            .finally(() => {
                if (isCurrent) setIsLoading(false);
            });

        return () => {
            isCurrent = false;
        };
    }, [userId]);

    if (!session) {
        return <Navigate to="/login" replace state={{ from: "/favorites" }} />;
    }

    const visibleFavorites = favorites.filter((favorite) =>
        favoriteIds.includes(Number(favorite.propertyAdId)),
    );
    const loading = isLoading || favoriteIdsLoading;

    return (
        <FavoritesContent
            favorites={visibleFavorites}
            isLoading={loading}
            error={error || favoriteIdsError}
        />
    );
}

export default FavoritesPage;
