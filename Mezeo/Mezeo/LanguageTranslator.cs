using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.Text.RegularExpressions;

namespace Mezeo
{
    public static class LanguageTranslator
    {
        private static string currentLanguage;
        private static ResourceManager resManager;

        public static string GetLanguage() 
        { 
            return currentLanguage; 
        } 
  

        public static void SetLanguage(string language)
        {
            Debugger.logMessage("LanguageTranslator", "Setting language to " + language);
            currentLanguage=language;
            resManager=Language.GetResourceManager(language);
        }

        public static string GetValue(string key) 
        {
            Debugger.logMessage("LanguageTranslator", "Getting value for key " + key);

            if (resManager == null)
            {
                Debugger.logMessage("LanguageTranslator", "Resource manager null.");
                return key;
            }
            string originalKey = key; 
            key = Regex.Replace(key, "[ ./]", "_"); 
  
            try 
            { 
                string value = resManager.GetString(key);
                if (value != null)
                {
                    Debugger.logMessage("LanguageTranslator", "Language: " + currentLanguage + ", Key: " + key + ", Value: " + value);
                    return value;
                }
                Debugger.logMessage("LanguageTranslator", "No value found for the Key: " + key + ", Language: " + currentLanguage);
                return originalKey; 
            } 
            catch (MissingManifestResourceException) 
            {
                Debugger.logMessage("LanguageTranslator", "Missing resource found for language: " + currentLanguage );
                throw new System.IO.FileNotFoundException("Could not locate the resource file for the language " + currentLanguage); 
                
            } 
            catch (NullReferenceException) 
            {
                Debugger.logMessage("LanguageTranslator", "NPE");
                return originalKey; 
            } 
        } 
    }
}
