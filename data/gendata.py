# Dieser Code wurde mit Hilfe von Gemini 3 Pro erstellt.
# Generierungsdatum: 2025-12-17
#

import random
from datetime import date, datetime, timedelta, time
import os

# --- KONFIGURATION ---
OUTPUT_DIR = "data"
FILES_TO_GENERATE = 4
TARGET_ROWS_PER_FILE = 100  
START_DATE = date(2018, 1, 6)
END_DATE = date(2025, 12, 11)

# Verhalten
SLEEP_START_HOUR = 23
SLEEP_END_HOUR = 7
NUM_SUB_CLANS = 5 # Anzahl der "Schattenmänner" / Gruppen

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
    duration_str = f"{duration_seconds // 60:02}:{duration_seconds % 60:02}"
    weekday = ['Mo.', 'Di.', 'Mi.', 'Do.', 'Fr.', 'Sa.', 'So.'][dt.weekday()]
    date_str = f"{weekday} {dt.strftime('%d.%m.%Y')}"
    time_str = dt.strftime("%H:%M:%S")
    return f"{date_str};{time_str};{caller_num};{receiver_num};SPRACHE;{direction};{duration_str}"

# --- WELT SIMULATION ---

class Suspect:
    def __init__(self, id):
        self.id = id
        self.raw_number = generate_raw_number()
        self.partners = []         # Interne Partner (andere überwachte Personen)
        self.external_numbers = [] # Externe Nummern (Mama, Pizza, Schattenmänner)
        self.logs = [] 

def create_network(num_suspects):
    suspects = [Suspect(i) for i in range(num_suspects)]
    
    # 1. Ring-Verbindung
    for i in range(num_suspects):
        prev_s = suspects[i-1]
        next_s = suspects[(i+1) % num_suspects]
        suspects[i].partners.append(prev_s)
        suspects[i].partners.append(next_s)
    
    # 2. Zufällige interne Querverbindungen
    for s in suspects:
        others = [x for x in suspects if x != s and x not in s.partners]
        if others:
            s.partners.extend(random.sample(others, k=min(len(others), random.randint(2, 4))))
            
        # 3. Private externe Kontakte (nur für diese Person)
        for _ in range(15):
            s.external_numbers.append(generate_raw_number())
            
    return suspects

def inject_sub_clans(suspects):
    """
    Erzeugt 'Schattenmänner' (Nummern), die von mehreren, 
    aber nicht allen Suspects kontaktiert werden.
    Das simuliert die Sub-Clans.
    """
    print(f"Injiziere {NUM_SUB_CLANS} Sub-Clans (Schattenmänner)...")
    
    for i in range(NUM_SUB_CLANS):
        # Das ist Person D (der Schattenmann)
        shadow_man_number = generate_raw_number()
        
        # Wähle zufällig 2 bis 5 Mitglieder für diesen Sub-Clan aus
        # Beispiel: Suspect 1, 4 und 8 kennen diesen Schattenmann. Suspect 0 nicht.
        clan_members = random.sample(suspects, k=random.randint(2, len(suspects)))
        
        member_ids = [str(s.id + 1) for s in clan_members]
        print(f"  -> Schattenmann {shadow_man_number} ist bekannt bei Logs: {', '.join(member_ids)}")
        
        for member in clan_members:
            # Wir fügen den Schattenmann MEHRFACH hinzu, damit er häufiger angerufen wird
            # als die zufällige 'Tante Erna'.
            for _ in range(5): 
                member.external_numbers.append(shadow_man_number)

def generate_global_calls(suspects):
    # Schätzung: Ein Anruf erzeugt im Schnitt 1.7 Log-Einträge 
    # (Intern=2 Einträge, Extern=1 Eintrag, bei 70/30 Verteilung)
    total_calls_needed = int((TARGET_ROWS_PER_FILE * len(suspects)) / 1.7)
    
    print(f"Simuliere {total_calls_needed} Anrufe, um ca. {TARGET_ROWS_PER_FILE} Zeilen pro Datei zu erreichen...")
    
    # Zeitgrenzen als Timestamp
    start_ts = datetime.combine(START_DATE, time(0,0)).timestamp()
    end_ts = datetime.combine(END_DATE, time(23,59)).timestamp()
    
    generated_count = 0
    
    # Wir loopen genau so oft, wie wir Anrufe brauchen
    for _ in range(total_calls_needed):
        
        # 1. Finde einen validen Zeitpunkt (außerhalb der Schlafenszeit)
        while True:
            ts = random.uniform(start_ts, end_ts)
            dt_sender = datetime.fromtimestamp(ts)
            # Wenn NICHT Schlafenszeit ist, brechen wir aus und nehmen den Zeitpunkt
            if not (dt_sender.hour >= SLEEP_START_HOUR or dt_sender.hour < SLEEP_END_HOUR):
                break
        
        caller = random.choice(suspects)
        
        # Intern (zu anderen Log-Dateien) vs Extern (zu unüberwachten Nummern)
        is_internal = random.random() < 0.7 
        
        # --- FALL A: INTERNER ANRUF ---
        if is_internal and caller.partners:
            receiver = random.choice(caller.partners)
            duration = random.randint(10, 600)
            
            # Sender Log (Ausgehend)
            log_entry_sender = format_log_entry(
                dt_sender, 
                format_number(caller.raw_number), 
                format_number(receiver.raw_number), 
                "S", 
                duration
            )
            caller.logs.append((ts, log_entry_sender))
            
            # Receiver Log (Eingehend) mit Jitter
            jitter_time = random.randint(0, 10)
            dt_receiver = dt_sender + timedelta(seconds=jitter_time)
            ts_receiver = dt_receiver.timestamp()
            
            jitter_duration = random.randint(-5, 5)
            duration_receiver = max(1, duration + jitter_duration)
            
            log_entry_receiver = format_log_entry(
                dt_receiver, 
                format_number(caller.raw_number), 
                format_number(receiver.raw_number), 
                "E", 
                duration_receiver
            )
            receiver.logs.append((ts_receiver, log_entry_receiver))
            
        # --- FALL B: EXTERNER ANRUF (Bidirektional!) ---
        else:
            # Hier greift deine "5x hinzugefügt"-Logik automatisch, 
            # weil die Schattenmänner öfter in dieser Liste stehen!
            external_num_str = random.choice(caller.external_numbers)
            duration = random.randint(10, 600)
            
            is_incoming = random.random() < 0.5
            
            if is_incoming:
                # Externer ruft Verdächtigen an (Eingehend)
                log_entry = format_log_entry(
                    dt_sender, 
                    format_number(external_num_str),  # Caller = Extern
                    format_number(caller.raw_number), # Receiver = Verdächtiger
                    "E", 
                    duration
                )
            else:
                # Verdächtiger ruft Externen an (Ausgehend)
                log_entry = format_log_entry(
                    dt_sender, 
                    format_number(caller.raw_number), # Caller = Verdächtiger
                    format_number(external_num_str),  # Receiver = Extern
                    "S", 
                    duration
                )
            
            caller.logs.append((ts, log_entry))

        generated_count += 1
        if generated_count % 5000 == 0:
            print(f"  ... {generated_count} Anrufe generiert.")

def main():
    ensure_dir(OUTPUT_DIR)
    
    print("Baue Basis-Netzwerk...")
    suspects = create_network(FILES_TO_GENERATE)
    
    # NEU: Hier werden die Sub-Clans gebildet
    inject_sub_clans(suspects)
    
    print("Simuliere Anrufe...")
    generate_global_calls(suspects)
    
    print("\nSchreibe sortierte Log-Dateien...")
    for s in suspects:
        filename = f"log{s.id + 1}.csv"
        filepath = os.path.join(OUTPUT_DIR, filename)
        
        s.logs.sort(key=lambda x: x[0])
        
        with open(filepath, "w", encoding="utf-8") as f:
            f.write("Datum;Zeit;Anrufer;Angerufener;Typ;Richtung;Dauer\n")
            for _, log_line in s.logs:
                f.write(log_line + "\n")
                
        print(f"  -> {filename}: {len(s.logs)} Zeilen")

if __name__ == "__main__":
    main()