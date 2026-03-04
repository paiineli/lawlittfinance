const chartColors = ['#4ade80', '#60a5fa', '#f97316', '#f472b6', '#a78bfa', '#34d399'];
const legendColor = '#e5e7eb';
const fontFamily = { family: 'JetBrains Mono', size: 11 };
const monthNames = ['Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
    'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'];

function initDashboard(categoryData, trendData) {
    if (categoryData.length > 0) buildPieChart(categoryData);
    buildBarChart(trendData);
}

function buildPieChart(data) {
    const collapsed = collapseCategories(data);
    new Chart(document.getElementById('pieChart'), {
        type: 'doughnut',
        data: {
            labels: collapsed.map(category => category.label),
            datasets: [{ data: collapsed.map(category => category.value), backgroundColor: chartColors, borderWidth: 0 }],
        },
        options: {
            cutout: '60%',
            plugins: {
                legend: { position: 'right', labels: { color: legendColor, font: fontFamily, padding: 14, boxWidth: 12 } },
                tooltip: { callbacks: { label: ctx => ` R$ ${ctx.parsed.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}` } },
            },
        },
    });
}

function buildBarChart(data) {
    if (!data || !data.length) return;

    const labels = data.map(trend => monthNames[trend.month - 1].slice(0, 3) + '/' + String(trend.year).slice(-2));
    const incomes = data.map(trend => trend.income);
    const expenses = data.map(trend => trend.expenses);

    new Chart(document.getElementById('barChart'), {
        type: 'bar',
        data: {
            labels,
            datasets: [
                {
                    label: 'Receitas',
                    data: incomes,
                    backgroundColor: 'rgba(74, 222, 128, 0.7)',
                    borderColor: '#4ade80',
                    borderWidth: 1,
                    borderRadius: 4,
                },
                {
                    label: 'Despesas',
                    data: expenses,
                    backgroundColor: 'rgba(248, 113, 113, 0.7)',
                    borderColor: '#f87171',
                    borderWidth: 1,
                    borderRadius: 4,
                },
            ],
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { labels: { color: legendColor, font: fontFamily, boxWidth: 12, padding: 12 } },
                tooltip: {
                    callbacks: {
                        label: ctx => ` ${ctx.dataset.label}: R$ ${ctx.parsed.y.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`,
                    },
                },
            },
            scales: {
                x: { ticks: { color: legendColor, font: fontFamily }, grid: { color: 'rgba(255,255,255,0.06)' } },
                y: {
                    ticks: {
                        color: legendColor,
                        font: fontFamily,
                        callback: value => 'R$ ' + value.toLocaleString('pt-BR', { minimumFractionDigits: 0 }),
                    },
                    grid: { color: 'rgba(255,255,255,0.06)' },
                },
            },
        },
    });
}

function collapseCategories(data, max = 5) {
    if (data.length <= max) return data;
    const top = data.slice(0, max);
    const othersTotal = data.slice(max).reduce((sum, category) => sum + category.value, 0);
    return [...top, { label: 'Outros', value: othersTotal }];
}

function initRankingBars() {
    document.querySelectorAll('.ranking-fill[data-fill-width]').forEach(function (element, index) {
        element.style.width = element.dataset.fillWidth + '%';
        element.style.backgroundColor = chartColors[index % chartColors.length];
    });
}

(function () {
    var dataElement = document.getElementById('dashboardData');
    if (!dataElement) return;
    var data = JSON.parse(dataElement.textContent);
    initDashboard(data.categories, data.trend);
    initRankingBars();
})();
