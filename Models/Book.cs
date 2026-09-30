using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author is required")]
        [StringLength(150)]
        public string Author { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "ISBN")]
        public string ISBN { get; set; } = string.Empty;

        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Range(0, 1000, ErrorMessage = "Total copies must be between 0 and 1000")]
        [Display(Name = "Total Copies")]
        public int TotalCopies { get; set; } = 1;

        [Range(0, 1000)]
        [Display(Name = "Available Copies")]
        public int AvailableCopies { get; set; } = 1;

        [DataType(DataType.Date)]
        [Display(Name = "Added On")]
        public DateTime AddedOn { get; set; } = DateTime.Now;

        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();

        public bool IsAvailable => AvailableCopies > 0;
    }
}
