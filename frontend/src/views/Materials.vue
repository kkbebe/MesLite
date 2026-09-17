<script setup>
import { ref, onMounted } from 'vue'
import api from '../api/http'

const materials = ref([])
const errorMsg = ref('')
const form = ref({ itemCode: '', name: '', type: 'RAW', stockQty: 0 })
const editingId = ref(null)

async function loadMaterials() {
  errorMsg.value = ''
  try {
    const res = await api.get('/materials')
    materials.value = res.data
  } catch (e) {
    errorMsg.value = '讀取失敗：' + (e.response?.data ?? e.message)
  }
}

function startEdit(m) {
  editingId.value = m.id
  form.value = { itemCode: m.itemCode, name: m.name, type: m.type, stockQty: m.stockQty }
}

function resetForm() {
  editingId.value = null
  form.value = { itemCode: '', name: '', type: 'RAW', stockQty: 0 }
}

async function submitForm() {
  errorMsg.value = ''
  try {
    if (editingId.value) {
      await api.put(`/materials/${editingId.value}`, form.value)
    } else {
      await api.post('/materials', form.value)
    }
    resetForm()
    await loadMaterials()
  } catch (e) {
    errorMsg.value = '儲存失敗：' + (e.response?.data ?? e.message)
  }
}

async function deleteMaterial(id) {
  if (!confirm('確定要刪除這筆物料嗎？')) return
  try {
    await api.delete(`/materials/${id}`)
    await loadMaterials()
  } catch (e) {
    errorMsg.value = '刪除失敗：' + (e.response?.data ?? e.message)
  }
}

onMounted(loadMaterials)
</script>

<template>
  <h2>物料與庫存主檔</h2>

  <div class="card">
    <h3>{{ editingId ? '編輯物料' : '新增物料' }}</h3>
    <div class="form-row">
      <input v-model="form.itemCode" placeholder="品號 (ItemCode)" />
      <input v-model="form.name" placeholder="品名" />
      <select v-model="form.type">
        <option value="RAW">RAW 原物料</option>
        <option value="FG">FG 成品</option>
      </select>
      <input v-model.number="form.stockQty" type="number" placeholder="庫存數量" />
      <button class="btn" @click="submitForm">{{ editingId ? '儲存' : '新增' }}</button>
      <button v-if="editingId" class="btn secondary" @click="resetForm">取消編輯</button>
    </div>
    <p v-if="errorMsg" class="error-msg">{{ errorMsg }}</p>
  </div>

  <div class="card">
    <table>
      <thead>
        <tr>
          <th>Id</th><th>品號</th><th>品名</th><th>類別</th><th>庫存</th><th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="m in materials" :key="m.id">
          <td>{{ m.id }}</td>
          <td>{{ m.itemCode }}</td>
          <td>{{ m.name }}</td>
          <td>{{ m.type }}</td>
          <td>{{ m.stockQty }}</td>
          <td>
            <button class="btn secondary" @click="startEdit(m)">編輯</button>
            <button class="btn danger" @click="deleteMaterial(m.id)">刪除</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
