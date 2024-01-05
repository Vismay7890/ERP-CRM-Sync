using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using LogLibrary;

namespace Salesforce
{
    class Program
    {
        private static SalesforceClient CreateClient(Logger logger)
        {

            return new SalesforceClient
            {
                Logger = logger,
                Username = ConfigurationManager.AppSettings["username"],
                Password = ConfigurationManager.AppSettings["password"],
                Token = ConfigurationManager.AppSettings["token"],
                ClientId = ConfigurationManager.AppSettings["clientId"],
                ClientSecret = ConfigurationManager.AppSettings["clientSecret"]
            };
        }

        static void Main()
        {
            Logger logger = new Logger();
            Logger.Log("Application Started");
            try { 
            var client = CreateClient(logger);
            client.login();

            while (true)
            {
                Console.WriteLine("Choose an option:");
                Console.WriteLine("1. Get Data");
                Console.WriteLine("2. Insert Record");
                Console.WriteLine("3. Upate or Insert Record");
                Console.WriteLine("4. Exit");

                Console.Write("Enter your choice (1-4): ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        // Get Data
                        string queryResult = client.Query("SELECT Id, Course_Name__c, Name, Student_Id__c FROM Student__c ORDER BY Id");
                        SalesforceClient.PrintJsonTable(queryResult);
                        break;

                    case "2":
                        // Insert Record
                        InsertRecord(client);
                        break;

                    case "3":
                        // Update Record
                        UpsertRecord(client);
                        break;
                    case "4":
                        // Exit the program
                        Environment.Exit(0);
                        break;

                    default:
                        Console.WriteLine("Invalid choice. Please enter a valid option.");
                        break;
                }
            }
            }
            catch (Exception ex) 
            {
                Logger.Log($"Error in the main loop: {ex.Message}");

            }
            finally
            {
                Logger.Log("Application Exiting");
            }
        }

        private static void InsertRecord(SalesforceClient client)
        {
            Logger.Log("Inserting a new record");

            Console.WriteLine("Inserting a new record:");
            try { 
            // Get user input for record data
            Console.Write("Enter Student Name: ");
            string studentName = Console.ReadLine();
            Console.Write("Enter Course Name: ");
            string courseName = Console.ReadLine();

            string insertRecordData = $@"{{
        ""Name"": ""{studentName}"",
        ""Course_Name__c"": ""{courseName}""
    }}";

            Console.WriteLine("Insert Result: " + client.Insert("Student__c", insertRecordData));
            }
            catch (Exception ex) 
            {
                Logger.Log($"Error during insert: {ex.Message}");
            }
        
            }



        private static void UpsertRecord(SalesforceClient client)
        {
            Logger.Log("Upserting a record");

            Console.WriteLine("Upserting a record:");
            try { 
            Console.Write("Enter External ID Value (Student_Id__c): ");
            string externalId = Console.ReadLine();
            Console.Write("Enter Student Name: ");
            string upsertedStudentName = Console.ReadLine();
            Console.Write("Enter Course Name: ");
            string upsertedCourseName = Console.ReadLine();

            string queryResult = client.Query($"SELECT Id FROM Student__c WHERE Student_Id__c = '{externalId}'");
            Console.WriteLine("Query results:",queryResult);
            if (RecordExists(queryResult))
            {
                string upsertRecordData = $@"{{
        ""Name"": ""{upsertedStudentName}"",
        ""Course_Name__c"": ""{upsertedCourseName}""
       }}";
                Console.WriteLine("Record with the provided External ID exists. Performing an update.");
                Console.WriteLine("Update Result: " + client.Update("Student__c", externalId, upsertRecordData));
            }
            else
            {
                Console.WriteLine("Record with the provided External ID does not exist. Performing an insert.");
                string upsertRecordData = $@"{{
            ""Name"": ""{upsertedStudentName}"",
            ""Course_Name__c"": ""{upsertedCourseName}""
        }}";

                Console.WriteLine("Insert Result: " + client.Insert("Student__c", upsertRecordData));
            }
            }
            catch (Exception ex) 
            {
                Logger.Log($"Error during upsert: {ex.Message}");
            }
        }
        private static  bool RecordExists(string queryResult)
        {
            try
            {

                dynamic resultObject = JsonConvert.DeserializeObject(queryResult);

                if (resultObject.records != null && resultObject.records.Count > 0)
                {
                    return true; 
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error parsing query result: {ex.Message}");
                Console.WriteLine("Error parsing query result: " + ex.Message);
                return false; 
            }
        }

    }
}
