using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using INFMDOTNET.Models;

namespace INFMDOTNET.Controllers
{
    public class UserController : Controller
    {
        //Reg Action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmalVarified,ActivationCode")]User user)
        {
            bool Status = false;
            string Message = "";

            //Model Validation
            if (ModelState.IsValid)
            {
                //Does email already exist
                var EmailExists = DoesEmailExist(user.EmailID);
                if (EmailExists)
                {
                    ModelState.AddModelError("EmailExists", "Email already exist");
                    return View(user);
                }

                //Generate Activation Code
                user.ActivationCode = Guid.NewGuid();

                //Password Hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);

                //IsEmalVarified if false the first time
                user.IsEmalVarified = false;

                //Save data to Database
                using (MyDatabaeEntities de = new MyDatabaeEntities())
                {
                    de.Users.Add(user);
                    de.SaveChanges();

                    //Send Email to users
                    SendVerificationLinkEmail(user.EmailID, user.ActivationCode.ToString());
                    Message = "Registration succesfuly done! Account activation link has been snt to your email: " + user.EmailID;
                    Status = true;
                }

            }
            else
            {
                Message = "Invalid Request";
            }

            ViewBag.Message = Message;
            ViewBag.Staus = Status;

            return View(user);
        }

        //Ver Email
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (MyDatabaeEntities dc = new MyDatabaseEntities())
            {
                dc.Configuration.ValidateOnSaveEnabled = false;  
                                                                
                var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmalVarified = true;
                    dc.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid Request";
                }
            }
            ViewBag.Status = Status;
            return View();
        }

        //Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        //Login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login, string ReturnUrl = "")
        {
            string message = "";
            using (MyDatabaseEntities dc = new MyDatabaseEntities())
            {
                var v = dc.Users.Where(a => a.EmailID == login.EmailID).FirstOrDefault();
                if (v != null)
                {
                    if (!v.IsEmalVarified)
                    {
                        ViewBag.Message = "Please verify your email first";
                        return View();
                    }
                    if (string.Compare(Crypto.Hash(login.Password), v.Password) == 0)
                    {
                        int timeout = login.RememberMe ? 525600 : 20; // 525600 min = 1 year
                        var ticket = new FormsAuthenticationTicket(login.EmailID, login.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);


                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        message = "Invalid credential provided";
                    }
                }
                else
                {
                    message = "Invalid credential provided";
                }
            }
            ViewBag.Message = message;
            return View();
        }

        //Logout
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }

        [NonAction]
        public Boolean DoesEmailExist(string emailID)
        {
            using (MyDatabaeEntities de = new MyDatabaeEntities())
            {
                var exists = de.Users.Where(a => a.EmailID == emailID).FirstOrDefault();
                return exists != null;
            }
        }

        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode)
        {
            var verifyURL = "/User/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyURL);

            var fromEmail = new MailAddress("infmnotnet@gmail.com", "INFMDOTNET");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "P@ssw0rd1234"; //pass

            string sybject = "Your account was succesfuly created!";
            string body = "<br/><br/>We are exited to tell you that yor account is created! Please click on the link below to verify your account. <br/><br/><a href ='" + link+"'>"+link+"</a>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
                
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = sybject,
                Body = body,
                IsBodyHtml = true
            })
            smtp.Send(message);

        }
    }

    internal class MyDatabaseEntities : MyDatabaeEntities
    {
    }
}