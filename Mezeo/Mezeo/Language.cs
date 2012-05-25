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

                 
                
                for (int i = 0; i < names.Length; i++)
                {
                    if (Path.GetExtension(names[i]).Equals(".resources", StringComparison.OrdinalIgnoreCase))
                    {
                        supportedLanguages.Add(Path.GetFileNameWithoutExtension(names[i]));
                    }
                }
            }
            return supportedLanguages; 
        }
 
        public static ResourceManager GetResourceManager(string languageCode) 
        {
            foreach (string name in GetSupportedLanguages()) 
            { 
                string[] arrLanguageCode = Regex.Split(name,"[ ./]");
 
                string supportedLanuageCode = arrLanguageCode[arrLanguageCode.Length - 1];
      
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
