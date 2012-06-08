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
       // private static ResourceManager branding;

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

            string keyValue;

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
                   keyValue = stringReplace(value);

                   return keyValue; 
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

        public static string stringReplace(string KeyValue)
        {
           KeyValue = KeyValue.Replace("$$COMPANY$$",global::Mezeo.Properties.Resources.BrAssemblyCompany);
           KeyValue = KeyValue.Replace("$$PRODUCT$$", global::Mezeo.Properties.Resources.BrSyncManagerTitle);
           return KeyValue;
        }
    }
}
