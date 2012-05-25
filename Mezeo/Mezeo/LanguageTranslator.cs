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
            currentLanguage=language;
            resManager=Language.GetResourceManager(language);
        }

        public static string GetValue(string key) 
        {
         
            if (resManager == null)
            {
                return key;
            }
            string originalKey = key; 
            key = Regex.Replace(key, "[ ./]", "_"); 
  
            try 
            { 
                string value = resManager.GetString(key);
                if (value != null)
                {
                    return value;
                }
                return originalKey; 
            } 
            catch (MissingManifestResourceException) 
            {
                throw new System.IO.FileNotFoundException("Could not locate the resource file for the language " + currentLanguage); 
                
            } 
            catch (NullReferenceException) 
            {
                return originalKey; 
            } 
        } 
    }
}
