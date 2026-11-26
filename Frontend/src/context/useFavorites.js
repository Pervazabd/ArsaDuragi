import { useContext } from "react";
import { FavoritesContext } from "./favoritesContext.js";

export function useFavorites() {
    const context = useContext(FavoritesContext);
    if (!context) {
        throw new Error("useFavorites must be used within FavoritesProvider.");
    }
    return context;
}
