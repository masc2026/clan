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

namespace CLAn.Infrastructure.Services
{
    public class ImportService(ILoggerFactory loggerFactory, ILogValidator logValidator, SqliteContext sqliteConsoleContext) : IImportService
    {
        private readonly ILogger<IImportService> logger = loggerFactory.CreateLogger<ImportService>();
        private readonly ILogValidator validator = logValidator;
        private readonly SqliteContext context = sqliteConsoleContext;

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

        private (Person,PhoneNumber) GetPersonByPhoneNumberOrNew(string number)
        {
            var person = context.Persons
                    .Where(p => p.PhoneNumbers != null && p.PhoneNumbers.Any(pn => pn.Number == number))
                    .FirstOrDefault();
            if (person == null)
            {
                String ownerName = GenerateRandomName();
                person = new Person()
                {
                    Name = ownerName
                };
                context.Add(person);
                context.SaveChanges();
                var phoneNumber = new PhoneNumber()
                {
                    Number = number,
                    Person = person,
                    PersonId = person.Id
                };
                context.Add(phoneNumber);
                context.SaveChanges();
                return (person,phoneNumber);
            }
            else {
                var phoneNumber = context.PhoneNumbers
                    .Where(p => p.Number == number)
                    .FirstOrDefault();
                return (person!,phoneNumber!);
            }
        }

        public void AddLogFile(string filePath)
        {
            var ownerPhoneNumber = ReadOwnerPhoneNumber(filePath);
            if(ownerPhoneNumber!=null) 
            {
                var owner = GetPersonByPhoneNumberOrNew(ownerPhoneNumber);

                var logFile = new LogFile()
                {
                    FullPath = filePath,
                    PersonId = owner.Item1.Id,
                    Person = owner.Item1,
                    OwnerPhoneNumber = owner.Item2,
                    OwnerPhoneNumberId = owner.Item2.Id
                };

                context.LogFiles.Add(logFile);
                context.SaveChanges();
                Ingest(filePath, logFile, owner.Item2);
                context.SaveChanges();
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
                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            LogValidationLevel lvl = validator.Validate(line);
                            if (validator.Validate(line) == LogValidationLevel.Valid && validator.GetItem<CSVLogValidator.CallDirection>("CallDirection") == CSVLogValidator.CallDirection.OutBound)
                            {
                                return validator.GetItem<string>("PhoneNumberA");
                            }
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
            }
            return null;
        }

        private void Ingest(string filePath, LogFile logFile, PhoneNumber ownerPhoneNumber)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    string? line;
                    int lineNumber = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        LogValidationLevel lvl = validator.Validate(line);
                        if (validator.Validate(line) == LogValidationLevel.Valid)
                        {
                            try 
                            {
                                var senderPhoneNumber = validator.GetItem<string>("PhoneNumberA");
                                var receiverPhoneNumber = validator.GetItem<string>("PhoneNumberB");

                                var sender = GetPersonByPhoneNumberOrNew(senderPhoneNumber);
                                var receiver = GetPersonByPhoneNumberOrNew(receiverPhoneNumber);

                                var exists = context.PhoneNumberLogFiles
                                    .Any(x => x.PhoneNumber.Number == senderPhoneNumber && x.LogFileId == logFile.Id);

                                if (!exists)
                                {
                                    var phoneNumber = context.PhoneNumbers
                                        .Where(p => p.Number == senderPhoneNumber)
                                        .FirstOrDefault();

                                    if (phoneNumber != null)
                                    {
                                        var phoneNumberLogFile = new PhoneNumberLogFile
                                        {
                                            PhoneNumber = phoneNumber,
                                            PhoneNumberId = phoneNumber.Id,
                                            LogFile = logFile,
                                            LogFileId = logFile.Id,
                                            LogFileOwnerPhoneNumber = ownerPhoneNumber,
                                            LogFileOwnerPhoneNumberId = ownerPhoneNumber.Id
                                        };
                                        context.PhoneNumberLogFiles.Add(phoneNumberLogFile);
                                        context.SaveChanges();
                                    }
                                }

                                exists = context.PhoneNumberLogFiles
                                    .Any(x => x.PhoneNumber.Number == receiverPhoneNumber && x.LogFileId == logFile.Id);

                                if (!exists)
                                {
                                    var phoneNumber = context.PhoneNumbers
                                        .Where(p => p.Number == receiverPhoneNumber)
                                        .FirstOrDefault();

                                    if (phoneNumber != null)
                                    {
                                        var phoneNumberLogFile = new PhoneNumberLogFile
                                        {
                                            PhoneNumber = phoneNumber,
                                            PhoneNumberId = phoneNumber.Id,
                                            LogFile = logFile,
                                            LogFileId = logFile.Id,
                                            LogFileOwnerPhoneNumber = ownerPhoneNumber,
                                            LogFileOwnerPhoneNumberId = ownerPhoneNumber.Id
                                        };
                                        context.PhoneNumberLogFiles.Add(phoneNumberLogFile);
                                        context.SaveChanges();
                                    }
                                }

                                DateTime dateTime = validator.GetItem<DateTime>("DateTime");
                                long duration = validator.GetItem<long>("Duration");
                                int senderId = sender.Item1.Id;
                                int receiverId = receiver.Item1.Id;

                                bool logExists = context.Logs.Any(log =>
                                    log.DateTime == dateTime &&
                                    log.Duration == duration &&
                                    log.SenderId == senderId &&
                                    log.ReceiverId == receiverId);

                                if (!logExists)
                                {
                                    var log = new Log()
                                    {
                                        LineNumber = lineNumber,
                                        DateTime = dateTime,
                                        Duration = duration,
                                        Raw = line,
                                        SenderId = senderId,
                                        Sender = sender.Item1,
                                        ReceiverId = receiverId,
                                        Receiver = receiver.Item1,
                                        LogFileOwnerId = logFile.PersonId,
                                        LogFileOwner = logFile.Person,
                                        LogFileOwnerPhoneNumberId = ownerPhoneNumber.Id,
                                        LogFileOwnerPhoneNumber = ownerPhoneNumber,
                                        LogFile = logFile,
                                        LogFileId = logFile.Id,
                                        Validation = (int)lvl
                                    };

                                    context.Logs.Add(log);
                                    context.SaveChanges();
                                }
                            }
                            catch(Exception e) {
                                logger.LogError(e.Message);
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException fne)
            {
                logger.LogError(fne.Message);
            }
        }
    }
}