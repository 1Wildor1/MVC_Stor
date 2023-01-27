using MVS_Stor.Models.Data;
using MVS_Stor.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVS_Stor.Controllers
{
    public class ShopController : Controller
    {
        //урок 18
        // GET: Shop
        public ActionResult Index()
        {

            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            //обьявляем модель  типа List<> CategoryVM
            List<CategoryVM> categoryVMList;

            //инизиализируем модель данными
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }

            //возвращяем частичное представление с моделью 
            return PartialView("_CategoryMenuPartial", categoryVMList);
        }

        //урок 19
        // GET: Shop/Catrgory/name
        public ActionResult Category(string name)
        {

            //обьявляем список типа list
            List<ProductVM> productVMList;

            using (Db db = new Db())
            {
                //получаем id категории
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();

                int catId = categoryDTO.Id;

                //инициализируем список данными 
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                //получаем имя категории
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();

                //проверка на NULL
                if(productCat == null)
                {
                    var catName = db.Categories.Where(x => x.Slug == name).Select(x => x.Name).FirstOrDefault();
                    ViewBag.CategoryName = catName;

                }
                else
                {
                    ViewBag.CategoryName = productCat.CategoryName;

                }
            }
            //возвращяем модель с представлением 
            return View(productVMList);
        }

        //урок 19
        // GET: Shop/product-details/name
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            //оьявляем модели DTO и VM
            ProductDTO dto;
            ProductVM model;

            //инициализируем Id продукта
            int id = 0;

            using (Db db = new Db())
            {
                //проверяем доступен ли продукт
                if(!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }
                //инициализируем модель DTO данными 
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                //получаем Id
                id = dto.Id;

                //инициалезируем модель VM данными
                model = new ProductVM(dto);
            }   
            //получаем изображения из галереи
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs")).Select(fn => Path.GetFileName(fn));

            //возвращяем модель в представление
            return View("ProductDetails", model);
        }
    }
}