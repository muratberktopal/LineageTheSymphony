using System.Collections.Generic;
using UnityEngine;

// ============================================================
// RandomEncounterDatabase_FULL.cs  —  500+ ENCOUNTER
//
// ÖNCELİK (priority) SİSTEMİ:
//   1 = Oyun başı  (nesil 1, nüfus 2-3, temel olaylar)
//   2 = Erken oyun (nesil 2, nüfus 4-6, ilk ilişkiler)
//   3 = Orta oyun  (nesil 3, ilk icatlar, lider, fraksiyon)
//   4 = Geç oyun   (nesil 4+, büyük siyasi olaylar)
//   5 = Oyun sonu  (nüfus 10+ veya nesil 5+)
//
// RandomEventSystem'de GetAllowedMaxPriority() çağrısıyla
// nesile/nüfusa göre hangi encounter'ların aktif olduğu belirlenir.
// ============================================================
public static class RandomEncounterDatabase
{
    public static List<RandomEventData> GetAllEncounters()
    {
        var e = new List<RandomEventData>();

        // kısa factory metodlar için alias
        // E = encounter, P = priority, C = condition, O = outcome

        #region ═══════════════════════════════════════════
        // ÖNCELİK 1 — OYUN BAŞI TEMEL OLAYLAR (1-100)
        #endregion

        // ── AÇLIK ──────────────────────────────────────────
        Add(e, "desperate_eats_raw", 1, "Personal", 0.12f, 60f, false,
            C("NeedAbove", "hunger", 90f),
            O_Need("hunger", -25f), O_Stat("health", -15f), O_N("{name} açlıktan çiğ yedi. Midesi bozulabilir.", "Social"));

        Add(e, "hunger_steals_neighbor", 1, "Personal", 0.06f, 120f, false,
            C("HasCard", "Hırsız"), C("NeedAbove", "hunger", 80f),
            O_Steal("nearest"), O_N("{name} aç kalmaya dayanamadı, komşusunun yiyeceğini çaldı.", "Drama"));

        Add(e, "hunger_turns_violent", 1, "Conflict", 0.05f, 120f, false,
            C("HasCard", "Sinir"), C("NeedAbove", "hunger", 85f), C("EmotionAbove", "Angry", 0.5f),
            O_Attack("nearest"), O_Cascade("Fight", 0.5f), O_N("{name} açlık ve sinirle kontrolünü kaybetti!", "Conflict"));

        Add(e, "starvation_vision", 1, "Chaos", 0.08f, 180f, false,
            C("HasCard", "Deli"), C("NeedAbove", "hunger", 95f),
            O_Emo("Terrified", 0.8f), O_N("{name} açlıktan halüsinasyon görüyor!", "Surprising"));

        Add(e, "hunger_begs_leader", 1, "Social", 0.08f, 90f, false,
            C("NeedAbove", "hunger", 75f), C("ReputationAbove", "", 30f),
            O_Rel("leader", 10f), O_Need("hunger", -20f), O_N("{name} liderden yardım istedi.", "Social"));

        Add(e, "hunger_prayer", 1, "Religion", 0.08f, 120f, false,
            C("NeedAbove", "hunger", 75f),
            O_Emo("Hopeful", 0.3f), O_N("{name} tanrılara açlıktan dua etti.", "Social"));

        Add(e, "communal_meal", 1, "Social", 0.08f, 180f, false,
            C("HasCard", "Sosyal"), C("ResourceAbove", "food", 30f),
            O_Emo("Happy", 0.4f), O_Need("loneliness", -20f), O_N("{name} herkesi ortak yemeğe davet etti!", "Social"));

        Add(e, "hunger_shares_last_food", 1, "Social", 0.06f, 180f, false,
            C("HasCard", "Sosyal"), C("NeedAbove", "hunger", 60f),
            O_Need("hunger", 15f), O_Rep(10f), O_N("{name} son yiyeceğini hasta komşusuna verdi.", "Social"));

        Add(e, "food_poisoning", 1, "Personal", 0.15f, 300f, false,
            C("NeedAbove", "hunger", 80f),
            O_Stat("health", -20f), O_N("{name} yediği şeyden zehirlendi!", "Conflict"));

        Add(e, "scavenging_forest", 1, "Personal", 0.10f, 90f, false,
            C("HasCard", "Maceracı"), C("NeedAbove", "hunger", 65f),
            O_Need("hunger", -20f), O_N("{name} ormanda yiyecek aradı.", "Social"));

        Add(e, "food_trade_attempt", 1, "Economy", 0.08f, 90f, false,
            C("HasCard", "Sosyal"), C("NeedAbove", "hunger", 55f),
            O_Need("hunger", -15f), O_Rel("nearest", 10f), O_N("{name} yiyecek takası teklif etti.", "Social"));

        Add(e, "starvation_death_ritual", 1, "Personal", 0.20f, 60f, false,
            C("NeedAbove", "hunger", 95f), C("IsInjured"),
            O_Heal("self"), O_N("{name} ölüm döşeğinde. Şaman yardıma koştu.", "Drama"));

        Add(e, "desperate_cannibal_thought", 1, "Chaos", 0.02f, 600f, false,
            C("HasCard", "Deli"), C("NeedAbove", "hunger", 95f), C("HasCard", "Sinir"),
            O_Attack("nearest"), O_Cascade("Fight", 0.8f), O_N("{name} açlıktan aklını yitirdi! Köyde panik var!", "Major"));

        // YENİ KARTLAR — AÇLIK P1
        Add(e, "gurur_aclik_yardim_reddeder", 1, "Personal", 0.08f, 120f, false,
            C("HasCard", "Gururlu"), C("NeedAbove", "hunger", 80f),
            O_Need("hunger", 10f), O_Emo("Humiliated", 0.3f), O_N("{name} açlıktan ölse de yardım istemeyecek. Gururu buna izin vermiyor.", "Drama"));

        Add(e, "bencil_hides_food", 1, "Personal", 0.08f, 180f, false,
            C("HasCard", "Bencil"), C("ResourceBelow", "food", 15f),
            O_Stat("hoardingTendency", 20f), O_Rep(-10f), O_N("{name} kıtlıkta bile yiyeceğini paylaşmıyor.", "Drama"));

        Add(e, "klumsy_drops_food", 1, "Comedy", 0.10f, 90f, false,
            C("HasCard", "Klumsy"), C("NeedBelow", "hunger", 60f),
            O_Need("hunger", 15f), O_N("{name} yiyeceği taşırken düşürdü! Bir kısım çamura gitti.", "Surprising"));

        Add(e, "asiri_hevesli_food_gather", 1, "Comedy", 0.10f, 90f, false,
            C("HasCard", "AşırıHevesli"), C("NeedAbove", "hunger", 55f),
            O_Need("hunger", -10f), O_N("{name} yiyecek toplamaya koştu ama yarıda döndü. Neden? Bilinmiyor.", "Surprising"));

        Add(e, "mizmir_food_complaint", 1, "Comedy", 0.12f, 60f, false,
            C("HasCard", "Mızmız"), C("NeedBelow", "hunger", 40f),
            O_N("{name} doymuş olmasına rağmen yiyecekten şikayet ediyor. Alışılacak bir şey.", "Surprising"));

        Add(e, "unutan_yemek_yakar", 1, "Comedy", 0.10f, 90f, false,
            C("HasCard", "Unutkan"), C("NeedAbove", "hunger", 50f),
            O_Need("hunger", 10f), O_N("{name} yemek pişirirken unutup gitti. Yemek yandı.", "Surprising"));

        Add(e, "hizli_avda_birinci", 1, "Personal", 0.10f, 90f, false,
            C("HasCard", "Hızlı"), C("HasCard", "Avcı"), C("NeedAbove", "hunger", 50f),
            O_Need("hunger", -25f), O_Rep(5f), O_N("{name} avda herkesten önce hedefe ulaştı. Hız işe yarıyor.", "Surprising"));

        Add(e, "kor_koku_ile_yiyecek", 1, "Personal", 0.08f, 90f, false,
            C("HasCard", "Kör"), C("NeedAbove", "hunger", 65f),
            O_Need("hunger", -15f), O_N("{name} göremese de kokuyla yiyecek kaynağını buldu.", "Surprising"));

        Add(e, "hoarding_discovered", 1, "Social", 0.10f, 300f, false,
            C("HoardingAbove", "", 40f), C("ResourceBelow", "food", 10f),
            O_Rep(-25f), O_Emo("Humiliated", 0.5f), O_Cascade("PublicHumiliation", 0.5f),
            O_N("{name}'ın yiyecek stoku keşfedildi! Açlık varken biriktiriyordu.", "Drama"));

        Add(e, "child_refuses_to_eat", 1, "Personal", 0.06f, 120f, false,
            C("EmotionAbove", "Depressed", 0.5f), C("NeedAbove", "hunger", 50f), C("GenerationAbove", "", 1f),
            O_Stat("health", -10f), O_N("{name} yemek yemeyi reddediyor. Bir şeyler onu üzüyor.", "Social"));

        Add(e, "hunger_induces_creativity", 1, "Invention", 0.04f, 0f, true,
            C("HasCard", "Zeka"), C("NeedAbove", "hunger", 70f), C("ThresholdMet", "hunger_high", 150f),
            O_Inv("Farming"), O_N("{name} açlıkla dövüşürken yeni bir fikir geliştirdi.", "Surprising"));

        // ── YORGUNLUK VE ENERJİ ────────────────────────────
        Add(e, "overwork_collapse", 1, "Personal", 0.10f, 120f, false,
            C("NeedAbove", "fatigue", 90f),
            O_Stat("health", -10f), O_N("{name} yorgunluktan yere yıkıldı!", "Conflict"));

        Add(e, "energetic_morning", 1, "Personal", 0.15f, 180f, false,
            C("NeedBelow", "fatigue", 20f), C("EmotionAbove", "Happy", 0.5f),
            O_Stat("workSpeed", 0.3f), O_N("{name} bu sabah çok enerjik! Herkesi heyecanlandırdı.", "Social"));

        Add(e, "night_work_exhaustion", 1, "Personal", 0.10f, 90f, false,
            C("IsNight"), C("NeedAbove", "fatigue", 70f),
            O_Stat("health", -5f), O_N("{name} gece yorgunlukla hata yaptı.", "Social"));

        Add(e, "nap_at_wrong_time", 1, "Personal", 0.10f, 120f, false,
            C("HasCard", "Tembel"), C("NeedAbove", "fatigue", 40f),
            O_N("{name} tam iş zamanı uyuya kaldı.", "Social"));

        Add(e, "overworked_anger", 1, "Conflict", 0.07f, 120f, false,
            C("HasCard", "Sinir"), C("NeedAbove", "fatigue", 75f),
            O_Attack("nearest"), O_N("{name} kendisi çalışırken başkalarının dinlenmesine tahammül edemedi!", "Conflict"));

        Add(e, "collective_rest", 1, "Social", 0.06f, 240f, false,
            C("HasCard", "Sosyal"), C("NeedAbove", "fatigue", 60f),
            O_Need("fatigue", -20f), O_Emo("Happy", 0.3f), O_N("{name} toplu mola önerdi.", "Social"));

        Add(e, "sleep_deprivation_hallucination", 1, "Chaos", 0.08f, 180f, false,
            C("HasCard", "Deli"), C("NeedAbove", "fatigue", 80f), C("IsNight"),
            O_Emo("Terrified", 0.6f), O_N("{name} uykusuzluktan hal hayal karışık!", "Surprising"));

        Add(e, "lazy_finds_shortcut", 1, "Invention", 0.06f, 180f, false,
            C("HasCard", "Tembel"), C("NeedAbove", "fatigue", 50f),
            O_Stat("workSpeed", 0.2f), O_N("{name} işi kolaylaştırmanın yolunu buldu. Tembeller de işe yarar.", "Surprising"));

        Add(e, "overworked_quits", 1, "Personal", 0.05f, 240f, false,
            C("NeedAbove", "fatigue", 85f), C("EmotionAbove", "Depressed", 0.4f),
            O_N("{name} artık dayanamıyor. İşi bıraktı.", "Social"));

        Add(e, "sleep_talking", 1, "Social", 0.05f, 180f, false,
            C("HasCard", "Dedikoduncu"),
            O_Cascade("Scandal", 0.3f), O_N("{name} uyurken sır verdi! Herkes duydu.", "Drama"));

        // YENİ KARTLAR — YORGUNLUK P1
        Add(e, "erken_kalkan_advantage", 1, "Personal", 0.15f, 120f, false,
            C("HasCard", "ErkenKalkan"), C("IsNight"),
            O_Stat("workSpeed", 0.3f), O_N("{name} herkes uyurken tek başına çalışıyor. Sabah geldiğinde iş yarı bitti.", "Surprising"));

        Add(e, "agir_uyuyan_misses_morning", 1, "Comedy", 0.12f, 120f, false,
            C("HasCard", "AğırUyuyan"),
            O_Need("fatigue", -20f), O_N("{name} yine geç kalktı. Köy çoktan işe başlamıştı.", "Surprising"));

        Add(e, "agir_uyuyan_night_surge", 1, "Personal", 0.10f, 120f, false,
            C("HasCard", "AğırUyuyan"), C("IsNight"),
            O_Need("fatigue", -20f), O_Stat("workSpeed", 0.25f), O_N("{name} gece geç saatlerde en üretken dönemine giriyor.", "Surprising"));

        Add(e, "erken_agir_hic_cakismaz", 1, "Comedy", 0.08f, 240f, false,
            C("HasCard", "ErkenKalkan"),
            O_N("{name} gece çalışıyor, diğeri gündüz. İkisi hiç karşılaşmıyor. Mükemmel iş bölümü mü, yoksa birbirlerinden kaçıyorlar mı?", "Surprising"));

        Add(e, "hizli_tires_fast", 1, "Personal", 0.10f, 90f, false,
            C("HasCard", "Hızlı"), C("NeedAbove", "fatigue", 60f),
            O_Need("fatigue", 20f), O_N("{name} herkesten hızlı gidiyor ama çok çabuk tükeniyor.", "Social"));

        Add(e, "klumsy_collapse", 1, "Comedy", 0.10f, 90f, false,
            C("HasCard", "Klumsy"), C("NeedAbove", "fatigue", 70f),
            O_Stat("health", -5f), O_N("{name} yorgunluktan taşıdıklarını düşürüp kendisi de düştü!", "Surprising"));

        Add(e, "unutan_gorev_dongu", 1, "Comedy", 0.12f, 60f, false,
            C("HasCard", "Unutkan"),
            O_Need("fatigue", 10f), O_N("{name} görevi yarıda bırakıp gitti. Sonra döndü. Sonra tekrar gitti. Döngü sürüyor.", "Surprising"));

        Add(e, "rekabetci_tembel_cildiriyor", 1, "Comedy", 0.10f, 90f, false,
            C("HasCard", "Rekabetçi"),
            O_Need("fatigue", 15f), O_N("{name} tembel biriyle aynı işe düştü. Tek başına çılgına dönüyor.", "Surprising"));

        Add(e, "asiri_hevesli_sakin_uyum", 1, "Comedy", 0.06f, 180f, false,
            C("HasCard", "AşırıHevesli"), C("HasCard", "Sakin"),
            O_Stat("workSpeed", 0.2f), O_N("{name} işi yarıda bırakıyor, sakin biri geliyor tamamlıyor. Beklenmedik mükemmel uyum.", "Surprising"));

        Add(e, "rest_day_invention", 1, "Invention", 0.04f, 300f, false,
            C("HasCard", "Tembel"), C("HasCard", "Zeka"), C("ThresholdMet", "idle_long", 180f),
            O_Emo("Proud", 0.5f), O_N("{name} dinlenirken aklına müthiş bir fikir geldi!", "Surprising"));

        Add(e, "obsesif_gece_calismasi", 1, "Personal", 0.08f, 180f, false,
            C("HasCard", "AşırıHevesli"), C("IsNight"),
            O_Need("fatigue", 30f), O_Stat("workSpeed", 0.2f), O_N("{name} gece yarısı hâlâ çalışıyor. Bırakmayı bilmiyor.", "Surprising"));

        // ── HASTALIK VE SAĞLIK ─────────────────────────────
        Add(e, "shaman_heals_successfully", 1, "Religion", 0.15f, 120f, false,
            C("HasCard", "Şaman"), C("HasCard", "Zeka"),
            O_Heal("nearest_sick"), O_Rep(10f), O_N("{name} hastayı iyileştirdi! Köy şükranını sunuyor.", "Social"));

        Add(e, "shaman_worsens_patient", 1, "Chaos", 0.12f, 120f, false,
            C("HasCard", "Şaman"), C("HasCard", "Deli"),
            O_Stat("health", -15f), O_N("{name}'ın tedavisi işe yaramadı! Hasta daha da kötü.", "Surprising"));

        Add(e, "injury_from_tree", 1, "Personal", 0.08f, 180f, false,
            C("HasCard", "Oduncu"), C("HasCard", "Sinir"), C("EmotionAbove", "Angry", 0.5f),
            O_Stat("health", -20f), O_N("{name} ağacı keserken yaralandı!", "Conflict"));

        Add(e, "wounded_warrior_refuses_help", 1, "Personal", 0.10f, 90f, false,
            C("HasCard", "Sinir"), C("IsInjured"),
            O_Stat("health", -5f), O_N("{name} yardımı kibarca değil, sertçe reddetti.", "Social"));

        Add(e, "epidemic_quarantine", 1, "Social", 0.12f, 300f, false,
            C("HasCard", "Koruyucu"), C("IsSick"),
            O_Rep(5f), O_N("{name} hastaları ayırmayı önerdi.", "Social"));

        Add(e, "healing_ritual", 1, "Religion", 0.12f, 120f, false,
            C("HasCard", "Şaman"),
            O_Rep(5f), O_N("{name} iyileştirme ritüeli yaptı.", "Religion"));

        Add(e, "wound_infection", 1, "Personal", 0.08f, 120f, false,
            C("IsInjured"), C("IsWinter"),
            O_Stat("health", -15f), O_N("{name}'ın yarası enfekte oldu!", "Conflict"));

        Add(e, "pain_induced_aggression", 1, "Conflict", 0.08f, 90f, false,
            C("HasCard", "Sinir"), C("IsInjured"),
            O_Attack("nearest"), O_N("{name} acısından yaklaşana vurdu!", "Conflict"));

        Add(e, "community_care", 1, "Social", 0.10f, 120f, false,
            C("HasCard", "Sosyal"), C("HasCard", "Koruyucu"),
            O_Heal("nearest_sick"), O_Rep(8f), O_N("{name} hasta komşusunun yanından ayrılmıyor.", "Social"));

        Add(e, "bad_water", 1, "Nature", 0.08f, 600f, false,
            C("ResourceAbove", "water", 0f),
            O_Stat("health", -10f), O_Cascade("Plague", 0.3f), O_N("Nehir suyu kirlenmiş! İçenler hasta oluyor.", "Major"));

        Add(e, "fever_delirium", 1, "Chaos", 0.10f, 180f, false,
            C("IsSick"), C("HasCard", "Deli"),
            O_Cascade("Scandal", 0.3f), O_N("{name} ateşle sayıklarken sırlarını ifşa etti!", "Drama"));

        Add(e, "self_medication_fails", 1, "Chaos", 0.08f, 120f, false,
            C("IsSick"), C("HasCard", "Deli"),
            O_Heal("self"), O_N("{name} kendi kendini tedavi etmeye çalıştı.", "Surprising"));

        Add(e, "bedrest_invention", 1, "Invention", 0.05f, 300f, false,
            C("HasCard", "Zeka"), C("IsInjured"), C("ThresholdMet", "idle_long", 120f),
            O_Emo("Proud", 0.4f), O_N("{name} yaralı yatarken aklına büyük bir fikir geldi.", "Surprising"));

        Add(e, "medicine_hoarding", 1, "Personal", 0.08f, 240f, false,
            C("HoardingAbove", "", 30f), C("HasInvention", "Medicine"),
            O_Rep(-20f), O_N("{name} ilacı paylaşmadı. Köy bunu unutmayacak.", "Drama"));

        Add(e, "miracle_recovery", 1, "Religion", 0.05f, 600f, false,
            C("IsSick"), C("IsInjured"),
            O_Stat("health", 50f), O_Emo("Happy", 0.8f), O_N("{name} mucizevi şekilde iyileşti! Köyde şenlik havası var.", "Surprising"));

        // YENİ KARTLAR — SAĞLIK P1
        Add(e, "gurur_refuses_healer", 1, "Personal", 0.10f, 120f, false,
            C("HasCard", "Gururlu"), C("IsInjured"),
            O_Stat("health", -5f), O_N("{name} hasta olduğunu kabul etmiyor. Şamanı reddetti.", "Drama"));

        Add(e, "kisa_omurlu_sick_dangerous", 1, "Personal", 0.12f, 60f, false,
            C("HasCard", "KısaÖmürlü"), C("IsSick"),
            O_Stat("health", -20f), O_N("{name} zaten kısa yaşıyor, hastalık çok tehlikeli.", "Drama"));

        Add(e, "uzun_omurlu_recovers", 1, "Personal", 0.10f, 120f, false,
            C("HasCard", "UzunÖmürlü"), C("IsSick"),
            O_Stat("health", 20f), O_N("{name}'ın vücudu güçlü. Hastalığı atlattı.", "Social"));

        Add(e, "kor_hears_sick", 1, "Personal", 0.10f, 90f, false,
            C("HasCard", "Kör"),
            O_Rep(5f), O_N("{name} göremese de hastalanmış birinin sesini duydu ve yardım etti.", "Surprising"));

        Add(e, "klumsy_hurts_shaman", 1, "Comedy", 0.06f, 180f, false,
            C("HasCard", "Klumsy"), C("IsSick"),
            O_Stat("health", -8f), O_N("{name} iyileşmeye çalışırken şamanın ilaç kaplarını devirdi.", "Surprising"));

        // ── TEMEL DUYGU VE DAVRANIŞ ────────────────────────
        Add(e, "happiness_spreads", 1, "Social", 0.10f, 120f, false,
            C("EmotionAbove", "Happy", 0.8f), C("HasCard", "Sosyal"),
            O_Emo("Happy", 0.3f), O_N("{name}'ın neşesi bulaşıcı! Köy şenleniyor.", "Social"));

        Add(e, "fear_paralyzes", 1, "Personal", 0.10f, 90f, false,
            C("HasCard", "Korkak"), C("NeedAbove", "fear", 80f),
            O_N("{name} korkudan dondu!", "Surprising"));

        Add(e, "sudden_mood_swing", 1, "Chaos", 0.15f, 60f, false,
            C("HasCard", "Deli"),
            O_Emo("Euphoric", 0.5f), O_N("{name}'ın ruh hali aniden değişti!", "Surprising"));

        Add(e, "crying_in_public", 1, "Social", 0.06f, 180f, false,
            C("EmotionAbove", "Sad", 0.7f), C("NeedAbove", "loneliness", 60f),
            O_Need("loneliness", -10f), O_N("{name} herkesin önünde ağladı.", "Social"));

        Add(e, "rage_against_nature", 1, "Chaos", 0.06f, 120f, false,
            C("HasCard", "Sinir"), C("EmotionAbove", "Angry", 0.8f),
            O_Need("fatigue", 20f), O_N("{name} öfkeyle doğaya saldırdı. Pek işe yaramadı.", "Surprising"));

        Add(e, "contentment_slows_ambition", 1, "Personal", 0.08f, 180f, false,
            C("EmotionAbove", "Happy", 0.7f), C("HasCard", "Tembel"), C("NeedBelow", "hunger", 30f),
            O_Stat("workSpeed", -0.2f), O_N("{name} çok mutlu. Belki bu yüzden hiç öteye geçmek istemiyor.", "Social"));

        Add(e, "guilt_overcompensation", 1, "Social", 0.08f, 120f, false,
            C("EmotionAbove", "Guilty", 0.5f),
            O_Rel("nearest", 15f), O_Rep(5f), O_N("{name} vicdan azabıyla herkese yardım ediyor.", "Social"));

        Add(e, "euphoria_overworks", 1, "Personal", 0.08f, 120f, false,
            C("EmotionAbove", "Euphoric", 0.6f), C("NeedBelow", "fatigue", 30f),
            O_Stat("workSpeed", 0.5f), O_Need("fatigue", 30f), O_N("{name} çok mutlu, durdurulamıyor! Ama bu uzun sürmez.", "Surprising"));

        Add(e, "anger_destroys_work", 1, "Conflict", 0.06f, 180f, false,
            C("HasCard", "Sinir"), C("EmotionAbove", "Angry", 0.7f),
            O_Rep(-5f), O_Cascade("PublicScandal", 0.3f), O_N("{name} öfkeyle emeklerini mahvetti!", "Conflict"));

        Add(e, "bravery_surge", 1, "Personal", 0.12f, 180f, false,
            C("HasCard", "Koruyucu"), C("NeedAbove", "fear", 50f), C("FriendCountAbove", "", 0f),
            O_Need("fear", -50f), O_Emo("Brave", 0.8f), O_N("{name} arkadaşını kurtarmak için tehlikeye atladı!", "Surprising"));

        Add(e, "euphoric_generosity", 1, "Social", 0.08f, 180f, false,
            C("EmotionAbove", "Happy", 0.8f), C("ResourceAbove", "food", 20f),
            O_Rep(10f), O_Need("loneliness", -15f), O_N("{name} çok mutlu! Herkese yiyecek dağıtıyor.", "Social"));

        Add(e, "paranoia_sets_in", 1, "Personal", 0.06f, 300f, false,
            C("NeedAbove", "fear", 60f), C("TrustBelow", "", 30f),
            O_Stat("trustLevel", -15f), O_Need("loneliness", 20f), O_N("{name} kimseye güvenmemeye başladı.", "Social"));

        // YENİ KARTLAR — DUYGU P1
        Add(e, "mizmir_iyi_haber_sikayet", 1, "Comedy", 0.12f, 60f, false,
            C("HasCard", "Mızmız"), C("EmotionAbove", "Happy", 0.5f),
            O_N("{name} için iyi bir şey oldu. Yine de şikayet etti. Bu sefer 'çok iyi, sorun çıkacak' dedi.", "Surprising"));

        Add(e, "asiri_sosyal_yalniz_takip", 1, "Comedy", 0.10f, 90f, false,
            C("HasCard", "AşırıSosyal"), C("NeedAbove", "loneliness", 50f),
            O_Rel("nearest", 10f), O_Need("loneliness", -15f), O_N("{name} yalnız birinin peşine takıldı. O kaçıyor, bu kovalıyor.", "Surprising"));

        Add(e, "asiri_sosyal_sinir_patlama", 1, "Conflict", 0.07f, 120f, false,
            C("HasCard", "AşırıSosyal"),
            O_Emo("Angry", 0.4f), O_Cascade("Fight", 0.3f), O_N("{name} sinirli minyonu bunalttı. Patlama geldi.", "Conflict"));

        Add(e, "ruya_goror_gece_yuruyus", 1, "Personal", 0.08f, 180f, false,
            C("HasCard", "RüyaGörür"), C("IsNight"),
            O_Need("coldness", 15f), O_N("{name} gece uyurgezer çıktı. Sabah hiçbir şey hatırlamıyor.", "Surprising"));

        Add(e, "ruya_goror_erken_kalkan_panik", 1, "Comedy", 0.07f, 180f, false,
            C("HasCard", "RüyaGörür"), C("IsNight"),
            O_Need("fear", 20f), O_N("{name} uyurgezer çıktı. Erken kalkan bunu gördü ve köyü uyandırdı.", "Surprising"));

        Add(e, "kor_isitme_tehlike", 1, "Personal", 0.10f, 90f, false,
            C("HasCard", "Kör"), C("NeedAbove", "fear", 30f),
            O_Need("fear", -15f), O_Rep(8f), O_N("{name} göremese de tehlikeyi sesle fark etti. En güvenilir uyarı o.", "Surprising"));

        Add(e, "kor_klumsy_carpma", 1, "Comedy", 0.10f, 60f, false,
            C("HasCard", "Kör"),
            O_Stat("health", -3f), O_N("{name} göremediği için klumsy minyona sürekli çarpıyor.", "Surprising"));

        Add(e, "klumsy_domino", 1, "Comedy", 0.07f, 90f, false,
            C("HasCard", "Klumsy"), C("HasCard", "Hızlı"),
            O_Stat("health", -5f), O_Cascade("ClumsyDomino", 0.3f), O_N("{name} hızla koşarken bir şeyleri devirdi. Domino başladı.", "Surprising"));

        Add(e, "shupheci_new_minyon", 1, "Social", 0.12f, 120f, false,
            C("HasCard", "Şüpheci"),
            O_Rel("nearest", -5f), O_N("{name} yeni gelen minyona uzun süre yaklaşmadı. Gözlüyor.", "Social"));

        Add(e, "haset_rivals_success", 1, "Personal", 0.10f, 90f, false,
            C("HasCard", "Haset"), C("EmotionAbove", "Jealous", 0.4f),
            O_Emo("Angry", 0.3f), O_N("{name} başkasının başarısını duyunca rengi soldu.", "Drama"));

        Add(e, "yansitici_ayni_kart_sempati", 1, "Social", 0.10f, 120f, false,
            C("HasCard", "Yansıtıcı"),
            O_Rel("nearest", 20f), O_Need("loneliness", -20f), O_N("{name} kendisiyle aynı kartı taşıyan biriyle çok derin bir bağ kurdu.", "Social"));

        Add(e, "onsezili_tehlikeden_kacis", 1, "Personal", 0.10f, 120f, false,
            C("HasCard", "Önsezili"),
            O_Need("fear", -20f), O_Rep(5f), O_N("{name} tehlikeyi herkesten önce hissetti. Kimse nasıl bildiğini anlamıyor.", "Surprising"));

        Add(e, "gurur_hata_kabul_etmez", 1, "Personal", 0.10f, 120f, false,
            C("HasCard", "Gururlu"), C("EmotionAbove", "Guilty", 0.3f),
            O_Emo("Angry", 0.3f), O_Rep(-5f), O_N("{name} hata yaptığını biliyor ama kabul etmeyecek. Özür yok.", "Drama"));

        Add(e, "asiri_hevesli_bina_yarim", 1, "Comedy", 0.07f, 180f, false,
            C("HasCard", "AşırıHevesli"), C("HasCard", "İnşaatçı"),
            O_N("{name} beş farklı bina başlattı. Hiçbirini bitiremedi. Köyde yarım yapılar birikmeye başladı.", "Surprising"));

        // ── DOĞA OLAYLARI ──────────────────────────────────
        Add(e, "sudden_storm", 1, "Nature", 0.05f, 600f, false,
            C("NeedBelow", "coldness", 30f),
            O_Need("coldness", 25f), O_Emo("Anxious", 0.4f), O_N("Aniden fırtına çıktı!", "Nature"));

        Add(e, "wolf_pack_attack", 1, "Nature", 0.06f, 300f, false,
            C("IsWinter"), C("NeedAbove", "fear", 20f),
            O_Stat("health", -15f), O_Need("fear", 30f), O_Cascade("WolfAttack", 0.5f), O_N("Kurt sürüsü köye yaklaştı!", "Conflict"));

        Add(e, "korkak_dogal_afet_panik", 1, "Nature", 0.10f, 120f, false,
            C("HasCard", "Korkak"), C("NeedAbove", "fear", 50f),
            O_Need("fear", 20f), O_Cascade("VillagePanic", 0.3f), O_N("{name} tehlikeden kaçarken köyü de paniğe sürükledi!", "Conflict"));

        Add(e, "hizli_tehlikeden_kacis", 1, "Nature", 0.10f, 90f, false,
            C("HasCard", "Hızlı"), C("NeedAbove", "fear", 40f),
            O_Need("fear", -20f), O_N("{name} tehlikeden çok hızlı kaçtı. Kurtuldu.", "Surprising"));

        Add(e, "kor_firtina_uyari", 1, "Nature", 0.08f, 120f, false,
            C("HasCard", "Kör"), C("NeedAbove", "fear", 30f),
            O_Rep(8f), O_N("{name} fırtınayı sesle fark etti ve diğerlerini uyardı.", "Surprising"));

        Add(e, "onsezili_dogal_afet_oncesi", 1, "Nature", 0.10f, 180f, false,
            C("HasCard", "Önsezili"), C("IsWinter"),
            O_Need("fear", -20f), O_Rep(5f), O_N("{name} fırtına gelmeden önce içeri girdi. Nasıl bildi?", "Surprising"));

        Add(e, "klumsy_storm_damage", 1, "Nature", 0.08f, 120f, false,
            C("HasCard", "Klumsy"), C("IsWinter"),
            O_Stat("health", -10f), O_N("{name} fırtınada bir şeylere çarptı. Ekstra hasar aldı.", "Surprising"));

        Add(e, "drought_begins", 1, "Nature", 0.04f, 900f, false,
            C("NeedBelow", "coldness", 20f), C("ResourceBelow", "water", 20f),
            O_Cascade("Drought", 0.6f), O_N("Kuraklık başladı. Nehir çekiliyor.", "Major"));

        Add(e, "abundant_harvest", 1, "Nature", 0.10f, 600f, false,
            C("HasInvention", "Farming"), C("NeedBelow", "coldness", 40f),
            O_Need("hunger", -30f), O_Emo("Happy", 0.6f), O_N("Bu yıl hasat çok bereketli! Köy şenlik yapıyor.", "Social"));

        Add(e, "fish_run_spring", 1, "Nature", 0.12f, 600f, false,
            C("HasInvention", "FishingRod"), C("NeedBelow", "coldness", 30f),
            O_Need("hunger", -25f), O_Rep(5f), O_N("Balıklar göç ediyor! Şimdi avlanma zamanı.", "Surprising"));

        Add(e, "bear_encounter", 1, "Nature", 0.06f, 300f, false,
            C("NeedAbove", "fear", 20f),
            O_Stat("health", -20f), O_Need("fear", 40f), O_N("{name} ormanda bir ayıyla karşılaştı!", "Conflict"));

        Add(e, "mushroom_bloom", 1, "Nature", 0.08f, 300f, false,
            C("NeedAbove", "hunger", 40f),
            O_Need("hunger", -15f), O_N("Ormanda mantar bolluğu! Ama hangisi zehirli?", "Surprising"));

        Add(e, "toxic_mushroom", 1, "Nature", 0.06f, 300f, false,
            C("NeedAbove", "hunger", 60f),
            O_Stat("health", -25f), O_N("{name} yanlış mantar yedi! Zehirlendi.", "Conflict"));

        #region ═══════════════════════════════════════════
        // ÖNCELİK 2 — ERKEN OYUN (101-200)
        #endregion

        // ── EVLİLİK VE AİLE ────────────────────────────────
        Add(e, "love_at_first_sight", 2, "Social", 0.06f, 120f, false,
            C("HasCard", "Romantik"), C("EmotionAbove", "Happy", 0.5f),
            O_Emo("Euphoric", 0.6f), O_Rel("nearest", 30f), O_N("{name} görür görmez {target}'a aşık oldu!", "Social"));

        Add(e, "jealous_spouse", 2, "Drama", 0.08f, 120f, false,
            C("HasCard", "Kıskanç"), C("IsMarried"),
            O_Emo("Jealous", 0.6f), O_Rel("spouse", -10f), O_N("{name} eşinin başkasıyla vakit geçirdiğini gördü. Kıskanç!", "Drama"));

        Add(e, "childless_grief", 2, "Personal", 0.06f, 300f, false,
            C("IsMarried"), C("EmotionAbove", "Sad", 0.4f),
            O_Emo("Sad", 0.3f), O_Rel("spouse", -5f), O_N("{name} çocuk sahibi olamamanın acısını yaşıyor.", "Social"));

        Add(e, "adoption", 2, "Social", 0.06f, 600f, false,
            C("HasCard", "Koruyucu"), C("IsMarried"),
            O_Rep(15f), O_Emo("Happy", 0.5f), O_N("{name} yetim bir çocuğu evlat edindi!", "Social"));

        Add(e, "mother_protects_child", 2, "Personal", 0.12f, 120f, false,
            C("HasCard", "Koruyucu"), C("NeedAbove", "fear", 50f),
            O_Emo("Brave", 0.8f), O_Rep(10f), O_N("{name} çocuğunu korumak için her şeyi göze aldı!", "Surprising"));

        Add(e, "wedding_feast", 2, "Social", 0.08f, 0f, false,
            C("IsMarried"), C("HasCard", "Sosyal"), C("ResourceAbove", "food", 20f),
            O_Emo("Happy", 0.7f), O_Need("loneliness", -30f), O_N("Köyde düğün yemeği! {name} evlendi.", "Social"));

        Add(e, "widow_remarries", 2, "Social", 0.05f, 300f, false,
            C("IsInMourning"),
            O_Marry("nearest"), O_Emo("Happy", 0.4f), O_N("{name} yeniden evlendi. Herkes buna ne diyecek?", "Social"));

        Add(e, "forbidden_love", 2, "Drama", 0.04f, 300f, false,
            C("HasCard", "Romantik"), C("FactionExists"),
            O_Rel("nearest", 25f), O_Cascade("Scandal", 0.4f), O_N("{name} ve {target} düşman olmalarına rağmen aşık oldu.", "Drama"));

        Add(e, "father_abandons_family", 2, "Drama", 0.03f, 600f, false,
            C("HasCard", "Maceracı"), C("IsMarried"), C("ThresholdMet", "idle_long", 300f),
            O_Divorce("spouse"), O_Rep(-25f), O_Cascade("Scandal", 0.5f), O_N("{name} ailesini terk etti. Köy şok oldu.", "Drama"));

        Add(e, "arranged_marriage_proposal", 2, "Social", 0.04f, 300f, false,
            C("HasCard", "Lider"), C("IsLeader"),
            O_Marry("nearest"), O_Rep(5f), O_N("{name} iki köylüyü evlendirmeye karar verdi.", "Social"));

        Add(e, "kisaomurlu_ask_dramatic", 2, "Drama", 0.06f, 180f, false,
            C("HasCard", "KısaÖmürlü"), C("HasCard", "Romantik"),
            O_Emo("Euphoric", 0.7f), O_Marry("nearest"), O_N("{name} çok yaşayamayacağını biliyor. Bu yüzden aşka daha hızlı düştü.", "Drama"));

        Add(e, "rekabetci_es_kavgasi", 2, "Drama", 0.07f, 180f, false,
            C("HasCard", "Rekabetçi"), C("IsMarried"),
            O_Rel("spouse", -10f), O_N("{name} eşinin kendisinden iyi iş yaptığını görünce rekabete girdi. Evde gerilim var.", "Drama"));

        Add(e, "mizmir_dugun_sikayet", 2, "Comedy", 0.10f, 120f, false,
            C("HasCard", "Mızmız"), C("EmotionAbove", "Happy", 0.3f),
            O_N("{name} düğünde bile bir şeylerden şikayet etti. Yemek soğukmuş.", "Surprising"));

        Add(e, "family_honor_revenge", 2, "Conflict", 0.05f, 240f, false,
            C("HasCard", "Sinir"), C("EnemyCountAbove", "", 0f), C("EmotionAbove", "Humiliated", 0.5f),
            O_Attack("nearest_enemy"), O_N("{name} aile onuru için harekete geçti!", "Conflict"));

        Add(e, "mother_sick_child", 2, "Personal", 0.08f, 180f, false,
            C("IsSick"), C("IsMarried"), C("GenerationAbove", "", 0f),
            O_Stat("health", -10f), O_N("{name}'ın çocuğu hasta doğdu.", "Social"));

        Add(e, "twins_born", 2, "Social", 0.04f, 0f, false,
            C("IsMarried"), C("IsNight"), C("NeedBelow", "fatigue", 50f),
            O_Emo("Happy", 0.7f), O_N("İkiz doğdu! {name} iki çocuğa aynı anda baktı.", "Surprising"));

        // ── ARKADAŞLIK VE DÜŞMANLIK ────────────────────────
        Add(e, "unexpected_friendship", 2, "Social", 0.10f, 180f, false,
            C("EnemyCountAbove", "", 0f), C("NeedAbove", "fear", 70f),
            O_Rel("nearest_enemy", 25f), O_N("{name} ve düşmanı birlikte tehlikeden kaçtı. Buz eridi.", "Social"));

        Add(e, "betrayal_of_trust", 2, "Drama", 0.04f, 300f, false,
            C("HasCard", "Hırsız"), C("FriendCountAbove", "", 0f),
            O_Steal("nearest_friend"), O_BecomeEnemies("nearest_friend"), O_Cascade("Betrayal", 0.7f), O_N("{name} en yakın arkadaşına ihanet etti!", "Drama"));

        Add(e, "defend_friend_in_fight", 2, "Conflict", 0.12f, 120f, false,
            C("HasCard", "Koruyucu"), C("FriendCountAbove", "", 0f), C("NearbyFightExists"),
            O_Attack("nearest_enemy"), O_Rel("nearest_friend", 20f), O_N("{name} arkadaşını korumak için kavgaya atıldı!", "Conflict"));

        Add(e, "peacekeeping_mediator", 2, "Social", 0.10f, 120f, false,
            C("HasCard", "Sosyal"), C("HasCard", "Zeka"), C("NearbyFightExists"),
            O_Rep(10f), O_Cascade("PeaceMade", 0.5f), O_N("{name} arabulucu oldu ve kavgayı durdurdu.", "Social"));

        Add(e, "gossip_destroys_friendship", 2, "Drama", 0.06f, 240f, false,
            C("HasCard", "Dedikoduncu"), C("FriendCountAbove", "", 0f),
            O_Rel("nearest_friend", -30f), O_N("{name}'ın dedikodular yüzünden arkadaşlığı sona erdi.", "Drama"));

        Add(e, "mentor_student_bond", 2, "Social", 0.06f, 300f, false,
            C("HasCard", "Zeka"), C("GenerationAbove", "", 0f),
            O_Rel("nearest", 15f), O_Stat("learningRate", 0.5f), O_N("{name} genç bir minyonu yetiştiriyor.", "Social"));

        Add(e, "old_grudge_resurfaces", 2, "Conflict", 0.06f, 180f, false,
            C("EnemyCountAbove", "", 0f), C("EmotionAbove", "Bitter", 0.4f),
            O_Attack("nearest_enemy"), O_N("{name} ve {enemy} eski bir hesabı yeniden açtı!", "Conflict"));

        Add(e, "friendship_gift", 2, "Social", 0.08f, 180f, false,
            C("EmotionAbove", "Happy", 0.6f), C("FriendCountAbove", "", 0f), C("ResourceAbove", "food", 15f),
            O_Rel("nearest_friend", 20f), O_N("{name} arkadaşına sürpriz hediye yaptı.", "Social"));

        Add(e, "unutan_arkadas_isini_bitirir", 2, "Comedy", 0.08f, 120f, false,
            C("HasCard", "Unutkan"), C("FriendCountAbove", "", 0f),
            O_Rel("nearest_friend", 10f), O_N("{name} işi unutunca arkadaşı tamamladı. Beklenmedik bir uyum.", "Surprising"));

        Add(e, "yansitici_asik_sadece_ayni", 2, "Social", 0.05f, 300f, false,
            C("HasCard", "Yansıtıcı"), C("HasCard", "Romantik"),
            O_Emo("Euphoric", 0.6f), O_N("{name} sadece kendisine çok benzediğini hissettiği birine aşık oldu.", "Social"));

        Add(e, "yansitici_deli_dengesizlesir", 2, "Chaos", 0.07f, 120f, false,
            C("HasCard", "Yansıtıcı"), C("HasCard", "Deli"),
            O_Emo("Anxious", 0.5f), O_N("{name} deli minyonun yanındayken kendisi de dengesizleşiyor.", "Surprising"));

        Add(e, "shupheci_tehdit_fark", 2, "Personal", 0.08f, 180f, false,
            C("HasCard", "Şüpheci"), C("NeedAbove", "fear", 40f),
            O_Rep(10f), O_N("{name} tehlikeyi herkesten önce fark etti. Kimse dinlemedi. Şimdi dinliyorlar.", "Surprising"));

        Add(e, "ruya_goror_onsezili", 2, "Personal", 0.05f, 240f, false,
            C("HasCard", "RüyaGörür"), C("HasCard", "Önsezili"),
            O_Rep(5f), O_N("{name} uyurgezer çıktı, tehlikeli bir alandan döndü. Sabah hiçbir şey hatırlamıyor ama sağ.", "Surprising"));

        Add(e, "onsezili_paranoyak_kacis", 2, "Comedy", 0.06f, 180f, false,
            C("HasCard", "Önsezili"), C("NeedAbove", "fear", 60f),
            O_N("{name} ve paranoyak biri aynı anda farklı yönlere kaçtı. Köy ne yapacağını bilmiyor.", "Surprising"));

        Add(e, "asiri_sosyal_col_olursa", 2, "Comedy", 0.07f, 120f, false,
            C("HasCard", "AşırıSosyal"), C("NeedAbove", "loneliness", 70f),
            O_Emo("Depressed", 0.5f), O_N("{name} etrafında kimse kalmayınca çöktü. İletişim olmadan yaşayamıyor.", "Drama"));

        Add(e, "rekabetci_haset_tatminsiz", 2, "Personal", 0.07f, 120f, false,
            C("HasCard", "Rekabetçi"), C("HasCard", "Haset"),
            O_Emo("Restless", 0.4f), O_N("{name} kazandı ama tatmin olmadı. Yeni rakip arıyor.", "Surprising"));

        // ── DİN VE İNANÇ ──────────────────────────────────
        Add(e, "shaman_prophecy", 2, "Religion", 0.05f, 300f, false,
            C("HasCard", "Şaman"),
            O_Rep(10f), O_N("{name} bir kehanet açıkladı! Köy tedirgin.", "Religion"));

        Add(e, "ritual_sacrifice", 2, "Religion", 0.04f, 300f, false,
            C("HasCard", "Şaman"), C("HasCard", "Sinir"), C("ResourceAbove", "food", 10f),
            O_Rep(10f), O_N("{name} tanrılara kurban sundu!", "Religion"));

        Add(e, "hunger_prayer_answered", 2, "Religion", 0.06f, 180f, false,
            C("HasCard", "Şaman"), C("NeedAbove", "hunger", 70f),
            O_Need("hunger", -20f), O_Rep(8f), O_N("{name} dua etti. Yiyecek bulundu. Tanrılar mı? Tesadüf mü?", "Religion"));

        Add(e, "shupheci_kehaneti_sorguluyor", 2, "Religion", 0.07f, 180f, false,
            C("HasCard", "Şüpheci"), C("HasCard", "Şaman"),
            O_Rep(-5f), O_N("{name} şamanın kehanetini sorgulayan tek kişi. Köy buna ne diyecek?", "Drama"));

        Add(e, "deli_yanlis_kehanet", 2, "Chaos", 0.07f, 180f, false,
            C("HasCard", "Deli"), C("HasCard", "Şaman"),
            O_Cascade("MadLaw", 0.3f), O_N("{name} kehaneti tamamen yanlış yorumladı. Köy buna göre davrandı.", "Surprising"));

        Add(e, "gurur_ritual_reddeder", 2, "Religion", 0.07f, 180f, false,
            C("HasCard", "Gururlu"), C("HasCard", "Şaman"),
            O_Rep(-5f), O_N("{name} ritüele katılmadı. Tanrılara boyun eğmeyecek kadar gururlu.", "Drama"));

        Add(e, "mizmir_ritual_sikayet", 2, "Comedy", 0.08f, 120f, false,
            C("HasCard", "Mızmız"), C("HasCard", "Şaman"),
            O_N("{name} ritüelin çok uzun sürdüğünden şikayet etti. Tanrılar duydu mu?", "Surprising"));

        Add(e, "onsezili_kehanet_dogrular", 2, "Religion", 0.06f, 240f, false,
            C("HasCard", "Önsezili"), C("HasCard", "Şaman"),
            O_Rep(10f), O_N("{name} kehaneti zaten hissediyordu. Gerçekleşince şaşırmadı.", "Surprising"));

        Add(e, "sacred_tree_declared", 2, "Religion", 0.03f, 0f, true,
            C("HasCard", "Şaman"),
            O_N("{name} bir ağacın kutsal olduğunu ilan etti! Artık o ağaç kesilmez.", "Religion"));

        Add(e, "rain_dance", 2, "Religion", 0.05f, 300f, false,
            C("HasCard", "Şaman"), C("NeedAbove", "hunger", 50f),
            O_Need("coldness", -10f), O_N("{name} yağmur için dans etti!", "Religion"));

        // ── EKONOMİ ────────────────────────────────────────
        Add(e, "resource_shortage", 2, "Economy", 0.12f, 300f, false,
            C("ResourceBelow", "food", 5f),
            O_Need("hunger", 15f), O_Emo("Anxious", 0.5f), O_N("Köyde yiyecek azaldı! Acil önlem gerekiyor.", "Major"));

        Add(e, "generous_donation", 2, "Economy", 0.06f, 240f, false,
            C("EmotionAbove", "Happy", 0.7f), C("HasCard", "Sosyal"), C("ResourceAbove", "food", 25f),
            O_Rep(15f), O_Emo("Happy", 0.2f), O_N("{name} fazla kaynaklarını köye bağışladı!", "Social"));

        Add(e, "bencil_pazar_tekel", 2, "Economy", 0.06f, 240f, false,
            C("HasCard", "Bencil"), C("HasInvention", "TradeSystem"),
            O_Rep(-10f), O_N("{name} takas sisteminde tekel kurmaya çalışıyor.", "Drama"));

        Add(e, "haset_ucuz_satar", 2, "Economy", 0.07f, 180f, false,
            C("HasCard", "Haset"), C("HasInvention", "TradeSystem"),
            O_Rep(5f), O_N("{name} rakibini geçmek için mallarını çok ucuza sattı.", "Surprising"));

        Add(e, "klumsy_ticaret_kaza", 2, "Comedy", 0.08f, 120f, false,
            C("HasCard", "Klumsy"), C("HasInvention", "TradeSystem"),
            O_N("{name} takas sırasında malların yarısını düşürdü. Alıcı beklenmedik indirim aldı.", "Surprising"));

        Add(e, "unutan_borc_odemiyor", 2, "Economy", 0.08f, 120f, false,
            C("HasCard", "Unutkan"),
            O_Rep(-5f), O_N("{name} borcunu ödemeyi unuttu. Kasıtlı mı? Değil. Ama sonuç aynı.", "Drama"));

        Add(e, "mizmir_pazar_begenmez", 2, "Comedy", 0.10f, 90f, false,
            C("HasCard", "Mızmız"), C("HasInvention", "TradeSystem"),
            O_N("{name} pazarda hiçbir şeyi beğenmedi. Hepsi ya çok pahalı ya kalitesiz.", "Surprising"));

        Add(e, "rekabetci_ticaret_yarisi", 2, "Economy", 0.07f, 180f, false,
            C("HasCard", "Rekabetçi"), C("HasInvention", "TradeSystem"),
            O_Rep(8f), O_N("{name} ticarette rakibini geçmek için her şeyi yapıyor.", "Surprising"));

        Add(e, "resource_windfall", 2, "Economy", 0.06f, 300f, false,
            C("HasCard", "Maceracı"),
            O_N("{name} beklenmedik büyük bir kaynak deposu keşfetti!", "Surprising"));

        Add(e, "price_gouging", 2, "Economy", 0.05f, 240f, false,
            C("HasCard", "Bencil"), C("ResourceBelow", "food", 8f),
            O_Rep(-15f), O_N("{name} kıtlıkta fahiş fiyat uyguluyor!", "Drama"));

        #region ═══════════════════════════════════════════
        // ÖNCELİK 3 — ORTA OYUN (201-320)
        #endregion

        // ── İCAT VE KEŞİF ─────────────────────────────────
        Add(e, "genius_fishing_rod", 3, "Invention", 0.05f, 0f, true,
            C("HasCard", "Zeka"), C("NeedAbove", "hunger", 70f), C("ThresholdMet", "hunger_high", 120f),
            O_Inv("FishingRod"), O_Emo("Proud", 0.6f), O_N("{name} açlığını çözmek için olta icat etti!", "Surprising"));

        Add(e, "fire_discovery", 3, "Invention", 0.04f, 0f, true,
            C("HasCard", "Zeka"), C("NeedAbove", "coldness", 60f), C("ThresholdMet", "cold_long", 150f),
            O_Inv("Campfire"), O_Need("coldness", -40f), O_Rep(20f), O_N("{name} ateş yakmayı öğrendi! Bu her şeyi değiştirir.", "Major"));

        Add(e, "wheel_invention", 3, "Invention", 0.03f, 0f, true,
            C("HasCard", "Zeka"), C("HasCard", "İnşaatçı"), C("ResourceAbove", "wood", 20f),
            O_Inv("Wheel"), O_Rep(15f), O_N("{name} yuvarlak bir şeyin işe yaradığını keşfetti!", "Surprising"));

        Add(e, "fermentation_discovery", 3, "Invention", 0.03f, 0f, true,
            C("HasCard", "Zeka"), C("ResourceAbove", "food", 15f), C("ThresholdMet", "idle_long", 120f),
            O_Inv("Fermentation"), O_N("{name} bırakılan yiyeceğin farklı bir şeye dönüştüğünü fark etti!", "Surprising"));

        Add(e, "pottery_invention", 3, "Invention", 0.03f, 0f, true,
            C("HasCard", "Zeka"), C("HasCard", "İnşaatçı"),
            O_Inv("Pottery"), O_N("{name} kili şekillendirerek kap yaptı!", "Surprising"));

        Add(e, "bow_and_arrow", 3, "Invention", 0.03f, 0f, true,
            C("HasCard", "Zeka"), C("HasCard", "Avcı"), C("ResourceAbove", "wood", 10f),
            O_Inv("BowAndArrow"), O_Rep(10f), O_N("{name} uzaktan isabet ettiren bir silah icat etti!", "Surprising"));

        Add(e, "music_creation", 3, "Invention", 0.04f, 0f, true,
            C("HasCard", "Zeka"), C("NeedAbove", "loneliness", 60f), C("ThresholdMet", "idle_long", 120f),
            O_Inv("Music"), O_Rep(8f), O_N("{name} sesler çıkaran bir alet yaptı! Müzik başlıyor.", "Surprising"));

        Add(e, "medicine_discovery", 3, "Invention", 0.03f, 0f, true,
            C("HasCard", "Şaman"), C("HasCard", "Zeka"), C("ReputationAbove", "", 60f),
            O_Inv("Medicine"), O_Rep(15f), O_N("{name} bitkilerin iyileştirici gücünü sistematize etti!", "Surprising"));

        Add(e, "boat_invention", 3, "Invention", 0.03f, 0f, true,
            C("HasCard", "Zeka"), C("HasCard", "İnşaatçı"), C("ResourceAbove", "wood", 30f),
            O_Inv("Boat"), O_N("{name} suya yüzen bir taşıt yaptı!", "Surprising"));

        Add(e, "barter_system_established", 3, "Invention", 0.03f, 0f, true,
            C("HasCard", "Sosyal"), C("HasCard", "Zeka"), C("FriendCountAbove", "", 2f),
            O_Inv("TradeSystem"), O_Rep(20f), O_N("{name} köyde resmi takas sistemini kurdu!", "Major"));

        Add(e, "emergency_ration_creation", 3, "Invention", 0.04f, 0f, true,
            C("HasCard", "İnşaatçı"), C("HasCard", "Zeka"), C("NeedAbove", "hunger", 60f), C("ResourceAbove", "wood", 20f),
            O_Inv("Storage"), O_N("{name} yiyecek depolama sistemi tasarladı!", "Surprising"));

        Add(e, "genius_trap", 3, "Invention", 0.04f, 0f, true,
            C("HasCard", "Zeka"), C("HasCard", "Avcı"), C("NeedAbove", "hunger", 60f), C("ThresholdMet", "hunger_high", 90f),
            O_Inv("Trap"), O_Emo("Proud", 0.5f), O_N("{name} hayvan tuzağı kurdu! Artık et toplanabilir.", "Surprising"));

        Add(e, "energy_drink_discovery", 3, "Invention", 0.03f, 0f, true,
            C("HasCard", "Zeka"), C("HasCard", "Şaman"), C("ThresholdMet", "fatigue_high", 120f),
            O_Inv("EnergyDrink"), O_Need("fatigue", -40f), O_N("{name} enerji veren bir içecek icat etti!", "Surprising"));

        Add(e, "rope_making", 3, "Invention", 0.04f, 0f, true,
            C("HasCard", "İnşaatçı"), C("HasCard", "Zeka"),
            O_Inv("Rope"), O_N("{name} bitki liflerinden ip ördü!", "Surprising"));

        Add(e, "stone_knife", 3, "Invention", 0.04f, 0f, true,
            C("HasCard", "Zeka"),
            O_Inv("StoneKnife"), O_N("{name} keskin bir taş parçasının bıçak gibi kullanılabileceğini keşfetti!", "Surprising"));

        Add(e, "bread_baking", 3, "Invention", 0.03f, 0f, true,
            C("HasCard", "Zeka"), C("HasInvention", "Campfire"),
            O_Inv("Bread"), O_N("{name} tahılı suyla karıştırıp ateşte pişirdi. Ekmek icat edildi!", "Surprising"));

        Add(e, "fishing_net", 3, "Invention", 0.03f, 0f, true,
            C("HasCard", "Zeka"), C("HasCard", "Avcı"), C("HasInvention", "FishingRod"),
            O_Inv("FishingNet"), O_N("{name} ağ yaparak çok daha fazla balık tutmayı öğrendi!", "Surprising"));

        Add(e, "shelter_door", 3, "Invention", 0.04f, 0f, true,
            C("HasCard", "İnşaatçı"), C("NeedAbove", "fear", 40f),
            O_Inv("Door"), O_N("{name} evine kapı takmanın akıllıca olduğunu keşfetti!", "Surprising"));

        Add(e, "smoke_preservation", 3, "Invention", 0.03f, 0f, true,
            C("HasCard", "Zeka"), C("HasInvention", "Campfire"),
            O_Inv("Smoking"), O_N("{name} eti dumanla tütsüleyerek saklamayı öğrendi!", "Surprising"));

        Add(e, "drying_rack", 3, "Invention", 0.04f, 0f, true,
            C("HasCard", "İnşaatçı"), C("ResourceAbove", "food", 10f),
            O_Inv("DryingRack"), O_N("{name} yiyecekleri kurutarak saklama süresini uzattı!", "Surprising"));

        Add(e, "herbal_medicine_discovery", 3, "Invention", 0.04f, 0f, true,
            C("HasCard", "Zeka"), C("HasCard", "Maceracı"),
            O_Inv("Medicine"), O_Heal("nearest_sick"), O_N("{name} iyileştirici bir bitki keşfetti!", "Surprising"));

        Add(e, "animal_domestication", 3, "Invention", 0.03f, 0f, true,
            C("HasCard", "Zeka"), C("HasCard", "Avcı"),
            O_Inv("Livestock"), O_N("{name} vahşi bir hayvanı evcilleştirdi!", "Surprising"));

        // YENİ KARTLAR — İCAT P3
        Add(e, "haset_zeka_rakip_gecer", 3, "Invention", 0.05f, 240f, false,
            C("HasCard", "Haset"), C("HasCard", "Zeka"), C("EmotionAbove", "Jealous", 0.5f),
            O_Stat("workSpeed", 0.4f), O_N("{name} rakibini geçmek için beklenmedik bir yöntem geliştirdi!", "Surprising"));

        Add(e, "klumsy_accidental_invention", 3, "Comedy", 0.04f, 300f, false,
            C("HasCard", "Klumsy"), C("HasCard", "Deli"),
            O_Rep(5f), O_N("{name} bir şeyleri devirirken kazara önemli bir şey keşfetti.", "Surprising"));

        Add(e, "asiri_hevesli_icat_yarim", 3, "Comedy", 0.08f, 120f, false,
            C("HasCard", "AşırıHevesli"), C("HasCard", "Zeka"),
            O_N("{name} büyük bir icat üzerinde çalışmaya başladı. Sonra başka bir şeye geçti. Yarım icat köyde duruyor.", "Surprising"));

        Add(e, "unutan_icat_tekrar_kesfeder", 3, "Comedy", 0.05f, 300f, false,
            C("HasCard", "Unutkan"), C("HasCard", "Zeka"),
            O_Rep(3f), O_N("{name} daha önce aynı şeyi keşfetmişti. Unutmuş. Şimdi tekrar icat etti. İkinci kez gurur duyuyor.", "Surprising"));

        Add(e, "kor_ses_bazli_icat", 3, "Invention", 0.04f, 300f, false,
            C("HasCard", "Kör"), C("HasCard", "Zeka"), C("ThresholdMet", "lonely_long", 180f),
            O_Inv("SoundSystem"), O_Rep(10f), O_N("{name} göremediği için ses bazlı bir sistem geliştirdi. Köy şaştı.", "Surprising"));

        Add(e, "melancholy_art", 3, "Invention", 0.06f, 240f, false,
            C("EmotionAbove", "Sad", 0.5f), C("HasCard", "Sakin"), C("ThresholdMet", "idle_long", 180f),
            O_Inv("Art"), O_Rep(5f), O_N("{name} hüznünü sanatla ifade etti.", "Surprising"));

        // ── DUYGU VE PSİKOLOJİ — ORTA OYUN ───────────────
        Add(e, "jealous_murder", 3, "Conflict", 0.03f, 600f, false,
            C("HasCard", "Sinir"), C("HasCard", "Kıskanç"), C("EmotionAbove", "Angry", 0.75f), C("IsMarried"), C("SpouseCheating"),
            O_Kill("spouse_cheater"), O_Rep(-20f), O_Cascade("Murder", 1f), O_N("{name} karısını aldatan {target}ı öldürdü!", "Drama"));

        Add(e, "depression_refuses_work", 3, "Personal", 0.06f, 180f, false,
            C("EmotionAbove", "Depressed", 0.6f), C("NeedAbove", "loneliness", 80f),
            O_N("{name} artık çalışmak istemiyor. Derin bir hüzün içinde.", "Social"));

        Add(e, "revenge_plan", 3, "Conflict", 0.06f, 240f, false,
            C("EmotionAbove", "Vengeful", 0.5f), C("HasCard", "Zeka"), C("EnemyCountAbove", "", 0f),
            O_Emo("Vengeful", 0.3f), O_N("{name} sessiz sedasız intikamını planlamaya başladı.", "Drama"));

        Add(e, "loneliness_creates_imaginary_friend", 3, "Chaos", 0.04f, 300f, false,
            C("HasCard", "Deli"), C("NeedAbove", "loneliness", 90f), C("ThresholdMet", "lonely_long", 300f),
            O_Need("loneliness", -20f), O_N("{name} görünmez biriyle konuşuyor. Endişe verici.", "Surprising"));

        Add(e, "bitter_gossip", 3, "Social", 0.08f, 120f, false,
            C("EmotionAbove", "Bitter", 0.5f), C("HasCard", "Dedikoduncu"),
            O_Rel("nearest", -15f), O_Cascade("Scandal", 0.3f), O_N("{name} acımasız dedikodular yayıyor.", "Drama"));

        Add(e, "pride_fall", 3, "Personal", 0.08f, 180f, false,
            C("EmotionAbove", "Proud", 0.7f), C("ReputationBelow", "", 40f),
            O_Emo("Humiliated", 0.6f), O_Emo("Angry", 0.4f), O_N("{name}'ın gururu kırıldı. Bunu kaldırması zor.", "Drama"));

        Add(e, "shame_isolation", 3, "Social", 0.08f, 180f, false,
            C("EmotionAbove", "Humiliated", 0.7f), C("ReputationBelow", "", 30f),
            O_Stat("sociability", -10f), O_Need("loneliness", 20f), O_N("{name} utançtan köyün kenarına çekildi.", "Social"));

        Add(e, "trauma_flashback", 3, "Personal", 0.08f, 180f, false,
            C("NeedAbove", "fear", 50f), C("TrustBelow", "", 40f),
            O_Emo("Terrified", 0.5f), O_Need("fear", 20f), O_N("{name} eski bir travmayı hatırladı. Zor anlar.", "Social"));

        Add(e, "restless_leaves_village", 3, "Personal", 0.04f, 300f, false,
            C("HasCard", "Maceracı"), C("NeedAbove", "boredom", 90f), C("ThresholdMet", "idle_long", 240f),
            O_N("{name} köyden ayrıldı. Döner mi?", "Drama"));

        Add(e, "nostalgic_grief", 3, "Social", 0.05f, 600f, false,
            C("IsInMourning"), C("HasCard", "İnşaatçı"),
            O_Inv("Monument"), O_Rep(8f), O_N("{name} kaybettiği kişi için bir anıt yaptı.", "Social"));

        Add(e, "gurur_lider_felaket", 3, "Political", 0.04f, 300f, false,
            C("HasCard", "Gururlu"), C("HasCard", "Lider"), C("IsLeader"),
            O_Rep(-10f), O_Cascade("WeakLeader", 0.4f), O_N("{name} yanlış kararında ısrar ediyor. Lider yanılmaz, değil mi?", "Drama"));

        Add(e, "gurur_pismanlık_telafi", 3, "Personal", 0.07f, 180f, false,
            C("HasCard", "Gururlu"), C("EmotionAbove", "Guilty", 0.5f),
            O_Rep(8f), O_Need("loneliness", -10f), O_N("{name} özür dileyemiyor ama kendini mahvedercesine telafi ediyor.", "Surprising"));

        Add(e, "haset_sinir_sabotaj", 3, "Conflict", 0.05f, 240f, false,
            C("HasCard", "Haset"), C("HasCard", "Sinir"), C("EmotionAbove", "Jealous", 0.6f),
            O_Rep(-10f), O_Cascade("Scandal", 0.3f), O_N("{name} başkasının işini sabote etti!", "Drama"));

        Add(e, "bencil_cömert_catisma", 3, "Social", 0.08f, 120f, false,
            C("HasCard", "Bencil"), C("NeedAbove", "injustice", 40f),
            O_Rel("nearest", -20f), O_N("{name} ve cömert biri aynı kaynağa ulaştı. Tuhaf bir çekişme başladı.", "Drama"));

        // ── LİDERLİK — ORTA OYUN ───────────────────────────
        Add(e, "leader_election", 3, "Political", 0.15f, 600f, false,
            C("HasCard", "Lider"),
            O_MakeLeader("self"), O_Rep(20f), O_N("Köyde lider seçimi zamanı!", "Major"));

        Add(e, "wise_leader_decision", 3, "Political", 0.08f, 180f, false,
            C("HasCard", "Lider"), C("HasCard", "Zeka"), C("IsLeader"),
            O_Rep(15f), O_Emo("Proud", 0.5f), O_N("{name} zor bir karar vererek köyü kurtardı!", "Surprising"));

        Add(e, "mad_law", 3, "Chaos", 0.06f, 180f, false,
            C("HasCard", "Deli"), C("IsLeader"),
            O_Cascade("MadLaw", 0.4f), O_N("{name} saçma bir yasa çıkardı! Köy ne yapacağını bilmiyor.", "Surprising"));

        Add(e, "exhausted_leader", 3, "Political", 0.06f, 180f, false,
            C("HasCard", "Lider"), C("NeedAbove", "fatigue", 80f), C("IsLeader"),
            O_Rep(-5f), O_Cascade("WeakLeader", 0.3f), O_N("{name} yorgunluktan liderlik görevlerini yapamıyor.", "Social"));

        Add(e, "second_wind", 3, "Personal", 0.12f, 180f, false,
            C("HasCard", "Koruyucu"), C("NeedAbove", "fatigue", 70f), C("NeedAbove", "fear", 60f),
            O_Need("fatigue", -40f), O_Emo("Brave", 0.8f), O_N("{name} tehlike anında ikinci nefes buldu!", "Surprising"));

        Add(e, "bencil_lider_zimmet", 3, "Political", 0.04f, 300f, false,
            C("HasCard", "Bencil"), C("IsLeader"), C("ThresholdMet", "idle_long", 180f),
            O_Rep(-20f), O_Cascade("Rebellion", 0.5f), O_N("{name} lider olarak köy kaynaklarını zimmetine geçiriyor!", "Major"));

        Add(e, "rekabetci_secim_yarisi", 3, "Political", 0.06f, 240f, false,
            C("HasCard", "Rekabetçi"), C("HasCard", "Lider"),
            O_Rep(10f), O_Cascade("Election", 0.4f), O_N("{name} seçimi kazanmak için her şeyi yapıyor.", "Drama"));

        Add(e, "shupheci_lider_spy", 3, "Political", 0.06f, 300f, false,
            C("HasCard", "Şüpheci"), C("IsLeader"),
            O_Rep(15f), O_N("{name} köyde casus olduğundan şüpheleniyor. Ve genellikle haklı çıkıyor.", "Surprising"));

        Add(e, "mizmir_lider_moral_cokus", 3, "Political", 0.06f, 180f, false,
            C("HasCard", "Mızmız"), C("IsLeader"),
            O_Rep(-8f), O_N("{name} lider olunca tüm köy şikayetlerini dinlemek zorunda kaldı. Moral çöktü.", "Drama"));

        Add(e, "mizmir_vergi_sikayet", 3, "Political", 0.10f, 120f, false,
            C("HasCard", "Mızmız"), C("NeedAbove", "injustice", 40f),
            O_Rep(-3f), O_N("{name} vergilerden şikayet ediyor. Her zamanki gibi. Ama bu sefer haklı olabilir.", "Social"));

        Add(e, "leader_favoritism", 3, "Political", 0.06f, 180f, false,
            C("HasCard", "Lider"), C("IsLeader"), C("FriendCountAbove", "", 0f),
            O_Rep(-8f), O_Need("injustice", 15f), O_N("{name} lider olarak arkadaşına ayrıcalık tanıdı.", "Drama"));

        #region ═══════════════════════════════════════════
        // ÖNCELİK 4 — GEÇ OYUN (321-430)
        #endregion

        // ── BÜYÜK SOSYAL ÇÖKÜŞ ────────────────────────────
        Add(e, "divorce_scandal", 4, "Social", 0.05f, 300f, false,
            C("IsMarried"), C("RelationshipBelow", "spouse", 20f),
            O_Divorce("spouse"), O_Rep(-15f), O_Cascade("Divorce", 0.5f), O_N("{name} ve {spouse} boşandı! Köy taraf seçmek zorunda.", "Drama"));

        Add(e, "secret_affair_discovered", 4, "Drama", 0.06f, 300f, false,
            C("HasCard", "Çapkın"), C("HasCard", "Dedikoduncu"),
            O_Cascade("Scandal", 0.6f), O_Rep(-15f), O_N("{name}'ın gizli ilişkisi ortaya çıktı!", "Drama"));

        Add(e, "humiliation_revenge_plot", 4, "Conflict", 0.06f, 240f, false,
            C("EmotionAbove", "Humiliated", 0.6f), C("ReputationBelow", "", 25f), C("HasCard", "Sinir"),
            O_Emo("Vengeful", 0.5f), O_N("{name} alenen aşağılandı. Bunu unutmayacak.", "Drama"));

        Add(e, "betrayal_revenge", 4, "Conflict", 0.05f, 300f, false,
            C("EmotionAbove", "Vengeful", 0.6f), C("EnemyCountAbove", "", 0f),
            O_Attack("nearest_enemy"), O_Cascade("Fight", 0.7f), O_N("{name} uzun süre bekledi. İntikam vakti geldi.", "Drama"));

        Add(e, "village_exile_criminal", 4, "Social", 0.15f, 0f, false,
            C("ReputationBelow", "", 10f), C("EmotionAbove", "Humiliated", 0.7f),
            O_Stat("sociability", -20f), O_Need("loneliness", 20f), O_Cascade("Exile", 0.5f), O_N("{name} köyden kovuldu!", "Major"));

        Add(e, "blood_feud", 4, "Conflict", 0.06f, 0f, false,
            C("HasCard", "Sinir"), C("EmotionAbove", "Vengeful", 0.7f), C("EnemyCountAbove", "", 0f),
            O_Attack("nearest_enemy"), O_Cascade("BloodFeud", 0.8f), O_N("{name}'ın ailesi kan davası başlattı!", "Major"));

        Add(e, "honor_duel", 4, "Conflict", 0.05f, 300f, false,
            C("HasCard", "Sinir"), C("EnemyCountAbove", "", 0f), C("EmotionAbove", "Humiliated", 0.5f),
            O_Attack("nearest_enemy"), O_Cascade("PublicFight", 0.5f), O_N("{name} ve {rival} düello için karşı karşıya!", "Drama"));

        Add(e, "plague_outbreak", 4, "Nature", 0.08f, 600f, false,
            C("IsSick"), C("IsWinter"),
            O_Cascade("Plague", 0.7f), O_N("Köyde salgın başladı! {name} ilk hasta.", "Major"));

        Add(e, "scapegoating", 4, "Social", 0.05f, 300f, false,
            C("ReputationBelow", "", 30f), C("HasCard", "Dedikoduncu"),
            O_Rep(-15f), O_N("{name} köy sorunlarının günah keçisi yapıldı!", "Drama"));

        Add(e, "mass_panic", 4, "Nature", 0.04f, 300f, false,
            C("NeedAbove", "fear", 70f), C("PopulationAbove", "", 5f),
            O_Cascade("VillagePanic", 0.8f), O_N("Köyde toplu panik başladı!", "Major"));

        // ── SİYASİ ÇATIŞMA ─────────────────────────────────
        Add(e, "coup_attempt", 4, "Political", 0.04f, 600f, false,
            C("HasCard", "Lider"), C("HasCard", "Sinir"), C("ReputationAbove", "", 65f),
            O_Cascade("PowerChallenge", 0.8f), O_FormFaction("CoupFaction"), O_N("{name} liderliğe el koymak istiyor!", "Major"));

        Add(e, "grassroots_rebellion", 4, "Political", 0.05f, 600f, false,
            C("NeedAbove", "injustice", 80f), C("ReputationBelow", "", 20f),
            O_Cascade("Rebellion", 0.9f), O_N("Köyde isyan başladı! {name}'a karşı ayaklanma.", "Major"));

        Add(e, "faction_war", 4, "Political", 0.04f, 600f, false,
            C("FactionExists"), C("EnemyCountAbove", "", 2f), C("ThresholdMet", "vengeful_state", 300f),
            O_Cascade("War", 1f), O_N("Fraksiyonlar arasında savaş başladı!", "Major"));

        Add(e, "holy_war_declared", 4, "Religion", 0.02f, 900f, false,
            C("HasCard", "Şaman"), C("IsLeader"), C("FactionExists"),
            O_Cascade("HolyWar", 1f), O_N("{name} inanç adına savaş ilan etti!", "Major"));

        Add(e, "dynasty_founded", 4, "Political", 0.03f, 0f, true,
            C("HasCard", "Lider"), C("IsLeader"), C("GenerationAbove", "", 2f), C("ReputationAbove", "", 70f),
            O_Rep(25f), O_N("{name}'ın soyu artık bir hanedanlık!", "Major"));

        Add(e, "justice_reform", 4, "Political", 0.03f, 0f, true,
            C("HasCard", "Zeka"), C("IsLeader"), C("NeedAbove", "injustice", 50f), C("HasInvention", "Writing"),
            O_Inv("LegalCode"), O_Rep(20f), O_Need("injustice", -30f), O_N("{name} köyün ilk yazılı kurallarını koydu!", "Major"));

        Add(e, "haset_siyasi_sabotaj", 4, "Political", 0.05f, 240f, false,
            C("HasCard", "Haset"), C("EmotionAbove", "Jealous", 0.6f), C("FactionExists"),
            O_Rep(-8f), O_Cascade("Scandal", 0.4f), O_N("{name} rakibinin yükselmesini kıskandı ve sabotaja girişti.", "Drama"));

        Add(e, "tax_rebellion", 4, "Political", 0.05f, 300f, false,
            C("IsLeader"), C("NeedAbove", "injustice", 60f),
            O_Cascade("Rebellion", 0.6f), O_N("Vergilere isyan başladı!", "Major"));

        Add(e, "propaganda_spread", 4, "Political", 0.06f, 240f, false,
            C("HasCard", "Dedikoduncu"), C("HasCard", "Lider"), C("FactionExists"),
            O_Rep(-5f), O_N("{name} rakibi hakkında propaganda yapıyor.", "Drama"));

        Add(e, "spy_discovered", 4, "Political", 0.04f, 300f, false,
            C("HasCard", "Dedikoduncu"), C("HasCard", "Zeka"), C("FactionExists"),
            O_Cascade("Scandal", 0.7f), O_N("{name} köyde casus olduğu için yakalandı!", "Major"));

        Add(e, "public_execution", 4, "Political", 0.03f, 300f, false,
            C("HasCard", "Sinir"), C("IsLeader"), C("EnemyCountAbove", "", 0f),
            O_Rep(-10f), O_Need("fear", 20f), O_N("{name} suçluyu halkın önünde cezalandırdı.", "Drama"));

        // ── BÜYÜK İCATLAR ─────────────────────────────────
        Add(e, "writing_invention", 4, "Invention", 0.02f, 0f, true,
            C("HasCard", "Zeka"), C("HasCard", "Sakin"), C("ThresholdMet", "lonely_long", 300f),
            O_Inv("Writing"), O_Rep(25f), O_N("{name} düşüncelerini taşa kazıdı. Yazı başladı!", "Major"));

        Add(e, "irrigation_discovery", 4, "Invention", 0.02f, 0f, true,
            C("HasCard", "Zeka"), C("HasCard", "İnşaatçı"), C("ResourceBelow", "water", 5f),
            O_Inv("Irrigation"), O_Rep(20f), O_N("{name} nehir suyunu tarlalara yönlendirmeyi keşfetti!", "Major"));

        Add(e, "metallurgy_hint", 4, "Invention", 0.02f, 0f, true,
            C("HasCard", "Zeka"), C("ResourceAbove", "stone", 10f), C("HasInvention", "Campfire"),
            O_Inv("Metallurgy"), O_Rep(20f), O_N("{name} ateşte metali eritip şekillendirebildiğini keşfetti!", "Major"));

        Add(e, "calendar_invention", 4, "Invention", 0.02f, 0f, true,
            C("HasCard", "Zeka"), C("ThresholdMet", "idle_long", 300f),
            O_Inv("Calendar"), O_Rep(15f), O_N("{name} mevsimlerin döngüsünü hesaplamayı öğrendi!", "Surprising"));

        Add(e, "astronomy_basics", 4, "Invention", 0.02f, 0f, true,
            C("HasCard", "Zeka"), C("HasCard", "Sakin"), C("IsNight"), C("ThresholdMet", "idle_long", 240f),
            O_Inv("Astronomy"), O_Rep(12f), O_N("{name} geceleri gökyüzünü inceliyor. Yıldız haritası yapıyor.", "Surprising"));

        Add(e, "crop_rotation", 4, "Invention", 0.02f, 0f, true,
            C("HasCard", "Zeka"), C("HasInvention", "Farming"),
            O_Inv("CropRotation"), O_N("{name} tarlaları dinlendirmenin faydalarını keşfetti!", "Surprising"));

        Add(e, "map_drawing", 4, "Invention", 0.03f, 0f, true,
            C("HasCard", "Maceracı"), C("HasCard", "Zeka"),
            O_Inv("Map"), O_N("{name} çevrenin haritasını çizdi!", "Surprising"));

        Add(e, "communal_granary", 4, "Invention", 0.03f, 0f, true,
            C("HasCard", "İnşaatçı"), C("HasCard", "Lider"), C("HasInvention", "Farming"),
            O_Inv("Granary"), O_N("{name} köy için ortak tahıl ambarı yaptırdı!", "Surprising"));

        // ── NESİL VE EVRİM ─────────────────────────────────
        Add(e, "trauma_inherited", 4, "Evolution", 0.08f, 0f, false,
            C("TrustBelow", "", 40f), C("GenerationAbove", "", 0f),
            O_Stat("fearThreshold", -10f), O_N("{name}, ebeveyninin yaşadıklarının izlerini taşıyor.", "Social"));

        Add(e, "genius_child_born", 4, "Evolution", 0.15f, 0f, false,
            C("HasCard", "Zeka"), C("GenerationAbove", "", 0f),
            O_Stat("learningRate", 0.5f), O_N("{name}'ın çocuğu olağanüstü zeki doğdu!", "Surprising"));

        Add(e, "family_feud_inheritance", 4, "Conflict", 0.10f, 0f, false,
            C("GenerationAbove", "", 1f), C("EnemyCountAbove", "", 0f),
            O_Attack("nearest"), O_Cascade("Fight", 0.4f), O_N("{name}'ın ailesi miras yüzünden kavgaya tutuştu!", "Drama"));

        Add(e, "generational_conflict", 4, "Social", 0.06f, 300f, false,
            C("GenerationAbove", "", 1f), C("EnemyCountAbove", "", 0f),
            O_Rep(-5f), O_N("Yaşlılar ve gençler arasında görüş ayrılığı büyüyor!", "Social"));

        Add(e, "kisaomurlu_olum_dramatik", 4, "Drama", 0.05f, 0f, false,
            C("HasCard", "KısaÖmürlü"), C("IsMarried"), C("AgeAbove", "", 15f),
            O_Emo("Sad", 0.9f), O_Cascade("Death", 0.6f), O_N("{name} sevgilisinden önce ölmek üzere. Köyde sessizlik var.", "Drama"));

        Add(e, "uzunomurlu_herkesi_kaybeder", 4, "Personal", 0.05f, 600f, false,
            C("HasCard", "UzunÖmürlü"), C("GenerationAbove", "", 1f),
            O_Need("loneliness", 30f), O_Emo("Sad", 0.6f), O_N("{name} tanıdığı herkesi kaybetti. Hâlâ yaşıyor.", "Drama"));

        Add(e, "uzunomurlu_lider_efsane", 4, "Political", 0.04f, 600f, false,
            C("HasCard", "UzunÖmürlü"), C("HasCard", "Lider"), C("IsLeader"), C("GenerationAbove", "", 1f),
            O_Rep(20f), O_N("{name} nesiller boyunca lider olarak kaldı. Artık bir efsane.", "Major"));

        Add(e, "yansitici_es_olum_yikici", 4, "Drama", 0.08f, 0f, false,
            C("HasCard", "Yansıtıcı"), C("IsInMourning"),
            O_Emo("Depressed", 0.9f), O_Need("loneliness", 40f), O_N("{name} aynı kartı taşıyan eşini kaybetti. Bu yası atlatamayabilir.", "Drama"));

        Add(e, "gurur_soy_dynasty", 4, "Political", 0.05f, 600f, false,
            C("HasCard", "Gururlu"), C("HasCard", "Lider"), C("GenerationAbove", "", 2f),
            O_Rep(20f), O_N("{name}'ın gururlu soyu nesiller boyunca liderlik etti.", "Major"));

        Add(e, "kisaomurlu_hizli_ureme", 4, "Evolution", 0.12f, 60f, false,
            C("HasCard", "KısaÖmürlü"), C("NeedBelow", "fatigue", 50f),
            O_Marry("nearest"), O_N("{name} çok az zamanı olduğunu biliyor. Aileyi hızlı kuruyor.", "Social"));

        Add(e, "generational_grudge", 4, "Conflict", 0.05f, 0f, false,
            C("HasCard", "Sinir"), C("EnemyCountAbove", "", 0f), C("GenerationAbove", "", 1f),
            O_Attack("nearest_enemy"), O_N("Kan davası bir nesil sonra da devam ediyor.", "Drama"));

        Add(e, "hybrid_vigor", 4, "Evolution", 0.06f, 0f, false,
            C("GenerationAbove", "", 1f), C("FriendCountAbove", "", 2f),
            O_Stat("learningRate", 0.3f), O_N("{name} iki farklı soyun en iyi özelliklerini taşıyor!", "Surprising"));

        // ── BÜYÜK DİNİ OLAYLAR ─────────────────────────────
        Add(e, "competing_shamans", 4, "Religion", 0.04f, 300f, false,
            C("HasCard", "Şaman"), C("FactionExists"),
            O_Cascade("Scandal", 0.4f), O_N("İki şaman güç için yarışıyor!", "Drama"));

        Add(e, "prophecy_fulfilled", 4, "Religion", 0.05f, 0f, false,
            C("HasCard", "Şaman"), C("ReputationAbove", "", 60f),
            O_Rep(20f), O_N("{name}'ın kehaneti gerçekleşti! Köy inanmak zorunda kaldı.", "Major"));

        Add(e, "ancestor_worship", 4, "Religion", 0.04f, 0f, true,
            C("GenerationAbove", "", 1f), C("HasCard", "Şaman"),
            O_Rep(10f), O_N("{name} ataları onurlandırma geleneği başlattı!", "Religion"));

        Add(e, "creation_myth", 4, "Religion", 0.02f, 0f, true,
            C("HasCard", "Şaman"), C("HasCard", "Zeka"), C("HasInvention", "Writing"),
            O_Inv("CreationMyth"), O_Rep(15f), O_N("{name} köyün yaratılış mitini yazdı!", "Major"));

        Add(e, "divine_punishment_claim", 4, "Religion", 0.06f, 180f, false,
            C("HasCard", "Şaman"), C("HasCard", "Sinir"), C("NeedAbove", "injustice", 50f),
            O_Need("fear", 15f), O_N("{name} felaketin {target}'ın günahı yüzünden geldiğini söylüyor!", "Drama"));

        #region ═══════════════════════════════════════════
        // ÖNCELİK 5 — OYUN SONU (431-500+)
        #endregion

        Add(e, "population_collapse", 5, "EndGame", 0.15f, 120f, false,
            C("IsWinter"), C("ResourceBelow", "food", 3f), C("NeedAbove", "hunger", 80f),
            O_Emo("Desperate", 0.9f), O_N("Köy çöküşün eşiğinde! Son minyonlar hayatta kalabilecek mi?", "Major"));

        Add(e, "golden_age", 5, "EndGame", 0.05f, 600f, false,
            C("NeedBelow", "hunger", 30f), C("NeedBelow", "fatigue", 30f), C("ReputationAbove", "", 70f),
            O_Emo("Euphoric", 0.7f), O_Stat("workSpeed", 0.3f), O_N("Köy altın çağında! Her şey mükemmel.", "Major"));

        Add(e, "new_beginning", 5, "EndGame", 0.10f, 0f, false,
            C("GenerationAbove", "", 1f), C("NeedBelow", "hunger", 50f),
            O_Emo("Hopeful", 0.6f), O_N("Yeni nesil devralıyor. Köy yeni bir sayfa açıyor.", "Social"));

        Add(e, "new_dawn", 5, "EndGame", 0.05f, 600f, false,
            C("GenerationAbove", "", 2f), C("NeedBelow", "hunger", 40f), C("ReputationAbove", "", 50f),
            O_Emo("Euphoric", 0.6f), O_Rep(15f), O_N("Her şeye rağmen köy ayakta! Yeni bir sabah başlıyor.", "Major"));

        Add(e, "village_first_law", 5, "Political", 0.03f, 0f, true,
            C("HasCard", "Lider"), C("HasCard", "Zeka"), C("IsLeader"), C("HasInvention", "Writing"),
            O_Inv("LegalCode"), O_Rep(20f), O_Need("injustice", -20f), O_N("{name} köyün ilk yasasını yazdı!", "Major"));

        Add(e, "last_words_of_wisdom", 5, "EndGame", 0.08f, 0f, false,
            C("HasCard", "Zeka"), C("AgeAbove", "", 38f),
            O_Stat("learningRate", 0.2f), O_Rep(10f), O_N("{name}'ın son sözleri köy tarihine geçecek.", "Social"));

        Add(e, "hope_in_darkness", 5, "EndGame", 0.08f, 300f, false,
            C("NeedAbove", "hunger", 70f), C("GenerationAbove", "", 0f),
            O_Emo("Hopeful", 0.6f), O_Need("loneliness", -20f), O_N("Her şey kötü giderken yeni bir hayat başladı. Umut var.", "Social"));

        Add(e, "essence_miracle_food", 5, "Divine", 0.10f, 300f, false,
            C("HasCard", "Şaman"), C("NeedAbove", "coldness", 30f),
            O_Rep(15f), O_Emo("Happy", 0.5f), O_N("Yağmur geldi! Köy bunun tesadüf olmadığını hissediyor.", "Religion"));

        Add(e, "essence_intervention_noticed", 5, "Divine", 0.08f, 300f, false,
            C("HasCard", "Şaman"), C("ReputationAbove", "", 40f),
            O_Rep(10f), O_N("{name} tanrıların aktif olduğunu hissetti! Törenler başladı.", "Religion"));

        Add(e, "repeated_intervention_addiction", 5, "Divine", 0.05f, 600f, false,
            C("HasCard", "Şaman"), C("ThresholdMet", "idle_long", 300f),
            O_Stat("workSpeed", -0.1f), O_N("Köy artık kendi sorunlarını çözmek yerine tanrıdan bekliyor.", "Social"));

        Add(e, "village_identity_formed", 5, "EndGame", 0.05f, 0f, true,
            C("GenerationAbove", "", 2f), C("ReputationAbove", "", 60f),
            O_Rep(15f), O_N("Köy artık kendi kimliğine sahip. Bir kültür doğdu.", "Major"));

        Add(e, "legendary_minion_emerges", 5, "EndGame", 0.03f, 0f, false,
            C("ReputationAbove", "", 90f), C("GenerationAbove", "", 2f),
            O_Rep(20f), O_N("{name} efsane olmaya aday. Köy bunu hissediyor.", "Major"));

        Add(e, "uzunomurlu_son_tanik", 5, "EndGame", 0.05f, 0f, false,
            C("HasCard", "UzunÖmürlü"), C("GenerationAbove", "", 2f), C("NeedAbove", "loneliness", 80f),
            O_Emo("Sad", 0.9f), O_N("{name} köyün kurucularını hatırlayan son kişi. Herkes gitti, o hâlâ burada.", "Drama"));

        Add(e, "gurur_son_an_kabul", 5, "EndGame", 0.06f, 0f, false,
            C("HasCard", "Gururlu"), C("AgeAbove", "", 38f),
            O_Rep(10f), O_Emo("Peaceful", 0.7f), O_N("{name} ömrü boyunca hiçbir şeyi kabul etmedi. Ölürken ilk kez boyun eğdi.", "Drama"));

        Add(e, "kisaomurlu_miras_hizli", 5, "EndGame", 0.08f, 0f, false,
            C("HasCard", "KısaÖmürlü"), C("AgeAbove", "", 12f),
            O_Rep(5f), O_N("{name} çok az vakti olduğunu biliyor. Bildiklerini hızla aktarıyor.", "Social"));

        Add(e, "uzunomurlu_dynasty_max", 5, "EndGame", 0.02f, 0f, true,
            C("HasCard", "UzunÖmürlü"), C("HasCard", "Lider"), C("IsLeader"), C("GenerationAbove", "", 2f), C("ReputationAbove", "", 80f),
            O_Rep(30f), O_N("{name}'ın soyu nesiller boyunca köyü yönetti. Bu, köy tarihinin en uzun hanedanlığı.", "Major"));

        Add(e, "dynasty_collapses", 5, "EndGame", 0.04f, 0f, false,
            C("HasCard", "Lider"), C("IsLeader"), C("GenerationAbove", "", 3f), C("IsInMourning"),
            O_Cascade("PowerVacuum", 0.9f), O_N("{name}'ın hanedanlığı sona erdi! Köy yeni bir sayfa açıyor.", "Major"));

        Add(e, "village_song", 5, "EndGame", 0.04f, 0f, true,
            C("HasInvention", "Music"), C("GenerationAbove", "", 2f), C("HasCard", "Sosyal"),
            O_Rep(10f), O_N("Köyün kendi şarkısı doğdu! Bu nesillere aktarılacak.", "Major"));

        Add(e, "great_flood_survival", 5, "Nature", 0.04f, 0f, false,
            C("IsWinter"), C("PopulationAbove", "", 5f), C("ResourceAbove", "wood", 20f),
            O_Rep(15f), O_Emo("Hopeful", 0.5f), O_N("Köy büyük felaketten kurtuldu! Bu hikaye nesillerce anlatılacak.", "Major"));

        Add(e, "dark_age", 5, "EndGame", 0.03f, 300f, false,
            C("ResourceBelow", "food", 5f), C("IsWinter"), C("ReputationBelow", "", 20f),
            O_Cascade("DarkAge", 0.8f), O_N("Köy tarihinin en karanlık dönemini yaşıyor.", "Major"));

        Add(e, "renaissance", 5, "EndGame", 0.03f, 600f, false,
            C("HasCard", "Zeka"), C("IsLeader"), C("HasInvention", "Writing"), C("ReputationAbove", "", 50f),
            O_Stat("workSpeed", 0.2f), O_Rep(10f), O_N("Karanlıktan sonra aydınlık! Köy yeniden canlanıyor.", "Major"));

        Add(e, "knowledge_preserved", 5, "Invention", 0.05f, 0f, false,
            C("HasInvention", "Writing"), C("GenerationAbove", "", 2f),
            O_Stat("learningRate", 0.3f), O_N("{name} önemli bilgiyi kayıt altına aldı. Nesiller bunu hatırlayacak.", "Major"));

        Add(e, "village_monument", 5, "EndGame", 0.04f, 0f, true,
            C("HasCard", "İnşaatçı"), C("HasCard", "Lider"), C("IsLeader"), C("ReputationAbove", "", 70f),
            O_Inv("Monument"), O_Rep(15f), O_N("{name} köy için büyük bir anıt inşa ettirdi!", "Major"));

        Add(e, "shupheci_tanri_sorguluyor", 5, "Divine", 0.06f, 240f, false,
            C("HasCard", "Şüpheci"), C("HasCard", "Şaman"),
            O_Rep(-3f), O_N("{name} tanrısal müdahaleyi sorgulayan tek kişi. 'Bunun doğal açıklaması var' diyor.", "Surprising"));

        Add(e, "onsezili_essence_hisseder", 5, "Divine", 0.07f, 180f, false,
            C("HasCard", "Önsezili"),
            O_Need("fear", -10f), O_N("{name} bir şeylerin değiştiğini hissetti. 'Tanrılar burada' dedi.", "Religion"));

        Add(e, "mizmir_tanri_sikayet", 5, "Comedy", 0.06f, 240f, false,
            C("HasCard", "Mızmız"), C("ReputationAbove", "", 30f),
            O_N("{name} tanrıların felaketi önlediğine şükretti ama 'neden bu kadar geç müdahale ettiler' diye şikayet etti.", "Surprising"));

        Add(e, "forgotten_hero", 5, "EndGame", 0.05f, 0f, false,
            C("GenerationAbove", "", 3f), C("ReputationBelow", "", 30f),
            O_N("{name}'ın yaptıkları artık kimse tarafından hatırlanmıyor.", "Social"));

        Add(e, "remembered_forever", 5, "EndGame", 0.04f, 0f, false,
            C("GenerationAbove", "", 2f), C("HasInvention", "Writing"), C("ReputationAbove", "", 80f),
            O_Rep(10f), O_N("{name}'ın hikayeleri nesilden nesile anlatılacak.", "Major"));

        Add(e, "the_great_mistake", 5, "EndGame", 0.02f, 0f, false,
            C("GenerationAbove", "", 2f), C("ReputationBelow", "", 15f), C("NeedAbove", "injustice", 70f),
            O_Cascade("DarkAge", 0.5f), O_N("Bu hata köy tarihine geçecek.", "Major"));

        Add(e, "population_boom", 5, "EndGame", 0.05f, 300f, false,
            C("NeedBelow", "hunger", 30f), C("NeedBelow", "fatigue", 30f), C("PopulationAbove", "", 5f),
            O_N("Köyde bebek patlaması! Nüfus hızla artıyor.", "Social"));

        Add(e, "great_invention_era", 5, "Invention", 0.04f, 0f, false,
            C("HasCard", "Zeka"), C("GenerationAbove", "", 2f), C("ReputationAbove", "", 60f),
            O_Stat("learningRate", 0.5f), O_N("Köy icat çağına girdi! Her gün yeni bir şey keşfediliyor.", "Major"));

        Add(e, "cascade_of_cascades", 5, "Chaos", 0.03f, 300f, false,
            C("NeedAbove", "fear", 70f), C("NeedAbove", "injustice", 60f), C("ReputationBelow", "", 25f),
            O_Cascade("TotalChaos", 1f), O_N("Her şey aynı anda oluyor! Köyde tam kaos.", "Major"));

        Add(e, "all_needs_met_simultaneously", 5, "EndGame", 0.02f, 600f, false,
            C("NeedBelow", "hunger", 20f), C("NeedBelow", "fatigue", 20f), C("NeedBelow", "loneliness", 20f), C("ReputationAbove", "", 60f),
            O_Emo("Euphoric", 0.8f), O_N("Nadir bir an: Köydeki herkes mutlu ve tok!", "Major"));

        return e;
    }

    // ================================================================
    // FACTORY METODLARI — kısa ve okunabilir
    // ================================================================

    static void Add(List<RandomEventData> list, string id, int priority, string category,
        float chance, float cd, bool unique,
        params object[] items)
    {
        var evt = new RandomEventData
        {
            eventId = id,
            priority = priority,
            category = category,
            triggerChance = chance,
            cooldown = cd,
            isUnique = unique
        };

        foreach (var item in items)
        {
            if (item is EventCondition c) evt.conditions.Add(c);
            else if (item is EventOutcome o) evt.outcomes.Add(o);
        }

        list.Add(evt);
    }

    // Condition factories
    static EventCondition C(string type, string param = "", float val = 0f)
    {
        var c = new EventCondition { parameter = param, value = val };
        switch (type)
        {
            case "HasCard": c.conditionType = EventCondition.ConditionType.HasCard; break;
            case "NeedAbove": c.conditionType = EventCondition.ConditionType.NeedAbove; break;
            case "NeedBelow": c.conditionType = EventCondition.ConditionType.NeedBelow; break;
            case "EmotionAbove": c.conditionType = EventCondition.ConditionType.EmotionAbove; break;
            case "EmotionBelow": c.conditionType = EventCondition.ConditionType.EmotionBelow; break;
            case "RelationshipBelow": c.conditionType = EventCondition.ConditionType.RelationshipBelow; break;
            case "RelationshipAbove": c.conditionType = EventCondition.ConditionType.RelationshipAbove; break;
            case "ThresholdMet": c.conditionType = EventCondition.ConditionType.ThresholdMet; break;
            case "IsMarried": c.conditionType = EventCondition.ConditionType.IsMarried; break;
            case "IsLeader": c.conditionType = EventCondition.ConditionType.IsLeader; break;
            case "HasInvention": c.conditionType = EventCondition.ConditionType.HasInvention; break;
            case "ReputationBelow": c.conditionType = EventCondition.ConditionType.ReputationBelow; break;
            case "ReputationAbove": c.conditionType = EventCondition.ConditionType.ReputationAbove; break;
            case "GenerationAbove": c.conditionType = EventCondition.ConditionType.GenerationAbove; break;
            case "FriendCountAbove": c.conditionType = EventCondition.ConditionType.FriendCountAbove; break;
            case "EnemyCountAbove": c.conditionType = EventCondition.ConditionType.EnemyCountAbove; break;
            case "IsNight": c.conditionType = EventCondition.ConditionType.IsNight; break;
            case "IsWinter": c.conditionType = EventCondition.ConditionType.IsWinter; break;
            case "HasShelter": c.conditionType = EventCondition.ConditionType.HasShelter; break;
            case "IsInjured": c.conditionType = EventCondition.ConditionType.IsInjured; break;
            case "IsSick": c.conditionType = EventCondition.ConditionType.IsSick; break;
            case "SpouseCheating": c.conditionType = EventCondition.ConditionType.SpouseCheating; break;
            case "NearbyFightExists": c.conditionType = EventCondition.ConditionType.NearbyFightExists; break;
            case "ResourceAbove": c.conditionType = EventCondition.ConditionType.ResourceAbove; break;
            case "ResourceBelow": c.conditionType = EventCondition.ConditionType.ResourceBelow; break;
            case "AgeAbove": c.conditionType = EventCondition.ConditionType.AgeAbove; break;
            case "HoardingAbove": c.conditionType = EventCondition.ConditionType.HoardingAbove; break;
            case "TrustBelow": c.conditionType = EventCondition.ConditionType.TrustBelow; break;
            case "IsInMourning": c.conditionType = EventCondition.ConditionType.IsInMourning; break;
            case "FactionExists": c.conditionType = EventCondition.ConditionType.FactionExists; break;
            case "PopulationAbove": c.conditionType = EventCondition.ConditionType.PopulationAbove; break;
            case "PopulationBelow": c.conditionType = EventCondition.ConditionType.PopulationBelow; break;
        }
        return c;
    }

    // Outcome factories
    static EventOutcome O_Inv(string item) => new EventOutcome { outcomeType = EventOutcome.OutcomeType.InventItem, parameter = item };
    static EventOutcome O_Emo(string em, float v) => new EventOutcome { outcomeType = EventOutcome.OutcomeType.SetEmotion, parameter = em, value = v };
    static EventOutcome O_Need(string n, float v) => new EventOutcome { outcomeType = EventOutcome.OutcomeType.ChangeNeed, parameter = n, value = v };
    static EventOutcome O_Stat(string s, float v) => new EventOutcome { outcomeType = EventOutcome.OutcomeType.ChangeStat, parameter = s, value = v };
    static EventOutcome O_Rep(float v) => new EventOutcome { outcomeType = EventOutcome.OutcomeType.ChangeReputation, value = v };
    static EventOutcome O_Rel(string t, float v) => new EventOutcome { outcomeType = EventOutcome.OutcomeType.ChangeRelationship, parameter = t, value = v };
    static EventOutcome O_Attack(string t) => new EventOutcome { outcomeType = EventOutcome.OutcomeType.AttackMinion, parameter = t };
    static EventOutcome O_Kill(string t) => new EventOutcome { outcomeType = EventOutcome.OutcomeType.KillMinion, parameter = t };
    static EventOutcome O_Steal(string t) => new EventOutcome { outcomeType = EventOutcome.OutcomeType.StealFrom, parameter = t };
    static EventOutcome O_Marry(string t) => new EventOutcome { outcomeType = EventOutcome.OutcomeType.Marry, parameter = t };
    static EventOutcome O_Divorce(string t) => new EventOutcome { outcomeType = EventOutcome.OutcomeType.Divorce, parameter = t };
    static EventOutcome O_Heal(string t) => new EventOutcome { outcomeType = EventOutcome.OutcomeType.HealMinion, parameter = t };
    static EventOutcome O_Cascade(string t, float v) => new EventOutcome { outcomeType = EventOutcome.OutcomeType.TriggerCascade, parameter = t, value = v };
    static EventOutcome O_MakeLeader(string t) => new EventOutcome { outcomeType = EventOutcome.OutcomeType.MakeLeader, parameter = t };
    static EventOutcome O_FormFaction(string n) => new EventOutcome { outcomeType = EventOutcome.OutcomeType.FormFaction, parameter = n };
    static EventOutcome O_BecomeEnemies(string t) => new EventOutcome { outcomeType = EventOutcome.OutcomeType.BecomeEnemies, parameter = t };
    static EventOutcome O_N(string msg, string type) => new EventOutcome
    {
        outcomeType = EventOutcome.OutcomeType.ShowNotification,
        notificationMessage = msg,
        notificationType = type switch
        {
            "Drama" => NotificationType.Drama,
            "Conflict" => NotificationType.Conflict,
            "Surprising" => NotificationType.Surprising,
            "Major" => NotificationType.Major,
            "Death" => NotificationType.Death,
            "Evolution" => NotificationType.Evolution,
            "Religion" => NotificationType.Religion,
            "Nature" => NotificationType.Nature,
            "Comedy" => NotificationType.Comedy,
            _ => NotificationType.Social
        }
    };
}