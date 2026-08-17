using MyS7NetPlus.UI.Models;
using MyS7NetPlus.UI.Repositories;

namespace MyS7NetPlus.UI.Services
{
    public class AlarmLogService
    {
        AlarmLogRepository _alarmLogRepository;
        public AlarmLogService(AlarmLogRepository alarmLogRepository)
        {
            _alarmLogRepository = alarmLogRepository;
        }

        public void Insert(AlarmLog alarmLog)
        {
            _alarmLogRepository.Insert(alarmLog);
        }

        public void BulkInsert(List<AlarmLog> alarmLogList)
        {
            _alarmLogRepository.BulkInsert(alarmLogList);
        }

        public List<AlarmLog> Selects(string deviceName, string groupName, string tagName, long startTime, long endTime, int count)
        {
            return _alarmLogRepository.Selects(deviceName, groupName, tagName, startTime, endTime, count);
        }
    }
}
