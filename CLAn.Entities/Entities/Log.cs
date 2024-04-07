/*
 * Copyright (c) 2024 Markus Schmid
 *
 * Dieser Quellcode ist Teil von CLAn - Call Log Analyzer und unterliegt der MIT-Lizenz, die
 * im Wurzelverzeichnis dieses Projekts als LICENSE-Datei hinterlegt ist. 
 * Eine Kopie der Lizenz können Sie unter folgendem Link einsehen:
 *
 * https://opensource.org/licenses/MIT
 */
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLAn.Entities
{
    public class Log
    {
        public int Id { get; set; }
        public required long LineNumber { get; set; }
        public required string Raw { get; set; }
        public required DateTime DateTime { get; set; }
        public required long Duration { get; set; }
        public int SenderId { get; set; }
        public Person? Sender { get; set; }
        public int ReceiverId { get; set; }
        public Person? Receiver { get; set; }
        public int LogFileId { get; set; }
        public LogFile? LogFile { get; set; }
        public int LogFileOwnerId { get; set; }
        public Person? LogFileOwner { get; set; }
        public int LogFileOwnerPhoneNumberId { get; set; }
        public PhoneNumber? LogFileOwnerPhoneNumber { get; set; }
        public required int Validation { get; set; }
    }

}
