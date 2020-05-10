using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.IO;
using System.Management;
using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography;

namespace cryptocheats_shit
{
    class Program
    {
        private static string ENCRYPT_KEY { get; set; }
        private static string ENCRYPT_SALT { get; set; }
        private static string username { get; set; }
        private static string password { get; set; }
        private static string URL = "http://loader.pop17.com/cryptocheats/index.php";

        static void Main(string[] args)
        {
            Console.Write("USERNAME: ");
            username = Console.ReadLine();
            Console.Write("PASSWORD: ");
            password = Console.ReadLine();

            Console.WriteLine("URL: " + URL);

            ENCRYPT_KEY = Convert.ToBase64String(Encoding.Default.GetBytes(Session_ID(32)));
            ENCRYPT_SALT = Convert.ToBase64String(Encoding.Default.GetBytes(Session_ID(16)));

            Console.WriteLine("ENCRYPT_KEY: " + ENCRYPT_KEY);
            Console.WriteLine("ENCRYPT_SALT: " + ENCRYPT_SALT);

            NameValueCollection nameValueCollection = new NameValueCollection();
            nameValueCollection["type"] = "login";
            //nameValueCollection["username"] = "upthere";
            nameValueCollection["username"] = username;
            //nameValueCollection["password"] = "iwantcheats";
            nameValueCollection["password"] = password;
            nameValueCollection["hwid"] = getUniqueID();
            nameValueCollection["session_id"] = ENCRYPT_KEY;
            nameValueCollection["session_salt"] = ENCRYPT_SALT;

            object arg = JsonConvert.DeserializeObject(Payload(nameValueCollection, true));



            Console.WriteLine("ARG: " + Environment.NewLine + arg);



            Console.WriteLine("\n\n\n\nPRESS ENTER TO EXIT");
            Console.ReadLine();
        }

        public static string Payload(NameValueCollection Values, bool encrypted = false)
        {
            string result;
            try
            {
                if (!encrypted)
                {
                    result = Encoding.Default.GetString(new WebClient().UploadValues(URL, Values));
                }
                else
                {
                    byte[] key = SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes(Encoding.Default.GetString(Convert.FromBase64String(ENCRYPT_KEY))));
                    byte[] bytes = Encoding.ASCII.GetBytes(Encoding.Default.GetString(Convert.FromBase64String(ENCRYPT_SALT)));
                    result = String_Encryption.DecryptString(Encoding.Default.GetString(new WebClient().UploadValues(URL, Values)), key, bytes);
                }
            }
            catch (WebException ex)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                HttpStatusCode statusCode = ((HttpWebResponse)ex.Response).StatusCode;
                if (statusCode <= HttpStatusCode.NotFound)
                {
                    if (statusCode != HttpStatusCode.Forbidden)
                    {
                        if (statusCode == HttpStatusCode.NotFound)
                        {
                            dictionary.Add("result", "net_error");
                        }
                    }
                    else
                    {
                        dictionary.Add("result", "net_error");
                    }
                }
                else if (statusCode != HttpStatusCode.RequestEntityTooLarge)
                {
                    if (statusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        dictionary.Add("result", "net_error");
                    }
                }
                else
                {
                    dictionary.Add("result", "net_error");
                }
                dictionary.Add("result", "net_error");
                result = JsonConvert.SerializeObject(dictionary);
            }
            return result;
        }

        public static string getUniqueID()
        {
            string text = "C";
            if (text == string.Empty)
            {
                foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
                {
                    if (driveInfo.IsReady)
                    {
                        text = driveInfo.RootDirectory.ToString();
                        break;
                    }
                }
            }
            if (text.EndsWith(":\\"))
            {
                text = text.Substring(0, text.Length - 2);
            }
            string volumeSerial = getVolumeSerial(text);
            string cpuid = getCPUID();
            return cpuid.Substring(13) + cpuid.Substring(1, 4) + volumeSerial + cpuid.Substring(4, 4);
        }

        private static string getVolumeSerial(string drive)
        {
            ManagementObject managementObject = new ManagementObject("win32_logicaldisk.deviceid=\"" + drive + ":\"");
            managementObject.Get();
            //string result = managementObject["VolumeSerialNumber"].ToString();
            string result = "123";
            managementObject.Dispose();
            return result;
        }

        private static string getCPUID()
        {
            string text = "";
            foreach (ManagementBaseObject managementBaseObject in new ManagementClass("win32_processor").GetInstances())
            {
                ManagementObject managementObject = (ManagementObject)managementBaseObject;
                if (text == "")
                {
                    text = managementObject.Properties["processorID"].Value.ToString();
                    break;
                }
            }
            return text;
        }

        private static string Session_ID(int length)
        {
            Random random = new Random();
            return new string((from s in Enumerable.Repeat<string>("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz", length)
                               select s[random.Next(s.Length)]).ToArray<char>());
        }
    }

    public class String_Encryption
    {
        // Token: 0x06000048 RID: 72 RVA: 0x0000414C File Offset: 0x0000234C
        public static string EncryptString(string plainText, byte[] key, byte[] iv)
        {
            Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Key = key;
            aes.IV = iv;
            MemoryStream memoryStream = new MemoryStream();
            ICryptoTransform transform = aes.CreateEncryptor();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
            byte[] bytes = Encoding.ASCII.GetBytes(plainText);
            cryptoStream.Write(bytes, 0, bytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] array = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(array, 0, array.Length);
        }

        // Token: 0x06000049 RID: 73 RVA: 0x000041C8 File Offset: 0x000023C8
        public static string DecryptString(string cipherText, byte[] key, byte[] iv)
        {
            Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Key = key;
            aes.IV = iv;
            MemoryStream memoryStream = new MemoryStream();
            ICryptoTransform transform = aes.CreateDecryptor();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
            string result = string.Empty;
            try
            {
                byte[] array = Convert.FromBase64String(cipherText);
                cryptoStream.Write(array, 0, array.Length);
                cryptoStream.FlushFinalBlock();
                byte[] array2 = memoryStream.ToArray();
                result = Encoding.ASCII.GetString(array2, 0, array2.Length);
            }
            finally
            {
                memoryStream.Close();
                cryptoStream.Close();
            }
            return result;
        }
    }
}