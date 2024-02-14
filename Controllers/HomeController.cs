﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BrightIdeas.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BrightIdeas.Controllers
{
    public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private MyContext _context;

    public HomeController(ILogger<HomeController> logger, MyContext context)
    {
        _logger = logger;
        _context = context;
    }


        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("register")]
        public IActionResult Register(User newUser)
        {
            if(ModelState.IsValid)
            {
                if(_context.Users.Any(user => user.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");

                    return View("Index");
                }

                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);

                _context.Users.Add(newUser);
                _context.SaveChanges();
                
                TempData["Message"] = "Registered Successfully. Please Login.";
                return RedirectToAction("Index");
            }
            return View("Index");
        }

        [HttpPost("checkLogin")]
        public IActionResult CheckLogin(Login login)
        {
            if(ModelState.IsValid)
            {
                User userInDb = _context.Users.FirstOrDefault(user => user.Email == login.LoginEmail);

                if(userInDb == null)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid login, email not register!");

                    ViewBag.AllUsers = _context.Users.ToList();
                    return View("Index");
                }
                PasswordHasher<Login> hasher = new PasswordHasher<Login>();

                var result = hasher.VerifyHashedPassword(login, userInDb.Password, login.LoginPassword);

                if(result == 0)
                {
                    ModelState.AddModelError("LoginPassword", "Invalid login, incorrect password!");

                    ViewBag.AllUsers = _context.Users.ToList();
                    return View("Index");
                }

                HttpContext.Session.SetInt32("userId", userInDb.UserId);

                return RedirectToAction("Feed");

            }

            return View("Index");
        }
        [SessionCheck]
        [HttpGet("Feed")]
        public IActionResult Feed()
        {
            int? loggedUserId = HttpContext.Session.GetInt32("userId");
            User currentUser = _context.Users.FirstOrDefault(e => e.UserId == loggedUserId);
            ViewBag.userName = currentUser;
            if(loggedUserId == null) return RedirectToAction("Index");

            ViewBag.LoggedUser = _context.Users.FirstOrDefault(user => user.UserId == loggedUserId);

            ViewBag.AllIdeas = _context.Ideas
                .Include(idea => idea.Creator)
                .Include(idea => idea.Likes)
                .OrderByDescending(idea => idea.Likes.Count)
                .ToList();
            
            ViewBag.AllUsers = _context.Users.ToList();

            return View();
        }
        [SessionCheck]
        [HttpPost("/idea/create")]
        public IActionResult CreateIdea(Idea newIdea)
        {
            int? loggedUserId = HttpContext.Session.GetInt32("userId");
            User currentUser = _context.Users.FirstOrDefault(e => e.UserId == loggedUserId);
            ViewBag.userName = currentUser;

            if(loggedUserId == null) return RedirectToAction("Index");

            if(ModelState.IsValid)
            {
                newIdea.UserId = (int)loggedUserId;
                _context.Add(newIdea);
                _context.SaveChanges();
                
                return RedirectToAction("Feed");
            }

            ViewBag.LoggedUser = _context.Users.FirstOrDefault(user => user.UserId == loggedUserId);

            ViewBag.AllIdeas = _context.Ideas
                .Include(idea => idea.Creator)
                .Include(idea => idea.Likes)
                .ToList();
            
            ViewBag.AllUsers = _context.Users.ToList();

            return View("Feed");   
        }
        [SessionCheck]
        [HttpGet("idea/{ideaId}/edit")]
        public IActionResult EditForm(int ideaId)
        {
            int? loggedUserId = HttpContext.Session.GetInt32("userId");
            User currentUser = _context.Users.FirstOrDefault(e => e.UserId == loggedUserId);
            ViewBag.userName = currentUser;
            Idea displayMe = _context.Ideas
                .FirstOrDefault(idea => idea.IdeaId == ideaId);

            return View(displayMe);
        }

        [HttpPost("ideas/submitEdit")]
        public IActionResult SubmitEdit(Idea editedIdea)
        {
            int? loggedUserId = HttpContext.Session.GetInt32("userId");
            
            User currentUser = _context.Users.FirstOrDefault(e => e.UserId == loggedUserId);
            ViewBag.userName = currentUser;
            if(loggedUserId == null) return RedirectToAction("Index");

            Idea editMe = _context.Ideas
                .FirstOrDefault(idea => idea.IdeaId == editedIdea.IdeaId);

            editMe.Title = editedIdea.Title;

            _context.SaveChanges();

            return RedirectToAction("SingleIdea", new {id = editedIdea.IdeaId});
        }
        [SessionCheck]
        [HttpGet("idea/{ideaId}/delete")]
        public IActionResult DeleteIdea(int ideaId)
        {
            int? loggedUserId = HttpContext.Session.GetInt32("userId");
            User currentUser = _context.Users.FirstOrDefault(e => e.UserId == loggedUserId);
            ViewBag.userName = currentUser;
            if(loggedUserId == null) return RedirectToAction("Index");

            Idea deleteMe = _context.Ideas
                .FirstOrDefault(idea => idea.IdeaId == ideaId);

            _context.Ideas.Remove(deleteMe);
            _context.SaveChanges();

            return RedirectToAction("Feed");
        }
        [SessionCheck]
        [HttpGet("user/{id}")]
        public IActionResult SingleUser(int id)
        {
            int? loggedUserId = HttpContext.Session.GetInt32("userId");
            
            User currentUser = _context.Users.FirstOrDefault(e => e.UserId == loggedUserId);
            ViewBag.userName = currentUser;
            if(loggedUserId == null) return RedirectToAction("Index");

            ViewBag.SingleUser = _context.Users
                .Include(user => user.CreatedIdeas)
                .ThenInclude(user => user.Likes)
                .FirstOrDefault(user => user.UserId == id);


            return View();
        }
        [SessionCheck]
        [HttpGet("ideas/{id}")]
        public IActionResult SingleIdea(int id)
        {
            int? loggedUserId = HttpContext.Session.GetInt32("userId");
            
            User currentUser = _context.Users.FirstOrDefault(e => e.UserId == loggedUserId);
            ViewBag.userName = currentUser;
            if(loggedUserId == null) return RedirectToAction("Index");

            ViewBag.LoggedUser = _context.Users.FirstOrDefault(user => user.UserId == loggedUserId);

            ViewBag.SingleIdea = _context.Ideas
                .Include(idea => idea.Likes)
                .ThenInclude(user => user.User)
                .Include(idea => idea.Creator)
                .FirstOrDefault(idea => idea.IdeaId == id);

            return View();
        }
        [SessionCheck]
        [HttpGet("ideas/{ideaId}/like")]
        public IActionResult NewLike(int ideaId, int userId)
        {   
            int? loggedUserId = HttpContext.Session.GetInt32("userId");
            
            User currentUser = _context.Users.FirstOrDefault(e => e.UserId == loggedUserId);
            ViewBag.userName = currentUser;
            if(loggedUserId == null) return RedirectToAction("Index");

            Like checkForLike = _context.Likes
                .FirstOrDefault(lk => lk.UserId ==(int)loggedUserId && lk.IdeaId == ideaId);

            if(checkForLike == null)
            {
                Like newLike = new Like();
                newLike.UserId = (int)loggedUserId;
                newLike.IdeaId = ideaId;

                _context.Add(newLike);
                _context.SaveChanges();
            }
            else
            {
                return RedirectToAction("Feed");
            }
            
            return RedirectToAction("Feed");
        }

        [HttpGet("Logout")]
        public IActionResult Logout ()
        {
            HttpContext.Session.Clear();
            return RedirectToAction ("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    internal class SessionCheckAttribute : Attribute
    {
    }
}