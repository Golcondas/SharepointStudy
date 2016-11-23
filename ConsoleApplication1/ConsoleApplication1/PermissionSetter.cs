using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Sharing;
using Microsoft.SharePoint.Client.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YeeOffice.SocketServer.BLL.ManagerContent
{
    public class PermissionSetter
    {
        public static PermissionSetter Singleton { get; private set; }

        static PermissionSetter()
        {
            Singleton = new PermissionSetter();
        }

        private PermissionSetter() { }

        /// <summary>
        /// 访问-SharePoint的数据库
        /// </summary>
        /// <returns></returns>
        public bool ResetDocLib()
        {
            //List<string> accountList = new List<string>();
            //accountList.Add("i:0#.f|membership|test1@xinguang.partner.onmschina.cn");

            var siteUrl = "https://yeeofficedev.sharepoint.cn/DMS/";
            var userAccount = "admin@yeeofficedev.partner.onmschina.cn";
            var password = "P@$$word";

            using (var clientContext = new ClientContext(siteUrl))
            {
                clientContext.AuthenticationMode = ClientAuthenticationMode.Default;
                clientContext.Credentials = BuildCredentials(userAccount, password);//new System.Net.NetworkCredential("spadmin", "1qaz@WSX", "hengdeligroup");


                clientContext.Load(clientContext.Web);
                clientContext.ExecuteQuery();


                var lists = clientContext.Web.Lists;
                clientContext.Load(lists);
                clientContext.ExecuteQuery();
                var list = lists.GetById(new Guid("2c9a497a-e7e8-4ee7-bc8a-4437d65d53dd"));//.GetByTitle("akmii");
                clientContext.Load(list);
                clientContext.ExecuteQuery();


                CamlQuery caml = new CamlQuery();
//                caml.ViewXml = @"<View Scope='RecursiveAll'>
//	<Query>
//		<Where>
//			<And>
//				<And>
//					<Eq>
//						<FieldRef Name='ContentType'/>
//						<Value Type='Text'>文档</Value>
//					</Eq>
//					<Eq>
//						<FieldRef Name='Author'/>
//						<Value Type='Text'>DevYeeOffice</Value>
//					</Eq>
//				</And>
//				<Contains>
//					<FieldRef Name='LinkFilename' />
//					<Value Type='Computed'></Value>
//				</Contains>
//			</And>
//		</Where>
//		<OrderBy>
//		<FieldRef Name='Created' Ascending='FALSE' />
//		</OrderBy>
//	</Query>
//</View>";

                //caml.ViewXml = "<View Scope='RecursiveAll'>" +
                //     "<Query>" +
                //       "<Where>" +
                //       "<And>" +
                //               "<Eq>" +
                //                   "<FieldRef Name='ContentType'/>" +
                //                   "<Value Type='Text'>文档</Value>" +
                //               "</Eq>" +
                //               "<Eq>" +
                //                   "<FieldRef Name='Author'/>" +
                //                   "<Value Type='Text'>DevYeeOffice</Value>" +
                //               "</Eq>" +
                //           "</And>" +
                //       "</Where><OrderBy><FieldRef Name='Created' Ascending='FALSE' /></OrderBy>" +
                //     "</Query>" +
                //   "</View>";

                caml.ViewXml = "<View Scope='RecursiveAll'><Query><Where><And>Demo<Eq><FieldRef Name='ContentType'/><Value Type='Text'>文档</Value></Eq><Eq><FieldRef Name='Author'/><Value Type='Text'>DevYeeOffice</Value></Eq></And><Contains><FieldRef Name='LinkFilename' /><Value Type='Computed'>Demo</Value></Contains></And></Where><OrderBy><FieldRef Name='Created' Ascending='FALSE' /></OrderBy></Query></View>";


                var files = list.GetItems(caml);

                clientContext.Load(files, item => item.Include(a => a["Author"]
                    , a => a["Created"]
                    , a => a["FileRef"]
                    , a => a["FileLeafRef"]
                    , a => a["File_x0020_Size"]
                    , a => a.Id
                    , a => a.FileSystemObjectType));

                clientContext.ExecuteQuery();

                foreach (var item in files)
                {
                    var a = item;
                }


                var listItem = list.GetItemById(4);
                clientContext.Load(listItem);
                clientContext.ExecuteQuery();

                return true;
            }
        }

        private SharePointOnlineCredentials BuildCredentials(string userAccount, string password)
        {
            var securePassword = new System.Security.SecureString();
            foreach (char pwChar in password)
            {
                securePassword.AppendChar(pwChar);
            }
            return new SharePointOnlineCredentials(userAccount, securePassword);
        }

        private List GetList(ClientContext clientContext, string listName)
        {
            clientContext.Load(clientContext.Web);
            clientContext.ExecuteQuery();

            var lists = clientContext.Web.Lists;
            clientContext.Load(lists);
            clientContext.ExecuteQuery();

            var list = clientContext.Web.Lists.GetByTitle(listName);
            clientContext.Load(list);
            clientContext.ExecuteQuery();



            return list;
        }


        private File GetFile(ClientContext clientContext, string fileUrl)
        {
            clientContext.Load(clientContext.Web);
            clientContext.ExecuteQuery();

            var file = clientContext.Web.GetFileByServerRelativeUrl(fileUrl);
            clientContext.Load(file);
            clientContext.ExecuteQuery();
            return file;
        }

        private void LoadItemProperties(ClientContext clientContext, ListItem listItem)
        {
            clientContext.Load(listItem
                , item => item.HasUniqueRoleAssignments
                , item => item.RoleAssignments
                , item => item.Id
                );
            clientContext.ExecuteQuery();
        }

        private void LoadItemProperties(ClientContext clientContext, List list)
        {
            clientContext.Load(list
                , item => item.HasUniqueRoleAssignments
                , item => item.RoleAssignments
                , item => item.RoleAssignments.Include(a => a.RoleDefinitionBindings, a => a.Member));
            clientContext.ExecuteQuery();
        }

        public void Reset(ClientContext clientContext, ListItem listItem, List<string> accountList, int competenceID)
        {
            if (listItem == null) throw new ArgumentNullException("listItem");

            try
            {
                Clear(clientContext, listItem, accountList);
                switch (competenceID)
                {
                    case 1: AssignRead(clientContext, listItem, accountList); break;
                    case 2: AssignContribute(clientContext, listItem, accountList); break;
                    case 3: AssignFullControl(clientContext, listItem, accountList); break;
                }
                listItem.Update();
                clientContext.ExecuteQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Reset permission Error.", ex);
            }
        }

        public void Reset(ClientContext clientContext, List list, List<string> accountList, int competenceID)
        {
            if (list == null) throw new ArgumentNullException("list");

            try
            {

                Clear(clientContext, list);
                switch (competenceID)
                {
                    case 1: AssignRead(clientContext, list, accountList); break;
                    case 2: AssignContribute(clientContext, list, accountList); break;
                    case 3: AssignFullControl(clientContext, list, accountList); break;
                }
                list.Update();
                clientContext.ExecuteQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Reset permission Error.", ex);
            }
        }

        private void Clear(ClientContext clientContext, ListItem listItem, List<string> accountList)
        {
            if (listItem.HasUniqueRoleAssignments)
            {
                for (var index = listItem.RoleAssignments.Count - 1; index >= 0; index--)
                {
                    clientContext.Load(listItem.RoleAssignments[index].Member);
                    clientContext.ExecuteQuery();
                    if (listItem.RoleAssignments[index].Member.LoginName.Equals(accountList[0]))
                    {
                        listItem.RoleAssignments[index].DeleteObject();
                    }
                }
            }
            else
            {
                listItem.BreakRoleInheritance(false, true);
            }
        }

        private void Clear(ClientContext clientContext, List list)
        {
            if (!list.HasUniqueRoleAssignments)
            {
                list.BreakRoleInheritance(false, true);
            }
            //else
            //{
            //    for (var index = list.RoleAssignments.Count - 1; index >= 0; index--)
            //    {
            //        //list.RoleAssignments[index].DeleteObject();

            //        list.RoleAssignments.GetByPrincipal(list.RoleAssignments[index].Member).DeleteObject();
            //    }
            //}

        }

        private void AssignFullControl(ClientContext clientContext, List list, List<string> accountList)
        {
            var roleDefinitionBindings = new RoleDefinitionBindingCollection(clientContext);
            roleDefinitionBindings.Add(clientContext.Web.RoleDefinitions.GetByType(RoleType.Administrator));
            foreach (var editor in accountList)
            {
                var user = clientContext.Web.EnsureUser(editor);
                list.RoleAssignments.Add(user, roleDefinitionBindings);
            }
        }

        private void AssignFullControl(ClientContext clientContext, ListItem listItem, List<string> accountList)
        {
            var roleDefinitionBindings = new RoleDefinitionBindingCollection(clientContext);
            roleDefinitionBindings.Add(clientContext.Web.RoleDefinitions.GetByType(RoleType.Administrator));
            foreach (var editor in accountList)
            {
                var user = clientContext.Web.EnsureUser(editor);
                listItem.RoleAssignments.Add(user, roleDefinitionBindings);
            }
        }

        private void AssignContribute(ClientContext clientContext, List list, List<string> accountList)
        {
            var roleDefinitionBindings = new RoleDefinitionBindingCollection(clientContext);
            roleDefinitionBindings.Add(clientContext.Web.RoleDefinitions.GetByType(RoleType.Contributor));
            foreach (var editor in accountList)
            {
                var user = clientContext.Web.EnsureUser(editor);

                list.RoleAssignments.Add(user, roleDefinitionBindings);
            }
        }

        private void AssignContribute(ClientContext clientContext, ListItem listItem, List<string> accountList)
        {
            var roleDefinitionBindings = new RoleDefinitionBindingCollection(clientContext);
            roleDefinitionBindings.Add(clientContext.Web.RoleDefinitions.GetByType(RoleType.Contributor));
            foreach (var editor in accountList)
            {
                var user = clientContext.Web.EnsureUser(editor);
                listItem.RoleAssignments.Add(user, roleDefinitionBindings);
            }
        }

        private void AssignRead(ClientContext clientContext, List list, List<string> accountList)
        {
            var roleDefinitionBindings = new RoleDefinitionBindingCollection(clientContext);
            roleDefinitionBindings.Add(clientContext.Web.RoleDefinitions.GetByType(RoleType.Reader));
            foreach (var editor in accountList)
            {
                var user = clientContext.Web.EnsureUser(editor);
                list.RoleAssignments.Add(user, roleDefinitionBindings);
            }
        }

        private void AssignRead(ClientContext clientContext, ListItem listItem, List<string> accountList)
        {
            var roleDefinitionBindings = new RoleDefinitionBindingCollection(clientContext);
            roleDefinitionBindings.Add(clientContext.Web.RoleDefinitions.GetByType(RoleType.Reader));
            foreach (var editor in accountList)
            {
                var user = clientContext.Web.EnsureUser(editor);
                listItem.RoleAssignments.Add(user, roleDefinitionBindings);
            }
        }
        private int GetUserIDByUserName(ClientContext clientContext, string name)
        {
            var user = clientContext.Web.EnsureUser(name);
            clientContext.Load(user);
            clientContext.ExecuteQuery();
            return user.Id;
        }
    }
}
