using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WorkFromHomeTracker1._0
{
    class EmployeeRecord
    {
        public int WeekNumber { get; set; }
        public string EmployeeID { get; set; }
        public string Name { get; set; }
        public int[] DailyHours { get; set; } = new int[5];

        public int WeeklyTotal => DailyHours.Sum();

        // CSV line format: Week,EmployeeID,Name,Mon,Tue,Wed,Thu,Fri
        public override string ToString()
        {
            // Ensure no commas in Name or ID; if needed, handle quoting/escaping.
            return $"{WeekNumber},{EmployeeID},{Name},{string.Join(",", DailyHours)}";
        }
    }

    class Program
    {
        // Stores all employee records for the session (and persisted to file)
        static List<EmployeeRecord> allEmployeeRecords = new List<EmployeeRecord>();

        // TODO: adjust this path as needed; ensure the program has write permissions
        static string filePath = @"D:\Certificate III Information Technology\ICTPRG302 Apply introductory programming techniques\ICTPRG302 AT2\WorkFromHomeTracker\WorkFromHomeTracker1.0\WorkFromHomeTracker1.0\EmployeeRecords.csv";

        static void Main(string[] args)
        {
            // On start, attempt to load existing records into memory
            LoadRecordsFromFile();

            while (true)
            {
                Console.Clear();
                // Title centered
                CenterText("~~Diamond Realty~~");
                CenterText("Work From Home Tracker");
                Console.WriteLine(); // blank line

                // Menu options (centered)
                
                CenterText("Press a key from 1 to 3 to select an option");
                Console.WriteLine();
                CenterText("1. Enter Daily Hours Worked");
                CenterText("2. Produce Hours Worked Report");
                CenterText("3. Exit");
                Console.WriteLine();

                // Prompt for choice, centered
                string choice = CenteredReadLine("Select an option: ");
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        EnterDailyHours();
                        break;
                    case "2":
                        ProduceReport();
                        break;
                    case "3":
                        return; // exit Main
                    default:
                        CenterText("Invalid choice. Press Enter to continue.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        /// <summary>
        /// Centers a line of text based on current console window width.
        /// </summary>
        static void CenterText(string text)
        {
            try
            {
                int windowWidth = Console.WindowWidth;
                int padding = Math.Max((windowWidth - text.Length) / 2, 0);
                Console.WriteLine(new string(' ', padding) + text);
            }
            catch
            {
                // If Console.WindowWidth not available or error, just write normally
                Console.WriteLine(text);
            }
        }

        /// <summary>
        /// Writes the prompt centered, sets cursor there, then reads input.
        /// </summary>
        static string CenteredReadLine(string prompt)
        {
            try
            {
                int windowWidth = Console.WindowWidth;
                int promptLength = prompt.Length;
                int startLeft = Math.Max((windowWidth - promptLength) / 2, 0);
                int origTop = Console.CursorTop;
                Console.SetCursorPosition(startLeft, origTop);
                Console.Write(prompt);
                return Console.ReadLine() ?? "";
            }
            catch
            {
                // fallback if SetCursorPosition fails
                Console.Write(prompt);
                return Console.ReadLine() ?? "";
            }
        }

        static void EnterDailyHours()
        {
            // Prompt for week number
            string weekInput = CenteredReadLine("Enter current Working Week Number: ");
            if (!int.TryParse(weekInput, out int weekNumber))
            {
                CenterText("Invalid week number. Press Enter to return to menu.");
                Console.ReadLine();
                return;
            }

            // Optionally: if you want to avoid duplicate entries for the same week in-memory,
            // you could remove existing entries with this week number:
            // allEmployeeRecords.RemoveAll(r => r.WeekNumber == weekNumber);

            for (int i = 0; i < 7; i++)
            {
                Console.WriteLine();
                CenterText($"[Employee {i + 1}]");

                Console.Write("Enter Employee ID: ");
                string id = Console.ReadLine() ?? "";

                Console.Write("Enter Employee Name: ");
                string name = Console.ReadLine() ?? "";

                int[] dailyHours = new int[5];
                string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

                int weeklyTotal = 0;
                for (int j = 0; j < 5; j++)
                {
                    Console.Write($"Enter Hours Worked for {days[j]}: ");
                    string hrsInput = Console.ReadLine();
                    if (!int.TryParse(hrsInput, out int hrs))
                    {
                        Console.WriteLine("  Invalid number; treating as 0 hours.");
                        hrs = 0;
                    }
                    dailyHours[j] = hrs;

                    if (hrs < 4)
                        Console.WriteLine($"  Insufficient hours worked on {days[j]}");
                    else if (hrs > 10)
                        Console.WriteLine($"  Too many hours worked on {days[j]}");

                    weeklyTotal += hrs;
                }

                Console.WriteLine($"Total Hours worked for Week {weekNumber}: {weeklyTotal} hours");
                if (weeklyTotal < 30)
                    Console.WriteLine("You didn't do enough work this week");
                else if (weeklyTotal > 40)
                    Console.WriteLine("You are working too hard!!");

                var record = new EmployeeRecord
                {
                    WeekNumber = weekNumber,
                    EmployeeID = id,
                    Name = name,
                    DailyHours = dailyHours
                };
                allEmployeeRecords.Add(record);

                Console.WriteLine(); // spacing between entries
            }

            // After collecting entries, save entire list to file (overwrite)
            SaveRecordsToFile();

            // Show summary for this session
            WeeklyEmployeeReport();

            CenterText("\nPress Enter to return to the Main Menu...");
            Console.ReadLine();
        }

        static void SaveRecordsToFile()
        {
            try
            {
                var lines = new List<string>
                {
                    "Week,EmployeeID,Name,Monday,Tuesday,Wednesday,Thursday,Friday"
                };
                foreach (var r in allEmployeeRecords)
                {
                    lines.Add(r.ToString());
                }
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving records to file: {ex.Message}");
            }
        }

        static void LoadRecordsFromFile()
        {
            allEmployeeRecords.Clear();

            if (!File.Exists(filePath))
                return;

            string[] lines;
            try
            {
                lines = File.ReadAllLines(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return;
            }

            if (lines.Length <= 1)
                return; // no data rows

            // Skip first line (header). Start from index 1.
            for (int idx = 1; idx < lines.Length; idx++)
            {
                string line = lines[idx].Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                var parts = line.Split(',');
                if (parts.Length < 8)
                    continue; // malformed

                // Parse WeekNumber
                if (!int.TryParse(parts[0], out int weekNumber))
                    continue; // skip header-like or invalid lines

                string empId = parts[1];
                string name = parts[2];

                int[] dailyHours = new int[5];
                bool parseError = false;
                for (int j = 0; j < 5; j++)
                {
                    if (!int.TryParse(parts[3 + j], out int hrs))
                    {
                        parseError = true;
                        break;
                    }
                    dailyHours[j] = hrs;
                }
                if (parseError)
                    continue;

                var record = new EmployeeRecord
                {
                    WeekNumber = weekNumber,
                    EmployeeID = empId,
                    Name = name,
                    DailyHours = dailyHours
                };
                allEmployeeRecords.Add(record);
            }
        }

        static void WeeklyEmployeeReport()
        {
            if (allEmployeeRecords.Count == 0)
            {
                CenterText("No records to summarize.");
                return;
            }

            var weeklyTotals = allEmployeeRecords.Select(r => r.WeeklyTotal).ToList();

            int lessThan30 = weeklyTotals.Count(h => h < 30);
            int moreThan40 = weeklyTotals.Count(h => h > 40);
            int between37And39 = weeklyTotals.Count(h => h >= 37 && h <= 39);

            CenterText("******** Weekly Employee Report ********");
            CenterText($"Employees who worked less than 30 hours: {lessThan30}");
            CenterText($"Employees who worked more than 40 hours: {moreThan40}");
            CenterText($"Employees who worked between 37 and 39 hours: {between37And39}");
        }

        static void ProduceReport()
        {
            LoadRecordsFromFile();

            if (allEmployeeRecords.Count == 0)
            {
                CenterText("No records found.");
                Console.ReadLine();
                return;
            }

            string input = CenteredReadLine("How many recent records would you like to see? ");
            if (!int.TryParse(input, out int numRecords) || numRecords <= 0)
            {
                CenterText("Invalid number. Press Enter to return.");
                Console.ReadLine();
                return;
            }

            var recentRecords = allEmployeeRecords
                .OrderByDescending(r => r.WeekNumber)
                .Take(numRecords);

            CenterText("******** Latest Employee Records ********");
            foreach (var record in recentRecords)
            {
                // e.g.: Week 12, E123, John Doe, 8, 8, 8, 8, 8
                string line = $"Week {record.WeekNumber}, {record.EmployeeID}, {record.Name}, {string.Join(", ", record.DailyHours)}";
                CenterText(line);
            }

            CenterText("\nPress Enter to return to the Main Menu...");
            Console.ReadLine();
        }
    }
}
