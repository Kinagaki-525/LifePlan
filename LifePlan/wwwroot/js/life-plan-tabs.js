(() => {
    const tabButtons = [...document.querySelectorAll('[data-life-plan-tab]')];
    const panes = [...document.querySelectorAll('[data-life-plan-panel]')];
    const resultPanel = document.querySelector('[data-life-plan-result]');
    const tabList = document.querySelector('.life-plan-tabs');

    const activateTab = (tabName) => {
        tabList?.classList.remove('life-plan-tabs--readonly');

        tabButtons.forEach((button) => {
            const isActive = button.dataset.lifePlanTab === tabName;
            button.setAttribute('aria-selected', String(isActive));
            button.setAttribute('tabindex', isActive ? '0' : '-1');
        });

        panes.forEach((pane) => {
            const isActive = pane.dataset.lifePlanPanel === tabName;
            pane.toggleAttribute('hidden', !isActive);
        });

        resultPanel?.setAttribute('hidden', '');
    };

    tabButtons.forEach((button) => {
        button.addEventListener('click', () => activateTab(button.dataset.lifePlanTab));
    });

    document.querySelectorAll('[data-life-plan-next]').forEach((button) => {
        button.addEventListener('click', () => activateTab(button.dataset.lifePlanNext));
    });

    const firstInvalidPane = panes.find((pane) =>
        pane.querySelector('.field-validation-error, .input-validation-error'));

    if (firstInvalidPane) {
        activateTab(firstInvalidPane.dataset.lifePlanPanel);
    }
})();
