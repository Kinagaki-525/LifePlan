(() => {
    const configElement = document.getElementById('life-plan-client-validation');

    if (!configElement?.textContent) {
        return;
    }

    const config = JSON.parse(configElement.textContent);

    if (window.jQuery?.validator && config.numberMessage) {
        window.jQuery.validator.messages.number = config.numberMessage;
    }

    Object.entries(config.rules ?? {}).forEach(([fieldName, rule]) => {
        const field = document.getElementsByName(fieldName)[0];

        if (!field) {
            return;
        }

        Object.entries(rule.attributes ?? {}).forEach(([attributeName, value]) => {
            field.setAttribute(attributeName, value);
        });

        Object.entries(rule.messages ?? {}).forEach(([ruleName, message]) => {
            field.setAttribute(`data-msg-${ruleName}`, message);
        });
    });
})();
