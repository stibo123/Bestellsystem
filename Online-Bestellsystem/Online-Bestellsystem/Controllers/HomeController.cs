using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Database;
using Microsoft.AspNetCore.Mvc;
using Online_Bestellsystem.Models;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Http;

namespace Online_Bestellsystem.Controllers
{
    public class HomeController : Controller
    {
        Context db;
        public HomeController(Context db)
        {
            this.db = db;

        }
        public IActionResult Index()
        {
            //addExampleUsers();
            //addRoles();
            return View(LoggedIn());
        }

        public string LoggedIn()
        {
            var username = getUsernameFromCookie();
            if (username != null && username != "")
            {
                try
                {
                    User user = db.Users.Single(x => x.Username == username);
                    return user.Role.Name;
                }
                catch
                {
                    return "Index";
                }
            }
            return "Index";
        }

        public List<Product> getProductsFromSupplier(Category category, User user)
        {
            List<Product> products = new List<Product> { };
            try{
                if (category != null)
                {
                    if(user!=null)
                    {
                        products = db.SupplierProperties.Single(x => x.User == user).Products.Where(x => x.Category==category).ToList();
                    }
                    else
                    {
                        foreach(var property in db.SupplierProperties)
                        {
                            products.AddRange(property.Products.Where(x => x.Category == category).ToList());
                        }
                    }
                }
                else
                {
                    if (user != null)
                    {
                        products = db.SupplierProperties.Single(x => x.User == user).Products.ToList();
                    }
                    else
                    {
                        foreach (var property in db.SupplierProperties)
                        {
                            products.AddRange(property.Products.ToList());
                        }
                    }
                }
                
            }
            catch{
                foreach (var property in db.SupplierProperties)
                {
                    products.AddRange(property.Products.ToList());
                }
            }
            return products;
        }

        public String getUsernameFromCookie()
        {
            return HttpContext.Session.GetString("username");
        }

        public IActionResult Admin()
        {
            
            return View();
        }
        public IActionResult Employee()
        {
            return View("Employee");
        }
        public IActionResult Supplier()
        {
            
            return View();
        }

        public IActionResult Reset()
        {
            
            return View();
        }

        public IActionResult Login()
        {
            string page = LoggedIn();
            if (page != "Index") return View(page);
            try
            {
                    string username = Request.Form["username"];
                    string password = Request.Form["password"];

                    User user = null;
                    try
                    {
                        user = db.Users.Single(x => x.Username == username);
                        if (user != null && user.Password == GenerateSHA512String(password))
                        {
                            HttpContext.Session.SetString("username", user.Username);
                            HttpContext.Session.SetString("firstlogin", DateTime.Now.ToString()); 
                            return View(user.Role.Name);
                        }
                    }
                    catch
                    {
                        return View("Index");
                    }
            }
            catch
            {
                return View("Index");
            }
            return View("Index");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.SetString("username", "");
            HttpContext.Session.SetString("firstlogin","");
            return View("Index");
        }

        public static string GenerateSHA512String(string inputString)
        {
            SHA512 sha512 = SHA512Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha512.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }

        private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }

        public String GeneratePassword(int length)
        {
            string digits = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var password = "";
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                password += digits[random.Next(digits.Length)];
            }
            return password;
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public void addRoles()
        {
            Role admin = new Role { Name = "Admin" };
            Role employee = new Role { Name = "Employee" };
            Role supplier = new Role { Name = "Supplier" };
            db.Roles.Add(admin);
            db.Roles.Add(employee);
            db.Roles.Add(supplier);
            db.SaveChanges();
        }

        public void addExampleUsers()
        {
            foreach (var user in db.Users)
            {
                db.Users.Remove(user);
            }
            foreach (var propertie in db.SupplierProperties)
            {
                db.SupplierProperties.Remove(propertie);
            }
            foreach (var product in db.Products)
            {
                db.Products.Remove(product);
            }
            foreach (var categorie in db.Categories)
            {
                db.Categories.Remove(categorie);
            }

            db.SaveChanges();
            
            User admin = new User
            {
                Firstname = "Heimo",
                Lastname = "Schusterzucker",
                Email = "schuzu@gmail.com",
                Username = "admin",
                Role = db.Roles.Single(x => x.Name == "Admin"),
                Password = GenerateSHA512String("password")
            };
            User employee = new User
            {
                Firstname = "Stefan",
                Lastname = "Bartos",
                Email = "stibo123@gmail.com",
                Username = "stibo",
                Role = db.Roles.Single(x => x.Name == "Employee"),
                Password = GenerateSHA512String("password123")
            };
            User supplier = new User
            {
                Firstname = "Sebbi",
                Lastname = "Frei",
                Email = "sebbi@gmail.com",
                Username = "sebbi",
                Role = db.Roles.Single(x => x.Name == "Supplier"),
                Password = GenerateSHA512String("password321")
            };
            

            db.Users.Add(admin);
            db.Users.Add(employee);
            db.Users.Add(supplier);
            db.SaveChanges();

            SupplierProperties properties = new SupplierProperties
            {
                CompanyName = "Volt Studios",
                DeadLine = "11:00",
                DeliveryDates = new List<DateTime> { DateTime.ParseExact("24.12.2018 23:00:00", "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) },
                WantEmail = true,
                User = db.Users.Single(x => x.Username == "sebbi"),
                Products = new List<Product> { new Product { Name = "Cola", Price = 1.5, Category = new Category { Name = "Getränk" }, ProductImage= new ProductImage { } } }
            };

            db.SupplierProperties.Add(properties);

            db.SaveChanges();

        }
        }
    }
