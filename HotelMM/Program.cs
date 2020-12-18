using System;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using Spectre.Console;
using System.Threading;

namespace HotelMM
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            //Azure connection stringbuilder
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "hoteldatabase-marcus.database.windows.net";
            builder.UserID = "MarcusT";
            builder.Password = "qwerty1234!";
            builder.InitialCatalog = "HotelMMv2";
            //Local connectionstring
            //var cs = @"Server=localhost\SQLEXPRESS;Database=mackesHotel;Trusted_Connection=True;";

            bool appRunning = true;
            while (appRunning)
            {
                //Meny
                Console.Clear();
                var rule = new Rule("Hotel M&M Management system");
                var menuTable = new Table();
                menuTable.Border = TableBorder.Double;
                menuTable.Centered();
                menuTable.AddColumn("Option: ");
                menuTable.AddColumn("Task:");

                menuTable.AddRow("1", "Render list of guests");
                menuTable.AddRow("2", "Show unpaid invoices");
                menuTable.AddRow("3", "Show archived invoices");
                menuTable.AddRow("4", "Pay and archive an invoice");
                menuTable.AddRow("5", "Add a new invoice");
                menuTable.AddRow("6", "Edit invoice");
                menuTable.AddRow("7", "Exit application");
                AnsiConsole.Render(rule);
                Console.WriteLine();
                Console.WriteLine();
                AnsiConsole.Render(menuTable);

                int selection;
                Console.Write("Option: ");
                string input = Console.ReadLine();

                bool validMenuChoice = int.TryParse(input, out selection);
                switch (selection)
                {
                    case 1:
                        showGuestList(builder.ConnectionString);
                        break;
                    case 2:
                        showInvoices(builder.ConnectionString);
                        Console.Read();
                        break;
                    case 3:
                        showResolvedInvoices(builder.ConnectionString);
                        break;
                    case 4:
                        payInvoice(builder.ConnectionString);
                        break;
                    case 5:
                        addInvoice(builder.ConnectionString);
                        break;
                    case 6:
                        editInvoice(builder.ConnectionString);
                        break;
                    case 7:
                        Environment.Exit(0);
                        break;
                }

            }



        }
    static void showGuestList(string cs)
        {
            Console.Clear();
            // Create a table and colums
            var table = new Table();
            table.Border = TableBorder.Double;
            table.Centered();
            table.AddColumn("Guest ID: ");
            table.AddColumn("Guest name: ");
            table.AddColumn("Guest surname: ");

            //Sql connection
            using var con = new SqlConnection(cs);
            con.Open();
            var Persons = con.Query<Person>("SELECT * FROM Persons").ToList();
            Persons.ForEach(person => table.AddRow($"{person.PersonId}", $"{person.FirstName}", $"{person.Surname}"));
            //Heading
            var rule = new Rule("List of guests");
            AnsiConsole.Render(rule);
            Console.WriteLine();
            Console.WriteLine();
            AnsiConsole.Render(table);

            Console.Read();
        }
        static void showInvoices(string cs)
        {
            Console.Clear();
            // Create a table and colums
            var table = new Table();
            table.Border = TableBorder.Double;
            table.Centered();
            table.AddColumn("ID: ");
            table.AddColumn("Room ID: ");
            table.AddColumn("Type ID: ");
            table.AddColumn("Amount: ");

            using var con = new SqlConnection(cs);
            con.Open();
            var Invoices = con.Query<Invoice>("SELECT * FROM Invoices").ToList();
            var rule = new Rule("Unpaid Invoices");
            AnsiConsole.Render(rule);
            Console.WriteLine();
            Console.WriteLine();
            //Add rows in table and populate with the fetched data
            Invoices.ForEach(invoice => table.AddRow($"{invoice.InvoiceNumberId}", $"{invoice.RoomId}", $"{invoice.TypeId}", $"{invoice.Amount} USD"));
            AnsiConsole.Render(table);

            //Console.Read();
        }
        static void showResolvedInvoices(string cs)
        {
            Console.Clear();
            // Create a table and colums
            var table = new Table();
            table.Border = TableBorder.Double;
            table.Centered();
            table.AddColumn("ID: ");
            table.AddColumn("Original Invoice ID: ");
            table.AddColumn("Room ID: ");
            table.AddColumn("Type ID: ");
            table.AddColumn("Amount: ");
            //Sql query and con
            using var con = new SqlConnection(cs);
            con.Open();
            var ResolvedInvoices = con.Query<ResolvedInvoice>("SELECT * FROM ResolvedInvoices").ToList();
            //heading
            var rule = new Rule("Archived / paid Invoices");
            AnsiConsole.Render(rule);
            Console.WriteLine();
            Console.WriteLine();
            //Add rows in table and populate with the fetched data
            ResolvedInvoices.ForEach(invoice => table.AddRow($"{invoice.ResolvedInvoiceId}",$"{invoice.InvoiceNumberId}", $"{invoice.RoomId}", $"{invoice.TypeId}", $"{invoice.Amount} USD"));
            AnsiConsole.Render(table);
            Console.Read();
        }
        static void payInvoice(string cs) 
        {
            showInvoices(cs);
            int selection;
            Console.Write("Enter ID of the invoice you want to pay: ");
            string input = Console.ReadLine();
            bool validMenuChoice = int.TryParse(input, out selection);
            if(validMenuChoice)
            {
              try
                {
                    using var con = new SqlConnection(cs);
                    con.Open();
                    con.Execute("DELETE FROM Invoices WHERE InvoiceNumberId=" + selection);
                } catch(System.Data.SqlClient.SqlException)
                {
                    Console.WriteLine("Error, Database didnt accept the input");
                    Console.Read();
                }
                Console.WriteLine($"Invoice with ID: {selection} was resolved and archived..");
                Console.Read();
            }

        }
        static void addInvoice(string cs)
        {
            Console.Clear();
            var rule = new Rule("Add a new invoice");
            AnsiConsole.Render(rule);

            // Create a table and colums
            var table = new Table();
            table.Centered();
            table.AddColumn("Rooms");
            table.AddColumn("Invoice types");
            var roomTable = new Table();
            roomTable.Border = TableBorder.Minimal;
            roomTable.AddColumn("Room ID:s available for charges:");
            var invoiceTypeTable = new Table();
            invoiceTypeTable.Border = TableBorder.Minimal;
            invoiceTypeTable.AddColumn("Invoice Type ID:");
            invoiceTypeTable.AddColumn("Invoice Type:");
            using var con = new SqlConnection(cs);
            con.Open();
            var availableRooms = con.Query<Room>("SELECT * FROM Rooms").ToList();
            var availableInvoiceType = con.Query<InvoiceTypes>("SELECT * FROM InvoiceTypes").ToList();
            availableRooms.ForEach(room => roomTable.AddRow($"{room.RoomId}"));
            availableInvoiceType.ForEach(type => invoiceTypeTable.AddRow($"{type.InvoiceTypeId}", $"{type.InvoiceType}"));
            table.AddRow(roomTable, invoiceTypeTable);
            AnsiConsole.Render(table);
            Console.WriteLine();
            Console.WriteLine();
            bool validMenuChoice = false;
            Console.WriteLine("What room do you wanna charge?");
            string roomToCharge = Console.ReadLine();
            int roomToChargeInt;
            Console.WriteLine("What type of charge is it?");
            string chargeType = Console.ReadLine();
            int chargeTypeInt;
            Console.WriteLine("How much do you want to charge? USD");
            string amountToCharge = Console.ReadLine();
            int amountToChargeInt;
            validMenuChoice = int.TryParse(roomToCharge, out roomToChargeInt);
            validMenuChoice = int.TryParse(chargeType, out chargeTypeInt);
            validMenuChoice = int.TryParse(amountToCharge, out amountToChargeInt);
            if(validMenuChoice)
            {
                try
                {
                    con.Query($"INSERT INTO Invoices VALUES({roomToCharge}, {chargeType}, {amountToCharge})");
                }
                catch (System.Data.SqlClient.SqlException)
                {
                    Console.WriteLine("Sorry, the database did not accept your input. Please try again..");
                    Console.Read();
                    return;
                }
            }else
            {
                Console.WriteLine("Please enter numbers only..");
                Console.Read();
                return;
            }
            Console.WriteLine("New invoice successfully Added!");
            Console.Read();
        }
        static void editInvoice(string cs)
        {
            //Tables
            Console.Clear();
            var rule = new Rule("Edit an existing invoice");
            AnsiConsole.Render(rule);
            Console.WriteLine();
            Console.WriteLine();
            var table = new Table();
            table.Centered();
            table.AddColumn("Rooms");
            table.AddColumn("Invoice types");
            table.AddColumn("Existing invoices");
            var roomTable = new Table();
            roomTable.Border = TableBorder.Minimal;
            roomTable.AddColumn("Room ID:s available for charges:");
            var invoiceTypeTable = new Table();
            invoiceTypeTable.Border = TableBorder.Minimal;
            invoiceTypeTable.AddColumn("Invoice Type ID:");
            invoiceTypeTable.AddColumn("Invoice Type:");
            var invoiceTable = new Table();
            invoiceTable.Border = TableBorder.Minimal;
            invoiceTable.AddColumn("Invoice ID");
            invoiceTable.AddColumn("Room ID");
            invoiceTable.AddColumn("Type ID");
            invoiceTable.AddColumn("Amount");
            //Sql connection
            using var con = new SqlConnection(cs);
            con.Open();
            //Select queries
            var availableRooms = con.Query<Room>("SELECT * FROM Rooms").ToList();
            var availableInvoiceType = con.Query<InvoiceTypes>("SELECT * FROM InvoiceTypes").ToList();
            var availableInvoices = con.Query<Invoice>("SELECT * FROM Invoices").ToList();

            availableRooms.ForEach(room => roomTable.AddRow($"{room.RoomId}"));
            availableInvoiceType.ForEach(type => invoiceTypeTable.AddRow($"{type.InvoiceTypeId}", $"{type.InvoiceType}"));
            availableInvoices.ForEach(invoice => invoiceTable.AddRow($"{invoice.InvoiceNumberId}", $"{invoice.RoomId}", $"{invoice.TypeId}", $"{invoice.Amount} USD"));

            table.AddRow(invoiceTable, roomTable, invoiceTypeTable);
            AnsiConsole.Render(table);

            bool validMenuChoice = false;
            Console.WriteLine("What invoice do you want to edit?");
            string invoiceToEdit = Console.ReadLine();
            int invoiceToEditInt;
            Console.WriteLine("What room should be charged?");
            string roomToCharge = Console.ReadLine();
            int roomToChargeInt;
            Console.WriteLine("What type of charge is it?");
            string invoiceType = Console.ReadLine();
            int invoiceTypeInt;
            Console.WriteLine("How much is the charge?");
            string amountToCharge = Console.ReadLine();
            int amountToChargeInt;
            validMenuChoice = int.TryParse(invoiceToEdit, out invoiceToEditInt);
            validMenuChoice = int.TryParse(roomToCharge, out roomToChargeInt);
            validMenuChoice = int.TryParse(invoiceType, out invoiceTypeInt);
            validMenuChoice = int.TryParse(amountToCharge, out amountToChargeInt);
            if(validMenuChoice)
            {
                try
                {
                    con.Query($"UPDATE Invoices SET RoomId = {roomToCharge}, TypeId = {invoiceType}, Amount = {amountToCharge} WHERE InvoiceNumberId = {invoiceToEdit}");
                }catch(System.Data.SqlClient.SqlException)
                {
                    Console.WriteLine("Sorry, the database did not accept your input. Please try again..");
                    Console.Read();
                }
            }
            else
            {
                Console.WriteLine("Please enter numbers only..");
                Console.Read();
                return;
            }
        }   
    }
}

