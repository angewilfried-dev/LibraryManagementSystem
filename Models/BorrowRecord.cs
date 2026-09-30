using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class BorrowRecord
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Book")]
        public int BookId { get; set; }

        [ForeignKey("BookId")]
        public Book? Book { get; set; }

        [Required]
        [Display(Name = "Member")]
        public int MemberId { get; set; }

        [ForeignKey("MemberId")]
        public Member? Member { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Borrow Date")]
        public DateTime BorrowDate { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(14);

        [DataType(DataType.Date)]
        [Display(Name = "Return Date")]
        public DateTime? ReturnDate { get; set; }

        public bool IsReturned => ReturnDate.HasValue;

        [NotMapped]
        public bool IsOverdue => !IsReturned && DateTime.Now.Date > DueDate.Date;

        [NotMapped]
        public string Status => IsReturned ? "Returned" : (IsOverdue ? "Overdue" : "Borrowed");

        /// <summary>Number of days the loan is overdue (0 if not overdue).</summary>
        [NotMapped]
        public int OverdueDays
        {
            get
            {
                var referenceDate = ReturnDate ?? DateTime.Now;
                var days = (referenceDate.Date - DueDate.Date).Days;
                return days > 0 ? days : 0;
            }
        }

        /// <summary>Fine owed for this loan, based on LibrarySettings.FinePerDayOverdue.</summary>
        [NotMapped]
        public decimal Fine => OverdueDays * LibraryManagementSystem.Helpers.LibrarySettings.FinePerDayOverdue;
    }
}
