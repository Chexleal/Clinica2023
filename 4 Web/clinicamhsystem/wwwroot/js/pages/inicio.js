var labels = JSON.parse(document.getElementById('labels').value);
var dataConsultas = JSON.parse(document.getElementById('dataConsultas').value);
var dataIngresos = JSON.parse(document.getElementById('dataIngresos').value);

var ctx = document.getElementById('graficaConsultas').getContext('2d');
var chart = new Chart(ctx, {
    type: 'bar',
    data: {
        labels: labels,
        datasets: [
            {
                backgroundColor: ['#B39DDB', '#F48FB1','#80DEEA'],
                fillColor: "rgba(100,194,132,0.4)",
                strokeColor: "#ACC26D",
                pointColor: "#fff",
                pointStrokeColor: "#9DB86D",
                data: dataConsultas
            }
        ]
    },
    options: {
        responsive: true,
        maintainAspectRatio: false, // This ensures the chart takes the size of its container
        scales: {
            y: {
                beginAtZero: true
            }
        },
        plugins: {
            legend: {
                display: false // Ocultar la leyenda
            }
        }
    }
});

var ctx2 = document.getElementById('graficaIngresos').getContext('2d');
var chart2 = new Chart(ctx2, {
    type: 'line',
    data: {
        labels: labels,
        datasets: [
            {
                backgroundColor: ['#B39DDB', '#F48FB1', '#80DEEA'],
                fillColor: "rgba(172,194,132,0.4)",
                strokeColor: "#ACC26D",
                pointColor: "#fff",
                pointStrokeColor: "#9DB86D",
                data: dataIngresos
            }
        ]
    },
    options: {
        responsive: true,
        maintainAspectRatio: false, // This ensures the chart takes the size of its container
        scales: {
            y: {
                beginAtZero: true
            }
        },
        plugins: {
            legend: {
                display: false // Ocultar la leyenda
            }
        }
    }
});

console.log('Chart configuration:', chart.options.plugins.legend.display);