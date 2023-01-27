using MVS_Stor.Models.Data;
using MVS_Stor.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVS_Stor.Areas.Admin.Controllers
{
    public class pagesController : Controller
    {
        [Authorize(Roles = "Admin")]
        // GET: Admin/pages
        public ActionResult Index()
        {
            //обьявляем список для предсавления(PageVM)
            List<PageVM> pageList;

            //Инициализируем список(DB)
            using (Db db = new Db())
            {
                pageList= db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }

            //возвращяем список в представление
            return View(pageList);
        }

        // GET: Admin/View/pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        // Post: Admin/View/pages/AddPage

        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //проверка модели на валидность
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {


                //обьявляем переменную для краткого описания(slug)
                string slug;

                //Инициализируем класс PageDTO
                PagesDTO dto= new PagesDTO();

                //присваеваем заголовок модели
                dto.Title = model.Title.ToUpper();

                //проверяем, есть ли краткое описание, если нет, присваеваем его
                if(string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug= model.Title.Replace(" ","-").ToLower();
                }
                else
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }

                //убеждаемся что заголовок и краткое описание - уникальны
                if(db.Pages.Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That title alredy exist.");
                    return View(model);
                }
                else if (db.Pages.Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "That slug alredy exist.");
                    return View(model);
                }

                //присваеваем оставшиеся значение модели 
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar= model.HasSidebar;
                dto.Sorting = 100;

                //сохроняем модель в базу данных
                db.Pages.Add(dto);
                db.SaveChanges();
            }
            //передаем сообщение через TempData
            TempData["SM"] = "you have added a new page!";
            //переадресовываем пользователяна метод INDEX
            return RedirectToAction("Index");
        }

        // GET: Admin/View/pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)    
        {
            //обтявляем модель PageVM
            PageVM model;

            using (Db db = new Db())
            {
                //получаем страницу
                PagesDTO dto = db.Pages.Find(id);

                //проверяем доступна ли страница
                if (dto == null)
                {
                    return Content("The pagedoes not exit.");
                }
                //инициализируем модель данными
                model = new PageVM(dto);
            }
            //Возвращяем модель в представление
            return View(model);
        }

        // Post: Admin/View/pages/AddPage
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //проверка на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                //получаем ID страницы
                int id = model.Id;

                //обьявляем переменую краткого заголовка
                string slug = "home";

                //Получаем страницу(по ID)
                PagesDTO dto = db.Pages.Find(id);

                //присваеваем название из полученной модели DTO
                dto.Title = model.Title;

                //проверяем краткий заголовок  и присваеваем его, если это необходимо
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                //проверяем slug и title на уникальность
                if(db.Pages.Where(x=> x.Id != id).Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That title alredy exist.");
                    return View(model);
                }
                else if(db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That slug alredy exist.");
                    return View(model);
                }

                //записываем остальные значения в класс DTO
                dto.Slug= slug;
                dto.Body= model.Body;
                dto.HasSidebar = model.HasSidebar;

                // сохраняем изменения  в базу
                db.SaveChanges();

            }
            //устанавливаем сообщение в TemData
            TempData["SM"] = "You have edited the page.";

            //Переадресация пользователя
            return RedirectToAction("EditPage");
        }

        //создаем метод страницы деталей (урок 4 )
        // GET: Admin/View/pages/PageDeTails/id
        public ActionResult PageDetails(int id)
        {
            //обьявляем модель PageVM
            PageVM model;

            using (Db db = new Db())
            {
                //Получаем страницу
                PagesDTO dto = db.Pages.Find(id);

                //Подтверждаем что страница доступна
                if(dto == null) 
                {
                    return Content("The page does not exist.");
                }
                
                //Присваеваем модели информацию из базы
                model = new PageVM(dto);

            }
            //Возвращяем модель в представление
            return View(model);
        }

        //создаем метод удаления (урок 5)
        // GET: Admin/View/pages/DeletePage/id
        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {

                //получение страницы
                PagesDTO dto= db.Pages.Find(id);

                //удаление страницы
                db.Pages.Remove(dto);

                //Сохронение изменений в базе
                db.SaveChanges();

                //сообщение пользавателю о удалении
            }
                TempData["SM"] = "You have deleted a page!";
            //переадрисовываем пользователя
            return RedirectToAction("Index");
        }

        //создаем метод сортировки(урок 5) 
        // GET: Admin/View/pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int[] id) 
        {
            using (Db db = new Db())
            {
                //реализуемначальный счетчик
                int count = 1;

                //Инизиализируем модель данных
                PagesDTO dto;

                //Устанавливаем сотрировку для каждой страницы
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }

        //(урок 6) 
        // GET: Admin/View/pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //Обьявляем модель 
            SidebarVM model;

            using (Db db = new Db())
            {
                //Получение данных из DTO
                SidebarDTO dto = db.Sidebars.Find(1); // Говнокод! Жосткие значения в коде не желательно добавлять!!!

                //Заполняем модель
                model = new SidebarVM(dto);
            }

            //Вернуть представление с модель
            return View(model);
        }

        //(урок 6) 
        //Post: Admin/View/pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                //получение данных из DTO
                SidebarDTO dto = db.Sidebars.Find(1);

                //присваиваем данные в тело (в свойство body)
                dto.Body = model.Body;

                //Сохранить 
                db.SaveChanges();
            }
            //Присваеваем сообщение в TempData
            TempData["SM"] = "you have edited Sidebar!";

            //переадресацие пользователя
            return RedirectToAction("EditSidebar");
        }
    }


}