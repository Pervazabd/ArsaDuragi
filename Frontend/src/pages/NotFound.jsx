import { useI18n } from "../i18n/I18nContext.jsx";

export default function NotFound() {
    const { t } = useI18n();
    return (
        <main className="grid min-h-[70vh] place-items-center px-6 text-center">
            <div>
                <div
                    aria-hidden="true"
                    className="flex items-center justify-center gap-2 text-7xl font-bold text-emerald-800"
                >
                    <span>4</span>
                    <span className="motion-safe:animate-bounce">
                        <span className="grid size-10 -rotate-45 place-items-center rounded-full rounded-bl-none bg-emerald-700">
                            <span className="size-3 rounded-full bg-white" />
                        </span>
                    </span>
                    <span>4</span>
                </div>

                <h1 className="mt-4 text-2xl font-semibold">Sayfa bulunamadı</h1>
                <p className="mt-2 text-slate-600">
                    Aradığınız ilan yayından kalkmış olabilir.
                </p>

                <a
                    href="/"
                    className="mt-6 inline-block rounded-lg bg-emerald-700 px-5 py-2.5 font-medium text-white hover:bg-emerald-800"
                >
                    Ana sayfaya dön
                </a>
            </div>
        </main>
    )
}