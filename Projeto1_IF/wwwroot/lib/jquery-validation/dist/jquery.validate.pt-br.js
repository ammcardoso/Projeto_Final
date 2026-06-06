/*
 * Adriana Cardoso
 * Tradução em Português-Brasil (pt-BR) para o plugin jQuery Validation 
 */
jQuery.extend(jQuery.validator.messages, {
    required: "Este campo é obrigatório.",
    date: "Por favor, forneça uma data válida.",
    dateISO: "Por favor, forneça uma data válida (ISO).",
    number: "Por favor, forneça um número válido.",
    digits: "Por favor, forneça somente números.",
    accept: "Por favor, forneça um valor com uma extensão válida.",
    maxlength: jQuery.validator.format("Por favor, forneça não mais que {0} caracteres."),
    minlength: jQuery.validator.format("Por favor, forneça ao menos {0} caracteres."),
    rangelength: jQuery.validator.format("Por favor, forneça um valor entre {0} e {1} caracteres."),
    range: jQuery.validator.format("Por favor, forneça um valor entre {0} e {1}."),
    max: jQuery.validator.format("Por favor, forneça um valor menor ou igual a {0}."),
    min: jQuery.validator.format("Por favor, forneça um valor maior ou igual a {0}.")
});

/*
 * Permite datas no formato DD/MM/AAAA e números decimais usando vírgula
 */
jQuery.extend(jQuery.validator.methods, {
    date: function (value, element) {
        return this.optional(element) || /^\d\d?[\/\-]\d\d?[\/\-]\d\d\d\d$/.test(value);
    },
    number: function (value, element) {
        return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:\.\d{3})+)(?:,\d+)?$/.test(value);
    }
});

/* Validação direta e simples de formato de CPF (000.000.000-00) */
jQuery.validator.addMethod("cpf", function (value, element) {
    return this.optional(element) || /^\d{3}\.\d{3}\.\d{3}-\d{2}$/.test(value);
}, "Por favor, digite um CPF válido.");


$(document).ready(function () {
    $(":input[data-val-required]").attr("data-val-required", "Este campo é obrigatório.");
    $("form").removeData("validator").removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse("form");
});

$('[name="cpf"]').mask('000.000.000-00');