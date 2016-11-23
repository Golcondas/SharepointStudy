using System.Data.Common;
using System.Data.Entity;
using YeeOffice.SocketServer.UDBContext.Model.DBModel;

namespace YeeOffice.SocketServer.UDBContext
{
    /// <summary>
    /// 尽量不要使用EF源生的内容,以后不好做改版!
    /// 2016年4月20日 15:18:48
    /// Ben.Lampson
    /// </summary>
    public class ContextManager : DbContext
    {
        private ContextManager(DbConnection conn) : base(conn, contextOwnsConnection: true)
        {
            Database.SetInitializer<ContextManager>(null);
        }

        public static ContextManager GetInstance(string conn)
        {
            var dbConn = DbProviderFactories.GetFactory("MySql.Data.MySqlClient").CreateConnection();
            dbConn.ConnectionString = conn;
            return new ContextManager(dbConn);
        }
        public DbSet<DocSharing> DocSharing { get; set; }
    }
}
