import { useEffect, useState } from "react";
import { MapPin, Plus, Pencil, Trash2, Building2, X } from "lucide-react";
import { apiRequest } from "../../services/api";

export default function AdminLocations() {
    const [cities, setCities] = useState([]);
    const [districts, setDistricts] = useState([]);
    const [tab, setTab] = useState("cities");
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const [modal, setModal] = useState(false);
    const [editing, setEditing] = useState(null);
    const [name, setName] = useState("");
    const [cityId, setCityId] = useState("");

    const load = async () => {
        setLoading(true);
        setError("");
        try {
            const [cityItems, districtItems] = await Promise.all([
                apiRequest("/cities", { auth: false }),
                apiRequest("/districts", { auth: false }),
            ]);
            setCities(cityItems ?? []);
            setDistricts(districtItems ?? []);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { load(); }, []);

    const openCreate = () => {
        setEditing(null);
        setName("");
        setCityId(cities[0]?.id ? String(cities[0].id) : "");
        setModal(true);
    };

    const openEdit = (item) => {
        setEditing(item);
        setName(item.name);
        setCityId(item.cityId ? String(item.cityId) : "");
        setModal(true);
    };

    const save = async (event) => {
        event.preventDefault();
        setError("");
        try {
            if (tab === "cities") {
                const payload = { name };
                if (editing) await apiRequest(`/cities/${editing.id}`, { method: "PUT", body: payload });
                else await apiRequest("/cities", { method: "POST", body: payload });
            } else {
                if (!cityId) throw new Error("Şehir seçin.");
                const payload = { name, cityId: Number(cityId) };
                if (editing) await apiRequest(`/districts/${editing.id}`, { method: "PUT", body: payload });
                else await apiRequest("/districts", { method: "POST", body: payload });
            }
            setModal(false);
            await load();
        } catch (err) {
            setError(err.message);
        }
    };

    const remove = async (item) => {
        if (!window.confirm(`"${item.name}" silinsin mi?`)) return;
        try {
            await apiRequest(`/${tab === "cities" ? "cities" : "districts"}/${item.id}`, { method: "DELETE" });
            await load();
        } catch (err) {
            setError(err.message);
        }
    };

    const rows = tab === "cities" ? cities : districts;

    return (
        <div className="mx-auto max-w-[1400px]">
            <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
                <div>
                    <p className="text-xs font-bold uppercase tracking-[0.18em] text-[#9a794f]">Konum</p>
                    <h1 className="mt-2 text-3xl font-bold">Şehir ve İlçeler</h1>
                </div>
                <button type="button" onClick={openCreate} className="inline-flex items-center justify-center gap-2 rounded-xl bg-[#252920] px-4 py-3 text-sm font-semibold text-white">
                    <Plus size={17} /> Yeni {tab === "cities" ? "Şehir" : "İlçe"}
                </button>
            </div>

            {error && <div className="mb-4 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>}

            <div className="mb-5 flex gap-2">
                <button type="button" onClick={() => setTab("cities")} className={`rounded-xl px-4 py-2.5 text-sm font-semibold ${tab === "cities" ? "bg-[#252920] text-white" : "border border-[#e2e4dc] bg-white"}`}>Şehirler</button>
                <button type="button" onClick={() => setTab("districts")} className={`rounded-xl px-4 py-2.5 text-sm font-semibold ${tab === "districts" ? "bg-[#252920] text-white" : "border border-[#e2e4dc] bg-white"}`}>İlçeler</button>
            </div>

            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                {rows.map((item) => (
                    <article key={item.id} className="rounded-2xl border border-[#e2e4dc] bg-white p-5">
                        <div className="flex items-start justify-between">
                            <span className="grid size-11 place-items-center rounded-xl bg-[#eee8dc] text-[#806844]">
                                {tab === "cities" ? <Building2 size={20} /> : <MapPin size={20} />}
                            </span>
                            <div className="flex gap-1">
                                <button type="button" onClick={() => openEdit(item)} className="grid size-9 place-items-center rounded-lg text-slate-600 hover:bg-slate-100"><Pencil size={16} /></button>
                                <button type="button" onClick={() => remove(item)} className="grid size-9 place-items-center rounded-lg text-red-600 hover:bg-red-50"><Trash2 size={16} /></button>
                            </div>
                        </div>
                        <h2 className="mt-4 font-semibold">{item.name}</h2>
                        {item.cityName && <p className="mt-1 text-xs text-slate-500">{item.cityName}</p>}
                    </article>
                ))}
                {loading && <p className="col-span-full py-14 text-center text-sm text-slate-500">Yükleniyor...</p>}
                {!loading && !rows.length && <p className="col-span-full py-14 text-center text-sm text-slate-500">Kayıt bulunamadı.</p>}
            </div>

            {modal && (
                <div className="fixed inset-0 z-[100] grid place-items-center bg-black/55 p-4">
                    <form onSubmit={save} className="w-full max-w-md rounded-2xl bg-white p-6 shadow-2xl">
                        <div className="mb-5 flex items-center justify-between">
                            <h2 className="text-xl font-bold">{editing ? "Düzenle" : "Yeni Ekle"}</h2>
                            <button type="button" onClick={() => setModal(false)} className="grid size-9 place-items-center rounded-lg hover:bg-slate-100"><X size={18} /></button>
                        </div>
                        <label className="block text-xs font-semibold">
                            Adı
                            <input value={name} onChange={(event) => setName(event.target.value)} required className="mt-2 h-11 w-full rounded-xl border border-[#e2e4dc] px-3 text-sm font-normal" />
                        </label>
                        {tab === "districts" && (
                            <label className="mt-4 block text-xs font-semibold">
                                Şehir
                                <select value={cityId} onChange={(event) => setCityId(event.target.value)} required className="mt-2 h-11 w-full rounded-xl border border-[#e2e4dc] bg-white px-3 text-sm font-normal">
                                    <option value="">Seçin</option>
                                    {cities.map((city) => <option key={city.id} value={city.id}>{city.name}</option>)}
                                </select>
                            </label>
                        )}
                        <div className="mt-6 flex justify-end gap-3">
                            <button type="button" onClick={() => setModal(false)} className="rounded-xl border border-[#e2e4dc] px-4 py-3 text-sm font-semibold">İptal</button>
                            <button type="submit" className="rounded-xl bg-[#252920] px-5 py-3 text-sm font-semibold text-white">Kaydet</button>
                        </div>
                    </form>
                </div>
            )}
        </div>
    );
}
