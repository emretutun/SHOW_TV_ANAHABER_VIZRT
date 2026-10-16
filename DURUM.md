# Proje durumu ve yayın uygulamasına dönüşüm yolu

Son güncelleme: 2026-10-16

## 1. Şu an ne çalışıyor

Hepsi test Engine'inde (127.0.0.1:6100, Viz Artist On Air) denendi.

| Alan | Durum | Kaynak |
|---|---|---|
| Engine bağlantısı, komut gönderme, nabız (`TEXTURE*MEMORY`), log | Çalışıyor | Eski uygulama logu |
| Sahne kontrolü ve yükleme (`GET_OBJECT`, `LOCATION_PATH`, `LOAD`, `SET_OBJECT`, `STAGE SHOW 0.0`) | Çalışıyor | Eski uygulama logu |
| Alt bant: haber, özel haber, sıcak haber, siyah metin, son dakika (giriş, intra, logo) | Çalışıyor | Eski uygulama logu |
| Akan yazı (az sonra) ver / kes | Çalışıyor | Eski uygulama logu |
| Yer KJ, Canlı, Canlı/Yer | Çalışıyor | Engine testi + canlı/yer logu |
| 10 logo, Facebook/Twitter, Whatsapp, BiP, spiker KJ | Çalışıyor | Eski uygulama logu |
| Flot (FLOT-01…20, arka katman) ver / al / katman boşaltma | Çalışıyor | Eski uygulama logu |
| Kelebekler (4 pencere sahnesi, arka katman, iki hedef) | Çalışıyor | Eski uygulama logu |
| Tümünü Al | Çalışıyor | Eski uygulama logu |
| Kulakçık kuralı (türe göre sağ üst logo değişimi) | Çalışıyor | Kullanıcı isteği (eski uygulamada yok) |
| Haberler sekmesi: tarih → akış → haber → sahne satırları | Çalışıyor (JSON veriyle) | Eski uygulama kaynak kodu |
| Listeler sekmesi (playlist'ler) | Çalışıyor (JSON veriyle) | Eski uygulama kaynak kodu |
| Ayarlar penceresi, `appsettings.json` | Çalışıyor | – |
| Veritabanı deposu (MySQL, yalnızca SELECT, oturum READ ONLY) | Yazıldı, canlıda denenmedi | Eski uygulama kaynak kodu |

## 2. Varsayımlar (logla doğrulanmadı)

Kodda ve notlarda "VARSAYIM" diye işaretli. Her biri eski uygulamadan tek bir logla kapanır.

1. **Tür değişimi**: haber açıkken son dakika verilince yeni türün `dir_intra_*`'sı çalıştırılıyor. Eski motor
   `tbl_gui.conflicting_gui_id` listesine bakıp "eskinin out'u + yeninin in'i" de yapıyor olabilir.
2. **Alt bant AL**: `dir_out_<tür>` + açık logoların out'u. (Akan yazı AL'da logo birlikte iniyor, doğrulandı.)
3. **Muhabir ve kamera KJ**: spikerle aynı kalıp varsayıldı (`dir_in_muhabirkj`, `dir_in_kamerakj`, `edt_txt_muhabir`, `edt_txt_kamera`).
4. **KJ İsim satırında görev**: Değer #2'de MUHABİR/KAMERA yazıyorsa ona göre tip seçiliyor.
5. **Canlı/Yer AL**: iki out director birlikte.
6. **Tümünü Al**: açık flot da birlikte çıkarılıyor.
7. **MySQL sütun adları**: eski kaynaktan çıkarıldı; saklı yordamla okunan veriler düz SELECT'e çevrildi, canlıda doğrulanmadı.

## 3. Hiç loglanmamış / yapılmamış

- EVDE KAL, KISA KISA logoları; ROLLCAPTION sahnesi (ana katmana yüklenen jenerik).
- Eski uygulamanın "Kelebekler" ekranındaki SİL ve KAYDET (yazma gerektirir).
- Program modları: Gece Hattı, 15 Temmuz, Seçim 2019 kontrolcüleri (yalnızca Ana Haber dolduruldu).
- Komut toplu gönderimi (eski uygulama bir işlemin komutlarını tek pakette yolluyor; biz tek tek).
- Katman başına ayrı meşgul kilidi (bizde ana sahne için tek kilit; kelebek panelleri ayrı).
- Sahne yüklemenin arka planda yapılması (şu an LOAD sırasında arayüz bekliyor).
- Dinamik alan kalp atışı (bu sahnede gerek yok).

## 4. Gerçek yayın uygulamasına dönüşüm adımları

### Adım 1 – Gerçek sahne tanımları (veritabanı yok)
Veritabanı yöneticisinden `tbl_gui`, `tbl_gui_elements`, `tbl_gui_dir`, `tbl_gui_controller`, `tbl_gui_controller_target`,
`tbl_playlist_group`, `tbl_saved_scenes`, `tbl_gui_values` tablolarının JSON dışa aktarımını iste; `data\` klasörüne koy.
Bütün tip tanımları ve düğmeler gerçek olur, 900 üstü geçici numaralar biter, 1–2 numaralı varsayımlar tablodan okunur.

### Adım 2 – Kalan logları toplamak
Eski uygulamada şu düğmelere basıp Engine konsolunu yapıştır: haber açıkken özel haber, alt bant AL, muhabir KJ, kamera KJ,
EVDE KAL, KISA KISA, Gece Hattı modunda bir KJ. Her log bir varsayımı kapatır.

### Adım 3 – Haber akışı için canlı veri
Yöneticiden senin makinenden bağlanabilen, yalnızca SELECT yetkili bir MySQL hesabı iste. Workbench ile dene, sonra
`appsettings.json` → `UseJsonData: false` + bağlantı dizeleri. Uygulama oturumu `READ ONLY` yaptığı için yazma imkânsız.
İlk bağlantıda sütun adı uymayan sorgu logda görünür; `MySqlShowRepository.cs` içinde düzeltilir.
Alternatif: yöneticiden dump alıp yerel MySQL'e yüklemek, canlıya hiç dokunmadan test etmek.

### Adım 4 – Sağlamlaştırma
- Sahne yükleme ve ön yüklemeyi arka plan iş parçacığına al (arayüz donmasın).
- Komutları işlem başına tek pakette gönder (`VizEngineClient`'a batch).
- Bağlantı kopunca otomatik yeniden bağlanma, Engine hatalarında yayını kilitlemeyen davranış.
- Katman başına meşgul kilidi.
- Ana sahne KJ'lerini de `GenericScenePublisher` üzerinden, `tbl_gui` tanımıyla oynatma seçeneği (tek motor).

### Adım 5 – Yayına alma
- Yayın Engine adresini ve iki kelebek makinesini ayarlara yaz; Artist değil gerçek Engine'e bağlan.
- Yayın bilgisayarında Release derlemesi, exe yanında `appsettings.json` ve `data\`.
- Eski uygulama ile bir süre paralel çalıştır (aynı Engine'e ikisi birden bağlanabilir); fark görülen düğmeyi logla düzelt.

## 5. Kurallar

- Eski WPF uygulamasına ve canlı veritabanına dokunulmaz; sadece log ve kaynak okunur.
- Uygulama hiçbir zaman veritabanına yazmaz. MySQL oturumu `SET SESSION TRANSACTION READ ONLY` ile açılır.
- `appsettings.json`, `log.txt`, `data\` kaynak kontrolüne girmez.
