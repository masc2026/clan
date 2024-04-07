/*
 * Copyright (c) 2024 Markus Schmid
 *
 * Dieser Quellcode ist Teil von CLAn - Call Log Analyzer und unterliegt der MIT-Lizenz, die
 * im Wurzelverzeichnis dieses Projekts als LICENSE-Datei hinterlegt ist. 
 * Eine Kopie der Lizenz können Sie unter folgendem Link einsehen:
 *
 * https://opensource.org/licenses/MIT
 */
 
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CLAn.Infrastructure.Services
{
    public class CSVLogValidator(ILoggerFactory loggerFactory, IConfigurationRoot configurationRoot, IPhoneNumberValidator numberValidator) : ILogValidator
    {
        public enum CallDirection
        {
            InBound = 0, // PhoneNumberB -> PhoneNumberA
            OutBound = 1 // PhoneNumberA -> PhoneNumberB
        }

        private readonly IConfigurationRoot config = configurationRoot;
        private readonly IPhoneNumberValidator validator = numberValidator;
        private Dictionary<string, object> dictionary = [];

        static private DateTime? GetDateTimeFromDEFormat(string date, string time) 
        {
            // format time ex.: "So. 01.03.2020";
            // format time ex.: "02:44:40";

            string dateString = date;
            string timeString = time;

            string[] dateParts = dateString.Split(' ');
            dateString = String.Join(" ", dateParts[1..]);

            string dateTimeString = dateString + " " + timeString;

            string format = "dd.MM.yyyy HH:mm:ss";

            if (DateTime.TryParseExact(dateTimeString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            else
            {
                return null;
            }

        }

        public LogValidationLevel Validate(string line) 
        {
            char separator = config.GetValue<char>("CSVData:Separator");
            var columns = line.Split(separator);
            dictionary = [];

            LogValidationLevel result=LogValidationLevel.Valid;

            if (columns.Length >= config.GetValue<int>("CSVData:MinColumns"))
            {
                // DateTime
                try
                {
                    DateTime? res=GetDateTimeFromDEFormat(columns[config.GetValue<int>("CSVData:Column:Date")],columns[config.GetValue<int>("CSVData:Column:Time")]);
                    if(res==null) 
                    {
                        return LogValidationLevel.DateTimeValidationError;
                    }
                    else 
                    {
                        dictionary.Add("DateTime", res);
                    }
                }
                catch (Exception)
                {
                    return LogValidationLevel.UnknownLogValidationError;
                }

                // Duration
                try
                {
                    long? res;
                    string s=columns[config.GetValue<int>("CSVData:Column:Duration")];
                    if(s.Length>0)
                    {
                        TimeSpan timeSpan = TimeSpan.Parse(s);
                        res = (long)timeSpan.TotalSeconds;
                        dictionary.Add("Duration", res);
                    }
                    else 
                    {
                        return LogValidationLevel.DurationValidationError;                    
                    }
                }
                catch (Exception)
                {
                    return LogValidationLevel.UnknownLogValidationError;
                }

                // CallDirection
                try 
                {
                    CallDirection? res;
                    string s=columns[config.GetValue<int>("CSVData:Column:Outbound")];
                    if(s.Length>0) 
                    {
                        char c=columns[config.GetValue<int>("CSVData:Column:Outbound")][0];
                        if(c=='E') {
                            res=CallDirection.InBound;
                        }
                        else {
                            res=CallDirection.OutBound;
                        }
                        dictionary.Add("CallDirection", res);
                    }
                    else 
                    {
                        return LogValidationLevel.CallDirectionValidationError;                    
                    }
                }
                catch (Exception)
                {
                    return LogValidationLevel.UnknownLogValidationError;
                }

                // PhoneNumberA
                try 
                {
                    string? res;
                    string s=columns[config.GetValue<int>("CSVData:Column:PhoneNumberA")];
                    PhonenumberValidationLevel vl=validator.Validate(s);
                    if(vl==PhonenumberValidationLevel.Valid)
                    {
                        res=validator.GetPhoneNumber();
                        dictionary.Add("PhoneNumberA", res);
                    }
                    else 
                    {
                        return LogValidationLevel.PhoneNumberValidationError;                  
                    }
                }
                catch (Exception)
                {
                    return LogValidationLevel.UnknownLogValidationError;
                }
                // PhoneNumberB
                try 
                {
                    string? res;
                    string s=columns[config.GetValue<int>("CSVData:Column:PhoneNumberB")];
                    PhonenumberValidationLevel vl=validator.Validate(s);
                    if(vl==PhonenumberValidationLevel.Valid)
                    {
                        res=validator.GetPhoneNumber();
                        dictionary.Add("PhoneNumberB", res);
                    }
                    else 
                    {
                        return LogValidationLevel.PhoneNumberValidationError;                
                    }
                }
                catch (Exception)
                {
                    return LogValidationLevel.UnknownLogValidationError;
                }
            }
            return result;
        }
        public T GetItem<T>(string key)
        {
            if (!dictionary.ContainsKey(key))
            {
                throw new KeyNotFoundException("Der angegebene Schlüssel existiert nicht im Dictionary.");
            }

            object value = dictionary[key];
            if (value is T tValue)
            {
                return tValue;
            }
            else
            {
                throw new InvalidCastException($"Der Wert kann nicht in den Typ {typeof(T).Name} umgewandelt werden.");
            }
        }

    }
}