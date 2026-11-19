
import {
    ArrowDown,
    ArrowRight,
    ArrowUpRight,
    CheckCircle2,
    MapPin,
    MoveUpRight,
    ShieldCheck,
} from "lucide-react";

function Hero() {
    return (
        <section
            id="home"
            className="relative isolate min-h-[650px] overflow-hidden bg-[#171815] text-white sm:min-h-[720px] lg:min-h-[760px]"
        >
            {/* Background image */}
            <img
                src="https://images.unsplash.com/photo-1500382017468-9049fed747ef?auto=format&fit=crop&w=2400&q=90"
                alt=""
                fetchPriority="high"
                className="absolute inset-0 -z-30 size-full object-cover object-center"
            />

            {/* Premium overlays */}
            <div className="absolute inset-0 -z-20 bg-[#171815]/45" />

            <div className="absolute inset-0 -z-10 bg-gradient-to-r from-[#171815]/95 via-[#171815]/80 to-[#171815]/25" />

            <div className="absolute inset-0 -z-10 bg-gradient-to-t from-[#171815]/80 via-transparent to-[#171815]/20" />

            {/* Decorative elements */}
            <div className="pointer-events-none absolute right-[-100px] top-20 -z-10 size-[320px] rounded-full border border-white/[0.08] sm:right-[-140px] sm:size-[460px] lg:right-[-100px] lg:size-[600px]" />

            <div className="pointer-events-none absolute right-[-50px] top-36 -z-10 size-[240px] rounded-full border border-white/[0.06] sm:right-[-70px] sm:size-[360px] lg:right-[-30px] lg:size-[460px]" />

            <div className="mx-auto flex min-h-[650px] max-w-7xl flex-col justify-center px-5 pb-28 pt-20 sm:min-h-[720px] sm:px-8 sm:pb-32 lg:min-h-[760px] lg:px-12">
                <div className="max-w-4xl">
                    {/* Main heading */}
                    <h1 className="max-w-4xl text-[clamp(2.8rem,7.5vw,6.5rem)] leading-[1.04] font-medium tracking-[-0.055em] text-white">
                        Geleceğinizi
                        <br />
                        <span className="font-serif font-normal italic text-[#d2b184]">
                            doğru toprakta
                        </span>
                        <br />
                        şekillendirin.
                    </h1>

                    {/* Description */}
                    <p className="mt-6 max-w-xl text-sm leading-7 text-white/70 sm:mt-8 sm:text-base sm:leading-8 lg:text-lg">
                        Hayalinizdeki projeye uygun arsa ve arazileri keşfedin.
                        Doğru lokasyonu bulun, yatırımınıza sağlam bir
                        başlangıç yapın.
                    </p>

                    {/* CTA buttons */}
                    <div className="mt-8 flex flex-col gap-3 min-[420px]:flex-row min-[420px]:items-center sm:mt-10">
                        <a
                            href="#properties"
                            className="group inline-flex min-h-[54px] items-center justify-center gap-4 rounded-xl bg-[#c5a475] px-6 py-4 text-sm font-semibold text-[#191914] shadow-lg shadow-black/10 transition duration-300 hover:-translate-y-0.5 hover:bg-[#d7b98d] sm:px-7"
                        >
                            Arsaları Keşfedin
                            <ArrowUpRight
                                size={18}
                                className="transition-transform duration-300 group-hover:translate-x-0.5 group-hover:-translate-y-0.5"
                            />
                        </a>

                        <a
                            href="#about"
                            className="group inline-flex min-h-[54px] items-center justify-center gap-3 rounded-xl border border-white/20 bg-white/[0.04] px-6 py-4 text-sm font-medium text-white/90 backdrop-blur-sm transition duration-300 hover:border-white/35 hover:bg-white/[0.09] sm:px-7"
                        >
                            Bizi Tanıyın
                            <ArrowRight
                                size={17}
                                className="text-[#d2b184] transition-transform duration-300 group-hover:translate-x-1"
                            />
                        </a>
                    </div>

                    {/* Trust indicators */}
                    <div className="mt-10 flex flex-wrap gap-x-6 gap-y-3 sm:mt-12 sm:gap-x-8">
                        <div className="flex items-center gap-2 text-xs text-white/65 sm:text-sm">
                            <ShieldCheck
                                size={17}
                                className="shrink-0 text-[#d2b184]"
                            />
                            Güvenilir ilanlar
                        </div>

                        <div className="flex items-center gap-2 text-xs text-white/65 sm:text-sm">
                            <MapPin
                                size={17}
                                className="shrink-0 text-[#d2b184]"
                            />
                            Doğru lokasyon
                        </div>

                        <div className="flex items-center gap-2 text-xs text-white/65 sm:text-sm">
                            <CheckCircle2
                                size={17}
                                className="shrink-0 text-[#d2b184]"
                            />
                            Yatırıma uygun arsalar
                        </div>
                    </div>
                </div>

                {/* Floating location card */}
                <div className="absolute bottom-6 right-5 left-5 sm:bottom-8 sm:right-8 sm:left-auto lg:bottom-10 lg:right-12">
                    <div className="ml-auto flex max-w-sm items-center gap-4 rounded-2xl border border-white/15 bg-[#191b17]/75 p-4 shadow-2xl shadow-black/20 backdrop-blur-xl sm:p-5">
                        <div className="grid size-12 shrink-0 place-items-center rounded-xl border border-[#c5a475]/20 bg-[#c5a475]/10 text-[#d2b184]">
                            <MapPin size={23} strokeWidth={1.6} />
                        </div>

                        <div className="min-w-0 flex-1">
                            <p className="text-[10px] font-medium tracking-[0.17em] text-white/45 uppercase">
                                Doğru yerden başlayın
                            </p>

                            <p className="mt-1 text-sm font-semibold text-white sm:text-base">
                                Size uygun araziyi bulun
                            </p>

                            <p className="mt-1 text-xs leading-5 text-white/55">
                                Seçenekleri keşfedin, geleceğinizi planlayın.
                            </p>
                        </div>

                        <a
                            href="#properties"
                            aria-label="Arsaları keşfet"
                            className="grid size-10 shrink-0 place-items-center rounded-full border border-white/15 text-white/80 transition hover:border-[#c5a475] hover:bg-[#c5a475] hover:text-[#191914]"
                        >
                            <MoveUpRight size={18} />
                        </a>
                    </div>
                </div>

                {/* Scroll indicator */}
                <a
                    href="#properties"
                    className="absolute bottom-10 left-8 hidden items-center gap-3 text-[10px] font-medium tracking-[0.2em] text-white/45 uppercase transition hover:text-[#d2b184] lg:flex lg:left-12"
                >
                    <span className="grid size-9 place-items-center rounded-full border border-white/15">
                        <ArrowDown size={15} />
                    </span>
                    Aşağı kaydırın
                </a>
            </div>
        </section>
    );
}

export default Hero;
