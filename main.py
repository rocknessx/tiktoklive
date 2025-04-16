from cmath import e
import socket
import asyncio
import pygame
from TikTokLive.client import TikTokLiveClient



from TikTokLive.events import GiftEvent, LikeEvent, FollowEvent, CommentEvent

# TikTokLiveClient'ı başlatıyoruz
client = TikTokLiveClient(unique_id="mezarci_pubg")

# Beğeni sayaçları
like_count = 0  # Toplam beğeni sayısı
likes_since_last_100 = 0  # Son 100 için sayım
likes_since_last_500 = 0  # Son 500 için sayım
like_leaderboard = {}  # Beğeni lider tablosu
comment_counts = {"gs": 0, "fb": 0, "ts": 0, "bjk": 0}

pygame.mixer.init()

def play_sound(team):
    sound_files = {
        "gs": "C:\\Sounds\\gs.wav",
        "fb": "C:\\Sounds\\fb.wav",
        "ts": "C:\\Sounds\\ts.wav",
        "bjk": "C:\\Sounds\\bjk.wav"
    }
    file_path = sound_files.get(team)
    if file_path:
        try:
            pygame.mixer.music.stop()
            pygame.mixer.music.load(file_path)
            pygame.mixer.music.play()
            print(f"{team.upper()} müziği çalınıyor...")
        except Exception as e:
            print(f"Ses dosyası çalınamadı: {e}")

def play_mp3_async(file_path):
    try:
        
        
        pygame.mixer.music.load(file_path)
        pygame.mixer.music.play()
        print(f"{file_path} dosyası çalmaya başladı.")
    except Exception as e:
        print(f"MP3 çalma hatası: {e}")


# Chat yorumlarını artıran fonksiyon
def process_chat_comment(comment):
    comment = comment.lower()
    if comment in comment_counts:
        comment_counts[comment] += 1
        



# GTA'ya veri gönderecek TCP istemcisi
def send_command_to_gta(command, user="Unknown"):
    try:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as gta_socket:
            gta_socket.connect(("127.0.0.1", 12346))  # GTA'nın dinlediği port
            message = f"{command}:{user}"  # Komut ve kullanıcı adı birleştiriliyor
            gta_socket.sendall(message.encode("utf-8"))
            print(f"GTA'ya '{message}' komutu gönderildi.")
    except Exception as e:
        print(f"GTA'ya komut gönderilemedi: {e}")

# En fazla yorumu kontrol eden fonksiyon
async def check_and_play_sound():
    last_played_team = None
    while True:
        await asyncio.sleep(5)
        top_team = max(comment_counts, key=comment_counts.get)
        print(f"En fazla yorum alan takım: {top_team.upper()} - {comment_counts[top_team]}")

        if top_team != last_played_team and comment_counts[top_team] > 0:
            play_sound(top_team)
            last_played_team = top_team 

async def on_comment(event: CommentEvent) -> None:
    try:
        comment = event.comment.lower()
        print(f"Yeni Yorum: {comment}")

        if comment == "selam":
            send_command_to_gta("selam", event.user.unique_id)
            print("top gönderildi")
        if comment in comment_counts:
            comment_counts[comment] += 1

        for team in comment_counts.keys():
            if team in comment:
                comment_counts[team] += 1
                print(f"{team.upper()} sayacı: {comment_counts[team]}")

    except Exception as e:
        print(f"hata var: {e}")  

# Yeni takipçi algılama
async def on_follow(event: FollowEvent) -> None:
    try:
        print(f"Yeni Takipçi! Kullanıcı ID: {event.user.unique_id}")
        send_command_to_gta("takip", event.user.unique_id)
        play_mp3_async("C:\\music\\kapitik.mp3")
    except Exception as e:
        print(f"Takipçi işleme hatası: {e}")

# Hediye etkinliği algılama
async def on_gift(event: GiftEvent) -> None:
    try:
        user_id = "Unknown"
        if event.user:
            user_id = getattr(event.user, "uniqueId", getattr(event.user, "nickname", "Unknown"))

        if event.gift.name.lower() == "rose":
            send_command_to_gta("gül", user_id)
            print(f"Rose hediyesi alındı, 'gül' komutu gönderildi. Gönderen: {user_id}")
            play_mp3_async("C:\\music\\rizz.mp3")
        if event.gift.name.lower() == "train":
            send_command_to_gta("tren", user_id)
            print(f"Tren hediyesi alındı, 'Tren' komutu gönderildi. Gönderen: {user_id}")
            play_mp3_async("C:\\music\\ahmetofbas.mp3")
        if event.gift.name.lower() == "money gun":
            send_command_to_gta("money", user_id)
            print(f"Para tabancası hediyesi alındı, 'Money Gun' komutu gönderildi. Gönderen: {user_id}") 
            play_mp3_async("C:\\music\\eyvahtufan.mp3")   
        if event.gift.name.lower() == "corgi":
            send_command_to_gta("dozer", user_id)
            print(f"Corgi hediyesi alındı, 'dozer' komutu gönderildi. Gönderen: {user_id}")    
            play_mp3_async("C:\\music\\tezgahlanbu.mp3")   
        if event.gift.name.lower() == "sceptre":
            send_command_to_gta("Sceptre", user_id)
            print(f"Sceptre hediyesi alındı, 'Sceptre' komutu gönderildi. Gönderen: {user_id}")   
            play_mp3_async("C:\\music\\adamyaada.mp3")  
        if event.gift.name.lower() == "hand heart":
            send_command_to_gta("ellekalp", user_id)
            print(f"Elle kalp hediyesi alındı, 'ellekalp' komutu gönderildi. Gönderen: {user_id}")  
            play_mp3_async("C:\\music\\noluyolan.mp3")  
        if event.gift.name.lower() == "mishka bear":
            send_command_to_gta("mishka", user_id)
            print(f"Mishka Bear hediyesi alındı, 'mishka' komutu gönderildi. Gönderen: {user_id}")  
            play_mp3_async("C:\\music\\yeterbekardes.mp3") 
        if event.gift.name.lower() == "perfume":
            send_command_to_gta("perfume", user_id)
            print(f"Perfume  hediyesi alındı, 'Perfume' komutu gönderildi. Gönderen: {user_id}")
            play_mp3_async("C:\\music\\aglama.mp3")   
        if event.gift.name.lower() == "tea":
            send_command_to_gta("Tea", user_id)
            print(f"Tea  hediyesi alındı, 'Tea' komutu gönderildi. Gönderen: {user_id}")  
            play_mp3_async("C:\\music\\sentehlikesin.mp3")        
        if event.gift.name.lower() == "doughnut":
            send_command_to_gta("Donat", user_id)
            print(f"Donat  hediyesi alındı, 'Donat' komutu gönderildi. Gönderen: {user_id}") 
            play_mp3_async("C:\\music\\usmanımnere.mp3")   
        if event.gift.name.lower() == "crown":
            send_command_to_gta("crown", user_id)
            print(f"Crown  hediyesi alındı, 'Crown' komutu gönderildi. Gönderen: {user_id}")    
            play_mp3_async("C:\\music\\hurdac.mp3")       
        if event.gift.name.lower() == "paper crane":
            send_command_to_gta("traktor", user_id)
            print(f"Paper Crane  hediyesi alındı, 'traktor' komutu gönderildi. Gönderen: {user_id}")  
            play_mp3_async("C:\\music\\oglumbunelan.mp3") 
        if event.gift.name.lower() == "heart":
            send_command_to_gta("heart", user_id)
            print(f"Heart hediyesi alındı, 'heart' komutu gönderildi. Gönderen: {user_id}")  
            play_mp3_async("C:\\music\\tesekkur.mp3")    
        if event.gift.name.lower() == "blue bead":
            send_command_to_gta("nazar", user_id)
            print(f"Blue Bead hediyesi alındı, 'nazar' komutu gönderildi. Gönderen: {user_id}")
            play_mp3_async("C:\\music\\pingmissing.mp3")   
        if event.gift.name.lower() == "finger heart":
            send_command_to_gta("parmak", user_id)
            print(f"Finger Heart hediyesi alındı, 'parmak' komutu gönderildi. Gönderen: {user_id}")    
            play_mp3_async("C:\\music\\nedenbukadar.mp3") 
        if event.gift.name.lower() == "tiktok":
            send_command_to_gta("tiktok", user_id)
            print(f"Tiktok hediyesi alındı, 'tiktok' komutu gönderildi. Gönderen: {user_id}") 
            play_mp3_async("C:\\music\\orhan.mp3")           

    except Exception as e:
        print(f"Hediye işleme hatası: {e}")

# Beğeni etkinliği algılama
async def on_like(event: LikeEvent):
    global like_count, likes_since_last_100, likes_since_last_500, like_leaderboard

    try:
        user_id = "Unknown"
        if event.user:
            user_id = getattr(event.user, "uniqueId", getattr(event.user, "nickname", "Unknown"))

        if hasattr(event, 'count'):
            like_count += event.count
            likes_since_last_100 += event.count
            likes_since_last_500 += event.count

            # Beğeni lider tablosu güncelleme
            if user_id in like_leaderboard:
                like_leaderboard[user_id] += event.count
            else:
                like_leaderboard[user_id] = event.count

            print(f"{user_id} toplam beğenisi: {like_leaderboard[user_id]}")

            # 500 beğeni kontrolü
            if likes_since_last_500 >= 10000:
                send_command_to_gta("sporaraba", user_id)
                print(f"10000 beğeniye ulaşıldı. 'sporaraba' komutu gönderildi. Gönderen: {user_id}")
                play_mp3_async("C:\\music\\putinwide.mp3")
                likes_since_last_500 -= 10000  # 500'ü sıfırla

            # 100 beğeni kontrolü
            if likes_since_last_100 >= 100:
                send_command_to_gta("akuma", user_id)
                play_mp3_async("C:\\music\\eyvallahkanka.mp3")
                print(f"100 beğeniye ulaşıldı. 'akuma' komutu gönderildi. Gönderen: {user_id}")
                likes_since_last_100 -= 100  # 100'ü sıfırla

    except Exception as e:
        print(f"Beğeni işleme hatası: {e}")

# En çok beğeni atanları düzenli olarak gönder
async def send_top_likers():
    global like_leaderboard
    while True:
        await asyncio.sleep(5)  # Her 5 saniyede bir çalışır

        try:
            # İlk 3 kullanıcıyı bul
            sorted_likes = sorted(like_leaderboard.items(), key=lambda x: x[1], reverse=True)[:10]
            for rank, (user_id, likes) in enumerate(sorted_likes):
                send_command_to_gta(f"top{rank+1}", user_id)
                print(f"En çok beğeni atan {rank+1}.: {user_id} - {likes} beğeni")

            if not sorted_likes:
                print("Beğeni atan kimse yok.")
        except Exception as e:
            print(f"Lider tablosu işleme hatası: {e}")

async def send_comment_stats():
    while True:
        await asyncio.sleep(5)  # Her 5 saniyede bir çalışır
        try:
            for key, count in comment_counts.items():
                if count > 0:
                    send_command_to_gta(f"{key}", str(count))
                    print(f"GTA'ya {key} için {count} yorumu gönderildi.")
                    comment_counts[key] = 0  # Sayaçları sıfırla
        except Exception as e:
            print(f"Yorum istatistiklerini gönderirken hata oluştu: {e}")


# Tüm işlemleri başlat
async def start():
    try:
        # TikTok client'ı bağla ve lider tablosunu başlat
        await asyncio.gather(client.connect(), send_top_likers(),send_comment_stats(),check_and_play_sound())
    except Exception as e:
        print(f"Bir hata oluştu: {e}")

if __name__ == "__main__":
    # TikTok etkinliklerini dinle
    client.add_listener(GiftEvent, on_gift)
    client.add_listener(LikeEvent, on_like)
    client.add_listener(FollowEvent, on_follow)
    client.add_listener(CommentEvent, on_comment)
    # Başlat
    asyncio.run(start())
