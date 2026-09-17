<script setup>
import { ref, onMounted, watch } from 'vue'
import api from '../api/http'

const workOrders = ref([])
const selectedWoId = ref('')
const transactions = ref([])
const operatorName = ref('')
const errorMsg = ref('')
const okMsg = ref('')

async function loadWorkOrders() {
  const res = await api.get('/workorders')
  workOrders.value = res.data
}

async function loadTransactions() {
  if (!selectedWoId.value) { transactions.value = []; return }
  const res = await api.get('/inventorytransactions', { params: { woId: selectedWoId.value } })
  transactions.value = res.data
}

async function doIssue() {
  errorMsg.value = ''
  okMsg.value = ''
  if (!selectedWoId.value) { errorMsg.value = '請先選擇工單'; return }
  try {
    await api.post('/inventorytransactions/auto-issue', {
      woId: selectedWoId.value,
      operatorName: operatorName.value || '未填寫'
    })
    okMsg.value = '領料成功'
    await loadTransactions()
    await loadWorkOrders()
  } catch (e) {
    errorMsg.value = '領料失敗：' + (e.response?.data ?? e.message)
  }
}

watch(selectedWoId, loadTransactions)
onMounted(loadWorkOrders)
</script>

<template>
  <h2>領料</h2>

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
      <input v-model="operatorName" placeholder="領料人姓名" />
      <button class="btn" @click="doIssue">依 BOM 自動領料</button>
    </div>
    <p v-if="errorMsg" class="error-msg">{{ errorMsg }}</p>
    <p v-if="okMsg" style="color:#16a34a; font-size:13px;">{{ okMsg }}</p>
  </div>

  <div class="card" v-if="selectedWoId">
    <h3>此工單領料紀錄</h3>
    <table>
      <thead><tr><th>物料</th><th>類型</th><th>數量</th><th>操作人</th><th>時間</th></tr></thead>
      <tbody>
        <tr v-for="t in transactions" :key="t.id">
          <td>{{ t.materialName }} ({{ t.materialCode }})</td>
          <td>{{ t.type }}</td>
          <td>{{ t.qty }}</td>
          <td>{{ t.operatorName }}</td>
          <td>{{ new Date(t.createdAt).toLocaleString() }}</td>
        </tr>
        <tr v-if="transactions.length === 0"><td colspan="5">尚無領料紀錄</td></tr>
      </tbody>
    </table>
  </div>
</template>
