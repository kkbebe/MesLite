<script setup>
import { ref, onMounted, watch } from 'vue'
import api from '../api/http'

const workOrders = ref([])
const selectedWoId = ref('')
const reports = ref([])
const operatorName = ref('')
const completedQty = ref(1)
const errorMsg = ref('')
const okMsg = ref('')

async function loadWorkOrders() {
  const res = await api.get('/workorders')
  // 只能對 ISSUED 狀態的工單報工
  workOrders.value = res.data
}

async function loadReports() {
  if (!selectedWoId.value) { reports.value = []; return }
  const res = await api.get('/workreports', { params: { woId: selectedWoId.value } })
  reports.value = res.data
}

async function submitReport() {
  errorMsg.value = ''
  okMsg.value = ''
  if (!selectedWoId.value) { errorMsg.value = '請先選擇工單'; return }
  try {
    const res = await api.post('/workreports', {
      woId: selectedWoId.value,
      operatorName: operatorName.value || '未填寫',
      completedQty: completedQty.value
    })
    okMsg.value = `報工成功，工單目前狀態：${res.data.woStatus}`
    completedQty.value = 1
    await loadReports()
    await loadWorkOrders()
  } catch (e) {
    errorMsg.value = '報工失敗：' + (e.response?.data ?? e.message)
  }
}

watch(selectedWoId, loadReports)
onMounted(loadWorkOrders)
</script>

<template>
  <h2>報工</h2>

  <div class="card">
    <div class="form-row">
      <select v-model="selectedWoId">
        <option disabled value="">選擇工單</option>
        <option v-for="w in workOrders" :key="w.id" :value="w.id">
          {{ w.woNumber || ('工單#' + w.id) }} - {{ w.productName }} ({{ w.status }})
        </option>
      </select>
    </div>
    <div class="form-row">
      <input v-model="operatorName" placeholder="作業員姓名" />
      <input v-model.number="completedQty" type="number" placeholder="本次完工數量" />
      <button class="btn" @click="submitReport">送出報工</button>
    </div>
    <p v-if="errorMsg" class="error-msg">{{ errorMsg }}</p>
    <p v-if="okMsg" style="color:#16a34a; font-size:13px;">{{ okMsg }}</p>
  </div>

  <div class="card" v-if="selectedWoId">
    <h3>此工單報工紀錄</h3>
    <table>
      <thead><tr><th>作業員</th><th>完工數量</th><th>報工時間</th></tr></thead>
      <tbody>
        <tr v-for="r in reports" :key="r.id">
          <td>{{ r.operatorName }}</td>
          <td>{{ r.completedQty }}</td>
          <td>{{ new Date(r.reportTime).toLocaleString() }}</td>
        </tr>
        <tr v-if="reports.length === 0"><td colspan="3">尚無報工紀錄</td></tr>
      </tbody>
    </table>
  </div>
</template>
