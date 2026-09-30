using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Members
        public async Task<IActionResult> Index(string? searchString)
        {
            var members = _context.Members.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                members = members.Where(m =>
                    m.FullName.Contains(searchString) ||
                    m.Email.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;
            return View(await members.OrderBy(m => m.FullName).ToListAsync());
        }

        // GET: Members/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.Members
                .Include(m => m.BorrowRecords)
                    .ThenInclude(r => r.Book)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null) return NotFound();

            return View(member);
        }

        // GET: Members/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Members/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Email,Phone,MembershipType")] Member member)
        {
            if (ModelState.IsValid)
            {
                member.JoinedOn = DateTime.Now;
                _context.Add(member);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"{member.FullName} was registered as a member.";
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.Members.FindAsync(id);
            if (member == null) return NotFound();

            return View(member);
        }

        // POST: Members/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email,Phone,MembershipType,JoinedOn")] Member member)
        {
            if (id != member.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"{member.FullName}'s details were updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.Members.FirstOrDefaultAsync(m => m.Id == id);
            if (member == null) return NotFound();

            return View(member);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                bool hasActiveLoans = await _context.BorrowRecords
                    .AnyAsync(r => r.MemberId == id && r.ReturnDate == null);

                if (hasActiveLoans)
                {
                    TempData["Error"] = "This member cannot be deleted because they currently have borrowed books.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Members.Remove(member);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"{member.FullName} was removed.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.Id == id);
        }
    }
}
