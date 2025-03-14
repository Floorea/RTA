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
            return $"Task: {TaskName}, WCET: {WCET}, Period: {Period}, Deadline: {Deadline}, Priority: {Priority}, WCRT: {WCRT}";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // 1. Read Task Data from CSV
            List<Task> tasks = ReadTasksFromCsv("exercise-TC3.csv"); // Replace with your CSV file name

            if (tasks == null || tasks.Count == 0)
            {
                Console.WriteLine("No tasks loaded. Check your CSV file.");
                return;
            }

            // 2. Run Response-Time Analysis (RTA)
            bool schedulable = RTA(tasks);

            // 3. Print Results
            if (schedulable)
            {
                Console.WriteLine("SCHEDULABLE");
                foreach (var task in tasks)
                {
                    Console.WriteLine(task);
                }
            }
            else
            {
                Console.WriteLine("UNSCHEDULABLE");
            }

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
                                WCET = int.Parse(values[1]),
                                BCET = int.Parse(values[2]),
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

                    if (R_new > task_i.Deadline)
                    {
                        task_i.WCRT = R_new; // for debugging
                        return false; // Task is not schedulable
                    }

                    if (Math.Abs(R_new - R_old) < 0.0001) // Convergence check (using a small epsilon, it's the same as R_new == R_old since WCET, Period, Deadline are integers)
                    {
                        break; // Response time converged
                    }
                }
                task_i.WCRT = R_new; // Store the final WCRT.
            }

            return true; // All tasks are schedulable
        }
    }
}