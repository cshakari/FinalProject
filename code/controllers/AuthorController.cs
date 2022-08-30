using Code.Models;
using Code.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Web;
using System.Drawing;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using MimeKit;

namespace Code.Controllers
{
    public class AuthorController : Controller
    {
        private const string FILE_SAVING_TEMP_FOLDER = "files";
        private const string AUTHOR_IMAGE_SAVING_TEMP_FOLDER = @"images\Author";
        private const string BOOK_COVER_IMAGE_SAVING_TEMP_FOLDER = @"images\bk_cv_img";

        private const string CLOUD_NAME = "dx4hle3qm";
        private const string API_KEY = "297654582183183";
        private const string API_SECRET = "1bzaIc28OtwHgHtSraBilCDpDT0";

        //private readonly AuthorService _authorService;
        //private readonly BookService _bookService;
        private readonly LoginService _userService;
        private IHostingEnvironment Environment;
        private static User user;
        private static bool isCreatingNew = false;

        public AuthorController(LoginService loginService, IHostingEnvironment environment)
        {
            //_authorService = authorService;
            //_bookService = bookService;
            _userService = loginService;
            Environment = environment;

        }

        public IActionResult Index(string id)
        {
            string userName = HttpContext.Session.GetString("userName");
            var allUsers = _userService.Read(); // all the users
            var usersWithProfile = allUsers.Where(x => x.Author != null); // users that have author profile set up
            var authorWithQuery = usersWithProfile.FirstOrDefault(x => x.Author.ProfileUrl == id); // author who's profile url matches the query string
            var userWithSession = allUsers.FirstOrDefault(x => x.UserName == userName && x.Author == null); // the user who is currently logged in but has no author profile set up
            var authorWithSession = usersWithProfile.FirstOrDefault(x => x.UserName == userName); // the author who is currently logged in and has a profile set up

            if (userWithSession != null && authorWithQuery != null && userWithSession.UserName != authorWithQuery.UserName) // if user is logged in and looking for an authors profile
            {
                if (authorWithQuery.Visibility != true && userWithSession.UserRole != "Admin")
                {
                    TempData["error"] = "The profile is not public";
                    return RedirectToAction("Edit", "Author");
                }
                return View(authorWithQuery.Author);
            }
            else if(userWithSession != null && authorWithSession == null && authorWithQuery == null)//user trying to access a their author profile when it is not set up
            {
                TempData["error"] = "The profile is not set up yet";
                return RedirectToAction("Edit", "Author");
            }
            else if(authorWithSession == authorWithQuery && authorWithSession != null && authorWithQuery != null) // if author thats logged in tried to access their own page
            {
                return View(authorWithSession.Author);                  
            }
            else if(authorWithSession != null && authorWithQuery != null && authorWithQuery != authorWithSession)// if author is logged in and tried to view another authors page
            {        
                if (authorWithQuery.Visibility != true && authorWithSession.UserRole != "Admin")
                {
                    TempData["error"] = "The profile is not public";
                    return RedirectToAction("Index", "Author");
                }
                return View(authorWithQuery.Author);
            }
            else if(authorWithQuery != null && authorWithSession == null && userWithSession == null) // not logged in but trying to view an author
            {
                if (authorWithQuery.Visibility != true)
                {
                    TempData["error"] = "The profile is not public";
                    return RedirectToAction("Login", "Users");
                }
                return View(authorWithQuery.Author);
            }
            else
            {
                TempData["error"] = "The profile does not exist";
                return RedirectToAction("Login", "Users");                
            }

            
        }

        [HttpPost]
        public IActionResult UploadImage(string file)
        {
            string userName = HttpContext.Session.GetString("userName");
            if (string.IsNullOrEmpty(userName))
            {
                return new ObjectResult(new { status = "timeout" });
            }

            if (!string.IsNullOrEmpty(file))
            {
                //User user = _userService.Find(userName);
                //string base64 = Request.Form["imgCropped"];
                string imageURL = string.Empty;
                string imagePublicId = string.Empty;
                byte[] bytes = Convert.FromBase64String(file.Split(',')[1]);

                string filePath = Path.Combine(this.Environment.WebRootPath, AUTHOR_IMAGE_SAVING_TEMP_FOLDER);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                string fileName = Path.Combine(filePath, $"{ userName.Replace('@', '_').Replace('.', '_') }.png");
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }

                try
                {
                    using (FileStream stream = new FileStream(fileName, FileMode.Create))
                    {
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Flush();


                    }

                    if (System.IO.File.Exists(fileName))
                    {



                        Account account = new Account(CLOUD_NAME, API_KEY, API_SECRET);

                        Cloudinary cloudinary = new Cloudinary(account);

                        if (user.Author != null && !string.IsNullOrEmpty(user.Author.AuthorImageUrl))
                        {
                            var r = cloudinary.DeleteResourcesByTag(Path.GetFileName(fileName));
                        }


                        var uploadParams = new ImageUploadParams()
                        {
                            Folder = @"Image/Author",
                            File = new FileDescription(@fileName),
                            PublicId = Path.GetFileName(fileName),
                            Tags = Path.GetFileName(fileName),
                            Unsigned = true,
                            UploadPreset = "gwjfherd"
                        };

                        var uploadResult = cloudinary.Upload(uploadParams);
                        if (uploadResult != null && uploadResult.Url != null)
                        {
                            imageURL = uploadResult.Url.AbsoluteUri;
                            if (user.Author == null)
                            {
                                user.Author = new Author();
                            }

                            user.Author.AuthorImageUrl = imageURL;
                            _userService.Update(user);
                        }

                        System.IO.File.Delete(fileName);
                    }

                    return new ObjectResult(new { status = "success", imageUrl = imageURL });
                }
                catch (Exception)
                {
                    return new ObjectResult(new { status = "failed" });
                }

            }
            else
            {
                return new ObjectResult(new { status = "failed" });
            }

        }

        //public async Task<IActionResult> UploadImage(IFormFile file)
        //{
        //    string userName = HttpContext.Session.GetString("userName");
        //    if (string.IsNullOrEmpty(userName))
        //    {
        //        return RedirectToAction("Login", "Users");
        //    }
        //    else
        //    {
        //        User user = _userService.Find(userName);

        //        if (file == null || file.Length == 0)
        //        {
        //            return BadRequest();
        //        }

        //        using (var memoryStream = new MemoryStream())
        //        {
        //            await file.CopyToAsync(memoryStream);
        //            using (var img = Image.FromStream(memoryStream))
        //            {
        //                //string base64 = Request.Form["imgCropped"];
        //                //byte[] bytes = Convert.FromBase64String(base64.Split(',')[1]);

        //                string filePath = Path.Combine(this.Environment.WebRootPath, IMAGE_SAVING_FOLDER, "Author");
        //                if (!Directory.Exists(filePath))
        //                {
        //                    Directory.CreateDirectory(filePath);
        //                }

        //                string fileName = Path.Combine(filePath, $"{ user.UserName.Replace('@', '_').Replace('.', '_') }.png");
        //                if (System.IO.File.Exists(fileName))
        //                {
        //                    System.IO.File.Delete(fileName);
        //                }
        //                //using (FileStream stream = new FileStream(filePath, FileMode.Create))
        //                //{
        //                //    stream.Write(bytes, 0, bytes.Length);
        //                //    stream.Flush();
        //                //}
        //                img.Save(fileName);

        //            }

        //            if (user.Author == null)
        //            {
        //                user.Author = new Author();
        //            }

        //            user.Author.AuthorImageUrl = $"{ user.UserName.Replace('@', '_').Replace('.', '_') }.png";
        //            _userService.Update(user);
        //        }
        //    }

        //    return RedirectToAction(nameof(Edit));

        //}

        // GET: Author/Edit/5
        public async Task<IActionResult> Edit(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                userName = HttpContext.Session.GetString("userName");
                if (string.IsNullOrEmpty(userName))
                {
                    return RedirectToAction("Login", "Users");

                }
            }

            if (!string.IsNullOrEmpty(userName))
            {
                user = _userService.Find(userName);

            }

            List<SelectListItem> pronounsList = new List<SelectListItem>()
            {
                new SelectListItem(){Value="Prefer Not to Say",Text="Prefer Not to Say"},
                new SelectListItem(){Value="He/Him/His",Text="He/Him/His"},
                new SelectListItem(){Value="She/Her/Hers",Text="She/Her/Hers"},
                new SelectListItem(){Value="They/Them/Their",Text="They/Them/Their"},
            };
            ViewBag.pronounsList = pronounsList;

            var author = user.Author;



            if (author == null)
            {
                author = CreateNewAuthor(userName);
            }
            else if (string.IsNullOrEmpty(author.Email))
            {
                author.Email = userName;
            }


            //author.bookList = _bookService.FindAll(author.Id).ToList();
            return View(author);
        }

        public ActionResult AddNewBook(int lastBookId)
        {
            string username = HttpContext.Session.GetString("userName");
            if (string.IsNullOrEmpty(username))
            {
                return new ObjectResult(new { status = "timeout" });
            }

            return PartialView("bookEditPartialView", new Book() { Id = lastBookId + 1 });
        }
        public ActionResult AddAnotherFile(int lastFileId)
        {
            string username = HttpContext.Session.GetString("userName");
            if (string.IsNullOrEmpty(username))
            {
                return new ObjectResult(new { status = "timeout" });
            }

            return PartialView("filePartialView", new UploadedFile() { Id = lastFileId + 1 });
        }
        public ActionResult AddAnotherVideo()
        {
            string username = HttpContext.Session.GetString("userName");
            if (string.IsNullOrEmpty(username))
            {
                return new ObjectResult(new { status = "timeout" });
            }

            return PartialView("videoPartialView", new VideoLink());
        }
        public ActionResult DeleteFile(string filePath, int id)
        {
            string username = HttpContext.Session.GetString("userName");
            if (string.IsNullOrEmpty(username))
            {
                return new ObjectResult(new { status = "timeout" });
            }

            if (!string.IsNullOrEmpty(filePath))
            {
                //string uploadsFolder = Path.Combine(Environment.WebRootPath, FILE_SAVING_TEMP_FOLDER);

                //string deletingFile = Path.Combine(uploadsFolder, filePath);

                //if (System.IO.File.Exists(deletingFile))
                //{
                //    System.IO.File.Delete(deletingFile);
                //} 

                string publicId = $"UploadedFiles/{filePath.Split('/').Last()}";
                Account account = new Account(CLOUD_NAME, API_KEY, API_SECRET);
                Cloudinary cloudinary = new Cloudinary(account);
                var deletionParams = new DeletionParams(publicId) { ResourceType = ResourceType.Raw };
                var r = cloudinary.Destroy(deletionParams);
            }

            if (user.Author != null && user.Author.FilePathList != null)
            {
                user.Author.FilePathList.RemoveAll(f => f.Id == id);
                _userService.Update(user);
            }

            return PartialView("filePartialView", new UploadedFile());
        }
        public ActionResult DeleteVidelLink(string linkName)
        {
            string username = HttpContext.Session.GetString("userName");
            if (string.IsNullOrEmpty(username))
            {
                return new ObjectResult(new { status = "timeout" });
            }

            if (user.Author != null && user.Author.VideoLinkList != null)
            {
                user.Author.VideoLinkList.RemoveAll(f => f.Link == linkName);
                _userService.Update(user);
            }

            return PartialView("filePartialView", new VideoLink());
        }
        public ActionResult RemoveBook(int id)
        {
            string username = HttpContext.Session.GetString("userName");
            if (string.IsNullOrEmpty(username))
            {
                return new ObjectResult(new { status = "timeout" });
            }

            //string uploadsFolder = Path.Combine(Environment.WebRootPath, BOOK_COVER_IMAGE_SAVING_TEMP_FOLDER);

            //string deletingFile = Path.Combine(uploadsFolder, user.Author.BookList.Find(b => b.Id == id).BookCoverUrl);

            //if (System.IO.File.Exists(deletingFile))
            //{
            //    System.IO.File.Delete(deletingFile);
            //}



            if (user.Author != null && user.Author.BookList != null)
            {


                if (user.Author.BookList.Where(b => b.Id == id).FirstOrDefault() != null && !string.IsNullOrEmpty(user.Author.BookList.Where(b => b.Id == id).FirstOrDefault().BookCoverUrl))
                {
                    Account account = new Account(CLOUD_NAME, API_KEY, API_SECRET);

                    Cloudinary cloudinary = new Cloudinary(account);
                    string fileName = $"{username.Replace('@', '_').Replace('.', '_')}_bc_{id}.png";
                    var r = cloudinary.DeleteResourcesByTag(fileName);
                }
                user.Author.BookList.RemoveAll(b => b.Id == id);
                _userService.Update(user);
            }

            return PartialView("bookEditPartialView", new Book());
        }

        private Author CreateNewAuthor(string userName)
        {
            isCreatingNew = true;
            Author authorCreating = new Author();
            authorCreating.Email = userName;
            string fullName = user.Name;

            if (!string.IsNullOrEmpty(fullName))
            {
                if(fullName.Contains(' '))
                {
                    authorCreating.FirstName = fullName.Substring(0, fullName.IndexOf(" "));
                    if (fullName.IndexOf(" ") > 0)
                    {
                        authorCreating.LastName = fullName.Substring(fullName.IndexOf(" ") + 1);
                    }
                }
                else
                {
                    authorCreating.FirstName = fullName;
                }
            }

            return authorCreating;
        }

        // POST: Author/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind] Author author)
        {
            string username = HttpContext.Session.GetString("userName");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction(nameof(Index), "Users");
            }

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("ISBN", "Not valid");
            }
            else
            {
                try
                {
                    //if (user.Author != null && user.Author.BookList != null && user.Author.BookList.Count() > 0)
                    //{
                    //    foreach (Book bk in user.Author.BookList)
                    //    {
                    //        Book bookInUpdatingList = author.BookList.Where(b => b.Id == bk.Id).First();
                    //    }

                    //}


                    //string base64 = Request.Form["imgCropped"];

                    //if (!string.IsNullOrEmpty(base64))
                    //{
                    //    byte[] bytes = Convert.FromBase64String(base64.Split(',')[1]);

                    //    string filePath = Path.Combine(this.Environment.WebRootPath, IMAGE_SAVING_FOLDER, "Author");
                    //    if (!Directory.Exists(filePath))
                    //    {
                    //        Directory.CreateDirectory(filePath);
                    //    }

                    //    string file = Path.Combine(filePath, $"{author.FirstName}_{author.LastName}.png");
                    //    if (Directory.Exists(file))
                    //    {
                    //        Directory.Delete(file);
                    //    }

                    //    using (FileStream stream = new FileStream(file, FileMode.Create))
                    //    {
                    //        stream.Write(bytes, 0, bytes.Length);
                    //        stream.Flush();
                    //    }


                    //    author.AuthorImageUrl = $"{author.FirstName}_{author.LastName}.png"; 
                    //}



                    if (author.BookList != null && author.BookList.Count > 0)
                    {
                        for (int i = 0; i < author.BookList.Count(); i++)
                        {
                            string imageName = $"bookCoverImageFile_{author.BookList[i].Id}";
                            IFormFile bookImage = Request.Form.Files[imageName];

                            if (bookImage != null)
                            {
                                string bookCoverUrl = SaveBookCoverImage(bookImage, username, author.BookList[i]);
                                if (!string.IsNullOrEmpty(bookCoverUrl))
                                {
                                    author.BookList[i].BookCoverUrl = bookCoverUrl;
                                }

                            }
                        }
                    }
                    author.BookList = author.BookList.Where(b => b != null && !IsBookEmpty(b)).ToList();

                    //author.FacebookUrl = author.FacebookUrl;
                    //author.TwitterUrl = author.TwitterUrl;
                    //author.PinterestUrl = author.PinterestUrl;
                    //author.WebsiteUrl = author.WebsiteUrl;
                    //author.InstagramUrl = author.InstagramUrl;

                    if (author.FilePathList != null && author.FilePathList.Count > 0)
                    {
                        for (int i = 0; i < author.FilePathList.Count(); i++)
                        {
                            string fileName = $"uploadedFile_{author.FilePathList[i].Id}";
                            IFormFile fileUploaded = Request.Form.Files[fileName];

                            if (fileUploaded != null)
                            {
                                string filePath = SaveFile(fileUploaded, username, author.FilePathList[i]);
                                if (!string.IsNullOrEmpty(filePath))
                                {
                                    author.FilePathList[i].Path = filePath;
                                }

                            }
                        }
                    }

                    //making the youtube links embed links
                    author.FilePathList = author.FilePathList.Where(f => f != null && f.Path != null).ToList();
                    author.VideoLinkList = author.VideoLinkList.Where(v => v != null && v.Link != null).ToList();

                    if (author.VideoLinkList != null)
                    {
                        foreach (var item in author.VideoLinkList)
                        {
                            if (!item.Link.Contains("embed"))
                            {
                                string originalLink = item.Link.Trim();
                                int length = originalLink.Length;
                                string linkStart = "https://youtube.com/embed/";
                                int equalLocation = originalLink.IndexOf("=") + 1;
                                string endOfLink = originalLink.Substring(equalLocation, length - equalLocation);
                                string newLink = linkStart + endOfLink;
                                item.Link = newLink;
                            }
                        }
                    }



                    //creating the profile url
                    if (author.ProfileUrl != null || author.ProfileUrl != "")
                    {
                        string profileUrlString = author.FirstName + "." + author.LastName;
                        var authorList = _userService.Read();
                        int repeatAuthorName = 0;
                        foreach (var item in authorList)
                        {
                            if (item.Author != null)
                            {
                                if (item.Author.ProfileUrl == profileUrlString)
                                {
                                    repeatAuthorName++; // add a number for each user with the same first and last name
                                }
                            }
                        }
                        if (repeatAuthorName != 0) //check if there was more than 1 user with the same first and last name
                        {
                            profileUrlString = profileUrlString + $"{repeatAuthorName + 1}"; // add a number to the profile url
                        }
                        author.ProfileUrl = profileUrlString;
                    }


                    user.Author = author;
                    user.UpdatedAt = DateTime.UtcNow;

                    string link = $"{System.Environment.GetEnvironmentVariable("BASE_URL")}/Users/Index?searchString={user.Name}";
                    if (isCreatingNew)
                    {
                        EmailHelper.SendEmail(EmailHelper.adminEmail, EmailHelper.EMAIL_AUTHOR_PROFILE_CREATED, EmailHelper.GetAuthorProfileChangedBody(link));
                    }
                    else
                    {
                        EmailHelper.SendEmail(EmailHelper.adminEmail, EmailHelper.EMAIL_AUTHOR_PROFILE_CHANGED, EmailHelper.GetAuthorProfileChangedBody(link));
                    }



                    _userService.Update(user);
                    //if (string.IsNullOrEmpty(id))
                    //{
                    //    user.
                    //    return View(author);

                    //}
                    //else
                    //{
                    //    _authorService.Update(author);
                    //    return View(author);


                    //}
                }
                catch (Exception ex)
                {

                }
            }
            return RedirectToAction(nameof(Edit));
        }

        private string SaveBookCoverImage(IFormFile bookImage, string username, Book book)
        {
            string imageURL = string.Empty;

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    memoryStream.Position = 0;
                    bookImage.CopyTo(memoryStream);
                    using (var img = Image.FromStream(memoryStream))
                    {
                        //CheckAndCreateSavingPath(ref usernameInFolderName, ref uploadsFolder, BOOK_COVER_IMAGE_SAVING_FOLDER, username);
                        string fileName = $"{username.Replace('@', '_').Replace('.', '_')}_bc_{book.Id}.png";
                        string filePath = Path.Combine(Environment.WebRootPath, BOOK_COVER_IMAGE_SAVING_TEMP_FOLDER, fileName);
                        img.Save(filePath);

                        if (System.IO.File.Exists(filePath))
                        {
                            Account account = new Account(CLOUD_NAME, API_KEY, API_SECRET);

                            Cloudinary cloudinary = new Cloudinary(account);

                            if (!string.IsNullOrEmpty(book.BookCoverUrl))
                            {
                                var r = cloudinary.DeleteResourcesByTag(fileName);
                            }

                            var uploadParams = new ImageUploadParams()
                            {
                                Folder = @"Image/Books",
                                File = new FileDescription(filePath),
                                PublicId = fileName,
                                Tags = fileName,
                                Unsigned = true,
                                UploadPreset = "gwjfherd"
                            };

                            var uploadResult = cloudinary.Upload(uploadParams);
                            if (uploadResult != null && uploadResult.Url != null)
                            {
                                imageURL = uploadResult.Url.AbsoluteUri;
                            }

                            System.IO.File.Delete(filePath);
                        }

                        return imageURL;
                    }
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public bool IsBookEmpty(Book book)
        {
            return string.IsNullOrEmpty(book.TitleOfBook) &&
                    string.IsNullOrEmpty(book.Publisher) &&
                    string.IsNullOrEmpty(book.ISBN) &&
                    string.IsNullOrEmpty(book.BookCoverUrl) &&
                    string.IsNullOrEmpty(book.BookDescription) &&
                    string.IsNullOrEmpty(book.Language) &&
                    string.IsNullOrEmpty(book.AgeGroup) &&
                    book.Price == 0.00m;
        }


        public string SaveFile(IFormFile file, string username, UploadedFile uf)
        {
            //string usernameInFolderName = string.Empty;
            //string uploadsFolder = string.Empty;
            //CheckAndCreateSavingPath(ref usernameInFolderName, ref uploadsFolder, FILE_SAVING_FOLDER, username);
            string savedFileName = string.Empty;

            if (file != null)
            {
                //string fileName = file.FileName;
                //string filePath = MakeUniqueFilePath(Path.Combine(uploadsFolder, fileName));
                string fileExtention = Path.GetExtension(file.FileName);
                string fileName = $"{username.Replace('@', '_').Replace('.', '_')}_f_{uf.Id}{fileExtention}";
                string filePath = Path.Combine(Environment.WebRootPath, FILE_SAVING_TEMP_FOLDER, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                if (System.IO.File.Exists(filePath))
                {
                    Account account = new Account(CLOUD_NAME, API_KEY, API_SECRET);

                    Cloudinary cloudinary = new Cloudinary(account);

                    var uploadParams = new RawUploadParams()
                    {
                        Folder = @"UploadedFiles/",
                        File = new FileDescription(filePath),
                        PublicId = fileName,
                        Tags = fileName
                    };

                    var uploadResult = cloudinary.Upload(uploadParams);
                    if (uploadResult != null && uploadResult.Url != null)
                    {
                        savedFileName = uploadResult.Url.AbsoluteUri;
                    }

                    System.IO.File.Delete(filePath);
                }
            }

            return savedFileName;
        }

        private void CheckAndCreateSavingPath(ref string usernameInFolderName, ref string uploadsFolder, string savingFolder, string username)
        {
            usernameInFolderName = username.Replace('@', '_').Replace('.', '_');

            uploadsFolder = Path.Combine(Environment.WebRootPath, savingFolder, usernameInFolderName);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
        }

        public string MakeUniqueFilePath(string path)
        {
            string dir = Path.GetDirectoryName(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            string fileExt = Path.GetExtension(path);

            for (int i = 1; ; ++i)
            {
                if (!System.IO.File.Exists(path))
                    return path;

                path = Path.Combine(dir, fileName + "_" + i + fileExt);
            }
        }
    }
}
