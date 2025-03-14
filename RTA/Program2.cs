/*using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Task
{
    public string Name { get; set; }
    public int WCET { get; set; }
    public int BCET { get; set; } //  Included, even though RTA doesn't use it directly
    public int Period { get; set; }
    public int Deadline { get; set; }
    public int Priority { get; set; }
    public int WCRT { get; set; } //  To store the calculated WCRT
}

public class RTAScheduler
{
    public static List<Task> ReadTasksFromCsv(string filePath)
    {
        List<Task> tasks = new List<Task>();
        try
        {
            using (var reader = new StreamReader(filePath))
            {
                // Skip the header line
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if (values.Length != 6)
                    {
                        Console.WriteLine($"Invalid line format: {line}");
                        continue; //  Or throw an exception
                    }

                    tasks.Add(new Task
                    {
                        Name = values[0],
                        WCET = int.Parse(values[1]),
                        BCET = int.Parse(values[2]),
                        Period = int.Parse(values[3]),
                        Deadline = int.Parse(values[4]),
                        Priority = int.Parse(values[5])
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading CSV file: {ex.Message}");
            return null; // Or re-throw the exception
        }

        return tasks;
    }

    public static bool RtaTest(List<Task> tasks)
    {
        // Sort tasks by priority (highest priority first, i.e., lowest priority number)
        tasks = tasks.OrderBy(t => t.Priority).ToList();

        foreach (var task in tasks)
        {
            int interference = 0;
            int rOld = 0;
            int rNew = task.WCET;
            int iterationCount = 0; // Add an iteration counter for safety
            const int maxIterations = 1000; // Safety limit

            while (iterationCount++ < maxIterations)
            {
                rOld = rNew;
                interference = 0;

                // Iterate through tasks with *higher* priority (lower priority number)
                foreach (var higherPriorityTask in tasks.Where(t => t.Priority < task.Priority))
                {
                    interference += (int)Math.Ceiling((double)rOld / higherPriorityTask.Period) * higherPriorityTask.WCET;
                }

                rNew = task.WCET + interference;

                if (rNew > task.Deadline)
                {
                    Console.WriteLine($"Task {task.Name} is UNSCHEDULABLE (Response Time: {rNew} > Deadline: {task.Deadline}).");
                    return false;
                }

                //if (Math.Abs(rNew - rOld) < 0.0001)  // For floating-point comparisons
                if (rNew == rOld) // Integer comparison. Much simpler!
                {
                    task.WCRT = rNew; // Store the calculated WCRT
                    break;
                }
                if (iterationCount == maxIterations) // Check here to prevent infinite loop.
                {
                    Console.WriteLine($"Task {task.Name} failed to converge within {maxIterations} iterations.  Likely UNSCHEDULABLE.");
                    return false;
                }
            }
        }

        return true; // All tasks are schedulable
    }

    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: RTAScheduler <csv_file_path>");
            return;
        }

        string csvFilePath = args[0];
        List<Task> tasks = ReadTasksFromCsv(csvFilePath);

        if (tasks == null)
        {
            return; // Error reading file
        }

        bool schedulable = RtaTest(tasks);

        if (schedulable)
        {
            Console.WriteLine("Task set is SCHEDULABLE.");
            Console.WriteLine("-------------------------");
            Console.WriteLine("Task | WCET | WCRT");
            Console.WriteLine("-------------------------");
            foreach (var task in tasks)
            {
                Console.WriteLine($"{task.Name,-4} | {task.WCET,-4} | {task.WCRT,-4}");
            }
            Console.WriteLine("-------------------------");
        }
        else
        {
            Console.WriteLine("Task set is UNSCHEDULABLE."); // We already print UNSCHEDULABLE when we find an unschedulable task in RtaTest()
        }
    }
}*/