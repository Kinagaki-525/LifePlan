(() => {
    const childAgeFields = Array.from(document.querySelectorAll('[data-life-plan-child-age="true"]'));
    const educationRows = Array.from(document.querySelectorAll('[data-life-plan-education-row]'));

    if (childAgeFields.length === 0) {
        return;
    }

    const updateEducationRow = (index, isEnabled) => {
        const row = educationRows.find((educationRow) => Number(educationRow.dataset.lifePlanEducationRow) === index);

        if (!row) {
            return;
        }

        row.classList.toggle('life-plan-education-row--disabled', !isEnabled);

        row.querySelectorAll('select').forEach((field) => {
            field.disabled = !isEnabled;

            if (!isEnabled) {
                field.value = '';
                field.classList.remove('input-validation-error');
                field.setAttribute('aria-invalid', 'false');

                const validationMessage = row.querySelector(`[data-valmsg-for="${field.name}"]`);
                validationMessage?.classList.remove('field-validation-error');
                validationMessage?.classList.add('field-validation-valid');

                if (validationMessage) {
                    validationMessage.textContent = '';
                }
            }
        });
    };

    const updateChildAgeFields = () => {
        childAgeFields.forEach((field, index) => {
            const isEnabled = index === 0 || childAgeFields[index - 1].value !== '';
            const hasChildAge = isEnabled && field.value !== '';

            field.disabled = !isEnabled;
            field.closest('.life-plan-child-field')?.classList.toggle('life-plan-child-field--disabled', !isEnabled);

            if (!isEnabled) {
                field.value = '';
            }

            updateEducationRow(index, hasChildAge);
        });
    };

    childAgeFields.forEach((field) => {
        field.addEventListener('change', updateChildAgeFields);
    });

    updateChildAgeFields();
})();
