import { useEffect } from "react";
import Footer from "./Footer.jsx";
import Navbar from "./Navbar.jsx";
import { Outlet, useLocation } from "react-router-dom";
import { FavoritesProvider } from "../context/FavoritesProvider.jsx";

function Layout() {
    const { pathname, hash } = useLocation();

    useEffect(() => {
        if (hash) {
            window.requestAnimationFrame(() => {
                document.querySelector(hash)?.scrollIntoView({
                    behavior: "smooth",
                    block: "start",
                });
            });
            return;
        }

        window.scrollTo({ top: 0, left: 0, behavior: "auto" });
    }, [pathname, hash]);

    return (
        <FavoritesProvider>
            <div className="flex min-h-screen flex-col overflow-x-hidden">
                <Navbar />
                <div key={pathname} className="page-enter flex flex-1 flex-col">
                    <main className="flex-1">
                        <Outlet />
                    </main>
                    <Footer />
                </div>
            </div>
        </FavoritesProvider>
    );
}

export default Layout;
