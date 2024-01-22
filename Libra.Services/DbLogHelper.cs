using Libra.Services.Database;
using System;
using System.Linq;

namespace Libra.Services
{
    public class DbLogHelper
    {
        public static int CreateLogRecord(string request, string response, int? userId, string path, string sourceAddr, string objectId)
        {
            using (var db = new LibraDb())
            {
                CommonWebApiLog commonWebApiLog = new CommonWebApiLog()
                {
                    Timestamp = DateTime.Now,
                    UserId = userId,
                    Path = path,
                    Request = request,
                    Response = response,
                    SourceIp = sourceAddr,
                    ObjectIdentifier = objectId
                };
                db.CommonWebApiLog.Add(commonWebApiLog);
                db.SaveChanges();

                return commonWebApiLog.LogOid;
            }
        }

        public static int UpdateLogRecord(int logOid, string response, short responseCode, int? userId)
        {
            using (var db = new LibraDb())
            {
               var log = db.CommonWebApiLog.FirstOrDefault(x => x.LogOid == logOid);
                log.Response = response;
                log.ResponseCode = responseCode;
                return logOid;
            }
        }
    }
}

