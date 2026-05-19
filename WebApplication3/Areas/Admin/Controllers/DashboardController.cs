using EduDirectory3.Data;
using EduDirectory3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduDirectory3.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsuarios = _userManager.Users.Count();
            ViewBag.TotalInstituciones = await _context.Institucion.CountAsync();
            ViewBag.TotalSedes = await _context.Sede.CountAsync();
            ViewBag.TotalReportados = await _context.Evaluacion.CountAsync(e => e.Reportado == true);

            ViewData["Title"] = "Dashboard Admin";
            return View();
        }
    }
}
