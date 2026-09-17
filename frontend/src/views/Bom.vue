<script setup>
import { ref, onMounted } from 'vue'
import api from '../api/http'

const boms = ref([])
const materials = ref([])
const errorMsg = ref('')
const form = ref({ parentItemId: '', childItemId: '', requiredQty: 1 })

async function loadMaterials() {
  const res = await api.get('/materials')
  materials.value = res.data
}

async function loadBoms() {
  errorMsg.value = ''
  try {
    const res = await api.get('/bom')
    boms.value = res.data
  } catch (e) {
    errorMsg.value = '讀取失敗：' + (e.response?.data ?? e.message)
  }
}

async function submitForm() {
  errorMsg.value = ''
  try {
    await api.post('/bom', form.value)
    form.value = { parentItemId: '', childItemId: '', requiredQty: 1 }
    await loadBoms()
  } catch (e) {
    errorMsg.value = '新增失敗：' + (e.response?.data ?? e.message)
  }
}

async function deleteBom(id) {
  if (!confirm('確定要刪除這筆 BOM 明細嗎？')) return
  try {
    await api.delete(`/bom/${id}`)
    await loadBoms()
  } catch (e) {
    errorMsg.value = '刪除失敗：' + (e.response?.data ?? e.message)
  }
}

onMounted(async () => {
  await loadMaterials()
  await loadBoms()
})
</script>

<template>
  <h2>BOM 物料清單</h2>

  <div class="card">
    <h3>新增 BOM 明細</h3>
    <div class="form-row">
      <select v-model.number="form.parentItemId">
        <option disabled value="">選成品</option>
        <option v-for="m in materials.filter(x => x.type === 'FG')" :key="m.id" :value="m.id">
          {{ m.name }} ({{ m.itemCode }})
        </option>
      </select>
      <select v-model.number="form.childItemId">
        <option disabled value="">選原物料</option>
        <option v-for="m in materials.filter(x => x.type === 'RAW')" :key="m.id" :value="m.id">
          {{ m.name }} ({{ m.itemCode }})
        </option>
      </select>
      <input v-model.number="form.requiredQty" type="number" step="0.01" placeholder="用量" />
      <button class="btn" @click="submitForm">新增</button>
    </div>
    <p v-if="errorMsg" class="error-msg">{{ errorMsg }}</p>
  </div>

  <div class="card">
    <table>
      <thead>
        <tr><th>Id</th><th>成品</th><th>原物料</th><th>用量</th><th>操作</th></tr>
      </thead>
      <tbody>
        <tr v-for="b in boms" :key="b.id">
          <td>{{ b.id }}</td>
          <td>{{ b.parentItemName }}</td>
          <td>{{ b.childItemName }} ({{ b.childItemCode }})</td>
          <td>{{ b.requiredQty }}</td>
          <td><button class="btn danger" @click="deleteBom(b.id)">刪除</button></td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
