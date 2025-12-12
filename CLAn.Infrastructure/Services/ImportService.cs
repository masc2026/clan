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
using Microsoft.Extensions.Logging;
using CLAn.Entities;
using CLAn.Infrastructure.Data;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CLAn.Infrastructure.Services
{
    public class ImportService(ILoggerFactory loggerFactory, ILogValidator logValidator, SqliteContext sqliteConsoleContext) : IImportService
    {
        private readonly ILogger<IImportService> logger = loggerFactory.CreateLogger<ImportService>();
        private readonly ILogValidator validator = logValidator;
        private readonly SqliteContext context = sqliteConsoleContext;
        private Dictionary<string, PhoneNumber> phoneNumberCache = new();

        private static string GenerateRandomName()
        {
            byte[] randomNumber = new byte[4];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }

            string hexString = BitConverter.ToString(randomNumber).Replace("-", string.Empty);
            return $"Person{hexString}";
        }

        private PhoneNumber GetPhoneNumberInternal(string number)
        {
            if (phoneNumberCache.TryGetValue(number, out var existingPn))
            {
                return existingPn;
            }

            // Fallback: Prüfen ob schon in DB (falls Cache noch leer war)
            var dbPn = context.PhoneNumbers
                .Include(p => p.Person)
                .FirstOrDefault(p => p.Number == number);

            if (dbPn != null)
            {
                phoneNumberCache[number] = dbPn;
                return dbPn;
            }
            var newPerson = new Person { Name = GenerateRandomName() };
            context.Persons.Add(newPerson);

            var newPn = new PhoneNumber 
            { 
                Number = number, 
                Person = newPerson,
                PersonId = newPerson.Id 
            }; 
            context.PhoneNumbers.Add(newPn);
            
            phoneNumberCache[number] = newPn; 
            
            return newPn;
        }

        public void AddLogFile(string filePath)
        {
            var ownerPhoneNumberString = ReadOwnerPhoneNumber(filePath);
            if (ownerPhoneNumberString != null)
            {
                phoneNumberCache = context.PhoneNumbers
                                    .Include(p => p.Person)
                                    .ToDictionary(k => k.Number, v => v);

                var ownerPhoneNumber = GetPhoneNumberInternal(ownerPhoneNumberString);

                var logFile = new LogFile()
                {
                    FullPath = filePath,
                    Person = ownerPhoneNumber.Person!,
                    OwnerPhoneNumber = ownerPhoneNumber
                };

                context.LogFiles.Add(logFile);
                
                context.SaveChanges(); 

                Ingest(filePath, logFile, ownerPhoneNumber);
                
                Cleanup();
            }
        }

        public void Cleanup() 
        {
            // Step 1: Lösche alle LogFiles und PhoneNumberLogFiles Einträge, die nicht mehr verwendet werden
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var logFileIdsWithoutLogs = context.LogFiles
                    .Where(lf => lf.Logs.Count == 0)
                    .Select(lf => lf.Id)
                    .ToList();

                var logFilesToDelete = context.LogFiles
                    .Where(lf => lf.Logs.Count == 0)
                    .ToList();
                    
                var phoneNumberLogFilesToDelete = context.PhoneNumberLogFiles
                    .Where(pnlf => logFileIdsWithoutLogs.Contains(pnlf.LogFileId))
                    .ToList();

                context.PhoneNumberLogFiles.RemoveRange(phoneNumberLogFilesToDelete);
                context.SaveChanges();

                context.LogFiles.RemoveRange(logFilesToDelete);
                context.SaveChanges();
                
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private string? ReadOwnerPhoneNumber(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    string? line;
                    int maxLines = 10; 
                    int current = 0;
                    while ((line = reader.ReadLine()) != null && current < maxLines)
                    {
                        current++;
                        if (validator.Validate(line) == LogValidationLevel.Valid && validator.GetItem<CSVLogValidator.CallDirection>("CallDirection") == CSVLogValidator.CallDirection.OutBound)
                        {
                            return validator.GetItem<string>("PhoneNumberA");
                        }
                    }
                }
            }
            catch (Exception e) { logger.LogError(e.Message); }
            return null;
        }

        private void Ingest(string filePath, LogFile logFile, PhoneNumber ownerPhoneNumber)
        {
            var sw = Stopwatch.StartNew();
            context.ChangeTracker.AutoDetectChangesEnabled = false;

            using var transaction = context.Database.BeginTransaction();

            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    string? line;
                    int lineNumber = 0;
                    int batchSize = 20000;

                    var processedPnLogFiles = new HashSet<string>();

                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        if (validator.Validate(line) == LogValidationLevel.Valid)
                        {
                            try 
                            {
                                var senderPhoneNumber = validator.GetItem<string>("PhoneNumberA");
                                var receiverPhoneNumber = validator.GetItem<string>("PhoneNumberB");

                                var senderPn = GetPhoneNumberInternal(senderPhoneNumber);
                                var receiverPn = GetPhoneNumberInternal(receiverPhoneNumber);

                                if (!processedPnLogFiles.Contains(senderPhoneNumber))
                                {
                                    if (context.Entry(senderPn).State == EntityState.Detached) context.PhoneNumbers.Attach(senderPn);
                                    
                                    context.PhoneNumberLogFiles.Add(new PhoneNumberLogFile
                                    {
                                        PhoneNumber = senderPn,
                                        LogFile = logFile,
                                        LogFileOwnerPhoneNumber = ownerPhoneNumber
                                    });
                                    processedPnLogFiles.Add(senderPhoneNumber);
                                }
                                if (!processedPnLogFiles.Contains(receiverPhoneNumber))
                                {
                                    if (context.Entry(receiverPn).State == EntityState.Detached) context.PhoneNumbers.Attach(receiverPn);
                                    
                                    context.PhoneNumberLogFiles.Add(new PhoneNumberLogFile
                                    {
                                        PhoneNumber = receiverPn,
                                        LogFile = logFile,
                                        LogFileOwnerPhoneNumber = ownerPhoneNumber
                                    });
                                    processedPnLogFiles.Add(receiverPhoneNumber);
                                }

                                DateTime dateTime = validator.GetItem<DateTime>("DateTime");
                                long duration = validator.GetItem<long>("Duration");

                                if (context.Entry(senderPn).State == EntityState.Detached) context.PhoneNumbers.Attach(senderPn);
                                if (context.Entry(receiverPn).State == EntityState.Detached) context.PhoneNumbers.Attach(receiverPn);
                                if (context.Entry(logFile).State == EntityState.Detached) context.LogFiles.Attach(logFile);
                                if (context.Entry(ownerPhoneNumber).State == EntityState.Detached) context.PhoneNumbers.Attach(ownerPhoneNumber);

                                var log = new Log()
                                {
                                    LineNumber = lineNumber,
                                    DateTime = dateTime,
                                    Duration = duration,
                                    Raw = line,
                                    Sender = senderPn.Person, // Achtung: Hier ggf. auch attach prüfen, aber EF macht das meist via PhoneNumber
                                    Receiver = receiverPn.Person,
                                    LogFile = logFile,
                                    LogFileOwner = logFile.Person,
                                    LogFileOwnerPhoneNumber = ownerPhoneNumber,
                                    Validation = (int)LogValidationLevel.Valid
                                };

                                context.Logs.Add(log);

                                if (lineNumber % batchSize == 0)
                                {
                                    context.SaveChanges();
                                    context.ChangeTracker.Clear();
                                    context.ChangeTracker.AutoDetectChangesEnabled = false; 
                                }
                            }
                            catch(Exception e) {
                                transaction.Rollback();
                                logger.LogError(e.Message);
                            }
                        }
                    }
                    context.SaveChanges();
                    transaction.Commit();
                    sw.Stop();
                    Console.WriteLine($"\nFertig! {lineNumber} Zeilen in {sw.Elapsed.TotalSeconds:F2}s ({lineNumber/sw.Elapsed.TotalSeconds:F0} Zeilen/s)");

                }
            }
            catch (FileNotFoundException fne)
            {
                logger.LogError(fne.Message);
            }
        }
    }
}