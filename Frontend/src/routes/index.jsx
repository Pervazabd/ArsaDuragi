import Layout from "../layout/clinet/layout.jsx";
import Home from "../pages/Home.jsx";
import Products from "../pages/Products.jsx";
import NotFound from "../pages/NotFound.jsx";
import AboutUs from "../pages/AboutUs.jsx";
import Map from "../pages/Map.jsx";
import Admin from "../pages/Admin.jsx";
import ProductDetail from "../pages/ProductDetail.jsx";

const routes = [
    { path: "/admin/*", element: <Admin /> },
    {
        path: "/",
        element: <Layout />,
        children: [
            { index: true, element: <Home /> },
            { path: "products", element: <Products /> },
            { path: "products/:id", element: <ProductDetail /> },
            { path: "properties", element: <Products /> },
            { path: "about-us", element: <AboutUs /> },
            { path: "map", element: <Map /> },
            { path: "*", element: <NotFound /> },
        ],
    },
];

export default routes;