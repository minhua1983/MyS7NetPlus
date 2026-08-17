using MyS7NetPlus.UI.Models;
using MyS7NetPlus.UI.Repositories;

namespace MyS7NetPlus.UI.Services
{
    public class TagLogService
    {
        TagLogRepository _tagLogRepository;
        public TagLogService(TagLogRepository tagLogRepository)
        {
            _tagLogRepository = tagLogRepository;
        }

        public void Insert(TagLog tagLog)
        {
            _tagLogRepository.Insert(tagLog);
        }

        public void BulkInsert(List<TagLog> tagLogList)
        {
            _tagLogRepository.BulkInsert(tagLogList);
        }

        public List<TagLog> Selects(string deviceName, string groupName, string tagName, long startTime, long endTime, int count)
        {
            return _tagLogRepository.Selects(deviceName, groupName, tagName, startTime, endTime, count);
        }
    }
}
