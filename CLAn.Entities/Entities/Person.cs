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
    public class Person
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public ICollection<PhoneNumber> PhoneNumbers { get; set; } = [];
        public ICollection<LogFile> LogFiles { get; set; } = [];
        public ICollection<PersonGroup> PersonGroups { get; set; } = [];
        public ICollection<Log> Logs { get; set; } = [];
    }

}
