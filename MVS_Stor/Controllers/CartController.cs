using MVS_Stor.Models.Data;
using MVS_Stor.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MVS_Stor.Controllers
{
    public class CartController : Controller
    {
        // урок 20
        // GET: Cart
        public ActionResult Index()
        {

            //обьявляем лист типа CartVM
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            //проверяем пуста ли карзина
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty.";
                return View();
            }
            //складываем сумму и записываемм во ViewBag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total+= item.Total;
            }

            ViewBag.GrandTotal = total;
            //возвращяем лист в преедставление
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            //обьявляем модель cartVM
            CartVM model = new CartVM();

            //оьявляем переменую количества
            int qty = 0;

            //обьявляем переменую цены
            decimal price = 0m;

            //проверяем сессию корзины 
            if (Session["cart"] != null)
            {
                //получаем общее количество товаров и цену
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                //или устанавливаем количество и цену в 0
                model.Quantity= 0;
                model.Price= 0m;
            }
            
            //возвращяем  частичное представление с моделью
            return PartialView("_CartPartial", model);
        }

        // урок 21
        public ActionResult AddToCartPartial(int id)
        {
            //обьявляем лист, параматризированный типом CartVM
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //обьявляем модель CartVM
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                //получаем продукт по Id
                ProductDTO product = db.Products.Find(id);

                //делаем проверку находиться ли товар уже в корзине
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                //если нет то добавляем новый товар в корзину
                if(productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id, 
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                //если да то добавляем единицу товара
                else
                {
                    productInCart.Quantity++;
                }
            }
            //получаем общяе количесто, цену, и добавляем данные в модель
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            //сохраняем состаяние корзины в сессию
            Session["cart"] = cart;

            //возврящяем частичное представление с моделью
            return PartialView("_AddToCartPartial", model);
        }

        //урок 21
        // get: /Cart/IncrementProduct
        public JsonResult IncrementProduct( int productId)
        {
            //обьявляем лист Cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //получаем модель CartVM из листа
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //добавляем количества
                model.Quantity++;

                //сохраняем необходимые данные
                var result = new {qty = model.Quantity, price = model.Price};

                //возврящяем JSON ответ с данными
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        //урок 22
        // get: /Cart/DecrementProduct
        public ActionResult DecrementProduct( int productId)
        {
            //обьявляем лист Cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //получаем модель CartVM из листа
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //отнимаем количества
                if(model.Quantity > 1)
                    model.Quantity--;
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                //сохраняем необходимые данные
                var result = new { qty = model.Quantity, price = model.Price };

                //возврящяем JSON ответ с данными
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        //урок 22
        // get: /Cart/DecrementProduct
        public void RemoveProduct(int productId)
        {
            //обьявляем лист Cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //получаем модель CartVM из листа
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                cart.Remove(model);
            }
        }

        //урок 26
        // get: /Cart/PaypalPartial
        public ActionResult PaypalPartial()
        {
            //получаем лист в корзине 
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //возвращяем частичное представление с листом 
            return PartialView(cart);
        }

        //урок 26
        // Post: /Cart/PlaceOrder
        [HttpPost]
        public void PlaceOrder()
        {
            //получаем список товаров
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //получаем имя пользователя
            string userName = User.Identity.Name;

            //обьявляем переменную для orderId
            int orderId = 0;

            using (Db db = new Db())
            {
                //обьявляем модель OrderDTO
                OrderDTO orderDTO = new OrderDTO();

                //получаем id пользователя
                var q = db.Users.FirstOrDefault(x => x.Username == userName);

                int userId = q.Id;

                //заполняем модель OrderDTO данными и сохраняем
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;


                db.Orders.Add(orderDTO);
                db.SaveChanges();

                //получаем orderId
                orderId = orderDTO.OrderId;

                //обьявляем модель OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                //добавляем данные
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);
                    db.SaveChanges();
                }

            }
            //отправляем письмо о заказе на почту администратора 
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("d4bca8448be513", "6eb5b240c5ad98"),
                EnableSsl = true
            };
            client.Send("shop@example.com", "willyosho3@gmail.com", "New Order", $"you have a new order. Order number: {orderId}");

            //обновляем сессию
            Session["cart"] = null;
        }
    }
}