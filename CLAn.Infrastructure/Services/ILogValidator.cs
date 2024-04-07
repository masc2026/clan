/*
 * Copyright (c) 2024 Markus Schmid
 *
 * Dieser Quellcode ist Teil von CLAn - Call Log Analyzer und unterliegt der MIT-Lizenz, die
 * im Wurzelverzeichnis dieses Projekts als LICENSE-Datei hinterlegt ist. 
 * Eine Kopie der Lizenz können Sie unter folgendem Link einsehen:
 *
 * https://opensource.org/licenses/MIT
 */
 
namespace CLAn.Infrastructure.Services
{
    public enum LogValidationLevel
    {
        Valid = 0,
        MissingData = 1,
        PhoneNumberValidationError = 2,
        DateTimeValidationError = 3,
        CallDirectionValidationError = 4,
        DurationValidationError = 5,
        DuplicateValidationError = 50,
        UnknownLogValidationError = 101
    }

    public enum PhonenumberValidationLevel
    {
        Valid = 0,
        RawFormatError = 1,
        PhoneNumberParseError = 2,
        UnknownPhoneNumberValidationError = 101
    }

    public interface ILogValidator
    {
        LogValidationLevel Validate(string line);
        T GetItem<T>(string key);
    }
}