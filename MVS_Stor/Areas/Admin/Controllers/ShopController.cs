using MVS_Stor.Areas.Admin.Models.Shop;
using MVS_Stor.Models.Data;
using MVS_Stor.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace MVS_Stor.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            //обьявляем модель типа List
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                //инициализируем модель данными
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }

            //Возвращяем List в представление 
            return View(categoryVMList);
        }

        //(урок 9)
        // Post: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //обьявляем преременную ID
            string id;

            using (Db db = new Db())
            {

                //проверяем имя на уникальность 
                if (db.Categories.Any(x => x.Name == catName))
                    return "titletaken";
                //инизиализируем модель DTO
                CategoryDTO dto = new CategoryDTO();

                //добавляем данные в модель
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                //сохранить
                db.Categories.Add(dto);
                db.SaveChanges();

                //получаем ID для возврата в представление
                id = dto.Id.ToString();

            }
            //возвращяем ID в представление
            return id;
        }

        //создаем метод сортировки(урок 9) 
        // Post: Admin/View/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //реализуемначальный счетчик
                int count = 1;

                //Инизиализируем модель данных
                CategoryDTO dto;

                //Устанавливаем сотрировку для каждой страницы
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }

        //создаем метод удаления (урок 9)
        // GET: Admin/View/Shop/DeleteCategory/id
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {

                //получение модель категории
                CategoryDTO dto = db.Categories.Find(id);

                //удаление категорию
                db.Categories.Remove(dto);

                //Сохронение изменений в базе
                db.SaveChanges();

                //сообщение пользавателю о удалении
            }
            TempData["SM"] = "You have deleted a Category!";
            //переадрисовываем пользователя
            return RedirectToAction("Categories");
        }

        //создаем метод удаления (урок 10)
        // Post: Admin/View/Shop/RenameCategory/id
        [HttpPost]
        public string RenameCategory( string newCatName, int id)
        {
            using (Db db = new Db())
            {

                //проверяем имя на уникальность 
                if(db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";

                //получаем модель из DTO
                CategoryDTO dto = db.Categories.Find(id); 

                //Редактируем модель DTO 
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ","-").ToLower();

                //сохраняем изменение
                db.SaveChanges();
            
            }
            //Возвращяем слово
            return "ok";
        }

        //создаем метод добавления товаров ( урок 11 )
        // GET: Admin/View/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //обьявляем модель данных
            ProductVM model = new ProductVM();

            //добавляем список категорий из базы в модель
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(),"id", "Name");
            }

            //возвращяем модель в представление
            return View(model);
        }

        //создаем метод добавления товаров ( урок 12)
        // Post Admin/View/Shop/AddProducts
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            //проверяем модель на валидность
            if(!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            //проверка имени продукта на уникальность
            using (Db db = new Db())
            {
                if(db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That product name is taken");
                    return View(model);
                }
            }
            //Обьявляем переменую ProductID
            int id;

            //инициализируем и сохраняем модель на основе ProductDTO
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price= model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;
            }

            //добавляем сообщение в TemoData
            TempData["SM"] = "You have added a product!";

            #region Upload Image

            //создаем необходимые ссылки на дерикторий
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            //проверяем наличие дерикторий (если нет, создаем)
            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            //проверяем,был ли файл загружен
            if (file != null && file.ContentLength > 0)
            {
                //Получаем расширение файла
                string ext = file.ContentType.ToLower();

                //проверяем расширение файла
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension");
                        return View(model);
                    }
                }
                //обтявляем переменную с именем изображения 
                string imageName = file.FileName;

                // Сохраняем имя изображения в модель DTO
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                //назначаем пути к оригинальному и уменьшенему изображению
                var path = string.Format($"{pathString2}\\{imageName}");
                var path2 = string.Format($"{pathString3}\\{imageName}");

                // сохраняем оригинальное изображение
                file.SaveAs( path );

                // создаем и сохраняем уменьшенную копию
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1, 1);
                img.Save(path2);
            }
            #endregion

            // переадресовать пользователя

            return RedirectToAction("AddProduct");
        }

        //создаем метод списка товаров ( урок 13)
        // Post Admin/View/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            //обьвляем ProductVM типа list
            List<ProductVM> listOfProductVM;

            //устанавлеваем номер страницы
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {

                //инизиализируем list и заполняем данными
                listOfProductVM = db.Products.ToArray().Where(x => catId == null || catId == 0 || x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                //заполняем категории данными
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //устанавливаем выбраную категорию
                ViewBag.SelectedCat = catId.ToString();
            }
            //устанавливаем постраничную навигацию
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.onePageOfProducts = onePageOfProducts;

            //возвращяем представление с данными
            return View(listOfProductVM);
        }

        //создаем метод редактирования товаров ( урок 14)
        // Get Admin/View/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {

            //обьявляем ProductVM
            ProductVM model;

            using (Db db = new Db())
            {

                //поучаем продукт
                ProductDTO dto = db.Products.Find(id);

                //проверяем доступен ли продукт 
                if (dto == null)
                {
                    return Content("That product does exist.");
                }

                //инизиализируем модель данных
                model = new ProductVM(dto);

                //Создаем список категорий
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //получаем все изображения из галереи
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs")).Select(fn => Path.GetFileName(fn));
            }
            //Возвращяем модель в представление 
            return View(model);
        }

        //создаем метод редактирования товаров ( урок 14)
        // Post Admin/View/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            //получаем id продукта
            int id = model.Id;

            //заполняем список категориями и изображениями
            using (Db db =new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "id", "Name");
            }

            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs")).Select(fn => Path.GetFileName(fn));

            //проверяем модель на валидность 
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // проверяем имя продукта на уникальность
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }
            //обновляем продукт
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }
            //устанавливаем сообщение в TempData
            TempData["SM"] = "You have edited the product!";

            //логика обработки изображений (урок 15)
            #region Image Upload

            //проверяем загружен ли файл
            if (file != null && file.ContentLength > 0)
            {
                //получаем рассширение файла
                string ext = file.ContentType.ToLower();

                //проверяем рассштрение файла
                if (ext != "image/jpg" &&
                   ext != "image/jpeg" &&
                   ext != "image/pjpeg" &&
                   ext != "image/gif" &&
                   ext != "image/x-png" &&
                   ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension");
                        return View(model);
                    }
                }
                //устанавливаем пути загрузки
                var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //удаляем существующие файли и директории
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach(var file2 in di1.GetFiles())
                {
                    file2.Delete();
                }

                foreach (var file3 in di2.GetFiles())
                {
                    file3.Delete();
                }

                //сохраняем имя изображения
                string imageName = file.FileName;

                using (Db db =new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                //сохраняем оригинал и превью версии
                var path = string.Format($"{pathString1}\\{imageName}");
                var path2 = string.Format($"{pathString2}\\{imageName}");

                // сохраняем оригинальное изображение
                file.SaveAs(path);

                // создаем и сохраняем уменьшенную копию
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1,1);
                img.Save(path2);
            }
            #endregion

            //переадресовываем пользователя

            return RedirectToAction("EditProduct");
        }


        //создаем метод удаления товаров ( урок 15)
        // Post Admin/View/Shop/DeleteProduct/id
        public ActionResult DeleteProduct(int id)
        {
            //удаляем товар из базы данных
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }

            //удаляем дериктории товаров(картинки)
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
            var pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);
            //переадрисовывам пользоателя
            return RedirectToAction("Products");
        }

        //создаем метод добавления изображений в галерею ( урок 16)
        // Post Admin/View/Shop/SaveGalleryImages/id
        [HttpPost]
        public void SaveGalleryImages( int id)
        {
            //перебераем  все полученые файлы 
            foreach (string fileName in Request.Files)
            {
                //инициализируем файлы 
                HttpPostedFileBase file = Request.Files[fileName];

                //проверяем на null
                if(file != null && file.ContentLength > 0)
                {
                    //назначяем все пути к дерикториям
                    var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    //назначаем пути самих изображений
                    var path = string.Format($"{pathString1}\\{file.FileName}");
                    var path2 = string.Format($"{pathString2}\\{file.FileName}");


                    //сохраняем ориг и уменьшеные копии изображений
                    file.SaveAs(path);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200).Crop(1,1);

                    img.Save(path2);
                }

            }
        }

        //создаем метод удаления изображений из галереи ( урок 16)
        // Post Admin/View/Shop/DeleteImage/id/imageName
        [HttpPost]
        public void DeleteImage(int id,string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }

        //создаем метод ввывода всех заказов для администратора ( урок 27)
        // Get Admin/View/Shop/Orders
        public ActionResult Orders()
        {
            //инициализируем модель OrdersForAdmin
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {

                //инициализируем модель OrderVM
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                //перебераем данные модели OrderVM
                foreach (var order in orders)
                {

                    //инициализируем словарь товаров
                    Dictionary<string,int> productAndQty = new Dictionary<string,int>();

                    //обьявляем переменную общей суммы 
                    decimal total = 0m;

                    //инициализируем лист OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    //получаем имя пользователя
                    UserDTO user = db.Users.FirstOrDefault(x => x.Id== order.UserId);

                    string userName = user.Username;

                    //перебираем список товаров  из OrderDetailsDTO
                    foreach (var orderDetails in orderDetailList)
                    {
                        //получаем товар 
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);

                        //получаем цену товара
                        decimal price = product.Price;

                        //получаем название товара
                        string productName = product.Name;

                        // добавляем товар в словарь 
                        productAndQty.Add(productName, orderDetails.Quantity);

                        //получаем общую стоимость товаров
                        total += orderDetails.Quantity * price;
                    }
                    //добавляем данные в модель OrdersForAdmin
                    ordersForAdmin.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        UserName = userName,
                        Total = total,
                        ProductsAndQty = productAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }
            //возвращяем представление с моделью OrdersForAdminVM
            return View(ordersForAdmin);
        }
    }
}