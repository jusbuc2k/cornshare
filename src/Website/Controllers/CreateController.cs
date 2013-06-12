using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website.Models;
using System.Data.Entity;
using BusinessLogic;
using System.Text;

namespace Website.Controllers
{
    [Authorize]
    public class CreateController : Controller
    {
        public CreateController(BusinessLogic.Services.IFileStorageService fileStorageService)
        {
            this.FileStorageService = fileStorageService;
            this.MaxShareTTL = int.Parse(System.Configuration.ConfigurationManager.AppSettings["MaxLinkLifetime"]);
        }

        public BusinessLogic.Services.IFileStorageService FileStorageService { get; set; }

        public int MaxShareTTL { get; set; }

        private string CreateLink(ShareToken shareToken)
        {
            var baseUrl = System.Configuration.ConfigurationManager.AppSettings["ShareBaseUrl"];

            if (string.IsNullOrEmpty(baseUrl))
            {
                return Url.Action("Index", "Link", new { ShareToken = shareToken.ToBase64String() }, Request.Url.Scheme);
            }

            baseUrl = baseUrl.TrimEnd('/');

            return string.Concat(baseUrl,Url.Action("Index", "Link", new { ShareToken = shareToken.ToBase64String() }));
        }
                
        public ActionResult Index()
        {
            var model = new ShareCreateModel();
            var list = new List<StoredFileModel>();

            using (var db = new BusinessLogic.Data.SharingDataContext())
            {
                BusinessLogic.Data.FileSet fileSet;

                // delete draft file sets that don't have any files
                var cleanupSets = db.FileSets
                    .Where(x => x.OwnerUsername == this.User.Identity.Name
                        && x.IsDraft
                        && x.StoredFiles.Count == 0)
                    .ToList();

                foreach (var set in cleanupSets)
                {
                    db.FileSets.Remove(set);
                }

                // find an orphan file set that we actually uploaded files to and resume it
                fileSet = db.FileSets
                    .Where(x => x.OwnerUsername == this.User.Identity.Name 
                        && x.IsDraft
                        && x.StoredFiles.Count > 0)
                    .Include(i => i.StoredFiles)
                    .OrderByDescending(o => o.CreateDateTime)
                    .FirstOrDefault();

                if (fileSet == null)
                {
                    model.IsNew = true;
                    fileSet = new BusinessLogic.Data.FileSet()
                    {
                        Name = string.Format("File share created on {0:g}", DateTime.Now),
                        CreateDateTime = DateTime.UtcNow,
                        OwnerUsername = this.User.Identity.Name,
                        ExpirationDateTime = DateTime.Now.AddDays(30),
                        IsDraft = true
                    };
                    db.FileSets.Add(fileSet);
                    db.SaveChanges();
                }
                else
                {
                    foreach (var file in fileSet.StoredFiles)
                    {
                        list.Add(new StoredFileModel(){ 
                            FileID = file.FileID,
                            Filename = file.Filename,
                            Length = FileSize.GetSize(file.Length)
                        });
                    }                    
                }

                model.Files = list;
                model.Name = fileSet.Name;
                model.FileSetID = fileSet.FileSetID;

                return View(model);
            }            
        }

        public ActionResult Abandon(int id)
        {
            using (var db = new BusinessLogic.Data.SharingDataContext())
            {
                BusinessLogic.Data.FileSet fileSet;

                fileSet = db.FileSets
                    .Where(x => x.OwnerUsername == this.User.Identity.Name && x.IsDraft && x.FileSetID == id)
                    .Include(i => i.StoredFiles)                    
                    .SingleOrDefault();

                if (fileSet != null)
                {                    
                    foreach (var file in fileSet.StoredFiles)
                    {
                        this.FileStorageService.Delete(file.StoragePath);
                    }                    
                    db.FileSets.Remove(fileSet);
                    db.SaveChanges();
                }                
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Links(SharePublishModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new BusinessLogic.Data.SharingDataContext())
                {
                    var set = db.FileSets.Single(x => x.FileSetID == model.FileSetID);
                    var shareExpires = model.ExpirationDate;
                    var fileLinks = new List<object>();
                    var shareToken = new ShareToken(BusinessLogic.SecureRandom.GetRandomBytes(20), 0);
                    byte[] passwordBytes = null;

                    if (shareExpires > DateTime.UtcNow.AddDays(this.MaxShareTTL))
                    {
                        shareExpires = DateTime.UtcNow.AddDays(this.MaxShareTTL);
                    }
                    
                    if (model.RequirePassword)
                    {
                        passwordBytes = Convert.FromBase64String(System.Web.Helpers.Crypto.HashPassword(model.Password));
                    }

                    set.Name = model.Name;
                    set.IsDraft = false;

                    var setShare = new BusinessLogic.Data.SharedFileSet()
                    {
                        ShareToken = shareToken.GetBytes(),
                        FileSet = set,
                        OwnerUsername = this.User.Identity.Name,
                        ExpirationDateTime = shareExpires,
                        Password = passwordBytes,
                        AllowUpload = model.AllowUpload,
                        CreateUsername = this.User.Identity.Name,
                        CreateDateTime = DateTime.UtcNow
                    };
                    db.SharedFileSets.Add(setShare);

                    BusinessLogic.Data.SharedFile fileShare;
                    ShareToken fileShareToken;

                    if (set.StoredFiles.Count > 1)
                    {
                        foreach (var file in set.StoredFiles)
                        {
                            fileShareToken = new ShareToken(BusinessLogic.SecureRandom.GetRandomBytes(20), file.FileID);

                            fileShare = new BusinessLogic.Data.SharedFile()
                            {
                                ShareToken = fileShareToken.GetBytes(),
                                StoredFile = file,
                                OwnerUsername = this.User.Identity.Name,
                                ExpirationDateTime = shareExpires,
                                Password = passwordBytes,
                                CreateUsername = this.User.Identity.Name,
                                CreateDateTime = DateTime.UtcNow
                            };

                            db.SharedFiles.Add(fileShare);

                            fileLinks.Add(new
                            {
                                url = this.CreateLink(fileShareToken),
                                fileName = file.Filename,
                                length = file.Length
                            });
                        }
                    }

                    db.SaveChanges();

                    return Json(new
                    {
                        success = true,
                        setLink = this.CreateLink(shareToken),
                        fileLinks = fileLinks
                    });
                }
            }

            return Json(new
            {
                success = false,
                errorMessage = "Something wasn't right about the data you provided."
            });
        }

        [HttpPost]
        public ActionResult Upload(int fileSetID)
        {
            try
            {
                using (var db = new BusinessLogic.Data.SharingDataContext())
                {
                    var set = db.FileSets
                        .Where(x => x.OwnerUsername == this.User.Identity.Name && x.FileSetID == fileSetID)
                        .Include(i => i.StoredFiles)
                        .SingleOrDefault();
                    
                    string storagePath;
                    var fallbackFiles = new List<StoredFileModel>();

                    foreach (var file in set.StoredFiles)
                    {
                        fallbackFiles.Add(new StoredFileModel() { Filename = System.IO.Path.GetFileName(file.Filename) });
                    }

                    foreach (var fileKey in this.Request.Files.AllKeys)
                    {
                        storagePath = this.FileStorageService.Put(this.Request.Files[fileKey].InputStream);

                        db.StoredFiles.Add(new BusinessLogic.Data.StoredFile()
                        {
                            FileSet = set,
                            ContentType = this.Request.Files[fileKey].ContentType,
                            Length = this.Request.Files[fileKey].ContentLength,
                            Filename = System.IO.Path.GetFileName(this.Request.Files[fileKey].FileName),
                            OwnerUsername = this.User.Identity.Name,
                            StoragePath = storagePath,
                            CreateUsername = this.User.Identity.Name,
                            CreateDateTime = DateTime.UtcNow
                        });

                        fallbackFiles.Add(new StoredFileModel() { Filename = System.IO.Path.GetFileName(this.Request.Files[fileKey].FileName) });
                    }

                    db.SaveChanges();

                    if (Request.IsAjaxRequest() || Request.Form["IsFlashRequest"] == "1")
                    {
                        return new HttpStatusCodeResult(200);
                    }
                    else
                    {
                        var model = new ShareCreateModel()
                        {
                            FileSetID = fileSetID,
                            Files = fallbackFiles
                        };

                        return View("Index", model);
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Status = "500 Error";
                Response.Write(ex.Message);
                Response.End();
                return null;
            }                       
        }

        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Downstream, Duration = 300)]
        public ActionResult Thumb(int id)
        {
            using (var db = new BusinessLogic.Data.SharingDataContext())
            {
                var file = db.StoredFiles
                    .Where(x => x.OwnerUsername == this.User.Identity.Name && x.FileID == id)
                    .SingleOrDefault();

                var thumbFileName = string.Concat(file.StoragePath, ".jpg");
                System.IO.Stream stream;

                if (!file.ContentType.StartsWith("image"))
                {
                    return File("~/images/trans.gif", "image/gif");
                }

                if (this.FileStorageService.Exists(thumbFileName))
                {
                    stream = this.FileStorageService.Get(thumbFileName);

                    return File(stream, "image/jpeg");
                }

                using (var srcStream = this.FileStorageService.Get(file.StoragePath))
                {
                    stream = BusinessLogic.Helpers.ThumbnailHelper.CreateThumbnail(srcStream);
                }

                this.FileStorageService.Put(stream, thumbFileName);
                stream.Position = 0;

                return File(stream, "image/jpeg");
            }
        }

        [HttpPost]
        public ActionResult SendEmail(SendEmailModel model)
        {
            if (ModelState.IsValid)
            {
                var msg = new System.Net.Mail.MailMessage(System.Configuration.ConfigurationManager.AppSettings["SmtpFrom"], model.To);
                var client = new System.Net.Mail.SmtpClient(System.Configuration.ConfigurationManager.AppSettings["SmtpHost"]);
                var body = new StringBuilder();

                msg.IsBodyHtml = false;
                msg.Subject = string.Format("{0} sent you a link from CornShare", this.User.Identity.Name);
                msg.ReplyToList.Add(model.From);

                body.AppendLine(model.Body);
                body.AppendLine();
                body.AppendLine("Click the link below to access the share. If you can't click the link, copy and paste it into your web browser's address bar.");
                body.AppendLine();
                body.AppendLine(model.Link);
                body.AppendLine();
                body.AppendFormat("- This link will expire after {0:d}.\n", model.ExpirationDate.ToLocalTime());

                if (model.RequirePassword)
                {
                    body.AppendLine("- *This share requires a password*, so you'll need to get that from the person that sent it.");
                }
                if (model.AllowUpload)
                {
                    body.AppendLine("- You can both upload and download files using this link.");
                }

                msg.Body = body.ToString();

                client.Send(msg);

                return Json(new {
                    success= true
                });
            }
            
            return Json(new {
                success=false,
                errorMessage = "Something wasn't right."
            });
        }

    }
}
