using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var overdueActiveLoans = await _context.BorrowRecords
                .Where(r => r.ReturnDate == null && r.DueDate < DateTime.Now)
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalBooks = await _context.Books.SumAsync(b => b.TotalCopies),
                DistinctTitles = await _context.Books.CountAsync(),
                TotalMembers = await _context.Members.CountAsync(),
                BooksCurrentlyBorrowed = await _context.BorrowRecords.CountAsync(r => r.ReturnDate == null),
                OverdueCount = overdueActiveLoans.Count,
                OutstandingFines = overdueActiveLoans.Sum(r => r.Fine),
                RecentBorrows = await _context.BorrowRecords
                    .Include(r => r.Book)
                    .Include(r => r.Member)
                    .OrderByDescending(r => r.BorrowDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }
    }

    public class DashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int DistinctTitles { get; set; }
        public int TotalMembers { get; set; }
        public int BooksCurrentlyBorrowed { get; set; }
        public int OverdueCount { get; set; }
        public decimal OutstandingFines { get; set; }
        public List<BorrowRecord> RecentBorrows { get; set; } = new();
    }
}
