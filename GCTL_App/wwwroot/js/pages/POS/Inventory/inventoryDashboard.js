$(document).ready(function () {
    let stockChart = null;

    function loadChartData() {
        $.ajax({
            url: '/Inventory/GetDashboardChartData',
            method: 'GET',
            success: function (data) {
                renderStockChart(data);
            },
            error: function () {
                console.error('Failed to load chart data');
            }
        });
    }

    function renderStockChart(data) {
        const ctx = document.getElementById('stockByLocationChart');

        if (stockChart) {
            stockChart.destroy();
        }

        stockChart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.labels,
                datasets: [{
                    label: 'Stock Value',
                    data: data.values,
                    backgroundColor: [
                        'rgba(54, 162, 235, 0.8)',
                        'rgba(255, 99, 132, 0.8)',
                        'rgba(255, 206, 86, 0.8)',
                        'rgba(75, 192, 192, 0.8)',
                        'rgba(153, 102, 255, 0.8)',
                        'rgba(255, 159, 64, 0.8)'
                    ],
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom'
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                let label = context.label || '';
                                if (label) {
                                    label += ': ';
                                }
                                label += '৳' + context.parsed.toFixed(2);
                                label += ' (' + data.productCounts[context.dataIndex] + ' products)';
                                return label;
                            }
                        }
                    }
                }
            }
        });
    }

    function loadRecentTransactions() {
        $.ajax({
            url: '/Inventory/GetRecentTransactions',
            method: 'GET',
            data: { count: 10 },
            success: function (transactions) {
                let html = '';

                if (transactions.length === 0) {
                    html = '<tr><td colspan="4" class="text-center text-muted">No transactions yet</td></tr>';
                } else {
                    $.each(transactions, function (i, t) {
                        const typeClass = t.transactionType === 'IN' ? 'text-success' : 'text-danger';
                        const typeIcon = t.transactionType === 'IN' ? '↑' : '↓';

                        html += `
                            <tr>
                                <td class="fs-10">${formatDate(t.transactionDate)}</td>
                                <td class="${typeClass} fw-bold">${typeIcon} ${t.transactionType}</td>
                                <td class="fs-10">${t.productName}</td>
                                <td class="fw-bold">${t.quantity}</td>
                            </tr>`;
                    });
                }

                $('#recentTransactionsBody').html(html);
            },
            error: function () {
                $('#recentTransactionsBody').html(
                    '<tr><td colspan="4" class="text-center text-danger">Failed to load transactions</td></tr>'
                );
            }
        });
    }

    function formatDate(dateStr) {
        if (!dateStr) return 'N/A';
        const d = new Date(dateStr);
        return d.toLocaleDateString('en-GB', {
            day: '2-digit',
            month: 'short',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    // Auto-refresh every 5 minutes
    setInterval(function () {
        loadRecentTransactions();
    }, 300000);

    // Initialize
    loadChartData();
    loadRecentTransactions();
});
