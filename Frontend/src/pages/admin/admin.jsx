import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { Building2, Clock3, CheckCircle2, Users, MapPin, ArrowRight } from "lucide-react";
import { apiRequest } from "../../services/api";

export default function AdminHome() {
    const [properties, setProperties] = useState([]);
    const [users, setUsers] = useState([]);
    const [propertyTotal, setPropertyTotal] = useState(0);
    const [userTotal, setUserTotal] = useState(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        Promise.all([
            apiRequest("/admin/properties?pageSize=50"),
            apiRequest("/admin/users?pageSize=100"),
        ])
            .then(([propertyPage, userPage]) => {
                setProperties(propertyPage?.items ?? []);
                setUsers(userPage?.items ?? []);
                setPropertyTotal(propertyPage?.totalCount ?? propertyPage?.items?.length ?? 0);
                setUserTotal(userPage?.totalCount ?? userPage?.items?.length ?? 0);
            })
            .catch((err) => setError(err.message))
            .finally(() => setLoading(false));
    }, []);

    const pending = properties.filter((item) => item.status === "Pending").length;
    const active = properties.filter((item) => item.status === "Active").length;
    const statusLabels = {
        Pending: "Onay Bekliyor",
        Active: "Aktif",
        Rejected: "Reddedildi",
        Sold: "Satıldı",
        Archived: "Arşivde",
    };
    const stats = [
        { label: "Toplam İlan", value: propertyTotal, icon: Building2, color: "bg-[#ebe4d7] text-[#806844]" },
        { label: "Aktif İlan", value: active, icon: CheckCircle2, color: "bg-emerald-100 text-emerald-700" },
        { label: "Onay Bekleyen", value: pending, icon: Clock3, color: "bg-amber-100 text-amber-700" },
        { label: "Kullanıcı", value: userTotal, icon: Users, color: "bg-blue-100 text-blue-700" },
    ];

    return (
        <div className="mx-auto max-w-[1400px]">
            <div className="mb-7 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
                <div>
                    <p className="text-xs font-bold uppercase tracking-[0.18em] text-[#9a794f]">Yönetim Paneli</p>
                    <h1 className="mt-2 text-3xl font-bold tracking-tight sm:text-4xl">Genel Bakış</h1>
                </div>
                <Link to="/admin/elanlar" className="inline-flex items-center gap-2 rounded-xl bg-[#252920] px-4 py-3 text-sm font-semibold text-white">
                    İlanları Görüntüle <ArrowRight size={16} />
                </Link>
            </div>

            {loading ? (
                <div className="grid min-h-52 place-items-center rounded-2xl border border-[#e2e4dc] bg-white text-sm text-slate-500">Veriler yükleniyor...</div>
            ) : error ? (
                <div className="rounded-2xl border border-red-200 bg-red-50 px-5 py-8 text-sm text-red-700">{error}</div>
            ) : (
                <>
                    <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
                        {stats.map(({ label, value, icon: Icon, color }) => (
                            <div key={label} className="rounded-2xl border border-[#e2e4dc] bg-white p-5 shadow-sm">
                                <div className="flex items-start justify-between">
                                    <div>
                                        <p className="text-sm text-[#777c71]">{label}</p>
                                        <p className="mt-2 text-3xl font-bold tabular-nums">{value}</p>
                                    </div>
                                    <span className={`grid size-11 place-items-center rounded-xl ${color}`}><Icon size={20} /></span>
                                </div>
                            </div>
                        ))}
                    </div>

                    <div className="mt-6 grid gap-6 lg:grid-cols-[1.35fr_0.65fr]">
                        <section className="overflow-hidden rounded-2xl border border-[#e2e4dc] bg-white">
                            <div className="flex items-center justify-between border-b border-[#eceee7] px-5 py-4">
                                <h2 className="font-semibold">Son İlanlar</h2>
                                <Link to="/admin/elanlar" className="text-xs font-semibold text-[#94713f]">Tümünü Gör</Link>
                            </div>
                            <div className="divide-y divide-[#f0f1ec]">
                                {properties.slice(0, 7).map((item) => (
                                    <div key={item.id} className="flex items-center justify-between gap-4 px-5 py-4">
                                        <div className="min-w-0">
                                            <p className="truncate font-medium">{item.title}</p>
                                            <p className="mt-1 flex items-center gap-1 text-xs text-slate-500"><MapPin size={12} /> {item.city} / {item.district}</p>
                                        </div>
                                        <span className="shrink-0 rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold">{statusLabels[item.status] ?? item.status}</span>
                                    </div>
                                ))}
                                {!properties.length && <p className="px-5 py-10 text-center text-sm text-slate-500">İlan bulunmuyor.</p>}
                            </div>
                        </section>

                        <section className="rounded-2xl border border-[#e2e4dc] bg-[#252920] p-5 text-white">
                            <h2 className="font-semibold">Hızlı İşlemler</h2>
                            <div className="mt-5 space-y-3">
                                <Link to="/admin/elanlar" className="flex items-center justify-between rounded-xl border border-white/10 bg-white/[0.04] px-4 py-3 text-sm hover:bg-white/[0.08]">
                                    Yeni İlan Oluştur <Building2 size={17} />
                                </Link>
                                <Link to="/admin/istifadeciler" className="flex items-center justify-between rounded-xl border border-white/10 bg-white/[0.04] px-4 py-3 text-sm hover:bg-white/[0.08]">
                                    Kullanıcıları Yönet <Users size={17} />
                                </Link>
                                <Link to="/admin/yerler" className="flex items-center justify-between rounded-xl border border-white/10 bg-white/[0.04] px-4 py-3 text-sm hover:bg-white/[0.08]">
                                    Şehir ve İlçeler <MapPin size={17} />
                                </Link>
                            </div>
                        </section>
                    </div>
                </>
            )}
        </div>
    );
}
