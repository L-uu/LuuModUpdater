using MelonLoader;
using System.Net;
using System.Security.Cryptography;
using System.Text;

[assembly: MelonInfo(typeof(LuuModUpdater.Main), "LuuModUpdater", "1", "Luu")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace LuuModUpdater
{
	class Main : MelonPlugin
	{
		public override void OnApplicationStart()
		{
			UpdateFunc("LuuMod", "Mods/LuuMod.dll", "https://github.com/L-uu/LuuMod/releases/latest/download/LuuMod.dll");
			UpdateFunc("ReMod.Core", "ReMod.Core.dll", "https://github.com/RequiDev/ReMod.Core/releases/latest/download/ReMod.Core.dll");
		}

		private static void UpdateFunc(string Name, string Path, string URL)
		{
			MelonLogger.Msg($"Checking for {Name} and updating if necessary...");
			byte[]? Bytes = null;
			if (File.Exists($"{Path}"))
			{
				Bytes = File.ReadAllBytes($"{Path}");
			}
			var Wc = new WebClient
			{
				Headers =
				{
					["User-Agent"] =
						"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.51 Safari/537.36 OPR/85.0.4341.18"
				}
			};
			byte[]? LatestBytes = null;
			try
			{
				LatestBytes = Wc.DownloadData($"{URL}");
			}
			catch (WebException ex)
			{
				MelonLogger.Msg($"Failed to download {Name}, you might encounter issues. " + ex.ToString());
			}
			if (Bytes == null)
			{
				if (LatestBytes == null)
				{
					MelonLogger.Error($"Failed to download {Name} and the file doesn't exist. The mod won't work.");
					return;
				}
				MelonLogger.Msg($"{Name} not found, will try and download now...");
				Bytes = LatestBytes;
				try
				{
					File.WriteAllBytes($"{Path}", Bytes);
				}
				catch (IOException ex)
				{
					MelonLogger.Warning($"Failed to write {Name} to disk, you might encounter issues. " + ex.ToString());
				}
			}
			else
			{
				if (LatestBytes != null)
				{
					var sha256 = SHA256.Create();
					var LatestHash = ComputeHash(sha256, LatestBytes);
					var CurrentHash = ComputeHash(sha256, Bytes);
					if (LatestHash != CurrentHash)
					{
						MelonLogger.Msg($"Updating {Name}...");
						Bytes = LatestBytes;
						try
						{
							File.WriteAllBytes($"{Path}", Bytes);
						}
						catch (IOException ex)
						{
							MelonLogger.Warning($"Failed to write {Name} to disk. You might encounter errors. " + ex.ToString());
						}
					}
				}
			}
		}

		// RequiDev/ReModCE/ReModCE.Loader/ReMod.Loader.cs #294
		private static string ComputeHash(HashAlgorithm Sha256, byte[] Data)
		{
			var Bytes = Sha256.ComputeHash(Data);
			var Sb = new StringBuilder();
			foreach (var b in Bytes)
			{
				Sb.Append(b.ToString("x2"));
			}

			return Sb.ToString();
		}
	}
}
