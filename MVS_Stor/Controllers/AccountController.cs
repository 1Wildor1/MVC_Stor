using MVS_Stor.Models.Data;
using MVS_Stor.Models.ViewModels.Account;
using MVS_Stor.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MVS_Stor.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        // GET: Account/Create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        // Post: Account/Create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            //проверяем модель на валидность 
            if (!ModelState.IsValid)
                return View("CreateAccount", model);

            //проверяем соответсвие пароля
            if(!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Password do not match!");
                return View("CreateAccount", model);
            }
            using (Db db = new Db())
            {
                //проверяем имя на уникальность
                if (db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", $"Username {model.Username} is taken.");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                //создаем экземпр=ляр класса UserDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAdress = model.EmailAdress,
                    Username = model.Username,
                    Password = model.Password
                };

                //добавляем все данные в модель
                db.Users.Add(userDTO); 

                //сохраняем данные
                db.SaveChanges();

                //добавляем роль пользователю
                int id = userDTO.Id;

                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
            }
            //Записуем сообщение в TempData
            TempData["SM"] = "You are now registered and can login.";
            //переадресовываем пользователя
            return RedirectToAction("Login");
        }

        // GET: Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            //подверждаем что пользователь не авторизован
            string userName = User.Identity.Name;

            if(!string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("user-profile");
            }

            //возвращяем представление
            return View();
        }

        // GET: Account/Login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            //проверяем модель на валидность
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            //проверяем пользователя на валидность 
            bool isValid = false;

            using (Db db = new Db())
            {
                if(db.Users.Any(x => x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                    isValid = true;
                
                if(!isValid) 
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
                }
            }
        }

        //Get: /account/Logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [Authorize]
        public ActionResult UserNavPartial()
        {
            //получаем имя пользователя
            string userName = User.Identity.Name;

            //обьявляем модель
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                //получаем пользователя
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                //заполняем модель данными из контекста
                model = new UserNavPartialVM()
                {
                    FirstName= dto.FirstName,
                    LastName= dto.LastName
                };
            }
            //возвращяем частичное представление с моделью
            return PartialView(model);
        }

        //Get: /account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            //полчаем имя пользователя 
            string userName = User.Identity.Name;

            //обьявляем модель
            UserProfileVM model;

            using (Db db = new Db())
            {
                //получаем пользователя
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                //инициализируем модель данными 
                model = new UserProfileVM(dto);
            }
            //возвращяем модель в представление
            return View("UserProfile", model);
        }

        //Post: /account/user-profile
        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            bool userNameIsChanged = false;
            //проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }


            //проверяем пороль (если пользователь его меняем)
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Passwords do not much");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                //получаем имя пользователя
                string userName = User.Identity.Name;

                //проверяем сменилось ли имя пользователя
                if(userName != model.Username)
                {
                    userName = model.Username;
                    userNameIsChanged = true;
                }

                //проверяем имя на уникальность
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == userName))
                {
                    ModelState.AddModelError("", $"Username {model.Username} alredy exists.");
                    model.Username = "";
                    return View("UserProfile", model);
                }

                //изменяем модель контекста данных
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAdress = model.EmailAdress;
                dto.Username = model.Username;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                //сохраняем изменения
                db.SaveChanges();
            }
            //устанавливаем сообщение в TempData
            TempData["SM"] = "you have edited your profile!";

            if(!userNameIsChanged)
                //возвращяем представление с моделью
                return View("UserProfile", model);
            else
            {
                return RedirectToAction("Logout");
            }
        }

        //урок 27
        //Get: /account/Orders
        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            //инициализируем модель OrdersForUserVM
            List< OrdersForUserVM > ordersForUser = new List<OrdersForUserVM>();

            using (Db db = new Db())
            {

                //получаем Id пользователя
                UserDTO user = db.Users.FirstOrDefault(x => x.Username == User.Identity.Name);
                int userId = user.Id;

                //инициализируем модель OrderVM
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();

                //перебираем список товаров в OrderVM
                foreach (var order in orders)
                {

                    //инициализируем словарь товаров 
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    //обьявляем переменную конечной суммы 
                    decimal total = 0m;

                    //инициализируем модель OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    //перебираем список OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsDTO)
                    {

                        //получаем товар 
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);

                        //поучаем цену товара
                        decimal price = product.Price;

                        //получаем имя товара
                        string productName = product.Name;

                        //добавляем товар в словарь
                        productsAndQty.Add(productName, orderDetails.Quantity);

                        //получаем конечную стоимость товаров
                        total += orderDetails.Quantity * price;
                    }
                    //добавляем полученые данные в модель OrdersForUserVM
                    ordersForUser.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total= total,
                        ProductsAndQty= productsAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }
            //возвращяем представление с моделью OrdersForUserVM
            return View(ordersForUser);
        }
    }
}