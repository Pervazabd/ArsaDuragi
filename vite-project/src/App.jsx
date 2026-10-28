import { Route, Routes } from "react-router-dom";
import Hero from "./components/Hero.jsx";
import Products from "./components/Products.jsx";
import Layout from "./layout/Layout.jsx";
import PropertiesPage from "./pages/PropertiesPage.jsx";
import PropertyDetailsPage from "./pages/PropertyDetailsPage.jsx";

function App() {
    return (
        <Routes>
            <Route element={<Layout />}>
                <Route
                    index
                    element={
                        <>
                            <Hero />
                            <Products />
                        </>
                    }
                />
                <Route path="properties" element={<PropertiesPage />} />
                <Route
                    path="properties/:id"
                    element={<PropertyDetailsPage />}
                />
                <Route
                    path="*"
                    element={
                        <section className="mx-auto flex min-h-[60vh] max-w-7xl flex-col items-center justify-center px-5 text-center">
                            <span className="text-sm font-semibold tracking-[0.2em] text-ochre uppercase">
                                404
                            </span>
                            <h1 className="mt-3 font-display text-4xl text-ink">
                                Sayfa bulunamadı
                            </h1>
                            <a
                                className="mt-6 rounded-xl bg-coal px-5 py-3 text-sm font-semibold text-white"
                                href="/"
                            >
                                Ana sayfaya dön
                            </a>
                        </section>
                    }
                />
            </Route>
        </Routes>
    );
}

export default App;
