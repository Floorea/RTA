# Response-Time Analyser (RTA)

Expected Input format:
This project expect the input file to be .csv file with columns structured as follows: Task,BCET,WCET,Period,Deadline,Priority

Output format:
After the analysis is completed, the results will be returned in the cmd. window, with columns structured as follows: Task,BCET,WCET,Period,Deadline,Priority,WCRT
The run time of the algorithm will also be displayed.
# Run the project:

1. This is Dotnet 8 project, if not installed already a dotnet 8 sdk is required, can be found [here](https://dotnet.microsoft.com/en-us/download)
2. Go to RTA folder (containing RTA.csproj).
3. Build project:

```console
dotnet build
```

4. Run project:

```console
dotnet run inputfile.csv
```

inputfile.csv should be replaced with correct file with data that it will be tested with. Recomended to keep file in same directory as well.
