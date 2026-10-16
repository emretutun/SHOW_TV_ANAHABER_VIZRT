# Eski uygulama (ShowTVReji, WPF) – kaynak koddan öğrenilenler

Kaynak: `C:\Users\metutun\Desktop\show_reji` (SADECE OKUNDU; derlenmedi, çalıştırılmadı, değiştirilmedi).
Çözüm: `ShowTV.sln` → `ShowTVReji` (WPF arayüz, ince) + `NewscastClient` (tüm mantık: Viz komutları, DB, yayın akışı) + `NewscastClient.Presentation` + `DotnetLibraries/CBG.*`.

> Ayar dosyalarında canlı veritabanı bağlantı bilgileri var. Buraya ve sohbete asla kopyalanmayacak.

## 1. Genel mimari: her şey veritabanından gelir

Eski uygulama sahneye özel kod içermiyor. "Sahne tipi" (GUI) tanımları, alan eşlemeleri, director adları,
çakışma kuralları ve düğme listeleri veritabanında. Uygulama bunları okuyup genel bir yayın motoruyla çalıştırıyor.
Bizim loglarda gördüğümüz tüm komut dizileri bu motorun ürettiği çıktı.

İki veritabanı (aynı MySQL sunucusu):
- **showtv_vizrt** (NewscastConnection): sahne tipi tanımları, kontrolcüler, playlist.
- **showtv** (NewscastNewsConnection): haberler, akışlar, habere bağlı kayıtlı sahneler ve değerleri.
- (showtv_vizrtlog: log; bize gerekmez.)

## 2. Tablolar

### showtv_vizrt
| Tablo | İçerik | Önemli sütunlar |
|---|---|---|
| `tbl_gui` | Sahne tipi (GUI) tanımı | `gui_id`, `gui_name`, `dir_id` (kategori), `in_director_name`, `out_director_name`, `intra_director_name`, `intra_director_delay`, `scene_reset_enabled` (STAGE START), `scene_full_path`, `conflicting_gui_id` (virgüllü liste), `target_layer` (0 front, 1 main, 2 back), `status` |
| `tbl_gui_elements` | Tipin doldurulan alanları | `gui_element_id`, `gui_id`, `element_name` (container adı, örn. edt_txt_haber), `element_type`, `element_gui_title` ("Değer #1" başlığı), `element_default_value`, `max_size`, `sira_no`, `use_default`, `status` |
| `tbl_gui_director` | Tipin elle oynatılabilir director'ları | `director_id`, `gui_id`, `director_name`, `director_title`, `director_length`, `status` |
| `tbl_gui_plugin` | Tipe bağlı eklentiler | `gp_id`, `gui_id`, … |
| `tbl_gui_dir` | Sahne tipi kategorileri (ağaç) | `dir_id`, `parent_dir_id`, `dir_name`, `status` |
| `tbl_gui_controller` | Düğme/liste tanımları ("kontrolcü") | `def_id`, `controller_name`, `gui_name` (identifier), `gui_id`, `controller_description`, `is_extended`, `target_index`, `status` |
| `tbl_playlist_group`, `tbl_gui_values` | Playlist ve playlist sahne değerleri | – |

### showtv
| Tablo | İçerik | Önemli sütunlar |
|---|---|---|
| `tbl_records` | Haberler | `auto_id`, `haber_id`, `baslik`, `haber` (metin), `kam_spk`, `sira`, `akis_haber_id`, `yayin_tarihi`, `dir_id`, `saat`, `status` |
| `tbl_yayin_saatleri` | Akışlar (tarih + saat + klasör) | `id`, `yayin_tarihi`, `dir_id`, `saat_id`, `status` |
| `def_yayin_saatleri` | Saat tanımları | `id`, `saat` |
| `tbl_dir_structure` | Klasör ağacı (akış klasörleri) | `id`, `pid`, `name` |
| `viz_saved_scenes` | Habere bağlı kayıtlı sahneler (orta paneldeki satırlar) | `saved_scene_id`, `scene_gui_id`, `scene_haber_id`, `description`, `container_id`, `sira_no`, `modification_time`, `scene_source`, `scene_metadata`, `status` |
| `viz_gui_values` | Kayıtlı sahnenin alan değerleri (Değer #1, #2…) | `value_id`, `gui_element_id`, `saved_scene_id`, `value`, `parameterValue`, `status` |

Eski uygulama bazı okumaları saklı yordamla yapıyor (`sp_GetNewsSavedScenes`, `sp_GetRundownsAtDate`…).
Bizim yetkimiz sadece SELECT olduğu için aynı veriyi tabloları birleştirerek okuyacağız (aşağıdaki sorgular).

## 3. Yalnızca SELECT ile aynı veriyi okuyan sorgular

Sahne tipleri (sağ liste):
```sql
SELECT gui_id, gui_name, dir_id, in_director_name, out_director_name, intra_director_name,
       intra_director_delay, scene_reset_enabled, scene_full_path, conflicting_gui_id, target_layer
FROM showtv_vizrt.tbl_gui
WHERE status = 1
ORDER BY gui_name;
```

Bir tipin alanları:
```sql
SELECT gui_element_id, element_name, element_type, element_gui_title, element_default_value, max_size, sira_no
FROM showtv_vizrt.tbl_gui_elements
WHERE gui_id = ? AND status = 1
ORDER BY sira_no, gui_element_id;
```

"Farklı KJ tipinde ver" düğmeleri ve logo listesi (kontrolcü adı program moduna göre: `ShowTVRejiAnaHaber`):
```sql
SELECT c.def_id, c.gui_name AS identifier, c.gui_id, c.controller_description, g.gui_name, g.target_layer
FROM showtv_vizrt.tbl_gui_controller AS c
LEFT JOIN showtv_vizrt.tbl_gui AS g ON g.gui_id = c.gui_id
WHERE c.controller_name = 'ShowTVRejiAnaHaber' AND c.status = 1
ORDER BY c.gui_name;
```
- `identifier` `HaberKJ…` ile başlayanlar KJ tipi düğmeleri, `HaberLogo…` ile başlayanlar logo listesi.

Akışlar (tarihe göre; eski uygulamadaki yorum satırındaki SQL):
```sql
SELECT ys.id, ys.yayin_tarihi, ys.dir_id, ys.saat_id, dys.saat AS time_of_day,
       d2.name AS dir_name, d1.id AS parent_dir_id, d1.name AS parent_dir_name
FROM showtv.tbl_yayin_saatleri AS ys
INNER JOIN showtv.def_yayin_saatleri AS dys ON ys.saat_id = dys.id
INNER JOIN showtv.tbl_dir_structure AS d2 ON ys.dir_id = d2.id
INNER JOIN showtv.tbl_dir_structure AS d1 ON d2.pid = d1.id
WHERE ys.status = 1 AND ys.yayin_tarihi = ?
ORDER BY dys.saat;
```

Haber listesi (seçili akış):
```sql
SELECT r.auto_id, r.haber_id, r.baslik, r.haber, r.kam_spk, r.sira
FROM showtv.tbl_records AS r
WHERE r.akis_haber_id IS NOT NULL AND r.status = 1
  AND r.yayin_tarihi = ? AND r.dir_id = ? AND r.saat = ?
ORDER BY r.sira, r.auto_id;
```

Seçili haberin sahne satırları + değerleri:
```sql
SELECT s.saved_scene_id, s.scene_gui_id, g.gui_name, s.description, s.sira_no,
       gv.gui_element_id, e.element_name, e.element_gui_title, gv.value
FROM showtv.viz_saved_scenes AS s
LEFT JOIN showtv_vizrt.tbl_gui AS g ON g.gui_id = s.scene_gui_id
LEFT JOIN showtv.viz_gui_values AS gv ON gv.saved_scene_id = s.saved_scene_id AND gv.status = 1
LEFT JOIN showtv_vizrt.tbl_gui_elements AS e ON e.gui_element_id = gv.gui_element_id
WHERE s.scene_haber_id = ? AND s.status = 1
ORDER BY s.sira_no, s.saved_scene_id, e.sira_no;
```
(`scene_source` / `scene_metadata` sütunları yeni tip kayıtlarda değerleri JSON olarak taşıyabilir; ilk sorguda kontrol edilecek.)

## 4. Yayın motorunun kuralları (NewscastVizContext.cs)

Bir kayıtlı sahne "ver" denince:
1. Katmanda aynı GUI'den bir sahne zaten varsa ve **intra director'ları aynıysa** → intra director oynatılır,
   `intra_director_delay` kadar beklenir, sonra alan değerleri yazılır. (Bizim "aynı türde ikinci başlık" dalı.)
2. Yeni GUI eski GUI ile `conflicting_gui_id` listesinde çakışıyorsa → eskinin **out director'ı** oynatılır, süresi beklenir,
   sonra yeninin in director'ı. "Örtük kaldırma" işaretli çakışmalarda out oynatılmadan silinir.
3. Çakışma yoksa yan yana durabilir (örneğin haber KJ + logo).
4. Katman boşsa ve `scene_reset_enabled` ise önce `STAGE START`.
5. Her director için `END_TIME GET` okunur, en uzunu kadar beklenir (bizim BusyUntil).

Yani **tür değişimi** (haber açıkken son dakika) sorusunun cevabı `tbl_gui.conflicting_gui_id` değerlerinde.
Büyük ihtimalle KJ tipleri birbirleriyle çakışık işaretli → eski türün out'u + yeni türün in'i.

"Farklı KJ tipinde ver": seçili satırın alan değerleri hedef tipin sahnesine kopyalanıp o tip yayınlanır
(NewsSceneControlAdapter.CommandKJFarkliVer_Invoke). Alan sayıları eşit olmalı.

"Tümünü Al": katmandaki tüm kayıtlı sahnelerin out director'ları (bizim OutAll ile aynı).

Flotlar: `tbl_gui` içinde `scene_full_path = SHOW_HABER/HABER2016/FLOT-xx`, `target_layer = 2` (arka katman) olan tipler.
Metin alanı tanımlı değil; sahne olduğu gibi oynatılır.

## 4b. Kelebekler (KelebekControlView / KelebekKJTargetViewModel)

- İki panel yan yana: **Viz #1** ve **Viz #2** (kontrolcü hedefleri `tbl_gui_controller_target`, target_index 0 ve 1).
- Her panel sabit bir **playlist**in kayıtlı sahnelerini listeler: ayarlardan `PlaylistKelebek1 = 25`, `PlaylistKelebek2 = 31`
  (`tbl_playlist_group.group_id`; sahneler `tbl_saved_scenes` + değerler `tbl_gui_values`).
- Yeni sahne sadece **kelebek kategorisindeki** tiplerden seçilir: `GuiDirIdKelebek = 14` (`tbl_gui.dir_id`).
- Düğmeler: TÜMÜNÜ HAZIRLA (tüm sahneleri belleğe al), YENİLE, SİL, YENİ, KAYDET, VER (Ctrl+Q), GÜNCELLE (Ctrl+G), AL (Ctrl+W).
- `UnpublishLastScene = true`: yeni kelebek verilince ekrandaki eski kelebek kaldırılır.
- **LOG İLE DOĞRULANDI (2026-10-16), pencere sahneleri (PENCERE_2, PENCERE_DSF_3, PENCERE_MUHABIR_SAG_DSF, PENCERE_MUHABIR_SOL_DSF):**
  ```
  1  RENDERER*BACK_LAYER GET_OBJECT
  1  SCENE*SHOW_HABER/HABER2016/PENCERE_2 LOAD
  -1 RENDERER*BACK_LAYER SET_OBJECT SCENE*SHOW_HABER/HABER2016/PENCERE_2
  -1 RENDERER*BACK_LAYER*STAGE SHOW 0.0
  -1 RENDERER*BACK_LAYER*TREE*$edt_txt_2li_pencere_sehir*GEOM*TEXT SET ANKARA
  -1 RENDERER*BACK_LAYER*TREE*$edt_txt_2li_pencere_muhabir*GEOM*TEXT SET KEMAL AKAGÜNDÜZ
  -1 RENDERER*BACK_LAYER*STAGE START                  <- in director YOK; stage girişi oynatıp durakta bekler
  ```
  AL: `RENDERER*BACK_LAYER*STAGE STOP` + `RENDERER*BACK_LAYER SET_OBJECT` (out director YOK, katman boşaltılır).
  Yani tbl_gui'de: in/out boş, target_layer = 2. Metin container'ları: PENCERE_2 → edt_txt_2li_pencere_sehir, edt_txt_2li_pencere_muhabir;
  PENCERE_DSF_3 → edt_txt_3lu_pencere_{sol,sag}_{sehir,muhabir}; MUHABIR_SAG_DSF → edt_txt_dsf_sol_muhabir; MUHABIR_SOL_DSF → edt_txt_dsf_sag_muhabir.
- Yeni uygulamada: `Ui/KelebekPanel`, yayın mantığı `Viz/GenericScenePublisher` (tbl_gui tanımına göre in/out/intra, katman, alanlar).
  SİL ve KAYDET yok (yazma yetkisi yok); "Yeni" sadece bu oturumda, ekranda.

## 4c. Yeni uygulamada yapılan yerler (2026-10-16)

- `Config/AppSettings.cs` → exe yanında `appsettings.json`: Engine adresleri, kontrolcü adı, kelebek playlist/kategori id'leri,
  **boş** veritabanı bağlantı dizeleri (kullanıcı kendi SELECT yetkili hesabını yazacak). Ayarlar düğmesi bu dosyayı düzenler.
- `Data/Models.cs`: SceneType, SceneTypeElement, SavedSceneItem, ControllerItem, ControllerTarget, EngineDefinition, Rundown, NewsStory, Playlist.
- `Data/IShowRepository.cs`: salt okunur arayüz + `NullShowRepository` (veritabanı yokken).
- `Data/MySqlShowRepository.cs`: bölüm 3'teki SELECT'ler (MySql.Data paketi). Sütun adları canlı tabloyla ilk denemede doğrulanacak.
- `Viz/GenericScenePublisher.cs`: tbl_gui tanımlı herhangi bir tipi oynatan genel motor (kelebekler bununla çalışır; ileride KJ'ler de buna taşınabilir).

## 4d. JSON veri kaynağı (veritabanı yerine) — 2026-10-16

Karar: canlı veritabanına hiç bağlanılmayacak (orada log tutuluyor). Bunun yerine exe yanındaki `data` klasöründe,
gerçek tablolarla **aynı ad ve aynı sütun adlarında** JSON dosyaları kullanılır (`Data/JsonShowRepository.cs`):

`tbl_gui`, `tbl_gui_elements`, `tbl_gui_dir`, `tbl_gui_controller`, `tbl_gui_controller_target`, `tbl_playlist_group`,
`tbl_saved_scenes`, `tbl_gui_values`, `tbl_yayin_saatleri`, `tbl_records`, `viz_saved_scenes`, `viz_gui_values`.

- Dosya yoksa `Data/JsonSampleData.cs` bildiğimiz tanımlarla örnekleri üretir (gerçek gui_id'ler: 122, 151, 152, 153, 154, 157, 158, 161, 162, 163; 900+ geçici).
- `yayin_tarihi` boş bırakılırsa akış/haber her tarihte görünür (test kolaylığı).
- Ayar: `appsettings.json` → `UseJsonData` (varsayılan true), `DataFolder` ("data").
- İleride gerçek tablolardan JSON dışa aktarılırsa aynı dosyaların yerine konur; MySQL deposu da kodda hazır durur.

## 5. Bize ne lazım

1. Yukarıdaki SELECT'leri MySQL aracında çalıştırıp sonuç örneklerini bakmak (özellikle `tbl_gui` ve `tbl_gui_controller`).
2. Uygulamada salt okunur bir `MySqlRepository`: akışlar → haberler → sahne satırları; sahne tipleri → KJ düğmeleri ve logo listesi.
3. `KjCatalog` ve `GraphicCatalog` sabit tablolarını `tbl_gui` verisiyle besleme seçeneği (elle tablo yedek kalır).
