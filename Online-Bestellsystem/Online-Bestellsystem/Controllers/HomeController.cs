using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Database;
using Microsoft.AspNetCore.Mvc;
using Online_Bestellsystem.Models;
using Microsoft.AspNetCore.Http;
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
            //AddRoles();
            //DeleteExampleData();
            //AddExampleData();
            string rolename = LoggedIn();
            CheckModel(rolename);
            return View(rolename);
        }

        public void CheckModel(string rolename)
        {
            if (rolename == "Employee") Employee();
            else if (rolename == "Admin") Admin();
            else if (rolename == "Supplier") Supplier();
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
                if (user.Email == email)
                {
                    var newpassword = GeneratePassword(8);
                    var title = "Neues Passwort";
                    var text = new TextPart("plain")
                    {
                        Text = $"Passwort: {newpassword}"
                    };
                    if (SendEmail(title, text, user))
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

        public bool SendEmail(string title, TextPart text, User user)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Bestellsystem Aspöck", "bestellsystemaspoeck@gmail.com"));
                message.To.Add(new MailboxAddress($"{user.Firstname} {user.Lastname}", user.Email));
                message.Subject = title;
                message.Body = text;

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("bestellsystemaspoeck@gmail.com", "aspoeck123bestellsystem456");
                    client.Send(message);
                    client.Disconnect(true);
                }
                return true;
            }
            catch (Exception e)
            {
                ViewBag.Reset = $"Es ist ein Fehler aufgetreten :(";
                SaveException(e.Message);
                return false;
            }
        }

        public void SaveException(string problem)
        {
            db.Exceptionsx.Add(new Exceptionx { Problem = problem, Date = DateTime.Now });
            db.SaveChanges();
        }

        public List<Product> GetProductsFromSupplier(Category category, User user)
        {
            List<Product> products = new List<Product> { };
            try
            {
                if (category != null)
                {
                    if (user != null)
                    {
                        products = db.SupplierProperties.Single(x => x.User == user).Products.Where(x => x.Category == category).ToList();
                    }
                    else
                    {
                        products = db.Products.Where(x => x.Category == category).ToList();
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
                        products = db.Products.ToList();
                    }
                }

            }
            catch (Exception e)
            {
                products = db.Products.ToList();
                SaveException(e.Message);
            }
            return products;
        }

        public String GetUsernameFromCookie()
        {
            return HttpContext.Session.GetString("username");
        }
        public IActionResult Supplier()
        {
            string page = LoggedIn();
            if (page != "Supplier") return View(page);

            return View();
        }
        public IActionResult Admin()
        {
            string page = LoggedIn();
            if (page != "Admin") return View(page);

            return View();
        }
        public IActionResult Employee()
        {
            string page = LoggedIn();
            if (page != "Employee") return View(page);

            var model = GetEmployeeModel();
            return View(model);
        }

        public EmployeeModel GetEmployeeModel()
        {
            EmployeeModel model = new EmployeeModel()
            {
                SupplierProperties = db.SupplierProperties.ToList(),
                Categories = db.Categories.ToList(),
                Products = GetProductsFromSupplier(null, null)
            };
            return model;
        }

        public IActionResult Reset()
        {
            return View();
        }

        public void GenerateUser(List<String> emails)
        {
            foreach (var email in emails)
            {
                Console.WriteLine(db.Users.Where(x => x.Email == email).Count());
                if (db.Users.Where(x => x.Email == email).Count() == 0)
                {
                    var username = email.Split('@')[0];
                    if (db.Users.Where(x => x.Username == username).Count() > 0)
                    {
                        username = username + GeneratePassword(8);
                    }
                    var firstname = "vorname";
                    var lastname = "lastname";
                    var password = GeneratePassword(8);

                    var user = new User
                    {
                        Firstname = firstname,
                        Lastname = lastname,
                        Email = email,
                        Password = GenerateSHA512String(password),
                        Username = username,
                        Role = db.Roles.Single(x => x.Name == "Employee")
                    };
                    db.Users.Add(user);
                    db.SaveChanges();
                    var text = new TextPart("plain")
                    {
                        Text = $@"
Dein Administrator hat einen Account für dich erstellt. 
Email:  {email}
Username: {username}
Passwort: {password}
Ändere bitte alsbald deinen Usernamen und Passwort."
                    };
                    SendEmail("Bestellsystem Aspöck - Account", text, user);
                }
            }
        }

        public IActionResult Login()
        {
            string rolename = LoggedIn();
            CheckModel(rolename);
            if (rolename != "Index") return View(rolename);

            try
            {
                string username = Request.Form["username"];
                string password = Request.Form["password"];

                User user = null;
                user = db.Users.Single(x => x.Username == username);
                if (user != null && user.Password == GenerateSHA512String(password))
                {
                    HttpContext.Session.SetString("username", user.Username);
                    HttpContext.Session.SetString("firstlogin", DateTime.Now.ToString());
                    CheckModel(user.Role.Name);
                    return View(user.Role.Name);
                }
                else
                {
                    ViewBag.Reset = "Falsches Passwort";
                }
            }
            catch
            {
                ViewBag.Reset = "Falscher Username";
            }
            return View("Index");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.SetString("username", "");
            HttpContext.Session.SetString("firstlogin", "");
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

        public void DeleteExampleData()
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
        }

        public void AddExampleData()
        {

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

            User musti = new User
            {
                Firstname = "Musti",
                Lastname = "Fleisch",
                Email = "weirdflexbotok@gmail.com",
                Username = "musti",
                Role = db.Roles.Single(x => x.Name == "Supplier"),
                Password = GenerateSHA512String("musti123")
            };


            db.Users.Add(admin);
            db.Users.Add(employee);
            db.Users.Add(supplier);
            db.Users.Add(test);
            db.Users.Add(musti);
            db.SaveChanges();

            SupplierProperties volt = new SupplierProperties
            {
                CompanyName = "Volt Studios",
                DeadLine = "11:00",
                DeliveryDates = new List<DateTime> { DateTime.ParseExact("24.12.2018 23:00:00", "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) },
                WantEmail = true,
                User = db.Users.Single(x => x.Username == "sebbi"),
                Products = new List<Product> { new Product { Name = "Cola", Price = 1.5, Category = new Category { Name = "Getränk" }, ProductImage = new ProductImage { } } }
            };

            SupplierProperties kebap = new SupplierProperties
            {
                CompanyName = "Kepab",
                DeadLine = "12:00",
                DeliveryDates = new List<DateTime> { DateTime.ParseExact("24.12.2018 23:00:00", "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) },
                WantEmail = true,
                User = db.Users.Single(x => x.Username == "musti"),
                Products = new List<Product> {
                    new Product { Name = "Kebap", Price = 3.5, Category = new Category { Name = "Kebap" },ProductImage = new ProductImage { } } ,
                    new Product { Name = "Fanta", Price = 1.5, Category = new Category { Name = "Fanti" }, ProductImage = new ProductImage { } }
                     }
            };

            db.SupplierProperties.Add(volt);
            db.SupplierProperties.Add(kebap);

            db.SaveChanges();

        }
    }
}
