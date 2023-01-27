using MVS_Stor.Models.Data;
using MVS_Stor.Models.ViewModels.Pages;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVS_Stor.Controllers
{
    public class PagesController : Controller
    {
        //17
        // GET: Index/{page}
        public ActionResult Index(string page = "")
        {
            // Получаем/утсанавливаем краткий заголовок(СЛАГ)
            if (page == "")
                page = "home";

            // Обьявляем модель и классс ДТО
            PageVM model;
            PagesDTO dto;
            //Проверка на доступность текущей страницы
            using (Db db = new Db())
            {
                if (!db.Pages.Any(x => x.Slug.Equals(page)))
                {
                    return RedirectToAction("Index", new { page = "" });
                }
            }
            // ПОлучаем DTO Страницы
            using (Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }

            //Устанавливаем  заголовок страницы (тайтл)
            ViewBag.PageTitle = dto.Title;

            //проверяем боковую панель
            if (dto.HasSidebar == true)
            {
                ViewBag.Sidebar = "Yes";
            }
            else
            {
                ViewBag.Sidebar = "No";
            }

            //заполняем модель данными
            model = new PageVM(dto);

            //возвращаем представление с моделью
            return View(model);
        }

        public ActionResult PagesMenuPartial()
        {
            // иницаилизируем лист пейдж вм
            List<PageVM> pageVMList;
            // получаем все страницы кроме Хоум
            using (Db db = new Db())
            {
                pageVMList = db.Pages.ToArray()
                    .OrderBy(x => x.Sorting)
                    .Where(x => x.Slug != "home")
                    .Select(x => new PageVM(x))
                    .ToList();
            }
            // возвращаем частичное представление с листом данными

            return PartialView("_PagesMenuPartial", pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            //обьявляем модель 
            SidebarVM model;

            //инициализируем модель
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebars.Find(1);

                model = new SidebarVM(dto);
            }

            //возвращяем модель в честичное представление 

            return PartialView("_SidebarPartial",model);
        }
    }
}