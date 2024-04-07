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
    public class PhoneNumberLogFile
    {
        public int PhoneNumberId { get; set; }
        public required PhoneNumber PhoneNumber { get; set; }
        public int LogFileId { get; set; }
        public required LogFile LogFile { get; set; }
        public int LogFileOwnerPhoneNumberId { get; set; }
        public required PhoneNumber LogFileOwnerPhoneNumber { get; set; }
    }

}
