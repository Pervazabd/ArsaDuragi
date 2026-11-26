import { createContext, useContext, useEffect, useMemo, useState } from "react";

export const languages = [
    { value: "az", label: "Azərbaycanca", countryLabel: "Azərbaycan", flag: "🇦🇿", triggerLabel: "AZ" },
    { value: "tr", label: "Türkçe", countryLabel: "Türkiye", flag: "🇹🇷", triggerLabel: "TR" },
    { value: "en", label: "English", countryLabel: "United Kingdom", flag: "🇬🇧", triggerLabel: "EN" },
    { value: "de", label: "Deutsch", countryLabel: "Deutschland", flag: "🇩🇪", triggerLabel: "DE" },
    { value: "es", label: "Español", countryLabel: "España", flag: "🇪🇸", triggerLabel: "ES" },
    { value: "ru", label: "Русский", countryLabel: "Россия", flag: "🇷🇺", triggerLabel: "RU" },
];

const dictionaries = {
    az: {},
    tr: {},
    en: {},
    de: {},
    es: {},
    ru: {},
};

const shared = {
    az: {
        home: "Ana Səhifə", listings: "Elanlar", contact: "Əlaqə", login: "Giriş",
        register: "Qeydiyyat", logout: "Çıxış", user: "İstifadəçi", admin: "Admin",
        discover: "Kəşf et", menuOpen: "Menyunu aç", menuClose: "Menyunu bağla",
        language: "Dil seç", backHome: "Ana səhifəyə qayıt", explore: "Araşdır",
        notFoundTitle: "Səhifə tapılmadı", notFoundText: "Axtardığınız səhifə mövcud deyil.",
        backToHome: "Ana səhifəyə qayıt",
        footerDescription: "Bakı və ətrafında əmlak axtarışınızın etibarlı ünvanı.",
        footerTagline: "Düzgün torpaq, düzgün investisiya.",
        footerLocation: "Bakı, Azərbaycan", footerExplore: "Kəşf edin",
        footerContact: "Əlaqə", footerTop: "Başa dön",
        heroTitle1: "Gələcəyinizi", heroTitle2: "düzgün torpaqda", heroTitle3: "formalaşdırın.",
        heroDescription: "Xəyalınızdakı layihəyə uyğun torpaq və əraziləri kəşf edin. Düzgün məkanı tapın, investisiyanıza möhkəm başlanğıc verin.",
        heroExplore: "Torpaqları kəşf edin", heroAbout: "Haqqımızda",
        trustedListings: "Etibarlı elanlar", rightLocation: "Düzgün məkan", investmentLand: "İnvestisiyaya uyğun torpaqlar",
        locationStart: "Düzgün yerdən başlayın", findLand: "Sizə uyğun torpağı tapın", locationText: "Seçimləri kəşf edin, gələcəyinizi planlaşdırın.",
        scrollDown: "Aşağı sürüşdürün",
        productsLabel: "Arsa Durağı seçimi", productsTitle1: "Gələcəyə investisiya,", productsTitle2: "düzgün ərazidə.",
        productsDescription: "Bileçik və ətrafında torpaq, bağ və investisiya imkanlarını kəşf edin.",
        allListings: "Bütün elanlar", searchPlaceholder: "Elan, torpaq növü və ya məkan axtar...",
        searchAria: "Elanlarda axtar", clearSearch: "Axtarışı təmizlə", filter: "Filtrlə",
        sortFeatured: "Seçilmişlər", sortPriceAsc: "Qiymət: artan", sortPriceDesc: "Qiymət: azalan", sortAreaDesc: "Sahə: böyükdən kiçiyə",
        sortAria: "Elanları sırala", region: "Rayon / bölgə", allRegions: "Bütün bölgələr",
        landType: "Torpaq növü", allLandTypes: "Bütün torpaq növləri", land: "Torpaq", field: "Tarla",
        garden: "Bağ / Bağça", investmentLandType: "İnvestisiya torpağı", clearFilters: "Filtirləri təmizlə",
        featuredCount: "seçilmiş elan", andAround: "Bileçik və ətrafı",
        noResultsTitle: "Axtarışınıza uyğun elan tapılmadı", noResultsText: "Fərqli rayon və ya torpaq növü seçərək yenidən sınayın.",
        contactTitle: "Bizimlə əlaqə", contactDescription: "Sorğularınız və torpaq ehtiyacınız üçün bizimlə asanlıqla əlaqə saxlaya bilərsiniz.",
        contactInfo: "Əlaqə məlumatlarımız", phone: "Telefon", whatsapp: "WhatsApp", email: "E-poçt", location: "Məkan",
        workingHours: "İş saatları", sendMessage: "Bizə mesaj göndərin", formDescription: "Formu doldurun, mesajınızı WhatsApp üzərindən göndərin.",
        fullName: "Ad Soyad", phoneLabel: "Telefon", emailLabel: "E-poçt", subject: "Maraqlanılan mövzu",
        selectSubject: "Mövzu seçin", buyLand: "Torpaq almaq", sellLand: "Torpaq satmaq", investmentAdvice: "İnvestisiya məsləhəti",
        listingInfo: "Elan haqqında məlumat", other: "Digər", message: "Mesajınız", sendWhatsApp: "WhatsApp ilə göndər",
        nameRequired: "Adınızı daxil edin.", phoneRequired: "Etibarlı telefon nömrəsi daxil edin.",
        emailRequired: "Etibarlı e-poçt ünvanı daxil edin.", subjectRequired: "Mövzu seçin.", messageRequired: "Mesajınızı yazın.",
        whatsappReady: "Mesajınız hazırdır", whatsappOpened: "WhatsApp açıldı. Göndərmək üçün WhatsApp-dakı göndər düyməsinə basın.",
        newMessage: "Yeni mesaj yarat",
    },
};

Object.assign(dictionaries.az, shared.az);

Object.assign(dictionaries.tr, {
    home: "Ana Sayfa", listings: "İlanlar", contact: "İletişim", login: "Giriş", register: "Kayıt Ol",
    logout: "Çıkış", user: "Kullanıcı", admin: "Yönetici", discover: "Keşfet", menuOpen: "Menüyü aç",
    menuClose: "Menüyü kapat", language: "Dil seç", backHome: "Ana sayfaya dön", explore: "Keşfet",
    notFoundTitle: "Sayfa bulunamadı", notFoundText: "Aradığınız sayfa mevcut değil.", backToHome: "Ana sayfaya dön",
    footerDescription: "Bilecik ve çevresinde emlak arayışınızın güvenilir adresi.", footerTagline: "Doğru arsa, doğru yatırım.",
    footerLocation: "Bilecik, Türkiye", footerExplore: "Keşfedin", footerContact: "İletişim", footerTop: "Başa dön",
    heroTitle1: "Geleceğinizi", heroTitle2: "doğru toprakta", heroTitle3: "şekillendirin.",
    heroDescription: "Hayalinizdeki projeye uygun arsa ve arazileri keşfedin. Doğru lokasyonu bulun, yatırımınıza sağlam bir başlangıç yapın.",
    heroExplore: "Arsaları Keşfedin", heroAbout: "Bizi Tanıyın", trustedListings: "Güvenilir ilanlar",
    rightLocation: "Doğru lokasyon", investmentLand: "Yatırıma uygun arsalar", locationStart: "Doğru yerden başlayın",
    findLand: "Size uygun araziyi bulun", locationText: "Seçenekleri keşfedin, geleceğinizi planlayın.", scrollDown: "Aşağı kaydırın",
    productsLabel: "Arsa Durağı seçkisi", productsTitle1: "Geleceğe yatırım,", productsTitle2: "doğru arazide.",
    productsDescription: "Bilecik ve çevresindeki arsa, tarla ve yatırım fırsatlarını keşfedin.",
    allListings: "Tüm ilanlar", searchPlaceholder: "İlan, arsa türü veya konum ara...", searchAria: "İlanlarda ara",
    clearSearch: "Aramayı temizle", filter: "Filtrele", sortFeatured: "Öne çıkanlar", sortPriceAsc: "Fiyat: Artan",
    sortPriceDesc: "Fiyat: Azalan", sortAreaDesc: "Alan: Büyükten küçüğe", sortAria: "İlanları sırala",
    region: "İlçe / Bölge", allRegions: "Tüm bölgeler", landType: "Arazi türü", allLandTypes: "Tüm arazi türleri",
    land: "Arsa", field: "Tarla", garden: "Bağ / Bahçe", investmentLandType: "Yatırımlık arazi",
    clearFilters: "Filtreleri temizle", featuredCount: "öne çıkan ilan", andAround: "Bilecik ve çevresi",
    noResultsTitle: "Aramanıza uygun ilan bulunamadı", noResultsText: "Farklı bir ilçe veya arazi türü seçerek yeniden deneyebilirsiniz.",
    contactTitle: "İletişime Geçin", contactDescription: "Sorularınız ve arsa talepleriniz için bizimle kolayca iletişime geçebilirsiniz.",
    contactInfo: "İletişim Bilgilerimiz", phone: "Telefon", whatsapp: "WhatsApp", email: "E-posta", location: "Konum",
    workingHours: "ÇALIŞMA SAATLERİ", sendMessage: "Bize Mesaj Gönderin", formDescription: "Formu doldurun, mesajınızı WhatsApp üzerinden iletin.",
    fullName: "Ad Soyad", phoneLabel: "Telefon", emailLabel: "E-posta", subject: "İlgilendiğiniz Konu",
    selectSubject: "Bir konu seçiniz", buyLand: "Arsa Satın Alma", sellLand: "Arsa Satışı",
    investmentAdvice: "Yatırım Danışmanlığı", listingInfo: "İlan Hakkında Bilgi", other: "Diğer",
    message: "Mesajınız", sendWhatsApp: "WhatsApp ile Gönder", nameRequired: "Adınızı giriniz.",
    phoneRequired: "Geçerli bir telefon numarası giriniz.", emailRequired: "Geçerli bir e-posta adresi giriniz.",
    subjectRequired: "Konu seçiniz.", messageRequired: "Mesajınızı yazınız.", whatsappReady: "Mesajınız Hazır",
    whatsappOpened: "WhatsApp açıldı. Mesajınızı iletmek için WhatsApp üzerinden gönder butonuna basmanız gerekiyor.",
    newMessage: "Yeni mesaj oluştur",
});

const en = {
    home:"Home",listings:"Listings",contact:"Contact",login:"Login",register:"Sign Up",logout:"Logout",user:"User",admin:"Admin",discover:"Explore",menuOpen:"Open menu",menuClose:"Close menu",language:"Select language",backHome:"Back to home",explore:"Explore",notFoundTitle:"Page not found",notFoundText:"The page you are looking for does not exist.",backToHome:"Back to home",
    footerDescription:"Your trusted partner for real estate searches in Bilecik and nearby areas.",footerTagline:"The right land, the right investment.",footerLocation:"Bilecik, Türkiye",footerExplore:"Explore",footerContact:"Contact",footerTop:"Back to top",
    heroTitle1:"Shape your future",heroTitle2:"on the right land",heroTitle3:"",heroDescription:"Discover land and plots suited to your project. Find the right location and begin your investment on solid ground.",heroExplore:"Explore Listings",heroAbout:"About Us",trustedListings:"Trusted listings",rightLocation:"Right location",investmentLand:"Investment-ready land",locationStart:"Start in the right place",findLand:"Find land for you",locationText:"Explore the options and plan your future.",scrollDown:"Scroll down",
    productsLabel:"Arsa Duragi selection",productsTitle1:"Invest in the future,",productsTitle2:"on the right land.",productsDescription:"Explore land, fields and investment opportunities in Bilecik and nearby areas.",allListings:"All listings",searchPlaceholder:"Search listing, land type or location...",searchAria:"Search listings",clearSearch:"Clear search",filter:"Filters",sortFeatured:"Featured",sortPriceAsc:"Price: Low to high",sortPriceDesc:"Price: High to low",sortAreaDesc:"Area: Largest first",sortAria:"Sort listings",region:"District / Area",allRegions:"All areas",landType:"Land type",allLandTypes:"All land types",land:"Plot",field:"Field",garden:"Vineyard / Garden",investmentLandType:"Investment land",clearFilters:"Clear filters",featuredCount:"featured listings",andAround:"Bilecik and nearby",
    noResultsTitle:"No listings match your search",noResultsText:"Try another district or land type.",contactTitle:"Get in touch",contactDescription:"Contact us easily with your questions or land requests.",contactInfo:"Our contact information",phone:"Phone",whatsapp:"WhatsApp",email:"Email",location:"Location",workingHours:"WORKING HOURS",sendMessage:"Send us a message",formDescription:"Fill out the form and send your message through WhatsApp.",fullName:"Full name",phoneLabel:"Phone",emailLabel:"Email",subject:"Interested topic",selectSubject:"Select a topic",buyLand:"Buy land",sellLand:"Sell land",investmentAdvice:"Investment advice",listingInfo:"Listing information",other:"Other",message:"Your message",sendWhatsApp:"Send via WhatsApp",nameRequired:"Enter your name.",phoneRequired:"Enter a valid phone number.",emailRequired:"Enter a valid email address.",subjectRequired:"Select a topic.",messageRequired:"Write your message.",whatsappReady:"Your message is ready",whatsappOpened:"WhatsApp opened. Press Send in WhatsApp to deliver your message.",newMessage:"Create new message",
};

const de = {home:"Startseite",listings:"Inserate",contact:"Kontakt",login:"Anmelden",register:"Registrieren",logout:"Abmelden",user:"Benutzer",admin:"Administrator",discover:"Entdecken",menuOpen:"Menü öffnen",menuClose:"Menü schließen",language:"Sprache wählen",backHome:"Zur Startseite",explore:"Entdecken",notFoundTitle:"Seite nicht gefunden",notFoundText:"Die gesuchte Seite existiert nicht.",backToHome:"Zurück zur Startseite",footerDescription:"Ihr zuverlässiger Partner für Immobilien in Bilecik und Umgebung.",footerTagline:"Das richtige Land, die richtige Investition.",footerLocation:"Bilecik, Türkei",footerExplore:"Entdecken",footerContact:"Kontakt",footerTop:"Nach oben",heroTitle1:"Gestalten Sie Ihre Zukunft",heroTitle2:"auf dem richtigen Grundstück",heroTitle3:"",heroDescription:"Entdecken Sie Grundstücke für Ihr Projekt. Finden Sie den richtigen Standort und beginnen Sie solide.",heroExplore:"Grundstücke entdecken",heroAbout:"Über uns",trustedListings:"Verifizierte Inserate",rightLocation:"Richtiger Standort",investmentLand:"Investitionsgrundstücke",locationStart:"Starten Sie am richtigen Ort",findLand:"Finden Sie Ihr Grundstück",locationText:"Entdecken Sie Optionen und planen Sie Ihre Zukunft.",scrollDown:"Nach unten scrollen",productsLabel:"Arsa Durağı-Auswahl",productsTitle1:"In die Zukunft investieren,",productsTitle2:"auf dem richtigen Grundstück.",productsDescription:"Entdecken Sie Grundstücke und Investmentmöglichkeiten in Bilecik und Umgebung.",allListings:"Alle Inserate",searchPlaceholder:"Inserat, Grundstücksart oder Ort suchen...",searchAria:"Inserate durchsuchen",clearSearch:"Suche löschen",filter:"Filtern",sortFeatured:"Empfohlen",sortPriceAsc:"Preis: aufsteigend",sortPriceDesc:"Preis: absteigend",sortAreaDesc:"Fläche: absteigend",sortAria:"Inserate sortieren",region:"Bezirk / Gebiet",allRegions:"Alle Gebiete",landType:"Grundstücksart",allLandTypes:"Alle Grundstücksarten",land:"Grundstück",field:"Feld",garden:"Wein-/Gartenland",investmentLandType:"Investmentgrundstück",clearFilters:"Filter löschen",featuredCount:"empfohlene Inserate",andAround:"Bilecik und Umgebung",noResultsTitle:"Keine passenden Inserate gefunden",noResultsText:"Versuchen Sie einen anderen Bezirk oder Grundstückstyp.",contactTitle:"Kontakt aufnehmen",contactDescription:"Kontaktieren Sie uns bei Fragen oder Grundstückswünschen.",contactInfo:"Unsere Kontaktdaten",phone:"Telefon",whatsapp:"WhatsApp",email:"E-Mail",location:"Standort",workingHours:"ÖFFNUNGSZEITEN",sendMessage:"Nachricht senden",formDescription:"Füllen Sie das Formular aus und senden Sie Ihre Nachricht über WhatsApp.",fullName:"Vor- und Nachname",phoneLabel:"Telefon",emailLabel:"E-Mail",subject:"Thema",selectSubject:"Thema auswählen",buyLand:"Grundstück kaufen",sellLand:"Grundstück verkaufen",investmentAdvice:"Investmentberatung",listingInfo:"Frage zum Inserat",other:"Sonstiges",message:"Ihre Nachricht",sendWhatsApp:"Über WhatsApp senden",nameRequired:"Geben Sie Ihren Namen ein.",phoneRequired:"Geben Sie eine gültige Telefonnummer ein.",emailRequired:"Geben Sie eine gültige E-Mail-Adresse ein.",subjectRequired:"Wählen Sie ein Thema.",messageRequired:"Schreiben Sie Ihre Nachricht.",whatsappReady:"Ihre Nachricht ist bereit",whatsappOpened:"WhatsApp wurde geöffnet. Drücken Sie in WhatsApp auf Senden.",newMessage:"Neue Nachricht erstellen"};
const es = {home:"Inicio",listings:"Anuncios",contact:"Contacto",login:"Iniciar sesión",register:"Registrarse",logout:"Cerrar sesión",user:"Usuario",admin:"Administrador",discover:"Explorar",menuOpen:"Abrir menú",menuClose:"Cerrar menú",language:"Seleccionar idioma",backHome:"Volver al inicio",explore:"Explorar",notFoundTitle:"Página no encontrada",notFoundText:"La página que buscas no existe.",backToHome:"Volver al inicio",footerDescription:"Su socio de confianza para inmuebles en Bilecik y alrededores.",footerTagline:"El terreno adecuado, la inversión adecuada.",footerLocation:"Bilecik, Türkiye",footerExplore:"Explorar",footerContact:"Contacto",footerTop:"Volver arriba",heroTitle1:"Diseña tu futuro",heroTitle2:"sobre el terreno adecuado",heroTitle3:"",heroDescription:"Descubre terrenos adecuados para tu proyecto. Encuentra la ubicación correcta y empieza con una base sólida.",heroExplore:"Explorar terrenos",heroAbout:"Conócenos",trustedListings:"Anuncios confiables",rightLocation:"Ubicación adecuada",investmentLand:"Terrenos para invertir",locationStart:"Empieza en el lugar adecuado",findLand:"Encuentra tu terreno",locationText:"Explora las opciones y planifica tu futuro.",scrollDown:"Desliza hacia abajo",productsLabel:"Selección de Arsa Duragi",productsTitle1:"Invierte en el futuro,",productsTitle2:"en el terreno adecuado.",productsDescription:"Descubre terrenos y oportunidades de inversión en Bilecik y alrededores.",allListings:"Todos los anuncios",searchPlaceholder:"Buscar anuncio, tipo de terreno o ubicación...",searchAria:"Buscar anuncios",clearSearch:"Borrar búsqueda",filter:"Filtrar",sortFeatured:"Destacados",sortPriceAsc:"Precio: menor a mayor",sortPriceDesc:"Precio: mayor a menor",sortAreaDesc:"Área: mayor primero",sortAria:"Ordenar anuncios",region:"Distrito / Zona",allRegions:"Todas las zonas",landType:"Tipo de terreno",allLandTypes:"Todos los tipos",land:"Terreno",field:"Campo",garden:"Viñedo / Jardín",investmentLandType:"Terreno de inversión",clearFilters:"Borrar filtros",featuredCount:"anuncios destacados",andAround:"Bilecik y alrededores",noResultsTitle:"No se encontraron anuncios",noResultsText:"Prueba con otro distrito o tipo de terreno.",contactTitle:"Ponte en contacto",contactDescription:"Contáctanos fácilmente para consultas o solicitudes de terrenos.",contactInfo:"Información de contacto",phone:"Teléfono",whatsapp:"WhatsApp",email:"Correo electrónico",location:"Ubicación",workingHours:"HORARIO",sendMessage:"Envianos un mensaje",formDescription:"Completa el formulario y envía el mensaje por WhatsApp.",fullName:"Nombre completo",phoneLabel:"Teléfono",emailLabel:"Correo electrónico",subject:"Tema",selectSubject:"Selecciona un tema",buyLand:"Comprar terreno",sellLand:"Vender terreno",investmentAdvice:"Asesoría de inversión",listingInfo:"Información del anuncio",other:"Otro",message:"Tu mensaje",sendWhatsApp:"Enviar por WhatsApp",nameRequired:"Introduce tu nombre.",phoneRequired:"Introduce un teléfono válido.",emailRequired:"Introduce un correo válido.",subjectRequired:"Selecciona un tema.",messageRequired:"Escribe tu mensaje.",whatsappReady:"Tu mensaje está listo",whatsappOpened:"WhatsApp se abrió. Pulsa Enviar en WhatsApp.",newMessage:"Crear nuevo mensaje"};
const ru = {home:"Главная",listings:"Объявления",contact:"Контакты",login:"Войти",register:"Регистрация",logout:"Выйти",user:"Пользователь",admin:"Администратор",discover:"Обзор",menuOpen:"Открыть меню",menuClose:"Закрыть меню",language:"Выбрать язык",backHome:"На главную",explore:"Просмотр",notFoundTitle:"Страница не найдена",notFoundText:"Искомая страница не существует.",backToHome:"Вернуться на главную",footerDescription:"Надежный партнер для поиска недвижимости в Биледжике и окрестностях.",footerTagline:"Правильный участок, правильная инвестиция.",footerLocation:"Биледжик, Турция",footerExplore:"Обзор",footerContact:"Контакты",footerTop:"Наверх",heroTitle1:"Формируйте будущее",heroTitle2:"на подходящей земле",heroTitle3:"",heroDescription:"Найдите участки для вашего проекта. Выберите подходящее место и начните инвестиции с надежной основы.",heroExplore:"Посмотреть участки",heroAbout:"О нас",trustedListings:"Проверенные объявления",rightLocation:"Подходящее место",investmentLand:"Участки для инвестиций",locationStart:"Начните с правильного места",findLand:"Найдите подходящий участок",locationText:"Изучите варианты и спланируйте будущее.",scrollDown:"Прокрутите вниз",productsLabel:"Подборка Arsa Duragi",productsTitle1:"Инвестируйте в будущее,",productsTitle2:"на подходящей земле.",productsDescription:"Изучайте участки и инвестиционные возможности в Биледжике и окрестностях.",allListings:"Все объявления",searchPlaceholder:"Поиск объявления, типа или места...",searchAria:"Поиск объявлений",clearSearch:"Очистить поиск",filter:"Фильтр",sortFeatured:"Рекомендуемые",sortPriceAsc:"Цена: по возрастанию",sortPriceDesc:"Цена: по убыванию",sortAreaDesc:"Площадь: по убыванию",sortAria:"Сортировать объявления",region:"Район / область",allRegions:"Все районы",landType:"Тип участка",allLandTypes:"Все типы участков",land:"Участок",field:"Поле",garden:"Виноградник / сад",investmentLandType:"Инвестиционный участок",clearFilters:"Сбросить фильтры",featuredCount:"рекомендуемых объявлений",andAround:"Биледжик и окрестности",noResultsTitle:"Подходящих объявлений не найдено",noResultsText:"Попробуйте выбрать другой район или тип участка.",contactTitle:"Свяжитесь с нами",contactDescription:"Свяжитесь с нами по вопросам или запросам о земельных участках.",contactInfo:"Контактная информация",phone:"Телефон",whatsapp:"WhatsApp",email:"Эл. почта",location:"Адрес",workingHours:"ЧАСЫ РАБОТЫ",sendMessage:"Отправить сообщение",formDescription:"Заполните форму и отправьте сообщение через WhatsApp.",fullName:"Имя и фамилия",phoneLabel:"Телефон",emailLabel:"Эл. почта",subject:"Интересующая тема",selectSubject:"Выберите тему",buyLand:"Купить участок",sellLand:"Продать участок",investmentAdvice:"Инвестиционная консультация",listingInfo:"Вопрос об объявлении",other:"Другое",message:"Сообщение",sendWhatsApp:"Отправить через WhatsApp",nameRequired:"Введите имя.",phoneRequired:"Введите действительный номер телефона.",emailRequired:"Введите действительный адрес электронной почты.",subjectRequired:"Выберите тему.",messageRequired:"Напишите сообщение.",whatsappReady:"Сообщение готово",whatsappOpened:"WhatsApp открыт. Нажмите Отправить в WhatsApp.",newMessage:"Создать новое сообщение"};

Object.assign(dictionaries.en, en);
Object.assign(dictionaries.de, de);
Object.assign(dictionaries.es, es);
Object.assign(dictionaries.ru, ru);

const favoriteTranslations = {
    az: {
        favorites: "Sevimlilər",
        favoritesEyebrow: "Şəxsi siyahınız",
        favoritesTitle: "Sevimli torpaqlar",
        favoritesDescription: "Bəyəndiyiniz elanları bir yerdə saxlayın və istədiyiniz vaxt yenidən nəzərdən keçirin.",
        favoritesEmptyTitle: "Hələ sevimli elanınız yoxdur",
        favoritesEmptyDescription: "Elan kartındakı ürək düyməsinə basaraq bəyəndiyiniz torpaqları buraya əlavə edin.",
        browseListings: "Elanlara bax",
        loadingListings: "Elanlar yüklənir...",
        addFavorite: "Sevimlilərə əlavə et",
        removeFavorite: "Sevimlilərdən çıxar",
        loginToFavorite: "Sevimlilərə əlavə etmək üçün giriş edin",
        totalLandArea: "Ümumi torpaq sahəsi",
        pricePerSquareMeter: "Kvadratmetr qiyməti",
        salePrice: "Satış qiyməti",
        viewListing: "Bax",
        featured: "Seçilmiş",
        forSale: "Satılır",
        verifiedListing: "Təsdiqlənmiş elan",
    },
    tr: {
        favorites: "Favoriler",
        favoritesEyebrow: "Kişisel listeniz",
        favoritesTitle: "Favori arsalar",
        favoritesDescription: "Beğendiğiniz ilanları bir arada saklayın ve istediğiniz zaman tekrar inceleyin.",
        favoritesEmptyTitle: "Henüz favori ilanınız yok",
        favoritesEmptyDescription: "Beğendiğiniz arsaları buraya eklemek için ilan kartındaki kalp düğmesine basın.",
        browseListings: "İlanlara göz at",
        loadingListings: "İlanlar yükleniyor...",
        addFavorite: "Favorilere ekle",
        removeFavorite: "Favorilerden çıkar",
        loginToFavorite: "Favorilere eklemek için giriş yapın",
        totalLandArea: "Toplam arazi alanı",
        pricePerSquareMeter: "Metrekare fiyatı",
        salePrice: "Satış fiyatı",
        viewListing: "İncele",
        featured: "Öne çıkan",
        forSale: "Satılık",
        verifiedListing: "Doğrulanmış ilan",
    },
    en: {
        favorites: "Favorites",
        favoritesEyebrow: "Your saved list",
        favoritesTitle: "Favorite properties",
        favoritesDescription: "Keep listings you like in one place and revisit them any time.",
        favoritesEmptyTitle: "You have no favorite listings yet",
        favoritesEmptyDescription: "Press the heart on a listing card to save properties you like here.",
        browseListings: "Browse listings",
        loadingListings: "Loading listings...",
        addFavorite: "Add to favorites",
        removeFavorite: "Remove from favorites",
        loginToFavorite: "Log in to add favorites",
        totalLandArea: "Total land area",
        pricePerSquareMeter: "Price per square meter",
        salePrice: "Sale price",
        viewListing: "View",
        featured: "Featured",
        forSale: "For sale",
        verifiedListing: "Verified listing",
    },
    de: {
        favorites: "Favoriten",
        favoritesEyebrow: "Ihre persönliche Liste",
        favoritesTitle: "Lieblingsgrundstücke",
        favoritesDescription: "Speichern Sie interessante Inserate an einem Ort und sehen Sie sie jederzeit wieder an.",
        favoritesEmptyTitle: "Noch keine Favoriten",
        favoritesEmptyDescription: "Tippen Sie auf das Herz eines Inserats, um Grundstücke hier zu speichern.",
        browseListings: "Inserate ansehen",
        loadingListings: "Inserate werden geladen...",
        addFavorite: "Zu Favoriten hinzufügen",
        removeFavorite: "Aus Favoriten entfernen",
        loginToFavorite: "Melden Sie sich an, um Favoriten zu speichern",
        totalLandArea: "Gesamtfläche",
        pricePerSquareMeter: "Preis pro Quadratmeter",
        salePrice: "Verkaufspreis",
        viewListing: "Ansehen",
        featured: "Empfohlen",
        forSale: "Zu verkaufen",
        verifiedListing: "Verifiziertes Inserat",
    },
    es: {
        favorites: "Favoritos",
        favoritesEyebrow: "Tu lista personal",
        favoritesTitle: "Terrenos favoritos",
        favoritesDescription: "Guarda los anuncios que te gustan en un solo lugar y vuelve a verlos cuando quieras.",
        favoritesEmptyTitle: "Aún no tienes anuncios favoritos",
        favoritesEmptyDescription: "Pulsa el corazón de un anuncio para guardar aquí los terrenos que te gustan.",
        browseListings: "Ver anuncios",
        loadingListings: "Cargando anuncios...",
        addFavorite: "Añadir a favoritos",
        removeFavorite: "Quitar de favoritos",
        loginToFavorite: "Inicia sesión para guardar favoritos",
        totalLandArea: "Superficie total",
        pricePerSquareMeter: "Precio por metro cuadrado",
        salePrice: "Precio de venta",
        viewListing: "Ver",
        featured: "Destacado",
        forSale: "En venta",
        verifiedListing: "Anuncio verificado",
    },
    ru: {
        favorites: "Избранное",
        favoritesEyebrow: "Ваш личный список",
        favoritesTitle: "Избранные участки",
        favoritesDescription: "Сохраняйте понравившиеся объявления в одном месте и возвращайтесь к ним в любое время.",
        favoritesEmptyTitle: "В избранном пока нет объявлений",
        favoritesEmptyDescription: "Нажмите на сердце в карточке объявления, чтобы сохранить участок здесь.",
        browseListings: "Смотреть объявления",
        loadingListings: "Загрузка объявлений...",
        addFavorite: "Добавить в избранное",
        removeFavorite: "Удалить из избранного",
        loginToFavorite: "Войдите, чтобы сохранять объявления",
        totalLandArea: "Общая площадь участка",
        pricePerSquareMeter: "Цена за квадратный метр",
        salePrice: "Цена продажи",
        viewListing: "Посмотреть",
        featured: "Рекомендуемое",
        forSale: "Продаётся",
        verifiedListing: "Проверенное объявление",
    },
};

for (const language of languages) {
    Object.assign(dictionaries[language.value], favoriteTranslations[language.value]);
}

const I18nContext = createContext(null);

export function I18nProvider({ children }) {
    const [language, setLanguageState] = useState(() => {
        const saved = localStorage.getItem("arsaDuragi_language");
        return languages.some((item) => item.value === saved) ? saved : "tr";
    });

    useEffect(() => {
        localStorage.setItem("arsaDuragi_language", language);
        document.documentElement.lang = language;
    }, [language]);

    const setLanguage = (value) => {
        if (languages.some((item) => item.value === value)) {
            setLanguageState(value);
        }
    };

    const value = useMemo(() => ({
        language,
        setLanguage,
        t: (key) => dictionaries[language]?.[key] ?? dictionaries.tr[key] ?? dictionaries.az[key] ?? key,
    }), [language]);

    return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>;
}

export function useI18n() {
    const context = useContext(I18nContext);
    if (!context) throw new Error("useI18n I18nProvider içinde kullanılmalıdır.");
    return context;
}
