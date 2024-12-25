using System.Diagnostics;

namespace async_space_counter
{

    #region Вспомогательные функц
    // Класс для чтения файлов
    public class FileReader
    {
        public static async Task<string> ReadFileAsync(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }

    // Класс для подсчета пробелов
    public class SpaceCounter
    {
        public static async Task<int> SpaceCounterAsync(string content)
        {
            return await Task.Run(() =>
            {
                int spaceCount = 0;
                foreach (char c in content)
                {
                    if (c == ' ')
                    {
                        spaceCount++;
                    }
                }
                return spaceCount;
            });
        }
    }
    #endregion

    #region варианты
    public class Sequential
    {
        public static async Task SequentialFileReading(string file)
        {
            Console.Out.WriteLine("а) Последовательное считывание файловв");
            Stopwatch TaskDur = Stopwatch.StartNew();
            //wait to read file
            string content = await FileReader.ReadFileAsync(file); //????????????????
            await Console.Out.WriteLineAsync($"{Path.GetFileName(file)} ----- was readed, with time {TaskDur.Elapsed.Milliseconds} ms");
            //counting space
            int spaceCount = await SpaceCounter.SpaceCounterAsync(content);

            TaskDur.Stop();
            await Console.Out.WriteLineAsync( $"{Path.GetFileName(file)} ----- Spaces: {spaceCount}, with time {TaskDur.ElapsedMilliseconds} ms");

        }

        public static async Task Start(string[] txtFiles)
        {
            Stopwatch AllTaskDur = Stopwatch.StartNew();

            foreach (var file in txtFiles)
            {
                
                await SequentialFileReading(file);
            }
            AllTaskDur.Stop();
            await Console.Out.WriteLineAsync($"All task duration time: {AllTaskDur.ElapsedMilliseconds}");

            Console.Out.WriteLine("\n-----------------------------------------------------------------------------\n");
        }
    }
        
    public class Parallel
    {
        static async Task ParallelFileReading(string[] txtFiles)
        {
            var counter = txtFiles.Select(async file =>
            {
                Stopwatch TaskDur = Stopwatch.StartNew();
                int spaceCount = await SpaceCounter.SpaceCounterAsync(file);//????????????????
                TaskDur.Stop();
                await Console.Out.WriteLineAsync($"{Path.GetFileName(file)} ----- Spaces: {spaceCount}, with time {TaskDur.ElapsedMilliseconds} ms");
            });

            await Task.WhenAll(counter);
        }

        public static async Task Start(string[] txtFiles)
        {
            Console.Out.WriteLine("б) Параллельное считывание всех файлов");
            Stopwatch AllTaskDur = Stopwatch.StartNew();
            await ParallelFileReading(txtFiles);
            await Console.Out.WriteLineAsync($"All task duration time: {AllTaskDur.ElapsedMilliseconds}");
            AllTaskDur.Stop();

            Console.Out.WriteLine("\n-----------------------------------------------------------------------------\n");
        }
    }

    public class LineByLine
    {
        static async Task LineByLineReading(string[] txtFiles)
        {

            var counter = txtFiles.Select(async file =>
            {

                Stopwatch TaskDur = Stopwatch.StartNew();
                int spaceCount = 9999999;

                await Task.Run(async () =>
                {
                    Stopwatch TaskDur = Stopwatch.StartNew();

                    // Считываем весь файл как строку
                    string content = await File.ReadAllTextAsync(file);

                    // Разбиваем содержимое на строки
                    string[] lines = content.Split(Environment.NewLine);

                    var tasks = lines.Select(async line =>
                    {
                        return await SpaceCounter.SpaceCounterAsync(line);
                    });

                    // Дожидаемся завершения всех задач и суммируем результаты
                    var results = await Task.WhenAll(tasks); //массив с результатами всех задач
                    spaceCount = results.Sum();

                });

                TaskDur.Stop();
                await Console.Out.WriteLineAsync($"{Path.GetFileName(file)} ----- Spaces: {spaceCount}, with time {TaskDur.ElapsedMilliseconds} ms");
            });

            await Task.WhenAll(counter);

        }

        public static async Task Start(string[] txtFiles)
        {
            Console.Out.WriteLine("в) Асинхронное считывание построчно");
            Stopwatch AllTaskDur = Stopwatch.StartNew();
            await LineByLineReading(txtFiles);
            await Console.Out.WriteLineAsync($"All task duration time: {AllTaskDur.ElapsedMilliseconds}");
            AllTaskDur.Stop();
            Console.Out.WriteLine("\n-----------------------------------------------------------------------------\n");
        }
    }

    #endregion
    class Program
    {
        static async Task Main(string[] args)
        {
            string folderPath = "C:/Users/danil/source/repos/WpfCalculator/async_space_counter/async_space_counter/files/";
            string[] txtFiles = Directory.GetFiles(folderPath, "*.txt");

            //await Sequential.Start(txtFiles);
            //await Parallel.Start(txtFiles);
            await LineByLine.Start(txtFiles);

            //// Вариант г: Асинхронный подсчет пробелов
            //await TimeTracker.TrackAsync(async () =>
            //{
            //    var tasks = txtFiles.Select(async file =>
            //    {
            //        string content = await FileReader.ReadFileAsync(file);
            //        int spaceCount = await SpaceCounter.CountSpacesAsync(content);
            //        Console.WriteLine($"File: {Path.GetFileName(file)}, Spaces: {spaceCount}");
            //    });

            //    await Task.WhenAll(tasks);
            //}, "Asynchronous Space Counting");
        }
    }
}
