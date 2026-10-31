import { useState } from "react";
import {
    ArrowLeft,
    ArrowRight,
    Eye,
    EyeOff,
    LockKeyhole,
    Mail,
    MapPin,
    Phone,
    ShieldCheck,
    UserRound,
} from "lucide-react";
import { Link } from "react-router-dom";
import logo from "../assets/arsa-duragi-logo.png";

const fieldClassName =
    "h-12 w-full rounded-xl border border-[#e8e5df] bg-[#fbfaf8] pl-11 pr-4 text-sm text-[#292725] outline-none transition placeholder:text-[#aaa59d] focus:border-[#b99561] focus:ring-4 focus:ring-[#b99561]/10";

function AuthPage({ mode }) {
    const isRegister = mode === "register";
    const [passwordVisible, setPasswordVisible] = useState(false);
    const [message, setMessage] = useState("");

    function handleSubmit(event) {
        event.preventDefault();

        const formData = new FormData(event.currentTarget);
        if (
            isRegister &&
            formData.get("password") !== formData.get("confirmPassword")
        ) {
            setMessage("Şifrələr uyğun gəlmir. Zəhmət olmasa yenidən yoxlayın.");
            return;
        }

        setMessage("Bu demo səhifəsində hesab əməliyyatları hələ aktiv deyil.");
    }

    return (
        <main className="min-h-screen bg-[#f4f2ee] px-4 py-5 sm:px-8 sm:py-7">
            <div className="mx-auto flex max-w-[1120px] items-center justify-between">
                <Link
                    to="/"
                    aria-label="Arsa Durağı ana səhifə"
                    className="inline-flex items-center"
                >
                    <img
                        src={logo}
                        alt="Arsa Durağı"
                        className="h-14 w-[126px] object-contain"
                    />
                </Link>
                <Link
                    to="/"
                    className="inline-flex items-center gap-2 rounded-full px-3 py-2 text-sm font-medium text-[#6d6964] transition hover:bg-white hover:text-[#292725]"
                >
                    <ArrowLeft size={16} />
                    <span className="hidden sm:inline">Ana səhifəyə qayıt</span>
                    <span className="sm:hidden">Ana səhifə</span>
                </Link>
            </div>

            <section className="mx-auto mt-5 grid max-w-[1120px] overflow-hidden rounded-[28px] bg-white shadow-[0_24px_80px_rgba(39,35,29,0.09)] sm:mt-8 lg:min-h-[680px] lg:grid-cols-[0.92fr_1.08fr]">
                <aside className="relative isolate flex min-h-[230px] flex-col justify-end overflow-hidden bg-[#22241f] p-7 text-white sm:min-h-[280px] sm:p-10 lg:min-h-full lg:p-12">
                    <img
                        src="https://images.unsplash.com/photo-1500382017468-9049fed747ef?auto=format&fit=crop&w=1400&q=85"
                        alt=""
                        className="absolute inset-0 -z-20 size-full object-cover"
                    />
                    <div className="absolute inset-0 -z-10 bg-gradient-to-t from-[#171815]/95 via-[#171815]/55 to-[#171815]/20" />
                    <div className="absolute right-8 top-8 hidden size-40 rounded-full border border-white/15 lg:block" />
                    <div className="absolute right-16 top-16 hidden size-24 rounded-full border border-white/10 lg:block" />

                    <div className="mb-5 inline-flex w-fit items-center gap-2 rounded-full border border-white/15 bg-white/10 px-3 py-2 text-[11px] font-medium text-white/85 backdrop-blur-md">
                        <MapPin size={14} className="text-[#d2b184]" />
                        Arsa Durağı ilə doğru yerdəsiniz
                    </div>
                    <h1 className="max-w-md font-display text-4xl leading-[1.05] font-medium sm:text-5xl">
                        {isRegister ? (
                            <>
                                Gələcəyinizə
                                <br />
                                <span className="italic text-[#d2b184]">
                                    doğru yerdən
                                </span>{" "}
                                başlayın.
                            </>
                        ) : (
                            <>
                                Xəyallarınıza
                                <br />
                                <span className="italic text-[#d2b184]">
                                    geri qayıdın.
                                </span>
                            </>
                        )}
                    </h1>
                    <p className="mt-4 max-w-sm text-sm leading-6 text-white/70">
                        Seçdiyiniz əraziləri izləyin, sizə uyğun fürsətləri
                        qaçırmayın.
                    </p>
                    <div className="mt-7 hidden items-center gap-2 text-xs text-white/65 lg:flex">
                        <ShieldCheck size={16} className="text-[#d2b184]" />
                        Etibarlı seçim, rahat başlanğıc
                    </div>
                </aside>

                <div
                    key={mode}
                    className="auth-panel-enter flex items-center justify-center px-6 py-9 sm:px-12 sm:py-12 lg:px-16"
                >
                    <div className="w-full max-w-[410px]">
                        <p className="text-xs font-semibold tracking-[0.19em] text-[#9a794f] uppercase">
                            Arsa Durağı
                        </p>
                        <h2 className="mt-3 font-display text-[38px] leading-tight font-medium text-[#292725] sm:text-[44px]">
                            {isRegister ? "Hesab yaradın" : "Xoş gəlmisiniz"}
                        </h2>
                        <p className="mt-2 text-sm leading-6 text-[#77726b]">
                            {isRegister
                                ? "Elanları izləmək üçün məlumatlarınızı daxil edin."
                                : "Davam etmək üçün hesabınıza daxil olun."}
                        </p>

                        <form
                            className="mt-7 space-y-4"
                            onSubmit={handleSubmit}
                            onChange={() => setMessage("")}
                        >
                            {isRegister && (
                                <label className="block">
                                    <span className="mb-2 block text-xs font-semibold text-[#494640]">
                                        Ad və soyad
                                    </span>
                                    <span className="relative block">
                                        <UserRound
                                            size={17}
                                            className="pointer-events-none absolute top-1/2 left-4 -translate-y-1/2 text-[#a19b92]"
                                        />
                                        <input
                                            className={fieldClassName}
                                            type="text"
                                            name="name"
                                            autoComplete="name"
                                            placeholder="Adınızı və soyadınızı yazın"
                                            required
                                        />
                                    </span>
                                </label>
                            )}

                            <label className="block">
                                <span className="mb-2 block text-xs font-semibold text-[#494640]">
                                    E-poçt ünvanı
                                </span>
                                <span className="relative block">
                                    <Mail
                                        size={17}
                                        className="pointer-events-none absolute top-1/2 left-4 -translate-y-1/2 text-[#a19b92]"
                                    />
                                    <input
                                        className={fieldClassName}
                                        type="email"
                                        name="email"
                                        autoComplete="email"
                                        placeholder="name@example.com"
                                        required
                                    />
                                </span>
                            </label>

                            {isRegister && (
                                <label className="block">
                                    <span className="mb-2 block text-xs font-semibold text-[#494640]">
                                        Telefon nömrəsi
                                        <span className="ml-1 font-normal text-[#9a958c]">
                                            (istəyə bağlı)
                                        </span>
                                    </span>
                                    <span className="relative block">
                                        <Phone
                                            size={17}
                                            className="pointer-events-none absolute top-1/2 left-4 -translate-y-1/2 text-[#a19b92]"
                                        />
                                        <input
                                            className={fieldClassName}
                                            type="tel"
                                            name="phone"
                                            autoComplete="tel"
                                            placeholder="+994 50 123 45 67"
                                        />
                                    </span>
                                </label>
                            )}

                            <label className="block">
                                <span className="mb-2 block text-xs font-semibold text-[#494640]">
                                    Şifrə
                                </span>
                                <span className="relative block">
                                    <LockKeyhole
                                        size={17}
                                        className="pointer-events-none absolute top-1/2 left-4 -translate-y-1/2 text-[#a19b92]"
                                    />
                                    <input
                                        className={`${fieldClassName} pr-12`}
                                        type={passwordVisible ? "text" : "password"}
                                        name="password"
                                        autoComplete={
                                            isRegister
                                                ? "new-password"
                                                : "current-password"
                                        }
                                        placeholder="Ən azı 8 simvol"
                                        minLength={8}
                                        required
                                    />
                                    <button
                                        type="button"
                                        onClick={() =>
                                            setPasswordVisible((visible) => !visible)
                                        }
                                        aria-label={
                                            passwordVisible
                                                ? "Şifrəni gizlət"
                                                : "Şifrəni göstər"
                                        }
                                        className="absolute top-1/2 right-3 grid size-8 -translate-y-1/2 place-items-center rounded-lg text-[#918b82] transition hover:bg-[#f0eee9] hover:text-[#494640]"
                                    >
                                        {passwordVisible ? (
                                            <EyeOff size={17} />
                                        ) : (
                                            <Eye size={17} />
                                        )}
                                    </button>
                                </span>
                            </label>

                            {isRegister && (
                                <label className="block">
                                    <span className="mb-2 block text-xs font-semibold text-[#494640]">
                                        Şifrəni təsdiqləyin
                                    </span>
                                    <span className="relative block">
                                        <LockKeyhole
                                            size={17}
                                            className="pointer-events-none absolute top-1/2 left-4 -translate-y-1/2 text-[#a19b92]"
                                        />
                                        <input
                                            className={fieldClassName}
                                            type={
                                                passwordVisible
                                                    ? "text"
                                                    : "password"
                                            }
                                            name="confirmPassword"
                                            autoComplete="new-password"
                                            placeholder="Şifrənizi yenidən yazın"
                                            minLength={8}
                                            required
                                        />
                                    </span>
                                </label>
                            )}

                            {message && (
                                <p
                                    role="status"
                                    aria-live="polite"
                                    className="rounded-xl bg-[#f8f3eb] px-4 py-3 text-xs leading-5 text-[#775d3c]"
                                >
                                    {message}
                                </p>
                            )}

                            <button
                                type="submit"
                                className="group flex h-12 w-full items-center justify-center gap-2 rounded-xl bg-[#292725] px-5 text-sm font-semibold text-white shadow-lg shadow-[#292725]/10 transition hover:bg-[#3b3834] focus:outline-none focus:ring-4 focus:ring-[#b99561]/20"
                            >
                                {isRegister ? "Hesab yarat" : "Daxil ol"}
                                <ArrowRight
                                    size={17}
                                    className="transition-transform group-hover:translate-x-1"
                                />
                            </button>
                        </form>

                        <p className="mt-6 text-center text-sm text-[#77726b]">
                            {isRegister
                                ? "Artıq hesabınız var?"
                                : "Hesabınız yoxdur?"}{" "}
                            <Link
                                to={isRegister ? "/login" : "/register"}
                                className="font-semibold text-[#8b6a40] underline-offset-4 transition hover:text-[#5f482d] hover:underline"
                            >
                                {isRegister ? "Daxil olun" : "Qeydiyyatdan keçin"}
                            </Link>
                        </p>

                        <p className="mt-8 text-center text-[11px] leading-5 text-[#aaa59d]">
                            Davam etməklə istifadə şərtlərimizi qəbul etmiş
                            olursunuz.
                        </p>
                    </div>
                </div>
            </section>
        </main>
    );
}

export default AuthPage;
