using GTA;
using GTA.Native;
using GTA.Math;
using System;
using System.Windows.Forms;
using System.Collections.Generic;

public class PlayerEnhancements : Script
{
    private Dictionary<Vehicle, DateTime> vehiclesToDelete = new Dictionary<Vehicle, DateTime>();

    private bool enhancementsEnabled = false; // Özellikler aktif mi?
    private const float speedMultiplier = 2.0f; // Koşma hızı çarpanı
    private const float moveRateMultiplier = 1.5f; // Hareket oranı çarpanı
    private const int maxHealth = 2000; // Maksimum can
    private DateTime airTimeStart = DateTime.MinValue; // Havada kalma süresini takip eder

    private Vector3 startPoint = new Vector3(-350, -620, 220); // A noktası
    private Vector3 endPoint = new Vector3(-163, -686, 376); // B noktası

    private int lapCount = 0; // Toplam lap sayısı
    private bool hasReached100 = false; // %100'e ulaşıldı mı?

    public PlayerEnhancements()
    {
        Tick += OnTick; // Her karede çalışır
        KeyDown += OnKeyDown; // Tuşlara basıldığında tetiklenir
        KeyDown += OnKeyDownHandler; 
    }

    private void OnTick(object sender, EventArgs e)
    {
        if (enhancementsEnabled)
        {
            try
            {
                List<Vehicle> vehiclesToRemove = new List<Vehicle>();
                // Sprint hızını artır
                IncreasePlayerSpeed();

                // Stamina sınırlamasını kaldır
                Function.Call(Hash.RESTORE_PLAYER_STAMINA, Game.Player.Handle);

                // Can ve HUD güncelleme
                UpdateHealth();

                // Yüksekteyken otomatik ışınla
                CheckAndTeleportIfInAir();

                // İlerleme yüzdesini göster ve lap mantığını kontrol et
                float progress = CalculateProgress();
                DisplayProgress(progress);
                CheckLap(progress);
                //////
                Ped[] allPeds = World.GetAllPeds();

                foreach (Ped ped in allPeds)
                {
                    // Ped geçerli mi ve oyuncu karakteri değil mi?
                    if (ped != null && ped.Exists() && !ped.IsPlayer)
                    {
                        // Kan efektlerini temizle
                        Function.Call(Hash.CLEAR_PED_BLOOD_DAMAGE, ped.Handle);

                        // Yanma efektlerini temizle
                        if (Function.Call<bool>(Hash.IS_ENTITY_ON_FIRE, ped.Handle))
                        {
                            Function.Call(Hash.STOP_ENTITY_FIRE, ped.Handle);
                        }
                    }
                }
                

                // Oyuncuyu ayrıca kontrol et (oyuncu dahil edilmek istiyorsa)
                Ped player = Game.Player.Character;

                Function.Call(Hash.CLEAR_PED_BLOOD_DAMAGE, player.Handle);

                if (Function.Call<bool>(Hash.IS_ENTITY_ON_FIRE, player.Handle))
                {
                    Function.Call(Hash.STOP_ENTITY_FIRE, player.Handle);
                }
                
                ///////////////   
                foreach (var entry in vehiclesToDelete)
                {
                    Vehicle vehicle = entry.Key;
                    DateTime deleteTime = entry.Value;

                    // Süre dolmuşsa aracı sil
                    if (DateTime.Now > deleteTime)
                    {
                        if (vehicle.Exists())
                        {
                            vehicle.Delete();
                            GTA.UI.Notification.Show("Araç haritadan silindi.");
                        }
                        vehiclesToRemove.Add(vehicle);
                    }
                }

                // Silinmiş araçları sözlükten kaldır
                foreach (var vehicle in vehiclesToRemove)
                {
                    vehiclesToDelete.Remove(vehicle);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }
    }


    private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        if (e.KeyCode == System.Windows.Forms.Keys.F7)
        {
            enhancementsEnabled = !enhancementsEnabled; // Özellikleri aç/kapat

            if (enhancementsEnabled)
            {
                // Özellikleri etkinleştir
                Function.Call(Hash.SET_ENTITY_MAX_HEALTH, Game.Player.Character.Handle, maxHealth);
                Function.Call(Hash.SET_PED_MAX_HEALTH, Game.Player.Character.Handle, maxHealth);
                Game.Player.Character.Health = maxHealth;

                TeleportPlayerToCoordinates(-350, -620, 220); // Karakteri ışınla
                GTA.UI.Screen.ShowSubtitle("Hız, can, ilerleme ve lap takibi aktif!", 5000);
            }
            else
            {
                GTA.UI.Screen.ShowSubtitle("Hız, can, ilerleme ve lap takibi devre dışı!", 5000);
            }
        }
    }
   private void OnKeyDownHandler(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F11) // F12 tuşu kontrolü
        {
            string modelName = "turismor"; // Araç modeli
            Vector3 spawnPosition = new Vector3(-183, -681, 392); // Araç pozisyonu
            for (int i = 0; i < 50; i++)
            {
            SpawnVehicle(modelName, spawnPosition);
            }
        }
    }
   private void SpawnVehicle(string modelName, Vector3 position)
    {
        Model model = new Model(modelName);

        // Modeli yüklemeye çalış
        model.Request(1000);

        if (!model.IsValid || !model.IsInCdImage || !model.IsLoaded)
        {
            GTA.UI.Notification.Show("Araç modeli geçersiz veya yüklenemedi!");
            return;
        }

        // Araç oluştur
        Vehicle vehicle = World.CreateVehicle(model, position);

        if (vehicle != null && vehicle.Exists())
        {
            vehicle.PlaceOnGround(); // Aracı zemine yerleştir
            vehicle.IsPersistent = true; // Araç kalıcı olarak işaretlenir
            GTA.UI.Notification.Show("Araç başarıyla oluşturuldu: " + modelName);

            // 30 saniye sonra silinmek üzere ekle
            vehiclesToDelete[vehicle] = DateTime.Now.AddSeconds(30);
        }
        else
        {
            GTA.UI.Notification.Show("Araç oluşturulamadı!");
        }

        // Modeli serbest bırak
        model.MarkAsNoLongerNeeded();
    }
    private void IncreasePlayerSpeed()
    {
        Ped player = Game.Player.Character;

        if (player.IsOnFoot)
        {
            // Sprint ve hareket hızını artır
            Function.Call(Hash.SET_RUN_SPRINT_MULTIPLIER_FOR_PLAYER, Game.Player.Handle, speedMultiplier);
            Function.Call(Hash.SET_PED_MOVE_RATE_OVERRIDE, player.Handle, moveRateMultiplier);
        }
    }

    private void UpdateHealth()
    {
        Ped player = Game.Player.Character;

        // Maksimum canı ayarla
        Function.Call(Hash.SET_ENTITY_MAX_HEALTH, player.Handle, maxHealth);
        Function.Call(Hash.SET_PED_MAX_HEALTH, player.Handle, maxHealth);

        // Canı maksimum yap
        if (player.Health < maxHealth)
        {
            player.Health = maxHealth;
        }

        // HUD'ı doğru şekilde güncellemek için fiziksel değişim yap
        Function.Call(Hash.SET_ENTITY_HEALTH, player.Handle, maxHealth);

        // Can barını zorla dolu göstermek için küçük değişiklikler
        player.Health -= 1;
        player.Health += 1;
    }

    private void CheckAndTeleportIfInAir()
    {
        Ped player = Game.Player.Character;

        if (player.IsInAir && !player.IsInVehicle())
        {
            // Havada kalma süresini başlat
            if (airTimeStart == DateTime.MinValue)
            {
                airTimeStart = DateTime.Now;
            }

            // Havada 10 saniye geçtiyse ışınla
            if ((DateTime.Now - airTimeStart).TotalSeconds >= 10)
            {
                TeleportPlayerToCoordinates(-350, -620, 220); // Belirtilen koordinatlara ışınla
                airTimeStart = DateTime.MinValue; // Havada kalma süresini sıfırla
                GTA.UI.Screen.ShowSubtitle("Havada uzun kaldınız, ışınlandınız!", 5000);
            }
        }
        else
        {
            // Karakter yere indiğinde havada kalma süresini sıfırla
            airTimeStart = DateTime.MinValue;
        }
    }

    private void TeleportPlayerToCoordinates(float x, float y, float z)
    {
        Ped player = Game.Player.Character;

        if (player.IsInVehicle())
        {
            // Eğer karakter araçtaysa, aracı ışınla
            Vehicle vehicle = player.CurrentVehicle;
            vehicle.Position = new Vector3(x, y, z);
        }
        else
        {
            // Karakteri ışınla
            player.Position = new Vector3(x, y, z);
        }
    }

    private float CalculateProgress()
    {
        Ped player = Game.Player.Character;

        // Karakterin mevcut pozisyonu
        Vector3 currentPosition = player.Position;

        // A ile B arasındaki toplam mesafe
        float totalDistance = startPoint.DistanceTo(endPoint);

        // A ile mevcut pozisyon arasındaki mesafe
        float currentDistance = startPoint.DistanceTo(currentPosition);

        // İlerleme yüzdesi
        float progress = (currentDistance / totalDistance) * 100;

        // %100'ü aşmaması için sınırla
        return Math.Min(progress, 100.0f);
    }

    private void DisplayProgress(float progress)
    {
        // Yüzdeyi ekranda göster
        string progressText = string.Format("İlerleme: %{0:F1}", progress); // Örneğin, %45.7

        // Ekranın ortasına yakın bir yere yazdır
        GTA.UI.Screen.ShowHelpTextThisFrame(progressText, false);

        // Sağ üst köşede lap sayısını göster
        string lapText = string.Format("Win: {0}", lapCount);
        GTA.UI.Screen.ShowSubtitle(lapText, 500);
    }

    private void CheckLap(float progress)
    {
        if (progress >= 100.0f && !hasReached100)
        {
            lapCount++; // Lap sayısını artır
            hasReached100 = true; // Tekrar artırmaması için işaretle
        }
        else if (progress < 100.0f)
        {
            hasReached100 = false; // %100 altına düştüğünde tekrar artırmaya hazırla
        }
    }

    private void LogError(string message)
    {
        System.IO.File.AppendAllText(@"C:\GTA_Error_Logs.txt", DateTime.Now + ": " + message + Environment.NewLine);
    }
}
