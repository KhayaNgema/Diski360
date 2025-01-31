using Microsoft.AspNetCore.Http;
using MyField.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Linq;
using MyField.Data;
using UAParser;

namespace MyField.Services
{
    public class DeviceInfoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly Ksans_SportsDbContext _context;

        public DeviceInfoService(IHttpContextAccessor httpContextAccessor, HttpClient httpClient, Ksans_SportsDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            _context = context;
        }

        public async Task<DeviceInfo> GetDeviceInfo()
        {
            var userAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"];
            var parser = Parser.GetDefault();
            var clientInfo = parser.Parse(userAgent);

            var deviceInfo = new DeviceInfo();

            deviceInfo.DeviceName = clientInfo.Device.Family;
            deviceInfo.DeviceModel = clientInfo.Device.Model;
            deviceInfo.Browser = clientInfo.UA.Family;
            deviceInfo.BrowserVersion = clientInfo.UA.Major;
            deviceInfo.OSName = clientInfo.OS.Family;
            deviceInfo.OSVersion = clientInfo.OS.Major;

            var ipAddress = GetIpAddress();
            deviceInfo.IpAddress = ipAddress;

            try
            {
                var ip2LocationApiKey = "B98F30EA4D6F54D5212B279C391A11B0";
                var response = await _httpClient.GetAsync($"https://api.ip2location.io/?key={ip2LocationApiKey}&ip={ipAddress}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var locationData = JObject.Parse(jsonString);

                    deviceInfo.Country = locationData["country_name"]?.ToString();
                    deviceInfo.Region = locationData["region_name"]?.ToString();
                    deviceInfo.City = locationData["city_name"]?.ToString();
                    deviceInfo.PostalCode = locationData["zip_code"]?.ToString();
                    deviceInfo.Latitude = locationData["latitude"]?.ToString();
                    deviceInfo.Longitude = locationData["longitude"]?.ToString();
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving location information: {ex.Message}");
            }

            return deviceInfo;
        }

        public string GetIpAddress()
        {
            var ipAddress = string.Empty;

            if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"];
            }
            else if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey("RemoteAddr"))
            {
                ipAddress = _httpContextAccessor.HttpContext.Request.Headers["RemoteAddr"];
            }
            else if (_httpContextAccessor.HttpContext.Connection.RemoteIpAddress != null)
            {
                ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            return ipAddress;
        }
    }
}