# CLAn - Call Log Analyzer

This little `C#` puzzle was actually a coding challenge for a data forensics 🕵️‍♂️ role [^note-2]

## Summary of the Task

> **Goal:** 
>
> Develop a console application in **C# (.NET 8.0)** to analyze two CSV files [^note-1] containing billing data from phone calls. 
> 
> **Tasks:**
> 
>* Identify the different telephone numbers: per file and in total.
>
>* Identify the queried telephone numbers (target numbers).
>
>* Examine the contacts between the two target numbers (when and how often?).
>
>* Identify common contacts (telephone numbers that appear in both source files).

[^note-1]: Two files each containing a few hundred lines were provided.

[^note-2]: I used AI (specifically ChatGPT 4.0 in early 2024) to help draft parts of the [Python test data generator](./data/gendata.py). Let's just say this approach raised some eyebrows among the interviewers—they asked if I'd managed *any* part of the task without AI! They also warned my script wouldn't handle a million lines of test data. Got me there! I think I forgot some indices and transaction techniques in the data model. I improved this with version 1.0.2.

## Implementation

A .NET 8.0 console application for analyzing telephone logs.

    ./CLAn/bin/Release/net8.0/publish/CLAn --help
    Description:
    CLAn - Call Log Analyzer

    Usage:
    CLAn [command] [options]

    Options:
    --version       Show version information
    -?, -h, --help  Show help and usage information

    Commands:
    import   Daten einlesen
    analyze  Analyse durchführen
    clean    Alle Daten löschen

## Install 

### SDK & Runtime

Check if available:

    dotnet --list-sdks

    dotnet --list-runtimes

[Download .NET 8.0 & Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0), if not yet installed.

### Load the Repository
    
    git clone https://github.com/masc2026/clan.git

## Build

### Restore (one a new machine):

    cd clan
    dotnet restore

### Build:

    cd clan
    dotnet clean
    dotnet publish

Test:

    ./CLAn/bin/Release/net8.0/publish/CLAn --version
    1.0.2

## About the Python Test Data Generator `gendata.py`

The [`gendata.py`](./data/gendata.py) script generates the CSV test data for the C# application. It simulates a realistic network of phone calls between "suspects."

#### Customize

**1. Volume & Output:**
* `FILES_TO_GENERATE`: Number of log files (suspects) to create. Default is `15`.
* `TARGET_ROWS`: The goal for the number of calls per file. *Note: The script stops early if the simulation reaches the end date.*
* `OUTPUT_DIR`: The folder where CSV files are saved.

**2. Timeframe:**
* `START_DATE` & `END_DATE`: The date range for the call logs.

**3. Behavior (The "Chatty" Factor):**
* `MIN_PAUSE_MINUTES` & `MAX_PAUSE_MINUTES`: The break time between calls. 
    * *Tip:* Lower numbers = "Chatty" person (more data). Higher numbers = relaxed person (less data).
* `SLEEP_START_HOUR` & `SLEEP_END_HOUR`: The hours when the person is sleeping (no calls generated).

**How to run:**

```bash
python3 data/gendata.py
```

## Run

There are three commands: `import`, `analyze`, `clean`. Get help with:

`./CLAn/bin/Release/net8.0/publish/CLAn <command> --help  `


### Performance Benchmark

Comparison between **Apple Mac Mini M1** (2020) and **Arch Linux PC** (2024).

| Step | Task / Command | Dataset size <br> *(Rows per File / Total)* | Runtime `macOS` <br> *(M1 16GB)* | Runtime `Linux` <br> *(Ryzen 7 9800X3D 64GB)* |
| :--- | :--- | :--- | :--- | :--- |
| **1.** | **Generate** (`gendata.py`) | 20k / 300k | 2.0s | 1.2s |
| **2.** | **Import** (`CLAn import`) | 20k / 300k | 116s (~2 min) | 108s (~1.8 min) |
| **3.** | **Analyze** (`CLAn analyze`) | 20k / 300k | 2.0s | 1.2s |
| | | | | |
| **1.** | **Generate** (`gendata.py`) | 50k / 750k | 5.0s | 3.0s |
| **2.** | **Import** (`CLAn import`) | 50k / 750k | 620s (~10 min) | 510s (~8.5 min) |
| **3.** | **Analyze** (`CLAn analyze`) | 50k / 750k | 3.0s | 2.1s |

### 1.) Test Data

Generate new Test data (the _call logs_) _or_ use log file data in [Sample.zip](./data/Sample.zip) _or_ chose your own:

#### a) Generate new Test data

    python3 data/gendata.py

#### b) Use the log file data included in the repository

Extract [Sample.zip](./data/Sample.zip) content to [./data](./data/).

> ⚠️ **Note:** All phone numbers in this dataset are randomly generated. Any resemblance to actual phone numbers is purely coincidental.

#### c) Choose your own files

See [Log file format](#log-file-format).

### 2.) Import Data

Example with 15 (according to the `FILES_TO_GENERATE` setting in [`gendata.py`](./data/gendata.py) ) call log files:

    for i in {1..15}; do ./CLAn/bin/Release/net8.0/publish/CLAn import -f data/log$i.csv; done

### 3.) Analyze Data

Example (using the log file data in [Sample.zip](./data/Sample.zip)):

    ./CLAn/bin/Release/net8.0/publish/CLAn analyze

    ############ F1: 
    Es gibt Abfragen zu:
        Person79AB6A3E (+4916xxxxxxxxx) in Log-Datei: /Users/user/Projekte/clan/data/log1.csv
        Person1DA9C6BA (+491xxxxxxxxx) in Log-Datei: /Users/user/Projekte/clan/data/log2.csv
        Person22780263 (+4916xxxxxxxxx) in Log-Datei: /Users/user/Projekte/clan/data/log3.csv
        PersonD81BAF6E (+491xxxxxxxxx) in Log-Datei: /Users/user/Projekte/clan/data/log4.csv
        Person7A524C13 (+491xxxxxxxxx) in Log-Datei: /Users/user/Projekte/clan/data/log5.csv
        PersonAC589DD1 (+491xxxxxxxxx) in Log-Datei: /Users/user/Projekte/clan/data/log6.csv
        Person99F9AA78 (+491xxxxxxxxx) in Log-Datei: /Users/user/Projekte/clan/data/log7.csv
        PersonAA275805 (+4917xxxxxxxxx) in Log-Datei: /Users/user/Projekte/clan/data/log8.csv
        Person1393D7C6 (+491xxxxxxxxx) in Log-Datei: /Users/user/Projekte/clan/data/log9.csv
        PersonFF37A5D7 (+491xxxxxxxxx) in Log-Datei: /Users/user/Projekte/clan/data/log10.csv
        PersonD56B2FEC (+491xxxxxxxxx) in Log-Datei: /Users/user/Projekte/clan/data/log11.csv
        Person80163E3A (+4917xxxxxxxxx) in Log-Datei: /Users/user/Projekte/clan/data/log12.csv
        Person7F34300D (+4917xxxxxxxxx) in Log-Datei: /Users/user/Projekte/clan/data/log13.csv
        Person0F939A50 (+491xxxxxxxxx) in Log-Datei: /Users/user/Projekte/clan/data/log14.csv
        Person5C468395 (+4917xxxxxxxxx) in Log-Datei: /Users/user/Projekte/clan/data/log15.csv

    ############ F2: 
    Statistik zu den Telefonnummern:
    Anzahl der unterschiedlichen Telefonnummern: 31 in Log-Datei: /Users/user/Projekte/clan/data/log1.csv
    Anzahl der unterschiedlichen Telefonnummern: 31 in Log-Datei: /Users/user/Projekte/clan/data/log2.csv
    Anzahl der unterschiedlichen Telefonnummern: 31 in Log-Datei: /Users/user/Projekte/clan/data/log3.csv
    Anzahl der unterschiedlichen Telefonnummern: 31 in Log-Datei: /Users/user/Projekte/clan/data/log4.csv
    Anzahl der unterschiedlichen Telefonnummern: 31 in Log-Datei: /Users/user/Projekte/clan/data/log5.csv
    Anzahl der unterschiedlichen Telefonnummern: 31 in Log-Datei: /Users/user/Projekte/clan/data/log6.csv
    Anzahl der unterschiedlichen Telefonnummern: 31 in Log-Datei: /Users/user/Projekte/clan/data/log7.csv
    Anzahl der unterschiedlichen Telefonnummern: 31 in Log-Datei: /Users/user/Projekte/clan/data/log8.csv
    Anzahl der unterschiedlichen Telefonnummern: 31 in Log-Datei: /Users/user/Projekte/clan/data/log9.csv
    Anzahl der unterschiedlichen Telefonnummern: 31 in Log-Datei: /Users/user/Projekte/clan/data/log10.csv
    Anzahl der unterschiedlichen Telefonnummern: 31 in Log-Datei: /Users/user/Projekte/clan/data/log11.csv
    Anzahl der unterschiedlichen Telefonnummern: 31 in Log-Datei: /Users/user/Projekte/clan/data/log12.csv
    Anzahl der unterschiedlichen Telefonnummern: 31 in Log-Datei: /Users/user/Projekte/clan/data/log13.csv
    Anzahl der unterschiedlichen Telefonnummern: 31 in Log-Datei: /Users/user/Projekte/clan/data/log14.csv
    Anzahl der unterschiedlichen Telefonnummern: 31 in Log-Datei: /Users/user/Projekte/clan/data/log15.csv
    Gesamtzahl der unterschiedlichen Telefonnummern über alle Log-Dateien: 336
    ############ F3: 
    Telefonate der abgefragten Telefonnummern untereinander:
    Es gab insgesamt 663 Telefonate zwischen Person99F9AA78 (+491xxxxxxxxx) und Person79AB6A3E (+4916xxxxxxxxx).
    Es gab 345 Anrufe von Person79AB6A3E (+4916xxxxxxxxx) an Person99F9AA78 (+491xxxxxxxxx):
        Datum: 06.01.2018 08:04:00 Dauer: 13680 [s]
        Datum: 06.01.2018 08:59:16 Dauer: 3000 [s]
        Datum: 07.01.2018 09:02:56 Dauer: 10620 [s]
    ...
    ############ F4: 
    Vorkommen von Telefonnummern in den Log-Files:
    Person79AB6A3E (+4916xxxxxxxxx) kommmt vor in:
        Log-Datei: /Users/user/Projekte/clan/data/log1.csv
        Log-Datei: /Users/user/Projekte/clan/data/log2.csv
        Log-Datei: /Users/user/Projekte/clan/data/log10.csv
        Log-Datei: /Users/user/Projekte/clan/data/log11.csv
        Log-Datei: /Users/user/Projekte/clan/data/log14.csv
    Person99F9AA78 (+491xxxxxxxxx) kommmt vor in:
        Log-Datei: /Users/user/Projekte/clan/data/log1.csv
        Log-Datei: /Users/user/Projekte/clan/data/log5.csv
        Log-Datei: /Users/user/Projekte/clan/data/log6.csv
        Log-Datei: /Users/user/Projekte/clan/data/log7.csv
    ...
    ############ F5: 
    Gemeinsame Telefonkontakte:
    Die Abgefragten Person79AB6A3E, Person99F9AA78 hatten jeweils Anrufe mit:
        PersonAA275805 (+4917xxxxxxxxx)
        PersonED9572B6 (+4916xxxxxxxxx)
    Die Abgefragten Person79AB6A3E, PersonAA275805 hatten jeweils Anrufe mit:
        Person99F9AA78 (+491xxxxxxxxx)
        Person5E812683 (+491xxxxxxxxx)
    ...

### Clean Data

This causes the database file (see [config.json](./CLAn/config.json)) to be deleted:

    ./CLAn/bin/Release/net8.0/publish/CLAn clean

## Customize

Customize the SQLite database name, the separator and the log data format in [config.json](./CLAn/config.json).

### Log file format

Default format settings given in [config.json](./CLAn/config.json):

    Datum;Zeit;Anrufer;Angerufener;Typ;Richtung;Dauer
    Mi. 06.04.2022;21:01:42;+491331234566;01511222222;SPRACHE;S;01:25
    Do. 07.04.2022;21:39:26;01511111111;001331234566;SPRACHE;E;00:43
    Fr. 08.04.2022;12:03:44;01331234566;01511111111;SPRACHE;S;01:37
    Sa. 09.04.2022;15:14:19;01511222222;+491331234566;SPRACHE;E;00:41
    So. 10.04.2022;04:40:40;001511234444;001331234566;SPRACHE;E;01:33
    ...