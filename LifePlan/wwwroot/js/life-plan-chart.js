(() => {
  const seriesDefinitions = [
    { key: 'totalIncomeManYen', label: '収入合計', colorVariable: '--ui-color-chart-income' },
    { key: 'totalExpenseManYen', label: '支出合計', colorVariable: '--ui-color-chart-expense' },
    { key: 'savingsBalanceWithoutReturnManYen', label: '貯蓄合計（0%運用）', colorVariable: '--ui-color-chart-savings-without-return' },
    { key: 'savingsBalanceWithReturnManYen', label: '貯蓄合計（想定年利運用）', colorVariable: '--ui-color-chart-savings-with-return' }
  ];

  const initialize = () => {
    const root = document.querySelector('[data-life-plan-chart]');
    const canvas = root?.querySelector('[data-life-plan-chart-canvas]');
    const canvasWrap = root?.querySelector('[data-life-plan-chart-canvas-wrap]');
    const tooltip = root?.querySelector('[data-life-plan-chart-tooltip]');
    const fallback = root?.querySelector('[data-life-plan-chart-fallback]');
    const guide = root?.querySelector('[data-life-plan-chart-guide]');
    const seriesOptions = root?.querySelector('[data-life-plan-chart-series-options]');
    const detail = root?.querySelector('[data-life-plan-chart-detail]');
    const yearSelectWrap = root?.querySelector('[data-life-plan-chart-year-select-wrap]');
    const yearSelect = root?.querySelector('[data-life-plan-chart-year-select]');
    const data = readChartData();

    if (!root || !canvas || data.length === 0) {
      showFallback(fallback);
      return;
    }

    const context = canvas.getContext('2d');

    if (!context) {
      showFallback(fallback);
      return;
    }

    const seriesControls = renderSeriesControls(seriesOptions);

    const getVisibleSeries = () => {
      if (seriesControls.length === 0) {
        return seriesDefinitions;
      }

      return seriesControls
        .filter((control) => control.input.checked)
        .map((control) => control.series);
    };

    // 固定中の年度（クリック・タップ・キー操作）と、マウスのホバーによる一時表示の年度は別に持つ
    let selectedIndex = null;
    let hoverIndex = null;
    let hoverPointer = null;
    let hoverDismissed = false;
    let plotArea = null;
    let renderedDetailKey = null;

    const getVisibleHoverIndex = () => (hoverDismissed ? null : hoverIndex);

    const redraw = () => {
      const visibleSeries = getVisibleSeries();
      const visibleHoverIndex = getVisibleHoverIndex();
      const detailKey = `${selectedIndex}|${visibleSeries.map((series) => series.key).join(',')}`;

      // 内容が変わらない再描画（リサイズなど）で読み上げが繰り返されないよう、詳細表示は変化時のみ更新する
      if (detailKey !== renderedDetailKey) {
        renderDetail(detail, data, visibleSeries, selectedIndex);
        renderedDetailKey = detailKey;
      }

      plotArea = drawChart(canvas, context, data, visibleSeries, selectedIndex, visibleHoverIndex) ?? plotArea;
      renderTooltip(tooltip, canvas, data, visibleSeries, visibleHoverIndex, hoverPointer, plotArea);
    };

    const selectIndex = (index) => {
      if (index === selectedIndex) {
        return;
      }

      selectedIndex = index;
      syncYearSelect(yearSelect, selectedIndex);
      redraw();
    };

    // ツールチップの位置は年度が変わったときだけ決め直す。同じ年度のまま縦に動かせば、ツールチップの上へ移れる
    const setHoverIndex = (index, pointer = null) => {
      if (index === hoverIndex) {
        return;
      }

      hoverIndex = index;
      hoverPointer = pointer;
      hoverDismissed = false;
      redraw();
    };

    const dismissHover = () => {
      if (getVisibleHoverIndex() === null) {
        return;
      }

      hoverDismissed = true;
      redraw();
    };

    seriesControls.forEach((control) => {
      control.input.addEventListener('change', redraw);
    });

    bindYearSelection(canvas, data, () => plotArea, () => selectedIndex, selectIndex, setHoverIndex);
    bindHoverEnd(canvasWrap, () => setHoverIndex(null), dismissHover);
    bindTooltipSelection(tooltip, getVisibleHoverIndex, selectIndex);
    bindYearSelect(yearSelectWrap, yearSelect, data, () => selectedIndex, selectIndex);

    guide?.removeAttribute('hidden');
    redraw();

    // 画面幅が変わると一時表示の位置がずれるため、一時表示を終えてから描き直す
    const handleResize = () => {
      hoverIndex = null;
      hoverDismissed = false;
      redraw();
    };

    if ('ResizeObserver' in window) {
      const observer = new ResizeObserver(handleResize);
      observer.observe(canvas.parentElement);
      return;
    }

    window.addEventListener('resize', handleResize);
  };

  const readChartData = () => {
    const script = document.getElementById('life-plan-chart-data');

    if (!script?.textContent) {
      return [];
    }

    try {
      const data = JSON.parse(script.textContent);

      return Array.isArray(data) ? data : [];
    } catch {
      return [];
    }
  };

  const bindYearSelection = (canvas, data, getPlotArea, getSelectedIndex, selectIndex, setHoverIndex) => {
    let activePointerId = null;
    let indexBeforeTouch = null;

    const indexFromEvent = (event) => {
      const plotArea = getPlotArea();

      if (!plotArea) {
        return null;
      }

      const rect = canvas.getBoundingClientRect();

      return toNearestIndex(event.clientX - rect.left, data.length, plotArea);
    };

    const selectFromEvent = (event) => {
      const index = indexFromEvent(event);

      if (index !== null) {
        selectIndex(index);
      }
    };

    canvas.addEventListener('pointerdown', (event) => {
      // ピンチ操作などの2本目以降の指は、1本目の操作中の状態を上書きしないよう無視する
      if (!event.isPrimary || (event.pointerType === 'mouse' && event.button !== 0)) {
        return;
      }

      activePointerId = event.pointerId;
      indexBeforeTouch = getSelectedIndex();

      // タッチは縦スクロールの開始と区別するため、指を離すか横に動かすまで確定しない
      if (event.pointerType !== 'touch') {
        // ボタンを押している間（クリック・ドラッグ）は一時表示を出さない
        setHoverIndex(null);
        canvas.setPointerCapture?.(event.pointerId);
        selectFromEvent(event);
      }
    });

    canvas.addEventListener('pointermove', (event) => {
      if (event.pointerId === activePointerId) {
        selectFromEvent(event);
        return;
      }

      // ホバーによる一時表示はマウスだけを対象とし、タッチ・ペンでは出さない
      if (event.pointerType === 'mouse' && event.buttons === 0) {
        const rect = canvas.getBoundingClientRect();

        setHoverIndex(indexFromEvent(event), { x: event.clientX - rect.left, y: event.clientY - rect.top });
      }
    });

    canvas.addEventListener('pointerup', (event) => {
      if (event.pointerId === activePointerId) {
        selectFromEvent(event);
        activePointerId = null;
      }
    });

    // 縦スクロールやピンチ操作が始まった場合は、操作前の選択状態（未選択を含む）に戻す
    canvas.addEventListener('pointercancel', (event) => {
      if (event.pointerId === activePointerId) {
        activePointerId = null;
        selectIndex(indexBeforeTouch);
      }
    });
  };

  // ツールチップはグラフ枠の中にあるため、グラフからツールチップへ移っても一時表示を続ける
  const bindHoverEnd = (canvasWrap, endHover, dismissHover) => {
    canvasWrap?.addEventListener('pointerleave', (event) => {
      if (event.pointerType === 'mouse') {
        endHover();
      }
    });

    document.addEventListener('keydown', (event) => {
      if (event.key === 'Escape') {
        dismissHover();
      }
    });
  };

  // ツールチップをクリックすると、表示中の年度を固定する（ツールチップ上ではグラフにクリックが届かないため）
  const bindTooltipSelection = (tooltip, getVisibleHoverIndex, selectIndex) => {
    tooltip?.addEventListener('click', (event) => {
      const index = getVisibleHoverIndex();
      const selection = window.getSelection?.();

      // 金額をドラッグで選択してコピーする操作では固定しない
      if (selection && !selection.isCollapsed && tooltip.contains(selection.anchorNode)) {
        return;
      }

      if (event.button === 0 && index !== null) {
        selectIndex(index);
      }
    });
  };

  // 一覧からの年度選択（PC・スマートフォンとも常に表示。キーボード・支援技術からも使う）。未選択は選べない先頭の項目で表す
  const bindYearSelect = (yearSelectWrap, yearSelect, data, getSelectedIndex, selectIndex) => {
    if (!yearSelect) {
      return;
    }

    const placeholder = document.createElement('option');
    placeholder.value = '';
    placeholder.textContent = '年度を選択';
    placeholder.disabled = true;
    placeholder.selected = true;

    yearSelect.replaceChildren(placeholder, ...data.map((point, index) => {
      const option = document.createElement('option');
      option.value = String(index);
      option.textContent = `${point.year}年`;

      return option;
    }));

    yearSelect.addEventListener('change', () => {
      if (yearSelect.value !== '') {
        selectIndex(Number(yearSelect.value));
      }
    });

    yearSelect.addEventListener('keydown', (event) => {
      const currentIndex = getSelectedIndex();
      const lastIndex = data.length - 1;
      const nextIndex = {
        ArrowLeft: currentIndex === null ? 0 : Math.max(0, currentIndex - 1),
        ArrowRight: currentIndex === null ? 0 : Math.min(lastIndex, currentIndex + 1),
        Home: 0,
        End: lastIndex
      }[event.key];

      if (nextIndex === undefined) {
        return;
      }

      event.preventDefault();
      selectIndex(nextIndex);
    });

    yearSelectWrap?.removeAttribute('hidden');
  };

  const syncYearSelect = (yearSelect, selectedIndex) => {
    if (yearSelect) {
      yearSelect.value = selectedIndex === null ? '' : String(selectedIndex);
    }
  };

  const toNearestIndex = (x, count, plotArea) => {
    const plotWidth = plotArea.width - plotArea.left - plotArea.right;

    if (count <= 1 || plotWidth <= 0) {
      return 0;
    }

    const index = Math.round(((x - plotArea.left) / plotWidth) * (count - 1));

    return Math.min(count - 1, Math.max(0, index));
  };

  const renderDetail = (detail, data, visibleSeries, selectedIndex) => {
    if (!detail) {
      return;
    }

    detail.removeAttribute('hidden');

    if (selectedIndex === null) {
      detail.replaceChildren(createHint('年度が選択されていません'));
      return;
    }

    const point = data[selectedIndex];

    detail.replaceChildren(
      createYearHeading('life-plan-chart__detail-year', point.year, '固定中'),
      createSeriesValues(point, visibleSeries)
    );
  };

  const renderTooltip = (tooltip, canvas, data, visibleSeries, hoverIndex, pointer, plotArea) => {
    if (!tooltip) {
      return;
    }

    if (hoverIndex === null || !plotArea) {
      tooltip.setAttribute('hidden', '');
      tooltip.replaceChildren();
      return;
    }

    const point = data[hoverIndex];

    tooltip.replaceChildren(
      createYearHeading('life-plan-chart__tooltip-year', point.year, 'カーソル位置'),
      createSeriesValues(point, visibleSeries)
    );
    tooltip.removeAttribute('hidden');
    positionTooltip(tooltip, canvas, hoverIndex, data.length, pointer, plotArea);
  };

  // ホバー中の年度の縦線を隠さないよう、縦線の右（収まらなければ左）に置く。
  // 同じ年度のまま横に動いてもツールチップに乗らないよう、その年度になる横の範囲（カーソルの高さ）と重ならない
  // 左右・上下の組み合わせを、カーソルと反対側の半分から優先して選ぶ。グラフ枠からははみ出さない。
  // 同じ半分に置く場合と、どの組み合わせでも重なる場合は、ツールチップがホバーの更新とクリックを妨げないよう、ポインター操作を透過させる
  const positionTooltip = (tooltip, canvas, index, count, pointer, plotArea) => {
    const container = tooltip.offsetParent;

    if (!container) {
      return;
    }

    const gap = 12;
    const margin = 4;
    const x = toX(index, count, plotArea);
    const plotWidth = plotArea.width - plotArea.left - plotArea.right;
    const halfStep = count <= 1 ? plotWidth / 2 : plotWidth / (count - 1) / 2;
    const bandLeft = canvas.offsetLeft + Math.max(plotArea.left, x - halfStep) - margin;
    const bandRight = canvas.offsetLeft + Math.min(plotArea.width - plotArea.right, x + halfStep) + margin;
    const anchorX = canvas.offsetLeft + x;
    const width = tooltip.offsetWidth;
    const height = tooltip.offsetHeight;
    const clampLeft = (left) => Math.max(margin, Math.min(left, container.clientWidth - width - margin));
    const clampTop = (top) => Math.max(margin, Math.min(canvas.offsetTop + top, container.clientHeight - height - margin));

    // 反対側の半分に置くときは、その年度のまま縦に動いて届くよう、縦線からの間隔を年度の横の範囲より狭くする
    const nearGap = Math.min(gap, Math.max(1, halfStep / 2));
    const fitsRight = anchorX + nearGap <= container.clientWidth - width - margin;
    const nearRight = anchorX + nearGap;
    const nearLeft = anchorX - nearGap - width;
    const beyondRight = bandRight + 1;
    const beyondLeft = bandLeft - width - 1;

    const plotBottom = plotArea.height - plotArea.bottom;
    const plotMiddle = (plotArea.top + plotBottom) / 2;
    const upperTop = plotArea.top;
    const lowerTop = plotBottom - height;
    const awayTop = pointer && pointer.y < plotMiddle ? lowerTop : upperTop;
    const sameTop = awayTop === upperTop ? lowerTop : upperTop;

    // 反対側の半分では縦線のすぐ横に置き、同じ年度のまま縦に動けばツールチップへ移れるようにする（WCAG 1.4.13）。
    // 同じ半分に置くときは、その年度になる横の範囲の外に置く
    const candidates = [
      [fitsRight ? nearRight : nearLeft, awayTop],
      [fitsRight ? nearLeft : nearRight, awayTop],
      [fitsRight ? beyondRight : beyondLeft, sameTop],
      [fitsRight ? beyondLeft : beyondRight, sameTop]
    ].map(([left, top]) => [clampLeft(left), clampTop(top)]);

    const pointerY = pointer ? canvas.offsetTop + pointer.y : null;
    const coversPointerPath = ([left, top]) => pointer !== null
      && left <= bandRight && left + width >= bandLeft
      && pointerY >= top - margin && pointerY <= top + height + margin;

    const clearIndex = candidates.findIndex((candidate) => !coversPointerPath(candidate));
    const [left, top] = candidates[Math.max(0, clearIndex)];

    // 同じ半分に置く候補（3・4）は、縦に動いても届かず、速い横移動ではわずかな余白を飛び越えて乗りうるため、
    // 重なりを避けられない場合と同じく、ホバーの更新とクリックを妨げないよう透過させる
    tooltip.classList.toggle('life-plan-chart__tooltip--passthrough', clearIndex === -1 || clearIndex >= 2);
    tooltip.style.left = `${left}px`;
    tooltip.style.top = `${top}px`;
  };

  const createHint = (text) => {
    const hint = document.createElement('p');
    hint.className = 'life-plan-chart__detail-hint';
    hint.textContent = text;

    return hint;
  };

  const createYearHeading = (className, year, statusText) => {
    const heading = document.createElement('p');
    heading.className = className;

    const status = document.createElement('span');
    status.className = 'life-plan-chart__year-status';
    status.textContent = statusText;

    heading.append(`${year}年`, status);

    return heading;
  };

  // 数値表示欄とツールチップで共用する、表示中の項目の金額一覧
  const createSeriesValues = (point, visibleSeries) => {
    if (visibleSeries.length === 0) {
      return createHint('表示項目が選択されていません。');
    }

    const list = document.createElement('dl');
    list.className = 'life-plan-chart__detail-list';

    list.append(...visibleSeries.map((series) => {
      const item = document.createElement('div');
      item.className = 'life-plan-chart__detail-item';

      const term = document.createElement('dt');
      term.append(createSeriesMarker(series), series.label);

      const unit = document.createElement('span');
      unit.className = 'life-plan-chart__unit';
      unit.textContent = '万円';

      const value = document.createElement('dd');
      value.append(formatDetailAmount(point[series.key]), unit);

      item.append(term, value);

      return item;
    }));

    return list;
  };

  const createSeriesMarker = (series) => {
    const marker = document.createElement('span');
    marker.className = 'life-plan-chart__legend-marker';
    marker.setAttribute('aria-hidden', 'true');
    marker.style.backgroundColor = getSeriesColor(series);

    return marker;
  };

  const showFallback = (fallback) => {
    fallback?.removeAttribute('hidden');
  };

  const renderSeriesControls = (seriesOptions) => {
    if (!seriesOptions) {
      return [];
    }

    const controls = seriesDefinitions.map((series) => {
      const option = document.createElement('label');
      option.className = 'life-plan-chart__series-option';

      const input = document.createElement('input');
      input.className = 'form-check-input';
      input.type = 'checkbox';
      input.value = series.key;
      input.checked = true;

      const label = document.createElement('span');
      label.textContent = series.label;

      // グラフ下部の凡例の代わりに、チェックの有無にかかわらず系列色を示す
      option.append(input, createSeriesMarker(series), label);

      return { series, option, input };
    });

    seriesOptions.replaceChildren(...controls.map((control) => control.option));

    return controls;
  };

  const drawChart = (canvas, context, data, visibleSeries, selectedIndex, hoverIndex) => {
    const width = Math.floor(canvas.clientWidth);
    const height = Math.floor(canvas.clientHeight || 360);

    if (width <= 0 || height <= 0) {
      return null;
    }

    const pixelRatio = window.devicePixelRatio || 1;
    canvas.width = Math.floor(width * pixelRatio);
    canvas.height = Math.floor(height * pixelRatio);
    context.setTransform(pixelRatio, 0, 0, pixelRatio, 0, 0);
    context.clearRect(0, 0, width, height);

    const plotArea = createPlotArea(width, height);
    const scale = createScale(data, visibleSeries);

    drawGrid(context, plotArea, scale);
    drawAxes(context, plotArea, scale);
    drawSeries(context, data, plotArea, scale, visibleSeries);
    drawHover(context, data, plotArea, scale, visibleSeries, hoverIndex);
    drawSelection(context, data, plotArea, scale, visibleSeries, selectedIndex);

    return plotArea;
  };

  const createPlotArea = (width, height) => ({
    left: width < 480 ? 56 : 72,
    right: width < 480 ? 12 : 24,
    top: 24,
    bottom: 48,
    width,
    height
  });

  const createScale = (data, visibleSeries) => {
    const values = [0];

    data.forEach((point) => {
      visibleSeries.forEach((series) => {
        values.push(toNumber(point[series.key]));
      });
    });

    const minValue = Math.min(...values);
    const maxValue = Math.max(...values);
    const range = maxValue - minValue || 1;
    const padding = Math.max(range * 0.08, 1);

    return createNiceScale(minValue - padding, maxValue + padding);
  };

  const createNiceScale = (minValue, maxValue) => {
    const tickCount = 5;
    const rawStep = (maxValue - minValue) / (tickCount - 1);
    const step = createNiceStep(rawStep);
    const min = Math.floor(minValue / step) * step;
    const max = Math.ceil(maxValue / step) * step;
    const ticks = [];

    for (let value = min; value <= max + step / 2; value += step) {
      ticks.push(value);
    }

    return { min, max, ticks };
  };

  const createNiceStep = (rawStep) => {
    const exponent = Math.floor(Math.log10(rawStep));
    const base = rawStep / (10 ** exponent);
    const niceBase = base <= 1 ? 1 : base <= 2 ? 2 : base <= 5 ? 5 : 10;

    return niceBase * (10 ** exponent);
  };

  const drawGrid = (context, plotArea, scale) => {
    context.save();
    context.strokeStyle = getCssColor('--ui-color-border', '#e0e0e0');
    context.lineWidth = 1;

    scale.ticks.forEach((tick) => {
      const y = toY(tick, plotArea, scale);
      drawLine(context, plotArea.left, y, plotArea.width - plotArea.right, y);
    });

    context.restore();
  };

  const drawAxes = (context, plotArea, scale) => {
    context.save();
    context.fillStyle = getCssColor('--ui-color-text', '#333333');
    context.strokeStyle = getCssColor('--ui-color-muted', '#888888');
    context.lineWidth = 1;
    context.font = '12px "Noto Sans JP", system-ui, sans-serif';
    context.textBaseline = 'middle';

    drawLine(context, plotArea.left, plotArea.top, plotArea.left, plotArea.height - plotArea.bottom);
    drawLine(context, plotArea.left, plotArea.height - plotArea.bottom, plotArea.width - plotArea.right, plotArea.height - plotArea.bottom);

    scale.ticks.forEach((tick) => {
      const y = toY(tick, plotArea, scale);
      context.textAlign = 'right';
      context.fillText(formatAmount(tick), plotArea.left - 8, y);
    });

    context.textAlign = 'left';
    context.textBaseline = 'top';
    context.fillText('万円', plotArea.left, 0);
    context.restore();
  };

  const drawSeries = (context, data, plotArea, scale, visibleSeries) => {
    drawYearLabels(context, data, plotArea);

    visibleSeries.forEach((series) => {
      context.save();
      context.strokeStyle = getSeriesColor(series);
      context.lineWidth = 2.5;
      context.lineJoin = 'round';
      context.lineCap = 'round';
      context.beginPath();

      data.forEach((point, index) => {
        const x = toX(index, data.length, plotArea);
        const y = toY(toNumber(point[series.key]), plotArea, scale);

        if (index === 0) {
          context.moveTo(x, y);
        } else {
          context.lineTo(x, y);
        }
      });

      context.stroke();
      context.restore();
    });
  };

  // 一時表示は実線と塗りつぶしのマーカーで、破線と白抜きの固定表示と区別する
  const drawHover = (context, data, plotArea, scale, visibleSeries, hoverIndex) => {
    if (hoverIndex === null) {
      return;
    }

    const x = toX(hoverIndex, data.length, plotArea);
    const point = data[hoverIndex];

    context.save();
    context.strokeStyle = getCssColor('--ui-color-muted', '#888888');
    context.lineWidth = 1;
    drawLine(context, x, plotArea.top, x, plotArea.height - plotArea.bottom);

    visibleSeries.forEach((series) => {
      const y = toY(toNumber(point[series.key]), plotArea, scale);

      context.beginPath();
      context.arc(x, y, 3.5, 0, Math.PI * 2);
      context.fillStyle = getSeriesColor(series);
      context.fill();
    });

    context.restore();
  };

  const drawSelection = (context, data, plotArea, scale, visibleSeries, selectedIndex) => {
    if (selectedIndex === null) {
      return;
    }

    const x = toX(selectedIndex, data.length, plotArea);
    const point = data[selectedIndex];

    context.save();
    context.strokeStyle = getCssColor('--ui-color-muted', '#888888');
    context.lineWidth = 1;
    context.setLineDash([4, 4]);
    drawLine(context, x, plotArea.top, x, plotArea.height - plotArea.bottom);
    context.setLineDash([]);

    visibleSeries.forEach((series) => {
      const y = toY(toNumber(point[series.key]), plotArea, scale);

      context.beginPath();
      context.arc(x, y, 4.5, 0, Math.PI * 2);
      context.fillStyle = getCssColor('--ui-color-surface', '#ffffff');
      context.fill();
      context.lineWidth = 2.5;
      context.strokeStyle = getSeriesColor(series);
      context.stroke();
    });

    context.restore();
  };

  const drawYearLabels = (context, data, plotArea) => {
    const plotWidth = plotArea.width - plotArea.left - plotArea.right;
    const interval = Math.max(1, Math.ceil(data.length / Math.max(2, Math.floor(plotWidth / 72))));
    const minimumLabelGap = 42;
    const lastX = toX(data.length - 1, data.length, plotArea);

    context.save();
    context.fillStyle = getCssColor('--ui-color-text', '#333333');
    context.strokeStyle = getCssColor('--ui-color-muted', '#888888');
    context.font = '12px "Noto Sans JP", system-ui, sans-serif';
    context.textAlign = 'center';
    context.textBaseline = 'top';

    data.forEach((point, index) => {
      const x = toX(index, data.length, plotArea);
      const overlapsLastLabel = index !== data.length - 1 && Math.abs(lastX - x) < minimumLabelGap;
      const shouldDraw = index === 0 || index === data.length - 1 || (index % interval === 0 && !overlapsLastLabel);

      if (!shouldDraw) {
        return;
      }

      const axisY = plotArea.height - plotArea.bottom;
      drawLine(context, x, axisY, x, axisY + 5);
      context.fillText(String(point.year), x, axisY + 10);
    });

    context.restore();
  };

  const drawLine = (context, fromX, fromY, toXValue, toYValue) => {
    context.beginPath();
    context.moveTo(fromX, fromY);
    context.lineTo(toXValue, toYValue);
    context.stroke();
  };

  const toX = (index, count, plotArea) => {
    const plotWidth = plotArea.width - plotArea.left - plotArea.right;

    if (count <= 1) {
      return plotArea.left + plotWidth / 2;
    }

    return plotArea.left + (plotWidth * index) / (count - 1);
  };

  const toY = (value, plotArea, scale) => {
    const plotHeight = plotArea.height - plotArea.top - plotArea.bottom;
    const ratio = (value - scale.min) / (scale.max - scale.min || 1);

    return plotArea.height - plotArea.bottom - ratio * plotHeight;
  };

  const toNumber = (value) => {
    const number = Number(value);

    return Number.isFinite(number) ? number : 0;
  };

  const getSeriesColor = (series) => {
    return getCssColor(series.colorVariable, '#333333');
  };

  const getCssColor = (variableName, fallback) => {
    const color = getComputedStyle(document.documentElement)
      .getPropertyValue(variableName)
      .trim();

    return color || fallback;
  };

  const formatAmount = (value) => new Intl.NumberFormat('ja-JP', {
    maximumFractionDigits: 0
  }).format(value);

  // キャッシュフロー表と同じく、万円の小数第1位へ0から遠い方向に四捨五入して表示する
  const formatDetailAmount = (value) => {
    const yen = Math.round(toNumber(value) * 10000);
    const tenthsOfManYen = Math.sign(yen) * Math.floor((Math.abs(yen) + 500) / 1000);

    return new Intl.NumberFormat('ja-JP', {
      minimumFractionDigits: 1,
      maximumFractionDigits: 1
    }).format(tenthsOfManYen === 0 ? 0 : tenthsOfManYen / 10);
  };

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initialize);
  } else {
    initialize();
  }
})();
