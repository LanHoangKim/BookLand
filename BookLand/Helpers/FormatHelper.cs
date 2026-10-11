using System.Globalization;

namespace BookLand.Helpers;

public static class FormatHelper
{
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    // 63200 -> "63.200 đ"
    public static string ToVnd(this decimal value) => value.ToString("#,##0", Vi) + " đ";

    public static string ToVnd(this decimal? value) => value.HasValue ? value.Value.ToString("#,##0", Vi) + " đ" : "";

    public static string ToNumber(this decimal value) => value.ToString("#,##0", Vi);

    public static string ToNumber(this int value) => value.ToString("#,##0", Vi);

    // 1250 -> "1.2k" (giống cách hiển thị số lượng đã bán)
    public static string ToShortCount(this int value) =>
        value >= 1000 ? (value / 1000d).ToString("0.#", Vi) + "k" : value.ToString(Vi);

    public static string StatusCss(string? status) => status switch
    {
        "Chờ xác nhận" => "status-pending",
        "Đang giao" => "status-shipping",
        "Đã giao" => "status-done",
        "Đã hủy" => "status-cancel",
        _ => "status-pending"
    };
}
