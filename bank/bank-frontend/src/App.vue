<script setup>
import { ref, onMounted } from 'vue'
import { Bar } from 'vue-chartjs'
import { 
  Chart as ChartJS, Title, Tooltip, Legend, 
  BarElement, CategoryScale, LinearScale 
} from 'chart.js'

// Registering Chart.js components
ChartJS.register(Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale)

// --- STATE MANAGEMENT ---
const mcc = ref('RESTAURANT')
const amount = ref(0)
const statusMessage = ref('')

// Chart configuration state
const chartData = ref({
  labels: [],
  datasets: [{ 
    label: 'Spending per Category ($)',
    backgroundColor: ['#42b883', '#34495e', '#3498db', '#f1c40f'],
    data: [] 
  }]
})

// --- API SERVICES ---

/**
 * Fetch spending statistics from the .NET Backend (Port 5138)
 */
async function fetchChartData() {
  try {
    const response = await fetch('http://localhost:5138/api/spending');
    if (response.ok) {
      const data = await response.json();
      
      chartData.value = {
        labels: data.map(item => item.mcc),
        datasets: [{
          label: 'Spending per Category ($)',
          backgroundColor: ['#42b883', '#34495e', '#3498db', '#f1c40f'],
          data: data.map(item => item.totalAmount)
        }]
      };
    }
  } catch (error) {
    console.error("API Error:", error);
  }
}

/**
 * Post a new transaction to the SQL Database via API (Port 5138)
 */
async function sendTransaction() {
  if (amount.value <= 0) {
    statusMessage.value = "⚠️ Please enter a valid amount.";
    return;
  }

  const payload = {
    mcc: mcc.value,
    amount: amount.value
  };

  try {
    const response = await fetch('http://localhost:5138/paga', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (response.ok) {
      statusMessage.value = "✅ Transaction recorded successfully!";
      amount.value = 0;
      await fetchChartData(); // Refresh chart with new data from SQL Server
    } else {
      statusMessage.value = "❌ Error processing transaction.";
    }
  } catch (error) {
    statusMessage.value = "🔌 Network Error: Is the Backend running on port 5138?";
  }
}

onMounted(() => {
  fetchChartData();
})
</script>

<template>
  <div class="container">
    <header>
      <h1>Expense Tracker 💰</h1>
      <p>Monitor your daily spending in real-time (SQL Server + .NET)</p>
    </header>
    
    <div class="card">
      <h3>New Transaction</h3>
      
      <label>Category</label>
      <select v-model="mcc">
        <option value="RESTAURANT">🍴 Restaurant</option>
        <option value="GROCERIES">🛒 Groceries</option>
        <option value="TRANSPORT">🚌 Transport</option>
        <option value="LEISURE">🎮 Leisure</option>
      </select>

      <label>Amount ($)</label>
      <input type="number" v-model="amount" placeholder="e.g. 15.50" step="0.01" />

      <button @click="sendTransaction">Submit Payment</button>
      
      <p v-if="statusMessage" :class="['status', statusMessage.includes('✅') ? 'success' : 'error']">
        {{ statusMessage }}
      </p>
    </div>

    <div class="chart-section" v-if="chartData.labels.length > 0">
      <h3>Spending Overview</h3>
      <div class="canvas-wrapper">
        <Bar :data="chartData" :options="{ responsive: true, maintainAspectRatio: false }" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.container { font-family: 'Inter', sans-serif; max-width: 500px; margin: 40px auto; padding: 20px; }
header { text-align: center; margin-bottom: 30px; color: #2c3e50; }

.card { background: #fff; padding: 25px; border-radius: 12px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); display: flex; flex-direction: column; gap: 15px; }
label { font-weight: 600; font-size: 0.9rem; color: #555; }
input, select { padding: 10px; border: 1.5px solid #eee; border-radius: 6px; font-size: 1rem; }

button { background: #42b883; color: white; padding: 12px; border: none; border-radius: 6px; font-weight: bold; cursor: pointer; transition: 0.3s; }
button:hover { background: #3aa876; transform: scale(1.02); }

.status { padding: 10px; border-radius: 4px; font-size: 0.85rem; text-align: center; margin-top: 10px; }
.success { background: #f0fdf4; color: #166534; border: 1px solid #bbf7d0; }
.error { background: #fef2f2; color: #991b1b; border: 1px solid #fecaca; }

.chart-section { margin-top: 40px; }
.canvas-wrapper { height: 300px; margin-top: 15px; }
</style>