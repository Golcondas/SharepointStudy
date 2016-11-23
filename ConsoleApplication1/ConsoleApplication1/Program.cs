using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YeeOffice.SocketServer.BLL.ManagerContent;
using YeeOffice.SocketServer.UDBContext;
using System.Configuration;
using Microsoft.SharePoint;
using System.Threading;
using System.Timers;
using Microsoft.SharePoint.Administration;
using System.Diagnostics;
using System.DirectoryServices;
using Microsoft.SharePoint.BusinessData.Administration;
namespace ConsoleApplication1
{
    class Program
    {
        public static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {

           // var b = Convert.ToDateTime("2016-11-30 21:50");
            //var a = DateTime.ParseExact("2016-11-30 21:50", "dd/MM/yyyy", null);

            //Console.WriteLine(b.ToString("yyyy-MM-dd HH:mm:ss"));



            //Test t = new Test();
            //t.Start();

            //YeeOfficeJob y = new YeeOfficeJob();
            //y.StartJob();

            //try
            //{
            //    ADInfo ad = new ADInfo();
            //    ad.GetADUsers();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}

            PermissionSetter.Singleton.ResetDocLib();

            Console.ReadLine();
        }
    }

    public class ADInfo
    {
        public void GetADUsers()
        {
            var path = ConfigurationManager.AppSettings["LDAP"];
            //string username = ConfigurationManager.AppSettings["AdAccount"];
            //string password = ConfigurationManager.AppSettings["AdPassword"];
            if (!string.IsNullOrWhiteSpace(path))
            {
                DirectoryEntry entry = new DirectoryEntry(path);
                DirectorySearcher searcher = new DirectorySearcher(entry);
                searcher.Filter = "(objectClass=user)";
                var userList = searcher.FindAll();
                foreach (SearchResult result in userList)
                {
                    var directoryEntry = result.GetDirectoryEntry();
                    if (directoryEntry.SchemaClassName == "user")
                    {
                        Console.WriteLine("AD账号:" + directoryEntry.Properties["userPrincipalName"].Value);
                        Console.WriteLine("电子邮件:" + directoryEntry.Properties["mail"].Value);
                        Console.WriteLine("姓:" + directoryEntry.Properties["sn"].Value);
                        Console.WriteLine("名:" + directoryEntry.Properties["givenName"].Value);
                        Console.WriteLine("显示名称:" + directoryEntry.Properties["displayName"].Value);
                        Console.WriteLine("公司:" + directoryEntry.Properties["company"].Value);
                        Console.WriteLine("部门:" + directoryEntry.Properties["department"].Value);
                        Console.WriteLine("职务:" + directoryEntry.Properties["title"].Value);
                        Console.WriteLine("经理:" + directoryEntry.Properties["manager"].Value);
                        Console.WriteLine("移动电话:" + directoryEntry.Properties["mobile"].Value);
                        Console.WriteLine("================================");
                    }

                    //foreach (PropertyValueCollection item in directoryEntry.Properties)
                    //{
                    //    Console.WriteLine("PropertyName:" + item.PropertyName);
                    //    Console.WriteLine("Value:" + item.Value);
                    //    Console.WriteLine("");
                    //}
                    //var a = directoryEntry;

                }

            }
        }
    }



    public class YeeOfficeJob
    {
        public string conn = ConfigurationManager.AppSettings["ConnectionString"];
        public int days = Convert.ToInt32(ConfigurationManager.AppSettings["Days"]);
        public int hours = Convert.ToInt32(ConfigurationManager.AppSettings["Hours"]);
        public int minutes = Convert.ToInt32(ConfigurationManager.AppSettings["Minutes"]);
        public int seconds = Convert.ToInt32(ConfigurationManager.AppSettings["Seconds"]);
        public double timing = Convert.ToDouble(ConfigurationManager.AppSettings["Timing"]);
        public void StartJob()
        {
            try
            {
                var timeCycle = new TimeSpan(days, hours, minutes, seconds);//Timer 运行周期

                var timeInterval = DateTime.Now.Date.AddHours(timing) - DateTime.Now;//

                if (timeInterval.TotalSeconds > 0)
                {
                    LogError(string.Format("{0}:延时{1}秒后启动，周期为{2}秒", DateTime.Now.ToString(), timeInterval.TotalSeconds.ToString(), timeCycle.TotalSeconds));

                    Thread.Sleep(timeInterval);
                }
                else
                {
                    LogError(string.Format("{0}:立即启动，周期为{1}秒", DateTime.Now.ToString(), timeCycle.TotalSeconds));
                }

                var timer = new System.Timers.Timer(timeCycle.TotalMilliseconds);
                timer.Elapsed += GetDataByToday;
                timer.Enabled = true;
                timer.Start();

                GetDataByToday(null, null);
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }
        public void GetDataByToday(Object source, ElapsedEventArgs e)
        {
            try
            {
                LogError(string.Format("{0}:=======================================================", DateTime.Now.ToString()));
                LogError(string.Format("{0}:开始执行操作", DateTime.Now.ToString()));

                var db = ContextManager.GetInstance(conn);
                {
                    var date = DateTime.Now.Date.AddDays(1);

                    var model = db.DocSharing.Where(item => item.LastCreated < date);
                    var modelList = model.Where(item => item.Effective == true).ToList();
                    foreach (var item in modelList)
                    {
                        try
                        {
                            SPSecurity.RunWithElevatedPrivileges(() =>
                            {
                                using (SPSite site = new SPSite(item.YGWebURL))
                                {
                                    using (var web = site.RootWeb)
                                    {

                                        var user = web.EnsureUser(item.ShareToUserDomainAccount);

                                        var listItem = web.GetListItem(item.YGDocURL);

                                        listItem.RoleAssignments.Remove(user);
                                    }
                                }
                            });
                            LogError(string.Format("{0}:删除成功!  文件【{1}】，权限【{2}】，成员【{3}】", DateTime.Now.ToString(), item.YGDocURL, item.ShareRole, item.ShareToUserDomainAccount));
                        }
                        catch (Exception ex)
                        {
                            LogError(string.Format("{0}:删除【{1}】权限失败,文档:{2},错误信息:{3}", DateTime.Now.ToString(), item.ShareToUserDomainAccount, item.YGDocURL, ex.Message));
                        }
                        item.Effective = false;
                        db.DocSharing.Attach(item);
                        db.Entry(item).Property(x => x.Effective).IsModified = true;
                    }
                    db.SaveChanges();
                }
                LogError(string.Format("{0}:执行结束", DateTime.Now.ToString()));
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }
        public void LogError(string message)
        {
            //log.Error(message);
            Console.WriteLine(message);
        }
    }

    public class Test
    {
        public void Start()
        {
            var timer = new System.Timers.Timer(60000);
            timer.Elapsed += StartTimer;
            timer.Start();
        }
        private void StartTimer(Object source, ElapsedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Thread.Sleep(300);

            sw.Stop();
            TimeSpan ts2 = sw.Elapsed;
            Console.WriteLine("{0}:Stopwatch总共花费{1}ms.", DateTime.Now.ToString(), ts2.TotalMilliseconds);
        }
    }


}
