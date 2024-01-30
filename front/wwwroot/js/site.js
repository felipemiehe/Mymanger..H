// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function showLoadingModal() {
    $('#loadingModal').modal('show');
}

function hideLoadingModal() {
    $('#loadingModal').modal('hide');
}

function showErrorModal(message, title, modalId) {
    // Se modalId não for fornecido, use o ID padrão 'errorModal'
    modalId = modalId || 'errorModal';

    var modalSelector = '#' + modalId;

    $(modalSelector + ' .modal-body #errorMessage').text(message);
    $(modalSelector + ' .modal-header #errorModalTitle').text(title);

    var modal = new bootstrap.Modal(document.getElementById(modalId));
    modal.show();
}


function showSuccessNotification(message) {
    toastr.success(message, 'Sucesso');
}

function showErrorNotification(message) {
    toastr.error(message, 'Erro');
}

function showInfoNotification(message) {
    toastr.info(message, 'Informação');
}

function showWarningNotification(message) {
    toastr.warning(message, 'Aviso');
}


toastr.options = {
    "closeButton": false,
    "debug": false,
    "newestOnTop": false,
    "progressBar": true,
    "positionClass": "toast-top-center",
    "preventDuplicates": false,
    "onclick": null,
    "showDuration": "300",
    "hideDuration": "500",
    "timeOut": "1500",
    "extendedTimeOut": "1000",
    "showEasing": "swing",
    "hideEasing": "linear",
    "showMethod": "fadeIn",
    "hideMethod": "fadeOut"
}


function copiarElemento(elemento) {
    var inputElement = document.createElement('input');
    inputElement.value = elemento;

    document.body.appendChild(inputElement);
    inputElement.select();
    inputElement.setSelectionRange(0, 99999);
    document.execCommand('copy');
    document.body.removeChild(inputElement);

    // Exibe alguma notificação
    showSuccessNotification('copiado!');
}

$(function () {
    var lastScrollTop = 0;

    $(window).scroll(function () {
        var currentScrollTop = $(window).scrollTop();

        if (currentScrollTop > lastScrollTop) {
            // Rolando para baixo
            $('#stickytypeheader').css({ position: 'fixed', top: '0px', right: '0px', 'z-index': 999 });
        } else {
            // Rolando para cima
            $('#stickytypeheader').css({ position: 'static', top: '0px', right: 'auto' });
        }

        lastScrollTop = currentScrollTop;
    });
});