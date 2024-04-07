/*
 * Copyright (c) 2024 Markus Schmid
 *
 * Dieser Quellcode ist Teil von CLAn - Call Log Analyzer und unterliegt der MIT-Lizenz, die
 * im Wurzelverzeichnis dieses Projekts als LICENSE-Datei hinterlegt ist. 
 * Eine Kopie der Lizenz können Sie unter folgendem Link einsehen:
 *
 * https://opensource.org/licenses/MIT
 */

using Microsoft.Extensions.Configuration;
using CLAn.Entities;
using CLAn.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CLAn.Infrastructure.Services
{
    public class AnalyzeService(SqliteContext sqliteConsoleContext) : IAnalyzeService
    {
        private readonly SqliteContext context = sqliteConsoleContext;

        private void PrintAllPhoneNumberPersonsLogFiles()
        {
            var query = (from phoneNumber in context.PhoneNumbers
                         select new
                         {
                             PhoneNumber = phoneNumber.Number,
                             LogFiles = phoneNumber.PhoneNumberLogFiles
                                                     .Select(pnl => pnl.LogFile.FullPath)
                                                     .ToList(),
                             PersonName = phoneNumber.Person!.Name
                         });
            Console.WriteLine($"Vorkommen von Telefonnummern in den Log-Files:");
            foreach (var result in query)
            {
                Console.WriteLine($"{result.PersonName} ({result.PhoneNumber}) kommmt vor in:");

                foreach (var logFilePath in result.LogFiles)
                {
                    Console.WriteLine($"\tLog-Datei: {logFilePath}");
                }
            }
        }
        private void PrintPhoneNumberStatistic()
        {
            var query = (from logFile in context.LogFiles
                         select new
                         {
                             LogFilePath = logFile.FullPath,
                             PhoneNumberCount = logFile.PhoneNumberLogFiles
                                                 .Select(pnl => pnl.PhoneNumberId)
                                                 .Count()
                         }).ToList();

            Console.WriteLine($"Statistik zu den Telefonnummern:");
            foreach (var result in query)
            {
                Console.WriteLine($"Anzahl der unterschiedlichen Telefonnummern: {result.PhoneNumberCount} in Log-Datei: {result.LogFilePath}");
            }

            var totalUniquePhoneNumbers = context.PhoneNumberLogFiles
                                            .Select(pnl => pnl.PhoneNumberId)
                                            .Distinct()
                                            .Count();

            Console.WriteLine($"Gesamtzahl der unterschiedlichen Telefonnummern über alle Log-Dateien: {totalUniquePhoneNumbers}");
        }

        private void PrintLoggedPhoneNumbers()
        {
            var query = from logFile in context.LogFiles
                        let phoneNumber = logFile.OwnerPhoneNumber
                        select new
                        {
                            FullPath = logFile.FullPath,
                            PhoneNumber = phoneNumber,
                            OwnerName = logFile.Person.Name
                        };
            
            Console.WriteLine($"Es gibt Abfragen zu:");
            foreach (var result in query)
            {
                Console.WriteLine($"\t{result.OwnerName} ({result.PhoneNumber.Number}) in Log-Datei: {result.FullPath}");
            }
        }

        private List<int> LoggedPersonIds()
        {
            var query = (from logFile in context.LogFiles
                        select logFile.PersonId).ToList();
            
            return query;
        }

        public class CommonPhoneNumberResult
        {
            public string? Name { get; set; }
            public string? Number { get; set; }

            public string? LogFilePaths { get; set; }
        }

        private void PrintPersonsCommonContactsPairwise(List<int> personids)
        {
            Console.WriteLine($"Gemeinsame Telefonkontakte:");
            foreach (var p1 in personids)
            {
                var now = 0;
                foreach (var p2 in personids)
                {
                    if (p1 == p2 && now == 0)
                    {
                        now = 1;
                    }
                    if (now == 2)
                    {
                        PrintPersonsCommonContacts([p1, p2]);
                    }
                    if (now == 1)
                    {
                        now = 2;
                    }
                }
            }
        }

        private void PrintPersonsCommonContacts(List<int> personids)
        {

            var personidsString = $"({string.Join(",", personids)})";

            string sql = @"
                SELECT ps.Name, pn.Number, GROUP_CONCAT(l.FullPath, ' und ') AS LogFilePaths
                FROM PhoneNumberLogFiles p
                JOIN LogFile l ON p.LogFileId = l.Id
                JOIN PhoneNumber pn ON p.PhoneNumberId = pn.Id
                JOIN Person ps ON p.PhoneNumberId = ps.Id
                WHERE 
                    NOT p.LogFileOwnerPhoneNumberId = p.PhoneNumberId 
                    AND p.LogFileOwnerPhoneNumberId IN "+personidsString+
                @" AND p.PhoneNumberId NOT IN "+personidsString+
                @" GROUP BY p.PhoneNumberId
                HAVING COUNT(DISTINCT l.FullPath) > 1;";

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                context.Database.OpenConnection();

                using (var result = command.ExecuteReader())
                {
                    var list = new List<CommonPhoneNumberResult>();
                    while (result.Read())
                    {
                        var commonResult = new CommonPhoneNumberResult
                        {
                            Name = result.GetString(result.GetOrdinal("Name")),
                            Number = result.GetString(result.GetOrdinal("Number")),
                            LogFilePaths = result.GetString(result.GetOrdinal("LogFilePaths"))
                        };
                        list.Add(commonResult);
                    }

                    var namen = context.PhoneNumbers
                                    .Where(pn => personids.Contains(pn.PersonId))
                                    .Select(pn => pn.Person!.Name)
                                    .ToList();
                                        
                    Console.WriteLine($"Die Abgefragten {string.Join(", ", namen)} hatten jeweils Anrufe mit:");
                    foreach (var res in list)
                    {
                        Console.WriteLine($"\t{res.Name} ({res.Number})");
                    }
                }
            }
        }

        private void PrintPersonsCalls(List<int> personids)
        {
            Console.WriteLine($"Telefonate der abgefragten Telefonnummern untereinander:");
            foreach (var p1 in personids)
            {
                var now = 0;
                foreach (var p2 in personids)
                {
                    if (p1 == p2 && now == 0)
                    {
                        now = 1;
                    }
                    if (now == 2)
                    {
                        PrintPersonsCalls(p1, p2);
                    }
                    if (now == 1)
                    {
                        now = 2;
                    }
                }
            }
        }

        private void PrintPersonsCalls(int p1Id, int p2Id)
        {
            var query = (from log in context.Logs
                         where (log.SenderId == p1Id && log.ReceiverId == p2Id && log.Validation == 0)
                         select new
                         {
                             log.DateTime,
                             log.Duration
                         })
                .OrderBy(log => log.DateTime);

            var queryRev = (from log in context.Logs
                            where (log.SenderId == p2Id && log.ReceiverId == p1Id && log.Validation == 0)
                            select new
                            {
                                log.DateTime,
                                log.Duration
                            })
                .OrderBy(log => log.DateTime);

            var p1PhoneNumber = context.PhoneNumbers
                                .Where(pn => pn.PersonId == p1Id)
                                .Select(pn => pn.Number)
                                .FirstOrDefault();

            var p2PhoneNumber = context.PhoneNumbers
                                .Where(pn => pn.PersonId == p2Id)
                                .Select(pn => pn.Number)
                                .FirstOrDefault();

            var p1Name = context.PhoneNumbers
                                .Where(pn => pn.PersonId == p1Id)
                                .Select(pn => pn.Person!.Name)
                                .FirstOrDefault();

            var p2Name = context.PhoneNumbers
                                .Where(pn => pn.PersonId == p2Id)
                                .Select(pn => pn.Person!.Name)
                                .FirstOrDefault();

            Console.WriteLine($"Es gab insgesamt {query.Count() + queryRev.Count()} Telefonate zwischen {p2Name} ({p2PhoneNumber}) und {p1Name} ({p1PhoneNumber}).");

            Console.WriteLine($"Es gab {query.Count()} Anrufe von {p1Name} ({p1PhoneNumber}) an {p2Name} ({p2PhoneNumber}):");

            foreach (var result in query)
            {
                Console.WriteLine($"\tDatum: {result.DateTime} Dauer: {result.Duration} [s]");
            }

            Console.WriteLine($"Es gab {queryRev.Count()} Anrufe von {p2Name} ({p2PhoneNumber}) an {p1Name} ({p1PhoneNumber}) :");

            foreach (var result in queryRev)
            {
                Console.WriteLine($"\tDatum: {result.DateTime} Dauer: {result.Duration} [s]");
            }

        }

        private void SetUp(int resolution)
        {
            // Step 1: Reset Resolution
            string sql = @"
                UPDATE Log 
                SET Validation=0
                WHERE Validation=50;";
            context.Database.ExecuteSqlRaw(sql);

            // Step 2: Set Resolution
            sql = @"
                WITH duplic AS (
                    SELECT 
                        id,
                        datetime,
                        senderId,
                        receiverId,
                        time_diff,
                        rs_changed,
                        LogFileId
                    FROM 
                        (SELECT 
                            id,
                            LogFileId,
                            datetime,
                            senderId,
                            receiverId,
                            CAST(strftime('%s', datetime) AS INTEGER) - LAG(CAST(strftime('%s', datetime) AS INTEGER)) OVER (ORDER BY receiverID||'|'||senderId,datetime) AS time_diff,
                            ((receiverID||'|'||senderId != LAG(receiverID||'|'||senderId) OVER (ORDER BY receiverID||'|'||senderId,datetime))) as rs_changed
                        FROM 
                            Log
                        )
                    WHERE
                        time_diff IS NULL OR time_diff>" + resolution + @" OR rs_changed)
                UPDATE Log 
                SET Validation=50
                WHERE Id NOT IN (SELECT Id FROM duplic);";
            context.Database.ExecuteSqlRaw(sql);
        }
        public void Analyze(int seconds)
        {
            SetUp(seconds);
            Console.WriteLine("############ F1: ");
            PrintLoggedPhoneNumbers();
            Console.WriteLine("############ F2: ");
            PrintPhoneNumberStatistic();
            Console.WriteLine("############ F3: ");
            PrintPersonsCalls(LoggedPersonIds());
            Console.WriteLine("############ F4: ");
            PrintAllPhoneNumberPersonsLogFiles();
            Console.WriteLine("############ F5: ");
            PrintPersonsCommonContactsPairwise(LoggedPersonIds());
        }

    }
}