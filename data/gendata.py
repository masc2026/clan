# Dieser Code wurde zum Teil mit Hilfe von ChatGPT, Version 4.0, generiert.
# Generierungsdatum: 2024-04-07
#
# Die Konfiguration, Initialiseriung, Anpassung und Überprüfung des Codes
# erfolgte manuell, um die spezifischen Anforderungen des Projekts zu erfüllen.

from datetime import date
from datetime import timedelta
import random

# Adjusting the script according to the new requirements

# Updating the week day names to German
def german_weekday(day):
    weekdays = {
        "Monday": "Mo.",
        "Tuesday": "Di.",
        "Wednesday": "Mi.",
        "Thursday": "Do.",
        "Friday": "Fr.",
        "Saturday": "Sa.",
        "Sunday": "So."
    }
    return weekdays[day]

# Updating the prefix format to vary randomly
def random_prefix():
    return random.choice(["+49", "0", "0049"])

# Adjusting the call duration to vary between 0 and 1200 seconds
def random_duration_adjusted():
    seconds = random.randint(0, 120)
    return f"{seconds // 60:02}:{seconds % 60:02}"

# Adjusting the function to generate call records within a specific date range
def generate_call_records_adjusted(start_date, end_date, main_number, phone_partners):
    call_records = []
    current_date = start_date

    while current_date <= end_date:
        # Generating random call date and time
        call_date = f"{german_weekday(current_date.strftime('%A'))} {current_date.strftime('%d.%m.%Y')}"
        call_time = f"{random.randint(0, 23):02}:{random.randint(0, 59):02}:{random.randint(0, 59):02}"

        # Selecting random phone partner with adjusted prefix
        partner_number = f"{random_prefix()}{random.choice(phone_partners)[3:]}"

        # Randomly deciding the direction of the call
        direction = random.choice(["E", "S"])

        # Assigning caller and receiver based on the direction with adjusted prefix
        if direction == "E":
            caller = partner_number
            receiver = f"{random_prefix()}{main_number[3:]}"
        else:
            caller = f"{random_prefix()}{main_number[3:]}"
            receiver = partner_number

        # Generating random call duration adjusted
        duration = random_duration_adjusted()

        # Adding the record to the list
        call_records.append(f"{call_date};{call_time};{caller};{receiver};SPRACHE;{direction};{duration}")

        # Incrementing the date for the next record
        current_date += timedelta(days=1)  # Making sure we cover every day within the range

    return call_records

# Setting the start and end date for the call records
start_date = date(2022, 4, 6)
end_date = date(2024, 8, 15)

# Generating call records within the specified date range

# log1.txt
#call_records_adjusted = generate_call_records_adjusted(start_date, end_date, "+491331234566",["+491221234567","+491511234555","+491511234444","+491511233333","+491511222222","+491511111111","+491555555555"])

# log2.txt
#call_records_adjusted = generate_call_records_adjusted(start_date, end_date, "+491221234567",["+491331234566","+491611234555","+491511234444","+491611233333","+491611222222","+491511111111","+491555555555"])

# log3.txt
call_records_adjusted = generate_call_records_adjusted(start_date, end_date, "+491611233333",["+491331234566","+491611234555","+491221234567","+491611233333","+491611222222","+491513333331","+491544444445"])

# Displaying the first few records to check adjustments
print("Datum;Zeit;Anrufer;Angerufener;Typ;Richtung;Dauer")
for record in call_records_adjusted[:100]:
    print(record)