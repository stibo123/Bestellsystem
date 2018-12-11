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
using MailKit;
using MimeKit;
using MailKit.Net.Smtp;

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
            //AddExampleUsers();
            //AddRoles();
            return View(LoggedIn());
        }

        public string LoggedIn()
        {
            var username = GetUsernameFromCookie();
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

        public IActionResult ResetPassword()
        {
            string page = LoggedIn();
            if (page != "Index") return View(page);
            try
            {
                string username = Request.Form["username"];
                string email = Request.Form["email"];

                User user = db.Users.Single(x => x.Username == username);
                if(user.Email == email)
                {
                    var newpassword = GeneratePassword(8);
                    if(sendEmail(newpassword, user))
                    {
                        db.Users.Single(x => x.Username == username).Password = GenerateSHA512String(newpassword);
                        db.SaveChanges();
                        ViewBag.Reset = $"Passwort an {email} gesendet";
                        return View("Index");
                    }
                    else
                    {
                        return View("Reset");
                    }
                    
                }
                else
                {
                    ViewBag.Reset = "Falsche Emailadresse";
                }
            }
            catch
            {
                ViewBag.Reset = "Falscher Username/Fehler";
            }
            return View("Reset");
        }

        public bool sendEmail(string password, User user)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Bestellsystem Aspöck", "bestellsystemaspoeck@gmail.com"));
                message.To.Add(new MailboxAddress($"{user.Firstname} {user.Lastname}", user.Email));
                message.Subject = "Neues Passwort";
                message.Body = new TextPart("plain")
                {
                    Text = $"Passwort: {password}"
                };

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("bestellsystemaspoeck@gmail.com", "aspoeck123bestellsystem456");
                    client.Send(message);
                    client.Disconnect(true);
                }
                return true;
            }
            catch(Exception e)
            {
                ViewBag.Reset = $"Fehler {e.Message}";
                return false;
            }
        }

        public List<Product> GetProductsFromSupplier(Category category, User user)
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

        public String GetUsernameFromCookie()
        {
            return HttpContext.Session.GetString("username");
        }

        public IActionResult Admin()
        {
            
            return View();
        }
        public IActionResult Employee()
        {
            EmployeeModel model = new EmployeeModel()
            {
                Supplier = db.Users.Where(x => x.Role.Name == "Supplier").ToList(),
                Categories = db.Categories.ToList(),
                Products = GetProductsFromSupplier(null, null)
            };

            return View(model);
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


        public void AddRoles()
        {
            Role admin = new Role { Name = "Admin" };
            Role employee = new Role { Name = "Employee" };
            Role supplier = new Role { Name = "Supplier" };
            db.Roles.Add(admin);
            db.Roles.Add(employee);
            db.Roles.Add(supplier);
            db.SaveChanges();
        }

        public void AddExampleUsers()
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

            User test = new User
            {
                Firstname = "Sebbi",
                Lastname = "Frei",
                Email = "testtesttesttesttest123123123huans3xyq@gmail.com",
                Username = "test",
                Role = db.Roles.Single(x => x.Name == "Employee"),
                Password = GenerateSHA512String("test")
            };


            db.Users.Add(admin);
            db.Users.Add(employee);
            db.Users.Add(supplier);
            db.Users.Add(test);
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
