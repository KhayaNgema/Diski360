using MyField.Data;
using MyField.Models;

namespace MyField.Services
{
    public class RequestLogService
    {
        Ksans_SportsDbContext _context;
        public RequestLogService(Ksans_SportsDbContext context) 
        { 
            _context = context;
        }

        public async Task LogSuceededRequest(string message, int StatusCode)
        {
            var succesfulRequest = new RequestLog
            {
                RequestDescription = message,
                RequestType = RequestType.Succeeded,
                ResponseCode = StatusCode,
                TimeStamp = DateTime.Now
            };

            _context.Add(succesfulRequest);
            await _context.SaveChangesAsync();
        }

        public async Task LogFailedRequest(string message, int StatusCode)
        {
            var succesfulRequest = new RequestLog
            {
                RequestDescription = message,
                RequestType = RequestType.Failed,
                ResponseCode = StatusCode,
                TimeStamp = DateTime.Now
            };

            _context.Add(succesfulRequest);
            await _context.SaveChangesAsync();
        }
    }
}
