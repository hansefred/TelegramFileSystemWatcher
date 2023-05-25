using FileSystemWatcher.Data;
using FileSystemWatcher.Model;
using Serilog;
using System;
using System.Linq;

namespace FileSystemWatcher.Services
{
    public class SystemDataService
    {

        private readonly SystemDataFactory _systemDataFactory;

        public SystemDataService(SystemDataFactory systemDataFactory)
        {
            _systemDataFactory = systemDataFactory;


        }

        public SystemData GetSystemData()
        {
            using (var context = _systemDataFactory.Create())
            {
                if (context.SystemDatas.Count() == 0)
                {
                    try
                    {
                       

                            context.Add(new SystemData() { CreatedDate = DateTime.UtcNow, ProcessedFiles = 0, SendPhotos = 0, SendVideos = 0 });
                            context.SaveChanges();
                        
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error($"Cant create Default System Data Entry: {ex.Message}");
                    }

                }

                return context.SystemDatas.OrderBy(o => o.CreatedDate).LastOrDefault();
            }
        }

        public void IncProcessedFiles()
        {

            var Latest = GetSystemData();
            using (var context = _systemDataFactory.Create())
            {
                context.Add(new SystemData() { CreatedDate = DateTime.UtcNow, ProcessedFiles = Latest.ProcessedFiles + 1, SendPhotos = Latest.SendPhotos, SendVideos = Latest.SendVideos });
                context.SaveChanges();
            }

        }

        public void IncSendPhotos()
        {

            var Latest = GetSystemData();
            using (var context = _systemDataFactory.Create())
            {
                context.Add(new SystemData() { CreatedDate = DateTime.UtcNow, ProcessedFiles = Latest.ProcessedFiles, SendPhotos = Latest.SendPhotos + 1, SendVideos = Latest.SendVideos });
                context.SaveChanges();
            }

        }
        public void IncSendVideos()
        {

            var Latest = GetSystemData();
            using (var context = _systemDataFactory.Create())
            { 
                context.Add(new SystemData() { CreatedDate = DateTime.UtcNow, ProcessedFiles = Latest.ProcessedFiles, SendPhotos = Latest.SendPhotos, SendVideos = Latest.SendVideos + 1 });
                context.SaveChanges();
            }
        }

    }
}
