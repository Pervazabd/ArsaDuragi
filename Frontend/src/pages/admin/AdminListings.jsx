import { useEffect, useState } from "react";
import { Building2, Plus, Search, Pencil, Trash2, Star, CheckCircle2, XCircle, X } from "lucide-react";
import { apiRequest } from "../../services/api";

const emptyForm = {
    title: "", description: "", address: "", price: "", area: "",
    propertyType: "Land", zoning: "Residential", terrain: "Flat",
    roadAccess: "RoadFrontage", roadDistanceMeters: "", utilities: [],
    cityId: "", districtId: "",
};

const statuses = { Pending: "Onay Bekliyor", Active: "Aktif", Rejected: "Reddedildi", Sold: "Satıldı", Archived: "Arşivde" };

export default function AdminListings() {
    const [items, setItems] = useState([]);
    const [cities, setCities] = useState([]);
    const [districts, setDistricts] = useState([]);
    const [status, setStatus] = useState("");
    const [search, setSearch] = useState("");
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const [modal, setModal] = useState(false);
    const [editing, setEditing] = useState(null);
    const [form, setForm] = useState(emptyForm);
    const [saving, setSaving] = useState(false);

    const load = async () => {
        setLoading(true);
        setError("");
        try {
            const params = new URLSearchParams({ page: "1", pageSize: "50" });
            if (search) params.set("search", search);
            if (status) params.set("status", status);
            const [page, cityItems, districtItems] = await Promise.all([
                apiRequest(`/admin/properties?${params}`),
                apiRequest("/cities", { auth: false }),
                apiRequest("/districts", { auth: false }),
            ]);
            setItems(page?.items ?? []);
            setCities(cityItems ?? []);
            setDistricts(districtItems ?? []);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { load(); }, [status]);

    const openCreate = () => {
        setEditing(null);
        setForm(emptyForm);
        setModal(true);
    };

    const openEdit = async (item) => {
        try {
            const detail = await apiRequest(`/properties/${item.id}`);
            setEditing(item);
            setForm({
                title: detail.title ?? "", description: detail.description ?? "",
                address: detail.address ?? "", price: detail.price ?? "", area: detail.area ?? "",
                propertyType: detail.typeCode ?? "Land", zoning: detail.zoningCode ?? "Residential",
                terrain: detail.terrainCode ?? "Flat", roadAccess: detail.roadAccessCode ?? "RoadFrontage",
                roadDistanceMeters: detail.roadDistanceMeters ?? "",
                utilities: detail.utilityCodes ?? [], cityId: detail.cityId ?? "",
                districtId: detail.districtId ?? "",
            });
            setModal(true);
        } catch (err) {
            setError(err.message);
        }
    };

    const save = async (event) => {
        event.preventDefault();
        setSaving(true);
        setError("");
        try {
            const payload = {
                ...form,
                price: Number(form.price), area: Number(form.area),
                roadDistanceMeters: form.roadDistanceMeters ? Number(form.roadDistanceMeters) : null,
                cityId: Number(form.cityId), districtId: Number(form.districtId),
            };
            if (editing) await apiRequest(`/properties/${editing.id}`, { method: "PUT", body: payload });
            else await apiRequest("/properties", { method: "POST", body: payload });
            setModal(false);
            await load();
        } catch (err) {
            setError(err.message);
        } finally {
            setSaving(false);
        }
    };

    const remove = async (item) => {
        if (!window.confirm(`"${item.title}" ilanını silmek istiyor musunuz?`)) return;
        try {
            await apiRequest(`/properties/${item.id}`, { method: "DELETE" });
            await load();
        } catch (err) {
            setError(err.message);
        }
    };

    const featured = async (item) => {
        await apiRequest(`/admin/properties/${item.id}/featured`, { method: "PUT", body: { isFeatured: !item.featured } });
        await load();
    };

    const approve = async (item) => {
        await apiRequest(`/admin/properties/${item.id}/approve`, { method: "POST" });
        await load();
    };

    const reject = async (item) => {
        const reason = window.prompt("Ret sebebini yazın:", item.rejectionReason ?? "");
        if (reason === null) return;
        await apiRequest(`/admin/properties/${item.id}/reject`, { method: "POST", body: { reason } });
        await load();
    };

    const toggleUtility = (value) => setForm((current) => ({
        ...current,
        utilities: current.utilities.includes(value)
            ? current.utilities.filter((item) => item !== value)
            : [...current.utilities, value],
    }));

    const filteredDistricts = districts.filter((item) => !form.cityId || item.cityId === Number(form.cityId));

    return (
        <div className="mx-auto max-w-[1400px]">
            <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
                <div>
                    <p className="text-xs font-bold uppercase tracking-[0.18em] text-[#9a794f]">Yönetim</p>
                    <h1 className="mt-2 text-3xl font-bold">İlanlar</h1>
                </div>
                <button type="button" onClick={openCreate} className="inline-flex items-center justify-center gap-2 rounded-xl bg-[#252920] px-4 py-3 text-sm font-semibold text-white">
                    <Plus size={17} /> Yeni İlan
                </button>
            </div>

            {error && <div className="mb-4 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>}

            <div className="mb-5 flex flex-col gap-3 rounded-2xl border border-[#e2e4dc] bg-white p-3 sm:flex-row">
                <label className="flex min-h-11 flex-1 items-center gap-2 rounded-xl border border-[#e7e8e1] px-3">
                    <Search size={17} className="text-[#9a794f]" />
                    <input value={search} onChange={(event) => setSearch(event.target.value)} onKeyDown={(event) => event.key === "Enter" && load()} placeholder="İlan ara..." className="min-w-0 flex-1 bg-transparent text-sm outline-none" />
                </label>
                <select value={status} onChange={(event) => setStatus(event.target.value)} className="h-11 rounded-xl border border-[#e7e8e1] bg-white px-3 text-sm">
                    <option value="">Tüm Durumlar</option>
                    {Object.entries(statuses).map(([value, label]) => <option key={value} value={value}>{label}</option>)}
                </select>
                <button type="button" onClick={load} className="h-11 rounded-xl border border-[#dfe1d8] px-4 text-sm font-semibold">Ara</button>
            </div>

            <div className="overflow-hidden rounded-2xl border border-[#e2e4dc] bg-white">
                <div className="overflow-x-auto">
                    <table className="w-full min-w-[900px] text-left text-sm">
                        <thead className="border-b border-[#eceee7] bg-[#fafbf8] text-xs uppercase tracking-wide text-[#7b8074]">
                            <tr><th className="px-5 py-4">İlan</th><th className="px-5 py-4">Konum</th><th className="px-5 py-4">Fiyat</th><th className="px-5 py-4">Durum</th><th className="px-5 py-4 text-right">İşlemler</th></tr>
                        </thead>
                        <tbody className="divide-y divide-[#f0f1ec]">
                            {!loading && items.map((item) => (
                                <tr key={item.id} className="hover:bg-[#fafbf8]">
                                    <td className="px-5 py-4"><p className="max-w-xs font-semibold">{item.title}</p><p className="mt-1 text-xs text-slate-500">{item.area} m² · {item.type}</p></td>
                                    <td className="px-5 py-4 text-slate-600">{item.city}<br /><span className="text-xs">{item.district}</span></td>
                                    <td className="px-5 py-4 font-semibold">{Number(item.price).toLocaleString("az-AZ")} AZN</td>
                                    <td className="px-5 py-4"><span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold">{statuses[item.status] ?? item.status}</span></td>
                                    <td className="px-5 py-4">
                                        <div className="flex justify-end gap-1">
                                            {item.status === "Pending" && <button title="Onayla" onClick={() => approve(item)} className="grid size-9 place-items-center rounded-lg text-emerald-700 hover:bg-emerald-50"><CheckCircle2 size={17} /></button>}
                                            {item.status === "Pending" && <button title="Reddet" onClick={() => reject(item)} className="grid size-9 place-items-center rounded-lg text-red-600 hover:bg-red-50"><XCircle size={17} /></button>}
                                            <button title="Öne Çıkan" onClick={() => featured(item)} className={`grid size-9 place-items-center rounded-lg hover:bg-amber-50 ${item.featured ? "text-amber-600" : "text-slate-400"}`}><Star size={17} fill={item.featured ? "currentColor" : "none"} /></button>
                                            <button title="Düzenle" onClick={() => openEdit(item)} className="grid size-9 place-items-center rounded-lg text-slate-600 hover:bg-slate-100"><Pencil size={17} /></button>
                                            <button title="Sil" onClick={() => remove(item)} className="grid size-9 place-items-center rounded-lg text-red-600 hover:bg-red-50"><Trash2 size={17} /></button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    {loading && <p className="py-14 text-center text-sm text-slate-500">Yükleniyor...</p>}
                    {!loading && !items.length && <p className="py-14 text-center text-sm text-slate-500">İlan bulunamadı.</p>}
                </div>
            </div>

            {modal && (
                <div className="fixed inset-0 z-[100] grid place-items-center overflow-y-auto bg-black/55 p-4">
                    <form onSubmit={save} className="my-4 w-full max-w-3xl rounded-2xl bg-white p-5 shadow-2xl sm:p-7">
                        <div className="mb-5 flex items-center justify-between">
                            <h2 className="text-xl font-bold">{editing ? "İlanı Düzenle" : "Yeni İlan"}</h2>
                            <button type="button" onClick={() => setModal(false)} className="grid size-9 place-items-center rounded-lg hover:bg-slate-100"><X size={18} /></button>
                        </div>
                        <div className="grid gap-4 sm:grid-cols-2">
                            <Field label="Başlık"><input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} required /></Field>
                            <Field label="Adres"><input value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} required /></Field>
                            <Field label="Fiyat (AZN)" type="number"><input type="number" value={form.price} onChange={(e) => setForm({ ...form, price: e.target.value })} required /></Field>
                            <Field label="Alan (m²)" type="number"><input type="number" value={form.area} onChange={(e) => setForm({ ...form, area: e.target.value })} required /></Field>
                            <Field label="Şehir"><select value={form.cityId} onChange={(e) => setForm({ ...form, cityId: e.target.value, districtId: "" })} required><option value="">Seçin</option>{cities.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></Field>
                            <Field label="İlçe"><select value={form.districtId} onChange={(e) => setForm({ ...form, districtId: e.target.value })} required><option value="">Seçin</option>{filteredDistricts.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></Field>
                            <Field label="Arazi Türü"><select value={form.propertyType} onChange={(e) => setForm({ ...form, propertyType: e.target.value })}><option value="Land">Arsa</option><option value="Field">Tarla</option><option value="VineyardOrchard">Bağ / Bahçe</option></select></Field>
                            <Field label="İmar"><select value={form.zoning} onChange={(e) => setForm({ ...form, zoning: e.target.value })}><option value="Residential">Konut imarlı</option><option value="Agricultural">Tarım arazisi</option><option value="Investment">Yatırımlık arsa</option></select></Field>
                            <Field label="Arazi Yapısı"><select value={form.terrain} onChange={(e) => setForm({ ...form, terrain: e.target.value })}><option value="Flat">Düz arazi</option><option value="SlightlySloped">Hafif eğimli arazi</option><option value="Sloped">Eğimli arazi</option><option value="FlatFertile">Düz ve verimli toprak</option></select></Field>
                            <Field label="Yol Durumu"><select value={form.roadAccess} onChange={(e) => setForm({ ...form, roadAccess: e.target.value })}><option value="RoadFrontage">Yola cepheli</option><option value="AsphaltFrontage">Asfalt yola cepheli</option><option value="DirtRoadFrontage">Toprak yola cepheli</option><option value="VillageRoadFrontage">Köy yoluna cepheli</option><option value="MainRoadNearby">Ana yola yakın</option><option value="AsphaltRoadNearby">Asfalt yola yakın</option></select></Field>
                            <Field label="Yol Mesafesi (m)"><input type="number" value={form.roadDistanceMeters} onChange={(e) => setForm({ ...form, roadDistanceMeters: e.target.value })} /></Field>
                            <div className="sm:col-span-2">
                                <span className="mb-2 block text-xs font-semibold">Altyapı</span>
                                <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
                                    {[["Electricity","Elektrik"],["Water","Su"],["NaturalGas","Doğalgaz"],["IrrigationWater","Sulama suyu"]].map(([value,label]) => (
                                        <label key={value} className="flex items-center gap-2 rounded-xl border border-[#e4e6de] px-3 py-2.5 text-sm"><input type="checkbox" checked={form.utilities.includes(value)} onChange={() => toggleUtility(value)} /> {label}</label>
                                    ))}
                                </div>
                            </div>
                            <Field label="Açıklama"><textarea rows={4} value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} required /></Field>
                        </div>
                        <div className="mt-6 flex justify-end gap-3">
                            <button type="button" onClick={() => setModal(false)} className="rounded-xl border border-[#e2e4dc] px-4 py-3 text-sm font-semibold">İptal</button>
                            <button type="submit" disabled={saving} className="rounded-xl bg-[#252920] px-5 py-3 text-sm font-semibold text-white disabled:opacity-60">{saving ? "Kaydediliyor..." : "Kaydet"}</button>
                        </div>
                    </form>
                </div>
            )}
        </div>
    );
}

function Field({ label, children }) {
    return (
        <label className="block text-xs font-semibold text-[#55594f]">
            {label}
            <span className="mt-2 block [&>input]:h-11 [&>input]:w-full [&>input]:rounded-xl [&>input]:border [&>input]:border-[#e2e4dc] [&>input]:px-3 [&>input]:text-sm [&>input]:font-normal [&>select]:h-11 [&>select]:w-full [&>select]:rounded-xl [&>select]:border [&>select]:border-[#e2e4dc] [&>select]:bg-white [&>select]:px-3 [&>select]:text-sm [&>select]:font-normal [&>textarea]:w-full [&>textarea]:rounded-xl [&>textarea]:border [&>textarea]:border-[#e2e4dc] [&>textarea]:px-3 [&>textarea]:py-3 [&>textarea]:text-sm [&>textarea]:font-normal [&>textarea]:outline-none">
                {children}
            </span>
        </label>
    );
}
