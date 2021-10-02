using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {

        private static readonly HashAlgorithm hash = new SHA1CryptoServiceProvider();
        static void Main(string[] args)
        {
            TcpClient clientSocket = new TcpClient("localhost", 6789);

            Stream ns = clientSocket.GetStream();  //provides a NetworkStream
            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns);
            sw.AutoFlush = true; // enable automatic flushing
            //read user inputs
            Dictionary<string, string> userInformation = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadLine());
            List<string> dictionaryWords = JsonConvert.DeserializeObject<List<string>>(sr.ReadLine());
            String match = "";
            foreach (String word in dictionaryWords)
            {
                string possibleMatch = CheckWithVariations(userInformation, word);
                if (possibleMatch != null)
                {
                    match = possibleMatch;
                    break;
                }
            }
            if (string.IsNullOrEmpty(match))
            {
                Console.WriteLine("There was no match!");
                Console.ReadLine();
            } 
            else
            {
                Console.WriteLine("The match is " + match);
                Console.ReadLine();
            }
            ns.Close();
            clientSocket.Close();
        }
        private static string CheckWithVariations(Dictionary<string, string> userInformation, string w)
        {
            List<string> words = new List<string>();
            words.Add(w);
            words.Add(w.ToUpper());
            words.Add(w.ToLower());
            words.Add(StringUtilities.Capitalize(w));
            words.Add(StringUtilities.Reverse(w));

            for (int i = 0; i < 100; i++)
            {
                words.Add(w + i);
                words.Add(i + w);

                words.Add(w.ToUpper() + i);
                words.Add(w.ToLower() + i);
                words.Add(StringUtilities.Capitalize(w) + i);
                words.Add(StringUtilities.Reverse(w) + i);

                words.Add(i + w.ToUpper());
                words.Add(i + w.ToLower());
                words.Add(i + StringUtilities.Capitalize(w));
                words.Add(i + StringUtilities.Reverse(w));

            }
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    words.Add(i + w + j);
                    words.Add(i + w.ToUpper() + j);
                    words.Add(i + w.ToLower() + j);
                    words.Add(i + StringUtilities.Capitalize(w) + j);
                    words.Add(i + StringUtilities.Reverse(w) + j);
                }
            }
            string match = "";

            match = words.Find((f) => CheckWord(f, userInformation));
            if (String.IsNullOrEmpty(match))
            {
                return null;
            }
            else
            {
                return match;
            }
        }
        public static string GetSha1(string value)
        {
            var data = Encoding.ASCII.GetBytes(value);
            var hashData = new SHA1Managed().ComputeHash(data);
            var hash = string.Empty;
            foreach (var b in hashData)
            {
                hash += b.ToString("X2");
            }
            return hash;
        }
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private static bool CheckWord(string f, Dictionary<string, string> userInformation)
        {
            //Først hasher ordet f
            f = GetSha1(f);
            //Så
            //Formaterer hash værdigen til en hash array eksempel: aabbcc1122 => aa bb cc 11 22
            byte[] fhex = StringToByteArray(f);
            //Laver stringbase64 om til en encoding base64 ud fra en den der array fhex
            string stringbase64 = Convert.ToBase64String(fhex);
            //her laver den det samme som medd passwordbytes
            byte[] wordBytes = Encoding.ASCII.GetBytes(stringbase64);

            foreach (var item in userInformation)
            {

                byte[] passwordBytes = Encoding.ASCII.GetBytes(item.Value);
                if (CompareBytes(wordBytes,passwordBytes))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool CompareBytes(IList<byte> firstArray, IList<byte> secondArray)
        {
            //if (secondArray == null)
            //{
            //    throw new ArgumentNullException("firstArray");
            //}
            //if (secondArray == null)
            //{
            //    throw new ArgumentNullException("secondArray");
            //}
            if (firstArray.Count != secondArray.Count)
            {
                return false;
            }
            for (int i = 0; i < firstArray.Count; i++)
            {
                if (firstArray[i] != secondArray[i])
                    return false;
            }
            return true;
        }
    }
}
