using System.Diagnostics;   // <<< ADDED >>> for Stopwatch

namespace RTAConsoleApp
{
    // Represents a Task
    public class Task
    {
        public string TaskName { get; set; }
        public int WCET { get; set; }
        public int BCET { get; set; } // Not directly used by RTA, but included for the full model
        public int Period { get; set; }
        public int Deadline { get; set; } // Equal to Period in your examples
        public int Priority { get; set; } // Lower number = higher priority
        public int WCRT { get; set; } = 0; // Calculated worst-case response time

        public override string ToString()
        {
            return $"Task: {TaskName}, WCET: {WCET}, Period: {Period}, Priority: {Priority},Deadline: {Deadline}, WCRT: {WCRT}";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // <<< ADDED >>> Start total program timer
            Stopwatch totalProgramStopwatch = Stopwatch.StartNew(); 

            if (args.Length != 1)
            {
                Console.WriteLine("Usage: RTA <csv_file_path>");
                return;
            }

            // 1. Read Task Data from CSV

            string csvFilePath = args[0];

            List<Task> tasks = ReadTasksFromCsv(csvFilePath);

            //List<Task> tasks = ReadTasksFromCsv("exercise-TC4.csv"); // Replace with your CSV file name
           

            if (tasks == null || tasks.Count == 0)
            {
                Console.WriteLine("No tasks loaded. Check your CSV file.");
                return;
            }

            // <<< ADDED >>> Start RTA calculation timer
            Stopwatch rtaStopwatch = Stopwatch.StartNew();

            // 2. Run Response-Time Analysis (RTA)
            bool schedulable = RTA(tasks);

            // <<< ADDED >>> Stop RTA calculation timer
            rtaStopwatch.Stop();

            // 3. Print Results
            if (schedulable)
                Console.WriteLine("SCHEDULABLE");
            else      
                Console.WriteLine("UNSCHEDULABLE");


            foreach (var task in tasks)
            {
                Console.Write(task);
                if (task.WCRT > task.Deadline)
                {
                    Console.Write(" - UNSCHEDULABLE TASK");
                }
                Console.WriteLine();
            }

            Console.WriteLine($"\nRTA Calculation Time: {rtaStopwatch.Elapsed.TotalMilliseconds:F4} ms");

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

        }



        // Reads tasks from the CSV file
        static List<Task> ReadTasksFromCsv(string filePath)
        {
            List<Task> tasks = new List<Task>();

            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    // Skip the header row
                    reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');

                        if (values.Length != 6) // Check if line has enough values
                        {
                            Console.WriteLine($"Skipping invalid line: {line}");
                            continue;
                        }

                        try
                        {
                            var task = new Task
                            {
                                TaskName = values[0],
                                BCET = int.Parse(values[1]),
                                WCET = int.Parse(values[2]),
                                Period = int.Parse(values[3]),
                                Deadline = int.Parse(values[4]),
                                Priority = int.Parse(values[5])
                            };
                            tasks.Add(task);
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine($"Skipping invalid line: {line} (Format error)");
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error: File not found: {filePath}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading the file: {ex.Message}");
                return null;
            }

            return tasks;
        }

        // Response-Time Analysis (RTA) Algorithm
        static bool RTA(List<Task> tasks)
        {
            // 1. Sort tasks by priority (highest priority first) - IMPORTANT!
            tasks = tasks.OrderBy(t => t.Priority).ToList(); // Assumes lower priority number = higher priority
            int schedulable_taskset = 1;

            foreach (var task_i in tasks)
            {
                int I = 0; // Interference
                int R_old;
                int R_new = task_i.WCET; // Initial response time guess

                while (true)
                {
                    R_old = R_new;
                    int interference_sum = 0;

                    // Calculate Interference from Higher Priority Tasks
                    for (int j = 0; j < tasks.IndexOf(task_i); j++)  // Iterate over tasks with higher priority
                    {
                        Task task_j = tasks[j];
                        interference_sum += (int)Math.Ceiling((double)R_new / task_j.Period) * task_j.WCET;
                    }
                    I = interference_sum;
                    R_new = task_i.WCET + I;

                    if (Math.Abs(R_new - R_old) < 0.0001) // Convergence check (using a small epsilon, it's the same as R_new == R_old since WCET, Period, Deadline are integers)
                    {
                        break; // Response time converged
                    }
                }

                task_i.WCRT = R_new; // Store the final WCRT.

                if(task_i.WCRT > task_i.Deadline) //Decide if the task (and through this, the whole taskset) is schedulable or not
                {
                    schedulable_taskset = 0;
                }
            }

            if (schedulable_taskset == 1)
            {
                return true; // All tasks are schedulable 
            }
            return false; // At least one task is unschedulable
        }
    }
}