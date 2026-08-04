// ==================== Settings Page JS ====================

$(document).ready(function () {
    // Currency Preview
    updateCurrencyPreview();
    $('#Currency, #CurrencySymbol, #CurrencyPosition, #DecimalPlaces, #ThousandSeparator, #DecimalSeparator').on('change input', function () {
        updateCurrencyPreview();
    });

    // SweetAlert on form submit
    $('#generalSettingsForm, #orgSettingsForm').on('submit', function () {
        Swal.fire({
            title: 'جاري الحفظ...',
            html: 'الرجاء الانتظار',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
    });

    // Show success/error messages
    if (typeof successMessage !== 'undefined' && successMessage) {
        Swal.fire({
            icon: 'success',
            title: 'تم بنجاح',
            text: successMessage,
            timer: 2500,
            showConfirmButton: false
        });
    }
});

// Currency Preview Function
function updateCurrencyPreview() {
    const symbol = $('#CurrencySymbol').val() || 'د.ل';
    const position = $('#CurrencyPosition').val() || 'after';
    const decimals = parseInt($('#DecimalPlaces').val()) || 2;
    const thousandSep = $('#ThousandSeparator').val() || ',';
    const decimalSep = $('#DecimalSeparator').val() || '.';

    let number = 1234.56;
    let parts = number.toFixed(decimals).split('.');
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, thousandSep);
    let formatted = parts.join(decimalSep);

    let result = position === 'before' ? `${symbol} ${formatted}` : `${formatted} ${symbol}`;
    $('#currencyPreview').text(result);
}

// Logo Preview
function previewLogo(input) {
    if (input.files && input.files[0]) {
        const file = input.files[0];

        // Validate size (2MB max)
        if (file.size > 2 * 1024 * 1024) {
            Swal.fire({
                icon: 'error',
                title: 'حجم الملف كبير',
                text: 'يجب أن لا يتجاوز حجم الصورة 2 ميجابايت'
            });
            input.value = '';
            return;
        }

        // Validate type
        if (!file.type.startsWith('image/')) {
            Swal.fire({
                icon: 'error',
                title: 'نوع ملف غير صحيح',
                text: 'يرجى اختيار ملف صورة'
            });
            input.value = '';
            return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
            $('#logoPreview').html(`<img src="${e.target.result}" alt="Logo Preview" />`);
        };
        reader.readAsDataURL(file);
    }
}