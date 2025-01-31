using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Security.Cryptography;

public class Program
{
    public static void Main(string[] args)
    {
        GenerateAndDisplayAesKeys();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Warning() 
            .WriteTo.File("Logs/myapp-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()  
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });

    private static void GenerateAndDisplayAesKeys()
    {
        using (Aes aes = Aes.Create())
        {
            aes.GenerateKey();
            aes.GenerateIV();

            string keyBase64 = Convert.ToBase64String(aes.Key);
            string ivBase64 = Convert.ToBase64String(aes.IV);

            System.IO.File.WriteAllText("aes-keys.txt", $"AES Key (Base64): {keyBase64}\nAES IV (Base64): {ivBase64}");

            Console.WriteLine("AES Key (Base64): " + keyBase64);
            Console.WriteLine("AES IV (Base64): " + ivBase64);
        }
    }
}
