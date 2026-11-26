import { useRef, useState } from "react";
import { Heart, LoaderCircle } from "lucide-react";
import { useLocation, useNavigate } from "react-router-dom";
import { useFavorites } from "../context/useFavorites.js";
import { useI18n } from "../i18n/I18nContext.jsx";
import "./favorites.css";

export default function FavoriteButton({
    propertyId,
    className = "",
    showLabel = false,
}) {
    const { t } = useI18n();
    const {
        session,
        favoriteIds,
        isLoading,
        error,
        toggleFavorite,
    } = useFavorites();
    const navigate = useNavigate();
    const location = useLocation();

    const saving = useRef(false);
    const [pending, setPending] = useState(false);
    const [message, setMessage] = useState("");
    const [animation, setAnimation] = useState(0);

    const id = Number(propertyId);
    if (!Number.isSafeInteger(id) || id <= 0) return null;

    const selected = Boolean(session) && favoriteIds.some((item) => Number(item) === id);
    const busy = pending || Boolean(session && isLoading);
    const unavailable = Boolean(session && error);
    const feedback = message || (unavailable ? String(error.message || error) : "");
    const label = t(
        !session ? "loginToFavorite" : selected ? "removeFavorite" : "addFavorite",
    );

    async function handleClick() {
        if (saving.current || busy || unavailable) return;

        if (!session) {
            navigate("/login", {
                state: { from: location.pathname + location.search + location.hash },
            });
            return;
        }

        saving.current = true;
        setPending(true);
        setMessage("");

        try {
            await toggleFavorite(id);
            setAnimation((previous) => previous + 1);
        } catch (requestError) {
            setMessage(requestError?.message || "Favorit yenilənə bilmədi.");
        } finally {
            saving.current = false;
            setPending(false);
        }
    }

    return (
        <div className={`${showLabel ? "" : "favorite-control--compact"} ${className}`}>
            <button
                type="button"
                className={`favorite-button ${showLabel ? "favorite-button--labeled" : ""}`}
                onClick={handleClick}
                aria-label={label}
                aria-pressed={session ? selected : undefined}
                aria-busy={busy}
                aria-disabled={busy || unavailable}
            >
                {busy ? (
                    <LoaderCircle size={19} className="favorite-spinner" aria-hidden="true" />
                ) : (
                    <Heart
                        key={animation}
                        size={19}
                        fill={selected ? "currentColor" : "none"}
                        className={animation ? "favorite-heart-pop" : undefined}
                        aria-hidden="true"
                    />
                )}
                {showLabel && <span>{label}</span>}
            </button>
            {feedback && <p role="alert" className="favorite-button-error">{feedback}</p>}
        </div>
    );
}