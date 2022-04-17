using R2API;
using System.Collections.Generic;
using System.IO;

namespace LunarHeresy
{
	public static class Languages
	{
		public static string RootLanguageFolderPath
		{
			get
			{
				return Path.Combine(Path.GetDirectoryName(LunarHeresy.PluginInfo.Location), "Languages");
			}
		}

		public static void Register()
		{
			try {
				// Load all language files in the /Languages directory
				foreach (string fileName in Directory.GetFiles(RootLanguageFolderPath))
				{
					LanguageAPI.AddPath(fileName);
					LunarHeresy.Logger.LogDebug($"Loaded language file {fileName}");
				}
			} catch (DirectoryNotFoundException) {
				LunarHeresy.Logger.LogError($"Could not load language files, directory '{RootLanguageFolderPath}' was not found.");
			}
			
		}
	}
}