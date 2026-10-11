using Microsoft.EntityFrameworkCore;

namespace BookLand.Models;

// Cấu hình bổ sung, tách riêng để không bị mất khi scaffold lại từ database
public partial class BookLandContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Cột bit có DEFAULT 1: luôn gửi giá trị lên DB, nếu không giá trị false sẽ bị bỏ qua khi thêm mới
        modelBuilder.Entity<Product>().Property(e => e.IsActive).ValueGeneratedNever();
        modelBuilder.Entity<Category>().Property(e => e.IsActive).ValueGeneratedNever();
    }
}
