using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Website.Models;
using BusinessLogic;

namespace Website.Controllers
{
    public class LinkController : Controller
    {
        public LinkController(BusinessLogic.Services.IFileStorageService fileStorageService)
        {
            this.FileStorageService = fileStorageService;
        }

        public BusinessLogic.Services.IFileStorageService FileStorageService { get; set; }

        private string CreateSessionID()
        {
            return BusinessLogic.SecureRandom.GetRandomBytes(16).ToHex();
        }

        private bool PasswordIsValidOrNotRequired(byte[] passwordHash, string password)
        {
            if (passwordHash == null)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(password))
            {
                return System.Web.Helpers.Crypto.VerifyHashedPassword(Convert.ToBase64String(passwordHash), password);
            }

            return false;
        }

        private string CreatePasswordToken(byte[] passwordHash, string sessionID)
        {
            using (var sha1 = new System.Security.Cryptography.SHA1Managed())
            {
                var sessionIDBytes = HexEncoding.GetBytes(sessionID);

                sha1.TransformBlock(passwordHash, 0, passwordHash.Length, null, 0);
                sha1.TransformFinalBlock(sessionIDBytes, 0, sessionIDBytes.Length);

                return sha1.Hash.ToHex();
            }
        }

        private bool ValidatePasswordToken(byte[] passwordHash, string sessionID, string passwordToken)
        {
            if (string.IsNullOrEmpty(sessionID) || string.IsNullOrEmpty(passwordToken))
                return false;

            var s = CreatePasswordToken(passwordHash, sessionID);

            return (string.Compare(s, passwordToken, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private bool ValidatePasswordCookie(byte[] passwordHash)
        {
            var cookie = Request.Cookies["cshare-session"];

            if (passwordHash == null)
                return true;

            if (cookie == null)
                return false;

            if (cookie.Value == null)
                return false;

            return ValidatePasswordToken(passwordHash, cookie.Values["id"], cookie.Values["token"]);
        }

        private BusinessLogic.Data.StoredFile GetStoredFile(DownloadModel model)
        {
            using (var db = new BusinessLogic.Data.SharingDataContext())
            {
                var shareToken = ShareToken.Parse(model.ShareToken);
                var shareTokenBytes = shareToken.GetBytes();
                byte[] sharePassword = null;
                BusinessLogic.Data.StoredFile storedFile;

                if (shareToken.FileID > 0)
                {
                    var sharedFile = db.SharedFiles
                        .Where(x => x.ShareToken == shareTokenBytes
                            && x.ExpirationDateTime > DateTime.UtcNow)
                        .Include(i => i.StoredFile)
                        .SingleOrDefault();

                    sharePassword = sharedFile.Password;
                    storedFile = sharedFile.StoredFile;
                }
                else
                {
                    var sharedFileSet = db.SharedFileSets
                        .Where(x => x.ShareToken == shareTokenBytes
                            && x.ExpirationDateTime > DateTime.UtcNow)
                        .Include(s => s.FileSet.StoredFiles)
                        .SingleOrDefault();

                    sharePassword = sharedFileSet.Password;
                    storedFile = sharedFileSet.FileSet.StoredFiles.SingleOrDefault(x => x.FileID == model.FileID);
                }

                if (!ValidatePasswordCookie(sharePassword))
                {
                    return null;
                }

                return storedFile;
            }
        }

        private HttpCookie CreateSessionCookie(byte[] passwordHash)
        {
            var sessionCookie = Request.Cookies["cshare-session"];
            var sessionID = CreateSessionID();

            if (sessionCookie == null)
            {
                sessionCookie = new HttpCookie("cshare-session");
                //sessionCookie.Domain = Request.Url.Host;
            }
            sessionCookie.Path = Request.Path;
            sessionCookie.HttpOnly = true;
            sessionCookie.Values["id"] = sessionID;
            sessionCookie.Values["token"] = this.CreatePasswordToken(passwordHash, sessionID);

            return sessionCookie;
        }

        // public action methods

        public ActionResult Index(string shareToken)
        {  
            if (string.IsNullOrEmpty(shareToken))
            {
                return HttpNotFound();
            }

            return this.Index(new LinkModel()
            {
                ShareToken = shareToken,
                Files = new List<DownloadModel>()
            });
        }
                
        [HttpPost]
        public ActionResult Index(LinkModel model)
        {
            var token = ShareToken.Parse(model.ShareToken);
            var tokenBytes = token.GetBytes();
            var files = new List<DownloadModel>();
            byte[] passwordHash;
            
            using (var db = new BusinessLogic.Data.SharingDataContext())
            {
                if (token.FileID > 0)
                {
                    var sharedFile = db.SharedFiles
                        .Where(x => x.ShareToken == tokenBytes
                            && x.ExpirationDateTime > DateTime.UtcNow)
                        .Include(i => i.StoredFile)
                        .SingleOrDefault();

                    model.PasswordRequired = sharedFile.Password != null;
                    passwordHash = sharedFile.Password;

                    files.Add(new DownloadModel()
                    {
                        ShareToken = model.ShareToken,
                        FileID = sharedFile.StoredFile.FileID,
                        FileName = sharedFile.StoredFile.Filename,
                        Length = FileSize.GetSize(sharedFile.StoredFile.Length)
                    });
                }
                else
                {
                    var sharedFileSet = db.SharedFileSets
                        .Where(x => x.ShareToken == tokenBytes
                            && x.ExpirationDateTime > DateTime.UtcNow)
                        .Include(i => i.FileSet.StoredFiles)
                        .SingleOrDefault();

                    model.PasswordRequired = sharedFileSet.Password != null;
                    passwordHash = sharedFileSet.Password;

                    model.AllowUpload = sharedFileSet.AllowUpload;
                    model.FileSetName = sharedFileSet.FileSet.Name;

                    foreach (var file in sharedFileSet.FileSet.StoredFiles)
                    {
                        files.Add(new DownloadModel()
                        {
                            ShareToken = model.ShareToken,
                            FileID = file.FileID,
                            FileName = file.Filename,
                            Length = FileSize.GetSize(file.Length)
                        });
                    }
                    
                }
                
                if (model.PasswordRequired) 
                {
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        model.PasswordIsValid = this.PasswordIsValidOrNotRequired(passwordHash, model.Password);
                        if (model.PasswordIsValid)
                        {
                            var cookie = CreateSessionCookie(passwordHash);
                            Response.SetCookie(cookie);
                        }
                    }
                    else
                    {
                        model.PasswordIsValid = this.ValidatePasswordCookie(passwordHash);
                    }           
                }

                model.Password = null;
                model.Files = files;

                if (!model.PasswordIsValid && model.PasswordRequired)
                {
                    model = new LinkModel()
                    {
                        ShareToken = model.ShareToken,
                        Files = new List<DownloadModel>(),
                        PasswordRequired = true
                    };
                }                
            }

            if (Request.IsAjaxRequest())
            {
                return Json(model);                
            }
            else
            {
                return View(model);
            }           
        }
                
        public ActionResult Download(DownloadModel model)
        {
            var file = this.GetStoredFile(model);

            var stream = this.FileStorageService.Get(file.StoragePath);

            return File(stream, file.ContentType, file.Filename);
        }

        [OutputCache(Location=System.Web.UI.OutputCacheLocation.Downstream, Duration=300)]
        public ActionResult Thumb(DownloadModel model)
        {
            var file = this.GetStoredFile(model);
            var thumbFileName = string.Concat(file.StoragePath, ".jpg");
            System.IO.Stream stream;

            //TODO: This isn't always the case with images because the content type is sometimes not image/*
            // for images if the uploader control doesn't set them that way (e.g. uploadify)
            if (!file.ContentType.StartsWith("image"))
            {
                return File("~/images/trans.gif", "image/gif");
            }            

            if (this.FileStorageService.Exists(thumbFileName))
            {
                stream = this.FileStorageService.Get(thumbFileName);

                return File(stream, "image/jpeg");
            }

            stream = BusinessLogic.Helpers.ThumbnailHelper.CreateThumbnail(this.FileStorageService.Get(file.StoragePath));

            this.FileStorageService.Put(stream, thumbFileName);
            stream.Position = 0;

            return File(stream, "image/jpeg");
        }

        [HttpPost]
        public ActionResult Upload(UploadModel model)
        {            
            try
            {
                using (var db = new BusinessLogic.Data.SharingDataContext())
                {
                    var shareToken = ShareToken.Parse(model.ShareToken);
                    var shareTokenBytes = shareToken.GetBytes();

                    var sharedSet = db.SharedFileSets
                        .Where(x => x.ShareToken == shareTokenBytes && x.ExpirationDateTime > DateTime.UtcNow && x.AllowUpload)
                        .Include(i=>i.FileSet)
                        .SingleOrDefault();

                    if (sharedSet == null || !ValidatePasswordCookie(sharedSet.Password) || !sharedSet.AllowUpload)
                    {
                        return new HttpStatusCodeResult(403);
                    }

                    string storagePath;
                    foreach (var fileKey in this.Request.Files.AllKeys)
                    {
                        storagePath = this.FileStorageService.Put(this.Request.Files[fileKey].InputStream);
                        
                        db.StoredFiles.Add(new BusinessLogic.Data.StoredFile()
                        {
                            FileSet = sharedSet.FileSet,
                            ContentType = this.Request.Files[fileKey].ContentType,
                            Length = this.Request.Files[fileKey].ContentLength,
                            Filename = System.IO.Path.GetFileName(this.Request.Files[fileKey].FileName),
                            OwnerUsername = this.User.Identity.Name,
                            StoragePath = storagePath,
                            CreateUsername = this.User.Identity.Name,
                            CreateDateTime = DateTime.UtcNow
                        });
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Response.ContentType = "text/plain";
                Response.Write(ex.Message);
                return new HttpStatusCodeResult(500);
            }

            if (Request.IsAjaxRequest() || Request.Form["IsFlashRequest"] == "1")
            {                
                Response.ContentType = "text/plain";
                Response.Write("OK");
                return new HttpStatusCodeResult(200);
            }
            else
            {
                return RedirectToAction("Index", new { id = model.ShareToken });
            }
        }

    }
}
