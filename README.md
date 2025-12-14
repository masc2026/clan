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

[^note-2]: I used AI (specifically ChatGPT 4.0 in early 2024) to help draft parts of the Python test data generator `data/gendata.py`. Let's just say this approach raised some eyebrows among the interviewers—they asked if I'd managed *any* part of the task without AI! They also warned my script wouldn't handle a million lines of test data. Got me there! I think I forgot some indices and transaction techniques in the data model. I improved this with version 1.0.2.

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

### Restore (on a new machine):

    cd clan
    dotnet restore

### Build:

    cd clan
    dotnet clean
    dotnet publish

Check the Version

    ./CLAn/bin/Release/net8.0/publish/CLAn --version
>
    1.0.2.1

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

Generate new Test data (the _call logs_) _or_ use log file data in (./data/) _or_ chose your own:

#### a) Generate new Test data

    python3 data/gendata.py

#### b) Use the log files included in the repository

> ⚠️ **Note:** All phone numbers in the dataset are randomly generated. Any resemblance to actual phone numbers is purely coincidental.

#### c) Choose your own files

See [Log file format](#log-file-format).

### 2.) Import Data

Example with 3 (according to the `FILES_TO_GENERATE` setting in [`gendata.py`](./data/gendata.py) ) call log files:

    for i in {1..3}; do ./CLAn/bin/Release/net8.0/publish/CLAn import -f data/log$i.csv; done

### 3.) Analyze Data

Example (using the log files data in [./data/](./data/)):

    ./CLAn/bin/Release/net8.0/publish/CLAn analyze

    ############ F1: 
    Es gibt Abfragen zu:
        Person456F4EBB (+49171*******) in Log-Datei: /Users/user/Projekte/github/clan/data/log1.csv
        Person59AC8447 (+491773*******) in Log-Datei: /Users/user/Projekte/github/clan/data/log2.csv
        PersonC3FC26BD (+491571*******) in Log-Datei: /Users/user/Projekte/github/clan/data/log3.csv
    ############ F2: 
    Statistik zu den Telefonnummern:
    Anzahl der unterschiedlichen Telefonnummern: 8 in Log-Datei: /Users/user/Projekte/github/clan/data/log1.csv
    Anzahl der unterschiedlichen Telefonnummern: 5 in Log-Datei: /Users/user/Projekte/github/clan/data/log2.csv
    Anzahl der unterschiedlichen Telefonnummern: 8 in Log-Datei: /Users/user/Projekte/github/clan/data/log3.csv
    Gesamtzahl der unterschiedlichen Telefonnummern über alle Log-Dateien: 15
    ############ F3: 
    Telefonate der abgefragten Telefonnummern untereinander:
    Es gab insgesamt 15 Telefonate zwischen PersonC3FC26BD (+491571*******) und Person456F4EBB (+49171*******).
    Es gab 9 Anrufe von Person456F4EBB (+49171*******) an PersonC3FC26BD (+491571*******):
        Datum: 17.09.2018 13:57:40 Dauer: 26280 [s]
        Datum: 28.09.2018 12:57:34 Dauer: 34080 [s]
        Datum: 06.07.2019 19:59:58 Dauer: 17520 [s]
        Datum: 22.09.2019 15:15:42 Dauer: 20340 [s]
        Datum: 22.05.2020 14:57:57 Dauer: 13680 [s]
        Datum: 10.12.2021 15:35:55 Dauer: 27600 [s]
        Datum: 25.06.2022 11:37:24 Dauer: 12900 [s]
        Datum: 14.01.2024 08:26:24 Dauer: 6600 [s]
        Datum: 11.03.2024 12:12:30 Dauer: 2820 [s]
    Es gab 6 Anrufe von PersonC3FC26BD (+491571*******) an Person456F4EBB (+49171*******) :
        Datum: 31.07.2018 18:42:50 Dauer: 19020 [s]
        Datum: 08.09.2018 16:29:16 Dauer: 14700 [s]
        Datum: 10.07.2020 10:39:04 Dauer: 31260 [s]
        Datum: 02.04.2023 22:48:11 Dauer: 20820 [s]
        Datum: 14.10.2023 18:23:04 Dauer: 12780 [s]
        Datum: 30.12.2023 22:15:57 Dauer: 8820 [s]
    Es gab insgesamt 17 Telefonate zwischen Person59AC8447 (+491773*******) und Person456F4EBB (+49171*******).
    Es gab 6 Anrufe von Person456F4EBB (+49171*******) an Person59AC8447 (+491773*******):
        Datum: 13.11.2018 12:01:52 Dauer: 14820 [s]
        Datum: 24.12.2019 07:21:02 Dauer: 33660 [s]
        Datum: 10.09.2020 14:43:25 Dauer: 3840 [s]
        Datum: 19.01.2022 16:32:46 Dauer: 18720 [s]
        Datum: 15.11.2022 07:33:37 Dauer: 19020 [s]
        Datum: 12.05.2024 08:15:10 Dauer: 11640 [s]
    Es gab 11 Anrufe von Person59AC8447 (+491773*******) an Person456F4EBB (+49171*******) :
        Datum: 27.08.2018 21:00:41 Dauer: 11040 [s]
        Datum: 03.12.2018 21:31:26 Dauer: 22620 [s]
        Datum: 07.09.2019 20:56:58 Dauer: 27000 [s]
        Datum: 04.11.2019 14:01:50 Dauer: 20160 [s]
        Datum: 23.11.2019 14:35:45 Dauer: 4320 [s]
        Datum: 04.01.2020 21:16:01 Dauer: 24360 [s]
        Datum: 31.05.2020 16:32:21 Dauer: 30600 [s]
        Datum: 25.03.2022 16:22:10 Dauer: 14520 [s]
        Datum: 28.07.2022 16:42:21 Dauer: 31800 [s]
        Datum: 07.03.2023 17:23:07 Dauer: 19920 [s]
        Datum: 17.05.2025 10:40:59 Dauer: 1500 [s]
    Es gab insgesamt 20 Telefonate zwischen Person59AC8447 (+491773*******) und PersonC3FC26BD (+491571*******).
    Es gab 9 Anrufe von PersonC3FC26BD (+491571*******) an Person59AC8447 (+491773*******):
        Datum: 24.09.2019 19:43:19 Dauer: 36000 [s]
        Datum: 12.07.2020 17:27:48 Dauer: 17760 [s]
        Datum: 26.01.2021 18:40:25 Dauer: 30840 [s]
        Datum: 13.09.2021 12:03:21 Dauer: 23520 [s]
        Datum: 28.04.2024 22:24:26 Dauer: 33780 [s]
        Datum: 20.06.2024 12:53:42 Dauer: 8160 [s]
        Datum: 25.05.2025 07:54:01 Dauer: 5280 [s]
        Datum: 17.06.2025 14:34:48 Dauer: 25500 [s]
        Datum: 23.06.2025 09:27:49 Dauer: 30720 [s]
    Es gab 11 Anrufe von Person59AC8447 (+491773*******) an PersonC3FC26BD (+491571*******) :
        Datum: 15.07.2018 21:07:59 Dauer: 21180 [s]
        Datum: 01.09.2018 11:52:28 Dauer: 20100 [s]
        Datum: 21.12.2018 11:41:15 Dauer: 5580 [s]
        Datum: 21.08.2019 20:50:37 Dauer: 15780 [s]
        Datum: 18.05.2021 14:29:37 Dauer: 15420 [s]
        Datum: 11.01.2022 09:05:45 Dauer: 21300 [s]
        Datum: 01.05.2022 12:52:26 Dauer: 19320 [s]
        Datum: 29.07.2022 09:29:44 Dauer: 36000 [s]
        Datum: 05.03.2023 07:51:53 Dauer: 14820 [s]
        Datum: 27.04.2023 14:17:18 Dauer: 10560 [s]
        Datum: 24.02.2025 22:42:43 Dauer: 14220 [s]
    ############ F4: 
    Vorkommen von Telefonnummern in den Log-Files:
    Person456F4EBB (+49171*******) kommmt vor in:
        Log-Datei: /Users/user/Projekte/github/clan/data/log1.csv
        Log-Datei: /Users/user/Projekte/github/clan/data/log2.csv
        Log-Datei: /Users/user/Projekte/github/clan/data/log3.csv
    Person58D55D1A (+491571*******) kommmt vor in:
        Log-Datei: /Users/user/Projekte/github/clan/data/log1.csv
    PersonC3FC26BD (+491571*******) kommmt vor in:
        Log-Datei: /Users/user/Projekte/github/clan/data/log1.csv
        Log-Datei: /Users/user/Projekte/github/clan/data/log2.csv
        Log-Datei: /Users/user/Projekte/github/clan/data/log3.csv
    Person59AC8447 (+491773*******) kommmt vor in:
        Log-Datei: /Users/user/Projekte/github/clan/data/log1.csv
        Log-Datei: /Users/user/Projekte/github/clan/data/log2.csv
        Log-Datei: /Users/user/Projekte/github/clan/data/log3.csv
    Person72B51001 (+491623*******) kommmt vor in:
        Log-Datei: /Users/user/Projekte/github/clan/data/log1.csv
    Person11D52BC1 (+491574*******) kommmt vor in:
        Log-Datei: /Users/user/Projekte/github/clan/data/log1.csv
    Person64E1E302 (+491722*******) kommmt vor in:
        Log-Datei: /Users/user/Projekte/github/clan/data/log1.csv
    Person91AD0477 (+49162*******) kommmt vor in:
        Log-Datei: /Users/user/Projekte/github/clan/data/log1.csv
    PersonEA75972A (+49162*******) kommmt vor in:
        Log-Datei: /Users/user/Projekte/github/clan/data/log2.csv
    Person876D6E6C (+49177*******) kommmt vor in:
        Log-Datei: /Users/user/Projekte/github/clan/data/log2.csv
    Person567C2AE6 (+49160*******) kommmt vor in:
        Log-Datei: /Users/user/Projekte/github/clan/data/log3.csv
    Person4B92451A (+491705*******) kommmt vor in:
        Log-Datei: /Users/user/Projekte/github/clan/data/log3.csv
    PersonC242941A (+491715*******) kommmt vor in:
        Log-Datei: /Users/user/Projekte/github/clan/data/log3.csv
    PersonE6A94DF9 (+49177*******) kommmt vor in:
        Log-Datei: /Users/user/Projekte/github/clan/data/log3.csv
    PersonFABF6FE9 (+49157*******) kommmt vor in:
        Log-Datei: /Users/user/Projekte/github/clan/data/log3.csv
    ############ F5: 
    Gemeinsame Telefonkontakte:
    Die Abgefragten Person456F4EBB, PersonC3FC26BD hatten jeweils Anrufe mit:
        Person59AC8447 (+491773*******)
    Die Abgefragten Person456F4EBB, Person59AC8447 hatten jeweils Anrufe mit:
        PersonC3FC26BD (+491571*******)
    Die Abgefragten PersonC3FC26BD, Person59AC8447 hatten jeweils Anrufe mit:
        Person456F4EBB (+49171*******)


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