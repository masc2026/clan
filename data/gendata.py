# Dieser Code wurde mit Hilfe von Gemini 3 Pro erstellt.
# Generierungsdatum: 2025-12-11
#

import random
from datetime import date, datetime, timedelta, time
import os

# --- KONFIGURATION FÜR MAXIMALE DATEN ---
OUTPUT_DIR = "data"
FILES_TO_GENERATE = 15      # Erzeugt log1.csv bis log15.csv
TARGET_ROWS = 50000         # Zielwert (wird durch Zeitlimit begrenzt, aber wir zielen hoch)
START_DATE = date(2018, 1, 6)
END_DATE = date(2025, 12, 11)

# Verhalten: "Plaudertasche" (kurze Pausen für mehr Daten)
MIN_PAUSE_MINUTES = 1       # Fast sofort weitertelefonieren
MAX_PAUSE_MINUTES = 15      # Max 15 Min Pause
SLEEP_START_HOUR = 23       # Ab 23 Uhr Ruhe
SLEEP_END_HOUR = 7          # Bis 7 Uhr Ruhe

# --- HILFSFUNKTIONEN ---

def ensure_dir(directory):
    if not os.path.exists(directory):
        os.makedirs(directory)

def random_prefix():
    return random.choice(["+49", "0", "0049"])

def generate_random_number():
    prefix = random.choice(["151", "160", "170", "171", "162", "172", "157", "177"])
    rest = "".join([str(random.randint(0, 9)) for _ in range(random.choice([7, 8]))])
    return f"{prefix}{rest}"

def format_number(raw_number):
    return f"{random_prefix()}{raw_number}"

# --- CORE LOGIC: REALISTIC SIMULATION ---

WEEKDAYS = {0: "Mo.", 1: "Di.", 2: "Mi.", 3: "Do.", 4: "Fr.", 5: "Sa.", 6: "So."}

def generate_log_content_simulated(max_rows, start_date, end_date, main_number_raw, partner_list_raw):
    call_records = []
    
    # Startzeitpunkt: 08:00 Uhr am ersten Tag
    current_dt = datetime.combine(start_date, time(8, 0, 0))
    end_dt_limit = datetime.combine(end_date, time(23, 59, 59))
    
    generated_count = 0
    
    while generated_count < max_rows and current_dt < end_dt_limit:
        
        # 1. PAUSE VOR DEM ANRUF
        pause_minutes = random.randint(MIN_PAUSE_MINUTES, MAX_PAUSE_MINUTES)
        current_dt += timedelta(minutes=pause_minutes)
        
        # 2. SCHLAF-CHECK
        if current_dt.hour >= SLEEP_START_HOUR or current_dt.hour < SLEEP_END_HOUR:
            if current_dt.hour >= SLEEP_START_HOUR:
                current_dt += timedelta(days=1)
            
            current_dt = current_dt.replace(hour=SLEEP_END_HOUR, minute=0, second=0)
            current_dt += timedelta(minutes=random.randint(5, 45)) # Kleiner Jitter
        
        if current_dt > end_dt_limit:
            break

        # 3. ANRUF DAUER (10s bis 5min)
        duration_seconds = random.randint(10, 300)
        
        call_date_str = f"{WEEKDAYS[current_dt.weekday()]} {current_dt.strftime('%d.%m.%Y')}"
        call_time_str = current_dt.strftime("%H:%M:%S")
        duration_str = f"{duration_seconds // 60:02}:{duration_seconds % 60:02}"
        
        # Partner & Richtung
        partner_raw = random.choice(partner_list_raw)
        p_num = format_number(partner_raw)
        m_num = format_number(main_number_raw)
        
        direction = random.choice(["E", "S"])
        if direction == "E":
            caller, receiver = p_num, m_num
        else:
            caller, receiver = m_num, p_num
            
        call_records.append(f"{call_date_str};{call_time_str};{caller};{receiver};SPRACHE;{direction};{duration_str}")
        
        # Zeit fortschreiben
        current_dt += timedelta(seconds=duration_seconds)
        generated_count += 1
        
    return call_records

# --- NETZWERK SIMULATOR ---

def create_scenarios(num_scenarios):
    scenarios = []
    # Globaler Pool von Nummern, die immer mal wieder bei verschiedenen Leuten auftauchen
    global_pool = [generate_random_number() for _ in range(100)] 
    
    # Die Hauptnummern für unsere 15 Dateien
    main_numbers = [generate_random_number() for _ in range(num_scenarios)]
    
    print(f"Planung des Netzwerks für {num_scenarios} Dateien...")
    
    for i in range(num_scenarios):
        current_main = main_numbers[i]
        partners = set()
        
        # Kette: Vorgänger & Nachfolger kennen
        if i > 0: partners.add(main_numbers[i-1])
        if i < num_scenarios - 1: partners.add(main_numbers[i+1])
        
        # Zufällige Vernetzung: Jeder kennt noch 1-2 andere Hauptverdächtige zufällig
        other_mains = [m for m in main_numbers if m != current_main]
        partners.update(random.sample(other_mains, k=2))

        # Gemeinsame Kontakte aus dem Pool (die "Unbeteiligten", die jeder kennt)
        partners.update(random.sample(global_pool, k=random.randint(5, 15)))
        
        # Exklusive Kontakte (Mama, Papa, Pizzabote - kennt nur diese Person)
        # Wir füllen auf bis ca 25-30 Kontakte pro Person
        while len(partners) < 30:
            partners.add(generate_random_number())
            
        scenarios.append((current_main, list(partners)))
        print(f"  Szenario {i+1}: Main {current_main} hat {len(partners)} Kontakt-Partner.")
        
    return scenarios

# --- MAIN ---

def main():
    ensure_dir(OUTPUT_DIR)
    scenarios = create_scenarios(FILES_TO_GENERATE)
    
    print(f"\nStarte Simulation (Zeitraum: {START_DATE} bis {END_DATE})...\n")
    
    total_calls = 0
    for idx, (main_num, partner_list) in enumerate(scenarios):
        filename = f"log{idx+1}.csv"
        filepath = os.path.join(OUTPUT_DIR, filename)
        
        rows = generate_log_content_simulated(TARGET_ROWS, START_DATE, END_DATE, main_num, partner_list)
        total_calls += len(rows)
        
        print(f"  -> {filename}: {len(rows)} Anrufe generiert.")
        
        with open(filepath, "w", encoding="utf-8") as f:
            f.write("Datum;Zeit;Anrufer;Angerufener;Typ;Richtung;Dauer\n")
            for row in rows:
                f.write(row + "\n")
                
    print(f"\nFertig! Insgesamt {total_calls} Datensätze in {FILES_TO_GENERATE} Dateien generiert.")

if __name__ == "__main__":
    main()