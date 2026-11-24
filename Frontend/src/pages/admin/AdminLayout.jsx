import { Outlet, Navigate } from "react-router-dom";
import { getSession } from "../../services/api";
import AdminSidebar from "./AdminSidebar.jsx";

export default function AdminLayout() {
    const session = getSession();
    const isAdmin = session?.roles?.some(
        (role) => String(role).toLowerCase() === "admin"
    );

    if (!session || !isAdmin) {
        return <Navigate to="/login" replace />;
    }

    return (
        <div className="min-h-screen bg-[#f4f5f1] text-[#20241e] md:grid md:grid-cols-[260px_minmax(0,1fr)]">
            <AdminSidebar />
            <main className="min-w-0 p-4 sm:p-6 lg:p-8">
                <Outlet />
            </main>
        </div>
    );
}
