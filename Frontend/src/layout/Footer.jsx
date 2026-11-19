import { Link } from "react-router-dom";

const currentYear = new Date().getFullYear();

function Footer() {
    return (
        <footer className="bg-coal-dark text-white" id="contact">
            <div className="mx-auto grid max-w-7xl gap-10 px-5 py-12 sm:px-8 sm:py-16 md:grid-cols-[1.4fr_0.7fr_0.9fr] lg:px-12">
                <div className="max-w-sm">
                    <a
                        className="group inline-flex items-center gap-3"
                        href="#home"
                        aria-label="Arsa Durağı ana sayfa"
                    >
                        <span className="grid size-10 place-items-center rounded-full bg-[#b49a77] text-lg font-semibold text-coal-dark transition-transform group-hover:rotate-[-8deg]">
                            A
                        </span>
                        <span className="flex flex-col text-[13px] leading-[1.05] font-bold tracking-[0.17em]">
                            ARSA
                            <span className="mt-1 text-[10px] font-medium tracking-[0.34em] text-white/60">
                                DURAĞI
                            </span>
                        </span>
                    </a>
                    <p className="mt-5 max-w-xs text-sm leading-6 text-white/60">
                        Bakü ve çevresinde gayrimenkul arayışınızın güvenilir
                        adresi.
                    </p>
                    <p className="mt-4 text-sm font-medium tracking-wide text-[#c8a779]">
                        Doğru arsa, doğru yatırım.
                    </p>
                </div>

                <div className="flex flex-col items-start gap-3 text-sm text-white/65">
                    <strong className="mb-1 text-xs font-semibold tracking-[0.16em] text-white uppercase">
                        Keşfedin
                    </strong>
                    <Link className="transition hover:text-white" to="/">
                        Ana sayfa
                    </Link>
                    <Link
                        className="transition hover:text-white"
                        to="/properties"
                    >
                        İlanlar
                    </Link>
                </div>

                <div className="flex flex-col items-start gap-3 text-sm text-white/65">
                    <strong className="mb-1 text-xs font-semibold tracking-[0.16em] text-white uppercase">
                        İletişim
                    </strong>
                    <span>Bakü, Azerbaycan</span>
                    <a
                        className="transition hover:text-white"
                        href="tel:+994501234567"
                    >
                        +994 50 123 45 67
                    </a>
                    <a
                        className="transition hover:text-white"
                        href="mailto:info@arsaduragi.az"
                    >
                        info@arsaduragi.az
                    </a>
                </div>
            </div>

            <div className="border-t border-white/12">
                <div className="mx-auto flex max-w-7xl flex-col gap-2 px-5 py-5 text-xs text-white/45 sm:flex-row sm:items-center sm:justify-between sm:px-8 lg:px-12">
                    <span>© {currentYear} Arsa Durağı</span>
                    <a className="transition hover:text-white/80" href="#home">
                        Başa dön ↑
                    </a>
                </div>
            </div>
        </footer>
    );
}

export default Footer;
