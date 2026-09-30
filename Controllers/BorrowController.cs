using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class BorrowController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BorrowController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Borrow  (list of all borrow records)
        public async Task<IActionResult> Index(string? status)
        {
            var records = _context.BorrowRecords
                .Include(r => r.Book)
                .Include(r => r.Member)
                .AsQueryable();

            if (status == "active")
                records = records.Where(r => r.ReturnDate == null);
            else if (status == "overdue")
                records = records.Where(r => r.ReturnDate == null && r.DueDate < DateTime.Now);
            else if (status == "returned")
                records = records.Where(r => r.ReturnDate != null);

            ViewData["CurrentStatus"] = status;

            var list = await records.OrderByDescending(r => r.BorrowDate).ToListAsync();
            return View(list);
        }

        // GET: Borrow/Create  (issue a book to a member)
        public async Task<IActionResult> Create()
        {
            ViewBag.Books = await _context.Books
                .Where(b => b.AvailableCopies > 0)
                .OrderBy(b => b.Title)
                .ToListAsync();
            ViewBag.Members = await _context.Members.OrderBy(m => m.FullName).ToListAsync();
            return View();
        }

        // POST: Borrow/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int bookId, int memberId, int loanDays = 14)
        {
            var book = await _context.Books.FindAsync(bookId);
            var member = await _context.Members.FindAsync(memberId);

            if (book == null || member == null)
            {
                ModelState.AddModelError(string.Empty, "Please select a valid book and member.");
            }
            else if (book.AvailableCopies <= 0)
            {
                ModelState.AddModelError(string.Empty, $"\"{book.Title}\" has no available copies right now.");
            }

            if (!ModelState.IsValid || book == null || member == null)
            {
                ViewBag.Books = await _context.Books.Where(b => b.AvailableCopies > 0).OrderBy(b => b.Title).ToListAsync();
                ViewBag.Members = await _context.Members.OrderBy(m => m.FullName).ToListAsync();
                return View();
            }

            var record = new BorrowRecord
            {
                BookId = bookId,
                MemberId = memberId,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(loanDays)
            };

            book.AvailableCopies -= 1;

            _context.BorrowRecords.Add(record);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"\"{book.Title}\" was issued to {member.FullName}. Due on {record.DueDate:MMM dd, yyyy}.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Borrow/Return/5  (confirm return)
        public async Task<IActionResult> Return(int? id)
        {
            if (id == null) return NotFound();

            var record = await _context.BorrowRecords
                .Include(r => r.Book)
                .Include(r => r.Member)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (record == null) return NotFound();
            if (record.IsReturned)
            {
                TempData["Error"] = "This book has already been returned.";
                return RedirectToAction(nameof(Index));
            }

            return View(record);
        }

        // POST: Borrow/Return/5
        [HttpPost, ActionName("Return")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnConfirmed(int id)
        {
            var record = await _context.BorrowRecords
                .Include(r => r.Book)
                .Include(r => r.Member)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (record == null) return NotFound();

            if (!record.IsReturned)
            {
                record.ReturnDate = DateTime.Now;
                if (record.Book != null)
                {
                    record.Book.AvailableCopies += 1;
                }
                await _context.SaveChangesAsync();
                TempData["Success"] = $"\"{record.Book?.Title}\" returned by {record.Member?.FullName}.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
