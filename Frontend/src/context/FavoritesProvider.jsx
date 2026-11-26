import { useCallback, useEffect, useMemo, useState } from "react";
import { FavoritesContext } from "./favoritesContext.js";
import {
    addFavorite,
    getMyFavoriteIds,
    getSession,
    removeFavorite,
} from "../services/api.js";

const EMPTY_FAVORITE_IDS = Object.freeze([]);

export function FavoritesProvider({ children }) {
    const [session, setSession] = useState(() => getSession());
    const [favoriteState, setFavoriteState] = useState({
        userId: null,
        ids: [],
        error: "",
    });
    const userId = session?.userId ?? null;

    useEffect(() => {
        const handleSessionChanged = () => setSession(getSession());
        window.addEventListener("arsaDuragi:sessionChanged", handleSessionChanged);
        return () => window.removeEventListener("arsaDuragi:sessionChanged", handleSessionChanged);
    }, []);

    useEffect(() => {
        if (!userId) return undefined;

        let isCurrent = true;
        getMyFavoriteIds()
            .then((ids) => {
                if (!Array.isArray(ids)) {
                    throw new Error("Sevimli elan siyahısı düzgün formatda deyil.");
                }
                if (isCurrent) {
                    setFavoriteState({ userId, ids: ids.map(Number), error: "" });
                }
            })
            .catch((error) => {
                if (isCurrent) {
                    setFavoriteState({ userId, ids: [], error: error.message });
                }
            });

        return () => {
            isCurrent = false;
        };
    }, [userId]);

    const isCurrentUserLoaded = favoriteState.userId === userId;
    const favoriteIds = useMemo(
        () => isCurrentUserLoaded ? favoriteState.ids : EMPTY_FAVORITE_IDS,
        [isCurrentUserLoaded, favoriteState.ids],
    );
    const isLoading = Boolean(userId) && !isCurrentUserLoaded;
    const error = isCurrentUserLoaded ? favoriteState.error : "";

    const toggleFavorite = useCallback(async (propertyId) => {
        const id = Number(propertyId);
        if (!userId) throw new Error("Sevimlilərə əlavə etmək üçün giriş edin.");
        if (!Number.isSafeInteger(id) || id < 1) {
            throw new Error("Bu elan sevimlilərə əlavə edilə bilmədi.");
        }

        const wasFavorite = favoriteIds.includes(id);
        if (wasFavorite) {
            await removeFavorite(id);
        } else {
            await addFavorite(id);
        }

        setFavoriteState((current) => {
            if (current.userId !== userId) return current;
            return {
                ...current,
                ids: wasFavorite
                    ? current.ids.filter((favoriteId) => favoriteId !== id)
                    : [...current.ids, id],
            };
        });
    }, [favoriteIds, userId]);

    const value = useMemo(() => ({
        session,
        favoriteIds,
        isLoading,
        error,
        toggleFavorite,
    }), [session, favoriteIds, isLoading, error, toggleFavorite]);

    return (
        <FavoritesContext.Provider value={value}>
            {children}
        </FavoritesContext.Provider>
    );
}
