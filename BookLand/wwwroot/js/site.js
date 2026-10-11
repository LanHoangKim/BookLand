/* BOOKLAND - JavaScript phía khách hàng */
(function ($) {
    'use strict';

    // ---------- Thông báo (toast) ----------
    function toast(message, type) {
        if (!message) return;
        const icons = { success: 'fa-circle-check', error: 'fa-circle-exclamation', info: 'fa-circle-info' };
        const $t = $('<div class="toast"></div>').addClass('toast-' + (type || 'info'))
            .append($('<i class="fa-solid"></i>').addClass(icons[type] || icons.info))
            .append($('<span></span>').text(message));
        $('#toast-container').append($t);
        setTimeout(() => { $t.addClass('hide'); setTimeout(() => $t.remove(), 350); }, 3800);
    }
    window.blToast = toast;

    const $tc = $('#toast-container');
    toast($tc.data('success'), 'success');
    toast($tc.data('error'), 'error');
    toast($tc.data('info'), 'info');

    // ---------- Menu danh mục trên header ----------
    $('.cat-toggle').on('click', function (e) {
        e.stopPropagation();
        const $menu = $(this).closest('.cat-menu').toggleClass('open');
        $(this).attr('aria-expanded', $menu.hasClass('open'));
    });
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.cat-menu').length) $('.cat-menu').removeClass('open');
    });
    $('.cat-parent').on('mouseenter', function () {
        $(this).addClass('active').siblings().removeClass('active');
        $('#' + $(this).data('panel')).addClass('active').siblings().removeClass('active');
    });

    // ---------- Slider banner ----------
    $('.hero-slider').each(function () {
        const $slider = $(this);
        const $slides = $slider.find('.hero-slide');
        const $dots = $slider.find('.hero-dots');
        let index = 0;
        let timer;
        $slides.each((i) => $dots.append($('<button type="button" class="hero-dot"></button>').attr('aria-label', 'Banner ' + (i + 1))));
        function show(i) {
            index = (i + $slides.length) % $slides.length;
            $slides.removeClass('active').eq(index).addClass('active');
            $dots.children().removeClass('active').eq(index).addClass('active');
        }
        function start() { clearInterval(timer); timer = setInterval(() => show(index + 1), $slider.data('interval') || 5000); }
        $slider.find('.prev').on('click', () => { show(index - 1); start(); });
        $slider.find('.next').on('click', () => { show(index + 1); start(); });
        $dots.on('click', '.hero-dot', function () { show($(this).index()); start(); });
        show(0);
        start();
    });

    // ---------- Hàng sản phẩm cuộn ngang ----------
    $('.scroll-row').each(function () {
        const $row = $(this);
        const track = $row.find('.row-track')[0];
        if (!track) return;
        const $prev = $row.find('.row-arrow.prev');
        const $next = $row.find('.row-arrow.next');
        function update() {
            $prev.prop('disabled', track.scrollLeft <= 4);
            $next.prop('disabled', track.scrollLeft + track.clientWidth >= track.scrollWidth - 4);
        }
        $prev.on('click', () => track.scrollBy({ left: -track.clientWidth * 0.8 }));
        $next.on('click', () => track.scrollBy({ left: track.clientWidth * 0.8 }));
        $(track).on('scroll', update);
        $(window).on('resize', update);
        update();
    });

    // ---------- Tab ----------
    $('[data-tabs]').on('click', '.tab-btn[data-tab]', function () {
        const $box = $(this).closest('[data-tabs]');
        $box.find('.tab-btn').removeClass('active');
        $(this).addClass('active');
        $box.find('.tab-panel').removeClass('active');
        $box.find('#' + $(this).data('tab')).addClass('active');
    });

    // ---------- Best sellers: chọn sản phẩm trong danh sách xếp hạng ----------
    $('.bs-item').on('mouseenter click', function () {
        const $panel = $(this).closest('.tab-panel');
        $(this).addClass('active').siblings().removeClass('active');
        $panel.find('.bs-preview').removeClass('active');
        $panel.find('#' + $(this).data('preview')).addClass('active');
    });

    // ---------- Đồng hồ đếm ngược Flash Sale (đến hết ngày) ----------
    $('[data-countdown]').each(function () {
        const $c = $(this);
        const pad = (n) => String(n).padStart(2, '0');
        function tick() {
            const now = new Date();
            const end = new Date(now); end.setHours(23, 59, 59, 999);
            let s = Math.max(0, Math.floor((end - now) / 1000));
            $c.find('.cd-h').text(pad(Math.floor(s / 3600)));
            $c.find('.cd-m').text(pad(Math.floor((s % 3600) / 60)));
            $c.find('.cd-s').text(pad(s % 60));
        }
        tick();
        setInterval(tick, 1000);
    });

    // ---------- Trang danh sách: tự gửi form khi đổi bộ lọc ----------
    $(document).on('change', '.auto-submit', function () {
        $('#filter-form').trigger('submit');
    });
    $('.filter-more').on('click', function () {
        const $section = $(this).closest('.collapsible').toggleClass('expanded');
        $(this).contents().first()[0].textContent = $section.hasClass('expanded') ? 'Thu Gọn ' : 'Xem Thêm ';
    });

    // ---------- Thư viện ảnh trang chi tiết ----------
    $('.pd-thumb').on('click', function () {
        $(this).addClass('active').siblings().removeClass('active');
        $('#pd-main-img').attr('src', $(this).data('src'));
    });

    // ---------- Hiện / ẩn mật khẩu ----------
    $('.toggle-password').on('click', function () {
        const $input = $(this).siblings('input');
        const show = $input.attr('type') === 'password';
        $input.attr('type', show ? 'text' : 'password');
        $(this).text(show ? 'Ẩn' : 'Hiện');
    });

    // Nút đăng nhập/đăng ký chuyển màu đỏ khi đã nhập đủ thông tin (giống Fahasa)
    $('form[data-require-filled]').each(function () {
        const $form = $(this);
        const $inputs = $form.find('input.input');
        function check() { $form.toggleClass('not-ready', $inputs.toArray().some((i) => !i.value.trim())); }
        $inputs.on('input', check);
        check();
    });

    // ---------- Ô tăng / giảm số lượng ----------
    function clamp(value, max) {
        let v = parseInt(value, 10);
        if (isNaN(v) || v < 1) v = 1;
        if (max && v > max) v = max;
        return v;
    }
    function refreshButtons($stepper) {
        const max = parseInt($stepper.data('max'), 10) || 0;
        const v = parseInt($stepper.find('.qty-input').val(), 10) || 1;
        $stepper.find('.qty-minus').prop('disabled', v <= 1);
        $stepper.find('.qty-plus').prop('disabled', max > 0 && v >= max);
    }
    $('.qty-stepper').each(function () { refreshButtons($(this)); });

    function stepperMessage($stepper, raw, max) {
        const v = parseInt(raw, 10);
        if (isNaN(v) || v < 1) return 'Số lượng tối thiểu là 1';
        if (max && v > max) return 'Chỉ còn ' + max + ' sản phẩm trong kho';
        return '';
    }

    $(document).on('click', '.qty-minus, .qty-plus', function () {
        const $stepper = $(this).closest('.qty-stepper');
        const $input = $stepper.find('.qty-input');
        const max = parseInt($stepper.data('max'), 10) || 0;
        const delta = $(this).hasClass('qty-plus') ? 1 : -1;
        $input.removeData('raw').val(clamp((parseInt($input.val(), 10) || 1) + delta, max)).trigger('qty:changed');
    });
    $(document).on('change', '.qty-input', function () {
        const $stepper = $(this).closest('.qty-stepper');
        const max = parseInt($stepper.data('max'), 10) || 0;
        const message = stepperMessage($stepper, this.value, max);
        if (message) {
            const $msg = $(this).closest('.cart-item').find('.ci-msg');
            if ($msg.length) $msg.text(message); else toast(message, 'error');
        }
        $(this).data('raw', this.value).val(clamp(this.value, max)).trigger('qty:changed');
    });
    $(document).on('qty:changed', '.qty-input', function () {
        refreshButtons($(this).closest('.qty-stepper'));
    });

    // ---------- Giỏ hàng: cập nhật / xóa bằng AJAX ----------
    const $cartPage = $('[data-cart-page]');
    if ($cartPage.length) {
        const token = $cartPage.find('input[name="__RequestVerificationToken"]').val();
        const timers = {};

        function post(url, data) {
            return $.ajax({ url: url, method: 'POST', data: data, headers: { 'RequestVerificationToken': token, 'X-Requested-With': 'XMLHttpRequest' } });
        }
        function applyTotals(r) {
            $('#cart-subtotal').text(r.subtotal);
            $('#cart-shipping').text(r.shippingFee);
            $('#cart-total').text(r.total);
            $('.cart-lines').text(r.cartCount);
            $('.cart-items-count').text($('.cart-item').length);
            const $count = $('.cart-count').text(r.cartCount).toggleClass('is-empty', r.cartCount === 0);
            $count.removeClass('bump'); void $count[0].offsetWidth; $count.addClass('bump');
            const subtotal = parseInt(String(r.subtotal).replace(/\D/g, ''), 10) || 0;
            const remain = Math.max(0, 300000 - subtotal);
            $('#freeship-progress').css('width', Math.min(100, subtotal / 3000) + '%');
            $('#freeship-text').html(remain > 0
                ? 'Mua thêm <b>' + remain.toLocaleString('vi-VN') + ' đ</b> để được miễn phí vận chuyển'
                : 'Đơn hàng đã được <b>miễn phí vận chuyển</b>');
            if (r.isEmpty) {
                // Giỏ rỗng: ẩn danh sách và nút Thanh toán
                $('#cart-layout').remove();
                $('#checkout-btn').remove();
                $('#cart-empty').prop('hidden', false);
            }
        }

        $cartPage.on('qty:changed', '.qty-input', function () {
            const $item = $(this).closest('.cart-item');
            const id = $item.data('id');
            const qty = $(this).data('raw') !== undefined ? $(this).data('raw') : this.value;   // gửi đúng giá trị người dùng nhập để server kiểm tra
            $(this).removeData('raw');
            clearTimeout(timers[id]);
            timers[id] = setTimeout(function () {
                $item.addClass('updating');
                post('/Cart/Update', { productId: id, quantity: qty })
                    .done(function (r) {
                        $item.find('.qty-input').val(r.quantity);
                        $item.find('.qty-stepper').data('max', r.stock).attr('data-max', r.stock);
                        refreshButtons($item.find('.qty-stepper'));
                        $item.find('.ci-total').text(r.lineTotal);
                        $item.find('.ci-msg').text(r.success ? '' : (r.message || ''));
                        applyTotals(r);
                    })
                    .fail(() => toast('Không cập nhật được giỏ hàng, vui lòng thử lại.', 'error'))
                    .always(() => $item.removeClass('updating'));
            }, 350);
        });

        $cartPage.on('submit', '.remove-form', function (e) {
            e.preventDefault();
            const $item = $(this).closest('.cart-item');
            $item.addClass('updating');
            post('/Cart/Remove', { productId: $item.data('id') })
                .done(function (r) {
                    $item.slideUp(200, function () { $item.remove(); applyTotals(r); });
                    toast(r.message, 'success');
                })
                .fail(() => { $item.removeClass('updating'); toast('Không xóa được sản phẩm.', 'error'); });
        });

        // Không gửi form cập nhật khi nhấn Enter, dùng AJAX thay thế
        $cartPage.on('submit', '.qty-form', (e) => e.preventDefault());
    }
})(jQuery);
