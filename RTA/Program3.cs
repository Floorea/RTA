/*using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Task
{
    public string Name { get; set; }
    public int WCET { get; set; }
    public int BCET { get; set; }
    public int Period { get; set; }
    public int Deadline { get; set; }
    public int Priority { get; set; }
    public int WCRT { get; set; }
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
                reader.ReadLine(); // Skip header

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if (values.Length != 6)
                    {
                        Console.WriteLine($"Invalid line format: {line}");
                        continue;
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
            Console.WriteLine($"Error reading CSV: {ex.Message}");
            return null;
        }
        return tasks;
    }

    public static bool RtaTest(List<Task> tasks)
    {
        tasks = tasks.OrderBy(t => t.Priority).ToList();

        foreach (var task in tasks)
        {
            int rNew = task.WCET;
            int rOld;
            int iterationCount = 0;
            const int maxIterations = 1000;

            do
            {
                rOld = rNew;
                int interference = 0;
                foreach (var higherPriorityTask in tasks.Where(t => t.Priority < task.Priority))
                {
                    interference += (int)Math.Ceiling((double)rOld / higherPriorityTask.Period) * higherPriorityTask.WCET;
                }
                rNew = task.WCET + interference;

                if (rNew > task.Deadline)
                {
                    Console.WriteLine($"Task {task.Name} UNSCHEDULABLE (RT: {rNew} > D: {task.Deadline}).");
                    return false;
                }

                if (rNew == rOld)
                {
                    task.WCRT = rNew;
                    break;
                }
                if (iterationCount++ >= maxIterations)
                {
                    Console.WriteLine($"Task {task.Name} failed to converge.");
                    return false;
                }
            } while (true);
        }
        return true;
    }
    // --- New Visualization Function ---
    public static void VisualizeSchedule(List<Task> tasks)
    {
        // Calculate hyperperiod (LCM of all periods)
        int hyperperiod = tasks.Select(t => t.Period).Aggregate(Lcm);

        Console.WriteLine($"\nHyperperiod: {hyperperiod}");
        Console.WriteLine("Schedule Visualization (Worst-Case Scenario):\n");

        // Create a 2D array to represent the schedule (tasks x time units)
        char[,] schedule = new char[tasks.Count, hyperperiod];

        // Initialize the schedule with underscores (idle)
        for (int i = 0; i < tasks.Count; i++)
        {
            for (int j = 0; j < hyperperiod; j++)
            {
                schedule[i, j] = '_';
            }
        }
        //Sort tasks by decreasing priority
        tasks = tasks.OrderBy(t => t.Priority).ToList();

        // Simulate the worst-case scenario (critical instant)
        List<(int TaskIndex, int ReleaseTime, int ExecutionTime, int Priority)> jobQueue = new List<(int, int, int, int)>();

        // Generate jobs for all tasks within the hyperperiod
        for (int i = 0; i < tasks.Count; i++)
        {
            for (int releaseTime = 0; releaseTime < hyperperiod; releaseTime += tasks[i].Period)
            {
                jobQueue.Add((i, releaseTime, tasks[i].WCET, tasks[i].Priority)); // TaskIndex, Release, RemainingExec, Priority
            }
        }


        // Simulate execution based on fixed-priority preemptive scheduling
        for (int time = 0; time < hyperperiod; time++)
        {
            // Find the highest-priority ready job
            int highestPriorityJobIndex = -1;
            int highestPriority = int.MaxValue; // Lower number means higher priority

            for (int j = 0; j < jobQueue.Count; j++)
            {
                if (jobQueue[j].ReleaseTime <= time && jobQueue[j].ExecutionTime > 0 && jobQueue[j].Priority < highestPriority)
                {
                    highestPriorityJobIndex = j;
                    highestPriority = jobQueue[j].Priority;
                }
            }

            // If a job is ready to execute, mark it in the schedule
            if (highestPriorityJobIndex != -1)
            {
                int taskIndex = jobQueue[highestPriorityJobIndex].TaskIndex;
                schedule[taskIndex, time] = '|'; // Mark as active

                // Reduce the remaining execution time of the job
                jobQueue[highestPriorityJobIndex] = (jobQueue[highestPriorityJobIndex].TaskIndex,
                                                    jobQueue[highestPriorityJobIndex].ReleaseTime,
                                                    jobQueue[highestPriorityJobIndex].ExecutionTime - 1,
                                                    jobQueue[highestPriorityJobIndex].Priority);


            }
        }



        // Print the schedule
        for (int i = 0; i < tasks.Count; i++)
        {
            Console.Write($"{tasks[i].Name,-3} "); // Print task name
            for (int j = 0; j < hyperperiod; j++)
            {
                Console.Write(schedule[i, j]);
            }
            Console.WriteLine();
        }
    }

    // Helper function to calculate the Least Common Multiple (LCM)
    private static int Lcm(int a, int b)
    {
        return (a * b) / Gcd(a, b);
    }

    // Helper function to calculate the Greatest Common Divisor (GCD)
    private static int Gcd(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
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

        if (tasks == null) return;

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

            VisualizeSchedule(tasks); // Call the visualization function
        }
        else
        {
            Console.WriteLine("Task set is UNSCHEDULABLE.");
        }
    }
}*/