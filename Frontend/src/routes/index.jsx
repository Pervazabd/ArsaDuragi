import { createBrowserRouter, Navigate } from "react-router-dom";

import Layout from "../layout/Layout.jsx";
import Hero from "../components/Hero.jsx";
import Products from "../components/Products.jsx";

import AuthPage from "../pages/AuthPage.jsx";
import PropertiesPage from "../pages/PropertiesPage.jsx";
import PropertyDetailsPage from "../pages/PropertyDetailsPage.jsx";
import Contact from "../pages/Contact.jsx";
import NotFoundPage from "../pages/NotFoundPage.jsx";
import AdminLayout from "../pages/admin/AdminLayout.jsx";
import AdminHome from "../pages/admin/admin.jsx";
import AdminListings from "../pages/admin/AdminListings.jsx";
import AdminUsers from "../pages/admin/AdminUsers.jsx";
import AdminLocations from "../pages/admin/AdminLocations.jsx";

function HomePage() {
    return (
        <>
            <Hero />
            <Products />
        </>
    );
}

const router = createBrowserRouter([
    { path: "/login", element: <AuthPage mode="login" /> },
    { path: "/register", element: <AuthPage mode="register" /> },
    {
        path: "/admin",
        element: <AdminLayout />,
        children: [
            { index: true, element: <AdminHome /> },
            { path: "elanlar", element: <AdminListings /> },
            { path: "istifadeciler", element: <AdminUsers /> },
            { path: "yerler", element: <AdminLocations /> },
        ],
    },
    {
        path: "/",
        element: <Layout />,
        children: [
            { index: true, element: <HomePage /> },
            { path: "properties", element: <PropertiesPage /> },
            { path: "properties/:id", element: <PropertyDetailsPage /> },
            { path: "contact", element: <Contact /> },
            { path: "products", element: <Navigate to="/properties" replace /> },
            { path: "products/:id", element: <Navigate to="/properties" replace /> },
            { path: "*", element: <NotFoundPage /> },
        ],
    },
]);

export default router;