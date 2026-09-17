<script setup>
import { ref, onMounted } from 'vue'
import api from '../api/http'

const workOrders = ref([])
const materials = ref([])
const errorMsg = ref('')
const form = ref({ woNumber: '', productId: '', targetQty: 1 })

async function loadMaterials() {
  const res = await api.get('/materials')
  materials.value = res.data.filter(m => m.type === 'FG')
}

async function loadWorkOrders() {
  errorMsg.value = ''
  try {
    const res = await api.get('/workorders')
    workOrders.value = res.data
  } catch (e) {
    errorMsg.value = '讀取失敗：' + (e.response?.data ?? e.message)
  }
}

async function submitForm() {
  errorMsg.value = ''
  try {
    await api.post('/workorders', form.value)
    form.value = { woNumber: '', productId: '', targetQty: 1 }
    await loadWorkOrders()
  } catch (e) {
    errorMsg.value = '建立失敗：' + (e.response?.data ?? e.message)
  }
}

onMounted(async () => {
  await loadMaterials()
  await loadWorkOrders()
})
</script>

<template>
  <h2>工單管理</h2>

  <div class="card">
    <h3>新增工單</h3>
    <div class="form-row">
      <input v-model="form.woNumber" placeholder="工單單號 (WO-xxxx)" />
      <select v-model.number="form.productId">
        <option disabled value="">選成品</option>
        <option v-for="m in materials" :key="m.id" :value="m.id">{{ m.name }} ({{ m.itemCode }})</option>
      </select>
      <input v-model.number="form.targetQty" type="number" placeholder="計畫生產數量" />
      <button class="btn" @click="submitForm">建立工單</button>
    </div>
    <p v-if="errorMsg" class="error-msg">{{ errorMsg }}</p>
  </div>

  <div class="card">
    <table>
      <thead>
        <tr><th>Id</th><th>單號</th><th>成品</th><th>計畫數量</th><th>狀態</th><th>建立時間</th><th></th></tr>
      </thead>
      <tbody>
        <tr v-for="w in workOrders" :key="w.id">
          <td>{{ w.id }}</td>
          <td>{{ w.woNumber }}</td>
          <td>{{ w.productName }}</td>
          <td>{{ w.targetQty }}</td>
          <td><span class="status-tag" :class="'status-' + w.status">{{ w.status }}</span></td>
          <td>{{ new Date(w.createdAt).toLocaleString() }}</td>
          <td><router-link :to="`/workorders/${w.id}`" class="btn secondary">詳情</router-link></td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
