namespace BookLand.Helpers;

// Kiểm tra và lưu ảnh upload vào wwwroot/images/{folder}/uploads
public static class ImageHelper
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    public const long MaxBytes = 2 * 1024 * 1024;

    // Trả về thông báo lỗi, hoặc null nếu file hợp lệ
    public static string? Validate(IFormFile file)
    {
        string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext)) return "Chỉ chấp nhận ảnh .jpg, .jpeg, .png, .webp, .gif";
        if (file.Length == 0) return "File ảnh rỗng";
        if (file.Length > MaxBytes) return "Ảnh không được vượt quá 2 MB";
        return null;
    }

    public static async Task<string> SaveAsync(IFormFile file, IWebHostEnvironment env, string folder)
    {
        string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        string fileName = $"{Guid.NewGuid():N}{ext}";
        string dir = Path.Combine(env.WebRootPath, "images", folder, "uploads");
        Directory.CreateDirectory(dir);

        await using (var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        return $"/images/{folder}/uploads/{fileName}";
    }

    // Chỉ xóa ảnh do người dùng upload, không xóa ảnh mẫu đi kèm project
    public static void DeleteUploaded(string? url, IWebHostEnvironment env)
    {
        if (string.IsNullOrEmpty(url) || !url.Contains("/uploads/")) return;
        string path = Path.Combine(env.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(path)) File.Delete(path);
    }
}
