using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace NGUSaveConvert
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var data = ReadSaveData();
            if (data == null)
                return;

            using (var dialog = new SaveFileDialog())
            {
                dialog.InitialDirectory =
                    Environment.ExpandEnvironmentVariables("%appdata%\\..\\locallow\\ngu industries\\ngu idle\\");
                dialog.FileName = "NGUSave.json";
                dialog.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
                dialog.Title = "Save JSON";
                dialog.RestoreDirectory = true;
                dialog.ShowDialog();

                if (dialog.FileName != "")
                {
                    File.WriteAllText(dialog.FileName, data);
                }
            }
        }

        private static string ReadSaveData()
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                var ngupath = Path.GetFullPath(Environment.ExpandEnvironmentVariables("%appdata%\\..\\locallow\\ngu industries\\ngu idle\\"));
                dialog.InitialDirectory = ngupath;
                
                dialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.RestoreDirectory = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var path = dialog.FileName;
                    if (!File.Exists(path))
                    {
                        Console.WriteLine("Bad filepath");
                        return null;
                    }

                    try
                    {
                        var content = File.ReadAllText(path);
                        var data = DeserializeBase64<SaveData>(content);
                        var checksum = GetChecksum(data.playerData);
                        if (checksum != data.checksum)
                        {
                            Console.WriteLine("Bad checksum");
                            return null;
                        }

                        var playerData = DeserializeBase64<PlayerData>(data.playerData);

                        var jsonData = JsonConvert.SerializeObject(playerData);
                        return jsonData;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return null;
                    }
                }

                return null;
            }
        }

        //Code sort of taken from ngu-save-analyzer
        private static T DeserializeBase64<T>(string base64Data)
        {
            var bytes = Convert.FromBase64String(base64Data);
            var formatter = new BinaryFormatter();

            using (var memoryStream = new MemoryStream(bytes))
            {
                return (T)formatter.Deserialize(memoryStream);
            }
        }

        private static string GetChecksum(string data)
        {
            var md5 = new MD5CryptoServiceProvider();

            return Convert.ToBase64String(md5.ComputeHash(Convert.FromBase64String(data)));
        }
    }
}
