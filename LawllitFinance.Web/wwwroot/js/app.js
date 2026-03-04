document.addEventListener('change', function (event) {
    if (event.target.matches('[data-auto-submit]')) event.target.form.submit();
    if (event.target.matches('[data-filter-type]')) filterCategoriesByType(event.target);
});

document.addEventListener('click', function (event) {
    var button = event.target.closest('[data-toggle-pwd]');
    if (!button) return;
    var input = document.getElementById(button.dataset.togglePwd);
    if (!input) return;
    var showing = input.type === 'text';
    input.type = showing ? 'password' : 'text';
    button.querySelector('i').className = showing ? 'bi bi-eye' : 'bi bi-eye-slash';
});

document.addEventListener('show.bs.modal', function (event) {
    var typeSelect = event.target.querySelector('[data-filter-type]');
    if (typeSelect) filterCategoriesByType(typeSelect);
});

setTimeout(function () {
    document.querySelectorAll('.alert-dismissible').forEach(function (alertElement) {
        alertElement.addEventListener('transitionend', function () { alertElement.remove(); }, { once: true });
        alertElement.classList.remove('show');
    });
}, 4000);

function filterCategoriesByType(typeSelect) {
    var modalBody = typeSelect.closest('.modal-body');
    if (!modalBody) return;
    var categorySelect = modalBody.querySelector('[data-filter-category]');
    if (!categorySelect) return;
    var type = typeSelect.value;
    var expenseGroup = categorySelect.querySelector('optgroup[label="Despesas"]');
    var incomeGroup = categorySelect.querySelector('optgroup[label="Receitas"]');
    if (expenseGroup) expenseGroup.hidden = type === 'Income';
    if (incomeGroup) incomeGroup.hidden = type === 'Expense';
    var selected = categorySelect.options[categorySelect.selectedIndex];
    if (selected && selected.closest('optgroup') && selected.closest('optgroup').hidden) {
        var firstVisible = Array.from(categorySelect.options).find(function (option) {
            return !option.closest('optgroup') || !option.closest('optgroup').hidden;
        });
        if (firstVisible) categorySelect.value = firstVisible.value;
    }
}
