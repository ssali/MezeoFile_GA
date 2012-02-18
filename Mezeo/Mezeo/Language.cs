using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Resources;
using System.Text.RegularExpressions;

namespace Mezeo
{
    public static class Language
    {
        private static List<string> supportedLanguages;

        public static List<string> GetSupportedLanguages() 
        {
            if (supportedLanguages == null)
            {
                string[] names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                supportedLanguages = new List<string>();

                Debugger.logMessage("Language", "Supported Language Count: " + names.Length);
                
                for (int i = 0; i < names.Length; i++)
                {
                    Debugger.logMessage("Language", "Resources Name: " + names[i]);

                    if (Path.GetExtension(names[i]).Equals(".resources", StringComparison.OrdinalIgnoreCase))
                    {
                        supportedLanguages.Add(Path.GetFileNameWithoutExtension(names[i]));
                        
                        Debugger.logMessage("Language", "Language Name: " + Path.GetFileNameWithoutExtension(names[i]));
                    }
                }
            }
            return supportedLanguages; 
        }
 
        public static ResourceManager GetResourceManager(string languageCode) 
        {
            //languageCode = "Mezeo." + languageCode;
            Debugger.logMessage("Language", "Getting resource manager for: " + languageCode);
            foreach (string name in GetSupportedLanguages()) 
            { 
                string[] arrLanguageCode = Regex.Split(name,"[ ./]");
 
                string supportedLanuageCode = arrLanguageCode[arrLanguageCode.Length - 1];

                Debugger.logMessage("Language", "Supported Language code: " + supportedLanuageCode);
                
                if (supportedLanuageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase)) 
                { 
                    ResourceManager resMan = new ResourceManager(name, Assembly.GetExecutingAssembly()); 
                    return resMan; 
                } 
            } 

            return null; 
        } 

    }
}
