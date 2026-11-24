import { useState } from "react";
import { Link, NavLink, useNavigate } from "react-router-dom";
import { LayoutDashboard, Building2, Users, MapPin, ExternalLink, LogOut } from "lucide-react";
import { logout } from "../../services/api";

const menu = [
    { label: "Genel Bakış", to: "/admin", icon: LayoutDashboard },
    { label: "İlanlar", to: "/admin/elanlar", icon: Building2 },
    { label: "Kullanıcılar", to: "/admin/istifadeciler", icon: Users },
    { label: "Şehir ve İlçeler", to: "/admin/yerler", icon: MapPin },
];

export default function AdminSidebar() {
    const [menuOpen, setMenuOpen] = useState(false);
    const navigate = useNavigate();

    const handleLogout = async () => {
        await logout();
        navigate("/login");
    };

    return (
        <aside className="border-b border-[#e2e4dc] bg-[#192019] p-5 text-white md:min-h-screen md:border-b-0 md:border-r md:border-white/10">
            <div className="flex items-center justify-between gap-3">
                <Link
                    to="/admin"
                    onClick={() => setMenuOpen(false)}
                    className="text-lg font-bold text-[#d7b783]"
                >
                    Arsa Durağı <span className="text-xs font-medium text-white/45">ADMIN</span>
                </Link>

                <button
                    type="button"
                    aria-expanded={menuOpen}
                    aria-controls="admin-menu"
                    onClick={() => setMenuOpen((open) => !open)}
                    className="rounded-lg border border-slate-200 px-3 py-2 text-sm md:hidden"
                >
                    {menuOpen ? "Kapat" : "Menü"}
                </button>
            </div>

            <nav
                id="admin-menu"
                aria-label="Admin menyusu"
                className={`${menuOpen ? "block" : "hidden"} mt-5 space-y-1 md:block`}
            >
                {menu.map(({ label, to, icon: Icon }) => (
                    <NavLink
                        key={to}
                        to={to}
                        end={to === "/admin"}
                        onClick={() => setMenuOpen(false)}
                        className={({ isActive }) =>
                            `flex items-center gap-3 rounded-xl px-3 py-3 text-sm font-medium transition ${isActive
                                ? "bg-[#c5a475] text-[#192019]"
                                : "text-white/65 hover:bg-white/[0.06] hover:text-white"
                            }`
                        }
                    >
                        <Icon size={18} />
                        {label}
                    </NavLink>
                ))}

                <div className="mt-5 border-t border-white/10 pt-4">
                    <Link to="/" className="flex items-center gap-3 rounded-xl px-3 py-3 text-sm text-white/60 hover:bg-white/[0.06] hover:text-white">
                        <ExternalLink size={17} />
                        Siteye dön
                    </Link>
                    <button type="button" onClick={handleLogout} className="flex w-full items-center gap-3 rounded-xl px-3 py-3 text-sm text-white/60 hover:bg-red-500/10 hover:text-red-300">
                        <LogOut size={17} />
                        Çıkış
                    </button>
                </div>
            </nav>
        </aside>
    );
}