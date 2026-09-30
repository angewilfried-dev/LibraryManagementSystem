using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index(string? searchString)
        {
            var books = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                books = books.Where(b =>
                    b.Title.Contains(searchString) ||
                    b.Author.Contains(searchString) ||
                    b.ISBN.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;
            return View(await books.OrderBy(b => b.Title).ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.BorrowRecords)
                    .ThenInclude(r => r.Member)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null) return NotFound();

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Author,ISBN,Category,TotalCopies")] Book book)
        {
            if (ModelState.IsValid)
            {
                book.AvailableCopies = book.TotalCopies;
                book.AddedOn = DateTime.Now;
                _context.Add(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"\"{book.Title}\" was added to the catalog.";
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,ISBN,Category,TotalCopies,AvailableCopies,AddedOn")] Book book)
        {
            if (id != book.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"\"{book.Title}\" was updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);
            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                bool hasActiveLoans = await _context.BorrowRecords
                    .AnyAsync(r => r.BookId == id && r.ReturnDate == null);

                if (hasActiveLoans)
                {
                    TempData["Error"] = "This book cannot be deleted because it currently has active loans.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"\"{book.Title}\" was removed from the catalog.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
