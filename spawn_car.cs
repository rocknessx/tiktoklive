using System;
using System.Net;
using System.Net.Sockets;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Media;

public class Server : Script
{
    private const int port = 12346;
    private TcpListener _tcpListener;

    private Dictionary<Ped, string> enemyUserIds = new Dictionary<Ped, string>();
    private Dictionary<Ped, DateTime> enemyDeletionTimers = new Dictionary<Ped, DateTime>();
    private Dictionary<Ped, string> followUserIds = new Dictionary<Ped, string>();
    private Dictionary<Ped, DateTime> followDeletionTimers = new Dictionary<Ped, DateTime>();
    private Dictionary<Vehicle, string> vehicleUserIds = new Dictionary<Vehicle, string>();
    private Dictionary<Vehicle, DateTime> vehicleDeletionTimers = new Dictionary<Vehicle, DateTime>();
    private Dictionary<Prop, string> propUserIds = new Dictionary<Prop, string>();
    private Dictionary<Prop, DateTime> propDeletionTimers = new Dictionary<Prop, DateTime>();

    private Dictionary<string, string> topUsers = new Dictionary<string, string>();


    private Ped top1Ped, top2Ped, top3Ped;
    private string top1User = "Top1";
    private string top2User = "Top2";
    private string top3User = "Top3";
    private bool topNPCsSpawned = false;

    private static Random randomGenerator = new Random();
    
    public Server()
    {
        try
        {
            _tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            _tcpListener.Start();
            Tick += OnTick;
            KeyDown += OnKeyDown;
            KeyDown += OnKeyDownc;  
            
        }
        catch (Exception ex)
        {
            LogError(ex.Message);
        }
    }
     

     

    private void OnTick(object sender, EventArgs e)
    {
        try
        {
            if (_tcpListener.Pending())
            {
                var client = _tcpListener.AcceptTcpClient();
                var stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string command = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                ProcessCommand(command);
                client.Close();
            }

            CleanupEnemyNPCs();
            CleanupFollowers();
            CleanupVehicles();
            CleanupProps();

            UpdateDisplays();
           
        }
        catch (Exception ex)
        {
            LogError(ex.Message);
        }
    }

    private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        if (e.KeyCode == System.Windows.Forms.Keys.F2)
        {
            if (!topNPCsSpawned)
            {
                SpawnTopNPCs();
                topNPCsSpawned = true;
            }
            else
            {
                DeleteTopNPCs();
                topNPCsSpawned = false;
            }
        }
    }
    private Dictionary<string, int> commentCounts = new Dictionary<string, int> {
    { "gs", 0 },
    { "fb", 0 },
    { "ts", 0 },
    { "bjk", 0 }
};

    private void ProcessCommand(string command)
    {
       

        if (string.IsNullOrEmpty(command)) return;
        
        string[] commandParts = command.Split(':');
        if (commandParts.Length < 1) return;
        string action = commandParts[0];
        string userId = commandParts.Length > 1 ? commandParts[1] : "Unknown";
        string value = commandParts.Length > 1 ? commandParts[1] : "0"; // Değer varsa al, yoksa "0"

    if (commandParts.Length == 2)
        {
            string team = commandParts[0].ToLower(); // İlk kısmı takım adı olarak al
            int parsedValue; // Değeri burada tanımla

            // İkinci kısmı tam sayıya çevir
            if (int.TryParse(commandParts[1], out parsedValue)) 
            {
                // Takım adı geçerliyse veriyi güncelle
                if (commentCounts.ContainsKey(team))
                {
                    commentCounts[team] = parsedValue; // Değeri güncelle
                    GTA.UI.Notification.Show(string.Format("{0} skoru güncellendi: {1}", team.ToUpper(), parsedValue)); // Bildirimi göster
                    UpdateFranklinClothing(); // Franklin'in kıyafetini güncelle
                }
            }
        }


        // Eğer action, top kullanıcılarından biriyse, topUsers'a ekle
        else if (action.StartsWith("top"))
        {
            topUsers[action] = value;
        }

        switch (action)
        {
            case "gül":
                CreateEnemy(userId);
                break;
            case "takip":
                CreateFollower(userId);
                break;
            case "akuma":  
                SpawnVehicle("akuma", userId);
                //spawnProp("prop_log_03", new Vector3(-180, -681, 600), userId);
                break;
             case "sporaraba":
                for (int i = 0; i < 100; i++)
                {
                    SpawnVehicle("turismor", userId);
                }
                break;
            case "tren":
                for (int i = 0; i < 1500; i++)
                {
                    SpawnVehicle("hydra", userId);
                }
                break;
             case "Perfume":
                for (int i = 0; i < 30; i++)
                {
                    SpawnVehicle("toro", userId);
                }
                break;    

            case "dozer":
                for (int i = 0; i < 500; i++)
                {
                    SpawnVehicle("bulldozer", userId);
                }
                break; 
            case "money":
                for (int i = 0; i < 750; i++)
                {
                    SpawnVehicle("buzzard", userId);
                }
                break; 
            case "Sceptre":
                for (int i = 0; i < 300; i++)
                {
                    SpawnVehicle("insurgent", userId);
                }
                break;
            case "ellekalp":
                for (int i = 0; i < 250; i++)
                {
                    SpawnVehicle("tourbus", userId);
                }
                break;                        
            case "selam":
                //SpawnVehicle("akuma", userId);
                SpawnProp("prop_beach_volball01", new Vector3(-180, -681, 600), userId);
                break;    
            case "mishka":
             for (int i = 0; i < 150; i++)
                {
                SpawnVehicle("akuma", userId);
                }
                break;
            case "Tea":
             for (int i = 0; i < 100; i++)
                {
                SpawnVehicle("hellion", userId);
                }
                break;    
            case "donat":
             for (int i = 0; i < 30; i++)
                {
                SpawnVehicle("apc", userId);
                }
                break;   
            case "crown":
                SpawnMeteor(Game.Player.Character.Position);         
                break;   
            case "traktor":
             for (int i = 0; i < 150; i++)
                {
                SpawnVehicle("tractor3", userId);
                }
                break;  
            case "heart":
             for (int i = 0; i < 15; i++)
                {
                SpawnVehicle("firetruk", userId);
                }
                break;   
            case "nazar":
             for (int i = 0; i < 7; i++)
                {
                SpawnVehicle("scrap", userId);
                }
                break;  
            case "parmak":
             for (int i = 0; i < 7; i++)
                {
                SpawnVehicle("rentalbus", userId);
                }
                break; 
            case "tiktok":
             for (int i = 0; i < 3; i++)
                {
                SpawnVehicle("turismor", userId);
                }
                break;                     
            case "top1":
                top1User = userId;
                topUsers[action] = userId;
                break;
            case "top2":
                top2User = userId;
                topUsers[action] = userId;
                break;
            case "top3":
                top3User = userId;
                topUsers[action] = userId;
                break;
            case "top4":
                top3User = userId;
                topUsers[action] = userId;
                break;    
            case "top5":
                top3User = userId;
                topUsers[action] = userId;
                break;  
            case "top6":
                top3User = userId;
                topUsers[action] = userId;
                break;  
            case "top7":
                top3User = userId;
                topUsers[action] = userId;
                break;    
            case "top8":
                top3User = userId;
                topUsers[action] = userId;
                break;    
            case "top9":
                top3User = userId;
                topUsers[action] = userId;
                break;   
            case "top10":
                top3User = userId;
                topUsers[action] = userId;
                break;         
        }
    }

    private void UpdateFranklinClothing()
    {
        // En yüksek skorlu takımı bul
        string topTeam = null;
        int maxScore = -1;

        foreach (var team in commentCounts)
        {
            if (team.Value > maxScore)
            {
                topTeam = team.Key;
                maxScore = team.Value;
            }
        }

        // Franklin'in forması değişecek
        if (!string.IsNullOrEmpty(topTeam))
        {
            ChangePlayerPedBasedOnTeam(topTeam);
        }
    }

    private void CreateEnemy(string userId)
    {
        Ped enemy = World.CreatePed(PedHash.MilitaryBum, Game.Player.Character.Position);
        if (enemy.Exists())
        {
            enemyUserIds[enemy] = userId;
            enemyDeletionTimers[enemy] = DateTime.Now.AddSeconds(3);
            enemy.Weapons.RemoveAll();
            enemy.Task.Combat(Game.Player.Character);
            GTA.UI.Notification.Show(userId + " Düşman Gönderdi");
        }
    }
    private void SpawnMeteor(Vector3 playerPosition)
    {
        // Meteorun başlangıç pozisyonu (oyuncunun üstünde, yukarıdan düşecek)
        Vector3 meteorStartPosition = new Vector3(playerPosition.X, playerPosition.Y, playerPosition.Z + 500);

        // Meteor düşüş pozisyonu
        Vector3 meteorTargetPosition = new Vector3(playerPosition.X, playerPosition.Y, playerPosition.Z);

        // Meteor modeli
        Model meteorModel = new Model("prop_rock_4_big2"); // Alternatif olarak taş modeli
        if (meteorModel.IsValid && meteorModel.Request(1000))
        {
            // Meteor modeli oluştur
            Prop meteorProp = World.CreateProp(meteorModel, meteorStartPosition, true, true);

            // Meteorun düşmesini sağla (aşağı yönlü kuvvet uygula)
            meteorProp.ApplyForce(new Vector3(0, 0, -100));

            // 1 saniye bekledikten sonra patlama oluştur
            Wait(1000); // Düşüş için süre
            CreateExplosion(meteorTargetPosition);
        }
        else
        {
            GTA.UI.Notification.Show("Meteor modeli yüklenemedi!");
        }
    }

    private void CreateExplosion(Vector3 position)
    {
        // Patlama oluştur ve hasar ver
        World.AddExplosion(position, ExplosionType.Rocket, 10.0f, 1.0f, null, true, false);

        // Oyuncuya yakınsa hasar uygula
        if (Game.Player.Character.Position.DistanceTo(position) < 10.0f) // 10 birim yakınlık kontrolü
        {
            Game.Player.Character.Health -= 50; // Oyuncunun sağlığını düşür
            GTA.UI.Notification.Show("Meteorun etkisiyle hasar aldınız!");
        }
    }

    private void CreateFollower(string userId)
    {
        PedHash[] validPeds = Enum.GetValues(typeof(PedHash))
            .Cast<PedHash>()
            .Where(ped => !IsAnimalPed(ped))
            .ToArray();

        if (validPeds.Length == 0)
        {
            LogError("No valid human ped hashes found.");
            return;
        }

        PedHash randomPed = validPeds[randomGenerator.Next(validPeds.Length)];
        Ped follower = World.CreatePed(randomPed,Game.Player.Character.Position);

        if (follower.Exists())
        {
            followUserIds[follower] = userId;
            followDeletionTimers[follower] = DateTime.Now.AddSeconds(60);
            GTA.UI.Notification.Show(userId +" Takip Etti Sagoll");
        }
    }

    private bool IsAnimalPed(PedHash ped)
    {
        PedHash[] animalPeds = {
            PedHash.Boar, PedHash.Chimp, PedHash.Coyote, PedHash.Cow, PedHash.Deer,
            PedHash.Fish, PedHash.Hen, PedHash.Cat, PedHash.MountainLion, PedHash.Pig,
            PedHash.Rat, PedHash.Dolphin, PedHash.KillerWhale, PedHash.Seagull,
            PedHash.Husky, PedHash.Retriever, PedHash.Poodle, PedHash.Pug,
            PedHash.Rottweiler, PedHash.Shepherd, PedHash.Westy
        };
        return animalPeds.Contains(ped);
    }

 
    private void SpawnVehicle(string modelName, string userId)
    {
        Vehicle vehicle = World.CreateVehicle(new Model(modelName), new Vector3(-180, -681, 600));
        if (vehicle.Exists())
        {
            vehicleUserIds[vehicle] = userId;
            vehicleDeletionTimers[vehicle] = DateTime.Now.AddSeconds(30);
            GTA.UI.Notification.Show("Vehicle '" + modelName + "' spawned for user: " + userId);
        }
    }
private void SpawnProp(string modelName, Vector3 position, string userId)
{
    // Modeli yükle
    Model propModel = new Model(modelName);
    if (!propModel.IsLoaded)
    {
        propModel.Request();
        while (!propModel.IsLoaded) Script.Wait(10); // Model yüklenmesini bekle
    }

    
    // Zemin yüksekliğini al ve pozisyonu güncelle
    float groundHeight = GetGroundHeight(position);
    position.Z = groundHeight + 1.0f; // Zeminin biraz üzerinde başlat

    // Prop'u oluşturma
    Prop prop = World.CreateProp(propModel, position, false, false); // Dinamik olmadan spawnla
    if (prop.Exists())
    {
        propUserIds[prop] = userId;
        propDeletionTimers[prop] = DateTime.Now.AddSeconds(30);
        // Pozisyonu yeniden ayarla
        Function.Call(Hash.SET_ENTITY_COORDS_NO_OFFSET, prop.Handle, position.X, position.Y, position.Z + 30.0f, false, false, false);


        // Çarpışmayı ve fizik motorunu etkinleştir
        Function.Call(Hash.SET_ENTITY_COLLISION, prop.Handle, true, true); // Çarpışmayı etkinleştir
        Function.Call(Hash.ACTIVATE_PHYSICS, prop.Handle);                // Fizik motorunu etkinleştir
        Function.Call(Hash.SET_ENTITY_DYNAMIC, prop.Handle, true);        // Prop'u dinamik yap
          
        // Pozisyonu serbest bırak
        Function.Call(Hash.FREEZE_ENTITY_POSITION, prop.Handle, false);
        AddCollisionBox(position);
        // Kullanıcıya bildirim göster
        GTA.UI.Notification.Show(" '" + modelName + "' gönderdi " + userId);
    }
    else
    {
        GTA.UI.Notification.Show("Failed to spawn prop '" + modelName + "' for user: " + userId);
    }
}

private float GetGroundHeight(Vector3 position)
{
    OutputArgument groundZ = new OutputArgument(); // Zemin yüksekliği için OutputArgument

    // Native fonksiyon çağrısı
    if (Function.Call<bool>(
        Hash.GET_GROUND_Z_FOR_3D_COORD,
        position.X,        // X koordinatı
        position.Y,        // Y koordinatı
        position.Z + 1,  // Başlangıç Z yüksekliği
        groundZ,           // Zemin yüksekliği için OutputArgument
        false              // Suyu dikkate alma (ignoreWater)
    ))
    {
        return groundZ.GetResult<float>(); // OutputArgument sonucunu al
    }

    return position.Z; // Eğer zemin yüksekliği bulunamazsa mevcut Z'yi döndür
}




private void AddCollisionBox(Vector3 position)
{
    // Basit bir çarpışma kutusu modeli
    Model collisionModel = new Model("prop_log_03");
    if (!collisionModel.IsLoaded)
    {
        collisionModel.Request();
        while (!collisionModel.IsLoaded) Script.Wait(10);
    }

    // Çarpışma kutusunu oluştur
    Prop collisionBox = World.CreateProp(collisionModel, position, false, true);
    if (collisionBox.Exists())
    {
        collisionBox.IsVisible = false; // Kutuyu görünmez yap
        Function.Call(Hash.SET_ENTITY_COLLISION, collisionBox.Handle, true, true); // Çarpışmayı etkinleştir
        Function.Call(Hash.FREEZE_ENTITY_POSITION, collisionBox.Handle, false);  
    }
}



    private void SpawnTopNPCs()
    {
        Vector3 playerPosition = Game.Player.Character.Position;

        top1Ped = World.CreatePed(PedHash.Michael, playerPosition + new Vector3(5, -2, 5));
        FreezeAndMakePedLookAtPlayer(top1Ped);

        top2Ped = World.CreatePed(PedHash.Trevor, playerPosition + new Vector3(5, 0, 5));
        FreezeAndMakePedLookAtPlayer(top2Ped);

        top3Ped = World.CreatePed(PedHash.Franklin, playerPosition + new Vector3(5, 2, 5));
        FreezeAndMakePedLookAtPlayer(top3Ped);
    }

    private void DeleteTopNPCs()
    {
        if (top1Ped != null && top1Ped.Exists()) top1Ped.Delete();
        if (top2Ped != null && top2Ped.Exists()) top2Ped.Delete();
        if (top3Ped != null && top3Ped.Exists()) top3Ped.Delete();

        top1Ped = null;
        top2Ped = null;
        top3Ped = null;
    }

    private void FreezeAndMakePedLookAtPlayer(Ped ped)
    {
        if (ped.Exists())
        {
            ped.IsInvincible = true;
            Function.Call(Hash.TASK_STAND_STILL, ped.Handle, -1);
            ped.Task.LookAt(Game.Player.Character);
        }
    }

    private void CleanupEnemyNPCs()
    {
        foreach (var enemy in enemyDeletionTimers.Where(e => !e.Key.IsAlive && DateTime.Now > e.Value).ToList())
        {
            if (enemy.Key.Exists()) enemy.Key.Delete();
            enemyUserIds.Remove(enemy.Key);
            enemyDeletionTimers.Remove(enemy.Key);
        }
    }

    private void CleanupFollowers()
    {
        foreach (var follower in followDeletionTimers.Where(f => DateTime.Now > f.Value).ToList())
        {
            if (follower.Key.Exists()) follower.Key.Delete();
            followUserIds.Remove(follower.Key);
            followDeletionTimers.Remove(follower.Key);
        }
    }

    private void CleanupVehicles()
    {
        foreach (var vehicle in vehicleDeletionTimers.Where(v => DateTime.Now > v.Value).ToList())
        {
            if (vehicle.Key.Exists()) vehicle.Key.Delete();
            vehicleUserIds.Remove(vehicle.Key);
            vehicleDeletionTimers.Remove(vehicle.Key);
        }
    }

    private void CleanupProps()
    {
        foreach (var prop in propDeletionTimers.Where(p => DateTime.Now > p.Value).ToList())
        {
            if (prop.Key.Exists()) prop.Key.Delete();
            propUserIds.Remove(prop.Key);
            propDeletionTimers.Remove(prop.Key);
        }
    }

  private void ChangePlayerPedBasedOnTeam(string team)
{
    if (Game.Player.Character.Model != PedHash.Franklin)
    {
        GTA.UI.Notification.Show("Franklin karakterinde olmadığınızdan işlem yapılmadı!");
        return;
    }

    int componentId = 3; // Torso bileşeni
    int drawableId = 4;  // Sabit Drawable ID (modlu kıyafet ID'si)
    int textureId = 0;   // Takıma göre değişen Texture ID
    string soundFilePath = "";

    // Önce bileşeni sıfırla (Drawable = 0, Texture = 0)
    Function.Call(Hash.SET_PED_COMPONENT_VARIATION,
        Game.Player.Character.Handle, // Oyuncu karakteri
        componentId,                  // Torso bileşeni (3)
        0,                            // Sıfır modeli (default)
        0,                            // Sıfır doku
        0                             // Palet ID
    );

    // Takıma göre Texture ID'yi ayarla
    switch (team)
    {
        case "fb":
            textureId = 0; // Fenerbahçe
            // soundFilePath = @"C:\Sounds\fb.wav"; // FB için dosya yolu
            break;
        case "bjk":
            textureId = 1; // Beşiktaş
            // soundFilePath = @"C:\Sounds\bjk.wav"; // BJK için dosya yolu
            break;
        case "ts":
            textureId = 2; // Trabzonspor
            // soundFilePath = @"C:\Sounds\ts.wav"; // TS için dosya yolu
            break;
        case "gs":
            textureId = 15; // Galatasaray
            // soundFilePath = @"C:\Sounds\gs.wav"; // GS için dosya yolu
            break;
        default:
            GTA.UI.Notification.Show("Geçersiz takım kodu!");
            return;
    }

    // Yeni kıyafeti uygula
    Function.Call(Hash.SET_PED_COMPONENT_VARIATION,
        Game.Player.Character.Handle, // Oyuncu karakteri
        componentId,                  // Torso bileşeni (3)
        drawableId,                   // Yeni model ID (4)
        textureId,                    // Takıma özel Texture ID
        0                             // Palette ID
    );

    try
    {
        using (SoundPlayer player = new SoundPlayer(soundFilePath))
        {
            player.Play(); // Asenkron olarak çalar
        }
    }
    catch (Exception ex)
    {
        GTA.UI.Notification.Show("Ses çalınamadı: " + ex.Message);
    }
    
}



    private void LogPedClothing(int component, int drawableId, int textureId)
    {
        string logMessage = "Slot " + component + ": Drawable " + drawableId + ", Texture " + textureId;
        System.IO.File.AppendAllText(@"C:\GTA_Clothing_Debug.txt", logMessage + Environment.NewLine);
    }

    private void DebugPedClothing(Ped ped)
    {
        if (ped.Exists())
        {
            for (int component = 0; component < 12; component++) // 0-11 arası tüm bileşenler
            {
                int drawableId = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, ped.Handle, component);
                int textureId = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, ped.Handle, component);

                // Bildirim olarak göster
                GTA.UI.Notification.Show("Slot " + component + ": Drawable " + drawableId + ", Texture " + textureId);

                // Log dosyasına yaz
                LogPedClothing(component, drawableId, textureId);
            }
        }
    }
    private void OnKeyDownc(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F1) // F3 tuşu kontrolü
        {
            DebugPedClothing(Game.Player.Character);
        }
    }

    private void PlaySound(string filePath)
    {
        try
        {
            if (System.IO.File.Exists(filePath))
            {
                using (SoundPlayer player = new SoundPlayer(filePath))
                {
                    player.Play();
                }
            }
            else
            {
                LogError("MP3 dosyası bulunamadı: " + filePath);
            }
        }
        catch (Exception ex)
        {
            LogError("Ses çalma hatası: " + ex.Message);
        }
    }


    private void ClearPedUpperClothing(Ped ped)
    {
        if (ped.Exists())
        {
            // Tüm kıyafetleri sıfırla
            for (int component = 0; component < 12; component++) // 0-11 bileşen slotları
            {
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, ped.Handle, component, 0, 0, 0);
            }

            // Tüm aksesuarları sıfırla
            for (int propIndex = 0; propIndex < 10; propIndex++) // 0-9 aksesuar slotları
            {
                Function.Call(Hash.CLEAR_PED_PROP, ped.Handle, propIndex);
            }
        }
    }
    private void CheckAndFixPedClothing(Ped ped, int component, int drawableId, int textureId)
    {
        // Kıyafetin doğru şekilde uygulanıp uygulanmadığını kontrol et
        int currentDrawable = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, ped.Handle, component);
        int currentTexture = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, ped.Handle, component);

        // Eğer kıyafet doğru uygulanmadıysa tekrar uygula
        if (currentDrawable != drawableId || currentTexture != textureId)
        {
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, ped.Handle, component, drawableId, textureId, 0);
        }
    }


    private void UpdateDisplays()
    {
        foreach (var enemy in enemyUserIds)
        {
            if (enemy.Key.Exists())
            {
                DrawTextAbovePed(enemy.Key, enemy.Value);
                DrawHealthBarAbovePed(enemy.Key);
            }
        }

        string commentDisplay = string.Join(" ", commentCounts.Select(kv => kv.Key + ":" + kv.Value));
        string topTeam = commentCounts.OrderByDescending(kv => kv.Value).First().Key;
        DrawTextOnScreen(commentDisplay, new Vector2(0.5f, 0.01f));
        

        foreach (var follower in followUserIds)
        {
            if (follower.Key.Exists())
            {
                DrawTextAbovePed(follower.Key, follower.Value);
            }
        }

        foreach (var vehicle in vehicleUserIds)
        {
            if (vehicle.Key.Exists())
            {
                DrawTextAboveVehicle(vehicle.Key, vehicle.Value);
            }
        }

        foreach (var prop in propUserIds)
        {
            if (prop.Key.Exists())
            {
                DrawTextAboveProp(prop.Key, prop.Value);
            }
        }

        if (top1Ped != null && top1Ped.Exists()) DrawTextAbovePed(top1Ped, "Top1: " + top1User);
        if (top2Ped != null && top2Ped.Exists()) DrawTextAbovePed(top2Ped, "Top2: " + top2User);
        if (top3Ped != null && top3Ped.Exists()) DrawTextAbovePed(top3Ped, "Top3: " + top3User);

        int offsetY = 0;
        foreach (var topUser in topUsers.OrderBy(kv => kv.Key))
        {
            DrawTextOnScreen(topUser.Key + ": " + topUser.Value, new Vector2(0.85f, 0.05f + offsetY * 0.03f));
            offsetY++;
        }
    }

    private void DrawTextOnScreen(string text, Vector2 position)
    {
        Function.Call(Hash.SET_TEXT_FONT, 0);
        Function.Call(Hash.SET_TEXT_SCALE, 0.35f, 0.35f);
        Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 255);
        Function.Call(Hash.SET_TEXT_OUTLINE);
        Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING");
        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, text);
        Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, position.X, position.Y);
    }

    private void DrawTextAbovePed(Ped ped, string text)
    {
        if (ped.Exists() && ped.IsAlive)
        {
            Vector3 position = ped.Position + new Vector3(0, 0, 1.0f);
            Vector2 screenCoords = WorldToScreen(position);

            if (!IsOnScreen(screenCoords)) return;

            Function.Call(Hash.SET_TEXT_FONT, 0);
            Function.Call(Hash.SET_TEXT_SCALE, 0.35f, 0.35f);
            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 255);
            Function.Call(Hash.SET_TEXT_OUTLINE);
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, text);
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, screenCoords.X, screenCoords.Y);
        }
    }

    private void DrawTextAboveVehicle(Vehicle vehicle, string text)
    {
        if (vehicle.Exists())
        {
            Vector3 position = vehicle.Position + new Vector3(0, 0, 1.5f);
            Vector2 screenCoords = WorldToScreen(position);

            if (!IsOnScreen(screenCoords)) return;

            Function.Call(Hash.SET_TEXT_FONT, 0);
            Function.Call(Hash.SET_TEXT_SCALE, 0.35f, 0.35f);
            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 255);
            Function.Call(Hash.SET_TEXT_OUTLINE);
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, text);
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, screenCoords.X, screenCoords.Y);
        }
    }

    private void DrawTextAboveProp(Prop prop, string text)
    {
        if (prop.Exists())
        {
            Vector3 position = prop.Position + new Vector3(0, 0, 1.0f);
            Vector2 screenCoords = WorldToScreen(position);

            if (!IsOnScreen(screenCoords)) return;

            Function.Call(Hash.SET_TEXT_FONT, 0);
            Function.Call(Hash.SET_TEXT_SCALE, 0.35f, 0.35f);
            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 255);
            Function.Call(Hash.SET_TEXT_OUTLINE);
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, text);
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, screenCoords.X, screenCoords.Y);
        }
    }

    private void DrawHealthBarAbovePed(Ped ped)
    {
        if (ped.Exists())
        {
            Vector3 pedPosition = ped.Position + new Vector3(0, 0, 1.0f);
            Vector2 screenCoords = WorldToScreen(pedPosition);

            if (!IsOnScreen(screenCoords)) return;

            float healthPercentage = ped.Health / (float)ped.MaxHealth;
            float barWidth = 0.03f;
            float barHeight = 0.005f;
            float barX = screenCoords.X - barWidth / 2;
            float barY = screenCoords.Y - 0.02f;

            Function.Call(Hash.DRAW_RECT, barX, barY, barWidth, barHeight, 0, 0, 0, 150);
            Function.Call(Hash.DRAW_RECT, barX, barY, barWidth * healthPercentage, barHeight, 255, 0, 0, 200);
        }
    }

    private Vector2 WorldToScreen(Vector3 worldPos)
    {
        float screenX = 0f, screenY = 0f;
        OutputArgument outX = new OutputArgument(), outY = new OutputArgument();

        bool isOnScreen = Function.Call<bool>(Hash.GET_SCREEN_COORD_FROM_WORLD_COORD, worldPos.X, worldPos.Y, worldPos.Z, outX, outY);

        if (isOnScreen)
        {
            screenX = outX.GetResult<float>();
            screenY = outY.GetResult<float>();
        }

        return new Vector2(screenX, screenY);
    }

    private bool IsOnScreen(Vector2 screenCoords)
    {
        return screenCoords.X >= 0 && screenCoords.X <= 1 && screenCoords.Y >= 0 && screenCoords.Y <= 1;
    }

    private void LogError(string message)
    {
        System.IO.File.AppendAllText(@"C:\GTA_Error_Logs.txt", DateTime.Now + ": " + message + Environment.NewLine);
    }
}
