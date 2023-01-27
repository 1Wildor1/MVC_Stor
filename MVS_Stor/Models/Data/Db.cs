using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;

namespace MVS_Stor.Models.Data
{
    public class Db:DbContext
    {
        public DbSet<PagesDTO> Pages { get; set; }
        //(урок 6)
        public DbSet<SidebarDTO> Sidebars { get; set; }

        //подключение таблицы Categoies(урок 8)
        public DbSet<CategoryDTO> Categories { get; set; }

        //(урок 11)
        public DbSet<ProductDTO> Products { get; set; }

        //(урок 22)
        public DbSet<UserDTO> Users { get; set; }

        //(урок 22)
        public DbSet<RoleDTO> Roles { get; set; }

        //(урок 23)
        public DbSet<UserRoleDTO> UserRoles { get; set; }

        //(урок 25)
        public DbSet<OrderDTO> Orders { get; set; }

        //(урок 25)
        public DbSet<OrderDetailsDTO> OrderDetails { get; set; }

    }
}