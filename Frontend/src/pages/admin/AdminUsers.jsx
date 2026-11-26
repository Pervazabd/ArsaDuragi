import { useEffect, useState } from "react";
import { Search, Lock, Unlock, ShieldCheck, UserRound, RefreshCw } from "lucide-react";
import { apiRequest } from "../../services/api";

export default function AdminUsers() {
    const [users, setUsers] = useState([]);
    const [search, setSearch] = useState("");
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const load = async () => {
        setLoading(true);
        setError("");
        try {
            const params = new URLSearchParams({ page: "1", pageSize: "100" });
            if (search) params.set("search", search);
            const page = await apiRequest(`/admin/users?${params}`);
            setUsers(page?.items ?? []);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { load(); }, []);

    const setLocked = async (user, lock) => {
        if (lock && !window.confirm(`${user.fullName || user.email} adlı kullanıcıyı engellemek istiyor musunuz?`)) return;
        try {
            await apiRequest(`/admin/users/${encodeURIComponent(user.id)}/${lock ? "lock" : "unlock"}`, { method: "PUT" });
            await load();
        } catch (err) {
            setError(err.message);
        }
    };

    return (
        <div className="mx-auto max-w-[1400px]">
            <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
                <div>
                    <p className="text-xs font-bold uppercase tracking-[0.18em] text-[#9a794f]">Yönetim</p>
                    <h1 className="mt-2 text-3xl font-bold">Kullanıcılar</h1>
                </div>
                <button type="button" onClick={load} className="inline-flex items-center gap-2 rounded-xl border border-[#dde0d5] bg-white px-4 py-3 text-sm font-semibold">
                    <RefreshCw size={16} /> Yenile
                </button>
            </div>

            {error && <div className="mb-4 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>}

            <div className="mb-5 flex rounded-2xl border border-[#e2e4dc] bg-white p-3">
                <label className="flex h-11 min-w-0 flex-1 items-center gap-2 rounded-xl border border-[#e7e8e1] px-3">
                    <Search size={17} className="text-[#9a794f]" />
                    <input value={search} onChange={(event) => setSearch(event.target.value)} onKeyDown={(event) => event.key === "Enter" && load()} placeholder="Ad, e-posta veya telefon ile ara..." className="min-w-0 flex-1 bg-transparent text-sm outline-none" />
                </label>
                <button type="button" onClick={load} className="ml-3 h-11 rounded-xl bg-[#252920] px-4 text-sm font-semibold text-white">Ara</button>
            </div>

            <div className="overflow-hidden rounded-2xl border border-[#e2e4dc] bg-white">
                <div className="overflow-x-auto">
                    <table className="w-full min-w-[760px] text-left text-sm">
                        <thead className="border-b border-[#eceee7] bg-[#fafbf8] text-xs uppercase tracking-wide text-[#7b8074]">
                            <tr><th className="px-5 py-4">Kullanıcı</th><th className="px-5 py-4">İletişim</th><th className="px-5 py-4">Rol</th><th className="px-5 py-4">Durum</th><th className="px-5 py-4 text-right">İşlem</th></tr>
                        </thead>
                        <tbody className="divide-y divide-[#f0f1ec]">
                            {users.map((user) => (
                                <tr key={user.id} className="hover:bg-[#fafbf8]">
                                    <td className="px-5 py-4">
                                        <div className="flex items-center gap-3">
                                            <span className="grid size-10 place-items-center rounded-xl bg-[#eee8dc] text-[#806844]"><UserRound size={18} /></span>
                                            <div><p className="font-semibold">{user.fullName || "Adı Yok"}</p><p className="mt-0.5 text-xs text-slate-500">ID: {user.id.slice(0, 8)}...</p></div>
                                        </div>
                                    </td>
                                    <td className="px-5 py-4"><p>{user.email}</p><p className="mt-1 text-xs text-slate-500">{user.phoneNumber || "Telefon yok"}</p></td>
                                    <td className="px-5 py-4">
                                        <div className="flex flex-wrap gap-1">{user.roles?.map((role) => <span key={role} className="inline-flex items-center gap-1 rounded-full bg-slate-100 px-2.5 py-1 text-xs"><ShieldCheck size={12} /> {role}</span>)}</div>
                                    </td>
                                    <td className="px-5 py-4"><span className={`rounded-full px-3 py-1 text-xs font-semibold ${user.isLockedOut ? "bg-red-100 text-red-700" : "bg-emerald-100 text-emerald-700"}`}>{user.isLockedOut ? "Engellenmiş" : "Aktif"}</span></td>
                                    <td className="px-5 py-4 text-right">
                                        {user.isLockedOut ? (
                                            <button type="button" onClick={() => setLocked(user, false)} className="inline-flex items-center gap-2 rounded-lg border border-emerald-200 px-3 py-2 text-xs font-semibold text-emerald-700 hover:bg-emerald-50"><Unlock size={15} /> Engel Kaldır</button>
                                        ) : (
                                            <button type="button" onClick={() => setLocked(user, true)} className="inline-flex items-center gap-2 rounded-lg border border-red-200 px-3 py-2 text-xs font-semibold text-red-700 hover:bg-red-50"><Lock size={15} /> Engelle</button>
                                        )}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    {loading && <p className="py-14 text-center text-sm text-slate-500">Yükleniyor...</p>}
                    {!loading && !users.length && <p className="py-14 text-center text-sm text-slate-500">Kullanıcı bulunamadı.</p>}
                </div>
            </div>
        </div>
    );
}
