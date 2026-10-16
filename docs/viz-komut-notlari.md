# Show Haber – Viz Engine komut notları

Yayındaki eski otomasyonun Engine konsolunda görülen komutlarından çıkarılmıştır.
Sahne: `SHOW_HABER/HABER2016/ShowHaber2016-2` (Viz Artist/Engine 3.14.4).

Kurallar:
- Okuma komutları kimlik `1` ile gönderilir, cevap beklenir.
- Yazma/oynatma komutları kimlik `-1` ile gönderilir, cevap beklenmez.
- Container'lara adla erişilir: `RENDERER*MAIN_LAYER*TREE*$<ad>`.
- Director'lara `RENDERER*MAIN_LAYER*STAGE*DIRECTOR*<ad>` ile erişilir (`$` yok).
- Komut dili: yıldızla ayrılan parçalar ADRES, boşluktan sonraki kelime KOMUT (`GET`, `SET x`, `START`, `CONTINUE`, `SHOW 0.0`, `STOP`).
- Periyodik nabız: `TEXTURE*MEMORY GET`.
- `RENDERER*MAIN_LAYER` komutları Artist editör modunda reddedilir ("not allowed in this mode"); On Air modunda ve gerçek Engine'de çalışır.

## 1. Bağlanma ve sahne kontrolü (her işlem öncesi)

| # | Komut | Cevap / amaç |
|---|---|---|
| 1 | `TEXTURE*MEMORY GET` | nabız |
| 2 | `RENDERER*MAIN_LAYER GET_OBJECT` | `#76188` – ana katmandaki nesne |
| 3 | `SCENE*#76188*LOCATION_PATH GET` | `SCENE*#76188` → yolu yok, bizim sahne değil |
| 4 | `SCENE*SHOW_HABER/HABER2016/ShowHaber2016-2 LOAD` | sahneyi belleğe yükler, cevap `#76538` (bekler, saniyeler sürer) |
| 5 | `-1 RENDERER*MAIN_LAYER SET_OBJECT SCENE*SHOW_HABER/HABER2016/ShowHaber2016-2` | sahneyi ana katmana koyar |
| 6 | `-1 RENDERER*MAIN_LAYER*STAGE SHOW 0.0` | tüm stage'i başa sarar |
| 7 | `SCENE*<yol>*STAGE*DIRECTOR*<dir>*END_TIME GET` | kullanılacak director'ların süresi (sn) |

Sahne zaten yüklüyse 3. adımın cevabı `SCENE*SHOW_HABER/HABER2016/ShowHaber2016-2` olur ve 4–6 atlanır.

## 2. Alt bant (KJ) – ilk giriş (ekranda KJ yokken)

Tüm türler aynı gizli metni ve aynı grubu kullanır; sadece director adı değişir.

```
-1 RENDERER*MAIN_LAYER*TREE*$edt_txt_haber*GEOM*TEXT SET <başlık>
-1 RENDERER*MAIN_LAYER*TREE*$sub_edt_txt_haber_group*ACTIVE SET 1
-1 RENDERER*MAIN_LAYER*STAGE START
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*<IN_DIRECTOR> SHOW 0.0
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*<IN_DIRECTOR> CONTINUE
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_in_haberlogo SHOW 0.0
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_in_haberlogo CONTINUE
[son dakika için ek:]
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_in_sondakikalogo SHOW 0.0
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_in_sondakikalogo CONTINUE
```

| Tür (butondaki ad) | IN director | INTRA director | OUT director | Ek logo | IN süresi |
|---|---|---|---|---|---|
| Haber (normal metin) | `dir_in_haberkj` | `dir_intra_haberkj` | `dir_out_haberkj` | haberlogo | 2.0 |
| Siyah metin | `dir_in_yaskj` | `dir_intra_yaskj` | `dir_out_yaskj` | haberlogo | 2.0 |
| Özel haber | `dir_in_ozelhaberkj` | `dir_intra_ozelhaberkj` | `dir_out_ozelhaberkj` | haberlogo | 2.0 |
| Sıcak haber | `dir_in_sicakhaberkj` | `dir_intra_sicakhaberkj` | `dir_out_sicakhaberkj` | haberlogo | 2.0 |
| Son dakika | `dir_in_sondakikakj` | `dir_intra_sondakikakj` | `dir_out_sondakikakj` | haberlogo + sondakikalogo | 2.0 |
| Az sonra | `dir_in_azsonrakj` | (yok) | `dir_out_azsonrakj` | ? (log yok) | ? |

Director süreleri: `dir_in_haberlogo` 1.1 sn, `dir_in_sondakikalogo` 0.92 sn, `dir_intra_*` 0.52 sn, `dir_out_haberkj` 0.42 sn.

Notlar:
- `STAGE START` tüm stage'i çalıştırır; LOOP director'ları ancak bundan sonra döner.
- Eski uygulama her butondan önce ilgili director'ların `END_TIME` değerini okur (animasyon süresince kilit için).

## 3. Alt bant – ikinci başlık (aynı tür ekrandayken) — LOG İLE DOĞRULANDI (haber)

```
-1 RENDERER*MAIN_LAYER*TREE*$edt_txt_haber*GEOM*TEXT SET <başlık>
-1 RENDERER*MAIN_LAYER*TREE*$sub_edt_txt_haber_group*ACTIVE SET 1
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_intra_haberkj SHOW 0.0
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_intra_haberkj CONTINUE
```

`dir_intra_*` sahne içindeki action keyframe'lerle gizli metni küpün arka yüzüne kopyalar, küpü döndürür,
sonra ön yüzü de günceller ve küpü başa alır (test edildi: her iki yüzün metni yeni başlık oluyor, küp 90° konumuna dönüyor).
Uygulamanın yüz takibi yapması gerekmez.

## 3b. Az sonra (alttan akan yazı) — LOG İLE DOĞRULANDI

Alt bant değil, akan yazıdır. Metnin tamamı akınca sahne kendisi kaldırır; OUT basılmaz.

```
-1 RENDERER*MAIN_LAYER*TREE*$edt_txt_akanyazi*GEOM*TEXT SET <uzun metin>
-1 RENDERER*MAIN_LAYER*TREE*$sub_edt_txt_akanyazi_group*ACTIVE SET 1   <- sahnede böyle bir container YOK, Engine "invalid container name" der; eski uygulama yine de gönderiyor, biz göndermiyoruz
-1 RENDERER*MAIN_LAYER*STAGE START
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_in_azsonrakj SHOW 0.0
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_in_azsonrakj CONTINUE
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_in_haberlogo SHOW 0.0
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_in_haberlogo CONTINUE
```

`dir_in_azsonrakj` END_TIME 3.4 sn (sadece giriş; akış süresi metin uzunluğuna bağlı, TrioScrollElement plugini sürüyor).

Az sonra AL (bitmesini beklemeden kesme) — LOG İLE DOĞRULANDI:

```
1  SCENE*<yol>*STAGE*DIRECTOR*dir_out_azsonrakj*END_TIME GET     -> 0.42
1  SCENE*<yol>*STAGE*DIRECTOR*dir_out_haberlogo*END_TIME GET     -> 1.12
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_out_azsonrakj SHOW 0.0
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_out_azsonrakj CONTINUE
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_out_haberlogo SHOW 0.0
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_out_haberlogo CONTINUE
```

Yani OUT ile birlikte haber logosu da iniyor. Alt bant OUT'unun da aynı olduğu varsayılıyor.

## 3c. Bağlanma ("Tekrar Bağlan") ve sahneyi boşaltma — LOG İLE DOĞRULANDI

Bağlanınca sahne zaten yüklüyse bile `-1 RENDERER*MAIN_LAYER*STAGE SHOW 0.0` gönderiliyor (stage başa sarılıyor).

Sahneyi boşaltma (uygulama kapanışı ya da "Belleği Temizle"; hangisi olduğu henüz net değil):

```
1 RENDERER*MAIN_LAYER*STAGE STOP
1 RENDERER*MAIN_LAYER SET_OBJECT          <- boş nesne = ana katmanı boşalt
```

## 3d. Gerçek uygulamanın arayüzü (ekran görüntüsünden)

"Show TV Reji (ANA HABER)" – hedef arayüz. Bileşenler:
- Üst araç çubuğu: Tekrar Bağlan, Sahneleri Yükle, **Tümünü Al**, Belleği Temizle, Ayarlar, Program Modu (Show Ana Haber).
- Sekmeler: Haberler, Listeler, Kelebekler.
- Tarih + Akış seçimi (örn. "18:25 HABER/ONAYLANANLAR") + Akış Yenile. → DB'den gelir.
- Sol: HABERLER listesi (haber başlıkları, kısaltma ekleri "-ULŞ", "-EC" muhabir/editör kodları gibi). → DB.
- Orta: SAHNE LİSTESİ (Sıra, Tip, Değer #1, Değer #2): seçili habere bağlı grafik satırları. Altında KAYDET, KAYDET-VER, VER (Ctrl+Q), AL (Ctrl+W), TÜMÜNÜ ANİMASYONLA AL (Ctrl+E).
- FARKLI KJ TİPİNDE VER: Normal Metin, Özel Haber, Sıcak Haber, Siyah Metin, Son Dakika. → bizim KjType.
- Logo listesi (Bir İnsan Hikayesi, Bu Habere Dikkat, Herkes Bunu Konuşuyor, Neler Oldu Neler, Özel Haber, ...) + LOGO VER / LOGO AL.
- Sol alt: CANLI VER/AL, YER (metin) + YER VER/AL, CANLI/YER VER/AL.
- Sağ: SAHNE TİPLERİ ağacı ve tip listesi: KJ İsim (122), KJ Metin (151), Az Sonra Kapak (152), Son Dakika (153), KJ Yer (154), Logo - Herkes Bunu Konuşuyor (1..), Logo - Özel Haber (157), Telefon Kapak (158), Sıcak Haber Kapak (161), Özel Haber Kapak (162), Logo - Sokak Ne Diyor (163)... → parantez içi sayılar muhtemelen DB'deki tip id'leri.
- Sağ üst: HABER GENEL / HABER FLOT sekmesi, logo sahneleri listesi (Show Haber Logo, Özel Haber Logo, Son Dakika Logo, Sıcak Haber Logo, Herkes Bunu Konuşuyor Logo, Sokak Ne Diyor Logo, EVDE KAL, Bir İnsan Hikayesi Logo, KISA KISA...) + VER / AL / TÜMÜNÜ YÜKLE.
- Durum çubuğu: VizRT doluluk yüzdesi (TEXTURE*MEMORY cevabından: ALLOCATED/TOTAL ≈ %39,6).

## 3e. Yer KJ — TEST ENGINE'DE DOĞRULANDI (eski uygulama logu değil, kendi denememiz)

```
-1 RENDERER*MAIN_LAYER*TREE*$edt_txt_haber_yer*GEOM*TEXT SET <yer>
-1 RENDERER*MAIN_LAYER*STAGE START
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_in_yerkj SHOW 0.0
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_in_yerkj CONTINUE
```

Çıkış: `dir_out_yerkj` SHOW 0.0 + CONTINUE. `dir_in_yerkj` END_TIME 0.62 sn. Grup açmak gerekmedi.
Not: editör modunda `STAGE START` reddedilir ("not allowed in this mode"); komutla oynatmak için Artist On Air olmalı.

## 3f. Canlı ve Canlı/Yer — LOG İLE DOĞRULANDI (CANLI/YER VER)

```
-1 RENDERER*MAIN_LAYER*TREE*$edt_txt_canli_yer*GEOM*TEXT SET <yer>
-1 RENDERER*MAIN_LAYER*TREE*$sub_edt_txt_canli_yer_group*ACTIVE SET 1   <- sahnede yok, Engine hata verir; biz göndermiyoruz
-1 RENDERER*MAIN_LAYER*STAGE START
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_in_canli SHOW 0.0
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_in_canli CONTINUE
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_in_canli_yerkj SHOW 0.0
-1 RENDERER*MAIN_LAYER*STAGE*DIRECTOR*dir_in_canli_yerkj CONTINUE
```

- Sadece CANLI etiketi: `dir_in_canli` (0.78 sn) / `dir_out_canli` (1.1 sn). Test Engine'de doğrulandı.
- `dir_in_canli_yerkj` (1.6 sn) tek başına yalnızca yer metnini getirir, CANLI etiketini getirmez; bu yüzden ikisi birlikte oynatılır.
- CANLI/YER AL: VARSAYIM `dir_out_canli_yerkj` + `dir_out_canli` (log bekleniyor).

## 3g. Logolar, sosyal medya, isim KJ'leri — LOG İLE DOĞRULANDI (2026-10-14)

Hepsi aynı kalıp: `[metin SET]` + `STAGE START` + `dir_in_X SHOW 0.0` + `CONTINUE`; çıkış `dir_out_X SHOW 0.0` + `CONTINUE`.

| Grafik | IN director | OUT director | Metin container | IN / OUT süre (sn) |
|---|---|---|---|---|
| Show Haber Logo | `dir_in_haberlogo` | `dir_out_haberlogo` | – | 1.10 / 1.12 |
| Özel Haber Logo | `dir_in_ozelhaberlogo` | `dir_out_ozelhaberlogo` | – | 0.86 / 1.10 |
| Son Dakika Logo | `dir_in_sondakikalogo` | `dir_out_sondakikalogo` | – | 0.92 / 0.92 |
| Sıcak Haber Logo | `dir_in_sicak_haber_logo` | `dir_out_sicak_haber_logo` | – | 0.70 / 1.50 |
| Herkes Bunu Konuşuyor | `dir_in_hbk` | `dir_out_hbk` | – | 2.26 / 2.28 |
| Sokak Ne Diyor | `dir_in_sbnd` | `dir_out_sbnd` | – | 1.36 / 1.38 |
| Neler Oldu Neler | `dir_in_neler` | `dir_out_neler` | – | 1.52 / 1.52 |
| Bir İnsan Hikayesi | `dir_in_bih` | `dir_out_bih` | – | 1.14 / 1.12 |
| Bu Habere Dikkat | `dir_in_bhd` | `dir_out_bhd` | – | 0.86 / 0.86 |
| Facebook / Twitter | `dir_in_facebook_twitter` | `dir_out_facebook_twitter` | – | 1.62 / 1.38 |
| Whatsapp ihbar | `dir_in_whatsapp` | `dir_out_whatsapp` | `edt_txt_whatsapp_tel` ("0536 2669000") | 2.36 / 2.38 |
| BiP ihbar | `dir_in_bip` | `dir_out_bip` | `edt_txt_bipno` ("0536 266 90 02") | 1.24 / 1.24 |
| Spiker KJ | `dir_in_spikerkj` | `dir_out_spikerkj` | `edt_txt_spiker` | 1.46 / ? |
| Muhabir KJ (VARSAYIM) | `dir_in_muhabirkj` | `dir_out_muhabirkj` | `edt_txt_muhabir` | ? |
| Kamera KJ (VARSAYIM) | `dir_in_kamerakj` | `dir_out_kamerakj` | `edt_txt_kamera` | ? |

Notlar:
- Whatsapp, BiP ve spiker için eski uygulama var olmayan `sub_edt_txt_*_group` container'ını açmaya çalışıyor (Engine hata verir); biz göndermiyoruz.
- Spiker adı değişince intra yok: metin SET + `dir_in_spikerkj` tekrar oynatılıyor.
- **Tümünü Al**: ekranda olduğu bilinen her grafiğin `END_TIME`'ı okunur, sonra hepsinin out director'u art arda oynatılır
  (logda: `dir_out_haberlogo`, `dir_out_bhd`, `dir_out_facebook_twitter` birlikte). Yani eski uygulama da durum listesi tutuyor.
- EVDE KAL ve KISA KISA henüz loglanmadı.

## 3h. Flot sahneleri (arka katman) — LOG İLE DOĞRULANDI (2026-10-16), uygulandı

FLOT-01 … FLOT-20 (ve klasördeki isimli flotlar) `SHOW_HABER/HABER2016/` altında ayrı sahnelerdir; **arka katmanda** oynar,
ana katmandaki ShowHaber sahnesine ve KJ'lere dokunmaz.

FLOT VER:
```
1  RENDERER*BACK_LAYER GET_OBJECT                        -> boş ya da önceki flot
1  SCENE*SHOW_HABER/HABER2016/FLOT-01 LOAD               -> #id (bekler)
-1 RENDERER*BACK_LAYER SET_OBJECT SCENE*SHOW_HABER/HABER2016/FLOT-01
-1 RENDERER*BACK_LAYER*STAGE SHOW 0.0
1  SCENE*<yol>*STAGE*DIRECTOR*dir_in_flot*END_TIME GET  -> sahneye göre değişir (FLOT-01: 0, FLOT-20: 2.58)
-1 RENDERER*BACK_LAYER*STAGE START
-1 RENDERER*BACK_LAYER*STAGE*DIRECTOR*dir_in_flot SHOW 0.0
-1 RENDERER*BACK_LAYER*STAGE*DIRECTOR*dir_in_flot CONTINUE
```

FLOT AL:
```
1  RENDERER*BACK_LAYER GET_OBJECT                        -> #id
1  SCENE*#id*LOCATION_PATH GET                           -> SCENE*SHOW_HABER/HABER2016/FLOT-01
1  SCENE*<yol>*STAGE*DIRECTOR*dir_out_flot*END_TIME GET -> 2.58
-1 RENDERER*BACK_LAYER*STAGE*DIRECTOR*dir_out_flot SHOW 0.0
-1 RENDERER*BACK_LAYER*STAGE*DIRECTOR*dir_out_flot CONTINUE
```

FLOT AL sonrası (2. log, 2026-10-16): çıkış animasyonu bitince arka katman boşaltılıyor:
```
1  RENDERER*BACK_LAYER*STAGE STOP
1  RENDERER*BACK_LAYER SET_OBJECT          <- boş = arka katmanı boşalt
```

- Aynı flot arka katmanda zaten yüklüyse (GET_OBJECT + LOCATION_PATH eşleşir) LOAD/SET_OBJECT atlanır; sadece STAGE SHOW 0.0, END_TIME, STAGE START, dir_in_flot.
- Bir flot açıkken başka flot verilince eski uygulama çıkış yapmadan yeni sahneyi doğrudan arka katmana yüklüyor.
- Her flot sahnesinin director adları aynı (`dir_in_flot` / `dir_out_flot`), süreleri farklı; bu yüzden süre önbelleğe alınmaz.
- Kod: `ShowHaberController.TakeFlot / OutFlot`, arayüzde "Haber Flot" sekmesi.

## 3h-2. ROLLCAPTION — LOG İLE GÖRÜLDÜ, henüz uygulanmadı

`SHOW_HABER/HABER2016/ROLLCAPTION` ana katmana yüklenir (ShowHaber'in yerine): `LOAD`, `SET_OBJECT`, `STAGE SHOW 0.0`, `STAGE START`.
Sonra ShowHaber'e dönüldüğünde sahne kontrolü ShowHaber'i yeniden yükler.

## 3i. Kulakçık kuralı (bizim kararımız, eski uygulamadan farklı)

Eski uygulama loglarında özel haber ve sıcak haber KJ'si yalnızca haber logosunu getiriyordu; son dakika ek olarak
son dakika logosunu getiriyordu. Yeni uygulamada kullanıcı isteğiyle her türün kulakçığı KJ ile birlikte yönetilir:

| Tür | Haber logosu | Kulakçık |
|---|---|---|
| Haber, Siyah haber | gelir | yok |
| Özel haber | gelir | `ozelhaberlogo` |
| Sıcak haber | gelir | `sicak_haber_logo` |
| Son dakika | gelir | `sondakikalogo` |
| Az sonra (akan yazı) | gelir | dokunulmaz |

- Tür değişince başka türün kulakçığı iner, yeni türünki gelir. Kulakçıklar birbirini dışlar.
- KJ OUT / Tümünü Al: haber logosu ve tüm kulakçıklar iner.
- Kod: `KjDefinition.EarLogoKey`, `ShowHaberController.PlayExtras`.

## 4. Henüz loglanmamış, kodda VARSAYIM olan davranışlar

- **Tür değişimi** (haber açıkken son dakika vermek): kodda yeni türün `dir_intra_*`'sı çalıştırılıyor. Log gelince kesinleşecek.
- **OUT**: kodda `dir_out_<tür>` + açık logoların `dir_out_*`'ları çalıştırılıyor. Log gelince kesinleşecek.
- **Az sonra, başka KJ açıkken**: eski uygulamanın ne yaptığı bilinmiyor; kodda KJ durumu değişmiyor, sadece akan yazı gönderiliyor.

## 5. Sahne yapısı (Engine'den okunan)

- Vizrt Transition Logic DEĞİL: Toggle/ControlText plugini yok. Tek büyük sahne, `dir_in/out/intra_*` director'ları ve action keyframe'ler.
- Küp (`$Cube`, `dir_in_tobleron_donus` altında): yüz `2` → `grp_on_*`, yüz `3` → `grp_arka_*`.
- Küp yüzlerindeki metinler: `sub_edt_txt_<tür>_on` / `sub_edt_txt_<tür>_arka` (haber, siyah_haber, ozel_haber, sondakika, sicak_haber).
- Gizli ana metinler: `edt_txt_haber` (`sub_edt_txt_haber_group` içinde), `edt_txt_spiker`, `edt_txt_muhabir`, `edt_txt_kamera`, `edt_txt_haber_yer`, `edt_txt_canli_yer`, `edt_txt_whatsapp_tel`, `edt_txt_akanyazi`.
- Tüm director'lar: `dir_in/out/intra_X` (X: haberkj, sondakikakj, ozelhaberkj, sicakhaberkj, yaskj, azsonrakj, yerkj, canli, canli_yerkj, spikerkj, muhabirkj, kamerakj, haberlogo, sondakikalogo, ozelhaberlogo, sicak_haber_logo, whatsapp, facebook_twitter, hbk, sbnd) + LOOP, HABERLOGO_LOOP, spiker/kamera/muhabir_loop, teaser'lar.

## 6. Loglanacaklar (sıra)

1. Haber açıkken son dakika / özel haber (tür değişimi)
2. KJ OUT
3. Sağ üst logolar (türlere özel)
4. Spiker, muhabir, kamera KJ
5. Yer KJ, canlı yer KJ
6. Whatsapp, sosyal medya, akan yazı, az sonra
