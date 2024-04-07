/*
 * Copyright (c) 2024 Markus Schmid
 *
 * Dieser Quellcode ist Teil von CLAn - Call Log Analyzer und unterliegt der MIT-Lizenz, die
 * im Wurzelverzeichnis dieses Projekts als LICENSE-Datei hinterlegt ist. 
 * Eine Kopie der Lizenz können Sie unter folgendem Link einsehen:
 *
 * https://opensource.org/licenses/MIT
 */
 
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace CLAn.Infrastructure.Services
{
    public partial class DEPhoneNumberValidator(ILoggerFactory loggerFactory) : IPhoneNumberValidator
    {
        private readonly ILogger<IPhoneNumberValidator> logger = loggerFactory.CreateLogger<DEPhoneNumberValidator>();
        private string phonenumber = "";

        private static string CleanInput(string input)
        {
            string result="";
            try 
            {
                input = MyRegex().Replace(input, "");
                result = MyRegex1().Replace(input, match => match.Value == "+" && match.Index == 0 ? "+" : "");
            }
            catch(Exception)
            {
                throw new Exception("Raw Format Error");
            }
            return result;
        }

        // https://en.wikipedia.org/wiki/National_conventions_for_writing_telephone_numbers
        private void Normalize(string input)
        {  
            Regex phoneRegex = MyRegex2();

            try 
            {
                var match = phoneRegex.Match(input);
                if (match.Success)
                {
                    string prefix = match.Groups[1].Value;
                    string countrycode = match.Groups[2].Value;
                    string trunkaccesscode = match.Groups[3].Value;
                    string number = match.Groups[4].Value;
                    
                    // Ersetzt die erste Gruppe durch "+49", falls sie leer ist
                    if (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(countrycode))
                    {
                        phonenumber = "+49"+ number;
                    }
                    if(!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(countrycode))
                    {
                        phonenumber = "+"+ countrycode + number;
                    }
                    if(string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(countrycode))
                    {
                        phonenumber = "+"+ countrycode + number;
                    }
                    else
                    {
                        phonenumber = "+49"+ number;
                    }
                }
                else
                {
                    phonenumber="";
                    throw new Exception("Phone Number Parse Error");
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        public PhonenumberValidationLevel Validate(string s) 
        {
            PhonenumberValidationLevel result=PhonenumberValidationLevel.Valid;
            String tmpRes="";
            try 
            {
                tmpRes=CleanInput(s);
            }
            catch(Exception ex)
            {
                logger.LogWarning(ex.Message);
                result=PhonenumberValidationLevel.RawFormatError;
            }
            if(result==PhonenumberValidationLevel.Valid)
            {
                try 
                {
                    Normalize(tmpRes);
                }
                catch(Exception ex) 
                {
                    logger.LogWarning(ex.Message);
                    result=PhonenumberValidationLevel.PhoneNumberParseError;
                }
            }
            return result;
        }

        public string GetPhoneNumber()
        {
            return phonenumber;
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex MyRegex();
        [GeneratedRegex(@"^(?=\+)|\D+")]
        // https://stackoverflow.com/questions/2113908/what-regular-expression-will-match-valid-international-phone-numbers
        private static partial Regex MyRegex1();
        [GeneratedRegex(@"^(00|\+|)(999|998|997|996|995|994|993|992|991|990|979|978|977|976|975|974|973|972|971|970|969|968|967|966|965|964|963|962|961|960|899|898|897|896|895|894|893|892|891|890|889|888|887|886|885|884|883|882|881|880|879|878|877|876|875|874|873|872|871|870|859|858|857|856|855|854|853|852|851|850|839|838|837|836|835|834|833|832|831|830|809|808|807|806|805|804|803|802|801|800|699|698|697|696|695|694|693|692|691|690|689|688|687|686|685|684|683|682|681|680|679|678|677|676|675|674|673|672|671|670|599|598|597|596|595|594|593|592|591|590|509|508|507|506|505|504|503|502|501|500|429|428|427|426|425|424|423|422|421|420|389|388|387|386|385|384|383|382|381|380|379|378|377|376|375|374|373|372|371|370|359|358|357|356|355|354|353|352|351|350|299|298|297|296|295|294|293|292|291|290|289|288|287|286|285|284|283|282|281|280|269|268|267|266|265|264|263|262|261|260|259|258|257|256|255|254|253|252|251|250|249|248|247|246|245|244|243|242|241|240|239|238|237|236|235|234|233|232|231|230|229|228|227|226|225|224|223|222|221|220|219|218|217|216|215|214|213|212|211|210|98|95|94|93|92|91|90|86|84|82|81|66|65|64|63|62|61|60|58|57|56|55|54|53|52|51|49|48|47|46|45|44|43|41|40|39|36|34|33|32|31|30|27|20|7|1|)(0){0,1}([0-9]{0,13})$")]
        private static partial Regex MyRegex2();
    }
}