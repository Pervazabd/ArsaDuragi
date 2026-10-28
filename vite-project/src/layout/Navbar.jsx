
import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import DropdownSelect from "../components/DropdownSelect.jsx";
import logo from "../assets/arsa-duragi-logo.png";
import {
    ArrowRight,
    House,
    LogIn,
    Menu,
    Phone,
    UserPlus,
    X,
    Building2,
} from "lucide-react";

const navigation = [
    { label: "Ana Sayfa", href: "/", icon: House },
    { label: "İlanlar", href: "/properties", icon: Building2 },
    { label: "İletişim", href: "#contact", icon: Phone },
];

const languages = [
    { value: "tr", label: "Türkçe", triggerLabel: "TR" },
    { value: "az", label: "Azərbaycanca", triggerLabel: "AZ" },
    { value: "en", label: "English", triggerLabel: "EN" },
];

function Navbar() {
    const [menuOpen, setMenuOpen] = useState(false);
    const [language, setLanguage] = useState("tr");

    useEffect(() => {
        const handleKeyDown = (event) => {
            if (event.key === "Escape") {
                setMenuOpen(false);
            }
        };

        document.addEventListener("keydown", handleKeyDown);

        return () => {
            document.removeEventListener("keydown", handleKeyDown);
        };
    }, []);

    useEffect(() => {
        document.body.style.overflow = menuOpen ? "hidden" : "";

        return () => {
            document.body.style.overflow = "";
        };
    }, [menuOpen]);

    const closeMenus = () => {
        setMenuOpen(false);
    };

    return (
        <header className="sticky top-0 z-50 border-b border-white/[0.08] bg-[#151714]/95 text-white shadow-[0_8px_35px_rgba(0,0,0,0.12)] backdrop-blur-xl">
            <nav
                className="relative mx-auto flex h-[76px] max-w-7xl items-center justify-between gap-3 px-4 sm:px-6 xl:px-10"
                aria-label="Ana navigasyon"
            >
                {/* Logo */}
                <Link
                    to="/"
                    onClick={closeMenus}
                    aria-label="Arsa Durağı ana sayfa"
                    className="group flex h-[72px] w-[148px] shrink-0 items-center overflow-hidden"
                >
                    <img
                        className="h-[68px] w-[132px] object-contain transition-transform duration-300 group-hover:scale-[1.03]"
                        src={logo}
                        alt="Arsa Durağı"
                    />
                </Link>

                {/* Desktop naviqasiya */}
                <div className="hidden items-center gap-1 lg:flex">
                    {navigation.map((item) => {
                        const Icon = item.icon;

                        return (
                            <Link
                                key={item.href}
                                to={item.href}
                                className="group relative inline-flex items-center gap-2 rounded-xl px-3 py-3 text-[13px] font-medium text-white/65 transition duration-200 hover:bg-white/[0.05] hover:text-white xl:px-4"
                            >
                                <Icon
                                    size={16}
                                    strokeWidth={1.7}
                                    className="text-white/40 transition group-hover:text-[#d7b783]"
                                />
                                {item.label}
                            </Link>
                        );
                    })}
                </div>

                {/* Sağ tərəf */}
                <div className="flex shrink-0 items-center gap-2 sm:gap-3">
                    {/* Dil seçimi */}
                    <DropdownSelect
                        className="w-[70px]"
                        variant="navbar"
                        ariaLabel="Dil seç"
                        value={language}
                        onChange={setLanguage}
                        options={languages}
                    />

                    {/* Desktop giriş / qeydiyyat */}
                    <div className="hidden items-center gap-2 sm:flex">
                        <a
                            href="/login"
                            onClick={closeMenus}
                            className="inline-flex h-10 items-center gap-2 rounded-xl px-3 text-[13px] font-medium text-white/70 transition hover:bg-white/[0.06] hover:text-white xl:px-4"
                        >
                            <LogIn size={16} />
                            <span>Giriş</span>
                        </a>

                        <a
                            href="/register"
                            onClick={closeMenus}
                            className="group inline-flex h-10 items-center justify-center gap-2 rounded-xl bg-[#b99561] px-3 text-[12px] font-semibold text-[#171713] shadow-lg shadow-[#b99561]/10 transition hover:bg-[#d0b184] sm:px-4 xl:px-5"
                        >
                            <UserPlus size={15} />
                            <span>Kayıt Ol</span>
                            <ArrowRight
                                size={14}
                                className="hidden transition-transform group-hover:translate-x-0.5 xl:block"
                            />
                        </a>
                    </div>

                    {/* Mobil menyu düyməsi */}
                    <button
                        type="button"
                        onClick={() => setMenuOpen((open) => !open)}
                        aria-label={menuOpen ? "Menüyü kapat" : "Menüyü aç"}
                        aria-expanded={menuOpen}
                        aria-controls="mobile-navigation"
                        className="grid size-10 shrink-0 place-items-center rounded-xl border border-white/10 bg-white/[0.03] transition hover:border-white/20 hover:bg-white/[0.08] lg:hidden"
                    >
                        {menuOpen ? (
                            <X size={21} />
                        ) : (
                            <Menu size={21} />
                        )}
                    </button>
                </div>
            </nav>

            {/* Mobil menyu */}
            <div
                id="mobile-navigation"
                aria-hidden={!menuOpen}
                className={`absolute left-0 right-0 top-full border-b border-white/10 bg-[#191b17] shadow-2xl shadow-black/25 transition-all duration-300 ease-out lg:hidden ${menuOpen
                        ? "visible translate-y-0 opacity-100"
                        : "invisible -translate-y-2 pointer-events-none opacity-0"
                    }`}
            >
                <div className="mx-auto max-h-[calc(100dvh-76px)] max-w-7xl overflow-y-auto px-4 pb-6 pt-4 sm:px-6">
                    <p className="mb-3 px-3 text-[10px] font-semibold uppercase tracking-[0.2em] text-white/35">
                        Keşfet
                    </p>

                    <div className="space-y-1">
                        {navigation.map((item) => {
                            const Icon = item.icon;

                            return (
                                <Link
                                    key={item.href}
                                    to={item.href}
                                    onClick={closeMenus}
                                    tabIndex={menuOpen ? 0 : -1}
                                    className="group flex items-center justify-between rounded-xl px-3 py-3.5 text-sm font-medium text-white/75 transition hover:bg-white/[0.06] hover:text-white"
                                >
                                    <span className="flex items-center gap-3">
                                        <span className="grid size-9 place-items-center rounded-xl bg-white/[0.05] text-[#c8a779] transition group-hover:bg-[#b99561]/15">
                                            <Icon size={18} />
                                        </span>
                                        {item.label}
                                    </span>

                                    <ArrowRight
                                        size={16}
                                        className="text-white/25 transition group-hover:translate-x-1 group-hover:text-[#d7b783]"
                                    />
                                </Link>
                            );
                        })}
                    </div>

                    <div className="my-5 border-t border-white/[0.08]" />

                    {/* Mobil giriş / qeydiyyat */}
                    <div className="grid grid-cols-2 gap-3">
                        <a
                            href="/login"
                            onClick={closeMenus}
                            tabIndex={menuOpen ? 0 : -1}
                            className="flex min-h-12 items-center justify-center gap-2 rounded-xl border border-white/10 px-3 text-sm font-medium text-white/80 transition hover:bg-white/[0.05]"
                        >
                            <LogIn size={17} />
                            Giriş Yap
                        </a>

                        <a
                            href="/register"
                            onClick={closeMenus}
                            tabIndex={menuOpen ? 0 : -1}
                            className="flex min-h-12 items-center justify-center gap-2 rounded-xl bg-[#b99561] px-3 text-sm font-semibold text-[#171713] transition hover:bg-[#d0b184]"
                        >
                            <UserPlus size={17} />
                            Kayıt Ol
                        </a>
                    </div>

                </div>
            </div>
        </header>
    );
}

export default Navbar;
