import { useState } from "react";
import { motion, useReducedMotion } from "framer-motion";

import {

    Phone,

    Mail,

    MapPin,

    Clock3,

    MessageCircle,

    ArrowUpRight,

    ChevronDown,

    CheckCircle2,

} from "lucide-react";
import { useI18n } from "../i18n/I18nContext.jsx";

const CONFIG = {

    whatsapp: "905308163456",

    phone: "+90 530 816 34 56",

    email: "info@arsaduragi.com",

    address: "Bilecik, Türkiye",

    workingHours: "Pzt - Cmt / 09:00 - 18:00",

};

const whatsappUrl = (message) =>

    `https://wa.me/${CONFIG.whatsapp}?text=${encodeURIComponent(message)}`;

const initialForm = {

    name: "",

    phone: "",

    email: "",

    subject: "",

    message: "",

};

const inputClass =

    "w-full rounded-xl border border-[#D8D5CE] bg-white px-4 py-3.5 text-sm text-[#242722] outline-none transition focus:border-[#B49367] focus:ring-2 focus:ring-[#B49367]/15 placeholder:text-[#999A93]";

function ContactForm() {
    const { t } = useI18n();

    const [form, setForm] = useState(initialForm);

    const [errors, setErrors] = useState({});

    const [submitted, setSubmitted] = useState(false);

    const update = (field, value) => {

        setForm((prev) => ({ ...prev, [field]: value }));

        setErrors((prev) => ({ ...prev, [field]: "" }));

    };

    const handleSubmit = (e) => {

        e.preventDefault();

        const nextErrors = {};

        if (!form.name.trim()) {

            nextErrors.name = t("nameRequired");

        }

        if (!/^[+\d\s()-]{7,20}$/.test(form.phone.trim())) {

            nextErrors.phone = t("phoneRequired");

        }

        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email.trim())) {

            nextErrors.email = t("emailRequired");

        }

        if (!form.subject) {

            nextErrors.subject = t("subjectRequired");

        }

        if (!form.message.trim()) {

            nextErrors.message = t("messageRequired");

        }

        if (Object.keys(nextErrors).length) {

            setErrors(nextErrors);

            return;

        }

        const message = [

            "Merhaba Arsa Durağı,",

            "",

            `Ad Soyad: ${form.name}`,

            `Telefon: ${form.phone}`,

            `E-posta: ${form.email}`,

            `Konu: ${form.subject}`,

            "",

            `Mesaj: ${form.message}`,

        ].join("\n");

        const url = whatsappUrl(message);

        const opened = window.open(url, "_blank");

        if (opened) {

            opened.opener = null;

            setSubmitted(true);

        } else {

            window.location.href = url;

        }

    };

    if (submitted) {

        return (

            <div className="flex min-h-[360px] flex-col items-center justify-center text-center">

                <div className="mb-5 flex h-16 w-16 items-center justify-center rounded-full bg-[#C5A475]/15">

                    <CheckCircle2 size={32} className="text-[#A88656]" />

                </div>

                <h3 className="text-2xl font-semibold text-[#242722]">

                    {t("whatsappReady")}

                </h3>

                <p className="mt-3 max-w-sm text-sm leading-7 text-[#777970]">

                    {t("whatsappOpened")}

                </p>

                <button

                    type="button"

                    onClick={() => {

                        setSubmitted(false);

                        setForm(initialForm);

                    }}

                    className="mt-6 text-sm font-semibold text-[#A38155] underline underline-offset-4"

                >

                    {t("newMessage")}

                </button>

            </div>

        );

    }

    return (

        <form onSubmit={handleSubmit} noValidate className="space-y-4">

            <div className="grid gap-4 sm:grid-cols-2">

                <div>

                    <label htmlFor="contact-name" className="mb-2 block text-xs font-semibold">

                        {t("fullName")} *

                    </label>

                    <input

                        id="contact-name"

                        autoComplete="name"

                        className={inputClass}

                        placeholder="Adınız Soyadınız"

                        value={form.name}

                        onChange={(e) => update("name", e.target.value)}

                    />

                    {errors.name && (

                        <p className="mt-1 text-xs text-red-600">{errors.name}</p>

                    )}

                </div>

                <div>

                    <label htmlFor="contact-phone" className="mb-2 block text-xs font-semibold">

                        {t("phoneLabel")} *

                    </label>

                    <input

                        id="contact-phone"

                        type="tel"

                        autoComplete="tel"

                        className={inputClass}

                        placeholder="+90 5XX XXX XX XX"

                        value={form.phone}

                        onChange={(e) => update("phone", e.target.value)}

                    />

                    {errors.phone && (

                        <p className="mt-1 text-xs text-red-600">{errors.phone}</p>

                    )}

                </div>

            </div>

            <div>

                <label htmlFor="contact-email" className="mb-2 block text-xs font-semibold">

                    {t("emailLabel")} *

                </label>

                <input

                    id="contact-email"

                    type="email"

                    autoComplete="email"

                    className={inputClass}

                    placeholder="ornek@email.com"

                    value={form.email}

                    onChange={(e) => update("email", e.target.value)}

                />

                {errors.email && (

                    <p className="mt-1 text-xs text-red-600">{errors.email}</p>

                )}

            </div>

            <div>

                <label htmlFor="contact-subject" className="mb-2 block text-xs font-semibold">

                    {t("subject")} *

                </label>

                <div className="relative">

                    <select

                        id="contact-subject"

                        className={`${inputClass} appearance-none`}

                        value={form.subject}

                        onChange={(e) => update("subject", e.target.value)}

                    >

                        <option value="">{t("selectSubject")}</option>
                        <option value={t("buyLand")}>{t("buyLand")}</option>
                        <option value={t("sellLand")}>{t("sellLand")}</option>
                        <option value={t("investmentAdvice")}>{t("investmentAdvice")}</option>
                        <option value={t("listingInfo")}>{t("listingInfo")}</option>
                        <option value={t("other")}>{t("other")}</option>

                    </select>

                    <ChevronDown

                        size={16}

                        className="pointer-events-none absolute right-4 top-1/2 -translate-y-1/2 text-[#777970]"

                    />

                </div>

                {errors.subject && (

                    <p className="mt-1 text-xs text-red-600">{errors.subject}</p>

                )}

            </div>

            <div>

                <label htmlFor="contact-message" className="mb-2 block text-xs font-semibold">

                    {t("message")} *

                </label>

                <textarea

                    id="contact-message"

                    rows={4}

                    className={`${inputClass} resize-none`}

                    placeholder="Size nasıl yardımcı olabiliriz?"

                    value={form.message}

                    onChange={(e) => update("message", e.target.value)}

                />

                {errors.message && (

                    <p className="mt-1 text-xs text-red-600">{errors.message}</p>

                )}

            </div>

            <button

                type="submit"

                className="flex w-full items-center justify-center gap-2 rounded-xl bg-[#B99768] px-5 py-4 text-sm font-semibold text-[#1D211D] transition hover:bg-[#D0AE7C]"

            >

                <MessageCircle size={18} />

                {t("sendWhatsApp")}

                <ArrowUpRight size={17} />

            </button>

            <p className="text-center text-[11px] leading-5 text-[#8B8C84]">

                WhatsApp açılır. Mesaj yalnızca WhatsApp üzerinden

                gönderdiğinizde iletilir.

            </p>

        </form>

    );

}

// Each item has its own entrance timing, so they arrive one after another.
const slideEase = [0.22, 1, 0.36, 1];
const CARD_DURATION = 0.62;
const CARD_GAP = 0.12;
const CARD_START = 0.65;

function ContactItem({ icon: Icon, label, value, href, order, reduceMotion }) {
    return (
        <motion.a
            initial={reduceMotion ? false : { opacity: 0, x: -130 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{
                duration: reduceMotion ? 0 : CARD_DURATION,
                delay: reduceMotion ? 0 : CARD_START + order * (CARD_DURATION + CARD_GAP),
                ease: slideEase,
            }}
            whileHover={reduceMotion ? undefined : { x: 5 }}
            href={href}
            target={href.startsWith("http") ? "_blank" : undefined}
            rel={href.startsWith("http") ? "noopener noreferrer" : undefined}
            className="group flex items-center gap-4 rounded-xl border border-white/10 bg-white/[0.035] p-4 transition-colors hover:border-[#C5A475]/40 hover:bg-white/[0.065]"
        >
            <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-lg bg-[#C5A475]/10 text-[#C9A879]">
                <Icon size={20} strokeWidth={1.7} />
            </div>
            <div className="min-w-0 flex-1">
                <p className="mb-1 text-[10px] font-semibold uppercase tracking-[0.15em] text-white/40">{label}</p>
                <p className="break-words text-sm font-medium text-white/90">{value}</p>
            </div>
            <ArrowUpRight size={17} className="shrink-0 text-white/30 transition group-hover:text-[#C5A475]" />
        </motion.a>
    );
}

export default function Contact() {
    const { t } = useI18n();
    const reduceMotion = useReducedMotion();
    const lastCardEnd = CARD_START + 5 * (CARD_DURATION + CARD_GAP);
    return (
        <main className="min-h-screen overflow-x-hidden bg-[#191D1A] font-sans text-white">
            <section className="mx-auto max-w-7xl px-5 py-12 sm:px-8 lg:px-12 lg:py-16">
                <motion.div
                    initial={reduceMotion ? false : { opacity: 0, y: 30 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: reduceMotion ? 0 : 0.5, ease: slideEase }}
                    className="mb-9"
                >
                    <span className="text-[11px] font-semibold uppercase tracking-[0.2em] text-[#C5A475]">ARSA DURAĞI</span>
                    <h1 className="mt-3 text-3xl font-medium tracking-tight sm:text-4xl">                    {t("contactTitle")}</h1>
                    <p className="mt-3 max-w-xl text-sm leading-7 text-white/50">
                        {t("contactDescription")}
                    </p>
                </motion.div>

                <div className="grid items-start gap-7 lg:grid-cols-[0.85fr_1.15fr] lg:gap-10">
                    <div>
                        <motion.h2
                            initial={reduceMotion ? false : { opacity: 0, x: -60 }}
                            animate={{ opacity: 1, x: 0 }}
                            transition={{ duration: reduceMotion ? 0 : 0.5, delay: reduceMotion ? 0 : 0.15, ease: slideEase }}
                            className="mb-5 text-lg font-medium"
                        >
                            {t("contactInfo")}
                        </motion.h2>
                        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-1">
                            <ContactItem order={0} reduceMotion={reduceMotion} icon={Phone}                             label={t("phone").toUpperCase()} value={CONFIG.phone} href={`tel:${CONFIG.phone.replace(/[^\d+]/g, "")}`} />
                            <ContactItem order={1} reduceMotion={reduceMotion} icon={MessageCircle} label="WHATSAPP" value="WhatsApp'tan yazın" href={whatsappUrl("Merhaba, bilgi almak istiyorum.")} />
                            <ContactItem order={2} reduceMotion={reduceMotion} icon={Mail}                             label={t("email").toUpperCase()} value={CONFIG.email} href={`mailto:${CONFIG.email}`} />
                            <ContactItem order={3} reduceMotion={reduceMotion} icon={MapPin}                             label={t("location").toUpperCase()} value={CONFIG.address} href={`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(CONFIG.address)}`} />
                        </div>
                        <motion.div
                            initial={reduceMotion ? false : { opacity: 0, x: -130 }}
                            animate={{ opacity: 1, x: 0 }}
                            transition={{ duration: reduceMotion ? 0 : CARD_DURATION, delay: reduceMotion ? 0 : CARD_START + 4 * (CARD_DURATION + CARD_GAP), ease: slideEase }}
                            className="mt-5 flex items-center gap-3 rounded-xl border border-white/10 px-4 py-4"
                        >
                            <Clock3 size={19} className="text-[#C5A475]" />
                            <div>
                                <p className="text-[10px] uppercase tracking-widest text-white/40">                                {t("workingHours")}</p>
                                <p className="mt-1 text-sm text-white/75">{CONFIG.workingHours}</p>
                            </div>
                        </motion.div>
                    </div>

                    <motion.div
                        initial={reduceMotion ? false : { opacity: 0, x: 110 }}
                        animate={{ opacity: 1, x: 0 }}
                        transition={{ duration: reduceMotion ? 0 : 0.8, delay: reduceMotion ? 0 : lastCardEnd, ease: slideEase }}
                        className="rounded-2xl bg-[#F4F2ED] p-6 text-[#242722] shadow-xl sm:p-8 lg:p-9"
                    >
                        <h2 className="text-2xl font-medium tracking-tight">                        {t("sendMessage")}</h2>
                        <p className="mb-7 mt-2 text-sm leading-6 text-[#777970]">{t("formDescription")}</p>
                        <ContactForm />
                    </motion.div>
                </div>
            </section>
        </main>
    );
}
