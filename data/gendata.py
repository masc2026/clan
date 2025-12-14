# Dieser Code wurde mit Hilfe von Gemini 3 Pro erstellt.
# Generierungsdatum: 2025-12-14
#

import random
from datetime import date, datetime, timedelta, time
import os

# --- KONFIGURATION ---
OUTPUT_DIR = "data"
FILES_TO_GENERATE = 3
TARGET_ROWS_PER_FILE = 50  
START_DATE = date(2018, 1, 6)
END_DATE = date(2025, 12, 11)

# Verhalten
SLEEP_START_HOUR = 23
SLEEP_END_HOUR = 7

# --- HELPER ---

def ensure_dir(directory):
    if not os.path.exists(directory):
        os.makedirs(directory)

def random_prefix():
    return random.choice(["+49", "0", "0049"])

def generate_raw_number():
    prefix = random.choice(["151", "160", "170", "171", "162", "172", "157", "177"])
    rest = "".join([str(random.randint(0, 9)) for _ in range(random.choice([7, 8]))])
    return f"{prefix}{rest}"

def format_number(raw_number):
    return f"{random_prefix()}{raw_number}"

def format_log_entry(dt, caller_num, receiver_num, direction, duration_seconds):
    """Erstellt den CSV-String für eine Zeile"""
    duration_str = f"{duration_seconds // 60:02}:{duration_seconds % 60:02}"
    
    # Wochentag auf Deutsch
    weekday = ['Mo.', 'Di.', 'Mi.', 'Do.', 'Fr.', 'Sa.', 'So.'][dt.weekday()]
    date_str = f"{weekday} {dt.strftime('%d.%m.%Y')}"
    time_str = dt.strftime("%H:%M:%S")
    
    return f"{date_str};{time_str};{caller_num};{receiver_num};SPRACHE;{direction};{duration_str}"

# --- WELT SIMULATION ---

class Suspect:
    def __init__(self, id):
        self.id = id
        self.raw_number = generate_raw_number()
        self.partners = [] 
        self.external_numbers = [] 
        # Wir speichern Tupel: (timestamp, csv_line_string) für korrektes Sortieren später
        self.logs = [] 

def create_network(num_suspects):
    suspects = [Suspect(i) for i in range(num_suspects)]
    
    # 1. Ring-Verbindung
    for i in range(num_suspects):
        prev_s = suspects[i-1]
        next_s = suspects[(i+1) % num_suspects]
        suspects[i].partners.append(prev_s)
        suspects[i].partners.append(next_s)
    
    # 2. Zufällige Querverbindungen
    for s in suspects:
        others = [x for x in suspects if x != s and x not in s.partners]
        if others:
            s.partners.extend(random.sample(others, k=min(len(others), random.randint(2, 4))))
            
        # 3. Externe Kontakte
        for _ in range(20):
            s.external_numbers.append(generate_raw_number())
            
    return suspects

def generate_global_calls(suspects):
    total_calls_needed = int(TARGET_ROWS_PER_FILE * len(suspects) * 0.7)
    print(f"Simuliere ca. {total_calls_needed} Anrufe weltweit mit Jitter...")
    
    delta_total = (END_DATE - START_DATE).days * 24 * 3600
    start_ts = datetime.combine(START_DATE, time(0,0)).timestamp()
    
    # Zeitstempel sortiert generieren
    timestamps = sorted([start_ts + random.randint(0, delta_total) for _ in range(total_calls_needed)])
    
    generated_count = 0
    
    for ts in timestamps:
        dt_sender = datetime.fromtimestamp(ts)
        
        # Nachtruhe
        if dt_sender.hour >= SLEEP_START_HOUR or dt_sender.hour < SLEEP_END_HOUR:
            continue
            
        caller = random.choice(suspects)
        
        # Intern vs Extern
        is_internal = random.random() < 0.8
        
        if is_internal and caller.partners:
            receiver = random.choice(caller.partners)
            receiver_num_str = receiver.raw_number
        else:
            receiver = None
            receiver_num_str = random.choice(caller.external_numbers)
        
        duration_sender = random.randint(10, 600)
        
        # --- SENDER LOG (Original) ---
        log_entry_sender = format_log_entry(
            dt_sender, 
            format_number(caller.raw_number), 
            format_number(receiver_num_str), 
            "S", 
            duration_sender
        )
        caller.logs.append((ts, log_entry_sender))
        
        # --- RECEIVER LOG (Mit Jitter) ---
        if receiver:
            # 1. Zeit-Jitter (0 bis 10 Sekunden Verzögerung beim Empfang)
            jitter_time = random.randint(0, 10)
            dt_receiver = dt_sender + timedelta(seconds=jitter_time)
            ts_receiver = dt_receiver.timestamp()
            
            # 2. Dauer-Jitter (-5 bis +5 Sekunden Abweichung, aber mind. 1 Sekunde)
            jitter_duration = random.randint(-5, 5)
            duration_receiver = max(1, duration_sender + jitter_duration)
            
            log_entry_receiver = format_log_entry(
                dt_receiver, 
                format_number(caller.raw_number), 
                format_number(receiver.raw_number), 
                "E", 
                duration_receiver
            )
            
            # WICHTIG: Wir speichern auch hier den Timestamp für die Sortierung
            receiver.logs.append((ts_receiver, log_entry_receiver))
            
        generated_count += 1
        if generated_count % 50000 == 0:
            print(f"  ... {generated_count} Anrufe verarbeitet.")

def main():
    ensure_dir(OUTPUT_DIR)
    
    print("Baue Netzwerk...")
    suspects = create_network(FILES_TO_GENERATE)
    
    print("Simuliere Anrufe...")
    generate_global_calls(suspects)
    
    print("\nSchreibe sortierte Log-Dateien...")
    for s in suspects:
        filename = f"log{s.id + 1}.csv"
        filepath = os.path.join(OUTPUT_DIR, filename)
        
        # SORTIERUNG: Wir sortieren nach dem Timestamp (Index 0 im Tupel)
        # Damit sind die Logs chronologisch korrekt, auch wenn Empfangs-Logs 
        # durch Jitter oder Generierungsreihenfolge durcheinander waren.
        s.logs.sort(key=lambda x: x[0])
        
        with open(filepath, "w", encoding="utf-8") as f:
            f.write("Datum;Zeit;Anrufer;Angerufener;Typ;Richtung;Dauer\n")
            for _, log_line in s.logs:
                f.write(log_line + "\n")
                
        print(f"  -> {filename}: {len(s.logs)} Zeilen")

if __name__ == "__main__":
    main()