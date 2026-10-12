(() => {
    const area = document.querySelector('[data-life-plan-cash-flow]');
    const scroller = area?.querySelector('.life-plan-cash-flow');
    const table = scroller?.querySelector('table');

    if (!area || !scroller || !table?.tHead) {
        return;
    }

    // 表の枠は横スクロールのためにスクロールコンテナになっており、元の見出しはページの縦スクロールで固定できない。
    // スマートフォン幅だけ、見出しの複製を表の枠の外に置いて縦に固定し、横位置を表の枠に合わせる。
    const compactQuery = window.matchMedia('(max-width: 575.98px)');
    const stickyAreaClass = 'life-plan-cash-flow-area--sticky';
    const headHeightProperty = '--life-plan-cash-flow-head-height';
    let stickyHead = null;
    let resizeObserver = null;

    const syncScroll = () => {
        if (stickyHead) {
            stickyHead.scrollLeft = scroller.scrollLeft;
        }
    };

    const syncSize = () => {
        if (!stickyHead) {
            return;
        }

        const tableWidth = table.getBoundingClientRect().width;

        if (tableWidth === 0) {
            return;
        }

        // 幅をすべて読み取ってから書き込み、列ごとにレイアウトの再計算が起きないようにする
        const sourceWidths = [...table.tHead.rows[0].cells].map((cell) => `${cell.getBoundingClientRect().width}px`);
        const cloneTable = stickyHead.querySelector('table');
        const cloneCells = [...cloneTable.tHead.rows[0].cells];

        cloneTable.style.width = `${tableWidth}px`;
        cloneTable.style.minWidth = `${tableWidth}px`;
        sourceWidths.forEach((width, index) => {
            cloneCells[index].style.width = width;
            cloneCells[index].style.minWidth = width;
            cloneCells[index].style.maxWidth = width;
        });

        // 罫線を重ねる表では、表の高さが thead の高さより下側の罫線のぶん大きくなるため、複製の表の高さで重ねる
        area.style.setProperty(headHeightProperty, `${cloneTable.getBoundingClientRect().height}px`);
        syncScroll();
    };

    const enable = () => {
        if (stickyHead) {
            return;
        }

        const cloneTable = document.createElement('table');
        cloneTable.className = table.className;
        cloneTable.append(table.tHead.cloneNode(true));
        cloneTable.querySelectorAll('[id]').forEach((element) => element.removeAttribute('id'));

        stickyHead = document.createElement('div');
        stickyHead.className = 'life-plan-cash-flow__sticky-head';
        stickyHead.setAttribute('aria-hidden', 'true');
        stickyHead.inert = true;
        stickyHead.append(cloneTable);

        scroller.before(stickyHead);
        area.classList.add(stickyAreaClass);
        scroller.addEventListener('scroll', syncScroll, { passive: true });

        if ('ResizeObserver' in window) {
            resizeObserver = new ResizeObserver(syncSize);
            resizeObserver.observe(table);
        }

        syncSize();
    };

    const disable = () => {
        if (!stickyHead) {
            return;
        }

        resizeObserver?.disconnect();
        resizeObserver = null;
        scroller.removeEventListener('scroll', syncScroll);
        stickyHead.remove();
        stickyHead = null;
        area.classList.remove(stickyAreaClass);
        area.style.removeProperty(headHeightProperty);
    };

    const update = () => {
        if (compactQuery.matches) {
            enable();
        } else {
            disable();
        }
    };

    // iOS Safari 14 未満の MediaQueryList は addEventListener を持たないため、addListener で代替する
    if (typeof compactQuery.addEventListener === 'function') {
        compactQuery.addEventListener('change', update);
    } else {
        compactQuery.addListener(update);
    }
    update();
})();
