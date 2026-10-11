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
    const fallback = root?.querySelector('[data-life-plan-chart-fallback]');
    const legend = root?.querySelector('[data-life-plan-chart-legend]');
    const seriesOptions = root?.querySelector('[data-life-plan-chart-series-options]');
    const detail = root?.querySelector('[data-life-plan-chart-detail]');
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

    let selectedIndex = null;
    let plotArea = null;
    let renderedDetailKey = null;

    const redraw = () => {
      const visibleSeries = getVisibleSeries();
      const detailKey = `${selectedIndex}|${visibleSeries.map((series) => series.key).join(',')}`;

      renderLegend(legend, visibleSeries);

      // 内容が変わらない再描画（リサイズなど）で読み上げが繰り返されないよう、詳細表示は変化時のみ更新する
      if (detailKey !== renderedDetailKey) {
        renderDetail(detail, data, visibleSeries, selectedIndex);
        renderedDetailKey = detailKey;
      }

      plotArea = drawChart(canvas, context, data, visibleSeries, selectedIndex) ?? plotArea;
    };

    const selectIndex = (index) => {
      if (index === selectedIndex) {
        return;
      }

      selectedIndex = index;
      redraw();
    };

    seriesControls.forEach((control) => {
      control.input.addEventListener('change', redraw);
    });

    bindYearSelection(canvas, data, () => plotArea, () => selectedIndex, selectIndex);

    redraw();

    if ('ResizeObserver' in window) {
      const observer = new ResizeObserver(redraw);
      observer.observe(canvas.parentElement);
      return;
    }

    window.addEventListener('resize', redraw);
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

  const bindYearSelection = (canvas, data, getPlotArea, getSelectedIndex, selectIndex) => {
    let activePointerId = null;
    let indexBeforeTouch = null;

    const selectFromEvent = (event) => {
      const plotArea = getPlotArea();

      if (!plotArea) {
        return;
      }

      const rect = canvas.getBoundingClientRect();

      selectIndex(toNearestIndex(event.clientX - rect.left, data.length, plotArea));
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
        canvas.setPointerCapture?.(event.pointerId);
        selectFromEvent(event);
      }
    });

    canvas.addEventListener('pointermove', (event) => {
      if (event.pointerId === activePointerId) {
        selectFromEvent(event);
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

    canvas.addEventListener('keydown', (event) => {
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
      const hint = document.createElement('p');
      hint.className = 'life-plan-chart__detail-hint';
      hint.textContent = 'グラフをタップ（クリック）すると、その年度の金額を表示します。';
      detail.replaceChildren(hint);
      return;
    }

    const point = data[selectedIndex];

    const year = document.createElement('p');
    year.className = 'life-plan-chart__detail-year';
    year.textContent = `${point.year}年`;

    if (visibleSeries.length === 0) {
      const empty = document.createElement('p');
      empty.className = 'life-plan-chart__detail-hint';
      empty.textContent = '表示項目が選択されていません。';
      detail.replaceChildren(year, empty);
      return;
    }

    const list = document.createElement('dl');
    list.className = 'life-plan-chart__detail-list';

    list.append(...visibleSeries.map((series) => {
      const item = document.createElement('div');
      item.className = 'life-plan-chart__detail-item';

      const marker = document.createElement('span');
      marker.className = 'life-plan-chart__legend-marker';
      marker.style.backgroundColor = getSeriesColor(series);

      const term = document.createElement('dt');
      term.append(marker, series.label);

      const value = document.createElement('dd');
      value.textContent = `${formatDetailAmount(point[series.key])}万円`;

      item.append(term, value);

      return item;
    }));

    detail.replaceChildren(year, list);
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

      option.append(input, label);

      return { series, option, input };
    });

    seriesOptions.replaceChildren(...controls.map((control) => control.option));

    return controls;
  };

  const renderLegend = (legend, visibleSeries) => {
    if (!legend) {
      return;
    }

    legend.replaceChildren(...visibleSeries.map((series) => {
      const item = document.createElement('span');
      item.className = 'life-plan-chart__legend-item';

      const marker = document.createElement('span');
      marker.className = 'life-plan-chart__legend-marker';
      marker.style.backgroundColor = getSeriesColor(series);

      const label = document.createElement('span');
      label.textContent = series.label;

      item.append(marker, label);

      return item;
    }));
  };

  const drawChart = (canvas, context, data, visibleSeries, selectedIndex) => {
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
