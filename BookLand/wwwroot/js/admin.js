/* BOOKLAND - JavaScript trang quản trị */
(function ($) {
    'use strict';

    // Mở/đóng sidebar trên màn hình nhỏ
    $('.sidebar-toggle').on('click', () => $('body').toggleClass('sidebar-open'));

    // Xem trước ảnh trước khi upload
    $(document).on('change', 'input[type=file][data-preview]', function () {
        const target = $(this).data('preview');
        const file = this.files && this.files[0];
        if (!file) return;
        const reader = new FileReader();
        reader.onload = (e) => $(target).attr('src', e.target.result);
        reader.readAsDataURL(file);
    });

    // Gửi form lọc khi đổi lựa chọn
    $(document).on('change', '.auto-submit', function () { $(this).closest('form').trigger('submit'); });

    // Tự ẩn thông báo sau vài giây
    setTimeout(() => $('.admin-content > .alert-success').fadeOut(400), 4000);
})(jQuery);
