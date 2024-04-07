/*
 * Copyright (c) 2024 Markus Schmid
 *
 * Dieser Quellcode ist Teil von CLAn - Call Log Analyzer und unterliegt der MIT-Lizenz, die
 * im Wurzelverzeichnis dieses Projekts als LICENSE-Datei hinterlegt ist. 
 * Eine Kopie der Lizenz können Sie unter folgendem Link einsehen:
 *
 * https://opensource.org/licenses/MIT
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CLAn.Infrastructure.Data;
using CLAn.Infrastructure.Services;
using System.CommandLine;

namespace CLAn
{
    class Program
    {
        static void Main(string[] args)
        {
            // DI Services

            var builder = new ConfigurationBuilder()
                .AddJsonFile("config.json", optional: false, reloadOnChange: true);
            var configuration = builder.Build();

            // Database
            var optionsBuilder = new DbContextOptionsBuilder<SqliteContext>()
                .UseSqlite(configuration.GetConnectionString("DefaultConnection"));
            var context = new SqliteContext(optionsBuilder.Options);
            //context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Services
            var services = new ServiceCollection()
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConsole();
                    loggingBuilder.AddFilter((provider, category, logLevel) =>
                    {
                        return logLevel > LogLevel.Warning;
                    });
                })
                .AddSingleton(configuration)
                .AddSingleton(optionsBuilder.Options)
                .AddSingleton<IPhoneNumberValidator, DEPhoneNumberValidator>()
                .AddSingleton<ILogValidator, CSVLogValidator>()
                .AddSingleton<IImportService, ImportService>()
                .AddSingleton<IAnalyzeService, AnalyzeService>()
                .AddDbContextPool<SqliteContext>(options => options.UseSqlite(configuration.GetConnectionString("DefaultConnection")))
                .BuildServiceProvider();

            var logger = (services.GetService<ILoggerFactory>() ?? throw new InvalidOperationException())
                .CreateLogger<Program>();

            // Arguments, Options, Commands
            var rootCommand = new RootCommand("CLAn - Call Log Analyzer");

            var importCommand = new Command("import", "Daten einlesen");
            var fileOption = new Option<FileInfo>(
                ["--file", "-f"], 
                description: "Pfad zur Datei") { IsRequired = true };

            importCommand.AddOption(fileOption);
            importCommand.SetHandler((FileInfo file) =>
            {
                var service = services.GetService<IImportService>();
 
                service?.AddLogFile(file.FullName);
                service?.Cleanup();

            }, fileOption);
            rootCommand.AddCommand(importCommand);

            var analyzeCommand = new Command("analyze", "Analyse durchführen");
            var resolutionOption = new Option<int>(
                ["--resolution", "-r"], 
                description: "Integer Wert für Abweichung von Anrufzeitpunkten in Sekunden, wo Anrufe gleicher Teilnehmer als gleiche Anrufe behandelt werden.",
                getDefaultValue: () => 15) { IsRequired = false };

            analyzeCommand.AddOption(resolutionOption);
            analyzeCommand.SetHandler((int resolution) =>
            {
                var a_service = services.GetService<IAnalyzeService>();

                a_service?.Analyze(resolution);
            }, resolutionOption);
            rootCommand.AddCommand(analyzeCommand);

            var cleanCommand = new Command("clean", "Alle Daten löschen");

            cleanCommand.SetHandler(() =>
            {
                context.Database.EnsureDeleted();
            });
            rootCommand.AddCommand(cleanCommand);

            rootCommand.Invoke(args);
        }
    }
}