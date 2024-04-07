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
    public class LogFile
    {
        public int Id { get; set; }
        public required string FullPath { get; set; }
        public int PersonId { get; set; }
        public required Person Person { get; set; }
        public int OwnerPhoneNumberId { get; set; }
        public required PhoneNumber OwnerPhoneNumber { get; set; }
        public ICollection<PhoneNumberLogFile> PhoneNumberLogFiles { get; set; } = [];
        public ICollection<Log> Logs { get; set; } = [];
    }

}
