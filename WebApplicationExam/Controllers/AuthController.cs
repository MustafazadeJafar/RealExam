﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplicationExam.Models;
using WebApplicationExam.Models.Static;
using WebApplicationExam.ViewModel.AuthVMs;

namespace WebApplicationExam.Controllers;

public class AuthController : Controller
{
    UserManager<AppUser> _userManager {  get; }
    RoleManager<IdentityRole> _roleManager { get; }
    SignInManager<AppUser> _signInManager { get; }

    public AuthController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signInManager)
    {
        this._userManager = userManager;
        this._roleManager = roleManager;
        this._signInManager = signInManager;
    }


    // GET: AuthController/Register
    public ActionResult Register()
    {
        return View();
    }

    // GET: AuthController/Create
    public ActionResult Login()
    {
        return View();
    }

    // POST: AuthController/Register
    [HttpPost]
    public async Task<ActionResult> Register(RegisterVM vm)
    {
        if (await this._userManager.FindByNameAsync(vm.Username) != null) ModelState.AddModelError("", "Username is used");
        if (vm.Password != vm.ConfirmPassword) ModelState.AddModelError("", "Passwords dosen't match");
        if (!ModelState.IsValid) return View(vm);

        AppUser user = vm.ToEntity();
        var result = await this._userManager.CreateAsync(user, vm.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(vm);
        }

        result = await this._userManager.AddToRoleAsync(user, nameof(AuthRoles.User));
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(vm);
        }

        return RedirectToAction(nameof(Login));
    }

    // POST: AuthController/Login
    [HttpPost]
    public async Task<ActionResult> Login(LoginVM vm)
    {
        AppUser user = await this._userManager.FindByNameAsync(vm.Username);
        if (user == null) 
        {
            ModelState.AddModelError("", "Username or password is wrong");
            return View(vm);
        }

        var result = await this._signInManager.PasswordSignInAsync(user, vm.Password, vm.Remember, true);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Username or password is wrong");
            return View(vm);
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    // GET: AuthController/Logout
    public async Task<ActionResult> Logout()
    {
        await this._signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    // GET: AuthController/UpdateRoles
    public async Task<ActionResult> UpdateRoles()
    {
        foreach (var item in Enum.GetNames<AuthRoles>())
        {
            if (!await this._roleManager.RoleExistsAsync(item))
            {
                await this._roleManager.CreateAsync(new IdentityRole(item));
            }
        }

        return Ok();
    }
}
