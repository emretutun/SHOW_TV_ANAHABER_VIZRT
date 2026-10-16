using System.Globalization;

namespace SHOW_TV_ANAHABER.Viz
{
    /// <summary>
    /// Sık kullanılan Viz Engine komutlarını metin olarak üreten yardımcılar.
    /// Burada hiçbir şey gönderilmez; sadece doğru komut dizisi oluşturulur.
    ///
    /// Komut dilinin iki parçası vardır:
    ///   ADRES  : yıldızla ayrılır            RENDERER*MAIN_LAYER*TREE*$edt_txt_haber*GEOM*TEXT
    ///   KOMUT  : boşluktan sonra gelir       GET | SET değer | START | CONTINUE | SHOW 0.0 | STOP
    ///
    /// Adres örnekleri:
    ///   RENDERER*MAIN_LAYER                      -> ana katmandaki sahne (On Air'de MAIN_SCENE ile aynı)
    ///   ...*TREE*$edt_txt_haber                  -> container'a adıyla erişim (ad sahnede benzersiz olmalı)
    ///   ...*TREE*/2/1/3/2                        -> container'a ağaçtaki indeks yoluyla erişim
    ///   ...*STAGE*DIRECTOR*dir_intra_haberkj     -> stage'deki bir director
    ///
    /// Bu adresleme, yayındaki eski uygulamanın Engine konsolunda görülen komutlarıyla birebir aynıdır.
    /// </summary>
    public static class VizCommands
    {
        /// <summary>Ana katman adresi. Gerçek Engine'de ve Artist On Air modunda çalışır.</summary>
        public const string MainLayer = "RENDERER*MAIN_LAYER";

        // ---- Bağlantı / sahne --------------------------------------------------

        /// <summary>Zararsız sorgu; eski uygulama bağlantı canlı mı diye bunu periyodik gönderiyor.</summary>
        public static string KeepAlive()
        {
            return "TEXTURE*MEMORY GET";
        }

        /// <summary>Ana katmandaki sahnenin nesne kimliğini verir, örn. "#50628".</summary>
        public static string GetMainLayerObject()
        {
            return MainLayer + " GET_OBJECT";
        }

        /// <summary>Nesne kimliğinden sahnenin Graphic Hub yolunu verir, örn. "SCENE*SHOW_HABER/HABER2016/ShowHaber2016-2".</summary>
        public static string GetScenePath(string objectId)
        {
            return "SCENE*" + objectId + "*LOCATION_PATH GET";
        }

        /// <summary>
        /// Sahneyi Graphic Hub'dan belleğe yükler (dosya, font, resim). Saniyeler sürer, cevap bekleyin.
        /// Cevap sahnenin nesne kimliğidir, örn. "#76538".
        /// </summary>
        public static string PreloadScene(string scenePath)
        {
            return "SCENE*" + scenePath + " LOAD";
        }

        /// <summary>Belleğe yüklü sahneyi ana katmana koyar. Örn: SHOW_HABER/HABER2016/ShowHaber2016-2</summary>
        public static string LoadMainScene(string scenePath)
        {
            return MainLayer + " SET_OBJECT SCENE*" + scenePath;
        }

        /// <summary>Tüm stage'i verilen saniyeye alır (0.0 = başa sar).</summary>
        public static string StageShow(double seconds)
        {
            return MainLayer + "*STAGE SHOW " + seconds.ToString("0.0##", CultureInfo.InvariantCulture);
        }

        /// <summary>Tüm stage'i çalıştırır; LOOP director'ları ancak bundan sonra döner.</summary>
        public static string StageStart()
        {
            return MainLayer + "*STAGE START";
        }

        /// <summary>Tüm stage'i durdurur (eski uygulama sahneyi boşaltmadan önce gönderiyor).</summary>
        public static string StageStop()
        {
            return MainLayer + "*STAGE STOP";
        }

        /// <summary>Ana katmanı boşaltır (sahneyi kaldırır). Eski uygulama: STAGE STOP + bu komut.</summary>
        public static string ClearMainLayer()
        {
            return MainLayer + " SET_OBJECT";
        }

        // ---- Katman bağımsız sürümler (flot sahneleri arka katmanda oynar) ----------------

        /// <summary>Arka katman adresi. Eski uygulama FLOT sahnelerini buraya yüklüyor; ana katmandaki KJ'ler etkilenmez.</summary>
        public const string BackLayer = "RENDERER*BACK_LAYER";

        public static string GetLayerObject(string layer)
        {
            return layer + " GET_OBJECT";
        }

        public static string LoadLayerScene(string layer, string scenePath)
        {
            return layer + " SET_OBJECT SCENE*" + scenePath;
        }

        public static string LayerStageShow(string layer, double seconds)
        {
            return layer + "*STAGE SHOW " + seconds.ToString("0.0##", CultureInfo.InvariantCulture);
        }

        public static string LayerStageStart(string layer)
        {
            return layer + "*STAGE START";
        }

        public static string LayerStageStop(string layer)
        {
            return layer + "*STAGE STOP";
        }

        /// <summary>Katmanı boşaltır (boş SET_OBJECT). Eski uygulama flot AL sonrası arka katmanı böyle temizliyor.</summary>
        public static string ClearLayer(string layer)
        {
            return layer + " SET_OBJECT";
        }

        public static string LayerDirector(string layer, string directorName)
        {
            return layer + "*STAGE*DIRECTOR*" + directorName;
        }

        public static string LayerDirectorShow(string layer, string directorName, double seconds)
        {
            return LayerDirector(layer, directorName) + " SHOW " + seconds.ToString("0.0##", CultureInfo.InvariantCulture);
        }

        public static string LayerDirectorContinue(string layer, string directorName)
        {
            return LayerDirector(layer, directorName) + " CONTINUE";
        }

        public static string LayerDirectorGetEndTime(string layer, string directorName)
        {
            return LayerDirector(layer, directorName) + "*END_TIME GET";
        }

        public static string GetMainSceneName()
        {
            return MainLayer + "*NAME GET";
        }

        // ---- Container -------------------------------------------------------

        /// <summary>Container adresi. containerRef: "$edt_txt_haber" gibi ad ya da "/2/1/3/2" gibi yol.</summary>
        public static string Container(string containerRef)
        {
            return MainLayer + "*TREE*" + containerRef;
        }

        /// <summary>Ad ile container adresi; "$" işaretini sizin yerinize ekler.</summary>
        public static string ByName(string containerName)
        {
            return "$" + containerName;
        }

        public static string GetText(string containerRef)
        {
            return Container(containerRef) + "*GEOM*TEXT GET";
        }

        public static string SetText(string containerRef, string text)
        {
            return Container(containerRef) + "*GEOM*TEXT SET " + Escape(text);
        }

        public static string SetActive(string containerRef, bool active)
        {
            return Container(containerRef) + "*ACTIVE SET " + (active ? "1" : "0");
        }

        public static string GetActive(string containerRef)
        {
            return Container(containerRef) + "*ACTIVE GET";
        }

        public static string GetFunctions(string containerRef)
        {
            return Container(containerRef) + "*FUNCTION GET";
        }

        /// <summary>Tüm ağacı tek seferde döker: {yol id ad bayrak} {…} …</summary>
        public static string GetTree()
        {
            return MainLayer + "*TREE GET";
        }

        // ---- Director / Stage ------------------------------------------------

        public static string Director(string directorName)
        {
            return MainLayer + "*STAGE*DIRECTOR*" + directorName;
        }

        /// <summary>Director'u baştan oynatır.</summary>
        public static string DirectorStart(string directorName)
        {
            return Director(directorName) + " START";
        }

        /// <summary>Director bulunduğu yerden (ya da stop point'ten) devam eder.</summary>
        public static string DirectorContinue(string directorName)
        {
            return Director(directorName) + " CONTINUE";
        }

        /// <summary>Director'u verilen saniyeye atlatır ve durur (animasyonsuz). Eski uygulama "SHOW 0.0" ile başa sarıyor.</summary>
        public static string DirectorShow(string directorName, double seconds)
        {
            return Director(directorName) + " SHOW " + seconds.ToString("0.0##", CultureInfo.InvariantCulture);
        }

        public static string DirectorStop(string directorName)
        {
            return Director(directorName) + " STOP";
        }

        /// <summary>Director'ün bitiş zamanı (saniye). Eski uygulama animasyon süresini bununla ölçüyor.</summary>
        public static string DirectorGetEndTime(string directorName)
        {
            return Director(directorName) + "*END_TIME GET";
        }

        public static string DirectorGetLength(string directorName)
        {
            return Director(directorName) + "*LENGTH GET";
        }

        public static string ListDirectors()
        {
            return MainLayer + "*STAGE*DIRECTOR GET";
        }

        // ---- Yardımcı --------------------------------------------------------

        /// <summary>
        /// Viz, metin içindeki satır sonunu "\n" (iki karakter) olarak bekler.
        /// NUL karakteri protokol sonlandırıcısı olduğu için asla metinde bulunmamalı.
        /// </summary>
        private static string Escape(string text)
        {
            if (text == null) return string.Empty;
            return text.Replace("\0", "").Replace("\r\n", "\\n").Replace("\n", "\\n");
        }
    }
}
