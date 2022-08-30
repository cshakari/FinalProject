using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Code.Data;
using Code.Models;
using Code.Services;
using System.Security.Claims;
using MimeKit;
using Microsoft.AspNetCore.Http;

namespace Code.Controllers
{
    public class UsersController : Controller
    {
        private readonly LoginService _loginService;
        private static User user;
        public UsersController(LoginService loginService)
        {
            _loginService = loginService;
        }

        // GET: Users
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            string userName = HttpContext.Session.GetString("userName");
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["UserNameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "user_name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["ActivationSortParm"] = String.IsNullOrEmpty(sortOrder) ? "activation" : "";
            ViewData["ActivationDateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "activation_date_desc" : "";
            ViewData["UpdatedDateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "updated_date_desc" : "";
            ViewData["RoleSortParm"] = String.IsNullOrEmpty(sortOrder) ? "role" : "";
            ViewData["VisibilityParm"] = String.IsNullOrEmpty(sortOrder) ? "visibility_desc" : "";
            ViewData["CurrentFilter"] = searchString;

            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "Users");
            }
            else
            {
                user = _loginService.Find(userName);
            }

            if (user.UserRole != "Admin")
            {
                TempData["error"] = "Unauthorized.";
                return RedirectToAction("Index", "Author");
            }
            var users = from s in _loginService.Read()
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                users = _loginService.ReadByName(searchString);
            }
            switch (sortOrder)
            {
                case "name_desc":
                    users = users.OrderBy(s => s.Name);
                    break;
                case "Date":
                    users = users.OrderByDescending(s => s.ActivatedAt);
                    break;
                case "date_desc":
                    users = users.OrderByDescending(s => s.CreatedAt);
                    break;
                case "user_name_desc":
                    users = users.OrderBy(s => s.UserName);
                    break;
                case "activation":
                    users = users.OrderBy(s => s.IsActive);
                    break;
                case "activation_date_desc":
                    users = users.OrderByDescending(s => s.ActivatedAt);
                    break;
                case "updated_date_desc":
                    users = users.OrderByDescending(s => s.UpdatedAt);
                    break;
                case "role":
                    users = users.OrderBy(s => s.UserRole);
                    break;
                case "visibility_desc":
                    users = users.OrderByDescending(s => s.Visibility);
                    break;
                default:
                    //users = users.OrderBy(s => s.Name);
                    break;
            }

            //return View(await users.AsNoTracking().ToListAsync());
            return View(users);
        }

        public async Task<IActionResult> Login(string userName, string password, bool rememberMe)
        {

            if (TempData["message"] != null)
            {
                ViewData["message"] = TempData["message"];
            }

            if (TempData["errorMessage"] != null)
            {
                ViewData["errorMessage"] = TempData["errorMessage"];
            }

            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                var user = _loginService.Find(userName);

                // check account found, verify password, check account active or not
                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password) || !user.IsActive)
                {
                    // authentication failed
                    ViewData["errorMessage"] = "Login failed. Wrong Email/Password or Account is not activated.";
                    return View();
                }
                else
                {
                    // authentication successful
                    if(rememberMe == true)
                    {
                        Response.Cookies.Append("HillstreetBooksId", userName);
                        Response.Cookies.Append("HillstreetBooksPass", password);
                    }
                    else
                    {
                        if(Request.Cookies["HillstreetBooksId"] != null && Request.Cookies["HillstreetBooksPass"] != null)
                        {
                            Response.Cookies.Delete("HillstreetBooksId");
                            Response.Cookies.Delete("HillstreetBooksPass");
                        }
                    }

                    await HttpContext.Session.LoadAsync();
                    HttpContext.Session.SetString("userName", userName);
                    HttpContext.Session.SetString("userRole", user.UserRole);
                                     
                    if (user.Author != null) // go to author profile if they already have a author page set up
                    {
                        HttpContext.Session.SetString("profileUrl", user.Author.ProfileUrl);
                        return RedirectToAction("Index", "Author", new { id = user.Author.ProfileUrl });
                    }
                    else
                    {
                        return RedirectToAction("Edit", "Author", new { userName = userName });
                    }

                }
            }
            else
            {
                return View();
            }
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("userName");
            HttpContext.Session.Remove("userRole");
            HttpContext.Session.Remove("profileUrl");
            return RedirectToAction(nameof(Index));

        }


        // GET: Users/Create
        public IActionResult Create()
        {
            if (TempData["message"] != null)
            {
                ViewData["message"] = TempData["message"];
            }
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserName,Password,PasswordConfirmation,UserRole,Name,CreatedAt,ActivatedAt,LastUpdated")] User user)
        {
            User existingUser = new User();
            if (UserExists(user.UserName, ref existingUser))
            {
                ModelState.AddModelError("UserName", "This email has already registered.");
            }

            if (ModelState.IsValid)
            {
                //user.UserId = (user.UserName).Split('@')[0];
                if (existingUser != null && !string.IsNullOrEmpty(existingUser.UserName))
                {
                    existingUser.UserName = user.UserName;
                    existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password); ;
                    existingUser.IsActive = true;
                    existingUser.CreatedAt = DateTime.UtcNow;
                    existingUser.Name = user.Name;
                    existingUser.UserRole = "Author";

                    _loginService.Update(existingUser);
                }
                else
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                    user.IsActive = false;
                    user.UserRole = "Author";
                    user.CreatedAt = DateTime.UtcNow;
                    _loginService.Create(user);
                }

                var token = UserHelper.CreateToken(user.UserName);

                //Prepare email to send user
                //string link = $"{Environment.GetEnvironmentVariable("BASE_URL")}/Users/ConfirmAccount?token={token}";
                string link = $"hsbooksapi.azurewebsites.net/Users/ConfirmAccount?token={token}";
                
                MailboxAddress userMailboxAddress = new MailboxAddress(user.Name, user.UserName);

                EmailHelper.SendEmail(userMailboxAddress, EmailHelper.EMAIL_CONFIRMATION_SUBJECT, EmailHelper.GetEmailConfirmationBody(user.Name, link));

                TempData["message"] = $"Just one more step! <br> An email has sent to you. <br>Please click the link in you email to activate your account.";
                return RedirectToAction(nameof(Create));
            }
            return View();
        }

        public async Task<IActionResult> ConfirmAccount(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["errorMessage"] = "Invalid activate link.";

                return RedirectToAction(nameof(Login));
            }
            else
            {
                var payload = UserHelper.ReadToken(token);
                string username = payload.Where(p => p.Key.Equals("username")).First().Value.ToString();
                string strRequestedTime = payload.Where(p => p.Key.Equals("requestedTime")).First().Value.ToString();
                var totalTime = DateTime.UtcNow - DateTime.Parse(strRequestedTime);
                if (totalTime.TotalMinutes <= 60)
                {
                    var user = _loginService.Find(username);
                    if (user != null)
                    {
                        user.IsActive = true;
                        user.ActivatedAt = DateTime.UtcNow;
                        _loginService.Update(user);
                    }
                    else
                    {
                        return NotFound();
                    }

                    TempData["message"] = "Your account is activated! Click Login to explore more!";
                    return RedirectToAction(nameof(Login), new { userName = user.UserName });

                }
                else
                {
                    TempData["errorMessage"] = "The activate link is expired";
                    return RedirectToAction(nameof(Login));
                }
            }
        }

        public IActionResult ForgotPassword()
        {
            if (TempData["message"] != null)
            {
                ViewData["message"] = TempData["message"];
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string userName)
        {
            User existingUser = new User();

            if (!UserExists(userName, ref existingUser))
            {
                ModelState.AddModelError("UserName", "This email is not registered.");
            }
            if (ModelState.IsValid)
            {
                User user = new User();
                user = _loginService.Find(userName);
                if (user == null)
                {
                    TempData["message"] = $"If the username exists, a password reset link will be sent to you.";
                    return RedirectToAction(nameof(Login));
                }
                var token = UserHelper.CreateToken(user.UserName);
                //string link = $"{Environment.GetEnvironmentVariable("BASE_URL")}/Users/PasswordReset?token={token}";
                string link = $"hsbooksapi.azurewebsites.net/Users/PasswordReset?token={token}";

                MailboxAddress userMailboxAddress = new MailboxAddress(user.Name, user.UserName);

                EmailHelper.SendEmail(userMailboxAddress, EmailHelper.EMAIL_FORGOT_PASSWORD, EmailHelper.GetPasswordResetBody(user.Name, link));

                TempData["message"] = $"If the username exists, a password reset link will be sent to you.";
                return RedirectToAction(nameof(Login));
            }
            return View();
        }

        public IActionResult PasswordReset(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["errorMessage"] = "Invalid reset link.";

                return RedirectToAction(nameof(Login));
            }
            else
            {
                var payload = UserHelper.ReadToken(token);
                string username = payload.Where(p => p.Key.Equals("username")).First().Value.ToString();
                string strRequestedTime = payload.Where(p => p.Key.Equals("requestedTime")).First().Value.ToString();
                var totalTime = DateTime.UtcNow - DateTime.Parse(strRequestedTime);
                if (totalTime.TotalMinutes <= 60)
                {
                    var user = _loginService.Find(username);
                    return View(user);

                }

            }
            TempData["errorMessage"] = "The reset link is expired";
            return RedirectToAction(nameof(Login));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PasswordReset(User user)
        {
            if (ModelState.IsValid)
            {
                User editingUser = _loginService.Find(user.UserName);
                editingUser.Password = user.Password;
                editingUser.Password = BCrypt.Net.BCrypt.HashPassword(editingUser.Password);
                _loginService.Update(editingUser);
                TempData["message"] = $"Your password has been reset.";
                return RedirectToAction(nameof(Login));
            }
            return View();
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                TempData["error"] = "The user does not exist";
                return RedirectToAction("Index", "Users");
            }
            string userName = HttpContext.Session.GetString("userName");
            var admin = _loginService.Find(userName);
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "Users");
            }
            else
            {
                user = _loginService.FindById(id);
            }

            if (admin.UserRole != "Admin")
            {
                return RedirectToAction("Login", "Users");
            }
            List<SelectListItem> RoleList = new List<SelectListItem>()
            {
                new SelectListItem(){Value="Admin",Text="Admin"},
                new SelectListItem(){Value="Author",Text="Author"}
            };
            TempData["Author"] = true;
            ViewBag.RoleList = RoleList;
            
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind] User user)
        {
            string userName = HttpContext.Session.GetString("userName");
            var currentUser = _loginService.Find(userName);
            var manageUser = _loginService.FindById(id);
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "Users");
            }
            if (currentUser.UserRole != "Admin")
            {
                TempData["error"] = "Unauthorized.";
                return RedirectToAction("Index", "Author");
            }
            user.Author = manageUser.Author;
            user.IsActive = manageUser.IsActive;
            user.PasswordConfirmation = user.Password;
            user.Name = manageUser.Name;

            _loginService.Update(user);

            return RedirectToAction(nameof(Index));
        }

        //// GET: Users/Delete/5
        //public async Task<IActionResult> Delete(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var user = await _context.User
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(user);
        //}

        // POST: Users/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            _loginService.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string userName, ref User existingUser)
        {
            existingUser = _loginService.Find(userName);

            return existingUser != null && !string.IsNullOrEmpty(existingUser.UserName) && existingUser.IsActive;
        }

        /*[HttpPost]
        [ValidateAntiForgeryToken]*/
        public async Task<IActionResult> EditVisible(string id)
        {
            string userName = HttpContext.Session.GetString("userName");
            var currentUser = _loginService.Find(userName);
            var manageUser = _loginService.FindById(id);
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "Users");
            }
            if (currentUser.UserRole != "Admin")
            {
                TempData["error"] = "Unauthorized.";
                return RedirectToAction("Index", "Author");
            }

            manageUser.Visibility = true;
            _loginService.Update(manageUser);

            return RedirectToAction(nameof(Index));
        }

        /*[HttpPost]
        [ValidateAntiForgeryToken]*/
        public async Task<IActionResult> EditInvisible(string id)
        {
            string userName = HttpContext.Session.GetString("userName");
            var currentUser = _loginService.Find(userName);
            var manageUser = _loginService.FindById(id);
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "Users");
            }
            if (currentUser.UserRole != "Admin")
            {
                TempData["error"] = "Unauthorized.";
                return RedirectToAction("Index", "Author");
            }

            manageUser.Visibility = false;
            _loginService.Update(manageUser);

            return RedirectToAction(nameof(Index));
        }
    }
}
