using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AccountCreator
{
    class Program
    {
        public static DataTable csvToDataTable(string filePath, bool isRowOneHeader)
        {
            DataTable csvDataTable = new DataTable();
            String[] csvData = File.ReadAllLines(filePath);
            if (csvData.Length == 0)
            {
                //throw new Exception("CSV File Appears to be Empty");
            }
            String[] headings = csvData[0].Replace("\"", "").Split(',');
            int index = 0;
            if (isRowOneHeader)
            {
                index = 1;
                for (int i = 0; i <= headings.Length - 1; i++)
                {
                    headings[i] = headings[i].Replace(" ", "_").Replace("\"", "");
                    csvDataTable.Columns.Add(headings[i], typeof(string));
                }
            }
            else
            {
                for (int i = 0; i <= headings.Length - 1; i++)
                {
                    csvDataTable.Columns.Add("col" + (i + 1).ToString(), typeof(string));
                }
            }

            try
            {

                for (int i = index; i <= csvData.Length - 1; i++)
                {
                    if (csvData[i].Contains(",\""))
                    {
                        if (csvData[i].Contains("\","))
                        {
                            String actualText = "", replaceText = "";

                            actualText = csvData[i].Substring(csvData[i].IndexOf(",\"") + 1, csvData[i].IndexOf("\",") - csvData[i].IndexOf(",\""));

                            replaceText = csvData[i].Substring(csvData[i].IndexOf(",\"") + 1, csvData[i].IndexOf("\",") - csvData[i].IndexOf(",\"")).Replace(',', ';');

                            csvData[i] = csvData[i].Replace(actualText, replaceText);
                        }
                    }
                    DataRow row = csvDataTable.NewRow();
                    for (int j = 0; j <= headings.Length - 1; j++)
                    {
                        row[j] = csvData[i].Replace("\"", "").Split(',')[j].Replace("\"", "");
                    }
                    csvDataTable.Rows.Add(row);
                }
            }
            catch (Exception)
            {
                //  MessageBox.Show("Error in Text File");
            }
            return csvDataTable;
        }
        static void Main(string[] args)
        {
           string LogPath= System.IO.Directory.GetCurrentDirectory() + @"\Logs\log.txt";
           if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }
            string Datapath = System.IO.Directory.GetCurrentDirectory() + @"\data.txt";
            
            CookieContainer cookieContainer = new CookieContainer();
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            DataRow proxy = null;
            string refrer;
            WebHeaderCollection header = new WebHeaderCollection();

            HttpHandler httphandle = new HttpHandler();

            string clientId = "";
            string clientSecret = "";
            string asxIdRandomId = "";
            string asxSecret = "";
            string body = "";
            DataTable Credentials = new DataTable();
            StreamWriter writer = new StreamWriter(LogPath);

            string result = httphandle.Get("https://www.axs.com/", cookieContainer, out response, out request, "");
            if (result.Contains("NAME=\"ROBOTS\""))
            {
                result = httphandle.GetFirstPageHTML(cookieContainer, proxy, result);

                result = httphandle.Get("https://www.axs.com/", cookieContainer, out response, out request, "");
            }

            try
            {
               clientId = httphandle
                   .GetStringInBetween("clientId = \"", "\";", result, false, false)[0].Trim();
                clientSecret = httphandle.GetStringInBetween("axsClientId = \"", "\";", result, false, false)[0].Trim();
                asxIdRandomId = httphandle.GetStringInBetween("axsIdRandomId = \"", "\";", result, false, false)[0].Trim();
                asxSecret = httphandle.GetStringInBetween("axsIdSecret = \"", "\";", result, false, false)[0].Trim();
                body = "";

                // result = httphandle.Post("https://api.axs.com/proxy/v2/flash/migrate/create?access_token=4f2be33d835e7197e245c54ff00e5fb4&locale=en-US&region=1", new NameValueCollection(), cookieContainer, body, out response, out request, "", proxy, "", "application/x-www-form-urlencoded", false, "", "", header);
                //string result2 = httphandle.createAcc(cookieContainer, body, response, request);

                
                Credentials = csvToDataTable(Datapath, true);

                
                writer.WriteLine("email,password,status,message");

            }
            catch (Exception)
            {
                
                
            }
            
            //string result2="";
            string message = "";
            string status = "";
            Console.Write("First Name\t");
            Console.Write("Last Name\t");
            Console.Write("Email Address\t");
            Console.Write("Password\t");
            Console.Write("Status\t");
            Console.Write("Message");
            Console.WriteLine();
            string first_name = "";
            string last_name = "";
            for (int i = 0; i < Credentials.Rows.Count; i++)
            {

                try
                {
                   
                    first_name = GenRandomFirstName();
                    last_name = GenRandomLastName();
                    body = @"{""email"":""" + Credentials.Rows[i]["email"].ToString() + @""",""name"":{""first"":""" + first_name + @""",""last"":""" + last_name + @"""},""password"":""" + Credentials.Rows[i]["password"].ToString() + @""",""marketing_consent"":true,""privacy_policy"":""https://www.axs.com/about-privacy-policy_US_v2.html"",""terms_conditions"":""https://www.axs.com/about-terms-of-use_US_v1.html"",""type"":""axsid"",""axsClientId"":""1"",""client_id"":""" + clientId + "_" + asxIdRandomId + @""",""client_secret"":""" + asxSecret + @""",""autoCreateFlashAccount"":""1""}";
                    // string result2 = httphandle.createAcc(cookieContainer, body, response, request,  header);

                    if (httphandle.Request_api_axs_com_post(out response, cookieContainer, body))
                    {
                        StreamReader streamReader = new System.IO.StreamReader(response.GetResponseStream());
                        result = streamReader.ReadToEnd().Trim();
                        response.Close();
                    }
                    if (result.Contains("\"userId\":"))
                    {
                        status = "created";
                        message = "Account has been created";
                    }
                    else {
                        status = "failed";
                        message = httphandle.GetStringInBetween("\"message\":\"", "\"}", result, false, false)[0].Trim();
 
                    }
                   // message = httphandle.Post("https://api.axs.com/proxy/v2/flash/migrate/create?access_token=4f2be33d835e7197e245c54ff00e5fb4&locale=en-US&region=1", new NameValueCollection(), cookieContainer, body, out response, out request, "", proxy, "", "application/x-www-form-urlencoded", false, "", "", header);
                   
                   

                }
                catch (Exception)
                {
                     
                    
                }
                Console.Write(first_name+"\t");
                Console.Write(last_name + "\t");
                Console.Write(Credentials.Rows[i]["email"] + "\t");
                Console.Write(Credentials.Rows[i]["password"] + "\t");
                Console.Write(status+"\t");
                Console.Write(message+"\n");
                Console.WriteLine();
                writer.WriteLine(Credentials.Rows[i]["email"] + "," + Credentials.Rows[i]["password"] + "," + status + "," + message);
            }
            writer.Close();




        }
        public static string GenRandomLastName()
        {
            Random rand = new Random();
            List<string> last = new List<string>();
            string str = string.Empty;
            last.Add("Smith");
            last.Add("Johnson");
            last.Add("Williams");
            last.Add("Jones");
            last.Add("Brown");
            last.Add("Davis");
            last.Add("Miller");
            last.Add("Wilson");
            last.Add("Moore");
            last.Add("Taylor");
            last.Add("Anderson");
            last.Add("Thomas");
            last.Add("Jackson");
            last.Add("White");
            last.Add("Harris");
            last.Add("Martin");
            last.Add("Thompson");
            last.Add("Garcia");
            last.Add("Martinez");
            last.Add("Robinson");
            last.Add("Clark");
            last.Add("Rodriguez");
            last.Add("Lewis");
            last.Add("Lee");
            last.Add("Walker");
            last.Add("Hall");
            last.Add("Allen");
            last.Add("Young");
            last.Add("Hernandez");
            last.Add("King");
            last.Add("Wright");
            last.Add("Lopez");
            last.Add("Hill");
            last.Add("Scott");
            last.Add("Green");
            last.Add("Adams");
            last.Add("Baker");
            last.Add("Gonzalez");
            last.Add("Nelson");
            last.Add("Carter");
            last.Add("Mitchell");
            last.Add("Perez");
            last.Add("Roberts");
            last.Add("Turner");
            last.Add("Phillips");
            last.Add("Campbell");
            last.Add("Parker");
            last.Add("Evans");
            last.Add("Edwards");
            last.Add("Collins");
            last.Add("Stewart");
            last.Add("Sanchez");
            last.Add("Morris");
            last.Add("Rogers");
            last.Add("Reed");
            last.Add("Cook");
            last.Add("Morgan");
            last.Add("Bell");
            last.Add("Murphy");
            last.Add("Bailey");
            last.Add("Rivera");
            last.Add("Cooper");
            last.Add("Richardson");
            last.Add("Cox");
            last.Add("Howard");
            last.Add("Ward");
            last.Add("Torres");
            last.Add("Peterson");
            last.Add("Gray");
            last.Add("Ramirez");
            last.Add("James");
            last.Add("Watson");
            last.Add("Brooks");
            last.Add("Kelly");
            last.Add("Sanders");
            last.Add("Price");
            last.Add("Bennett");
            last.Add("Wood");
            last.Add("Barnes");
            last.Add("Ross");
            last.Add("Henderson");
            last.Add("Coleman");
            last.Add("Jenkins");
            last.Add("Perry");
            last.Add("Powell");
            last.Add("Long");
            last.Add("Patterson");
            last.Add("Hughes");
            last.Add("Flores");
            last.Add("Washington");
            last.Add("Butler");
            last.Add("Simmons");
            last.Add("Foster");
            last.Add("Gonzales");
            last.Add("Bryant");
            last.Add("Alexander");
            last.Add("Russell");
            last.Add("Griffin");
            last.Add("Diaz");
            last.Add("Hayes");

            return last[rand.Next(0, last.Count)];


        }
        public static string GenRandomFirstName()
        {
            Random rand = new Random();
            List<string> first = new List<string>();
            string str = string.Empty;
            first.Add("Aiden");
            first.Add("Jackson");
            first.Add("Mason");
            first.Add("Liam");
            first.Add("Jacob");
            first.Add("Jayden");
            first.Add("Ethan");
            first.Add("Noah");
            first.Add("Lucas");
            first.Add("Logan");
            first.Add("Caleb");
            first.Add("Caden");
            first.Add("Jack");
            first.Add("Ryan");
            first.Add("Connor");
            first.Add("Michael");
            first.Add("Elijah");
            first.Add("Brayden");
            first.Add("Benjamin");
            first.Add("Nicholas");
            first.Add("Alexander");
            first.Add("William");
            first.Add("Matthew");
            first.Add("James");
            first.Add("Landon");
            first.Add("Nathan");
            first.Add("Dylan");
            first.Add("Evan");
            first.Add("Luke");
            first.Add("Andrew");
            first.Add("Gabriel");
            first.Add("Gavin");
            first.Add("Joshua");
            first.Add("Owen");
            first.Add("Daniel");
            first.Add("Carter");
            first.Add("Tyler");
            first.Add("Cameron");
            first.Add("Christian");
            first.Add("Wyatt");
            first.Add("Henry");
            first.Add("Eli");
            first.Add("Joseph");
            first.Add("Max");
            first.Add("Isaac");
            first.Add("Samuel");
            first.Add("Anthony");
            first.Add("Grayson");
            first.Add("Zachary");
            first.Add("David");
            first.Add("Christopher");
            first.Add("John");
            first.Add("Isaiah");
            first.Add("Levi");
            first.Add("Jonathan");
            first.Add("Oliver");
            first.Add("Chase");
            first.Add("Cooper");
            first.Add("Tristan");
            first.Add("Colton");
            first.Add("Austin");
            first.Add("Colin");
            first.Add("Charlie");
            first.Add("Dominic");
            first.Add("Parker");
            first.Add("Hunter");
            first.Add("Thomas");
            first.Add("Alex");
            first.Add("Ian");
            first.Add("Jordan");
            first.Add("Cole");
            first.Add("Julian");
            first.Add("Aaron");
            first.Add("Carson");
            first.Add("Miles");
            first.Add("Blake");
            first.Add("Brody");
            first.Add("Adam");
            first.Add("Sebastian");
            first.Add("Adrian");
            first.Add("Nolan");
            first.Add("Sean");
            first.Add("Riley");
            first.Add("Bentley");
            first.Add("Xavier");
            first.Add("Hayden");
            first.Add("Jeremiah");
            first.Add("Jason");
            first.Add("Jake");
            first.Add("Asher");
            first.Add("Micah");
            first.Add("Jace");
            first.Add("Brandon");
            first.Add("Josiah");
            first.Add("Hudson");
            first.Add("Nathaniel");
            first.Add("Bryson");
            first.Add("Ryder");
            first.Add("Justin");
            first.Add("Bryce");
            first.Add("Sophia");
            first.Add("Emma");
            first.Add("Isabella");
            first.Add("Olivia");
            first.Add("Ava");
            first.Add("Lily");
            first.Add("Chloe");
            first.Add("Madison");
            first.Add("Emily");
            first.Add("Abigail");
            first.Add("Addison");
            first.Add("Mia");
            first.Add("Madelyn");
            first.Add("Ella");
            first.Add("Hailey");
            first.Add("Kaylee");
            first.Add("Avery");
            first.Add("Kaitlyn");
            first.Add("Riley");
            first.Add("Aubrey");
            first.Add("Brooklyn");
            first.Add("Peyton");
            first.Add("Layla");
            first.Add("Hannah");
            first.Add("Charlotte");
            first.Add("Bella");
            first.Add("Natalie");
            first.Add("Sarah");
            first.Add("Grace");
            first.Add("Amelia");
            first.Add("Kylie");
            first.Add("Arianna");
            first.Add("Anna");
            first.Add("Elizabeth");
            first.Add("Sophie");
            first.Add("Claire");
            first.Add("Lila");
            first.Add("Aaliyah");
            first.Add("Gabriella");
            first.Add("Elise");
            first.Add("Lillian");
            first.Add("Samantha");
            first.Add("Makayla");
            first.Add("Audrey");
            first.Add("Alyssa");
            first.Add("Ellie");
            first.Add("Alexis");
            first.Add("Isabelle");
            first.Add("Savannah");
            first.Add("Evelyn");
            first.Add("Leah");
            first.Add("Keira");
            first.Add("Allison");
            first.Add("Maya");
            first.Add("Lucy");
            first.Add("Sydney");
            first.Add("Taylor");
            first.Add("Molly");
            first.Add("Lauren");
            first.Add("Harper");
            first.Add("Scarlett");
            first.Add("Brianna");
            first.Add("Victoria");
            first.Add("Liliana");
            first.Add("Aria");
            first.Add("Kayla");
            first.Add("Annabelle");
            first.Add("Gianna");
            first.Add("Kennedy");
            first.Add("Stella");
            first.Add("Reagan");
            first.Add("Julia");
            first.Add("Bailey");
            first.Add("Alexandra");
            first.Add("Jordyn");
            first.Add("Nora");
            first.Add("Carolin");
            first.Add("Mackenzie");
            first.Add("Jasmine");
            first.Add("Jocelyn");
            first.Add("Kendall");
            first.Add("Morgan");
            first.Add("Nevaeh");
            first.Add("Maria");
            first.Add("Eva");
            first.Add("Juliana");
            first.Add("Abby");
            first.Add("Alexa");
            first.Add("Summer");
            first.Add("Brooke");
            first.Add("Penelope");
            first.Add("Violet");
            first.Add("Kate");
            first.Add("Hadley");
            first.Add("Ashlyn");
            first.Add("Sadie");
            first.Add("Paige");
            first.Add("Katherine");
            first.Add("Sienna");
            first.Add("Piper");


            return first[rand.Next(0, first.Count)];
            //str = first.OrderBy(xx => rnd.Next()).First();
            // return str;
        }
    }
}