# CLAn - Call Log Analyzer

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
    
    git clone https://gitlab.com/ms152718212/clan.git

## Build

    cd clan
    dotnet clean
    dotnet publish

Test:

    ./CLAn/bin/Release/net8.0/publish/CLAn --version
    1.0.0

## Run

There are three commands: `import`, `analyze`, `clean`. Get help with:

`./CLAn/bin/Release/net8.0/publish/CLAn <command> --help  `

### Import Data

Example:

    ./CLAn/bin/Release/net8.0/publish/CLAn import -f data/log1.txt
    ./CLAn/bin/Release/net8.0/publish/CLAn import -f data/log2.txt
    ./CLAn/bin/Release/net8.0/publish/CLAn import -f data/log3.txt

### Analyze Data

Example:

    ./CLAn/bin/Release/net8.0/publish/CLAn analyze

    ############ F1: 
    Es gibt Abfragen zu:
        Person58E6EA9A (+491331234566) in Log-Datei: /Users/user/Projects/CLAn/data/log1.txt
        PersonCAC0273A (+491221234567) in Log-Datei: /Users/user/Projects/CLAn/data/log2.txt
        Person3BC64A12 (+491611233333) in Log-Datei: /Users/user/Projects/CLAn/data/log3.txt
    ############ F2: 
    Statistik zu den Telefonnummern:
    Anzahl der unterschiedlichen Telefonnummern: 8 in Log-Datei: /Users/user/Projects/CLAn/data/log1.txt
    Anzahl der unterschiedlichen Telefonnummern: 8 in Log-Datei: /Users/user/Projects/CLAn/data/log2.txt
    Anzahl der unterschiedlichen Telefonnummern: 7 in Log-Datei: /Users/user/Projects/CLAn/data/log3.txt
    Gesamtzahl der unterschiedlichen Telefonnummern über alle Log-Dateien: 13
    ############ F3: 
    Telefonate der abgefragten Telefonnummern untereinander:
    Es gab insgesamt 27 Telefonate zwischen PersonCAC0273A (+491221234567) und Person58E6EA9A (+491331234566).
    Es gab 12 Anrufe von Person58E6EA9A (+491331234566) an PersonCAC0273A (+491221234567):
        Datum: 25.04.2022 23:33:42 Dauer: 6540 [s]
        Datum: 01.05.2022 05:28:23 Dauer: 2640 [s]
        Datum: 04.05.2022 20:37:25 Dauer: 2460 [s]
        Datum: 11.05.2022 17:16:04 Dauer: 3300 [s]
        Datum: 14.05.2022 00:23:15 Dauer: 3780 [s]
        Datum: 17.05.2022 05:31:33 Dauer: 1860 [s]
        Datum: 28.05.2022 05:16:47 Dauer: 6900 [s]
        Datum: 02.06.2022 08:18:31 Dauer: 3240 [s]
        Datum: 14.06.2022 23:55:02 Dauer: 60 [s]
        Datum: 28.06.2022 23:42:23 Dauer: 5520 [s]
        Datum: 05.07.2022 04:56:24 Dauer: 4320 [s]
        Datum: 09.07.2022 10:09:09 Dauer: 780 [s]
    Es gab 15 Anrufe von PersonCAC0273A (+491221234567) an Person58E6EA9A (+491331234566) :
        Datum: 15.04.2022 04:27:26 Dauer: 5040 [s]
        Datum: 22.04.2022 05:25:47 Dauer: 6900 [s]
        Datum: 28.04.2022 06:46:49 Dauer: 1680 [s]
        Datum: 29.04.2022 11:03:22 Dauer: 6180 [s]
        Datum: 17.05.2022 21:28:07 Dauer: 3900 [s]
        Datum: 24.05.2022 07:44:32 Dauer: 5340 [s]
        Datum: 07.06.2022 14:36:16 Dauer: 6780 [s]
        Datum: 15.06.2022 04:23:44 Dauer: 780 [s]
        Datum: 17.06.2022 08:07:17 Dauer: 0 [s]
        Datum: 21.06.2022 13:06:18 Dauer: 2640 [s]
        Datum: 21.06.2022 14:04:14 Dauer: 6420 [s]
        Datum: 27.06.2022 03:26:37 Dauer: 5280 [s]
        Datum: 29.06.2022 08:40:10 Dauer: 6840 [s]
        Datum: 02.07.2022 18:34:28 Dauer: 2940 [s]
        Datum: 13.07.2022 13:00:50 Dauer: 2580 [s]
    Es gab insgesamt 12 Telefonate zwischen Person3BC64A12 (+491611233333) und Person58E6EA9A (+491331234566).
    Es gab 6 Anrufe von Person58E6EA9A (+491331234566) an Person3BC64A12 (+491611233333):
        Datum: 09.04.2022 19:40:23 Dauer: 4620 [s]
        Datum: 22.04.2022 19:42:39 Dauer: 180 [s]
        Datum: 15.05.2022 22:18:49 Dauer: 4980 [s]
        Datum: 16.06.2022 03:18:06 Dauer: 3480 [s]
        Datum: 20.06.2022 16:56:07 Dauer: 5040 [s]
        Datum: 08.07.2022 16:30:28 Dauer: 2460 [s]
    Es gab 6 Anrufe von Person3BC64A12 (+491611233333) an Person58E6EA9A (+491331234566) :
        Datum: 17.04.2022 15:03:23 Dauer: 120 [s]
        Datum: 19.04.2022 18:30:04 Dauer: 4800 [s]
        Datum: 24.04.2022 08:22:04 Dauer: 6000 [s]
        Datum: 25.05.2022 18:49:01 Dauer: 5460 [s]
        Datum: 01.06.2022 20:49:10 Dauer: 720 [s]
        Datum: 13.07.2022 03:39:38 Dauer: 0 [s]
    Es gab insgesamt 27 Telefonate zwischen Person3BC64A12 (+491611233333) und PersonCAC0273A (+491221234567).
    Es gab 9 Anrufe von PersonCAC0273A (+491221234567) an Person3BC64A12 (+491611233333):
        Datum: 08.04.2022 23:09:21 Dauer: 5760 [s]
        Datum: 13.04.2022 15:18:34 Dauer: 3660 [s]
        Datum: 27.04.2022 15:20:41 Dauer: 960 [s]
        Datum: 11.05.2022 20:15:11 Dauer: 1980 [s]
        Datum: 16.05.2022 12:51:24 Dauer: 4620 [s]
        Datum: 26.05.2022 23:56:24 Dauer: 5640 [s]
        Datum: 12.06.2022 03:24:45 Dauer: 4680 [s]
        Datum: 25.06.2022 13:08:31 Dauer: 4020 [s]
        Datum: 10.07.2022 03:13:16 Dauer: 2760 [s]
    Es gab 18 Anrufe von Person3BC64A12 (+491611233333) an PersonCAC0273A (+491221234567) :
        Datum: 07.04.2022 17:09:00 Dauer: 2580 [s]
        Datum: 10.04.2022 01:16:56 Dauer: 5160 [s]
        Datum: 18.04.2022 01:07:12 Dauer: 540 [s]
        Datum: 19.04.2022 16:55:00 Dauer: 240 [s]
        Datum: 21.04.2022 04:09:54 Dauer: 1680 [s]
        Datum: 21.04.2022 12:26:19 Dauer: 6660 [s]
        Datum: 23.04.2022 06:00:22 Dauer: 6360 [s]
        Datum: 03.05.2022 16:39:20 Dauer: 840 [s]
        Datum: 05.05.2022 06:02:06 Dauer: 3120 [s]
        Datum: 09.05.2022 01:11:30 Dauer: 2040 [s]
        Datum: 18.05.2022 15:14:49 Dauer: 4020 [s]
        Datum: 31.05.2022 22:17:29 Dauer: 3480 [s]
        Datum: 09.06.2022 08:21:49 Dauer: 840 [s]
        Datum: 23.06.2022 07:33:03 Dauer: 3900 [s]
        Datum: 24.06.2022 08:27:36 Dauer: 1140 [s]
        Datum: 06.07.2022 02:12:44 Dauer: 6660 [s]
        Datum: 07.07.2022 04:54:06 Dauer: 1200 [s]
        Datum: 12.07.2022 09:01:15 Dauer: 60 [s]
    ############ F4: 
    Vorkommen von Telefonnummern in den Log-Files:
    Person58E6EA9A (+491331234566) kommmt vor in:
        Log-Datei: /Users/user/Projects/CLAn/data/log1.txt
        Log-Datei: /Users/user/Projects/CLAn/data/log2.txt
        Log-Datei: /Users/user/Projects/CLAn/data/log3.txt
    Person6F798279 (+491511234555) kommmt vor in:
        Log-Datei: /Users/user/Projects/CLAn/data/log1.txt
    Person97F73A32 (+491555555555) kommmt vor in:
        Log-Datei: /Users/user/Projects/CLAn/data/log1.txt
        Log-Datei: /Users/user/Projects/CLAn/data/log2.txt
    PersonBEE3455E (+491511233333) kommmt vor in:
        Log-Datei: /Users/user/Projects/CLAn/data/log1.txt
    PersonCF9D7E86 (+491511222222) kommmt vor in:
        Log-Datei: /Users/user/Projects/CLAn/data/log1.txt
    Person8A41A102 (+491511111111) kommmt vor in:
        Log-Datei: /Users/user/Projects/CLAn/data/log1.txt
        Log-Datei: /Users/user/Projects/CLAn/data/log2.txt
    PersonCAC0273A (+491221234567) kommmt vor in:
        Log-Datei: /Users/user/Projects/CLAn/data/log1.txt
        Log-Datei: /Users/user/Projects/CLAn/data/log2.txt
        Log-Datei: /Users/user/Projects/CLAn/data/log3.txt
    Person74988BC6 (+491511234444) kommmt vor in:
        Log-Datei: /Users/user/Projects/CLAn/data/log1.txt
        Log-Datei: /Users/user/Projects/CLAn/data/log2.txt
    PersonCDC9EE54 (+491611222222) kommmt vor in:
        Log-Datei: /Users/user/Projects/CLAn/data/log2.txt
        Log-Datei: /Users/user/Projects/CLAn/data/log3.txt
    Person3BC64A12 (+491611233333) kommmt vor in:
        Log-Datei: /Users/user/Projects/CLAn/data/log2.txt
        Log-Datei: /Users/user/Projects/CLAn/data/log3.txt
    PersonD0E70F7D (+491611234555) kommmt vor in:
        Log-Datei: /Users/user/Projects/CLAn/data/log2.txt
        Log-Datei: /Users/user/Projects/CLAn/data/log3.txt
    PersonAA388837 (+491544444445) kommmt vor in:
        Log-Datei: /Users/user/Projects/CLAn/data/log3.txt
    PersonAF32B9BD (+491513333331) kommmt vor in:
        Log-Datei: /Users/user/Projects/CLAn/data/log3.txt
    ############ F5: 
    Gemeinsame Telefonkontakte:
    Die Abgefragten Person58E6EA9A, PersonCAC0273A hatten jeweils Anrufe mit:
        Person97F73A32 (+491555555555)
        Person8A41A102 (+491511111111)
        Person74988BC6 (+491511234444)
    Die Abgefragten Person58E6EA9A, Person3BC64A12 hatten jeweils Anrufe mit:
        PersonCAC0273A (+491221234567)
    Die Abgefragten PersonCAC0273A, Person3BC64A12 hatten jeweils Anrufe mit:
        Person58E6EA9A (+491331234566)
        PersonCDC9EE54 (+491611222222)
        PersonD0E70F7D (+491611234555)


### Clean Data

This causes the database file (see [config.json](./CLAn/config.json)) to be deleted:

    ./CLAn/bin/Release/net8.0/publish/CLAn clean

## Customize

Customize the SQLite database name, the separator and the log data format in [config.json](./CLAn/config.json).

### Sample Log file

[log1.txt](data/log1.txt) fits the default format setting in [config.json](./CLAn/config.json):

    Datum;Zeit;Anrufer;Angerufener;Typ;Richtung;Dauer
    Mi. 06.04.2022;21:01:42;+491331234566;01511222222;SPRACHE;S;01:25
    Do. 07.04.2022;21:39:26;01511111111;001331234566;SPRACHE;E;00:43
    Fr. 08.04.2022;12:03:44;01331234566;01511111111;SPRACHE;S;01:37
    Sa. 09.04.2022;15:14:19;01511222222;+491331234566;SPRACHE;E;00:41
    So. 10.04.2022;04:40:40;001511234444;001331234566;SPRACHE;E;01:33
    ...

Random sample data using [gendata.py](data/gendata.py):

    python3 data/gendata.py > data/randomData.txt
