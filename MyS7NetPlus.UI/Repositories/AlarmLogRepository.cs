using Dapper;
using Microsoft.Data.Sqlite;
using MyS7NetPlus.UI.Models;

namespace MyS7NetPlus.UI.Repositories
{
    public class AlarmLogRepository : IDisposable
    {
        SqliteConnection _connection;
        private bool disposedValue;

        public AlarmLogRepository()
        {
            _connection = new("Data Source=MyS7NetPlus1.db;");
            _connection.Open();
            // 1. 开启WAL模式（读写不阻塞，工控日志必备）
            _connection.Execute("PRAGMA journal_mode = WAL;");
            // 2. 加大内存缓存，减少磁盘IO
            _connection.Execute("PRAGMA cache_size = -100000;");
        }

        public void Insert(AlarmLog alarmLog)
        {
            alarmLog.Id = _connection.ExecuteScalar<int>("INSERT INTO AlarmLog(DeviceName, GroupName, TagName, TagValue, IsNoticed, IsAlarmed, Message, Duration, TriggeredAt) VALUES(@DeviceName, @GroupName, @TagName, @TagValue, @IsNoticed, @IsAlarmed, @Message, @Duration, @TriggeredAt);SELECT last_insert_rowid()", alarmLog);
        }

        public void BulkInsert(List<AlarmLog> alarmLogList)
        {
            var transaction = _connection.BeginTransaction();

            alarmLogList.ForEach(tagLog => _connection.Execute("INSERT INTO AlarmLog(DeviceName, GroupName, TagName, TagValue, IsNoticed, IsAlarmed, Message, Duration, TriggeredAt) VALUES(@DeviceName, @GroupName, @TagName, @TagValue, @IsNoticed, @IsAlarmed, @Message, @Duration, @TriggeredAt)", alarmLogList, transaction));

            transaction.Commit();
        }

        public List<AlarmLog> Selects(string deviceName, string groupName, string tagName, long startTime, long endTime, int count)
        {
            return _connection.Query<AlarmLog>(@"SELECT * FROM AlarmLog
WHERE DeviceName = @DeviceName AND GroupName = @GroupName AND TagName = @TagName AND TriggeredAt >= @StartTime AND TriggeredAt <= @EndTime ORDER BY Id ASC LIMIT @Count", new
            {
                DeviceName = deviceName,
                GroupName = groupName,
                TagName = tagName,
                StartTime = startTime,
                EndTime = endTime,
                Count = count
            }).ToList();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                _connection?.Dispose();
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TagLogRepository()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~AlarmLogRepository()
        {
            Dispose(disposing: false);
        }
    }
}
