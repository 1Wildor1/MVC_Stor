using MVS_Stor.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MVS_Stor
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        //урок 25 Создаем метод обработки запросов аутентификации
        protected void Application_AuthenticateRequest()
        {
            //проверяем авторизован ли пользователь 
            if (User == null)
                return;
            // получаем имя пользователя 
            string userName = Context.User.Identity.Name;

            //обьявляем массив ролей
            string[] roles = null;

            using (Db db = new Db())
            {
                //заполняем роли
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username== userName);

                if(dto == null)
                    return;
                
                roles = db.UserRoles.Where(x => x.UserId == dto.Id).Select(x => x.Role.Name).ToArray();
            }

            //создаем обьект интерфейса IPrincipal
            IIdentity userIdentity = new GenericIdentity(userName);
            IPrincipal newUserObj = new GenericPrincipal(userIdentity, roles);

            //обьявляем и инициалезируем данными Context.User 
            Context.User = newUserObj;
        }
    }
}
